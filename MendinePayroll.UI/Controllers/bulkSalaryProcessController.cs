using Common.Utility;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using MendinePayroll.UI.Common;
using MendinePayroll.UI.Models;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Newtonsoft.Json;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
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
using UI.BLL;
using static Org.BouncyCastle.Math.EC.ECCurve;

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
            if (Session["UserName"] == null)
                return RedirectToAction("Index", "Login");

            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "EMPLOYEE-BULK-SALARY-ENTRY";
            clsAccessLog.AccessLog_Save(info);
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
            string UserId = Session["UserId"].ToString();
            DataTable dt = clsDatabase.fnDataTable("Payroll_SaaS_PayGroupTypewise", payrollType, TenantId, UserId);

            var data = dt.AsEnumerable().Select(x => new
            {
                value = x["PayGroupID"]?.ToString(),
                text = x["PayGroupName"]?.ToString(),
                desc = x["Description"]?.ToString()
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEmployees(string payGroup, string payrollType, int? processMonth, int? processYear, DateTime? fromDate, DateTime? toDate)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_Payroll_GetEmployeesForBatch", TenantId,
                payGroup, payrollType == "1" ? "MONTHLY" : "CONTRACTUAL", processMonth, processYear, fromDate, toDate, 1);

            var data = dt.AsEnumerable().Select(x => new
            {
                employeeId = x["EmployeeID"].ToString(),
                code = x["EmpCode"].ToString(),
                name = x["EmployeeName"].ToString(),
                status = x["EmpIsActive"].ToString() == "ACTIVE" ? "Active" : "Inactive",
                payrollStatus = GetEmployeeBatchStatus(x),
                isFinalized = GetEmployeeBatchStatus(x) == 2,
                isPaid = Convert.ToInt32(x["IsAlreadyPaid"]) == 1,
                isEligible = Convert.ToInt32(x["IsEligible"]) == 1,
                isSelectable = GetEmployeeBatchStatus(x) != 2 && Convert.ToInt32(x["IsEligible"]) == 1,
                isChecked = x["EmpIsActive"].ToString() == "ACTIVE"
                    && Convert.ToInt32(x["IsEligible"]) == 1
                    && GetEmployeeBatchStatus(x) != 2
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private int GetEmployeeBatchStatus(DataRow row)
        {
            if (row.Table.Columns.Contains("Status") && row["Status"] != DBNull.Value)
                return Convert.ToInt32(row["Status"]);

            if (row.Table.Columns.Contains("StatusCode") && row["StatusCode"] != DBNull.Value)
                return Convert.ToInt32(row["StatusCode"]);

            if (row.Table.Columns.Contains("ProcessStatus") && row["ProcessStatus"] != DBNull.Value)
                return Convert.ToInt32(row["ProcessStatus"]);

            return row.Table.Columns.Contains("IsAlreadyPaid")
                && row["IsAlreadyPaid"] != DBNull.Value
                && Convert.ToInt32(row["IsAlreadyPaid"]) == 1
                    ? 2
                    : 0;
        }

        [HttpGet]
        public JsonResult GenerateRefNo( string payGroupId, string payrollType, int? processMonth, int? processYear, DateTime? fromDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(payGroupId))
                return Json(new { refNo = "" }, JsonRequestBehavior.AllowGet);

            string payrollTypeText =
                payrollType == "1" ? "MONTHLY" : "CONTRACTUAL";

            DataTable dt = clsDatabase.fnDataTable("SP_Payroll_GenerateBatchRefNo", TenantId, payGroupId, payrollTypeText,
                processMonth, processYear, fromDate, toDate);

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
                PayConfigId = x["PayConfigId"],
                name = x["PayConfigName"].ToString(),
                timeBasis = x["TimeBasis"].ToString()
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAttendanceEmployees(string payGroupId, int payrollType, string employeeIds, int? processMonth, int? processYear, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (string.IsNullOrEmpty(employeeIds))
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }

                var dt = new DataTable();
                dt = clsDatabase.fnDataTable("SP_Payroll_GetAttendanceEmployees", TenantId, payGroupId, payrollType, employeeIds, processMonth, processYear, fromDate, toDate);

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
            if (request == null || request.Employees == null || !request.Employees.Any())
            {
                return Json(new { Success = false, Message = "Employee list empty" });
            }

            string entryUser = Session["UserName"].ToString();

            int totalDays = request.PayrollType == "1"
                ? DateTime.DaysInMonth(request.ProcessYear.Value, request.ProcessMonth.Value)
                : (request.ToDate.Value - request.FromDate.Value).Days + 1;

            /* =============================
               1️⃣ CREATE PREVIEW BATCH
            ============================= */
            long previewBatchId = SP_Payroll_SaveBatch_Preview(
                TenantId,
                request.refNo,
                DateTime.Now,
                request.PayrollType,
                request.ProcessMonth,
                request.ProcessYear,
                request.FromDate,
                request.ToDate,
                request.Employees.Count,
                0m,
                0m,
                request.Comments,
                entryUser
            );
            /* 🔴 BLOCKED CASE */
            if (previewBatchId == -1)
            {
                return Json(new
                {
                    PreviewBatchID = -1,
                    Message = "Payroll draft already exists or is finalized."
                });
            }

            decimal totalCTC = 0m;
            decimal totalNet = 0m;

            var results = new List<EmployeeSalaryResponse>();

            /* =============================
               2️⃣ EMPLOYEE LOOP
            ============================= */
            foreach (var emp in request.Employees)
            {
                /* --- Salary Header --- */
                DataTable dtHeader = clsDatabase.fnDataTable(
                    "Payroll_SaaS_GetSalaryHeader",
                    TenantId,
                    emp.EmployeeId
                );

                if (dtHeader.Rows.Count == 0)
                    continue;

                DataRow h = dtHeader.Rows[0];

                int empSalaryConfigId = Convert.ToInt32(h["EmpSalaryConfigID"]);
                bool isPF = Convert.ToBoolean(h["IsPF_Applicable"]);
                bool isESIC = Convert.ToBoolean(h["IsESIC_Applicable"]);
                bool isPTax = Convert.ToBoolean(h["IsPTax_Applicable"]);
                int PayGroupID=Convert.ToInt32(h["PayGroupID"]);
                /* --- Salary Components --- */
                DataTable dtComp = clsDatabase.fnDataTable(
                    "Payroll_SaaS_GetSalaryDetails",
                    empSalaryConfigId
                );

                var components = MapComponents(dtComp, emp.PaidDays, totalDays);

                var salary = CalculateMonthlySalary(
                    components,
                    emp.EmployeeId,
                    request.ProcessMonth.Value,
                    request.ProcessYear.Value,
                    isPF,
                    isESIC,
                    isPTax,
                    PayrollValueResolver
                );

                if (salary == null)
                    continue;

                totalCTC += salary.MonthlyCTC;
                totalNet += salary.NetPay;

                /* =============================
                   3️⃣ SAVE PREVIEW EMPLOYEE
                   (NO RETURN VALUE)
                ============================= */
                long PreviewEmployeeID=SP_Payroll_SaveEmployee_Preview(
                    previewBatchId,
                    PayGroupID,
                    emp.EmployeeId,
                    emp.PaidDays,
                    salary.TotalEarnings,
                    salary.TotalDeductions,
                    salary.MonthlyCTC,
                    salary.NetPay,
                    entryUser
                );

                /* =============================
                   4️⃣ SAVE PREVIEW COMPONENTS (JSON)
                ============================= */
                string componentJson = JsonConvert.SerializeObject(
                    salary.Components.Select(c => new
                    {
                        c.PayConfigId,
                        c.PayConfigName,
                        c.PayType,
                        c.LogicType,
                        Amount = c.Amount,
                        ReferenceIds = c.ReferenceIds
                    })
                );

                clsDatabase.fnDBOperation(
                    "SP_Payroll_SaveEmployeeDetail_Preview",
                    PreviewEmployeeID,  
                    componentJson
                );

                results.Add(new EmployeeSalaryResponse
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeName = emp.EmployeeName,
                    PaidDays = emp.PaidDays,

                    TotalEarnings = salary.TotalEarnings,
                    TotalDeductions = salary.TotalDeductions,
                    NetPay = salary.NetPay,
                    MonthlyCTC = salary.MonthlyCTC,

                    Components = salary.Components
                });


            }


            /* =============================
               5️⃣ UPDATE PREVIEW TOTALS
            ============================= */
            SP_Payroll_UpdateBatchTotals_Preview(
                previewBatchId,
                totalCTC,
                totalNet
            );

            // =============================
            // BUILD PREVIEW (NO DB HIT)
            // =============================
            PayrollPivotResponse pivot = BuildPivot(request, results);
            // 🔑 attach PreviewBatchID
            pivot.PreviewBatchID = previewBatchId;
            // =============================
            // RETURN TO UI
            // =============================
            return Json(pivot, JsonRequestBehavior.AllowGet);
        }
        public static EmployeeSalaryResponse CalculateMonthlySalary(List<SalaryComponentResult> components, int employeeId, int processMonth, int processYear,
                                                               bool isPFApplicable, bool isESICApplicable, bool isPTaxApplicable, ComponentValueResolver valueResolver)
        {
            if (components == null || components.Count == 0)
                return null;

            // Determine total days in month for proration
            decimal totalDays = components.First().TotalDays > 0
                ? components.First().TotalDays
                : DateTime.DaysInMonth(processYear, processMonth);

            decimal totalEarnings = 0;
            decimal totalDeductions = 0;

            // Used for FORMULA evaluation mapping
            Dictionary<string, decimal> valueMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            /* ============================================================
                PASS 1: FIXED / MANUAL / EXCEL RAW DATA
               ============================================================ */
            foreach (var c in components.Where(x => x.LogicType == "FIXED" ||
                                                    x.LogicType == "MANUAL_INPUT" ||
                                                    x.LogicType == "EXCEL_UPLOAD" ||
                                                    (x.IsOther && x.OtherType == "LOANRECOVERY")))
            {
                if (c.LogicType == "EXCEL_UPLOAD" || (c.IsStatutory && c.StatutoryType == "TDS") || (c.IsOther && c.OtherType == "LOANRECOVERY"))
                {
                    // 1. Fetch raw value from DB (e.g., OT hours or Bonus amount)
                    decimal rawVal = valueResolver(employeeId, c.PayConfigId, processMonth, processYear, c);

                    c.BaseAmount = rawVal;
                    c.OriginalAmount = rawVal;
                    c.Rate = c.Amount; // Keep master rate in Rate field
                    c.Amount = ApplyRounding(c.BaseAmount, "");
                    valueMap[c.PayConfigName] = c.Amount;
                    continue;
                }

                // Normal Fixed/Manual Logic with Proration
                c.BaseAmount = c.Amount;
                decimal prorated = Prorate(c.BaseAmount, c.PaidDays, totalDays);
                c.OriginalAmount = prorated;
                c.Amount = ApplyRounding(prorated, c.RoundingType);
                valueMap[c.PayConfigName] = c.Amount;
            }

            ///* ============================================================
            //    PASS 2 : FORMULA & EXCEL LOGIC
            //   ============================================================ */
            //foreach (var c in components.Where(x => x.LogicType == "FORMULA" || x.LogicType == "EXCEL_UPLOAD"))
            //{
            //    decimal baseValue = 0;
            //    bool formulaEvaluated = false;

            //    if (c.LogicType == "EXCEL_UPLOAD")
            //    {
            //        if (!string.IsNullOrEmpty(c.CalculationExpression))
            //        {
            //            valueMap["{VAL}"] = c.BaseAmount;
            //            valueMap["{RATE}"] = c.Rate;
            //            valueMap["{PAIDDAYS}"] = c.PaidDays;
            //            valueMap["{WORKING_DAYS}"] = totalDays;

            //            baseValue = EvaluateDynamicFormulaEXCEL(c.CalculationExpression, valueMap);
            //            formulaEvaluated = true;
            //        }
            //        else
            //        {
            //            baseValue = c.BaseAmount;
            //        }
            //    }
            //    else
            //    {
            //        // Standard [BASIC]+[DA] formulas
            //        baseValue = EvaluateDynamicFormula(c.CalculationExpression, valueMap);
            //    }

            //    c.BaseAmount = baseValue;
            //    decimal calculated = 0;

            //    if (c.LogicType == "EXCEL_UPLOAD" && formulaEvaluated)
            //    {
            //        calculated = baseValue;
            //    }
            //    else
            //    {
            //        calculated = c.IsPercentage ? baseValue * (c.Rate / 100) : baseValue;
            //    }

            //// Apply Max Limit
            //if (c.MaxLimit.HasValue && c.MaxLimit.Value > 0 && calculated > c.MaxLimit.Value)
            //    calculated = c.MaxLimit.Value;

            //if (c.LogicType == "EXCEL_UPLOAD")
            //{
            //    c.OriginalAmount = calculated;
            //    c.Amount = ApplyRounding(calculated, c.RoundingType);
            //}
            //else
            //{
            //    decimal proratedFormula = Prorate(calculated, c.PaidDays, totalDays);
            //    c.OriginalAmount = proratedFormula;
            //    c.Amount = ApplyRounding(proratedFormula, c.RoundingType);
            //}

            //valueMap[c.PayConfigName] = c.Amount;

            /* ============================================================
                PASS 2 : FORMULA & EXCEL LOGIC
               ============================================================ */
            foreach (var c in components.Where(x => x.LogicType == "FORMULA" || x.LogicType == "EXCEL_UPLOAD"))
            {
                decimal baseValue = 0;
                bool formulaEvaluated = false;

                if (c.LogicType == "EXCEL_UPLOAD")
                {
                    if (!string.IsNullOrWhiteSpace(c.CalculationExpression))
                    {
                        valueMap["{VAL}"] = c.BaseAmount;
                        valueMap["{RATE}"] = c.Rate;
                        valueMap["{PAIDDAYS}"] = c.PaidDays;
                        valueMap["{WORKING_DAYS}"] = totalDays;

                        baseValue = EvaluateDynamicFormulaEXCEL(c.CalculationExpression, valueMap);
                        formulaEvaluated = true;
                    }
                    else
                    {
                        baseValue = c.BaseAmount;
                    }
                }
                else
                {
                    // Formula uses already-prorated values from valueMap
                    baseValue = EvaluateDynamicFormula(c.CalculationExpression, valueMap);
                }

                c.BaseAmount = baseValue;

                decimal calculated;

                if (c.LogicType == "EXCEL_UPLOAD" && formulaEvaluated)
                {
                    calculated = baseValue;
                }
                else
                {
                    calculated = c.IsPercentage
                        ? baseValue * (c.Rate / 100)
                        : baseValue;
                }

                // Apply Max Limit on final calculated value
                if (c.MaxLimit.HasValue && c.MaxLimit.Value > 0 && calculated > c.MaxLimit.Value)
                {
                    calculated = c.MaxLimit.Value;
                }

                c.OriginalAmount = calculated;
                c.Amount = ApplyRounding(calculated, c.RoundingType);

                valueMap[c.PayConfigName] = c.Amount;
            }
            /* ============================================================
                DYNAMIC GROSS CALCULATION (For Factory Employees)
               ============================================================ */
            decimal calculatedGrossSum = components
                .Where(x => (x.PayType ?? "").Trim().ToUpperInvariant() == "ALLOWANCES")
                .Sum(x => x.Amount);

            var grossComp = components.FirstOrDefault(x => x.IsGrossComponent);
            if (grossComp != null && grossComp.Amount == 0)
            {
                grossComp.Amount = calculatedGrossSum;
                grossComp.OriginalAmount = calculatedGrossSum;
                grossComp.BaseAmount = calculatedGrossSum;
                valueMap[grossComp.PayConfigName] = calculatedGrossSum;
            }

            /* ============================================================
                PASS 3 : STATUTORY CALCULATION
               ============================================================ */
            decimal grossAmount = (grossComp != null && grossComp.Amount > 0) ? grossComp.Amount : calculatedGrossSum;
            decimal actualMonthlyGross = (grossComp != null && grossComp.BaseAmount > 0) ? grossComp.BaseAmount : grossAmount;

            foreach (var c in components.Where(x => x.IsStatutory))
            {
                decimal dynamicBaseValue = ApplyRounding(EvaluateDynamicFormula(c.CalculationExpression, valueMap), c.RoundingType);

                // 🔹 PF
                if (c.StatutoryType.StartsWith("PF"))
                {
                    if (!isPFApplicable) { c.Amount = 0; continue; }
                    decimal pfBase = dynamicBaseValue;
                    if (c.MaxLimit.HasValue && c.MaxLimit.Value > 0 && pfBase > c.MaxLimit.Value)
                        pfBase = c.MaxLimit.Value;

                    c.BaseAmount = pfBase;
                    c.OriginalAmount = pfBase * (c.Rate / 100);
                    c.Amount = ApplyRounding(c.OriginalAmount, c.RoundingType);
                    valueMap[c.PayConfigName] = c.Amount;
                }
                // 🔹 ESIC
                else if (c.StatutoryType.StartsWith("ESIC"))
                {
                    bool isSlabEligible = false;
                    int slabStart = 0, slabEnd = 0, currentID = 0;
                    DataTable esidt = clsDatabase.fnDataTable("SP_Check_ESIC_SLAB", employeeId);
                    if (esidt != null && esidt.Rows.Count > 0)
                    {
                        slabStart = Convert.ToInt32(esidt.Rows[0]["SlabStartMonth"]);
                        slabEnd = Convert.ToInt32(esidt.Rows[0]["SlabEndMonth"]);
                        isSlabEligible = Convert.ToBoolean(esidt.Rows[0]["IsEligible"]);
                        currentID = Convert.ToInt32(esidt.Rows[0]["ID"]);
                    }

                    bool isInActiveWindow = false;
                    if (isSlabEligible)
                    {
                        if (slabStart <= slabEnd) isInActiveWindow = (processMonth >= slabStart && processMonth <= slabEnd);
                        else isInActiveWindow = (processMonth >= slabStart || processMonth <= slabEnd);
                    }

                    decimal esicLimit = c.MaxLimit ?? 21000;
                    if (!isESICApplicable || (!isInActiveWindow && actualMonthlyGross > esicLimit))
                    {
                        c.Amount = 0;
                        c.OriginalAmount = 0;
                    }
                    else
                    {
                        c.BaseAmount = dynamicBaseValue;
                        c.OriginalAmount = dynamicBaseValue * (c.Rate / 100);
                        c.Amount = Math.Ceiling(c.OriginalAmount); // ESIC rounds UP
                        if (processMonth == slabEnd) clsDatabase.fnDataTable("SP_UPDATE_ESIC_TAG", employeeId, currentID);
                    }
                    valueMap[c.PayConfigName] = c.Amount;
                }
                // 🔹 PTAX
                else if (c.StatutoryType == "PTAX")
                {
                    if (!isPTaxApplicable) { c.Amount = 0; continue; }
                    decimal slabAmount = CalculatePTaxFromJson(grossAmount, c.PTaxSlabsJson);
                    c.BaseAmount = dynamicBaseValue;
                    c.OriginalAmount = slabAmount;
                    c.Amount = ApplyRounding(slabAmount, c.RoundingType);
                    valueMap[c.PayConfigName] = c.Amount;
                }
                // 🔹 GRATUITY
                else if (c.StatutoryType == "GRATUITY")
                {
                    c.BaseAmount = dynamicBaseValue;
                    c.OriginalAmount = dynamicBaseValue * (c.Rate / 100);
                    c.Amount = ApplyRounding(c.OriginalAmount, c.RoundingType);
                    valueMap[c.PayConfigName] = c.Amount;
                }
            }

            /* ============================================================
                TOTALS & RESPONSE
               ============================================================ */
            decimal totalEmployerCC = 0;
            foreach (var c in components)
            {
                var pt = (c.PayType ?? "").Trim().ToUpperInvariant();
                if (pt == "ALLOWANCES") totalEarnings += c.OriginalAmount;
                else if (pt == "DEDUCTION") totalDeductions += c.OriginalAmount;
                else if (pt == "CC") totalEmployerCC += c.OriginalAmount;
            }

            decimal roundedEarnings = Math.Round(totalEarnings, 0);
            decimal roundedDeductions = Math.Round(totalDeductions, 0);
            decimal roundedEmployerCC = Math.Round(totalEmployerCC, 0);

            return new EmployeeSalaryResponse
            {
                TotalEarnings = roundedEarnings,
                TotalDeductions = roundedDeductions,
                NetPay = roundedEarnings - roundedDeductions,
                EmployerContribution = roundedEmployerCC,
                MonthlyCTC = roundedEarnings + roundedEmployerCC,
                Components = components
            };

            //return new EmployeeSalaryResponse
            //{
            //    TotalEarnings = Math.Round(totalEarnings, 0),
            //    TotalDeductions = Math.Round(totalDeductions, 0),
            //    //NetPay = Math.Round(totalEarnings - totalDeductions, 0),
            //    NetPay = (decimal)Math.Floor(totalEarnings - totalDeductions),
            //    EmployerContribution = Math.Round(totalEmployerCC, 0),
            //    MonthlyCTC = Math.Round(totalEarnings + totalEmployerCC, 0),
            //    Components = components
            //};

        }

        //public static EmployeeSalaryResponse CalculateMonthlySalary(List<SalaryComponentResult> components, int employeeId, int processMonth, int processYear,
        //                                                             bool isPFApplicable, bool isESICApplicable, bool isPTaxApplicable, ComponentValueResolver valueResolver)
        //{
        //    if (components == null || components.Count == 0)
        //        return null;

        //    decimal totalDays = components.First().TotalDays > 0
        //        ? components.First().TotalDays
        //        : DateTime.DaysInMonth(processYear, processMonth);

        //    //decimal totalDays = 30;

        //    decimal totalEarnings = 0;
        //    decimal totalDeductions = 0;

        //    // Used ONLY for FORMULA evaluation
        //    Dictionary<string, decimal> valueMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        //    /* ============================================================
        //       PASS 1: FIXED / MANUAL / EXCEL RAW DATA
        //       ============================================================ */
        //    foreach (var c in components.Where(x => x.LogicType == "FIXED" ||
        //                                           x.LogicType == "MANUAL_INPUT" ||
        //                                           x.LogicType == "EXCEL_UPLOAD" ||
        //                                           (x.IsOther && x.OtherType == "LOANRECOVERY")))
        //    {
        //        if (c.LogicType == "EXCEL_UPLOAD" || (c.IsStatutory && c.StatutoryType == "TDS") || (c.IsOther && c.OtherType == "LOANRECOVERY"))
        //        {
        //            // 1. Fetch raw value from DB (e.g., 2.1 for OT or 1.0 for Attendance)
        //            decimal rawVal = valueResolver(employeeId, c.PayConfigId, processMonth, processYear, c);

        //            // 2. Store it in the component object for Pass 2
        //            c.BaseAmount = rawVal;

        //            // 3. Map the component name so OTHER formulas can use this result
        //            // We DON'T set {VAL} or {RATE} here because they will get overwritten in the loop.
        //            c.OriginalAmount = rawVal;
        //            c.Rate = c.Amount;
        //            c.Amount = ApplyRounding(c.BaseAmount, "");
        //            valueMap[c.PayConfigName] = c.Amount;

        //            continue;
        //        } 
        //        // Normal Fixed/Manual Logic
        //        c.BaseAmount = c.Amount;
        //        c.OriginalAmount = Prorate(c.BaseAmount, c.PaidDays, totalDays);
        //        c.Amount = ApplyRounding(Prorate(c.BaseAmount, c.PaidDays, totalDays), c.RoundingType);
        //        valueMap[c.PayConfigName] = c.Amount;
        //    }

        //    /* ============================================================
        //        PASS 2 : FORMULA & EXCEL LOGIC (Handles Fixed & Formula)
        //       ============================================================ */
        //    foreach (var c in components.Where(x => x.LogicType == "FORMULA" || x.LogicType == "EXCEL_UPLOAD" ))
        //    {
        //        decimal baseValue = 0;
        //        bool formulaEvaluated = false;
        //        if (c.LogicType == "EXCEL_UPLOAD")
        //        {
        //            // 1. If there IS a formula (like Attendance Bonus or OT)
        //            if (!string.IsNullOrEmpty(c.CalculationExpression))
        //            {
        //                valueMap["{VAL}"] = c.BaseAmount;
        //                valueMap["{RATE}"] = c.Rate;
        //                valueMap["{PAIDDAYS}"] = c.PaidDays;
        //                valueMap["{WORKING_DAYS}"] = totalDays;

        //                baseValue = EvaluateDynamicFormulaEXCEL(c.CalculationExpression, valueMap);
        //                formulaEvaluated = true;
        //            }
        //            else
        //            {
        //                // 2. If there is NO formula (e.g., a simple Incentive upload)
        //                // Just use the raw value collected in Pass 1
        //                baseValue = c.BaseAmount;
        //            }
        //        }
        //        else
        //        {
        //            // Standard [BASIC]+[DA] formulas
        //            baseValue = EvaluateDynamicFormula(c.CalculationExpression, valueMap);
        //        }

        //        c.BaseAmount = baseValue;

        //        //// Apply Percentage logic (usually 100% for fixed uploads)
        //        //decimal calculated = c.IsPercentage ? baseValue * (c.Rate / 100) : baseValue;
        //        // --- LOGIC UPDATE STARTS HERE ---
        //        decimal calculated = 0;

        //        // If it's an Excel Upload with a formula, we assume the formula 
        //        // result (baseValue) is the final amount.
        //        if (c.LogicType == "EXCEL_UPLOAD" && formulaEvaluated)
        //        {
        //            calculated = baseValue;
        //        }
        //        else
        //        {
        //            // Standard behavior: Apply Percentage/Rate logic
        //            calculated = c.IsPercentage ? baseValue * (c.Rate / 100) : baseValue;
        //        }
        //        // --- LOGIC UPDATE ENDS HERE ---
        //        // Apply Max Limit
        //        if (c.MaxLimit.HasValue && c.MaxLimit.Value > 0 && calculated > c.MaxLimit.Value)
        //            calculated = c.MaxLimit.Value;

        //        if (c.LogicType == "EXCEL_UPLOAD")
        //        {
        //            // No proration for manual Excel uploads
        //            c.OriginalAmount = calculated;
        //            c.Amount = ApplyRounding(calculated, c.RoundingType);
        //        }
        //        else
        //        {
        //            // Standard formula proration
        //            c.OriginalAmount = Prorate(calculated, c.PaidDays, totalDays);
        //            c.Amount = ApplyRounding(Prorate(calculated, c.PaidDays, totalDays), c.RoundingType);
        //        }

        //        // Update the map for next components
        //        valueMap[c.PayConfigName] = c.Amount;
        //    }

        //    /* =========================
        //       PASS 3 : STATUTORY (DYNAMIC VIA FORMULA)
        //       ========================= */
        //    decimal basicAmount = components
        //        .Where(x => x.IsBasicComponent)
        //        .Sum(x => x.Amount);

        //    decimal grossAmount = components
        //        .Where(x => x.IsGrossComponent)
        //        .Sum(x => x.Amount);

        //    decimal actualMonthlyGross = components
        //        .Where(x => x.IsGrossComponent)
        //        .Select(x => x.BaseAmount)
        //        .FirstOrDefault();
        //    foreach (var c in components.Where(x => x.IsStatutory))
        //    {
        //        // 1. DYNAMIC BASE CALCULATION
        //        // Use the stored formula (CalculationExpression) to find the Base (e.g., "[BASIC]+[DA]")
        //        decimal dynamicBaseValue = ApplyRounding(EvaluateDynamicFormula(
        //            c.CalculationExpression,
        //            valueMap
        //        ), c.RoundingType);

        //        // 🔹 PF CALCULATION
        //        if (c.StatutoryType.StartsWith("PF"))
        //        {
        //            if (!isPFApplicable) { c.Amount = 0; continue; }

        //            decimal pfBase = dynamicBaseValue;

        //            // Apply Statutory Ceiling (e.g., 15,000)
        //            if (c.MaxLimit.HasValue && c.MaxLimit.Value > 0 && pfBase > c.MaxLimit.Value)
        //                pfBase = c.MaxLimit.Value;

        //            c.BaseAmount = pfBase;
        //            // Calculation on already-prorated base
        //            c.OriginalAmount = pfBase * (c.Rate / 100);
        //            c.Amount = ApplyRounding(pfBase * (c.Rate / 100), c.RoundingType);

        //            valueMap[c.PayConfigName] = c.Amount;
        //        }

        //        //// 🔹 ESIC CALCULATION
        //        //else if (c.StatutoryType.StartsWith("ESIC"))
        //        //{
        //        //    // Eligibility check usually remains against the MASTER Gross (Fixed)
        //        //    // You can use actualMonthlyGross calculated earlier for this.
        //        //    if (!isESICApplicable || (c.MaxLimit.HasValue && c.MaxLimit.Value > 0 && actualMonthlyGross > c.MaxLimit.Value))
        //        //    {
        //        //        c.Amount = 0;
        //        //        continue;
        //        //    }

        //        //    c.BaseAmount = dynamicBaseValue; // Based on formula like "[GROSS]"
        //        //    c.Amount = ApplyRounding(dynamicBaseValue * (c.Rate / 100), c.RoundingType);

        //        //    valueMap[c.PayConfigName] = c.Amount;
        //        //}

        //        // 🔹 ESIC CALCULATION (Integrated with Slab Logic)
        //        else if (c.StatutoryType.StartsWith("ESIC"))
        //        {
        //            bool isSlabEligible = false;
        //            int slabStart = 0;
        //            int slabEnd = 0;
        //            int currentID = 0;

        //            // 1. Fetch the Slab Status from your stored procedure
        //            DataTable esidt = clsDatabase.fnDataTable("SP_Check_ESIC_SLAB", employeeId);

        //            if (esidt != null && esidt.Rows.Count > 0)
        //            {
        //                slabStart = Convert.ToInt32(esidt.Rows[0]["SlabStartMonth"]);
        //                slabEnd = Convert.ToInt32(esidt.Rows[0]["SlabEndMonth"]);
        //                isSlabEligible = Convert.ToBoolean(esidt.Rows[0]["IsEligible"]);
        //                currentID = Convert.ToInt32(esidt.Rows[0]["ID"]);
        //            }

        //            // 2. Cross-Year Window Logic
        //            // If slabStart is 10 (Oct) and slabEnd is 3 (March), then months 10,11,12 OR 1,2,3 are valid.
        //            bool isInActiveWindow = false;
        //            if (isSlabEligible)
        //            {
        //                if (slabStart <= slabEnd) // Normal window (e.g., 4 to 9)
        //                    isInActiveWindow = (processMonth >= slabStart && processMonth <= slabEnd);
        //                else // Crossing New Year window (e.g., 10 to 3)
        //                    isInActiveWindow = (processMonth >= slabStart || processMonth <= slabEnd);
        //            }

        //            // 3. Eligibility Check: Apply ESIC if in Window OR if Gross <= Limit
        //            // (ActualMonthlyGross is the fixed master gross used for entry-level eligibility)
        //            decimal esicLimit = c.MaxLimit ?? 21000;

        //            if (!isESICApplicable || (!isInActiveWindow && actualMonthlyGross > esicLimit))
        //            {
        //                c.OriginalAmount = 0;
        //                c.Amount = 0;
        //                valueMap[c.PayConfigName] = 0;
        //            }
        //            else
        //            {
        //                // 4. Calculate based on dynamicBaseValue (the earned gross/formula result)
        //                c.BaseAmount = dynamicBaseValue;

        //                // ESIC Rule: Round UP to the next higher rupee (Ceiling)
        //                c.OriginalAmount = dynamicBaseValue * (c.Rate / 100);
        //                c.Amount = Math.Ceiling(dynamicBaseValue * (c.Rate / 100));

        //                valueMap[c.PayConfigName] = c.Amount;

        //                // 5. Update the Tag if we've hit the end of the contribution period
        //                if (processMonth == slabEnd)
        //                {
        //                    clsDatabase.fnDataTable("SP_UPDATE_ESIC_TAG", employeeId, currentID);
        //                }
        //            }
        //        }
        //        // 🔹 PTAX CALCULATION
        //        else if (c.StatutoryType == "PTAX")
        //        {
        //            if (!isPTaxApplicable) { c.Amount = 0; continue; }

        //            // Use formula result (Actual Earned Gross) for slab check
        //            decimal slabAmount = CalculatePTaxFromJson(grossAmount, c.PTaxSlabsJson);

        //            c.BaseAmount = dynamicBaseValue;
        //            c.OriginalAmount = slabAmount;
        //            c.Amount = ApplyRounding(slabAmount, c.RoundingType);

        //            valueMap[c.PayConfigName] = c.Amount;
        //        }
        //        // 🔹 PTAX CALCULATION
        //        else if (c.StatutoryType == "GRATUITY")
        //        { 
        //            c.BaseAmount = dynamicBaseValue;
        //            c.OriginalAmount = dynamicBaseValue * (c.Rate / 100);
        //            c.Amount = ApplyRounding(dynamicBaseValue * (c.Rate / 100), c.RoundingType);

        //            valueMap[c.PayConfigName] = c.Amount;
        //        }
        //    }
        //    /* =========================
        //       TOTALS
        //    ========================= */
        //    decimal totalEmployerCC = 0;        // Employer contributions (CTC part, NOT net)

        //    foreach (var c in components)
        //    {
        //        var pt = (c.PayType ?? "").Trim().ToUpperInvariant();

        //        if (pt == "ALLOWANCES")
        //        {
        //            totalEarnings += c.OriginalAmount;
        //        }
        //        else if (pt == "DEDUCTION")
        //        {
        //            totalDeductions += c.OriginalAmount;
        //        }
        //        else if (pt == "CC")
        //        {
        //            totalEmployerCC += c.OriginalAmount;   // employer cost only
        //        }
        //    }

        //    var netPay = totalEarnings - totalDeductions;          // ✅ CC excluded
        //    var monthlyCTC = totalEarnings + totalEmployerCC;      // ✅ optional for pivot use


        //    return new EmployeeSalaryResponse
        //    {
        //        TotalEarnings = Math.Round(totalEarnings, 0),
        //        TotalDeductions = Math.Round(totalDeductions, 0),
        //        NetPay = Math.Round(netPay, 0),
        //        EmployerContribution = Math.Round(totalEmployerCC, 0), // ✅ add this property (recommended)
        //        MonthlyCTC = Math.Round(monthlyCTC, 0),                // ✅ add this property (recommended)
        //        Components = components
        //    };

        //}
        private static decimal EvaluateDynamicFormulaEXCEL(string formula, Dictionary<string, decimal> valueMap)
        {
            if (string.IsNullOrWhiteSpace(formula)) return 0;

            try
            {
                string processedFormula = formula;

                // 1. CONVERT EXCEL "IF" TO .NET "IIF"
                // .NET DataTable.Compute uses "IIF" for logic.
                processedFormula = Regex.Replace(processedFormula, @"\bIF\(", "IIF(", RegexOptions.IgnoreCase);

                // 2. DYNAMIC TAG REPLACEMENT
                // This finds everything inside { } and looks it up in your valueMap
                processedFormula = Regex.Replace(processedFormula, @"\{([^}]+)\}", match =>
                {
                    string key = match.Value; // e.g., "{BASIC AMOUNT}" or "{VAL}"

                    if (valueMap.TryGetValue(key, out decimal val))
                    {
                        return val.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }

                    // Fallback for component names without braces if they exist in map
                    string plainKey = match.Groups[1].Value;
                    if (valueMap.TryGetValue(plainKey, out decimal val2))
                    {
                        return val2.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }

                    return "0"; // If variable not found, return 0 to prevent crash
                });

                // 3. CLEANUP
                // Replace common Excel symbols if users typed them (like single = for comparison)
                // Note: DataTable.Compute handles most math (+, -, *, /) automatically.

                // 4. EXECUTE CALCULATION
                using (System.Data.DataTable dt = new System.Data.DataTable())
                {
                    object result = dt.Compute(processedFormula, "");
                    return Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                // Log error: formula was likely malformed
                return 0;
            }
        }
        private static decimal EvaluateDynamicFormula(string formula, Dictionary<string, decimal> valueMap)
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
        private decimal PayrollValueResolver(int employeeId, int payConfigId, int month, int year, SalaryComponentResult component)
        {
           
            if (component.IsOther && component.OtherType == "LOANRECOVERY")
            {
                string actualLoanId;
                decimal emi = GetEmployeeLoanEMI(employeeId, month, year, out actualLoanId);

                // 🔑 Save the LoanId into the component for the database
                component.ReferenceIds = actualLoanId;

                return emi;
            }

            // TDS ... rest of your code
            if (component.IsStatutory && component.StatutoryType == "TDS")
                return GetEmployeeTDS(employeeId, month, year);

            if (component.LogicType == "EXCEL_UPLOAD")
                return GetExcelUploadedAmount(employeeId, payConfigId, month, year);

            return component.Amount;
        } 
        private decimal IsLoanAlreadyPaid(int employeeId, int month, int year, out string loanIds)
        {
            loanIds = "0";
            DataTable dt = clsDatabase.fnDataTable("PRC_CheckLoanPaidForMonth", employeeId, month, year, TenantId);

            if (dt != null && dt.Rows.Count > 0 && Convert.ToDecimal(dt.Rows[0]["AmountPaid"]) > 0)
            {
                loanIds = dt.Rows[0]["CombinedLoanIds"].ToString(); // "101,102"
                return Convert.ToDecimal(dt.Rows[0]["AmountPaid"]);
            }
            return 0m;
        }
        private decimal GetEmployeeLoanEMI(int employeeId, int month, int year, out string loanIdStr)
        {
            loanIdStr = "0";

            // 1. Check if multiple loans were already paid
            decimal amountPaid = IsLoanAlreadyPaid(employeeId, month, year, out string paidIds);

            if (amountPaid > 0)
            {
                loanIdStr = paidIds;
                return amountPaid;
            }

            // 2. Else: Follow your "TOP 1" rule for PENDING loans
            string monthName = new DateTime(year, month, 1).ToString("MMMM", CultureInfo.InvariantCulture);
            DataTable dt = clsDatabase.fnDataTable("PRC_Bulk_EmployeeLoan_PendingList", employeeId.ToString(), monthName, year, TenantId);

            if (dt != null && dt.Rows.Count > 0)
            {
                loanIdStr = dt.Rows[0]["IDLoan"].ToString(); // Only Top 1 ID
                return Convert.ToDecimal(dt.Rows[0]["AmountToBePaid"]);
            }

            return 0m;
        }
        private decimal GetEmployeeTDS(int employeeId, int monthSalaryMonth, int SalaryYear)
        {
            DataTable dt = clsDatabase.fnDataTable("Payroll_GetEmployeeTDS", employeeId, monthSalaryMonth, SalaryYear, TenantId);

            if (dt != null && dt.Rows.Count > 0)
            {
                 
                return Convert.ToDecimal(dt.Rows[0]["TdsAmount"]);
            }

            return 0m;
        }
        private decimal GetExcelUploadedAmount(int employeeId, int payConfigId, int month, int year)
        {
            try
            {
                // Calling the SP
                object result = clsDatabase.fnDBOperation("SP_Payroll_GetVariableComponentInput", employeeId, payConfigId,month,year,TenantId);

                // If result is null, return 0; otherwise convert to decimal
                return result != null ? Convert.ToDecimal(result) : 0m;
            }
            catch (Exception ex)
            {
                // Log error if needed
                return 0m;
            }
        } 
        private PayrollPivotResponse BuildPivot(PayrollProcessRequest request, List<EmployeeSalaryResponse> results)
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
                        IsBasicComponent = c.IsBasicComponent,

                        // 🔒 Only loan is conceptually waivable/editable
                        IsWaivable = isLoan,
                        IsEditable = isLoan,
                        ReferenceIds=c.ReferenceIds,
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
                    ManualAddition = emp.ManualAddition,
                    ManualDeduction = emp.ManualDeduction,

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

                    // 🔑 Fix: Initialize with default value to satisfy the compiler
                    string processedLoanIds = "0";

                    decimal loanPaidAmount = isLoanCell
                            ? IsLoanAlreadyPaid(
                                emp.EmployeeId,
                                request.ProcessMonth.GetValueOrDefault(),
                                request.ProcessYear.GetValueOrDefault(),
                                out processedLoanIds // Now properly assigned
                                )
                            : 0m;

                    bool loanAlreadyPaid = loanPaidAmount > 0;

                    // Use processed IDs if paid, otherwise use the calculated component IDs
                    string finalReferenceIds = loanAlreadyPaid ? processedLoanIds : (comp?.ReferenceIds ?? "0");

                    row.Cells.Add(new PivotCellDto
                    {
                        PayConfigId = col.PayConfigId,
                        Amount = amount,
                        BaseAmount = comp?.BaseAmount ?? 0,
                        IsEditable = isLoanCell && !loanAlreadyPaid,
                        IsWaivable = isLoanCell && !loanAlreadyPaid,
                        IsWaived = false,
                        OriginalAmount = isLoanCell ? amount : 0,

                        // 🔑 Pass the IDs to the UI
                        ReferenceIds = isLoanCell ? finalReferenceIds : null
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
                DataTable dtEmpBatch = clsDatabase.fnDataTable("SP_Payroll_SaaS_SaveBatchEmployee", payrollBatchId, employeesJson, TenantId, EntryUser);

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
            string UserId = Session["UserId"].ToString();
            var dt = clsDatabase.fnDataTable("SP_Payroll_SaaS_GetBatchSalary", TenantId, UserId);

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

                ProcessMonth = r["ProcessMonth"].ToString(),
                ProcessYear = r["ProcessYear"].ToString(),
                CreatedDate = Convert.ToDateTime(r["CreatedDate"])
            }).ToList();


            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetPayrollEmployeeWiseDashboard()
        {
            try
            {
                string UserId = Session["UserId"].ToString();
                DataTable dt = clsDatabase.fnDataTable("SP_Payroll_SaaS_GetEmployeeSalary", TenantId, UserId);

                var result = dt.AsEnumerable().Select(r => new
                {
                    PreviewBatchID = Convert.ToInt64(r["PreviewBatchID"]), // Using Int64 for safety with BIGINT
                    PayrollProcessEmployeeID = Convert.ToInt64(r["PayrollProcessEmployeeID"]),
                    EmployeeId = Convert.ToInt32(r["EmployeeID"]),
                    EmployeeCode = r["EmployeeCode"]?.ToString(),
                    EmployeeNo = r["EmployeeNo"]?.ToString(),
                    EmployeeName = r["EmployeeName"]?.ToString(),

                    PayGroupID = r.Table.Columns.Contains("PayGroupID") && r["PayGroupID"] != DBNull.Value
                                 ? Convert.ToInt32(r["PayGroupID"]) : 0,
                    PayGroupName = r["PayGroupName"]?.ToString(),
                    PeriodText = r["PeriodText"]?.ToString(),

                    ProcessMonth = Convert.ToInt32(r["ProcessMonth"]),
                    ProcessYear = Convert.ToInt32(r["ProcessYear"]),

                    GrossPay = Convert.ToDecimal(r["GrossPay"]),
                    Deduction = Convert.ToDecimal(r["Deduction"]),
                    NetPay = Convert.ToDecimal(r["NetPay"]),

                    StatusText = r["StatusText"]?.ToString(),
                    StatusClass = r["StatusClass"]?.ToString(),
                    RejectionReason = r["RejectionReason"].ToString(),
                    // 🔑 IMPORTANT: Capture numeric status (1=Draft, 2=Approved, 3=Rejected)
                    Status = r.Table.Columns.Contains("StatusCode") ? Convert.ToInt32(r["StatusCode"]) : 1
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
        public JsonResult GetPayGroupList()
        {
            try
            {
                string UserId = Session["UserId"].ToString();
                // 1. Reuse the SAME Stored Procedure
                DataTable dt = clsDatabase.fnDataTable("SP_Payroll_SaaS_GetEmployeeSalary", TenantId, UserId);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }

                // 2. Select Distinct PayGroupID and PayGroupName using LINQ
                var result = dt.AsEnumerable()
                    .Select(r => new
                    {
                        PayGroupId = r["PayGroupID"] != DBNull.Value ? Convert.ToInt32(r["PayGroupID"]) : 0,
                        PayGroupName = r["PayGroupName"]?.ToString()
                    })
                    .Where(x => x.PayGroupId > 0 && !string.IsNullOrEmpty(x.PayGroupName)) // Filter out invalid rows
                    .GroupBy(x => x.PayGroupId) // Group by ID to ensure distinctness
                    .Select(g => g.First())     // Select the first item from each group
                    .OrderBy(x => x.PayGroupName) // Optional: Sort alphabetically
                    .ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Handle error gracefully
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }
        

        [HttpGet]
        public JsonResult OpenBatchView(int payrollBatchId, int payGroupId, int batchStatus)
        {
            try
            {
                DataTable dt;

                if (batchStatus == 2) //FINAL/APPROVE
                {
                    dt = clsDatabase.fnDataTable(
                        "SP_Payroll_SaaS_GetSalaryBatch",
                        payrollBatchId,
                        TenantId
                    );
                }
                else // DRAFT/REJECT
                {
                    dt = clsDatabase.fnDataTable(
                        "SP_Payroll_SaaS_GetSalaryBatch_Preview",
                        payrollBatchId,
                        TenantId
                    );
                }


                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "No payroll batch data found."
                    }, JsonRequestBehavior.AllowGet);
                }

                /* ===============================
                   1️⃣ Employees
                =============================== */
                var employees = new List<EmployeeSalaryResponse>();

                foreach (DataRow r in dt.Rows)
                {
                    employees.Add(new EmployeeSalaryResponse
                    {
                        EmployeeId = GetInt(r, "EmployeeID"),
                        EmployeeName = GetString(r, "EmployeeName"),

                        PaidDays = GetDecimal(r, "TotalPaidDays"),
                        TotalEarnings = GetDecimal(r, "TotalGross"),
                        TotalDeductions = GetDecimal(r, "TotalDeduction"),
                        NetPay = GetDecimal(r, "TotalTakeHome"),
                        MonthlyCTC = GetDecimal(r, "MonthlyCTC"),

                        ManualAddition = GetDecimal(r, "ManualAddition"),
                        ManualDeduction = GetDecimal(r, "ManualDeduction"),
                        
                        Components = r["ComponentJson"] == DBNull.Value
                            ? new List<SalaryComponentResult>()
                            : JsonConvert.DeserializeObject<List<SalaryComponentResult>>(
                                r["ComponentJson"].ToString()
                            )
                    });
                }

                /* ===============================
                   2️⃣ Payroll Context
                =============================== */
                DataRow h = dt.Rows[0];

                var request = new PayrollProcessRequest
                {
                    PayrollType =
                        h["PayrollType"].ToString() == "MONTHLY" ? "1" : "2",

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

                /* ===============================
                   3️⃣ Build Pivot
                =============================== */
                var pivot = BuildPivot(request, employees);

                return Json(new
                {
                    Success = true,
                    RefNo = GetString(h, "RefNo"),
                    ProcessMonth = GetInt(h, "ProcessMonth"),
                    ProcessYear = GetInt(h, "ProcessYear"),
                    ApprovedCount = GetInt(h, "ApprovedCount"),
                    BatchStatus = batchStatus,   // 🔑 IMPORTANT
                    Data = pivot
                }, JsonRequestBehavior.AllowGet);
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

        
        [HttpPost]
        public JsonResult ApproveBatch(long previewBatchId)
        {
            try
            {
                string validationLoanId;
                string userName = Session["UserName"]?.ToString();
                if (string.IsNullOrEmpty(userName))
                    return Json(new { success = false, message = "Session expired" });

                if (previewBatchId <= 0)
                    return Json(new { success = false, message = "Invalid preview batch" });

                /* =========================================
                   1. GET PREVIEW BATCH CONTEXT
                ========================================= */
                DataTable dtBatch = clsDatabase.fnDataTable(
                    "SP_Payroll_SaaS_GetPreviewBatchContext",
                    previewBatchId,
                    TenantId
                );

                if (dtBatch == null || dtBatch.Rows.Count == 0)
                    return Json(new { success = false, message = "Preview batch not found" });

                int processMonth = Convert.ToInt32(dtBatch.Rows[0]["ProcessMonth"]);
                int processYear = Convert.ToInt32(dtBatch.Rows[0]["ProcessYear"]);

                string monthName = new DateTime(processYear, processMonth, 1)
                    .ToString("MMMM", CultureInfo.InvariantCulture);

                /* =========================================
                   2. GET TENANT LOAN PAYCONFIG
                ========================================= */
                int? loanPayConfigId = null;

                DataTable dtLoanCfg = clsDatabase.fnDataTable(
                    "SP_Payroll_SaaS_GetTenantLoanPayConfigId",
                    TenantId
                );

                if (dtLoanCfg != null && dtLoanCfg.Rows.Count > 0)
                    loanPayConfigId = Convert.ToInt32(dtLoanCfg.Rows[0]["PayConfigId"]);

                /* =========================================
                   3. BUILD LOAN JSON FROM PREVIEW TABLES
                ========================================= */
                var loanJsonList = new List<object>();

                if (loanPayConfigId.HasValue)
                {
                    DataTable dtLoans = clsDatabase.fnDataTable(
                        "SP_Payroll_SaaS_GetLoanComponents_FromPreview",
                        previewBatchId,
                        loanPayConfigId.Value,
                        TenantId
                    );

                    foreach (DataRow r in dtLoans.Rows)
                    {
                        decimal previewPayValue = Convert.ToDecimal(r["PayValue"]);
                        if (previewPayValue <= 0)
                            continue;

                        decimal expectedEmi = GetEmployeeLoanEMI(
                            Convert.ToInt32(r["EmployeeID"]),
                            processMonth,
                            processYear,
                            out validationLoanId
                        );

                        if (expectedEmi != previewPayValue)
                            throw new Exception($"Loan EMI mismatch for Employee {r["EmployeeID"]}");

                        DataTable dtPending = clsDatabase.fnDataTable(
                                          "PRC_Bulk_EmployeeLoan_PendingList",
                                            Convert.ToInt32(r["EmployeeID"]),
                                           monthName,
                                           processYear,
                                            TenantId
                                        );

                        if (dtPending == null || dtPending.Rows.Count == 0)
                            continue;

                        DataRow row = dtPending.Rows[0];

                        loanJsonList.Add(new
                        {
                            IDLoan = Convert.ToInt32(row["IDLoan"]),
                            IDEmployee = Convert.ToInt32(r["EmployeeID"]),
                            InstallmentMonth = processMonth,
                            InstallmentYear = processYear,
                            BalanceBefore = Convert.ToDecimal(row["BalanceBefore"]),
                            PrincipalComponent = Convert.ToDecimal(row["PrincipalComponent"]),
                            InterestComponent = Convert.ToDecimal(row["InterestComponent"]),
                            AmountPaid = Convert.ToDecimal(row["AmountToBePaid"]),
                            BalanceAfter = Convert.ToDecimal(row["BalanceAfter"]),
                            WaiverAmount = Convert.ToDecimal(row["WaiverAmount"]),
                            WaiverType = row["WaiverType"]?.ToString(),
                            EmployeeNo = row["EmployeeNo"]?.ToString(),
                            EmployeeName = row["EmployeeName"]?.ToString()
                        });
                    }
                }

                /* =========================================
                   4. PROCESS LOAN (ONCE)
                ========================================= */
                if (loanJsonList.Any())
                {
                    string jsonData = JsonConvert.SerializeObject(loanJsonList);

                    DataTable dtLoanProcess = clsDatabase.fnDataTable(
                        "PRC_Bulk_EmployeeLoan_Process",
                        jsonData,
                        monthName,
                        processYear,
                        userName,
                        TenantId, "Salary", "Processed via Salary Process"
                    );

                    if (dtLoanProcess == null ||
                        Convert.ToInt32(dtLoanProcess.Rows[0]["Code"]) != 1)
                    {
                        throw new Exception(dtLoanProcess.Rows[0]["Result"].ToString());
                    }
                }

                /* =========================================
                   5. FINALIZE PREVIEW → FINAL (NO JSON)
                ========================================= */
                string ApprovedBy= userName;
                DataTable dtFinalize = clsDatabase.fnDataTable(
                    "SP_Payroll_SaaS_FinalizeBatch_FromPreview",
                    previewBatchId,
                    TenantId,
                    ApprovedBy
                );

                if (dtFinalize == null ||
                    Convert.ToInt32(dtFinalize.Rows[0]["StatusCode"]) != 1)
                {
                    throw new Exception(dtFinalize.Rows[0]["Message"].ToString());
                }

                return Json(new
                {
                    success = true,
                    message = "Payroll batch approved successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        static decimal GetDecimal(DataRow r, string col)
         => r[col] == DBNull.Value ? 0m : Convert.ToDecimal(r[col]);

        static int GetInt(DataRow r, string col)
         => r[col] == DBNull.Value ? 0 : Convert.ToInt32(r[col]);

        static string GetString(DataRow r, string col)
         => r[col] == DBNull.Value ? "" : Convert.ToString(r[col]);


        private static List<SalaryComponentResult> MapComponents(DataTable dtComponents,decimal paidDays,int totalDays)
        {
            var components = new List<SalaryComponentResult>();

            if (dtComponents == null || dtComponents.Rows.Count == 0)
                return components;

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
                    PayType = row["PayType"]?.ToString(),
                    LogicType = row["LogicType"]?.ToString(),

                    /* =========================
                       Calculation
                    ========================= */
                    Amount = row["MonthlyAmount"] != DBNull.Value
                                ? Convert.ToDecimal(row["MonthlyAmount"])
                                : 0m,

                    BaseAmount = 0m,

                    Rate = row["ManualRate"] != DBNull.Value
                                ? Convert.ToDecimal(row["ManualRate"])
                                : 0m,

                    /* =========================
                       Flags
                    ========================= */
                    IsPercentage = row.Table.Columns.Contains("ISPercentage")
                                && row["ISPercentage"] != DBNull.Value && Convert.ToDecimal(row["ISPercentage"]) > 0
                                && Convert.ToBoolean(row["ISPercentage"]),

                    IsStatutory = row.Table.Columns.Contains("IsStatutory")
                                && row["IsStatutory"] != DBNull.Value && Convert.ToDecimal(row["IsStatutory"]) > 0
                                && Convert.ToBoolean(row["IsStatutory"]),

                    IsBasicComponent = row.Table.Columns.Contains("IsBasicComponent")
                                && row["IsBasicComponent"] != DBNull.Value && Convert.ToDecimal(row["IsBasicComponent"]) > 0
                                && Convert.ToBoolean(row["IsBasicComponent"]),

                    IsGrossComponent = row.Table.Columns.Contains("IsGrossComponent")
                                && row["IsGrossComponent"] != DBNull.Value && Convert.ToDecimal(row["IsGrossComponent"]) > 0
                                && Convert.ToBoolean(row["IsGrossComponent"]),

                    StatutoryType = row.Table.Columns.Contains("StatutoryType")
                                ? row["StatutoryType"]?.ToString()
                                : null,

                    PTaxSlabsJson = row.Table.Columns.Contains("PTaxSlabsJson")
                                ? row["PTaxSlabsJson"]?.ToString()
                                : null,

                    IsAllowance = row.Table.Columns.Contains("IsAllowance")
                                && row["IsAllowance"] != DBNull.Value && Convert.ToDecimal(row["IsAllowance"]) > 0
                                && Convert.ToBoolean(row["IsAllowance"]),

                    AllowanceType = row.Table.Columns.Contains("AllowanceType")
                                ? row["AllowanceType"]?.ToString()
                                : null,

                    IsOther = row.Table.Columns.Contains("IsOther")
                                && row["IsOther"] != DBNull.Value && Convert.ToDecimal(row["IsOther"]) > 0
                                && Convert.ToBoolean(row["IsOther"]),

                    OtherType = row.Table.Columns.Contains("OtherType")
                                ? row["OtherType"]?.ToString()
                                : null,

                    /* =========================
                       Attendance Context
                    ========================= */
                    PaidDays = paidDays,
                    TotalDays = totalDays,

                    /* =========================
                       Limits & Rounding
                    ========================= */
                    MaxLimit = row.Table.Columns.Contains("MaxLimit") && row["MaxLimit"] != DBNull.Value && Convert.ToDecimal(row["MaxLimit"]) > 0
                                ? Convert.ToDecimal(row["MaxLimit"])
                                : (decimal?)null,

                    RoundingType = row.Table.Columns.Contains("RoundingType")
                                ? row["RoundingType"]?.ToString()
                                : null,

                    /* =========================
                       Audit / Debug
                    ========================= */
                    CalculationExpression = row.Table.Columns.Contains("CalculationFormula")
                                ? row["CalculationFormula"]?.ToString()
                                : null
                });
            }

            return components;
        }
        private long SP_Payroll_SaveBatch_Preview( string TenantID, string RefNo, DateTime RefDate, string PayrollType,
                int? ProcessMonth,
                int? ProcessYear,
                DateTime? FromDate,
                DateTime? ToDate,
                int TotalEmployees,
                decimal TotalCost,
                decimal TotalNetPay,
                string Comments,
                string CreatedBy
        )
        {
             

            DataTable dt = clsDatabase.fnDataTable(
                "SP_Payroll_SaveBatch_Preview",
                TenantID,
                RefNo, 
                PayrollType,
                ProcessMonth,
                ProcessYear,
                FromDate,
                ToDate,
                TotalEmployees,
                TotalCost,
                TotalNetPay,
                Comments,
                CreatedBy
            );

            if (dt == null || dt.Rows.Count == 0 || !dt.Columns.Contains("PreviewBatchID"))
                throw new Exception("SP_Payroll_SaveBatch_Preview failed.");

            return Convert.ToInt64(dt.Rows[0]["PreviewBatchID"]);
        }

        private long SP_Payroll_SaveEmployee_Preview(long PreviewBatchID,int PayGroupID, int EmployeeID, decimal TotalPaidDays, decimal TotalEarnings, 
                decimal TotalDeductions,
                decimal MonthlyCTC,
                decimal NetPay,
                string CreatedBy
        )
        {


            DataTable dt = clsDatabase.fnDataTable( "SP_Payroll_SaveEmployee_Preview",
                PreviewBatchID,
                EmployeeID,
                PayGroupID,
                TotalPaidDays,
                TotalEarnings,
                TotalDeductions,
                MonthlyCTC,
                NetPay,
                CreatedBy 
            );

            if (dt == null || dt.Rows.Count == 0 || !dt.Columns.Contains("PreviewEmployeeID"))
                throw new Exception("SP_Payroll_SaveEmployee_Preview failed.");

            return Convert.ToInt64(dt.Rows[0]["PreviewEmployeeID"]);
        }
        private void SP_Payroll_UpdateBatchTotals_Preview(
            long PreviewBatchID,
            decimal TotalCost,
            decimal TotalNetPay
        )
        {
            clsDatabase.fnDBOperation(
                "SP_Payroll_UpdateBatchTotals_Preview",
                PreviewBatchID,
                TotalCost,
                TotalNetPay
            );
        }
        
        [HttpPost]
        public JsonResult SubmitPreviewEdits(SubmitPreviewEditsRequest request)
        {
            if (request == null || request.PreviewBatchId <= 0)
                return Json(new { success = false, message = "Invalid preview batch" });

            try
            {
                string userName = Session["UserName"]?.ToString() ?? "System";

                /* --- CASE 1: NO CHANGES --- */
                if (request.Edits == null || !request.Edits.Any())
                {
                    clsDatabase.fnDBOperation("SP_Payroll_MarkPreviewBatchReviewed", request.PreviewBatchId, userName);
                    return Json(new { success = true, message = "No changes detected. Batch marked as reviewed." });
                }

                /* --- CASE 2: CHANGES (SKIP LOAN / MANUAL ADJ) --- */
                // When UI skips a loan, the ReferenceIds (e.g., "101") are passed in the JSON
                string editsJson = JsonConvert.SerializeObject(request.Edits);

                // 1. Save edits to the audit/delta table (Payroll_SaaS_Salary_Process_Preview_Edit)
                // Ensure this SP accepts the new ReferenceIds column in its OPENJSON part
                clsDatabase.fnDBOperation("SP_Payroll_SavePreviewEdits", request.PreviewBatchId, editsJson, userName);

                // 2. Apply changes to the Preview Detail & Employee tables
                // This triggers your recalculation CTE we wrote earlier
                clsDatabase.fnDBOperation("SP_Payroll_ApplyPreviewEdits", request.PreviewBatchId);

                return Json(new { success = true, message = "Changes applied successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        
        [HttpPost]
        public JsonResult GetEmployeeSalaryForEdit(EditSalaryRequest req)
        {
            if (req == null || req.EmployeeID <= 0)
                return Json(new { Success = false, Message = "Invalid request" });

            /* ==========================================================
                1) LOAD PREVIEW CONTEXT (ALWAYS)
            ========================================================== */
            DataTable dtCtx = clsDatabase.fnDataTable(
                "SP_Payroll_GetEmployeePreviewContext", TenantId, req.PreviewBatchID, req.PayrollProcessEmployeeID,  req.EmployeeID,
                req.Status
            );

            if (dtCtx.Rows.Count == 0)
                return Json(new { Success = false, Message = "Preview record not found" });

            DataRow ctx = dtCtx.Rows[0];

            string payrollType = ctx["PayrollType"].ToString();
            int processMonth = Convert.ToInt32(ctx["ProcessMonth"]);
            int processYear = Convert.ToInt32(ctx["ProcessYear"]);
            string employeename = ctx["EmployeeName"].ToString();
            string empcode = ctx["empcode"].ToString();
            string empno = ctx["empno"].ToString();
            DateTime? fromDate = ctx["FromDate"] == DBNull.Value ? null : (DateTime?)ctx["FromDate"];
            DateTime? toDate = ctx["ToDate"] == DBNull.Value ? null : (DateTime?)ctx["ToDate"];

            int totalDays = payrollType == "1"
                ? DateTime.DaysInMonth(processYear, processMonth)
                : (toDate.Value - fromDate.Value).Days + 1;

            decimal paidDays = req.IsDaysChanged ? req.PaidDays.Value :
                              (ctx["TotalPaidDays"] == DBNull.Value ? totalDays : Convert.ToDecimal(ctx["TotalPaidDays"]));

            decimal manualAdd = ctx["ManualAddition"] == DBNull.Value ? 0 : Convert.ToDecimal(ctx["ManualAddition"]);
            decimal manualDed = ctx["ManualDeduction"] == DBNull.Value ? 0 : Convert.ToDecimal(ctx["ManualDeduction"]);
            long previewEmployeeId = Convert.ToInt64(ctx["PreviewEmployeeID"]);
            bool loanWaived = ctx["LoanWaivedYN"] != DBNull.Value && Convert.ToBoolean(ctx["LoanWaivedYN"]);
            /* ==========================================================
                    1.1) CHECK FOR EXTERNAL LOANS
                ========================================================== */
            bool isLoanExternal = false;
            DataTable dtExternalCheck = clsDatabase.fnDataTable("PRC_CheckLoanPaidForMonth", req.EmployeeID, processMonth, processYear, TenantId);
            if (dtExternalCheck.Rows.Count > 0)
            {
                // If AmountPaid > 0, it means an external payment exists for this month
                if (Convert.ToDecimal(dtExternalCheck.Rows[0]["AmountPaid"]) > 0)
                {
                    isLoanExternal = true;
                }
            }
            /* ==========================================================
                2) FIRST LOAD → RETURN STORED PREVIEW DATA
            ========================================================== */
            if (!req.IsDaysChanged)
            {
                DataTable dtPrev = clsDatabase.fnDataTable("SP_Payroll_GetEmployeePreviewDetails", TenantId, previewEmployeeId, req.EmployeeID,req.Status);
                

                var finalDetails = dtPrev.AsEnumerable().Select(r => {
                    int payConfigId = Convert.ToInt32(r["PayConfigID"]);
                    decimal currentAmount = Convert.ToDecimal(r["PayValue"]);
                    string otherType = r["OtherType"]?.ToString() ?? "";
                    string refIds = r["ReferenceIds"]?.ToString() ?? "";
                    decimal originalAmount = currentAmount;

                    if (otherType == "LOANRECOVERY" && !string.IsNullOrEmpty(refIds) && refIds != "0")
                    {
                        DataTable dtLoanMaster = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeActiveLoanTotal", TenantId, req.EmployeeID, refIds);
                        if (dtLoanMaster.Rows.Count > 0)
                            originalAmount = Convert.ToDecimal(dtLoanMaster.Rows[0]["TotalEMI"]);
                    }

                    return new
                    {
                        PayConfigId = payConfigId,
                        PayConfigName = r["PayConfigName"].ToString(),
                        PayType = r["PayType"].ToString(),
                        Amount = currentAmount,
                        OriginalAmount = originalAmount,
                        ReferenceIds = refIds,
                        IsManualAllowed = Convert.ToInt32(r["IsManualAllowed"]),
                        IsAutoCalculated = Convert.ToInt32(r["IsAutoCalculated"]),
                        OtherType = otherType,
                        LogicType = r["LogicType"].ToString(),
                        StatutoryType = r.Table.Columns.Contains("StatutoryType") ? r["StatutoryType"]?.ToString() : ""
                    };
                }).ToList();

                return Json(new { Success = true, Mode = "PREVIEW_LOAD", Header = new { EmployeeName = employeename, Empno = empno, EmpCode = empcode, PaidDays = paidDays, TotalMonthDays = totalDays, ManualAddition = manualAdd, ManualDeduction = manualDed, LoanWaivedYN = loanWaived, TotalGross = ctx["TotalGross"], TotalDeduction = ctx["TotalDeduction"], TotalTakeHome = ctx["TotalTakeHome"], IsLoanExternal = isLoanExternal }, Details = finalDetails }, JsonRequestBehavior.AllowGet);
            }

            /* ==========================================================
                3) DAYS CHANGED → RECALC & RESPECT PREVIOUS WAIVER
            ========================================================== */
            DataTable dtHeader = clsDatabase.fnDataTable("Payroll_SaaS_GetSalaryHeader", TenantId, req.EmployeeID);
            if (dtHeader.Rows.Count == 0) return Json(new { Success = false, Message = "Salary master not configured" });

            DataRow h = dtHeader.Rows[0];
            int empSalaryConfigId = Convert.ToInt32(h["EmpSalaryConfigID"]);
            bool isPF = Convert.ToBoolean(h["IsPF_Applicable"]);
            bool isESIC = Convert.ToBoolean(h["IsESIC_Applicable"]);
            bool isPTax = Convert.ToBoolean(h["IsPTax_Applicable"]);

            DataTable dtComp = clsDatabase.fnDataTable("Payroll_SaaS_GetSalaryDetails", empSalaryConfigId);
            var components = MapComponents(dtComp, paidDays, totalDays);
            var salary = CalculateMonthlySalary(components, req.EmployeeID, processMonth, processYear, isPF, isESIC, isPTax, PayrollValueResolver);

            if (salary == null) return Json(new { Success = false, Message = "Salary calculation failed" });

            // Fetch existing preview to check memory of "Waived" status
            DataTable dtExisting = clsDatabase.fnDataTable("SP_Payroll_GetEmployeePreviewDetails", TenantId, previewEmployeeId, req.EmployeeID);

            decimal totalDeductionOverride = salary.TotalDeductions;
            decimal totalNetOverride = salary.NetPay;
            bool currentWaiverState = false;

            var patchedDetails = salary.Components.Select(c => {
                var existingRow = dtExisting.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["PayConfigId"]) == c.PayConfigId);
                string refIds = existingRow != null ? existingRow["ReferenceIds"]?.ToString() : "";
                //bool wasWaived = existingRow != null && existingRow["IsWaived"] != DBNull.Value && Convert.ToBoolean(existingRow["IsWaived"]);

                decimal amount = c.Amount;
                decimal masterEMI = c.Amount;

                if (c.OtherType == "LOANRECOVERY" && !string.IsNullOrEmpty(refIds) && refIds != "0")
                {
                    DataTable dtLoanMaster = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeActiveLoanTotal", TenantId, req.EmployeeID, refIds);
                    if (dtLoanMaster.Rows.Count > 0)
                        masterEMI = Convert.ToDecimal(dtLoanMaster.Rows[0]["TotalEMI"]);

                    // 🔑 THE FIX: If it was waived, keep it waived (0.00)
                    if (loanWaived)
                    {
                        // Subtract the engine's calculated loan amount from deductions and add to net
                        totalDeductionOverride -= amount;
                        totalNetOverride += amount;
                        amount = 0;
                        currentWaiverState = true;
                    }
                }

                return new
                {
                    c.PayConfigId,
                    c.PayConfigName,
                    c.PayType,
                    Amount = amount,
                    OriginalAmount = masterEMI,
                    ReferenceIds = refIds,
                    c.OtherType,
                    IsManualAllowed = existingRow != null ? Convert.ToInt32(existingRow["IsManualAllowed"]) : 0,
                    IsAutoCalculated = existingRow != null ? Convert.ToInt32(existingRow["IsAutoCalculated"]) : 1,
                    LogicType = existingRow != null ? existingRow["LogicType"].ToString() : "",
                    c.StatutoryType
                };
            }).ToList();

            return Json(new
            {
                Success = true,
                Mode = "DAYS_RECALC",
                Header = new
                {
                    EmployeeName = employeename,
                    Empno = empno,
                    EmpCode = empcode,
                    PaidDays = paidDays,
                    TotalMonthDays = totalDays,
                    ManualAddition = manualAdd,
                    ManualDeduction = manualDed,
                    TotalGross = salary.TotalEarnings + manualAdd,
                    TotalDeduction = totalDeductionOverride + manualDed,
                    TotalTakeHome = totalNetOverride + manualAdd - manualDed,
                    TotalContribution = salary.MonthlyCTC - salary.TotalEarnings,
                    MonthlyCTC = salary.MonthlyCTC + manualAdd,
                    LoanWaivedYN = currentWaiverState, // 💰 Preserve the checkbox state
                    IsLoanExternal = isLoanExternal
                },
                Details = patchedDetails
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RecalculateEmployeeSalaryForEdit(EditSalaryRecalculateRequest req)
        {
            if (req == null || req.EmployeeId <= 0 || req.PreviewBatchId <= 0)
                return Json(new { Success = false, Message = "Invalid request" });

            try
            {
                int employeeId = Convert.ToInt32(req.EmployeeId);
                DataTable dtCtx = clsDatabase.fnDataTable("SP_Payroll_GetEmployeePreviewContext",TenantId,req.PreviewBatchId,req.PayrollProcessEmployeeID,employeeId,req.Status);

                if (dtCtx.Rows.Count == 0)
                    return Json(new { Success = false, Message = "Preview record not found" });

                DataRow ctx = dtCtx.Rows[0];
                string employeename = ctx["EmployeeName"].ToString();
                string empcode = ctx["empcode"].ToString();
                string empno = ctx["empno"].ToString();
                int processMonth = Convert.ToInt32(ctx["ProcessMonth"]);
                int processYear = Convert.ToInt32(ctx["ProcessYear"]);
                string payrollType = ctx["PayrollType"].ToString();
                DateTime? fromDate = ctx["FromDate"] == DBNull.Value ? null : (DateTime?)ctx["FromDate"];
                DateTime? toDate = ctx["ToDate"] == DBNull.Value ? null : (DateTime?)ctx["ToDate"];

                int totalDays = payrollType == "1"
                    ? DateTime.DaysInMonth(processYear, processMonth)
                    : (toDate.Value - fromDate.Value).Days + 1;

                decimal paidDays = req.PaidDays > 0
                    ? req.PaidDays
                    : (ctx["TotalPaidDays"] == DBNull.Value ? totalDays : Convert.ToDecimal(ctx["TotalPaidDays"]));

                decimal manualAdd = req.ManualAddition;
                decimal manualDed = req.ManualDeduction;
                long previewEmployeeId = Convert.ToInt64(ctx["PreviewEmployeeID"]);

                bool isLoanExternal = false;
                DataTable dtExternalCheck = clsDatabase.fnDataTable("PRC_CheckLoanPaidForMonth", employeeId, processMonth, processYear, TenantId);
                if (dtExternalCheck.Rows.Count > 0 && Convert.ToDecimal(dtExternalCheck.Rows[0]["AmountPaid"]) > 0)
                {
                    isLoanExternal = true;
                }

                DataTable dtPrev = clsDatabase.fnDataTable("SP_Payroll_GetEmployeePreviewDetails", TenantId, previewEmployeeId, employeeId, req.Status);
                var postedComponents = (req.Components ?? new List<ComponentUpdateDto>())
                    .GroupBy(x => x.PayConfigId)
                    .ToDictionary(g => g.Key, g => g.Last());

                DataTable dtHeader = clsDatabase.fnDataTable("Payroll_SaaS_GetSalaryHeader", TenantId, employeeId);
                if (dtHeader.Rows.Count == 0)
                    return Json(new { Success = false, Message = "Salary master not configured" });

                DataRow h = dtHeader.Rows[0];
                int empSalaryConfigId = Convert.ToInt32(h["EmpSalaryConfigID"]);
                int payGroupId = Convert.ToInt32(h["PayGroupID"]);
                bool isPF = Convert.ToBoolean(h["IsPF_Applicable"]);
                bool isESIC = Convert.ToBoolean(h["IsESIC_Applicable"]);
                bool isPTax = Convert.ToBoolean(h["IsPTax_Applicable"]);

                var salaryComponentValues = dtPrev.AsEnumerable()
                    .Select(r =>
                    {
                        int payConfigId = Convert.ToInt32(r["PayConfigID"]);
                        decimal amount = postedComponents.TryGetValue(payConfigId, out ComponentUpdateDto posted)
                            ? posted.Amount
                            : Convert.ToDecimal(r["PayValue"]);

                        return new
                        {
                            Row = r,
                            PayConfigId = payConfigId,
                            Amount = amount
                        };
                    })
                    .Where(x => IsSalaryMasterEditableComponent(x.Row))
                    .Select(x => new SalaryDetailsAllowanceValue
                    {
                        AllowanceId = x.PayConfigId,
                        Value = x.Amount
                    })
                    .ToList();

                if (salaryComponentValues.Any())
                {
                    SaveSalaryDetailsComponentsForEdit(new SalaryDetailsComponentSaveRequest
                    {
                        EmployeeID = employeeId,
                        PayGroupID = payGroupId,
                        EmpSalaryConfigID = empSalaryConfigId,
                        EffectiveFrom = new DateTime(processYear, processMonth, 1),
                        IsPF = isPF,
                        IsESIC = isESIC,
                        IsPTAX = isPTax,
                        Allowances = salaryComponentValues
                    });

                    dtHeader = clsDatabase.fnDataTable("Payroll_SaaS_GetSalaryHeader", TenantId, employeeId);
                    h = dtHeader.Rows[0];
                    empSalaryConfigId = Convert.ToInt32(h["EmpSalaryConfigID"]);
                    isPF = Convert.ToBoolean(h["IsPF_Applicable"]);
                    isESIC = Convert.ToBoolean(h["IsESIC_Applicable"]);
                    isPTax = Convert.ToBoolean(h["IsPTax_Applicable"]);
                }

                DataTable dtComp = clsDatabase.fnDataTable("Payroll_SaaS_GetSalaryDetails", empSalaryConfigId);
                var recalculatedComponents = MapComponents(dtComp, paidDays, totalDays);
                var recalculatedSalary = CalculateMonthlySalary(
                    recalculatedComponents,
                    employeeId,
                    processMonth,
                    processYear,
                    isPF,
                    isESIC,
                    isPTax,
                    PayrollValueResolver
                );

                if (recalculatedSalary != null)
                {
                    decimal totalDeductionOverride = recalculatedSalary.TotalDeductions;
                    decimal totalNetOverride = recalculatedSalary.NetPay;
                    bool recalculatedWaiverState = false;

                    var recalculatedDetails = recalculatedSalary.Components.Select(c =>
                    {
                        var existingRow = dtPrev.AsEnumerable()
                            .FirstOrDefault(r => Convert.ToInt32(r["PayConfigID"]) == c.PayConfigId);

                        string refIds = existingRow != null ? existingRow["ReferenceIds"]?.ToString() : "";
                        if (postedComponents.TryGetValue(c.PayConfigId, out ComponentUpdateDto posted)
                            && !string.IsNullOrWhiteSpace(posted.ReferenceIds))
                        {
                            refIds = posted.ReferenceIds;
                        }

                        decimal amount = c.Amount;
                        decimal originalAmount = c.Amount;

                        if (string.Equals(c.OtherType, "LOANRECOVERY", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(refIds) && refIds != "0")
                        {
                            DataTable dtLoanMaster = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeActiveLoanTotal", TenantId, employeeId, refIds);
                            if (dtLoanMaster.Rows.Count > 0)
                                originalAmount = Convert.ToDecimal(dtLoanMaster.Rows[0]["TotalEMI"]);

                            if (req.LoanWaivedYN)
                            {
                                totalDeductionOverride -= amount;
                                totalNetOverride += amount;
                                amount = 0;
                                recalculatedWaiverState = true;
                            }
                        }

                        return new
                        {
                            c.PayConfigId,
                            c.PayConfigName,
                            c.PayType,
                            Amount = amount,
                            OriginalAmount = originalAmount,
                            ReferenceIds = refIds,
                            c.OtherType,
                            IsManualAllowed = existingRow != null ? Convert.ToInt32(existingRow["IsManualAllowed"]) : 0,
                            IsAutoCalculated = existingRow != null ? Convert.ToInt32(existingRow["IsAutoCalculated"]) : 1,
                            c.LogicType,
                            c.StatutoryType
                        };
                    }).ToList();

                    return Json(new
                    {
                        Success = true,
                        Mode = "MODAL_RECALC",
                        Header = new
                        {
                            EmployeeName = employeename,
                            Empno = empno,
                            EmpCode = empcode,
                            PaidDays = paidDays,
                            TotalMonthDays = totalDays,
                            ManualAddition = manualAdd,
                            ManualDeduction = manualDed,
                            LoanWaivedYN = recalculatedWaiverState,
                            TotalGross = recalculatedSalary.TotalEarnings + manualAdd,
                            TotalDeduction = totalDeductionOverride + manualDed,
                            TotalTakeHome = totalNetOverride + manualAdd - manualDed,
                            TotalContribution = recalculatedSalary.MonthlyCTC - recalculatedSalary.TotalEarnings,
                            MonthlyCTC = recalculatedSalary.MonthlyCTC + manualAdd,
                            IsLoanExternal = isLoanExternal
                        },
                        Details = recalculatedDetails
                    }, JsonRequestBehavior.AllowGet);
                }

                decimal totalGross = 0;
                decimal totalDeduction = 0;
                decimal totalContribution = 0;
                decimal? grossOverride = null;
                bool currentWaiverState = false;

                var finalDetails = dtPrev.AsEnumerable().Select(r =>
                {
                    int payConfigId = Convert.ToInt32(r["PayConfigID"]);
                    string payType = r["PayType"].ToString();
                    string payTypeKey = payType.Trim().ToUpperInvariant();
                    string payConfigName = r["PayConfigName"].ToString();
                    bool isGrossAmount = string.Equals(payConfigName.Trim(), "GROSS AMOUNT", StringComparison.OrdinalIgnoreCase);
                    string otherType = r["OtherType"]?.ToString() ?? "";
                    string refIds = r["ReferenceIds"]?.ToString() ?? "";
                    decimal currentAmount = Convert.ToDecimal(r["PayValue"]);
                    decimal storedAmount = currentAmount;

                    if (postedComponents.TryGetValue(payConfigId, out ComponentUpdateDto posted))
                    {
                        currentAmount = posted.Amount;
                        if (!string.IsNullOrWhiteSpace(posted.ReferenceIds))
                            refIds = posted.ReferenceIds;

                        if (isGrossAmount && posted.Amount != storedAmount)
                            grossOverride = posted.Amount;
                    }

                    decimal originalAmount = currentAmount;
                    bool isLoan = string.Equals(otherType, "LOANRECOVERY", StringComparison.OrdinalIgnoreCase);

                    if (isLoan && !string.IsNullOrEmpty(refIds) && refIds != "0")
                    {
                        DataTable dtLoanMaster = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeActiveLoanTotal", TenantId, employeeId, refIds);
                        if (dtLoanMaster.Rows.Count > 0)
                            originalAmount = Convert.ToDecimal(dtLoanMaster.Rows[0]["TotalEMI"]);

                        if (req.LoanWaivedYN)
                        {
                            currentAmount = 0;
                            currentWaiverState = true;
                        }
                        else if (currentAmount == 0 && originalAmount > 0)
                        {
                            currentAmount = originalAmount;
                        }
                    }

                    if (!isGrossAmount)
                    {
                        if (payTypeKey == "ALLOWANCES")
                            totalGross += currentAmount;
                        else if (payTypeKey == "MANUAL")
                            totalGross += currentAmount;
                        else if (payTypeKey == "DEDUCTION")
                            totalDeduction += currentAmount;
                        else if (payTypeKey == "CC")
                            totalContribution += currentAmount;
                    }

                    return new
                    {
                        PayConfigId = payConfigId,
                        PayConfigName = payConfigName,
                        PayType = payType,
                        Amount = currentAmount,
                        OriginalAmount = originalAmount,
                        ReferenceIds = refIds,
                        IsManualAllowed = Convert.ToInt32(r["IsManualAllowed"]),
                        IsAutoCalculated = Convert.ToInt32(r["IsAutoCalculated"]),
                        OtherType = otherType,
                        LogicType = r["LogicType"].ToString(),
                        StatutoryType = r.Table.Columns.Contains("StatutoryType") ? r["StatutoryType"]?.ToString() : ""
                    };
                }).ToList();

                decimal finalGross = (grossOverride ?? totalGross) + manualAdd;
                decimal finalDeduction = totalDeduction + manualDed;
                decimal totalTakeHome = finalGross - finalDeduction;
                decimal monthlyCtc = finalGross + totalContribution;

                return Json(new
                {
                    Success = true,
                    Mode = "MODAL_RECALC",
                    Header = new
                    {
                        EmployeeName = employeename,
                        Empno = empno,
                        EmpCode = empcode,
                        PaidDays = paidDays,
                        TotalMonthDays = totalDays,
                        ManualAddition = manualAdd,
                        ManualDeduction = manualDed,
                        LoanWaivedYN = currentWaiverState,
                        TotalGross = finalGross,
                        TotalDeduction = finalDeduction,
                        TotalTakeHome = totalTakeHome,
                        TotalContribution = totalContribution,
                        MonthlyCTC = monthlyCtc,
                        IsLoanExternal = isLoanExternal
                    },
                    Details = finalDetails
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }

        private static bool IsSalaryMasterEditableComponent(DataRow row)
        {
            string payType = row["PayType"]?.ToString().Trim().ToUpperInvariant() ?? "";
            string logicType = row["LogicType"]?.ToString().Trim().ToUpperInvariant() ?? "";
            string statutoryType = row.Table.Columns.Contains("StatutoryType")
                ? row["StatutoryType"]?.ToString().Trim().ToUpperInvariant()
                : "";

            bool isSupportedPayType =
                payType == "MANUAL" ||
                payType == "ALLOWANCES" ||
                payType == "DEDUCTION";

            return isSupportedPayType
                && logicType == "FIXED"
                && statutoryType != "PTAX";
        }

        private void SaveSalaryDetailsComponentsForEdit(SalaryDetailsComponentSaveRequest model)
        {
            var employeeCtcController = new EmployeeCTCController
            {
                ControllerContext = ControllerContext
            };

            JsonResult result = employeeCtcController.SaveSalaryDetailsComponents(model);
            object data = result.Data;

            if (data == null)
                throw new Exception("Salary details save failed.");

            var dataType = data.GetType();
            var successProperty = dataType.GetProperty("Success");
            var messageProperty = dataType.GetProperty("Message");

            bool success = successProperty != null && Convert.ToBoolean(successProperty.GetValue(data, null));
            string message = messageProperty != null
                ? Convert.ToString(messageProperty.GetValue(data, null))
                : "Salary details save failed.";

            if (!success)
                throw new Exception(message);
        }
        [HttpGet]
        public JsonResult GetEmployeeLoanLedger(int employeeId, int payConfigId, decimal Currentemi, int ProcessMonth, int ProcessYear, string ReferenceIds)
        {
            try
            {
                // 1. Resolve which Loan ID to use from the ReferenceIds string
                // If multiple IDs exist (e.g. "101,102"), we take the first one to show the ledger
                string targetLoanId = (ReferenceIds ?? "").Split(',')[0];

                // 1. FALLBACK LOGIC: If ReferenceIds is "0" or empty, fetch from processed details
                if (string.IsNullOrEmpty(targetLoanId) || targetLoanId == "0")
                {
                     
                    object dbRef = clsDatabase.fnDBOperation("SP_get_Loan_ReferenceIds", employeeId,payConfigId,Currentemi,ProcessMonth,ProcessYear,TenantId);

                    targetLoanId = (dbRef ?? "").ToString().Split(',')[0];
                }

                if (string.IsNullOrEmpty(targetLoanId) || targetLoanId == "0")
                {
                    return Json(new { Success = false, Message = "No active loan linked to this record." }, JsonRequestBehavior.AllowGet);
                }

                // 2. Fetch Full Schedule directly using the Loan ID provided
                DataTable dtSchedule = clsDatabase.fnDataTable("SP_GetLoanSchedule", targetLoanId);

                if (dtSchedule == null || dtSchedule.Rows.Count == 0)
                    return Json(new { Success = false, Message = "Loan schedule not found." }, JsonRequestBehavior.AllowGet);

                var allScheduleRows = dtSchedule.AsEnumerable().Select(r => new
                {
                    EmiNo = Convert.ToInt32(r["EMI_No"]),
                    MonthNo = Convert.ToInt32(r["MonthNo"]),
                    YearNo = Convert.ToInt32(r["YearNo"]),
                    Period = $"{r["MonthName"]} {r["YearNo"]}",
                    Opening = Convert.ToDecimal(r["BalanceBefore"]),
                    Principal = Convert.ToDecimal(r["PrincipalDue"]),
                    Paid = r["AmountPaid"] == DBNull.Value ? 0 : Convert.ToDecimal(r["AmountPaid"]),
                    Closing = Convert.ToDecimal(r["BalanceAfter"]),
                    Status = r["Status"].ToString()
                }).ToList();

                //// 3. Find the Current Month Index in the schedule
                //int currentIndex = allScheduleRows.FindIndex(r => r.MonthNo == ProcessMonth && r.YearNo == ProcessYear);
                // 1. Calculate Previous Month and Year
                int prevMonth = (ProcessMonth == 1) ? 12 : ProcessMonth - 1;
                int prevYear = (ProcessMonth == 1) ? ProcessYear - 1 : ProcessYear;

                // 2. Find the Previous Month Index in the schedule
                int currentIndex = allScheduleRows.FindIndex(r => r.MonthNo == prevMonth && r.YearNo == prevYear);
                // If not found in schedule, fallback to the latest record
                if (currentIndex == -1) currentIndex = allScheduleRows.Count - 1;

                // 4. Slice Data: Current Month + 2 Previous Months (max 3 rows)
                int startFrom = Math.Max(0, currentIndex - 2);
                int takeCount = currentIndex - startFrom + 1;

                var filteredSchedule = allScheduleRows
                    .Skip(startFrom)
                    .Take(takeCount)
                    .OrderByDescending(x => x.EmiNo)
                    .Select(s => new {
                        s.EmiNo,
                        s.Period,
                        s.Opening,
                        s.Principal,
                        s.Closing,
                        s.Status,
                        IsCurrent = (s.MonthNo == ProcessMonth && s.YearNo == ProcessYear)
                    }).ToList();

                // 5. Return Result
                var currentEntry = allScheduleRows[currentIndex];
                return Json(new
                {
                    Success = true,
                    OpeningBalance = currentEntry.Opening,
                    CurrentEMI = currentEntry.Principal,
                    ClosingBalance = currentEntry.Closing,
                    Schedule = filteredSchedule
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SubmitFullPreviewUpdate(FullUpdateDto req)
        {
            try
            {
                string UpdatedBy = Session["UserName"].ToString();
                // Convert components list to JSON for SQL processing
                string jsonDetails = JsonConvert.SerializeObject(req.Components);

                string result = clsDatabase.fnDBOperation("SP_Payroll_SaveFullPreviewRecalc",
                    TenantId,
                    req.PreviewBatchId,
                    req.EmployeeId,
                    req.PaidDays,
                    req.ManualAddition,
                    req.ManualDeduction,
                    jsonDetails,
                    UpdatedBy
                );

                bool isSuccess = (result != null && (result.ToLower() == "success" || result == "1"));

                return Json(new
                {
                    success = isSuccess,
                    message = isSuccess ? "Salary Preview updated successfully." : result
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeletePreviewBatch(long previewBatchId)
        {
            try
            {
                // One single call to the SP
                clsDatabase.fnDBOperation("SP_Payroll_SaaS_DeleteBatchDraft", previewBatchId, TenantId);

                return Json(new { success = true, message = "Draft batch deleted successfully." });
            }
            catch (Exception ex)
            {
                // This will capture the "Cannot delete... already finalized" message from SQL
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public void DownloadVariableTemplate(int payGroupId, int ConfigureSalaryComponentId, int month, int year)
        {
            // Convert month number to Month Name (e.g., 1 -> January)
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            // 1. Fetch Template Data using the new SP
            System.Data.DataSet ds = clsDatabase.fnDataSet("SP_Payroll_GetTemplateData", TenantId, payGroupId, ConfigureSalaryComponentId);

            if (ds == null || ds.Tables.Count < 2 || ds.Tables[1].Rows.Count == 0)
            {
                // Handle error: No employees or component not found
                return;
            }

            string componentName = ds.Tables[0].Rows[0]["PayConfigName"].ToString();
            DataTable dtEmployees = ds.Tables[1];
            // Professional Labeling Logic
            string valueHeader = "Value";
            if (componentName.ToUpper().Contains("CONTINUOUS ATTENDANCE"))
                valueHeader = "Day Off";
            else if (componentName.ToUpper().Contains("OVERTIME"))
                valueHeader = "Overtime Hours";
            else if (componentName.ToUpper().Contains("PRODUCTION BONUS"))
                valueHeader = "Target Achieved %";
            else if (componentName.ToUpper().Contains("SPECIAL ALLOWANCE"))
                valueHeader = "Special Amount";
            // Set the license context to NonCommercial (REQUIRED for free usage)
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
             
            using (ExcelPackage package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("UploadSheet");

                // --- Define Headers ---
                ws.Cells[1, 1].Value = "EmployeeID";    // Hidden Column
                ws.Cells[1, 2].Value = "Employee Code";
                ws.Cells[1, 3].Value = "Employee Name";
                ws.Cells[1, 4].Value = "Month";
                ws.Cells[1, 5].Value = "Year";
                ws.Cells[1, 6].Value = "Value (" + valueHeader + ")"; // Your {VAL}

                // --- Styling (Applied to all 6 columns) ---
                using (var range = ws.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(14, 119, 119));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                // --- Fill Data ---
                int currentRow = 2;
                foreach (DataRow dr in dtEmployees.Rows)
                {
                    ws.Cells[currentRow, 1].Value = dr["EmployeeID"];
                    ws.Cells[currentRow, 2].Value = dr["EmployeeCode"];
                    ws.Cells[currentRow, 3].Value = dr["EmployeeName"];
                    ws.Cells[currentRow, 4].Value = monthName; // Passing Month Name
                    ws.Cells[currentRow, 5].Value = year;      // Passing Year
                    ws.Cells[currentRow, 6].Value = 0;         // Default Input Value
                    currentRow++;
                }

                // --- Formatting ---
                ws.Column(1).Hidden = true; // Protect EmployeeID for easier backend mapping
                ws.Column(2).AutoFit();
                ws.Column(3).AutoFit();
                ws.Column(4).AutoFit();
                ws.Column(5).AutoFit();
                ws.Column(6).Width = 25;

                // --- Response ---
                string fileName = $"{componentName.Replace(" ", "_")}_Template_{monthName}_{year}.xlsx";
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }
        }
        [HttpPost]
        public JsonResult UploadVariableComponentValues(HttpPostedFileBase file, int configId, int month, int year,int paygroupid, bool forceUpdate = false)
        {
            if (file == null || file.ContentLength == 0)
                return Json(new { success = false, message = "Please select a valid Excel file." });

            try
            {
                // 1. Check if data already exists for this month/year/config before processing the file
                if (!forceUpdate)
                {
                    // Assume fnExecuteScalar returns the count of existing records
                    int existingCount = Convert.ToInt32(clsDatabase.fnDBOperation("SP_Get_Payroll_SaaS_Variable_Component_Inputs", configId, month, year, TenantId, paygroupid));

                    if (existingCount > 0)
                    {
                        return Json(new
                        {
                            success = true,
                            exists = true,
                            message = $"Data already exists for {month}/{year}. Do you want to overwrite it?"
                        });
                    }
                }

                // 2. Process Excel (Standard EPPlus Logic)
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var uploadList = new List<object>(); // List to hold JSON data

                using (var package = new ExcelPackage(file.InputStream))
                {
                    var ws = package.Workbook.Worksheets[0];
                    int rowCount = ws.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var empIdStr = ws.Cells[row, 1].Value?.ToString();
                        var valStr = ws.Cells[row, 6].Value?.ToString();

                        if (!string.IsNullOrEmpty(empIdStr) && decimal.TryParse(valStr, out decimal inputVal))
                        {
                            uploadList.Add(new
                            {
                                EmployeeID = int.Parse(empIdStr),
                                InputValue = inputVal
                            });
                        }
                    }
                }

                if (uploadList.Count > 0)
                {
                    // Convert list to JSON string
                    string JsonData = Newtonsoft.Json.JsonConvert.SerializeObject(uploadList);
                    string User = Session["UserName"]?.ToString() ?? "System";
                    // Send to Stored Procedure in ONE call
                    clsDatabase.fnDBOperation("SP_Payroll_SaveVariableComponentInput_Bulk", JsonData, configId,month,year,TenantId,User, paygroupid);

                    return Json(new { success = true, message = $"Successfully updated {uploadList.Count} records." });
                }

                return Json(new { success = false, message = "No valid data found in Excel." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetSalaryHistory(int EmployeeId, int ProcessMonth, int ProcessYear)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_Get_Previous3Month_Salary", TenantId,EmployeeId,ProcessMonth, ProcessYear);

            var data = dt.AsEnumerable().Select(x => new
            {
                empid = Convert.ToInt32(x["EmployeeID"]),
                employeeName = x["EmployeeName"].ToString(),
                employeeNo = x["EmployeeNO"].ToString(),
                paidDays = Convert.ToInt32(x["PaidDays"]),
                monthName = x["MonthName"].ToString(),
                processMonth = Convert.ToInt32(x["ProcessMonth"]),
                processYear = Convert.ToInt32(x["Year"]),
                basic = Convert.ToDecimal(x["Basic"]),
                gross = Convert.ToDecimal(x["Gross"]),
                totalAllowances = Convert.ToDecimal(x["TotalAllowances"]),
                totalDeduction = Convert.ToDecimal(x["TotalDeduction"]),
                netPay = Convert.ToDecimal(x["NetPay"])
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}
