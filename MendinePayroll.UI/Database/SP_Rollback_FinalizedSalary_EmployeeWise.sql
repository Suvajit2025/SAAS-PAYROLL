IF OBJECT_ID('dbo.Payroll_SaaS_FinalizedSalary_Delete_Audit', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payroll_SaaS_FinalizedSalary_Delete_Audit
    (
          AuditID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY
        , PayrollProcessEmployeeID BIGINT NOT NULL
        , PayrollBatchID BIGINT NOT NULL
        , PreviewBatchID BIGINT NULL
        , EmployeeID BIGINT NOT NULL
        , PayGroupID BIGINT NULL
        , ProcessMonth INT NOT NULL
        , ProcessYear INT NOT NULL
        , TotalGross DECIMAL(18,2) NULL
        , TotalDeduction DECIMAL(18,2) NULL
        , TotalTakeHome DECIMAL(18,2) NULL
        , Reason VARCHAR(500) NOT NULL
        , DeletedBy VARCHAR(100) NOT NULL
        , DeletedOn DATETIME NOT NULL CONSTRAINT DF_Payroll_FinalizedSalary_Delete_Audit_DeletedOn DEFAULT (GETDATE())
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_Rollback_FinalizedSalary_EmployeeWise
(
      @ProcessMonth INT
    , @ProcessYear INT
    , @PayrollProcessEmployeeIDs VARCHAR(MAX)
    , @Reason VARCHAR(500)
    , @UserName VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        ------------------------------------------------------------
        -- Employees to Rollback
        ------------------------------------------------------------
        SELECT
              E.PayrollProcessEmployeeID
            , E.PayrollBatchID
            , E.PayGroupID
            , E.EmployeeID
            , B.PreviewBatchID
            , E.TotalGross
            , E.TotalDeduction
            , E.TotalTakeHome
        INTO #EmployeeToRollback
        FROM Payroll_SaaS_Salary_Process_Employee E
        INNER JOIN Payroll_SaaS_Salary_Process_Batch B
            ON B.PayrollBatchID = E.PayrollBatchID
        INNER JOIN
        (
            SELECT DISTINCT TRY_CAST(value AS BIGINT) AS PayrollProcessEmployeeID
            FROM STRING_SPLIT(@PayrollProcessEmployeeIDs, ',')
            WHERE TRY_CAST(value AS BIGINT) IS NOT NULL
        ) X
            ON X.PayrollProcessEmployeeID = E.PayrollProcessEmployeeID
        WHERE B.ProcessMonth = @ProcessMonth
          AND B.ProcessYear = @ProcessYear;

        IF NOT EXISTS (SELECT 1 FROM #EmployeeToRollback)
        BEGIN
            RAISERROR('No finalized employee salary found for rollback.', 16, 1);
            RETURN;
        END

        ------------------------------------------------------------
        -- Audit Selected Employees Before Delete
        ------------------------------------------------------------
        INSERT INTO dbo.Payroll_SaaS_FinalizedSalary_Delete_Audit
        (
              PayrollProcessEmployeeID
            , PayrollBatchID
            , PreviewBatchID
            , EmployeeID
            , PayGroupID
            , ProcessMonth
            , ProcessYear
            , TotalGross
            , TotalDeduction
            , TotalTakeHome
            , Reason
            , DeletedBy
            , DeletedOn
        )
        SELECT
              E.PayrollProcessEmployeeID
            , E.PayrollBatchID
            , E.PreviewBatchID
            , E.EmployeeID
            , E.PayGroupID
            , @ProcessMonth
            , @ProcessYear
            , E.TotalGross
            , E.TotalDeduction
            , E.TotalTakeHome
            , @Reason
            , @UserName
            , GETDATE()
        FROM #EmployeeToRollback E;

        ------------------------------------------------------------
        -- Loan Transactions to Rollback for Selected Employees
        ------------------------------------------------------------
        SELECT LT.*
        INTO #LoanRollback
        FROM Payroll_Employee_Loan_Transaction LT
        WHERE LT.InstallmentMonth = @ProcessMonth
          AND LT.InstallmentYear = @ProcessYear
          AND LT.ProcessSource = 'Salary'
          AND EXISTS
          (
                SELECT 1
                FROM Payroll_SaaS_Salary_Process_Employee_Detail D
                INNER JOIN #EmployeeToRollback E
                    ON E.PayrollProcessEmployeeID = D.PayrollProcessEmployeeID
                WHERE D.PayConfigID = 52
                  AND TRY_CAST(D.ReferenceIds AS BIGINT) = LT.IDLoan
          );

        ------------------------------------------------------------
        -- Reverse Loan Master
        ------------------------------------------------------------
        UPDATE L
           SET BalanceAmount =
                    ISNULL(L.BalanceAmount, 0)
                    + ISNULL(R.PrincipalComponent, 0),

               TotalReceivedAmount =
                    ISNULL(L.TotalReceivedAmount, 0)
                    - ISNULL(R.AmountPaid, 0),

               TotalInterestReceived =
                    ISNULL(L.TotalInterestReceived, 0)
                    - ISNULL(R.InterestPaid, 0),

               TotalWaivedAmount =
                    ISNULL(L.TotalWaivedAmount, 0)
                    - ISNULL(R.WaiverAmount, 0),

               RemainingTenure =
                    ISNULL(L.RemainingTenure, 0) + 1,

               CompleteYN = 0,
               LoanStatus = 'ACTIVE',

               LastProcessedMonth =
                    CASE
                        WHEN @ProcessMonth = 1 THEN 12
                        ELSE @ProcessMonth - 1
                    END,

               LastProcessedYear =
                    CASE
                        WHEN @ProcessMonth = 1 THEN @ProcessYear - 1
                        ELSE @ProcessYear
                    END,

               UpdateUser = @UserName,
               UpdateDate = GETDATE()
        FROM Payroll_EmployeeLoan L
        INNER JOIN #LoanRollback R
            ON L.IDLoan = R.IDLoan;

        ------------------------------------------------------------
        -- Delete Loan Transactions
        ------------------------------------------------------------
        DELETE LT
        FROM Payroll_Employee_Loan_Transaction LT
        INNER JOIN #LoanRollback R
            ON LT.IDTransaction = R.IDTransaction;

        ------------------------------------------------------------
        -- Delete Salary Detail for Selected Employees
        ------------------------------------------------------------
        DELETE D
        FROM Payroll_SaaS_Salary_Process_Employee_Detail D
        INNER JOIN #EmployeeToRollback E
            ON D.PayrollProcessEmployeeID = E.PayrollProcessEmployeeID;

        ------------------------------------------------------------
        -- Delete Salary Employee for Selected Employees
        ------------------------------------------------------------
        DELETE E
        FROM Payroll_SaaS_Salary_Process_Employee E
        INNER JOIN #EmployeeToRollback T
            ON E.PayrollProcessEmployeeID = T.PayrollProcessEmployeeID;

        ------------------------------------------------------------
        -- Delete Empty Batches
        ------------------------------------------------------------
        DELETE B
        FROM Payroll_SaaS_Salary_Process_Batch B
        WHERE EXISTS
        (
            SELECT 1
            FROM #EmployeeToRollback T
            WHERE T.PayrollBatchID = B.PayrollBatchID
        )
        AND NOT EXISTS
        (
            SELECT 1
            FROM Payroll_SaaS_Salary_Process_Employee E
            WHERE E.PayrollBatchID = B.PayrollBatchID
        );

        ------------------------------------------------------------
        -- Restore Preview Batch Status for Affected Preview Batches
        ------------------------------------------------------------
        UPDATE BP
           SET Status = 1,
               ApprovedBy = @UserName,
               ApprovedDate = GETDATE()
        FROM Payroll_SaaS_Salary_Process_Batch_Preview BP
        WHERE EXISTS
        (
            SELECT 1
            FROM #EmployeeToRollback T
            WHERE T.PreviewBatchID = BP.PreviewBatchID
        );

        ------------------------------------------------------------
        -- Restore Preview Employee Status for Selected Employees
        ------------------------------------------------------------
        UPDATE EP
           SET Status = 1
        FROM Payroll_SaaS_Salary_Process_Employee_Preview EP
        INNER JOIN #EmployeeToRollback T
            ON T.PreviewBatchID = EP.PreviewBatchID
           AND T.EmployeeID = EP.EmployeeID;

        DECLARE @DeletedCount INT = (SELECT COUNT(1) FROM #EmployeeToRollback);

        DROP TABLE #EmployeeToRollback;
        DROP TABLE #LoanRollback;

        COMMIT TRANSACTION;

        SELECT
              1 AS Status
            , @DeletedCount AS DeletedCount
            , 'Selected employee finalized salary rollback completed successfully.' AS Message;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        IF OBJECT_ID('tempdb..#EmployeeToRollback') IS NOT NULL
            DROP TABLE #EmployeeToRollback;

        IF OBJECT_ID('tempdb..#LoanRollback') IS NOT NULL
            DROP TABLE #LoanRollback;

        SELECT
              0 AS Status
            , 0 AS DeletedCount
            , ERROR_MESSAGE() AS Message;
    END CATCH
END
