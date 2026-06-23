IF OBJECT_ID('dbo.Payroll_GetEmployeeTDS', 'P') IS NOT NULL
    DROP PROCEDURE dbo.Payroll_GetEmployeeTDS;
GO

CREATE PROCEDURE dbo.Payroll_GetEmployeeTDS
(
    @EmployeeId BIGINT,
    @SalaryMonth INT,
    @SalaryYear INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @SalaryDate DATE,
        @FYId INT,
        @MonthlyTDS DECIMAL(18, 2);

    SET @SalaryDate = DATEFROMPARTS(@SalaryYear, @SalaryMonth, 1);

    SELECT TOP (1)
        @FYId = FY.IDFinancialYear
    FROM dbo.Payroll_FinancialYear FY
    WHERE @SalaryDate BETWEEN CAST(FY.StartDate AS DATE) AND CAST(FY.EndDate AS DATE)
      AND ISNULL(FY.IsActive, 1) = 1
    ORDER BY FY.StartDate DESC;

    SELECT TOP (1)
        @MonthlyTDS = P.MonthlyTDS
    FROM dbo.Payroll_SaaS_TDS_Projection P
    WHERE P.EmployeeId = @EmployeeId
      AND P.FinancialYear = @FYId
      AND ISNULL(P.ActiveFlag, 1) = 1
    ORDER BY
        ISNULL(P.IsFinalized, 0) DESC,
        P.UpdatedOn DESC,
        P.CreatedOn DESC,
        P.ProjectionId DESC;

    SELECT ISNULL(@MonthlyTDS, 0) AS TDSAmount;
END;
GO
