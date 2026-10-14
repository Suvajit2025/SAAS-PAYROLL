-- Run before deploying the ESIC configuration changes.
-- ESIC eligibility defaults to full-month gross; tenants may opt into prorated gross.
IF COL_LENGTH('dbo.Payroll_CycleConfiguration', 'ESICEligibilityOnProratedGross') IS NULL
BEGIN
    ALTER TABLE dbo.Payroll_CycleConfiguration
    ADD ESICEligibilityOnProratedGross BIT NOT NULL
        CONSTRAINT DF_Payroll_CycleConfiguration_ESICEligibilityOnProratedGross DEFAULT (0) WITH VALUES;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_PayrollCycleConfiguration_List
(
    @TenantID UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PayrollConfigID,
        TenantID,
        TypeID,
        PayrollCycleType,
        StartDay,
        EndDay,
        PaidDaysSource,
        ESICEligibilityOnProratedGross,
        IsActive,
        CreatedBy,
        CreatedOn,
        ModifiedBy,
        ModifiedOn
    FROM dbo.Payroll_CycleConfiguration
    WHERE TenantID = @TenantID
      AND IsActive = 1
    ORDER BY TypeID, PayrollConfigID DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_PayrollCycleConfiguration_GetById
(
    @PayrollConfigID INT,
    @TenantID UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        PayrollConfigID,
        TenantID,
        TypeID,
        PayrollCycleType,
        StartDay,
        EndDay,
        PaidDaysSource,
        ESICEligibilityOnProratedGross,
        IsActive
    FROM dbo.Payroll_CycleConfiguration
    WHERE PayrollConfigID = @PayrollConfigID
      AND TenantID = @TenantID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_PayrollCycleConfiguration_GetActive
(
    @TenantID UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        PayrollConfigID,
        TenantID,
        TypeID,
        PayrollCycleType,
        StartDay,
        EndDay,
        PaidDaysSource,
        ESICEligibilityOnProratedGross,
        IsActive
    FROM dbo.Payroll_CycleConfiguration
    WHERE TenantID = @TenantID
      AND IsActive = 1
    ORDER BY PayrollConfigID DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_PayrollCycleConfiguration_Save
(
    @PayrollConfigID INT,
    @TenantID UNIQUEIDENTIFIER,
    @TypeID TINYINT,
    @PayrollCycleType TINYINT = NULL,
    @StartDay TINYINT = NULL,
    @EndDay TINYINT = NULL,
    @PaidDaysSource TINYINT = 1,
    @UserName VARCHAR(50),
    @ESICEligibilityOnProratedGross BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @ESICEligibilityOnProratedGross IS NULL
        BEGIN
            SELECT 0 AS Code, 'Select a valid ESIC eligibility basis.' AS Message, @PayrollConfigID AS PayrollConfigID;
            RETURN;
        END;

        IF @TypeID NOT IN (1, 2)
        BEGIN
            SELECT 0 AS Code, 'Invalid payroll type.' AS Message, @PayrollConfigID AS PayrollConfigID;
            RETURN;
        END;

        IF ISNULL(@PaidDaysSource, 1) NOT IN (1, 2)
        BEGIN
            SELECT 0 AS Code, 'Invalid paid days source.' AS Message, @PayrollConfigID AS PayrollConfigID;
            RETURN;
        END;

        IF @TypeID = 2
        BEGIN
            SET @PayrollCycleType = NULL;
            SET @StartDay = NULL;
            SET @EndDay = NULL;
        END;

        IF @TypeID = 1 AND @PayrollCycleType NOT IN (1, 2)
        BEGIN
            SELECT 0 AS Code, 'Invalid payroll cycle type.' AS Message, @PayrollConfigID AS PayrollConfigID;
            RETURN;
        END;

        IF @TypeID = 1 AND @PayrollCycleType = 1
        BEGIN
            SET @StartDay = NULL;
            SET @EndDay = NULL;
        END;

        IF @TypeID = 1 AND @PayrollCycleType = 2
           AND (
                ISNULL(@StartDay, 0) NOT BETWEEN 1 AND 31
                OR ISNULL(@EndDay, 0) NOT BETWEEN 1 AND 31
           )
        BEGIN
            SELECT 0 AS Code, 'Start Day and End Day must be between 1 and 31.' AS Message, @PayrollConfigID AS PayrollConfigID;
            RETURN;
        END;

        BEGIN TRANSACTION;

        IF ISNULL(@PayrollConfigID, 0) > 0
        BEGIN
            IF EXISTS
            (
                SELECT 1
                FROM dbo.Payroll_CycleConfiguration
                WHERE TenantID = @TenantID
                  AND IsActive = 1
                  AND PayrollConfigID <> @PayrollConfigID
            )
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 0 AS Code, 'Only one active payroll cycle configuration is allowed for this tenant.' AS Message, @PayrollConfigID AS PayrollConfigID;
                RETURN;
            END;

            IF EXISTS
            (
                SELECT 1
                FROM dbo.Payroll_CycleConfiguration
                WHERE TenantID = @TenantID
                  AND TypeID = @TypeID
                  AND IsActive = 1
                  AND PayrollConfigID <> @PayrollConfigID
            )
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 0 AS Code, 'A configuration already exists for this payroll type.' AS Message, @PayrollConfigID AS PayrollConfigID;
                RETURN;
            END;

            UPDATE dbo.Payroll_CycleConfiguration
            SET
                TypeID = @TypeID,
                PayrollCycleType = @PayrollCycleType,
                StartDay = @StartDay,
                EndDay = @EndDay,
                PaidDaysSource = @PaidDaysSource,
                ESICEligibilityOnProratedGross = @ESICEligibilityOnProratedGross,
                IsActive = 1,
                ModifiedBy = @UserName,
                ModifiedOn = GETDATE()
            WHERE PayrollConfigID = @PayrollConfigID
              AND TenantID = @TenantID;

            IF @@ROWCOUNT = 0
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 0 AS Code, 'Configuration not found.' AS Message, @PayrollConfigID AS PayrollConfigID;
                RETURN;
            END;
        END;
        ELSE
        BEGIN
            IF EXISTS
            (
                SELECT 1
                FROM dbo.Payroll_CycleConfiguration
                WHERE TenantID = @TenantID
                  AND IsActive = 1
            )
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 0 AS Code, 'Only one active payroll cycle configuration is allowed for this tenant. Please edit or delete the existing setup.' AS Message, 0 AS PayrollConfigID;
                RETURN;
            END;

            INSERT INTO dbo.Payroll_CycleConfiguration
            (
                TenantID,
                TypeID,
                PayrollCycleType,
                StartDay,
                EndDay,
                PaidDaysSource,
                ESICEligibilityOnProratedGross,
                IsActive,
                CreatedBy,
                CreatedOn
            )
            VALUES
            (
                @TenantID,
                @TypeID,
                @PayrollCycleType,
                @StartDay,
                @EndDay,
                @PaidDaysSource,
                @ESICEligibilityOnProratedGross,
                1,
                @UserName,
                GETDATE()
            );

            SET @PayrollConfigID = CAST(SCOPE_IDENTITY() AS INT);
        END;

        COMMIT TRANSACTION;

        SELECT 1 AS Code, 'Payroll cycle configuration saved successfully.' AS Message, @PayrollConfigID AS PayrollConfigID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 0 AS Code, ERROR_MESSAGE() AS Message, ISNULL(@PayrollConfigID, 0) AS PayrollConfigID;
    END CATCH;
END;
GO

