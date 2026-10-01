IF EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE name = 'UQ_Payroll_Configuration'
      AND parent_object_id = OBJECT_ID('dbo.Payroll_CycleConfiguration')
)
BEGIN
    ALTER TABLE dbo.Payroll_CycleConfiguration
    DROP CONSTRAINT UQ_Payroll_Configuration;
END;
GO
