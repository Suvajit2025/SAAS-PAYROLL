using ClosedXML.Excel;
using Common.Utility;
using DocumentFormat.OpenXml.EMMA;
using MendinePayroll.UI.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using UI.BLL;
using System.Web;

namespace MendinePayroll.UI.Controllers
{

    public class PayrollSalaryReportController : Controller
    {
        // GET: PayrollSalaryReport
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
        public ActionResult SalaryReport() {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "SALARY-REPORT";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        [HttpGet]
        public JsonResult GetFilterData()
        { 

            DataSet ds = clsDatabase.fnDataSet("SP_Payroll_SalaryRegister_FilterData", TenantId);

            var companies = ds.Tables[0].AsEnumerable().Select(r => new {
                Value = r["Value"]?.ToString(),
                Text = r["Text"]?.ToString()
            }).ToList();

            var payGroups = ds.Tables[1].AsEnumerable().Select(r => new {
                Value = r["Value"]?.ToString(),
                Text = r["Text"]?.ToString()
            }).ToList();

            return Json(new { success = true, companies, payGroups }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetGrid(int month, int year, int? companyId, int? payGroupId, string search,int? status, int pageNumber, int pageSize)
        {

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            DataSet ds = clsDatabase.fnDataSet( "SP_Payroll_SalaryRegister_Grid",TenantId, month, year, companyId, payGroupId, search, status, pageNumber, pageSize);

            int totalRows = ds.Tables[0].Rows.Count > 0 ? Convert.ToInt32(ds.Tables[0].Rows[0]["TotalRows"]) : 0;

            var rows = ds.Tables[1].AsEnumerable().Select(r => new {
                PayrollProcessEmployeeID = r["PayrollProcessEmployeeID"],
                EmployeeID = Convert.ToInt32(r["EmployeeID"]).ToString(),
                EmployeeCode = r["EmployeeCode"]?.ToString(),
                EmployeeName = r["EmployeeName"]?.ToString() ?? "",
                CompanyName = r["CompanyName"]?.ToString() ?? "",
                PayGroupName = r["PayGroupName"]?.ToString() ?? "",
                BasicAmount = Convert.ToDecimal(r["BasicAmount"] == DBNull.Value ? 0 : r["BasicAmount"]),
                AllowancesAmount = Convert.ToDecimal(r["AllowancesAmount"] == DBNull.Value ? 0 : r["AllowancesAmount"]),
                TotalGross = Convert.ToDecimal(r["TotalGross"] == DBNull.Value ? 0 : r["TotalGross"]),
                TotalAddition = Convert.ToDecimal(r["TotalAddition"] == DBNull.Value ? 0 : r["TotalAddition"]),
                TotalDeduction = Convert.ToDecimal(r["TotalDeduction"] == DBNull.Value ? 0 : r["TotalDeduction"]),
                TotalTakeHome = Convert.ToDecimal(r["TotalTakeHome"] == DBNull.Value ? 0 : r["TotalTakeHome"]),
                Status = Convert.ToInt32(r["Status"] == DBNull.Value ? 0 : r["Status"]),
                PaidStatus = Convert.ToInt32(r["PaidStatus"] == DBNull.Value ? 0 : r["PaidStatus"])
            }).ToList();

            return Json(new { success = true, totalRows, rows }, JsonRequestBehavior.AllowGet);
        }
        // PayrollSalaryReportController.cs

        [HttpPost]
        public JsonResult MarkPaid(int month, int year, string paidDate, string payrollProcessEmployeeIds)
        {

            try
            {
                
                var userName = Session["UserName"]?.ToString() ;

                if (string.IsNullOrWhiteSpace(userName))
                    return Json(new { success = false, message = "No Valid User." });
                if (string.IsNullOrWhiteSpace(payrollProcessEmployeeIds))
                    return Json(new { success = false, message = "No employees selected." });

                if (!DateTime.TryParse(paidDate, out DateTime paidDt))
                    return Json(new { success = false, message = "Invalid paid date." });


                clsAccessLogInfo info = new clsAccessLogInfo();
                info.AccessType = "SALARY-MARK-PAID";
                clsAccessLog.AccessLog_Save(info);

                /* =========================================================
                   1. VALIDATE STATUSES 
                   Only "Finalized" (Status 2) records can be marked as Paid.
                ========================================================= */
                DataTable dtValidation = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeStatusesForValidation", payrollProcessEmployeeIds);

                // Identify who is NOT Finalized (Draft or Rejected) to show as skipped
                var validationSkipped = dtValidation.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["Status"]) != 2)
                    .Select(r => new {
                        EmployeeName = r["EmployeeName"].ToString(),
                        SkipReason = Convert.ToInt32(r["Status"]) == 1 ? "Still in Draft" :
                                     Convert.ToInt32(r["Status"]) == 3 ? "Record is Rejected" : "Invalid Status"
                    }).ToList();

                // Filter only valid Finalized IDs to pass to the Mark Paid SP
                string finalizedIds = string.Join(",", dtValidation.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["Status"]) == 2)
                    .Select(r => r["RequestedID"].ToString()));

                if (string.IsNullOrEmpty(finalizedIds))
                {
                    return Json(new
                    {
                        success = false,
                        message = "No finalized records found to mark as paid.",
                        skippedEmployees = validationSkipped
                    });
                }

                /* =========================================================
                   2. EXECUTE MARK PAID PROCESS
                ========================================================= */
                DataSet ds = clsDatabase.fnDataSet("SP_Payroll_SalaryRegister_MarkPaid",
                                                    TenantId, month, year, paidDt.Date, userName, finalizedIds);

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return Json(new { success = false, message = "No response from payment process." });

                // Result Set 1 : Summary
                DataRow summary = ds.Tables[0].Rows[0];
                int paidUpdated = Convert.ToInt32(summary["PaidUpdated"]);
                int totalEmployees = Convert.ToInt32(summary["TotalEmployees"]);
                int paidEmployees = Convert.ToInt32(summary["PaidEmployees"]);
                string message = summary["Message"]?.ToString();

                // Result Set 2 : Detailed Skipped Employees (from SQL logic)
                var sqlSkipped = new List<object>();
                if (ds.Tables.Count > 1)
                {
                    sqlSkipped = ds.Tables[1].AsEnumerable()
                        .Select(r => new
                        {
                            EmployeeName = r["EmployeeName"].ToString(),
                            SkipReason = r["SkipReason"].ToString()
                        }).ToList<object>();
                }

                // Combine Validation Skips and SQL Skips
                var allSkipped = validationSkipped.Cast<object>().Concat(sqlSkipped).ToList();

                if(totalEmployees > 0)
                {
                    System.Web.Hosting.HostingEnvironment.QueueBackgroundWorkItem(ct =>
                        SendBulkPayslips(finalizedIds, month, year, TenantId)
                    );
                }
                return Json(new
                {
                    success = true,
                    paidUpdated,
                    totalEmployees,
                    paidEmployees,
                    skippedEmployees = allSkipped,
                    message
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetEmployeeBank(long EmployeeID)
        {
            try
            {
                DataTable dt = clsDatabase.fnDataTable(
                    "SP_Payroll_Employee_BankDetails",
                    EmployeeID,
                    TenantId
                );

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "Bank details not available" },
                                JsonRequestBehavior.AllowGet);
                }

                DataRow r = dt.Rows[0];

                string rawAccountNo = r["empaccountno"]?.ToString();

                return Json(new
                {
                    success = true,
                    bankName = r["BankName"]?.ToString(),
                    branch = r["empbankbranchname"]?.ToString(),
                    ifsc = r["empbankbranchifsccode"]?.ToString(),
                    accountNo = MaskAccountNumber(rawAccountNo)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }
        private string MaskAccountNumber(string accountNo)
        {
            if (string.IsNullOrWhiteSpace(accountNo))
                return "—";

            accountNo = accountNo.Trim();

            if (accountNo.Length <= 4)
                return accountNo;

            return new string('*', accountNo.Length - 4) + accountNo.Substring(accountNo.Length - 4);
        }

        
        [HttpGet]
        public ActionResult ExportExcel(int month, int year, string payrollProcessEmployeeIds)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_Payroll_SalaryReport_Export", TenantId, month, year, payrollProcessEmployeeIds);
            if (dt == null || dt.Rows.Count == 0) return Content("No data found.");

            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Salary Sheet");

                // Dynamic Setup
                DataView dv = dt.DefaultView;
                dv.Sort = "Department ASC";
                DataTable sortedDt = dv.ToTable();
                int totalCols = sortedDt.Columns.Count;

                // 1. Title
                var title = ws.Range(1, 1, 1, totalCols).Merge();
                title.Value = $"Salary Sheet Report for the Month of - {new DateTime(year, month, 1):MMMM yyyy}";
                title.Style.Font.Bold = true;
                title.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int headerRow = 4;
                int currentRow = 5;

                // 2. Headers
                for (int i = 0; i < totalCols; i++)
                {
                    var cell = ws.Cell(headerRow, i + 1);
                    string colName = sortedDt.Columns[i].ColumnName;
                    cell.Value = colName;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // Specific Column Colors
                    if (colName.Contains("ACTUAL_GROSS")) cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    if (colName.Contains("PAIDDAYS")) cell.Style.Fill.BackgroundColor = XLColor.LightSalmon;
                    if (colName.Contains("EARNED_GROSS")) cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                }

                // 3. Data Loop with Grouping
                string currentDept = "";
                int deptStartRow = currentRow;

                for (int i = 0; i < sortedDt.Rows.Count; i++)
                {
                    string rowDept = sortedDt.Rows[i]["Department"].ToString();

                    if (i > 0 && rowDept != currentDept)
                    {
                        AddSummary(ws, ref currentRow, deptStartRow, currentDept + " TOTAL", sortedDt.Columns, XLColor.GoldenYellow, totalCols);
                        deptStartRow = currentRow;
                    }

                    for (int j = 0; j < totalCols; j++)
                    {
                        var cell = ws.Cell(currentRow, j + 1);
                        string cellVal = sortedDt.Rows[i][j]?.ToString() ?? "";
                        cell.Value = cellVal;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        // Status Logic
                        if (sortedDt.Columns[j].ColumnName.ToUpper() == "STATUS")
                        {
                            if (cellVal == "Draft") cell.Style.Fill.BackgroundColor = XLColor.BananaMania;
                            else if (cellVal == "Finalized") cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                            else if (cellVal == "Rejected") cell.Style.Fill.BackgroundColor = XLColor.Coquelicot;
                        }
                    }
                    currentDept = rowDept;
                    currentRow++;
                }

                // 4. Totals (Yellow for Group, Green for Grand)
                if (sortedDt.Rows.Count > 0)
                    AddSummary(ws, ref currentRow, deptStartRow, currentDept + " TOTAL", sortedDt.Columns, XLColor.GoldenYellow, totalCols);

                AddSummary(ws, ref currentRow, 5, "GRAND TOTAL", sortedDt.Columns, XLColor.GreenRyb, totalCols);

                ws.Columns(1, totalCols).AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalaryReport.xlsx");
                }
            }
        }

