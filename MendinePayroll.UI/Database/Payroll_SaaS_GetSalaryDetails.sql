-- Return active Pay Group components, plus saved Gross when it is not
-- configured in the Pay Group. Saved rows must not restore inactive components.
CREATE OR ALTER PROCEDURE [dbo].[Payroll_SaaS_GetSalaryDetails]
(
    @EmpSalaryConfigID INT,
    @TenantId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH EmployeeConfig AS
    (
        SELECT EmpSalaryConfigID, TenantID, PayGroupID
        FROM [dbo].[Payroll_EmployeeSalaryConfigaration]
        WHERE EmpSalaryConfigID = @EmpSalaryConfigID
          AND TenantID = @TenantId
    ),
    CurrentComponents AS
    (
        SELECT CSC.PayConfigId, CSC.PayType,
            CSC.TimeBasis, CSC.LogicType, CSC.MappedColumn,
            CSC.CalculationFormula, CSC.ManualRate, CSC.ISPercentage
        FROM EmployeeConfig AS ES
        INNER JOIN [dbo].[SalaryConfigure] AS SC
            ON SC.PayGroupID = ES.PayGroupID
            AND SC.TenantID = ES.TenantID
            AND SC.ActiveFlag = 1
        INNER JOIN [dbo].[ConfigureSalaryComponent] AS CSC
            ON CSC.SalaryConfigureID = SC.SalaryConfigureID
            AND CSC.TenantID = ES.TenantID
            AND CSC.ActiveFlag = 1
    ),
    ComponentIds AS
    (
        SELECT PayConfigId
        FROM CurrentComponents

        UNION

        SELECT EC.PayConfigId
        FROM [dbo].[Payroll_detail_EmployeeSalaryConfigaration] AS EC
        INNER JOIN EmployeeConfig AS ES
            ON ES.EmpSalaryConfigID = EC.EmpSalaryConfigID
            AND ES.TenantID = EC.TenantID
        INNER JOIN [dbo].[PayConfig] AS PC
            ON PC.PayConfigId = EC.PayConfigId
            AND PC.TenantID = ES.TenantID
            AND PC.IsGrossComponent = 1
            AND PC.IsActive = 1
        -- Gross may be absent from configuration, but must not be explicitly
        -- deactivated in the active Pay Group configuration.
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[SalaryConfigure] AS SC
            INNER JOIN [dbo].[ConfigureSalaryComponent] AS CSC
                ON CSC.SalaryConfigureID = SC.SalaryConfigureID
                AND CSC.TenantID = SC.TenantID
            WHERE SC.PayGroupID = ES.PayGroupID
              AND SC.TenantID = ES.TenantID
              AND SC.ActiveFlag = 1
              AND CSC.PayConfigId = EC.PayConfigId
              AND CSC.ActiveFlag = 0
        )
    )
    SELECT
        EC.EmpSalaryDetailID,
        ES.EmpSalaryConfigID,
        ES.TenantID,

        C.PayConfigId,
        PC.PayConfigName,

        COALESCE(CSC.PayType, EC.PayType) AS PayType,
        -- New components have no employee amounts until assigned/calculated.
        EC.MonthlyAmount,
        EC.YearlyAmount,

        -- Use current configuration when the component is configured.
        -- For saved-only components, use the employee's saved configuration.
        CASE WHEN CSC.PayConfigId IS NULL THEN EC.TimeBasis
            ELSE CSC.TimeBasis END AS TimeBasis,
        CASE WHEN CSC.PayConfigId IS NULL THEN EC.LogicType
            ELSE CSC.LogicType END AS LogicType,
        CASE WHEN CSC.PayConfigId IS NULL THEN EC.MappedColumn
            ELSE CSC.MappedColumn END AS MappedColumn,
        CASE WHEN CSC.PayConfigId IS NULL THEN EC.CalculationFormula
            ELSE CSC.CalculationFormula END AS CalculationFormula,
        CASE WHEN CSC.PayConfigId IS NULL THEN EC.ManualRate
            ELSE CSC.ManualRate END AS ManualRate,
        CASE WHEN CSC.PayConfigId IS NULL THEN EC.ISPercentage
            ELSE CSC.ISPercentage END AS ISPercentage,

        PC.IsAllowance,
        PC.AllowanceType,
        PC.IsProratable,
        PC.IsIncludedInGross,
        PC.IsBasicComponent,
        PC.IsGrossComponent,
        PC.IsTaxable,
        PC.IsStatutory,
        PC.StatutoryType,

        SR.CalculationRule,

        EC.IsOther,
        EC.OtherType,
        EC.MaxLimit,
        EC.RoundingType,
        EC.PTaxSlabsJson

    FROM EmployeeConfig AS ES
    CROSS JOIN ComponentIds AS C

    LEFT JOIN [dbo].[Payroll_detail_EmployeeSalaryConfigaration] AS EC
        ON EC.EmpSalaryConfigID = ES.EmpSalaryConfigID
        AND EC.PayConfigId = C.PayConfigId
        AND ES.TenantID = EC.TenantID

    -- Current configuration is optional for saved-only components such as Gross.
    LEFT JOIN CurrentComponents AS CSC
        ON CSC.PayConfigId = C.PayConfigId

    -- Require an active master component for every result, including saved Gross.
    INNER JOIN [dbo].[PayConfig] AS PC
        ON PC.PayConfigId = C.PayConfigId
        AND PC.TenantID = ES.TenantID
        AND PC.IsActive = 1

    LEFT JOIN [dbo].[Payroll_StatutoryRules] AS SR
        ON SR.PayConfigId = C.PayConfigId
        AND SR.StatutoryType = PC.StatutoryType
        AND SR.TenantID = ES.TenantID
        AND SR.ActiveFlag = 1

    ORDER BY
        CASE
            WHEN PC.IsGrossComponent = 1 THEN 0
            WHEN PC.IsBasicComponent = 1 THEN 1
            WHEN COALESCE(CSC.PayType, EC.PayType) = 'Allowances' THEN 2
            WHEN COALESCE(CSC.PayType, EC.PayType) = 'Deduction' THEN 3
            WHEN COALESCE(CSC.PayType, EC.PayType) = 'CC' THEN 4
            ELSE 5
        END,
        PC.SortOrder,
        EC.EmpSalaryDetailID;
END;
GO
