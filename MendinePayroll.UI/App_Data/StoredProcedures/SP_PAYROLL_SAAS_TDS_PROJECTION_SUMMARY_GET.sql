--SP_PAYROLL_SAAS_TDS_PROJECTION_SUMMARY_GET '6B1B6590-C5CA-4FD6-A0BB-FEBA6DB8FB14'  
ALTER PROCEDURE dbo.SP_PAYROLL_SAAS_TDS_PROJECTION_SUMMARY_GET  
    @TenantId NVARCHAR(100)  
AS  
BEGIN  
    SET NOCOUNT ON;  
  
    SELECT  
	    FinancialYearCode,
        FinancialYear,  
        RegimeId,  
        CASE  
            WHEN RegimeId = 1 THEN 'Old Regime'  
            WHEN RegimeId = 2 THEN 'New Regime'  
            ELSE CONCAT('Regime ', RegimeId)  
        END AS RegimeName,  
        COUNT(DISTINCT EmployeeId) AS EmployeeCount,  
        SUM(ISNULL(TotalTaxBeforeCess, 0)) AS TotalTaxBeforeCess,  
        SUM(ISNULL(CessAmount, 0)) AS TotalCessAmount,  
        SUM(ISNULL(AnnualTax, 0)) AS TotalTDS,  
        MAX(FinalizedBy) AS FinalizedBy,  
        MAX(FinalizedOn) AS FinalizedOn,  
        CASE  
            WHEN MAX(CAST(ISNULL(IsFinalized, 0) AS INT)) = 1 THEN 'Finalized'  
            ELSE 'Draft'  
        END AS Status  
    FROM dbo.Payroll_SaaS_TDS_Projection ST
	Inner Join Payroll_FinancialYear PF ON PF.IDFinancialYear=ST.FinancialYear
    WHERE TenantId = @TenantId  
      AND ISNULL(ActiveFlag, 1) = 1  
    GROUP BY  
	    FinancialYearCode,
        FinancialYear,  
        RegimeId  
    ORDER BY  
        FinancialYear DESC,  
        RegimeId;  
END;  