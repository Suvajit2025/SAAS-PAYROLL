IF OBJECT_ID('dbo.SP_PAYROLL_SAAS_TDS_PROJECTION_MARK_FINALIZE', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_PAYROLL_SAAS_TDS_PROJECTION_MARK_FINALIZE;
GO

CREATE PROCEDURE dbo.SP_PAYROLL_SAAS_TDS_PROJECTION_MARK_FINALIZE
    @TenantId NVARCHAR(100),
    @FinancialYear NVARCHAR(20),
    @RegimeId INT,
    @FinalizedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Payroll_SaaS_TDS_Projection
    SET
        IsFinalized = 1,
        FinalizedOn = GETDATE(),
        FinalizedBy = @FinalizedBy,
        UpdatedOn = GETDATE(),
        UpdatedBy = @FinalizedBy
    WHERE TenantId = @TenantId
      AND FinancialYear = @FinancialYear
      AND RegimeId = @RegimeId
      AND ISNULL(ActiveFlag, 1) = 1
      AND ISNULL(IsFinalized, 0) = 0;

    SELECT
        @@ROWCOUNT AS AffectedRows,
        CASE
            WHEN @@ROWCOUNT > 0 THEN 'Projection finalized successfully.'
            ELSE 'Projection is already finalized or no active draft rows were found.'
        END AS Message;
END;
GO
