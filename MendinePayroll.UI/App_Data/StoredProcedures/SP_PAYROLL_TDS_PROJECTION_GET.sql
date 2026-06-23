IF OBJECT_ID('dbo.SP_PAYROLL_TDS_PROJECTION_GET', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_PAYROLL_TDS_PROJECTION_GET;
GO

CREATE PROCEDURE dbo.SP_PAYROLL_TDS_PROJECTION_GET
(
    @FYId INT,
    @CompanyIds VARCHAR(MAX),
    @DeptIds VARCHAR(MAX),
    @EmpIds VARCHAR(MAX),
    @isRebateApplied INT,
    @TenantId VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TblComp TABLE (Pk INT);
    DECLARE @TblDept TABLE (ID INT);
    DECLARE @TblEmp  TABLE (ID INT);

    IF ISNULL(@CompanyIds, '') <> ''
        INSERT INTO @TblComp
        SELECT TRY_CAST(value AS INT)
        FROM STRING_SPLIT(@CompanyIds, ',')
        WHERE TRY_CAST(value AS INT) IS NOT NULL;

    IF ISNULL(@DeptIds, '') <> ''
        INSERT INTO @TblDept
        SELECT TRY_CAST(value AS INT)
        FROM STRING_SPLIT(@DeptIds, ',')
        WHERE TRY_CAST(value AS INT) IS NOT NULL;

    IF ISNULL(@EmpIds, '') <> ''
        INSERT INTO @TblEmp
        SELECT TRY_CAST(value AS INT)
        FROM STRING_SPLIT(@EmpIds, ',')
        WHERE TRY_CAST(value AS INT) IS NOT NULL;

    DECLARE
        @StdDeduction DECIMAL(18, 2),
        @RebateThreshold DECIMAL(18, 2),
        @CessRate DECIMAL(18, 2),
        @FYStartDate DATE,
        @FYEndDate DATE,
        @FYDays DECIMAL(18, 6);

    SELECT
        @StdDeduction = TC.StandardDeduction,
        @RebateThreshold = TC.RebateIncomeThreshold,
        @CessRate = TC.HealthEduCessRate,
        @FYStartDate = CAST(FY.StartDate AS DATE),
        @FYEndDate = CAST(FY.EndDate AS DATE)
    FROM dbo.Payroll_FinancialYear FY
    INNER JOIN dbo.Payroll_SaaS_TaxConstant TC
        ON FY.FinancialYearCode = TC.FinancialYear
    WHERE FY.IDFinancialYear = @FYId
      AND TC.RegimeId = 2
      AND TC.IsActive = 1;

    SET @FYDays = DATEDIFF(DAY, @FYStartDate, @FYEndDate) + 1;

    IF @FYStartDate IS NULL OR @FYEndDate IS NULL OR ISNULL(@FYDays, 0) <= 0
    BEGIN
        SELECT
            CAST(NULL AS INT) AS EmployeeID,
            CAST(NULL AS VARCHAR(250)) AS EmployeeName,
            CAST(0 AS DECIMAL(18, 2)) AS AnnualGross,
            CAST(0 AS DECIMAL(18, 2)) AS NetTaxable,
            CAST(0 AS DECIMAL(18, 2)) AS TotalTaxBeforeCess,
            CAST(0 AS DECIMAL(18, 2)) AS CessAmount,
            CAST(0 AS DECIMAL(18, 2)) AS AnnualTaxLiability,
            CAST(0 AS DECIMAL(18, 2)) AS MonthlyTDS
        WHERE 1 = 0;

        RETURN;
    END;

    ;WITH RawIncome AS
    (
        SELECT
            E.empid AS EmployeeID,
            dbo.Employee_FullName(E.empfirstname, E.empmiddlename, E.emplastname) + ' (' + CAST(E.empno AS VARCHAR(20)) + ')' AS EmployeeName,
            ServicePeriod.RemainingMonths,
            CAST(
                ISNULL(D.YearlyAmount, 0)
                * CAST(ServicePeriod.ServiceDays AS DECIMAL(18, 6))
                / NULLIF(@FYDays, 0)
                AS DECIMAL(18, 2)
            ) AS ProjectedYearlyAmount
        FROM dbo.Empbasic E
        INNER JOIN dbo.Payroll_EmployeeSalaryConfigaration C
            ON E.empid = C.EmployeeID
           AND C.ActiveFlag = 1
        INNER JOIN dbo.Payroll_detail_EmployeeSalaryConfigaration D
            ON C.EmpSalaryConfigID = D.EmpSalaryConfigID
        INNER JOIN dbo.Company Com
            ON Com.CompnayCode = E.companycode
        CROSS APPLY
        (
            SELECT TRY_CONVERT(DATE, E.joining) AS JoiningDate
        ) JoiningInfo
        CROSS APPLY
        (
            SELECT
                CASE
                    WHEN JoiningInfo.JoiningDate IS NULL THEN @FYStartDate
                    WHEN JoiningInfo.JoiningDate < @FYStartDate THEN @FYStartDate
                    ELSE JoiningInfo.JoiningDate
                END AS ProjectionStartDate
        ) ProjectionPeriod
        CROSS APPLY
        (
            SELECT
                CASE
                    WHEN ProjectionPeriod.ProjectionStartDate > @FYEndDate THEN 0
                    ELSE DATEDIFF(DAY, ProjectionPeriod.ProjectionStartDate, @FYEndDate) + 1
                END AS ServiceDays,
                CASE
                    WHEN ProjectionPeriod.ProjectionStartDate > @FYEndDate THEN 0
                    ELSE DATEDIFF(MONTH, ProjectionPeriod.ProjectionStartDate, @FYEndDate) + 1
                END AS RemainingMonths
        ) ServicePeriod
        WHERE E.TenantId = @TenantId
          AND ServicePeriod.ServiceDays > 0
          AND (ISNULL(@CompanyIds, '') = '' OR Com.companyPk IN (SELECT Pk FROM @TblComp))
          AND (ISNULL(@DeptIds, '') = '' OR E.empdept IN (SELECT ID FROM @TblDept))
          AND (ISNULL(@EmpIds, '') = '' OR E.empid IN (SELECT ID FROM @TblEmp))
          AND (
                (D.PayType = 'Allowances' AND ISNULL(D.IsGrossComponent, 0) <> 1)
                OR D.OtherType = 'BONUS'
              )
    ),
    EmpAnnualIncome AS
    (
        SELECT
            EmployeeID,
            EmployeeName,
            MAX(RemainingMonths) AS RemainingMonths,
            SUM(ProjectedYearlyAmount) AS AnnualGross,
            CASE
                WHEN SUM(ProjectedYearlyAmount) - ISNULL(@StdDeduction, 0) < 0 THEN 0
                ELSE SUM(ProjectedYearlyAmount) - ISNULL(@StdDeduction, 0)
            END AS NetTaxable
        FROM RawIncome
        GROUP BY EmployeeID, EmployeeName
    ),
    SlabTax AS
    (
        SELECT
            A.EmployeeID,
            A.EmployeeName,
            A.AnnualGross,
            A.NetTaxable,
            A.RemainingMonths,
            SUM(
                CASE
                    WHEN A.NetTaxable <= T.FromAmount THEN 0
                    ELSE
                        (
                            CASE
                                WHEN A.NetTaxable >= ISNULL(T.ToAmount, A.NetTaxable)
                                    THEN ISNULL(T.ToAmount, A.NetTaxable) - T.FromAmount
                                ELSE A.NetTaxable - T.FromAmount
                            END
                        ) * (T.TaxRate / 100.0)
                END
            ) AS ComputedSlabTax
        FROM EmpAnnualIncome A
        CROSS JOIN dbo.Payroll_SaaS_TaxSlab T
        INNER JOIN dbo.Payroll_FinancialYear FY
            ON FY.FinancialYearCode = T.FinancialYear
        WHERE FY.IDFinancialYear = @FYId
          AND T.RegimeId = 2
          AND T.IsActive = 1
        GROUP BY
            A.EmployeeID,
            A.EmployeeName,
            A.AnnualGross,
            A.NetTaxable,
            A.RemainingMonths
    ),
    FinalTax AS
    (
        SELECT
            *,
            CASE
                WHEN @isRebateApplied = 1
                 AND NetTaxable <= ISNULL(@RebateThreshold, 0)
                    THEN 0
                ELSE ComputedSlabTax
            END AS FinalBaseTax
        FROM SlabTax
    )
    SELECT
        EmployeeID,
        EmployeeName,
        CAST(AnnualGross AS DECIMAL(18, 2)) AS AnnualGross,
        CAST(NetTaxable AS DECIMAL(18, 2)) AS NetTaxable,
        CAST(FinalBaseTax AS DECIMAL(18, 2)) AS TotalTaxBeforeCess,
        CAST(FinalBaseTax * (ISNULL(@CessRate, 0) / 100.0) AS DECIMAL(18, 2)) AS CessAmount,
        CAST(FinalBaseTax + (FinalBaseTax * (ISNULL(@CessRate, 0) / 100.0)) AS DECIMAL(18, 2)) AS AnnualTaxLiability,
        CAST((FinalBaseTax + (FinalBaseTax * (ISNULL(@CessRate, 0) / 100.0))) / NULLIF(RemainingMonths, 0) AS DECIMAL(18, 2)) AS MonthlyTDS
    FROM FinalTax
    ORDER BY EmployeeName;
END;
GO
