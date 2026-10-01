IF OBJECT_ID('dbo.Payroll_SaaS_Attendance_Cache', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payroll_SaaS_Attendance_Cache
    (
        AttendanceCacheID BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Payroll_SaaS_Attendance_Cache PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        PayGroupID INT NOT NULL,
        EmployeeID INT NOT NULL,
        ProcessMonth INT NOT NULL,
        ProcessYear INT NOT NULL,
        EmployeeName NVARCHAR(250) NULL,
        PaidDays DECIMAL(18,2) NOT NULL,
        CreatedBy VARCHAR(100) NULL,
        CreatedTime DATETIME NOT NULL
            CONSTRAINT DF_Payroll_SaaS_Attendance_Cache_CreatedTime DEFAULT (GETDATE()),
        UpdatedBy VARCHAR(100) NULL,
        UpdatedTime DATETIME NULL
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Payroll_SaaS_Attendance_Cache_PerEmployeeMonth'
      AND object_id = OBJECT_ID('dbo.Payroll_SaaS_Attendance_Cache')
)
BEGIN
    CREATE UNIQUE INDEX UX_Payroll_SaaS_Attendance_Cache_PerEmployeeMonth
    ON dbo.Payroll_SaaS_Attendance_Cache
    (
        TenantId,
        PayGroupID,
        EmployeeID,
        ProcessMonth,
        ProcessYear
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Payroll_AttendanceCache_Get
(
    @TenantId UNIQUEIDENTIFIER,
    @PayGroupID INT,
    @EmployeeIds VARCHAR(MAX),
    @ProcessMonth INT,
    @ProcessYear INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.EmployeeID AS EmployeeId,
        C.EmployeeName,
        C.PaidDays AS PaidUnits
    FROM dbo.Payroll_SaaS_Attendance_Cache C
    INNER JOIN
    (
        SELECT DISTINCT TRY_CAST(value AS INT) AS EmployeeID
        FROM STRING_SPLIT(@EmployeeIds, ',')
        WHERE TRY_CAST(value AS INT) IS NOT NULL
    ) E
        ON E.EmployeeID = C.EmployeeID
    WHERE C.TenantId = @TenantId
      AND C.PayGroupID = @PayGroupID
      AND C.ProcessMonth = @ProcessMonth
      AND C.ProcessYear = @ProcessYear
    ORDER BY C.EmployeeName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Payroll_AttendanceCache_Save
(
    @TenantId UNIQUEIDENTIFIER,
    @PayGroupID INT,
    @ProcessMonth INT,
    @ProcessYear INT,
    @AttendanceJson NVARCHAR(MAX),
    @UserName VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH AttendanceRows AS
    (
        SELECT
            EmployeeID,
            EmployeeName,
            PaidDays
        FROM OPENJSON(@AttendanceJson)
        WITH
        (
            EmployeeID INT '$.EmployeeId',
            EmployeeName NVARCHAR(250) '$.EmployeeName',
            PaidDays DECIMAL(18,2) '$.PaidDays'
        )
        WHERE EmployeeID IS NOT NULL
    )
    MERGE dbo.Payroll_SaaS_Attendance_Cache AS T
    USING AttendanceRows AS S
       ON T.TenantId = @TenantId
      AND T.PayGroupID = @PayGroupID
      AND T.EmployeeID = S.EmployeeID
      AND T.ProcessMonth = @ProcessMonth
      AND T.ProcessYear = @ProcessYear
    WHEN MATCHED THEN
        UPDATE SET
            T.EmployeeName = S.EmployeeName,
            T.PaidDays = ISNULL(S.PaidDays, 0),
            T.UpdatedBy = @UserName,
            T.UpdatedTime = GETDATE()
    WHEN NOT MATCHED BY TARGET THEN
        INSERT
        (
            TenantId,
            PayGroupID,
            EmployeeID,
            ProcessMonth,
            ProcessYear,
            EmployeeName,
            PaidDays,
            CreatedBy
        )
        VALUES
        (
            @TenantId,
            @PayGroupID,
            S.EmployeeID,
            @ProcessMonth,
            @ProcessYear,
            S.EmployeeName,
            ISNULL(S.PaidDays, 0),
            @UserName
        );
END;
GO