        private void AddSummary(IXLWorksheet ws, ref int row, int startRow, string label, DataColumnCollection cols, XLColor color, int maxCol)
        {
            ws.Cell(row, 1).Value = label;
            ws.Range(row, 1, row, maxCol).Style.Font.Bold = true;
            ws.Range(row, 1, row, maxCol).Style.Fill.BackgroundColor = color;
            ws.Range(row, 1, row, maxCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int c = 1; c <= maxCol; c++)
            {
                string name = cols[c - 1].ColumnName.ToUpper();
                if (name.Contains("ACTUAL") || name.Contains("EARNED") || name.Contains("GROSS") || name.Contains("PAY") || name.Contains("DEDUCTION"))
                {
                    ws.Cell(row, c).FormulaA1 = $"=SUM({ws.Cell(startRow, c).Address}:{ws.Cell(row - 1, c).Address})";
                }
                ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            row++;
        }
        
        public ActionResult CTCReport()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "CTC-REPORT";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }
        [HttpGet]
        public JsonResult GetCTCReport()
        {
            DataTable dt = clsDatabase.fnDataTable("PRC_SaaS_Employee_CTC_REPORT",TenantId);

            var columns = dt.Columns.Cast<DataColumn>()
                            .Select(c => c.ColumnName)
                            .ToList();

            var rows = new List<Dictionary<string, object>>();

            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    row[col.ColumnName] = dr[col];
                rows.Add(row);
            }

