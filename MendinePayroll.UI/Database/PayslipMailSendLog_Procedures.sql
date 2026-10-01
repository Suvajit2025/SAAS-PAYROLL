IF OBJECT_ID('dbo.tbl_Saas_Payslip_Mail_Send_Log', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Saas_Payslip_Mail_Send_Log
    (
        Id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_tbl_Saas_Payslip_Mail_Send_Log PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        EmployeeId INT NOT NULL,
        ProcessMonth INT NOT NULL,
        ProcessYear INT NOT NULL,
        ToEmail VARCHAR(250) NULL,
        SendStatus VARCHAR(20) NOT NULL,
        AttemptCount INT NOT NULL
            CONSTRAINT DF_tbl_Saas_Payslip_Mail_Send_Log_AttemptCount DEFAULT (1),
        LastError NVARCHAR(MAX) NULL,
        CreatedTime DATETIME NOT NULL
            CONSTRAINT DF_tbl_Saas_Payslip_Mail_Send_Log_CreatedTime DEFAULT (GETDATE()),
        UpdatedTime DATETIME NULL
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_tbl_Saas_Payslip_Mail_Send_Log_PerEmployeeMonth'
      AND object_id = OBJECT_ID('dbo.tbl_Saas_Payslip_Mail_Send_Log')
)
BEGIN
    CREATE UNIQUE INDEX UX_tbl_Saas_Payslip_Mail_Send_Log_PerEmployeeMonth
    ON dbo.tbl_Saas_Payslip_Mail_Send_Log
    (
        TenantId,
        EmployeeId,
        ProcessMonth,
        ProcessYear
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Saas_PayslipMailSendLog_TryBegin
(
    @TenantId UNIQUEIDENTIFIER,
    @EmployeeId INT,
    @ProcessMonth INT,
    @ProcessYear INT,
    @ToEmail VARCHAR(250)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @CanSend BIT = 0;

    BEGIN TRANSACTION;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.tbl_Saas_Payslip_Mail_Send_Log WITH (UPDLOCK, HOLDLOCK)
        WHERE TenantId = @TenantId
          AND EmployeeId = @EmployeeId
          AND ProcessMonth = @ProcessMonth
          AND ProcessYear = @ProcessYear
          AND SendStatus IN ('Processing', 'Sent')
    )
    BEGIN
        SET @CanSend = 0;
    END
    ELSE IF EXISTS
    (
        SELECT 1
        FROM dbo.tbl_Saas_Payslip_Mail_Send_Log WITH (UPDLOCK, HOLDLOCK)
        WHERE TenantId = @TenantId
          AND EmployeeId = @EmployeeId
          AND ProcessMonth = @ProcessMonth
          AND ProcessYear = @ProcessYear
    )
    BEGIN
        UPDATE dbo.tbl_Saas_Payslip_Mail_Send_Log
        SET SendStatus = 'Processing',
            ToEmail = @ToEmail,
            AttemptCount = AttemptCount + 1,
            LastError = NULL,
            UpdatedTime = GETDATE()
        WHERE TenantId = @TenantId
          AND EmployeeId = @EmployeeId
          AND ProcessMonth = @ProcessMonth
          AND ProcessYear = @ProcessYear;

        SET @CanSend = 1;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.tbl_Saas_Payslip_Mail_Send_Log
        (
            TenantId,
            EmployeeId,
            ProcessMonth,
            ProcessYear,
            ToEmail,
            SendStatus
        )
        VALUES
        (
            @TenantId,
            @EmployeeId,
            @ProcessMonth,
            @ProcessYear,
            @ToEmail,
            'Processing'
        );

        SET @CanSend = 1;
    END;

    COMMIT TRANSACTION;

    SELECT @CanSend AS CanSend;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Saas_PayslipMailSendLog_Complete
(
    @TenantId UNIQUEIDENTIFIER,
    @EmployeeId INT,
    @ProcessMonth INT,
    @ProcessYear INT,
    @SendStatus VARCHAR(20),
    @LastError NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tbl_Saas_Payslip_Mail_Send_Log
    SET SendStatus = @SendStatus,
        LastError = NULLIF(@LastError, ''),
        UpdatedTime = GETDATE()
    WHERE TenantId = @TenantId
      AND EmployeeId = @EmployeeId
      AND ProcessMonth = @ProcessMonth
      AND ProcessYear = @ProcessYear;
END;
GO
