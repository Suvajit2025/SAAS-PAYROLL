CREATE OR ALTER PROCEDURE [dbo].[SP_Payroll_GetVariableComponentInput]
(
    @EmployeeID   INT,
    @PayConfigID  INT,
    @Month        INT,
    @Year         INT,
    @TenantID     UNIQUEIDENTIFIER,
    @PayGroupID   INT,
    @PayGroupName NVARCHAR(250) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ISNULL(MAX(CI.InputValue), 0) AS InputValue
    FROM [dbo].[Payroll_SaaS_Variable_Component_Inputs] AS CI
    INNER JOIN [dbo].[ConfigureSalaryComponent] AS SC
        ON SC.ConfigureSalaryComponentID = CI.ConfigureSalaryComponentID
    INNER JOIN [dbo].[PayConfig] AS PC
        ON PC.PayConfigId = SC.PayConfigId
    WHERE CI.EmployeeID = @EmployeeID
      AND PC.PayConfigId = @PayConfigID
      AND CI.[Month] = @Month
      AND CI.[Year] = @Year
      AND CI.TenantID = @TenantID
      AND CI.PayGroupID = @PayGroupID;
END;
GO
