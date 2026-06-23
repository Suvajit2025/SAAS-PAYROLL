IF OBJECT_ID('dbo.SP_PAYROLL_SAAS_TDS_PROJECTION_DETAIL_GET', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_PAYROLL_SAAS_TDS_PROJECTION_DETAIL_GET;
GO

CREATE PROCEDURE dbo.SP_PAYROLL_SAAS_TDS_PROJECTION_DETAIL_GET
    @TenantId NVARCHAR(100),
    @FinancialYear NVARCHAR(20),
    @RegimeId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.ProjectionId,
        P.EmployeeId AS EmployeeID,
        ISNULL(
            dbo.Employee_FullName(E.empfirstname, E.empmiddlename, E.emplastname) + ' (' + CAST(E.empno AS VARCHAR(20)) + ')',
            CAST(P.EmployeeId AS NVARCHAR(50))
        ) AS EmployeeName,
        P.AnnualGross,
        P.NetTaxable,
        P.TotalTaxBeforeCess,
        P.CessAmount,
        P.AnnualTax,
        P.MonthlyTDS,
        P.RebateApplied,
        CASE
            WHEN ISNULL(P.IsFinalized, 0) = 1 THEN 'Finalized'
            ELSE 'Draft'
        END AS Status
    FROM dbo.Payroll_SaaS_TDS_Projection P
    LEFT JOIN dbo.Empbasic E
        ON E.empid = P.EmployeeId
       AND E.TenantId = P.TenantId
    WHERE P.TenantId = @TenantId
      AND P.FinancialYear = @FinancialYear
      AND P.RegimeId = @RegimeId
      AND ISNULL(P.ActiveFlag, 1) = 1
    ORDER BY EmployeeName;
END;
GO
