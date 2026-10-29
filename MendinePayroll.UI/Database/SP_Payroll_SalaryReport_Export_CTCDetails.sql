-- Apply before deploying the tenant-specific salary Excel export.
-- Selected employees come from SP_Payroll_GetEmployeeStatusesForValidation.
-- JSON avoids coupling INSERT...EXEC to that procedure's result column count.
CREATE OR ALTER PROCEDURE [dbo].[SP_Payroll_SalaryReport_Export_CTCDetails]
(
    @TenantId UNIQUEIDENTIFIER,
    @Month INT,
    @Year INT,
    @SelectedEmployeesJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Requested AS
    (
        SELECT DISTINCT RequestedID, EmployeeId, Status
        FROM OPENJSON(@SelectedEmployeesJson)
        WITH
        (
            RequestedID BIGINT '$.RequestedID',
            EmployeeId INT '$.EmployeeId',
            Status INT '$.Status'
        )
    ),
    SelectedPayroll AS
    (
        SELECT R.RequestedID, R.EmployeeId, R.Status, E.PayGroupID
        FROM Requested AS R
        INNER JOIN dbo.Payroll_SaaS_Salary_Process_Employee AS E
            ON E.PayrollProcessEmployeeID = R.RequestedID
            AND E.EmployeeID = R.EmployeeId
        INNER JOIN dbo.Payroll_SaaS_Salary_Process_Batch AS B
            ON B.PayrollBatchID = E.PayrollBatchID
            AND B.TenantID = @TenantId
            AND B.ProcessMonth = @Month
            AND B.ProcessYear = @Year
        WHERE R.Status = 2

        UNION ALL

        SELECT R.RequestedID, R.EmployeeId, R.Status, E.PayGroupID
        FROM Requested AS R
        INNER JOIN dbo.Payroll_SaaS_Salary_Process_Employee_Preview AS E
            ON E.PreviewEmployeeID = R.RequestedID
            AND E.EmployeeID = R.EmployeeId
        INNER JOIN dbo.Payroll_SaaS_Salary_Process_Batch_Preview AS B
            ON B.PreviewBatchID = E.PreviewBatchID
            AND B.TenantID = @TenantId
            AND B.ProcessMonth = @Month
            AND B.ProcessYear = @Year
        WHERE R.Status IN (1, 3)
    ),
    EmployerDetails AS
    (
        SELECT P.RequestedID, P.Status, D.PayConfigID, D.PayValue
        FROM SelectedPayroll AS P
        INNER JOIN dbo.Payroll_SaaS_Salary_Process_Employee_Detail AS D
            ON D.PayrollProcessEmployeeID = P.RequestedID
        WHERE P.Status = 2

        UNION ALL

        SELECT P.RequestedID, P.Status, D.PayConfigID, D.PayValue
        FROM SelectedPayroll AS P
        INNER JOIN dbo.Payroll_SaaS_Salary_Process_Employee_Detail_Preview AS D
            ON D.PreviewEmployeeID = P.RequestedID
        WHERE P.Status IN (1, 3)
    )
    SELECT
        EB.empcode AS EmpCode,
        PG.PayGroupName AS [Salary Format],
        CASE P.Status WHEN 1 THEN 'Draft' WHEN 2 THEN 'Finalized' ELSE 'Rejected' END AS Status,
        SC.MonthlyCTC,
        SC.AnnualCTC,
        SUM(CASE WHEN UPPER(LTRIM(RTRIM(PC.StatutoryType))) = 'PF_ER'
            THEN ISNULL(D.PayValue, 0) ELSE 0 END) AS EmployerPF,
        SUM(CASE WHEN UPPER(LTRIM(RTRIM(PC.StatutoryType))) = 'ESIC_ER'
            THEN ISNULL(D.PayValue, 0) ELSE 0 END) AS EmployerESIC,
        SUM(CASE WHEN UPPER(LTRIM(RTRIM(PC.StatutoryType))) = 'GRATUITY'
                OR UPPER(LTRIM(RTRIM(PC.PayConfigName))) = 'GRATUITY'
            THEN ISNULL(D.PayValue, 0) ELSE 0 END) AS Gratuity,
        SUM(CASE WHEN UPPER(LTRIM(RTRIM(PC.OtherType))) IN ('EX_GRATIA', 'EXGRATIA', 'EX GRATIA')
                OR UPPER(LTRIM(RTRIM(PC.PayConfigName))) IN ('EX GRATIA', 'EX-GRATIA', 'EXGRATIA')
            THEN ISNULL(D.PayValue, 0) ELSE 0 END) AS ExGratia,
        SUM(CASE WHEN PC.PayConfigId IS NOT NULL
            THEN ISNULL(D.PayValue, 0) ELSE 0 END) AS TotalEmployerContribution
    FROM SelectedPayroll AS P
    INNER JOIN dbo.Empbasic AS EB
        ON EB.empid = P.EmployeeId
        AND EB.TenantID = @TenantId
    -- CTC comes from the employee's active salary configuration.
    LEFT JOIN dbo.Payroll_EmployeeSalaryConfigaration AS SC
        ON SC.EmployeeID = P.EmployeeId
        AND SC.TenantID = @TenantId
        AND SC.ActiveFlag = 1
    LEFT JOIN dbo.PayGroup AS PG
        ON PG.PayGroupID = P.PayGroupID
        AND PG.TenantID = @TenantId
    LEFT JOIN EmployerDetails AS D
        ON D.RequestedID = P.RequestedID
        AND D.Status = P.Status
    LEFT JOIN dbo.PayConfig AS PC
        ON PC.PayConfigId = D.PayConfigID
        AND PC.TenantID = @TenantId
        AND UPPER(LTRIM(RTRIM(PC.PayConfigType))) = 'CC'
    -- Historical payroll values remain reportable after a component is deactivated.
    GROUP BY P.RequestedID, P.EmployeeId, P.Status, EB.empcode, PG.PayGroupName,
        SC.MonthlyCTC, SC.AnnualCTC;
END;
GO