            return Json(new
            {
                columns,
                rows
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BulkApprove(int month, int year, string payrollProcessEmployeeIds)
        {
            try
            {
                string userName = Session["UserName"]?.ToString();
                if (string.IsNullOrEmpty(userName))
                    return Json(new { success = false, message = "Session expired" });

                if (string.IsNullOrEmpty(payrollProcessEmployeeIds))
                    return Json(new { success = false, message = "No employees selected" });

                clsAccessLogInfo info = new clsAccessLogInfo();
                info.AccessType = "SALARY-APPROVE";
                clsAccessLog.AccessLog_Save(info);
                /* =========================================================
                       1. IDENTIFY SKIPPED VS VALID (ONLY STATUS 1 ALLOWED)
                    ========================================================= */
                DataTable dtValidation = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeStatusesForValidation",
                                                         payrollProcessEmployeeIds);

                if (dtValidation == null || dtValidation.Rows.Count == 0)
                    return Json(new { success = false, message = "No records found for the selected IDs." });

                // Identify Skipped vs. Valid
                var skippedEmployees = dtValidation.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["Status"]) != 1) // Filter anyone NOT in Draft
                    .Select(r => new {
                        Name = r["EmployeeName"].ToString(),
                        Code = r["empcode"].ToString(),
                        // 🔑 THIS IS HOW YOU GET THE REASON:
                        Reason = Convert.ToInt32(r["Status"]) == 2 ? "Already Finalized" :
                                 Convert.ToInt32(r["Status"]) == 3 ? "Rejected (Fix first)" :
                                 Convert.ToInt32(r["Status"]) == 0 ? "Record Missing" : "Invalid Status"
                    }).ToList();

                // Filter only the Draft IDs for the next processing steps
                string validDraftIds = string.Join(",", dtValidation.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["Status"]) == 1)
                    .Select(r => r["RequestedID"].ToString()));

