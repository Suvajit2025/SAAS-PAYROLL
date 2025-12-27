using Common.Utility;
using DocumentFormat.OpenXml.Bibliography;
using MendinePayroll.UI.Common;
using MendinePayroll.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace MendinePayroll.UI.Controllers
{
    public class bulkSalaryProcessController : Controller
    {
        // GET: bulkSalaryProcess

        private string TenantId
        {
            get
            {
                try
                {
                    if (Session["TenantID"] == null)
                    {
                        // Redirect to login or handle gracefully
                        throw new Exception("Session expired. Please log in again.");
                    }
                    return Session["TenantID"].ToString();
                }
                catch (Exception ex)
                {
                    // Log the error (you can use your existing logging mechanism)
                    System.Diagnostics.Debug.WriteLine("TenantID retrieval error: " + ex.Message);
                    throw; // Let it bubble to global error handler if configured
                }
            }
        }

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetCompanies()
        {
            DataTable dt = clsDatabase.fnDataTable("Payroll_SaaS_Company", TenantId);

            var data = dt.AsEnumerable().Select(x => new
            {
                value = x["CompanyID"]?.ToString(),
                text = x["CompanyName"]?.ToString(),
                code = x["CompnayCode"]?.ToString()
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetPayGroups(int payrollType)
        {
            DataTable dt = clsDatabase.fnDataTable("Payroll_SaaS_PayGroupTypewise", payrollType,TenantId);

            var data = dt.AsEnumerable().Select(x => new
            {
                value = x["PayGroupID"]?.ToString(),
                text = x["PayGroupName"]?.ToString(),
                desc= x["Description"]?.ToString()
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEmployees(string payGroup,string payrollType,int? processMonth,int? processYear,DateTime? fromDate,DateTime? toDate,string companyCode)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_Payroll_GetEmployeesForBatch",TenantId,
                payGroup,payrollType == "1" ? "MONTHLY" : "CONTRACTUAL",processMonth,processYear,fromDate,toDate, companyCode, 1);

            var data = dt.AsEnumerable().Select(x => new {
                employeeId = x["EmployeeID"].ToString(),
                code = x["EmpCode"].ToString(),
                name = x["EmployeeName"].ToString(),
                status = x["EmpIsActive"].ToString() == "ACTIVE" ? "Active" : "Inactive",
                isPaid = Convert.ToInt32(x["IsAlreadyPaid"]) == 1,
                isEligible = Convert.ToInt32(x["IsEligible"]) == 1
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GenerateRefNo(string companyCode,string payGroupId,string payrollType,int? processMonth,int? processYear,DateTime? fromDate,DateTime? toDate)
        {
            if (string.IsNullOrEmpty(companyCode))
                return Json(new { refNo = "" }, JsonRequestBehavior.AllowGet);

            string payrollTypeText =
                payrollType == "1" ? "MONTHLY" : "CONTRACTUAL";

            DataTable dt = clsDatabase.fnDataTable("SP_Payroll_GenerateBatchRefNo",TenantId,companyCode,payGroupId,payrollTypeText,
                processMonth,processYear,fromDate,toDate);

            string refNo = dt.Rows.Count > 0
                ? dt.Rows[0]["RefNo"].ToString()
                : "";

            return Json(new { refNo }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAttendanceComponents(int payGroupId, string payrollType)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_Payroll_GetAttendanceComponents", TenantId,
                payGroupId,
                payrollType == "1" ? "MONTHLY" : "CONTRACTUAL"
            );

            var data = dt.AsEnumerable().Select(x => new
            {
                id = x["ConfigureSalaryComponentID"],
                name = x["PayConfigName"].ToString(),
                timeBasis = x["TimeBasis"].ToString()
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAttendanceEmployees(string payGroupId,int payrollType,string employeeIds,int? processMonth,int? processYear,DateTime? fromDate,DateTime? toDate)
        {
            try
            {
                if (string.IsNullOrEmpty(employeeIds))
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }

                var dt = new DataTable();
                dt = clsDatabase.fnDataTable("SP_Payroll_GetAttendanceEmployees", TenantId, payGroupId, payrollType,employeeIds,processMonth,processYear,fromDate,toDate);

                // 🔹 Convert DataTable to generic JSON
                var rows = new List<Dictionary<string, object>>();

                foreach (DataRow dr in dt.Rows)
                {
                    var row = new Dictionary<string, object>();

                    foreach (DataColumn col in dt.Columns)
                    {
                        row[col.ColumnName] = dr[col] == DBNull.Value ? null : dr[col];
                    }

                    rows.Add(row);
                }

                return Json(new
                {
                    columns = dt.Columns.Cast<DataColumn>()
                                         .Select(c => c.ColumnName)
                                         .ToList(),
                    data = rows
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        /*========== Salary Calculation ================*/
        [HttpPost]
        public JsonResult CalculateSalary(PayrollProcessRequest request)
        {
            // 🔹 1. Validate request
            if (request == null || request.Employees == null || !request.Employees.Any())
            {
                return Json(new
                {
                    Success = false,
                    Message = "Invalid payroll request. Employee list is empty."
                }, JsonRequestBehavior.AllowGet);
            }

            int totalDays = 0;

            if (request.PayrollType == "1") // Monthly
            {
                if (!request.ProcessMonth.HasValue || !request.ProcessYear.HasValue)
                    throw new Exception("Process Month & Year required for Monthly payroll");

                totalDays = DateTime.DaysInMonth(
                    request.ProcessYear.Value,
                    request.ProcessMonth.Value
                );
            }
            else // Contractual / Period
            {
                if (!request.FromDate.HasValue || !request.ToDate.HasValue)
                    throw new Exception("FromDate & ToDate required for Contractual payroll");

                totalDays = (request.ToDate.Value - request.FromDate.Value).Days + 1;
            }


            var results = new List<EmployeeSalaryResponse>();

            foreach (var emp in request.Employees)
            {
                // 🔹 2. Fetch Salary Header
                int EmployeeID = emp.EmployeeId;
                DataTable dtHeader = clsDatabase.fnDataTable("Payroll_SaaS_GetSalaryHeader",TenantId, EmployeeID);

                if (dtHeader == null || dtHeader.Rows.Count == 0)
                    continue;

                DataRow headerRow = dtHeader.Rows[0];

                int empSalaryConfigId = Convert.ToInt32(headerRow["EmpSalaryConfigID"]); 
                bool isPFApplicable = Convert.ToBoolean(headerRow["IsPF_Applicable"]);
                bool isESICApplicable = Convert.ToBoolean(headerRow["IsESIC_Applicable"]);
                bool isPTaxApplicable = Convert.ToBoolean(headerRow["IsPTax_Applicable"]);

                // 🔹 3. Fetch Salary Components
                DataTable dtComponents = clsDatabase.fnDataTable("Payroll_SaaS_GetSalaryDetails",empSalaryConfigId);

                var components = new List<SalaryComponentResult>();

                if (dtComponents != null && dtComponents.Rows.Count > 0)
                {
                    foreach (DataRow row in dtComponents.Rows)
                    {
                        components.Add(new SalaryComponentResult
                        {
                            /* =========================
                               Identification
                            ========================= */
                            PayConfigId = Convert.ToInt32(row["PayConfigId"]),
                            PayConfigName = row["PayConfigName"]?.ToString(),

                            /* =========================
                               Classification
                            ========================= */
                            PayType = row["PayType"]?.ToString(),            // Allowances / Deduction / CC / MANUAL
                            LogicType = row["LogicType"]?.ToString(),        // FIXED / FORMULA / MANUAL_INPUT

                            /* =========================
                               Calculation
                            ========================= */
                            Amount = row["MonthlyAmount"] != DBNull.Value
                                        ? Convert.ToDecimal(row["MonthlyAmount"])
                                        : 0,

                            BaseAmount = 0, // 🔑 Will be dynamically set during formula/statutory calculation

                            Rate = row["ManualRate"] != DBNull.Value
                                        ? Convert.ToDecimal(row["ManualRate"])
                                        : 0,

                            /* =========================
                               Flags
                            ========================= */
                            IsPercentage = row["ISPercentage"] != DBNull.Value
                                        && Convert.ToBoolean(row["ISPercentage"]),

                            IsStatutory = row["IsStatutory"] != DBNull.Value
                                        && Convert.ToBoolean(row["IsStatutory"]),

                            IsBasicComponent = row.Table.Columns.Contains("IsBasicComponent")
                                        && row["IsBasicComponent"] != DBNull.Value
                                        && Convert.ToBoolean(row["IsBasicComponent"]),

                            IsGrossComponent = row.Table.Columns.Contains("IsGrossComponent")
                                        && row["IsGrossComponent"] != DBNull.Value
                                        && Convert.ToBoolean(row["IsGrossComponent"]),

                            StatutoryType = row["StatutoryType"]?.ToString(),
                            PTaxSlabsJson = row["PTaxSlabsJson"]?.ToString(),

                            IsAllowance = Convert.ToBoolean(row["IsAllowance"]),
                            AllowanceType = row["AllowanceType"]?.ToString(), 

                            IsOther = Convert.ToBoolean(row["IsOther"]),
                            OtherType = row["OtherType"]?.ToString(),

                            /* =========================
                               Attendance Context
                            ========================= */
                            PaidDays = emp.PaidDays,
                            TotalDays = totalDays,

                            /* =========================
                               Limits & Rounding
                            ========================= */
                            MaxLimit = row["MaxLimit"] != DBNull.Value
                                        ? Convert.ToDecimal(row["MaxLimit"])
                                        : (decimal?)null,

                            RoundingType = row["RoundingType"]?.ToString(),

                            /* =========================
                               Audit / Debug
                            ========================= */
                            CalculationExpression = row["CalculationFormula"]?.ToString()
                        });
                    }
                }

                // 🔹 4. Calculate Salary (FINAL & CORRECT)
                EmployeeSalaryResponse salaryResult = null;

                if (request.PayrollType == "1") // Monthly
                {
                    salaryResult = CalculateMonthlySalary(
                        components,
                        emp.EmployeeId,
                        request.ProcessMonth ?? DateTime.Now.Month,
                        request.ProcessYear ?? DateTime.Now.Year,
                        isPFApplicable,
                        isESICApplicable,
                        isPTaxApplicable,
                        PayrollValueResolver      // 🔑 external resolver for Loan / TDS / Excel
                    );
                }

                // 🔹 Safety check
                if (salaryResult == null)
                    continue;

                // 🔹 5. Build response
                results.Add(new EmployeeSalaryResponse
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeName= emp.EmployeeName,
                    PaidDays = emp.PaidDays,
                    NetPay = salaryResult.NetPay,
                    TotalEarnings = salaryResult.TotalEarnings,
                    TotalDeductions = salaryResult.TotalDeductions,
                    EmployerContribution=salaryResult.EmployerContribution,
                    MonthlyCTC=salaryResult.MonthlyCTC,
                    Components = salaryResult.Components
                });


            }

            // 🔹 6. Return JSON (MVC style)
            var pivot = BuildPivot(request, results);

            return Json(pivot, JsonRequestBehavior.AllowGet);

        }
        public static EmployeeSalaryResponse CalculateMonthlySalary(List<SalaryComponentResult> components,int employeeId,int processMonth,int processYear,
                                                                     bool isPFApplicable,bool isESICApplicable,bool isPTaxApplicable,ComponentValueResolver valueResolver)
        {
            if (components == null || components.Count == 0)
                return null;

            decimal totalDays = components.First().TotalDays > 0
                ? components.First().TotalDays
                : DateTime.DaysInMonth(processYear, processMonth);

            decimal totalEarnings = 0;
            decimal totalDeductions = 0;

            // Used ONLY for FORMULA evaluation
            Dictionary<string, decimal> valueMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            /* =========================
               PASS 1 : FIXED / MANUAL / EXCEL
            ========================= */
            foreach (var c in components.Where(x =>
                   x.LogicType == "FIXED"
                || x.LogicType == "MANUAL_INPUT"
                || x.LogicType == "EXCEL_UPLOAD"
                || (x.IsOther && x.OtherType == "LOANRECOVERY")
                || (x.IsStatutory && x.StatutoryType == "TDS")
            ))
            {
                // 🔹 Resolver-based components (ABSOLUTE – NO PRORATE)
                if (
                    c.LogicType == "EXCEL_UPLOAD"
                    || (c.IsOther && c.OtherType == "LOANRECOVERY")
                    || (c.IsStatutory && c.StatutoryType == "TDS")
                )
                {
                    c.BaseAmount = valueResolver(
                        employeeId,
                        c.PayConfigId,
                        processMonth,
                        processYear,
                        c
                    );

                    c.Amount = ApplyRounding(c.BaseAmount,"");   // 🔒 NO PRORATE
                    valueMap[c.PayConfigName] = c.Amount;
                    continue;
                }

                // 🔹 Attendance-based components
                c.BaseAmount = c.Amount;
                c.Amount = ApplyRounding(Prorate(c.BaseAmount, c.PaidDays, totalDays),c.RoundingType);
                valueMap[c.PayConfigName] = c.Amount;
            }


            /* =========================
               PASS 2 : FORMULA
            ========================= */
            foreach (var c in components.Where(x => x.LogicType == "FORMULA"))
            {
                decimal baseValue = ApplyRounding(EvaluateDynamicFormula(
                    c.CalculationExpression,
                    valueMap
                ),c.RoundingType);

                c.BaseAmount = baseValue;

                decimal calculated = c.IsPercentage
                    ? baseValue * c.Rate / 100
                    : baseValue;

                if (c.MaxLimit.HasValue && calculated > c.MaxLimit.Value)
                    calculated = c.MaxLimit.Value;

                c.Amount = ApplyRounding(Prorate(calculated, c.PaidDays, totalDays),c.RoundingType);
                valueMap[c.PayConfigName] = c.Amount;
            }

            /* =========================
               PASS 3 : STATUTORY (MONTHLY RULES)
            ========================= */
            decimal basicAmount = components
                .Where(x => x.IsBasicComponent)
                .Sum(x => x.Amount);

            decimal grossAmount = components
                .Where(x => x.IsGrossComponent)
                .Sum(x => x.Amount);

            decimal actualMonthlyGross = components
                .Where(x => x.IsGrossComponent)
                .Select(x => x.BaseAmount)
                .FirstOrDefault();

            foreach (var c in components.Where(x => x.IsStatutory))
            {
                // 🔹 PF
                if (c.StatutoryType.StartsWith("PF"))
                {
                    if (!isPFApplicable)
                    {
                        c.Amount = 0;
                        continue;
                    }

                    decimal pfBase = basicAmount;
                    if (c.MaxLimit.HasValue && pfBase > c.MaxLimit.Value)
                        pfBase = c.MaxLimit.Value;

                    c.BaseAmount = pfBase;
                    c.Amount = ApplyRounding(Prorate(pfBase * c.Rate / 100, c.PaidDays, totalDays),c.RoundingType);
                }

                // 🔹 ESIC (MONTHLY GROSS CHECK – NOT PRORATED)
                else if (c.StatutoryType.StartsWith("ESIC"))
                {
                    if (!isESICApplicable ||
                        (c.MaxLimit.HasValue && actualMonthlyGross > c.MaxLimit.Value))
                    {
                        c.Amount = 0;
                        continue;
                    }

                    c.BaseAmount = grossAmount;
                    c.Amount = ApplyRounding(Prorate(grossAmount * c.Rate / 100, c.PaidDays, totalDays), c.RoundingType);
                }

                // 🔹 PTAX
                else if (c.StatutoryType == "PTAX")
                {
                    if (!isPTaxApplicable)
                    {
                        c.Amount = 0;
                        continue;
                    }

                    decimal slabAmount =
                        CalculatePTaxFromJson(grossAmount, c.PTaxSlabsJson);

                    c.BaseAmount = actualMonthlyGross;
                    c.Amount = ApplyRounding(slabAmount,c.RoundingType);
                }
            }

            /* =========================
               TOTALS
            ========================= */ 
            decimal totalEmployerCC = 0;        // Employer contributions (CTC part, NOT net)

            foreach (var c in components)
            {
                var pt = (c.PayType ?? "").Trim().ToUpperInvariant();

                if (pt == "ALLOWANCES")
                {
                    totalEarnings += c.Amount;
                }
                else if (pt == "DEDUCTION")
                {
                    totalDeductions += c.Amount;
                }
                else if (pt == "CC")
                {
                    totalEmployerCC += c.Amount;   // employer cost only
                }
            }

            var netPay = totalEarnings - totalDeductions;          // ✅ CC excluded
            var monthlyCTC = totalEarnings + totalEmployerCC;      // ✅ optional for pivot use


            return new EmployeeSalaryResponse
            {
                TotalEarnings = Math.Round(totalEarnings, 2),
                TotalDeductions = Math.Round(totalDeductions, 2),
                NetPay = Math.Round(netPay, 2),
                EmployerContribution = Math.Round(totalEmployerCC, 2), // ✅ add this property (recommended)
                MonthlyCTC = Math.Round(monthlyCTC, 2),                // ✅ add this property (recommended)
                Components = components
            };

        }

        private static decimal EvaluateDynamicFormula(string formula,Dictionary<string, decimal> valueMap)
        {
            if (string.IsNullOrWhiteSpace(formula))
                return 0;

            var tokens = Regex
                .Split(formula, @"([\+\-\*\/\%\(\)])")
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t));

            foreach (var token in tokens)
            {
                if (decimal.TryParse(token, out _))
                    continue;

                var match = valueMap.FirstOrDefault(x =>
                        x.Key.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0); 

                if (!string.IsNullOrEmpty(match.Key))
                {
                    formula = Regex.Replace(
                        formula,
                        Regex.Escape(token),
                        match.Value.ToString(CultureInfo.InvariantCulture),
                        RegexOptions.IgnoreCase
                    );
                }
            }

            return Convert.ToDecimal(new DataTable().Compute(formula, ""));
        }
        private static decimal Prorate(decimal amount, decimal paidDays, decimal totalDays)
        {
            if (totalDays <= 0 || paidDays <= 0) return 0;
            return Math.Round((amount / totalDays) * paidDays, 2);
        }
        private static decimal CalculatePTaxFromJson(decimal gross, string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return 0;

            var slabs = JsonConvert.DeserializeObject<List<PTaxSlab>>(json);

            foreach (var s in slabs)
            {
                if (gross >= s.MinSalary && gross <= s.MaxSalary)
                    return s.PTAXAmount;
            }

            return 0;
        }
        private decimal PayrollValueResolver(int employeeId,int payConfigId,int month,int year,SalaryComponentResult component)
        {
            // LOAN
            if (component.IsOther && component.OtherType == "LOANRECOVERY")
                return GetEmployeeLoanEMI(employeeId, month, year);

            // TDS
            if (component.IsStatutory && component.StatutoryType == "TDS")
                return GetEmployeeTDS(employeeId, month, year);

            // EXCEL UPLOAD
            if (component.LogicType == "EXCEL_UPLOAD")
                return GetExcelUploadedAmount(employeeId, payConfigId, month, year);

            return component.Amount;
        }

        private decimal GetEmployeeLoanEMI(int employeeId, int month,int Year)
        {

            // 🔒 Check if already paid via salary
            decimal amountPaid = IsLoanAlreadyPaid(employeeId, month, Year);

            // ✅ If already paid, RETURN that amount
            if (amountPaid > 0)
                return amountPaid;

            // 🔁 Else calculate pending EMI
            string EmployeeIds = employeeId.ToString();
            // ✅ Convert month number to month name
            string Month = new DateTime(Year, month, 1)
                                .ToString("MMMM", CultureInfo.InvariantCulture);

            DataTable dt = clsDatabase.fnDataTable("PRC_Bulk_EmployeeLoan_PendingList", EmployeeIds, Month, Year, TenantId);

            if (dt == null || dt.Rows.Count == 0)
                return 0m;

            if (dt.Columns.Contains("AmountToBePaid") && dt.Rows[0]["AmountToBePaid"] != DBNull.Value)
                return Convert.ToDecimal(dt.Rows[0]["AmountToBePaid"]);

            return 0m;
        }
        private decimal GetEmployeeTDS(int employeeId,int month,int year)
        {
            // TODO:
            // Call SP like: Payroll_GetEmployeeTDS
            // Params: employeeId, month, year
            // Return calculated TDS for the month

            return 0m;
        }

        private decimal GetExcelUploadedAmount(int employeeId,int payConfigId,int month,int year)
        {
            // TODO:
            // Call SP like: Payroll_GetExcelUploadedComponentAmount
            // Params: employeeId, payConfigId, month, year
            // Used for LogicType = EXCEL_UPLOAD

            return 0m;
        }
         
        private bool IsWaivableComponent(SalaryComponentResult c)
        {
            if (c == null)
                return false;

            var payType = (c.PayType ?? "").Trim().ToUpperInvariant();
            var otherType = (c.OtherType ?? "").Trim().ToUpperInvariant();
            var statutoryType = (c.StatutoryType ?? "").Trim().ToUpperInvariant();

            // ❌ Employer contribution is NEVER waivable
            if (payType == "CC")
                return false;

            // ❌ Statutory deductions are NEVER waivable
            if (c.IsStatutory)
                return false;

            // ✅ Loan / Advance deductions are waivable
            if (payType == "DEDUCTION" &&
                c.IsOther &&
                (otherType == "LOANRECOVERY" || otherType == "ADVANCE"))
                return true;

            return false;
        }
        private PayrollPivotResponse BuildPivot( PayrollProcessRequest request, List<EmployeeSalaryResponse> results)
        {
            /* =========================
               1. Collect all components
            ========================= */
            var allComponents = results
                .Where(r => r.Components != null)
                .SelectMany(r => r.Components)
                .ToList();

            /* =========================
               2. Build Pivot Columns
               (EMPLOYEE-AGNOSTIC)
            ========================= */
            var columns = allComponents
                .GroupBy(c => c.PayConfigId)
                .Select(g =>
                {
                    var c = g.First();

                    bool isLoan =
                        (c.PayType ?? "").Equals("DEDUCTION", StringComparison.OrdinalIgnoreCase)
                        && c.IsOther
                        && (c.OtherType ?? "").Equals("LOANRECOVERY", StringComparison.OrdinalIgnoreCase);

                    return new PivotColumnDto
                    {
                        PayConfigId = c.PayConfigId,
                        PayConfigName = c.PayConfigName,
                        PayType = c.PayType,
                        LogicType = c.LogicType,

                        IsAllowance = c.IsAllowance,
                        AllowanceType = c.AllowanceType,

                        IsStatutory = c.IsStatutory,
                        StatutoryType = c.StatutoryType,

                        IsOther = c.IsOther,
                        OtherType = c.OtherType,

                        // 🔥 CRITICAL: propagate Gross metadata to UI
                        IsGrossComponent = c.IsGrossComponent,
                        IsBasicComponent=c.IsBasicComponent,

                        // 🔒 Only loan is conceptually waivable/editable
                        IsWaivable = isLoan,
                        IsEditable = isLoan,

                        DisplayOrder = GetDisplayOrder(c)
                    };
                })
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.PayConfigName)
                .ToList();

            /* =========================
               3. Build Pivot Rows
               (EMPLOYEE-AWARE)
            ========================= */
            var rows = new List<PivotRowDto>();

            foreach (var emp in results)
            {
                var row = new PivotRowDto
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeName = emp.EmployeeName,
                    PaidDays = emp.PaidDays,

                    TotalEarnings = emp.TotalEarnings,
                    TotalDeductions = emp.TotalDeductions,
                    NetPay = emp.NetPay,
                    MonthlyCTC = emp.MonthlyCTC,

                    Cells = new List<PivotCellDto>()
                };

                foreach (var col in columns)
                {
                    var comp = emp.Components
                        ?.FirstOrDefault(x => x.PayConfigId == col.PayConfigId);

                    bool isLoanCell =
                        col.IsWaivable
                        && (col.OtherType ?? "")
                            .Equals("LOANRECOVERY", StringComparison.OrdinalIgnoreCase);

                    decimal amount = comp?.Amount ?? 0;

                    // 🔒 Check if loan already paid in this month
                    
                    decimal loanPaidAmount = isLoanCell
                            ? IsLoanAlreadyPaid(
                                emp.EmployeeId,
                                request.ProcessMonth.GetValueOrDefault(),
                                request.ProcessYear.GetValueOrDefault()
                                )
                            : 0m;

                    bool loanAlreadyPaid = loanPaidAmount > 0;

                    row.Cells.Add(new PivotCellDto
                    {
                        PayConfigId = col.PayConfigId,
                        Amount = amount,
                        BaseAmount = comp?.BaseAmount ?? 0,

                        // 🔒 Disable skip if already paid
                        IsEditable = isLoanCell && !loanAlreadyPaid,
                        IsWaivable = isLoanCell && !loanAlreadyPaid,

                        IsWaived = false,

                        // 🔒 Preserve EMI only for loan
                        OriginalAmount = isLoanCell ? amount : 0
                    });
                }

                rows.Add(row);
            }

            /* =========================
               4. Return Response
            ========================= */
            return new PayrollPivotResponse
            {
                Success = true,

                PayrollType = request.PayrollType,
                ProcessMonth = request.ProcessMonth,
                ProcessYear = request.ProcessYear,
                FromDate = request.FromDate,
                ToDate = request.ToDate,

                Columns = columns,
                Rows = rows
            };
        }

        private int GetDisplayOrder(SalaryComponentResult c)
        {
            var payType = (c.PayType ?? "").Trim().ToUpperInvariant();

            /* 1️⃣ Allowances (earnings except gross) */
            if (payType == "ALLOWANCES" && !c.IsGrossComponent)
                return 10;

            /* 2️⃣ Gross summary component */
            if (c.IsGrossComponent)
                return 20;

            /* 3️⃣ Deductions */
            if (payType == "DEDUCTION")
                return 30;

            /* 4️⃣ Employer Contributions */
            if (payType == "CC")
                return 50;

            /* fallback */
            return 99;
        } 
        private static decimal ApplyRounding(decimal value, string roundingType)
        {
            switch ((roundingType ?? "").ToUpperInvariant())
            {
                case "CEIL":
                    return Math.Ceiling(value);
                case "FLOOR":
                    return Math.Floor(value);
                case "ROUND":
                    return Math.Round(value, 0, MidpointRounding.AwayFromZero);
                default:
                    return Math.Round(value, 0);
            }
        }

        private decimal IsLoanAlreadyPaid(int EmployeeId, int Month, int Year)
        {
            DataTable dt = clsDatabase.fnDataTable( "PRC_CheckLoanPaidForMonth", EmployeeId, Month, Year,TenantId);

            if (dt == null || dt.Rows.Count == 0)
                return 0m;

            if (dt.Columns.Contains("AmountPaid") &&
                dt.Rows[0]["AmountPaid"] != DBNull.Value)
            {
                return Convert.ToDecimal(dt.Rows[0]["AmountPaid"]);
            }

            return 0m;
        }

        [HttpPost]
        public JsonResult SavePayrollBatch(PayrollBatchSaveDto dto)
        {
            try
            {
                // =============================
                // 1. Validate payload
                // =============================
                if (dto == null || dto.Employees == null || dto.Employees.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid payroll payload. No employee data found."
                    });
                }

                var EntryUser = Session["UserName"].ToString();

                // =============================
                // 2. Calculate batch totals
                // =============================
                dto.TotalEmployees = dto.Employees.Count;
                dto.TotalCost = dto.Employees.Sum(e => e.TotalGross);
                dto.TotalNetPay = dto.Employees.Sum(e => e.NetPay);

                // ===================================================================
                // 3. Save Batch Header & Get Batch ID
                // ===================================================================
                DataTable dtBatch = clsDatabase.fnDataTable(
                    "SP_Payroll_SaaS_SaveBatchSalary",
                    dto.RefNo,
                    dto.RefDate, 
                    dto.PayrollType,
                    dto.ProcessMonth,
                    dto.ProcessYear,
                    dto.FromDate,
                    dto.ToDate,
                    dto.TotalEmployees,
                    dto.TotalCost,
                    dto.TotalNetPay,
                    dto.Comments,
                    TenantId,
                    EntryUser
                );

                // ❌ No response
                if (dtBatch == null || dtBatch.Rows.Count == 0)
                {
                    throw new Exception("No response from payroll batch save procedure.");
                }

                // ❌ Missing expected column
                if (!dtBatch.Columns.Contains("PayrollBatchID"))
                {
                    throw new Exception("Invalid response from payroll batch save procedure.");
                }

                DataRow row = dtBatch.Rows[0];

                // ❌ Error returned from SP
                if (dtBatch.Columns.Contains("StatusCode")
                    && Convert.ToInt32(row["StatusCode"]) == 0)
                {
                    string msg = row["Message"]?.ToString() ?? "Payroll batch save failed.";
                    throw new Exception(msg);
                }

                // ✅ Success
                int payrollBatchId = Convert.ToInt32(row["PayrollBatchID"]);

                string employeesJson = JsonConvert.SerializeObject(dto.Employees);
                // ===================================================================
                // 4. Save Employee Header & Get Payroll Process Employee ID
                // ===================================================================
                DataTable dtEmpBatch= clsDatabase.fnDataTable("SP_Payroll_SaaS_SaveBatchEmployee",payrollBatchId,employeesJson,TenantId,EntryUser);

                // ❌ No response
                if (dtEmpBatch == null || dtEmpBatch.Rows.Count == 0)
                {
                    throw new Exception("No response from payroll employee batch save procedure.");
                }

                // ❌ Missing expected column
                if (!dtEmpBatch.Columns.Contains("StatusCode"))
                {
                    throw new Exception("Invalid response from payroll employee batch save procedure.");
                }

                DataRow empRow = dtEmpBatch.Rows[0];

                // ❌ Error returned from SP
                if (Convert.ToInt32(empRow["StatusCode"]) == 0)
                {
                    string msg = empRow["Message"]?.ToString()
                                 ?? "Payroll employee save failed.";
                    throw new Exception(msg);
                }

                // ===================================================================
                // 5. Save Employee Salary Component Details
                // ===================================================================

                DataTable dtEmpDetail = clsDatabase.fnDataTable(
                    "SP_Payroll_SaaS_SaveBatchEmployeeDetail",
                    payrollBatchId,
                    employeesJson
                );

                if (dtEmpDetail == null || dtEmpDetail.Rows.Count == 0)
                    throw new Exception("No response from payroll employee detail save procedure.");

                if (!dtEmpDetail.Columns.Contains("PayrollProcessEmployeeID"))
                    throw new Exception("Invalid response from payroll employee detail save procedure.");

                // =============================
                // 6. Success response
                // =============================
                return Json(new
                {
                    success = true,
                    message = "Payroll batch saved successfully.",
                    payrollBatchId = payrollBatchId
                });
            }
            catch (SqlException ex)
            {
                // 🔴 SQL-related issues
                return Json(new
                {
                    success = false,
                    message = "Database error while saving payroll batch.",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                // 🔴 Any other runtime error
                return Json(new
                {
                    success = false,
                    message = "Unexpected error while saving payroll batch.",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public JsonResult GetPayrollDashboard()
        {
            var dt = clsDatabase.fnDataTable("SP_Payroll_SaaS__GetBatchSalary",TenantId);

            var list = dt.AsEnumerable().Select(r => new
            {
                PayrollBatchID = Convert.ToInt32(r["PayrollBatchID"]),

                PayGroupID = Convert.ToInt32(r["PayGroupID"]),

                RefNo = r["RefNo"]?.ToString(),

                PeriodText = r["PeriodText"]?.ToString(),

                PayGroupName = r["PayGroupName"]?.ToString(),

                PayrollTypeText = r["PayrollTypeText"]?.ToString(),

                TotalEmployees = Convert.ToInt32(r["TotalEmployees"]),

                TotalCost = Convert.ToDecimal(r["TotalCost"]),

                NetPayout = Convert.ToDecimal(r["NetPayout"]),

                // 🔥 STATUS IS INT → convert properly
                Status = Convert.ToInt32(r["Status"]),

                CreatedDate = Convert.ToDateTime(r["CreatedDate"])
            }).ToList();


            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetPayrollEmployeeWiseDashboard()
        {
            try
            {
                DataTable dt = clsDatabase.fnDataTable("SP_Payroll_SaaS__GetEmployeeSalary",TenantId);

                var result = dt.AsEnumerable().Select(r => new
                {
                    PayrollProcessEmployeeID = Convert.ToInt32(r["PayrollProcessEmployeeID"]),
                    EmployeeId = Convert.ToInt32(r["EmployeeID"]),
                    EmployeeCode = r["EmployeeCode"]?.ToString(),
                    EmployeeName = r["EmployeeName"]?.ToString(),
                    PayGroupName = r["PayGroupName"]?.ToString(),
                    PeriodText = r["PeriodText"]?.ToString(),

                    GrossPay = Convert.ToDecimal(r["GrossPay"]),
                    Deduction = Convert.ToDecimal(r["Deduction"]),
                    NetPay = Convert.ToDecimal(r["NetPay"]),

                    StatusText = r["StatusText"]?.ToString(),
                    StatusClass = r["StatusClass"]?.ToString()
                }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new
                {
                    success = false,
                    message = "Failed to load employee payroll dashboard",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult OpenBatchView(int payrollBatchId, int payGroupId)
        {
            try
            {
                // ===============================
                // 1. Load saved batch employee data
                // ===============================
                DataTable dt = clsDatabase.fnDataTable("SP_Payroll_SaaS_GetSalaryBatch",payrollBatchId,TenantId);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new { Success = false, Message = "No payroll batch data found." },
                                JsonRequestBehavior.AllowGet);
                }

                // ===============================
                // 2. Convert to EmployeeSalaryResponse
                // ===============================
                var employees = new List<EmployeeSalaryResponse>();

                foreach (DataRow r in dt.Rows)
                {
                    //employees.Add(new EmployeeSalaryResponse
                    //{
                    //    EmployeeId = Convert.ToInt32(r["EmployeeID"] ?? 0),
                    //    EmployeeName = Convert.ToString(r["EmployeeName"]) ?? string.Empty,

                    //    PaidDays = Convert.ToDecimal(r["TotalPaidDays"] ?? 0m),

                    //    TotalEarnings = Convert.ToDecimal(r["TotalGross"] ?? 0m),
                    //    TotalDeductions = Convert.ToDecimal(r["TotalDeduction"] ?? 0m),
                    //    NetPay = Convert.ToDecimal(r["TotalTakeHome"] ?? 0m),
                    //    MonthlyCTC = Convert.ToDecimal(r["MonthlyCTC"] ?? 0m),

                    //    Components = string.IsNullOrWhiteSpace(Convert.ToString(r["ComponentJson"]))
                    //        ? new List<SalaryComponentResult>()
                    //        : JsonConvert.DeserializeObject<List<SalaryComponentResult>>(
                    //              Convert.ToString(r["ComponentJson"])
                    //          )
                    //});
                    employees.Add(new EmployeeSalaryResponse
                    {
                        EmployeeId = GetInt(r, "EmployeeID"),
                        EmployeeName = GetString(r, "EmployeeName"),

                        PaidDays = GetDecimal(r, "TotalPaidDays"),

                        TotalEarnings = GetDecimal(r, "TotalGross"),
                        TotalDeductions = GetDecimal(r, "TotalDeduction"),
                        NetPay = GetDecimal(r, "TotalTakeHome"),
                        MonthlyCTC = GetDecimal(r, "MonthlyCTC"),

                        Components = r["ComponentJson"] == DBNull.Value
                            ? new List<SalaryComponentResult>()
                            : JsonConvert.DeserializeObject<List<SalaryComponentResult>>(
                                  r["ComponentJson"].ToString()
                              )
                    });

                }


                // ===============================
                // 3. Build PayrollProcessRequest (EDIT MODE)
                // ===============================
                DataRow h = dt.Rows[0];

                var request = new PayrollProcessRequest
                {
                    PayrollType = Convert.ToString(h["PayrollType"]), // "1" or "2"

                    ProcessMonth = h["ProcessMonth"] == DBNull.Value
                        ? (int?)null
                        : Convert.ToInt32(h["ProcessMonth"]),

                    ProcessYear = h["ProcessYear"] == DBNull.Value
                        ? (int?)null
                        : Convert.ToInt32(h["ProcessYear"]),

                    FromDate = h["FromDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(h["FromDate"]),

                    ToDate = h["ToDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(h["ToDate"])
                };

                // ===============================
                // 4. Build Pivot (EDIT MODE)
                // ===============================
                var pivot = BuildPivot(request, employees);

                return Json(pivot, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Failed to open payroll batch.",
                    Error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        static decimal GetDecimal(DataRow r, string col)
    => r[col] == DBNull.Value ? 0m : Convert.ToDecimal(r[col]);

        static int GetInt(DataRow r, string col)
            => r[col] == DBNull.Value ? 0 : Convert.ToInt32(r[col]);

        static string GetString(DataRow r, string col)
            => r[col] == DBNull.Value ? "" : Convert.ToString(r[col]);

    }
}