using Common.Utility;
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
using UI.BLL;

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
                 
                var userName = Session["UserName"]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(payrollProcessEmployeeIds))
                    return Json(new { success = false, message = "No employees selected." });

                if (!DateTime.TryParse(paidDate, out DateTime paidDt))
                    return Json(new { success = false, message = "Invalid paid date." });

                DataSet ds = clsDatabase.fnDataSet("SP_Payroll_SalaryRegister_MarkPaid",TenantId,month,year,paidDt.Date,userName,payrollProcessEmployeeIds );

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return Json(new { success = false, message = "No response from payment process." });

                // =============================
                // Result Set 1 : Summary
                // =============================
                DataRow summary = ds.Tables[0].Rows[0];

                int paidUpdated = Convert.ToInt32(summary["PaidUpdated"]);
                int totalEmployees = Convert.ToInt32(summary["TotalEmployees"]);
                int paidEmployees = Convert.ToInt32(summary["PaidEmployees"]);
                string message = summary["Message"]?.ToString();

                // =============================
                // Result Set 2 : Skipped Employees
                // =============================
                var skippedEmployees = new List<object>();

                if (ds.Tables.Count > 1)
                {
                    skippedEmployees = ds.Tables[1].AsEnumerable()
                        .Select(r => new
                        {
                            EmployeeID = r["EmployeeID"].ToString(),
                            EmployeeName = r["EmployeeName"].ToString(),
                            SkipReason = r["SkipReason"].ToString()
                        })
                        .ToList<object>();
                }

                // =============================
                // FINAL RESPONSE
                // =============================
                return Json(new
                {
                    success = true,
                    paidUpdated,
                    totalEmployees,
                    paidEmployees,
                    skippedEmployees,
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
        public ActionResult ExportExcel(int month,int year,int? companyId,int? payGroupId,int? status,string search)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_Payroll_SalaryRegister_Export",TenantId,month,year,companyId,payGroupId,status,search);

            if (dt == null || dt.Rows.Count == 0)
                return Content("No data available for export.");

            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Salary Register");
                ws.Cell(1, 1).InsertTable(dt);

                ws.Columns().AdjustToContents();
                ws.Row(1).Style.Font.Bold = true;

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"SalaryRegister_{month}_{year}.xlsx";

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }
            }
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

                /* =========================================================
                   1. GROUP SELECTED EMPLOYEES BY BATCH (Month/Year)
                ========================================================= */
                DataTable dtBatchGroups = clsDatabase.fnDataTable("SP_Payroll_SaaS_GetSelectedGroupsForApproval",
                                             month, year, payrollProcessEmployeeIds, TenantId);

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
                    message = $"Successfully finalized employees across {totalBatchesProcessed} payroll periods."
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
                // 1. Basic Validation
                if (model == null || string.IsNullOrEmpty(model.payrollProcessEmployeeIds))
                {
                    return Json(new { success = false, message = "No employees selected for rejection." });
                }

                string userName = Session["UserName"]?.ToString();

                DataTable dt = clsDatabase.fnDataTable("SP_Payroll_BulkReject",model.payrollProcessEmployeeIds,model.reason, model.month, model.year, userName, TenantId);

                return Json(new { success = false, message = "Unable to process rejection." });
            }
            catch (Exception ex)
            {
                // Log Error here
                return Json(new { success = false, message = "Server Error: " + ex.Message });
            }
        } 
    }
}