                if (string.IsNullOrEmpty(validDraftIds))
                {
                    return Json(new
                    {
                        success = false,
                        message = "No valid draft records selected for approval.",
                        skipped = skippedEmployees
                    });
                }
                /* =========================================================
                   1.1 GROUP SELECTED EMPLOYEES BY BATCH (Month/Year)
                ========================================================= */
                DataTable dtBatchGroups = clsDatabase.fnDataTable("SP_Payroll_SaaS_GetSelectedGroupsForApproval",
                                             month, year, validDraftIds, TenantId);

                if (dtBatchGroups == null || dtBatchGroups.Rows.Count == 0)
                    return Json(new { success = false, message = "No valid draft records found for approval" });

                int totalBatchesProcessed = 0;

                foreach (DataRow batchRow in dtBatchGroups.Rows)
                {
                    // 🔑 Context for THIS specific batch group
                    int currentMonth = Convert.ToInt32(batchRow["ProcessMonth"]);
                    int currentYear = Convert.ToInt32(batchRow["ProcessYear"]);
                    string empIdsInThisBatch = batchRow["EmployeeIdList"].ToString();
                    string currentMonthName = new DateTime(currentYear, currentMonth, 1).ToString("MMMM", CultureInfo.InvariantCulture);

                    /* =========================================================
                       2. BUILD LOAN JSON (Only for this Batch group)
                    ========================================================= */
                    DataTable dtLoans = clsDatabase.fnDataTable("SP_Payroll_SaaS_GetSelectedLoanComponents_FromPreview",
                                                 empIdsInThisBatch, TenantId);

                    var loanJsonList = new List<object>();

                    foreach (DataRow r in dtLoans.Rows)
                    {
                        string ReferenceIds = r["ReferenceIds"]?.ToString();
                        if (!string.IsNullOrEmpty(ReferenceIds) && ReferenceIds != "0")
                        {
                            DataTable dtPending = clsDatabase.fnDataTable("PRC_Bulk_EmployeeLoan_SpecificPending", ReferenceIds, currentMonthName,currentYear,TenantId);
                            foreach (DataRow pRow in dtPending.Rows)
                            {
                                bool alreadyProcessed = Convert.ToBoolean(pRow["AlreadyProcessedExternally"]);
                                if (alreadyProcessed) continue;

                                bool isWaived = Convert.ToBoolean(r["LoanWaivedYN"]);
                                decimal previewPayValue = Convert.ToDecimal(r["PayValue"]);

                                loanJsonList.Add(new
                                {
                                    
                                    IDLoan = Convert.ToInt32(pRow["IDLoan"]),
                                    IDEmployee = Convert.ToInt32(r["EmployeeID"]),
                                    InstallmentMonth = currentMonth,
                                    InstallmentYear = currentYear,
                                    BalanceBefore = Convert.ToDecimal(pRow["BalanceBefore"]),
                                    PrincipalComponent = Convert.ToDecimal(pRow["PrincipalComponent"]),
                                    InterestComponent = Convert.ToDecimal(pRow["InterestComponent"]),
                                    AmountPaid = Convert.ToDecimal(pRow["AmountToBePaid"]),
                                    BalanceAfter = Convert.ToDecimal(pRow["BalanceAfter"]),
                                    WaiverAmount = Convert.ToDecimal(pRow["WaiverAmount"]),
                                    WaiverType = pRow["WaiverType"]?.ToString(),
                                    EmployeeNo = pRow["EmployeeNo"]?.ToString(),
                                    EmployeeName = pRow["EmployeeName"]?.ToString()
                                });
                            }
                        }
                    }

                    /* =========================================================
                        3. PROCESS LOANS & FINALIZE FOR THIS BATCH GROUP
                       ========================================================= */

                    // A. Update Loan Ledger first
                    if (loanJsonList.Any())
                    {
                        string jsonData = JsonConvert.SerializeObject(loanJsonList);
                        // 🔑 Use the loop-specific currentMonthName and currentYear
                        clsDatabase.fnDBOperation("PRC_Bulk_EmployeeLoan_Process", jsonData, currentMonthName, currentYear, userName, TenantId,"Salary", "Processed via Salary Process");
                    }

                    // B. Move employees from this specific batch group to Main Tables (Status 1 -> 2)
                    DataTable dtFinalize = clsDatabase.fnDataTable(
                        "SP_Payroll_SaaS_FinalizeSelected_FromPreview",
                        empIdsInThisBatch,
                        currentMonth,
                        currentYear,
                        TenantId,
                        userName
                    );

                    if (dtFinalize == null || Convert.ToInt32(dtFinalize.Rows[0]["StatusCode"]) != 1)
                    {
                        throw new Exception($"Finalization failed for batch {currentMonthName}: " + (dtFinalize?.Rows[0]["Message"]?.ToString() ?? "Unknown error"));
                    }

                    totalBatchesProcessed++;
                }

