ALTER PROC dbo.SP_Payroll_GetAttendanceEmployees
(
    @TenantId UNIQUEIDENTIFIER,
    @PayGroupId INT,
    @PayrollType INT,          -- 1 = Monthly, 2 = Contractual
    @EmployeeIds VARCHAR(MAX), -- comma separated
    @ProcessMonth INT = NULL,
    @ProcessYear  INT = NULL,
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RangeStart DATE;
    DECLARE @RangeEnd   DATE;

    IF @FromDate IS NOT NULL AND @ToDate IS NOT NULL
    BEGIN
        SET @RangeStart = @FromDate;
        SET @RangeEnd   = @ToDate;
    END
    ELSE IF @PayrollType = 1
    BEGIN
        SET @RangeStart = DATEFROMPARTS(@ProcessYear, @ProcessMonth, 1);
        SET @RangeEnd   = EOMONTH(@RangeStart);
    END
    ELSE
    BEGIN
        SET @RangeStart = @FromDate;
        SET @RangeEnd   = @ToDate;
    END

    SELECT
        e.empid AS EmployeeId,
        dbo.Employee_FullName(e.empfirstname, e.empmiddlename, e.emplastname) + '(' + e.EmpCode + ')' AS EmployeeName,
        CASE
            WHEN calc.EffectiveEnd < calc.EffectiveStart THEN 0
            ELSE DATEDIFF(DAY, calc.EffectiveStart, calc.EffectiveEnd) + 1
        END AS PaidUnits
    FROM empbasic e
    CROSS APPLY
    (
        SELECT
            CASE
                WHEN CAST(e.joining AS DATE) > @RangeStart THEN CAST(e.joining AS DATE)
                ELSE @RangeStart
            END AS EffectiveStart,
            CASE
                WHEN e.closeingdate IS NOT NULL AND CAST(e.closeingdate AS DATE) < @RangeEnd THEN CAST(e.closeingdate AS DATE)
                ELSE @RangeEnd
            END AS EffectiveEnd
    ) calc
    WHERE e.TenantId = @TenantId
      AND e.empid IN
      (
          SELECT TRY_CAST(value AS INT)
          FROM STRING_SPLIT(@EmployeeIds, ',')
          WHERE TRY_CAST(value AS INT) IS NOT NULL
      )
      AND CAST(e.joining AS DATE) <= @RangeEnd
      AND (e.closeingdate IS NULL OR CAST(e.closeingdate AS DATE) >= @RangeStart)
    ORDER BY e.empfirstname;
END