                return Json(new
                {
                    success = true,
                    message = $"Successfully finalized valid employees across {totalBatchesProcessed} payroll periods.",
                    skipped = skippedEmployees // 🔑 THIS IS THE KEY: Send the list to the UI
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

         
        [HttpPost]
        public JsonResult BulkReject(RejectRequest model)
        {
            try
            {
                string userName = Session["UserName"]?.ToString();
                if (string.IsNullOrEmpty(userName))
                    return Json(new { success = false, message = "Session expired" });
                if (model == null || string.IsNullOrEmpty(model.payrollProcessEmployeeIds))
                {
                    return Json(new { success = false, message = "No employees selected for rejection." });
                }

                clsAccessLogInfo info = new clsAccessLogInfo();
                info.AccessType = "SALARY-REJECT";
                clsAccessLog.AccessLog_Save(info);
                /* =========================================================
                   1. VALIDATE STATUSES (Using our same SP)
                ========================================================= */
                DataTable dtValidation = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeStatusesForValidation",
                                                                 model.payrollProcessEmployeeIds);

                // Identify who to SKIP
                var skippedEmployees = dtValidation.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["Status"]) != 1) // Anything not a Draft
                    .Select(r => new {
                        Name = r["EmployeeName"].ToString(),
                        Code = r["empcode"].ToString(),
                        Reason = Convert.ToInt32(r["Status"]) == 2 ? "Already Finalized" :
                                 Convert.ToInt32(r["Status"]) == 3 ? "Already Rejected" : "Record Missing"
                    }).ToList();

                // Identify valid IDs to REJECT (Only Status 1)
                string validIdsToReject = string.Join(",", dtValidation.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["Status"]) == 1)
                    .Select(r => r["RequestedID"].ToString()));

                if (string.IsNullOrEmpty(validIdsToReject))
                {
                    return Json(new
                    {
                        success = false,
                        message = "No valid draft records found to reject.",
                        skipped = skippedEmployees
                    });
                }

                /* =========================================================
                   2. CALL REJECT SP FOR VALID IDS ONLY
                ========================================================= */
                 
                DataTable dt = clsDatabase.fnDataTable("SP_Payroll_BulkReject", validIdsToReject, model.reason, model.month, model.year, userName,TenantId);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int statusCode = Convert.ToInt32(dt.Rows[0]["StatusCode"]);
                    string resultMessage = dt.Rows[0]["Message"].ToString();

                    return Json(new
                    {
                        success = (statusCode == 1),
                        message = resultMessage,
                        skipped = skippedEmployees // 🔑 Send to Modal
                    });
                }

                return Json(new { success = false, message = "Database did not return a response." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server Error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetAllSelectedIds(int month, int year, int? companyId, int? payGroupId, string search, int? status)
        {
            try
            {
                // 1. We call the same Grid SP but pass 1 for pageNumber and 999999 for pageSize 
                // to retrieve every single ID matching the filters at once.
                DataSet ds = clsDatabase.fnDataSet("SP_Payroll_SalaryRegister_Grid",
                                                    TenantId, month, year, companyId, payGroupId, search, status, 1, 999999);

                if (ds == null || ds.Tables.Count < 2)
                    return Json(new { success = false, message = "No data found" }, JsonRequestBehavior.AllowGet);

                // 2. We only need the PayrollProcessEmployeeID column for selection memory
                var idList = ds.Tables[1].AsEnumerable()
                               .Select(r => r["PayrollProcessEmployeeID"].ToString())
                               .ToList();

                return Json(new { success = true, ids = idList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //private void SendBulkPayslips(string payrollProcessEmployeeIds,int month,int year,string tenantId)
        //{
        //    try
        //    {
        //        DataTable dtValidation =
        //            clsDatabase.fnDataTable(
        //                "SP_Payroll_GetEmployeeStatusesForValidation",
        //                payrollProcessEmployeeIds);

        //        string monthName =
        //            CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

        //        // 🔥 FULL MVC CONTEXT (REQUIRED)
        //        var request = new HttpRequest("", "http://localhost/", "");
        //        var response = new HttpResponse(new StringWriter());
        //        var httpContext = new HttpContext(request, response);
        //        System.Web.HttpContext.Current = httpContext; // 🔴 FIX: Use Syst
        //                                                      // em.Web.HttpContext.Current
        //        var contextWrapper = new HttpContextWrapper(httpContext);

        //        // 🔥 REAL controller (NOT empty)
        //        var payslipCtrl = new PayslipController
        //        {
        //            ManualTenantId = tenantId,
        //            ViewData = new ViewDataDictionary(),
        //            TempData = new TempDataDictionary()
        //        };

        //        var routeData = new RouteData();
        //        routeData.Values["controller"] = "Payslip";
        //        routeData.Values["action"] = "Index";

        //        var controllerContext = new ControllerContext(
        //            contextWrapper,
        //            routeData,
        //            payslipCtrl
        //        );

        //        payslipCtrl.ControllerContext = controllerContext;

        //        foreach (DataRow dr in dtValidation.Rows)
        //        {
        //            try
        //            {
        //                int empId = Convert.ToInt32(dr["EmployeeId"]);
        //                string email = dr["empemail"]?.ToString();
        //                string firstName = dr["empfirstname"]?.ToString();

        //                if (string.IsNullOrWhiteSpace(email))
        //                    continue;

        //                // 1️⃣ Data
        //                var model = payslipCtrl.GetPayslipData(
        //                    empId,
        //                    monthName,
        //                    year,
        //                    true);

        //                if (model == null)
        //                    continue;

        //                // 2️⃣ PDF
        //                var pdf = new Rotativa.ViewAsPdf(
        //                    "~/Views/Payslip/Index.cshtml",
        //                    model)
        //                {
        //                    PageSize = Rotativa.Options.Size.A4,
        //                    PageOrientation = Rotativa.Options.Orientation.Portrait
        //                };

        //                byte[] pdfBytes = pdf.BuildFile(controllerContext);

        //                if (pdfBytes == null || pdfBytes.Length == 0)
        //                    throw new Exception("Rotativa generated empty PDF.");
        //                        var mailService = new MailService();
        //                        mailService.SendMails(
        //                             toEmail: email,
        //                             fromEmail: "",
        //                             bodyHtml: $"Dear {firstName},<br/><br/>Please find your payslip attached.<br/><br/>Regards,<br/>Payroll Team",
        //                             subject: $"Payslip for {monthName} {year}",
        //                             attachmentBytes: pdfBytes,
        //                             fileNameWithoutExt: $"Payslip_{monthName}_{year}"
        //                         );
        //                    }
        //            catch (Exception ex)
        //            {
        //                clsDatabase.fnErrorLog("BulkPayslipLoopError", ex.ToString());
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsDatabase.fnErrorLog("BulkPayslipMainError", ex.ToString());
        //    }
        //}
        private void SendBulkPayslips(string payrollProcessEmployeeIds,int month,int year,string tenantId)
        {
            try
            {
                DataTable dtValidation =
                    clsDatabase.fnDataTable(
                        "SP_Payroll_GetEmployeeStatusesForValidation",
                        payrollProcessEmployeeIds);

                if (dtValidation == null || dtValidation.Rows.Count == 0)
                    return;

                string monthName =
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

                // 🔥 Create FULL HttpContext (REQUIRED for Rotativa)
                var request = new HttpRequest("", "http://localhost/", "");
                var response = new HttpResponse(new StringWriter());
                var httpContext = new HttpContext(request, response);

                System.Web.HttpContext.Current = httpContext;

                var contextWrapper = new HttpContextWrapper(httpContext);

                // 🔥 Resolve TenantId safely
                string resolvedTenantId = tenantId;

                // 1️⃣ If not passed explicitly, try Session
                if (string.IsNullOrWhiteSpace(resolvedTenantId))
                {
                    if (System.Web.HttpContext.Current?.Session?["TenantID"] != null)
                    {
                        resolvedTenantId =
                            System.Web.HttpContext.Current.Session["TenantID"].ToString();
                    }
                }

                // 2️⃣ Final guard (VERY IMPORTANT)
                if (string.IsNullOrWhiteSpace(resolvedTenantId))
                {
                    throw new Exception("TenantId not available for payslip generation.");
                }

                // 🔥 Real controller instance (SAFE NOW)
                var payslipCtrl = new PayslipController
                {
                    ManualTenantId = resolvedTenantId,
                    ViewData = new ViewDataDictionary(),
                    TempData = new TempDataDictionary()
                };


                var routeData = new RouteData();
                routeData.Values["controller"] = "Payslip";
                routeData.Values["action"] = "Index";

                var controllerContext = new ControllerContext(
                    contextWrapper,
                    routeData,
                    payslipCtrl
                );

                payslipCtrl.ControllerContext = controllerContext;

                // 🔥 Mail service (create once)
                var mailService = new MailService();

                foreach (DataRow dr in dtValidation.Rows)
                {
                    try
                    {
                        int empId = Convert.ToInt32(dr["EmployeeId"]);
                        string email = dr["empemail"]?.ToString();
                        string firstName = dr["empfirstname"]?.ToString();

                        if (string.IsNullOrWhiteSpace(email))
                            continue;

                        // 1️⃣ Get payslip data
                        var model = payslipCtrl.GetPayslipData(
                            empId,
                            monthName,
                            year,
                            true);

                        if (model == null)
                            continue;

                        // 2️⃣ Generate PDF
                        var pdf = new Rotativa.ViewAsPdf(
                            "~/Views/Payslip/Index.cshtml",
                            model)
                        {
                            PageSize = Rotativa.Options.Size.A4,
                            PageOrientation = Rotativa.Options.Orientation.Portrait
                        };

                        byte[] pdfBytes = pdf.BuildFile(controllerContext);

                        if (pdfBytes == null || pdfBytes.Length == 0)
                            throw new Exception("Rotativa generated empty PDF.");

                        // 3️⃣ Send mail
                        //mailService.SendMails(
                        //    toEmail: email,
                        //    fromEmail: "",
                        //    bodyHtml:
                        //        $"Dear {firstName},<br/><br/>" +
                        //        $"Please find your payslip for {monthName} {year} attached.<br/><br/>" +
                        //        $"Regards,<br/>Payroll Team",
                        //    subject: $"Payslip for {monthName} {year}",
                        //    attachmentBytes: pdfBytes,
                        //    fileNameWithoutExt: $"Payslip_{monthName}_{year}"
                        //);
                        mailService.SendMails(
                            toEmail: email,
                            fromEmail: "",
                            bodyHtml:
                                $"<b style='color:red;'>*** THIS IS A TESTING MAIL – PLEASE IGNORE ***</b><br/><br/>" +
                                $"Dear {firstName},<br/><br/>" +
                                $"Please find your payslip for <b>{monthName} {year}</b> attached.<br/><br/>" +
                                $"Regards,<br/>Payroll Team",
                            subject: $"[TEST MAIL – PLEASE IGNORE] Payslip for {monthName} {year}",
                            attachmentBytes: pdfBytes,
                            fileNameWithoutExt: $"Payslip_{monthName}_{year}"
                        );
                    }
                    catch (Exception ex)
                    {
                        clsDatabase.fnErrorLog(
                            "BulkPayslipLoopError",
                            ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                clsDatabase.fnErrorLog(
                    "BulkPayslipMainError",
                    ex.ToString());
            }
        }

    }
}