using ClosedXML.Excel;
using Common.Utility;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using MendinePayroll.Models;
using MendinePayroll.UI.BLL;
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
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
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
        public JsonResult GetActivePayrollCycle()
        {
            try
            {
                DataTable dt = clsDatabase.fnDataTable("SP_PayrollCycleConfiguration_GetActive", TenantId);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Payroll cycle configuration is not set."
                    }, JsonRequestBehavior.AllowGet);
                }

                DataRow row = dt.Rows[0];
                byte paidDaysSource = row.Table.Columns.Contains("PaidDaysSource") && row["PaidDaysSource"] != DBNull.Value
                    ? Convert.ToByte(row["PaidDaysSource"])
                    : (byte)1;

                return Json(new
                {
                    success = true,
                    PaidDaysSource = paidDaysSource,
                    paidDaysSource = paidDaysSource
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetFilterData()
        {
            string UserId = Session["UserId"].ToString();
            DataSet ds = clsDatabase.fnDataSet("SP_Payroll_SalaryRegister_FilterData", TenantId, UserId);

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
            string UserId = Session["UserId"].ToString();
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            DataSet ds = clsDatabase.fnDataSet( "SP_Payroll_SalaryRegister_Grid",TenantId, month, year, companyId, payGroupId, search, status,  pageNumber, pageSize, UserId);

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

                var tenantId = TenantId;
                var applicationBaseUrl = GetApplicationBaseUrl();
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
                DataTable dtValidation = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeStatusesForValidation", payrollProcessEmployeeIds,TenantId);

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
                DataSet ds = clsDatabase.fnDataSet("SP_Payroll_SalaryRegister_MarkPaid",tenantId, month, year, paidDt.Date, userName, finalizedIds);

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

                if(paidUpdated > 0)
                {
                    LogBulkPayslipDebug(
                        "BulkPayslipQueued",
                        $"Ids={finalizedIds}; Month={month}; Year={year}; TenantId={tenantId}; BaseUrl={applicationBaseUrl}");

                    System.Web.Hosting.HostingEnvironment.QueueBackgroundWorkItem(ct =>
                        SendBulkPayslips(finalizedIds, month, year, tenantId, applicationBaseUrl)
                    );
                }
                else
                {
                    LogBulkPayslipDebug(
                        "BulkPayslipNotQueued",
                        $"No newly paid employees. Ids={finalizedIds}; Month={month}; Year={year}; TenantId={tenantId}; PaidUpdated={paidUpdated}; TotalEmployees={totalEmployees}");
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

        [HttpPost]
        public JsonResult SendPayslipMail(int month, int year, string payrollProcessEmployeeId)
        {
            try
            {
                if (Session["UserName"] == null)
                    return Json(new { success = false, message = "Session expired" });

                if (string.IsNullOrWhiteSpace(payrollProcessEmployeeId))
                    return Json(new { success = false, message = "No employee selected." });

                var tenantId = TenantId;
                var applicationBaseUrl = GetApplicationBaseUrl();

                DataTable dtValidation = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeStatusesForValidation",payrollProcessEmployeeId,tenantId);

                if (dtValidation == null || dtValidation.Rows.Count == 0)
                    return Json(new { success = false, message = "Payroll row not found." });

                DataRow row = dtValidation.Rows[0];
                int status = Convert.ToInt32(row["Status"] == DBNull.Value ? 0 : row["Status"]);
                int paidStatus = GetIntColumnValue(row, "PaidStatus");
                if (paidStatus != 1 && !row.Table.Columns.Contains("PaidStatus"))
                {
                    paidStatus = GetPaidStatusFromGrid(month, year, payrollProcessEmployeeId);
                }

                if (status == 3)
                    return Json(new { success = false, message = "Payslip mail cannot be sent for rejected payroll." });

                if (paidStatus != 1)
                    return Json(new { success = false, message = "Please mark this salary as paid before sending payslip mail." });

                clsAccessLogInfo info = new clsAccessLogInfo();
                info.AccessType = "SALARY-PAYSLIP-MAIL";
                clsAccessLog.AccessLog_Save(info);

                LogBulkPayslipDebug(
                    "SinglePayslipQueued",
                    $"Id={payrollProcessEmployeeId}; Month={month}; Year={year}; TenantId={tenantId}; BaseUrl={applicationBaseUrl}");

                System.Web.Hosting.HostingEnvironment.QueueBackgroundWorkItem(ct =>
                    SendBulkPayslips(payrollProcessEmployeeId, month, year, tenantId, applicationBaseUrl, true)
                );

                return Json(new { success = true, message = "Payslip mail queued successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetIntColumnValue(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            return Convert.ToInt32(row[columnName]);
        }

        private int GetPaidStatusFromGrid(int month, int year, string payrollProcessEmployeeId)
        {
            string userId = Session["UserId"]?.ToString();
            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            DataSet ds = clsDatabase.fnDataSet(
                "SP_Payroll_SalaryRegister_Grid",
                TenantId,
                month,
                year,
                null,
                null,
                null,
                null,
                1,
                999999,
                userId);

            if (ds == null || ds.Tables.Count < 2)
                return 0;

            DataRow row = ds.Tables[1].AsEnumerable()
                .FirstOrDefault(r => r["PayrollProcessEmployeeID"].ToString() == payrollProcessEmployeeId);

            return GetIntColumnValue(row, "PaidStatus");
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

        
        [HttpPost]
        public ActionResult ExportExcel(int month, int year, List<long> payrollProcessEmployeeIds)
        {
            if (payrollProcessEmployeeIds == null || payrollProcessEmployeeIds.Count == 0)
                return Content("No employees selected.");

            string selectedIds = string.Join(",", payrollProcessEmployeeIds);
            DataTable dt = clsDatabase.fnDataTable("SP_Payroll_SalaryReport_Export", TenantId, month, year, selectedIds);
            if (dt == null || dt.Rows.Count == 0) return Content("No data found.");

            if (Guid.Parse(TenantId) == new Guid("808E0AC6-94CE-4887-AD9D-85AF318ED335"))
            {
                DataTable selectedEmployees = clsDatabase.fnDataTable(
                    "SP_Payroll_GetEmployeeStatusesForValidation", selectedIds, TenantId);
                if (selectedEmployees == null || selectedEmployees.Rows.Count == 0)
                    return Content("No selected payroll records found.");

                string selectedEmployeesJson = JsonConvert.SerializeObject(
                    selectedEmployees.AsEnumerable().Select(r => new
                    {
                        RequestedID = Convert.ToInt64(r["RequestedID"]),
                        EmployeeId = Convert.ToInt32(r["EmployeeId"]),
                        Status = Convert.ToInt32(r["Status"])
                    }));

                DataTable ctcDetails = clsDatabase.fnDataTable(
                    "SP_Payroll_SalaryReport_Export_CTCDetails", TenantId, month, year, selectedEmployeesJson);

                return File(BuildCtcSalaryExport(dt, ctcDetails, month, year),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalaryReport.xlsx");
            }

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

        private static byte[] BuildCtcSalaryExport(DataTable salaryReport, DataTable ctcDetails, int month, int year)
        {
            if (ctcDetails == null || ctcDetails.Rows.Count == 0)
                throw new InvalidOperationException("CTC and employer contribution data was not found for this payroll period.");

            Tuple<string, string, string> GetKey(DataRow row)
            {
                string Normalize(string column) => Convert.ToString(row[column]).Trim().ToUpperInvariant();
                return Tuple.Create(Normalize("EmpCode"), Normalize("Salary Format"), Normalize("Status"));
            }

            decimal GetAmount(DataRow row, params string[] columnNames)
            {
                foreach (string column in columnNames)
                {
                    if (row.Table.Columns.Contains(column) && row[column] != DBNull.Value)
                        return Convert.ToDecimal(row[column], CultureInfo.InvariantCulture);
                }
                return 0m;
            }

            var ctcByEmployee = ctcDetails.AsEnumerable().ToDictionary(GetKey);
            var employees = salaryReport.AsEnumerable()
                .OrderBy(r => Convert.ToString(r["Department"]))
                .ThenBy(r => Convert.ToString(r["EmployeeName"]))
                .ThenBy(r => Convert.ToString(r["EmpCode"]))
                .ToList();

            string[] headers =
            {
                "Sl No", "Emp ID", "Employee Name", "Type", "Paid Days", "Monthly CTC", "Annual CTC",
                "Basic", "HRA", "Conveyance", "Gross Salary", "ACTIVITY INCENTIVE",
                "VARIABLE INCENTIVE", "Employer PF", "Employer ESIC", "Gratuity", "Ex Gratia",
                "Total Employer Contrib.", "Employee PF", "Employee ESIC", "Professional Tax",
                "Total Deductions", "Monthly Expense", "Net Take Home"
            };

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Salary Sheet");
                var title = sheet.Range(1, 1, 1, headers.Length).Merge();
                title.Value = $"Salary Sheet Report for the Month of - {new DateTime(year, month, 1):MMMM yyyy}";
                title.Style.Font.Bold = true;
                title.Style.Font.FontSize = 14;
                title.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                const int headerRow = 4;
                for (int i = 0; i < headers.Length; i++)
                    sheet.Cell(headerRow, i + 1).Value = headers[i];

                var header = sheet.Range(headerRow, 1, headerRow, headers.Length);
                header.Style.Font.Bold = true;
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#0E7777");
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Alignment.WrapText = true;
                sheet.Row(headerRow).Height = 34;

                int currentRow = headerRow + 1;
                foreach (DataRow employee in employees)
                {
                    if (!ctcByEmployee.TryGetValue(GetKey(employee), out DataRow ctc))
                        throw new InvalidOperationException($"CTC data was not found for employee {employee["EmpCode"]} in this payroll period.");
                    if (ctc["MonthlyCTC"] == DBNull.Value || ctc["AnnualCTC"] == DBNull.Value)
                        throw new InvalidOperationException($"Monthly or Annual CTC is missing from the active salary configuration for employee {employee["EmpCode"]}.");

                    sheet.Cell(currentRow, 1).Value = currentRow - headerRow;
                    sheet.Cell(currentRow, 2).SetValue<string>(Convert.ToString(employee["EmpCode"]));
                    sheet.Cell(currentRow, 3).SetValue<string>(Convert.ToString(employee["EmployeeName"]));
                    sheet.Cell(currentRow, 4).SetValue<string>(Convert.ToString(employee["Status"]));
                    sheet.Cell(currentRow, 5).Value = GetAmount(employee, "TotalPaidDays");

                    decimal monthlyCtc = GetAmount(ctc, "MonthlyCTC");
                    decimal[] amounts =
                    {
                        monthlyCtc,
                        GetAmount(ctc, "AnnualCTC"),
                        GetAmount(employee, "BASIC"),
                        GetAmount(employee, "HRA"),
                        GetAmount(employee, "CONVEYANCE"),
                        GetAmount(employee, "GROSS"),
                        GetAmount(employee, "ACTIVITY INCENTIVE"),
                        GetAmount(employee, "VARIABLE INCENTIVE"),
                        GetAmount(ctc, "EmployerPF"),
                        GetAmount(ctc, "EmployerESIC"),
                        GetAmount(ctc, "Gratuity"),
                        GetAmount(ctc, "ExGratia"),
                        GetAmount(ctc, "TotalEmployerContribution"),
                        GetAmount(employee, "EMPLOYEE PF"),
                        GetAmount(employee, "EMPLOYEE ESI", "EMPLOYEE ESIC"),
                        GetAmount(employee, "P TAX", "PTAX", "PROFESSIONAL TAX"),
                        GetAmount(employee, "TOTAL_DEDUCTION"),
                        GetAmount(employee, "MONTHLY EXPENCE", "MONTHLY EXPENSE"),
                        GetAmount(employee, "NET_PAY")
                    };
                    for (int i = 0; i < amounts.Length; i++)
                        sheet.Cell(currentRow, i + 6).Value = amounts[i];
                    currentRow++;
                }

                sheet.Range(currentRow, 1, currentRow, 4).Merge().Value = "GRAND TOTAL";
                for (int column = 5; column <= headers.Length; column++)
                {
                    sheet.Cell(currentRow, column).FormulaA1 =
                        $"SUM({sheet.Cell(headerRow + 1, column).Address}:{sheet.Cell(currentRow - 1, column).Address})";
                }
                var totals = sheet.Range(currentRow, 1, currentRow, headers.Length);
                totals.Style.Font.Bold = true;
                totals.Style.Fill.BackgroundColor = XLColor.LightGreen;

                sheet.Range(headerRow + 1, 5, currentRow, 5).Style.NumberFormat.Format = "0.##";
                sheet.Range(headerRow + 1, 6, currentRow, headers.Length).Style.NumberFormat.Format = "#,##0.00";
                var used = sheet.Range(headerRow, 1, currentRow, headers.Length);
                used.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                used.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                sheet.Range(headerRow, 1, currentRow - 1, headers.Length).SetAutoFilter();
                sheet.SheetView.FreezeRows(headerRow);
                sheet.SheetView.FreezeColumns(3);
                sheet.Column(1).Width = 8;
                sheet.Column(2).Width = 18;
                sheet.Column(3).Width = 32;
                sheet.Columns(4, headers.Length).Width = 18;
                sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                sheet.PageSetup.FitToPages(1, 0);
                sheet.PageSetup.SetRowsToRepeatAtTop(headerRow, headerRow);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
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
            string UserId = Session["UserId"].ToString();
            DataTable dt = clsDatabase.fnDataTable("PRC_SaaS_Employee_CTC_REPORT",TenantId, UserId);

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

            return Json(new {columns,rows}, JsonRequestBehavior.AllowGet);
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
                                                         payrollProcessEmployeeIds,TenantId);

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
                                if (isWaived || previewPayValue <= 0)
                                    continue;

                                //var prevDate = new DateTime(currentYear, currentMonth, 1).AddMonths(-1);
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
                DataTable dtValidation = clsDatabase.fnDataTable("SP_Payroll_GetEmployeeStatusesForValidation",model.payrollProcessEmployeeIds,TenantId);

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
                string UserId = Session["UserId"].ToString(); 

                DataSet ds = clsDatabase.fnDataSet("SP_Payroll_SalaryRegister_Grid", TenantId, month, year, companyId, payGroupId, search, status, 1, 999999, UserId);

                 
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
        
        private string GetApplicationBaseUrl()
        {
            var configuredUrl = ConfigurationManager.AppSettings["PayslipBaseUrl"];
            if (!string.IsNullOrWhiteSpace(configuredUrl))
            {
                configuredUrl = configuredUrl.Trim();
                return configuredUrl.EndsWith("/") ? configuredUrl : configuredUrl + "/";
            }

            var request = System.Web.HttpContext.Current?.Request;
            if (request?.Url == null)
                return "https://payroll.iecsl.in/";

            var appPath = request.ApplicationPath;
            if (string.IsNullOrWhiteSpace(appPath) || appPath == "/")
                appPath = string.Empty;

            return request.Url.GetLeftPart(UriPartial.Authority) + appPath + "/";
        }

        private void LogBulkPayslipDebug(string step, string message)
        {
            try
            {
                clsDatabase.fnErrorLog(step, message);
                System.Diagnostics.Debug.WriteLine($"{step}: {message}");
            }
            catch
            {
                // Do not allow debug logging to stop payslip processing.
            }
        }

        private string GetPayslipMailSubject(PayslipViewModel model, string firstName, string email, string monthName, int year)
        {
            var template = ConfigurationManager.AppSettings["PayslipMailSubject"];
            if (string.IsNullOrWhiteSpace(template))
                template = "Payslip for {MonthName} {Year}";

            return RenderPayslipMailTemplate(template, model, firstName, email, monthName, year);
        }

        private string GetPayslipMailBody(PayslipViewModel model, string firstName, string email, string monthName, int year)
        {
            var template = ConfigurationManager.AppSettings["PayslipMailBodyHtml"];
            if (string.IsNullOrWhiteSpace(template))
            {
                template =
                    "Dear {FirstName},<br/><br/>" +
                    "Please find your payslip for <b>{MonthName} {Year}</b> attached.<br/><br/>" +
                    "Regards,<br/>Payroll Team";
            }

            return RenderPayslipMailTemplate(template, model, firstName, email, monthName, year);
        }

        private string RenderPayslipMailTemplate(string template, PayslipViewModel model, string firstName, string email, string monthName, int year)
        {
            if (template == null)
                template = string.Empty;

            string employeeName = model?.EmpName ?? firstName ?? string.Empty;
            string employeeCode = model?.EmpCode ?? string.Empty;
            string displayFirstName = string.IsNullOrWhiteSpace(firstName) ? employeeName : firstName;

            return template
                .Replace("{FirstName}", displayFirstName)
                .Replace("{EmployeeName}", employeeName)
                .Replace("{EmployeeCode}", employeeCode)
                .Replace("{Email}", email ?? string.Empty)
                .Replace("{MonthName}", monthName ?? string.Empty)
                .Replace("{Year}", year.ToString(CultureInfo.InvariantCulture))
                .Replace("{MonthYear}", $"{monthName} {year}");
        }

        private bool TryReservePayslipMail(string tenantId, int employeeId, int month, int year, string toEmail)
        {
            try
            {
                DataTable dt = clsDatabase.fnDataTable(
                    "SP_Saas_PayslipMailSendLog_TryBegin",
                    tenantId,
                    employeeId,
                    month,
                    year,
                    toEmail ?? string.Empty);

                if (dt == null || dt.Rows.Count == 0 || !dt.Columns.Contains("CanSend"))
                    return true;

                return Convert.ToInt32(dt.Rows[0]["CanSend"]) == 1;
            }
            catch (Exception ex)
            {
                clsDatabase.fnErrorLog("PayslipMailReserveError", ex.ToString());
                return false;
            }
        }

        private void MarkPayslipMailSent(string tenantId, int employeeId, int month, int year)
        {
            try
            {
                clsDatabase.fnDBOperation(
                    "SP_Saas_PayslipMailSendLog_Complete",
                    tenantId,
                    employeeId,
                    month,
                    year,
                    "Sent",
                    string.Empty);
            }
            catch (Exception ex)
            {
                clsDatabase.fnErrorLog("PayslipMailSentLogError", ex.ToString());
            }
        }

        private void MarkPayslipMailFailed(string tenantId, int employeeId, int month, int year, string errorMessage)
        {
            try
            {
                clsDatabase.fnDBOperation(
                    "SP_Saas_PayslipMailSendLog_Complete",
                    tenantId,
                    employeeId,
                    month,
                    year,
                    "Failed",
                    errorMessage ?? string.Empty);
            }
            catch (Exception ex)
            {
                clsDatabase.fnErrorLog("PayslipMailFailedLogError", ex.ToString());
            }
        }

        private void SendBulkPayslips(string payrollProcessEmployeeIds,int month,int year,string tenantId,string applicationBaseUrl, bool forceSend = false)
        {
            try
            {
                LogBulkPayslipDebug(
                    "BulkPayslipStarted",
                    $"Ids={payrollProcessEmployeeIds}; Month={month}; Year={year}; TenantId={tenantId}; BaseUrl={applicationBaseUrl}; ForceSend={forceSend}");

                DataTable dtValidation =
                    clsDatabase.fnDataTable(
                        "SP_Payroll_GetEmployeeStatusesForValidation",
                        payrollProcessEmployeeIds,
                        tenantId);

                if (dtValidation == null || dtValidation.Rows.Count == 0)
                {
                    LogBulkPayslipDebug(
                        "BulkPayslipNoRows",
                        $"No employees returned by SP_Payroll_GetEmployeeStatusesForValidation. Ids={payrollProcessEmployeeIds}");

                    return;
                }

                LogBulkPayslipDebug(
                    "BulkPayslipRows",
                    $"EmployeeCount={dtValidation.Rows.Count}; Ids={payrollProcessEmployeeIds}");

                string monthName =
                    CultureInfo.CurrentCulture
                        .DateTimeFormat
                        .GetMonthName(month);

                // ------------------------------------------------------------
                // Create synthetic HTTP context because this runs in background
                // ------------------------------------------------------------

                var request =
                    new HttpRequest(
                        "",
                        applicationBaseUrl,
                        "");

                var response =
                    new HttpResponse(
                        new StringWriter());

                var httpContext =
                    new HttpContext(
                        request,
                        response);

                System.Web.HttpContext.Current =
                    httpContext;

                var contextWrapper =
                    new HttpContextWrapper(
                        httpContext);


                // ------------------------------------------------------------
                // Resolve Tenant
                // ------------------------------------------------------------

                string resolvedTenantId =
                    tenantId;

                if (string.IsNullOrWhiteSpace(resolvedTenantId))
                {
                    if (System.Web.HttpContext.Current?
                            .Session?["TenantID"] != null)
                    {
                        resolvedTenantId =
                            System.Web.HttpContext.Current
                                .Session["TenantID"]
                                .ToString();
                    }
                }

                if (string.IsNullOrWhiteSpace(resolvedTenantId))
                {
                    LogBulkPayslipDebug(
                        "BulkPayslipTenantMissing",
                        $"Ids={payrollProcessEmployeeIds}; Month={month}; Year={year}");

                    throw new Exception(
                        "TenantId not available for payslip generation.");
                }


                // ------------------------------------------------------------
                // Create Payslip controller
                // ------------------------------------------------------------

                var payslipCtrl =
                    new PayslipController
                    {
                        ManualTenantId =
                            resolvedTenantId,

                        ViewData =
                            new ViewDataDictionary(),

                        TempData =
                            new TempDataDictionary()
                    };


                var routeData =
                    new RouteData();

                routeData.Values["controller"] =
                    "Payslip";

                routeData.Values["action"] =
                    "Index";


                var controllerContext =
                    new ControllerContext(
                        contextWrapper,
                        routeData,
                        payslipCtrl);

                payslipCtrl.ControllerContext =
                    controllerContext;


                // ------------------------------------------------------------
                // Services
                // ------------------------------------------------------------

                var mailService =
                    new MailService();

                var pdfService =
                    new PayslipPdfService();


                // ------------------------------------------------------------
                // Process each employee SEQUENTIALLY
                // ------------------------------------------------------------

                foreach (DataRow dr in dtValidation.Rows)
                {
                    bool mailReserved =
                        false;

                    int empId =
                        0;

                    string email =
                        string.Empty;


                    try
                    {
                        // ----------------------------------------------------
                        // Employee details
                        // ----------------------------------------------------

                        empId =
                            Convert.ToInt32(
                                dr["EmployeeId"]);

                        email =
                            dr["empemail"]?
                                .ToString();

                        string firstName =
                            dr["empfirstname"]?
                                .ToString();


                        if (string.IsNullOrWhiteSpace(email))
                        {
                            LogBulkPayslipDebug(
                                "BulkPayslipSkippedNoEmail",
                                $"EmployeeId={empId}; Month={monthName}; Year={year}");

                            continue;
                        }


                        // ----------------------------------------------------
                        // Reserve email
                        // ----------------------------------------------------

                        if (!forceSend)
                        {
                            mailReserved =
                                TryReservePayslipMail(
                                    resolvedTenantId,
                                    empId,
                                    month,
                                    year,
                                    email);


                            if (!mailReserved)
                            {
                                LogBulkPayslipDebug(
                                    "BulkPayslipMailDuplicateSkipped",
                                    $"EmployeeId={empId}; Email={email}; Month={monthName}; Year={year}; TenantId={resolvedTenantId}");

                                continue;
                            }
                        }
                        else
                        {
                            mailReserved =
                                true;

                            LogBulkPayslipDebug(
                                "BulkPayslipMailForceSend",
                                $"EmployeeId={empId}; Email={email}; Month={monthName}; Year={year}; TenantId={resolvedTenantId}");
                        }


                        LogBulkPayslipDebug(
                            "BulkPayslipEmployeeStart",
                            $"EmployeeId={empId}; Email={email}; Month={monthName}; Year={year}");


                        // ----------------------------------------------------
                        // 1. Get payslip model
                        // ----------------------------------------------------

                        PayslipViewModel model =
                            payslipCtrl.GetPayslipData(
                                empId,
                                monthName,
                                year,
                                true,
                                1);


                        if (model == null ||
                            model.EmpId == 0)
                        {
                            MarkPayslipMailFailed(
                                resolvedTenantId,
                                empId,
                                month,
                                year,
                                "Payslip model not available.");

                            LogBulkPayslipDebug(
                                "BulkPayslipSkippedNoModel",
                                $"EmployeeId={empId}; Email={email}; Month={monthName}; Year={year}");

                            continue;
                        }


                        model.isPdf =
                            true;


                        // ----------------------------------------------------
                        // 2. Determine Razor view
                        // ----------------------------------------------------

                        string viewName =
                            payslipCtrl.GetPayslipViewName(
                                model.TenantId);


                        if (string.IsNullOrWhiteSpace(viewName))
                        {
                            viewName =
                                "Index";
                        }


                        LogBulkPayslipDebug(
                            "BulkPayslipPdfStart",
                            $"EmployeeId={empId}; Email={email}; EmpCode={model.EmpCode}; ViewName={viewName}; Month={monthName}; Year={year}");


                        // ----------------------------------------------------
                        // 3. Safe PDF filename
                        // ----------------------------------------------------

                        string safeEmpCode =
                            MakeSafePayslipFileName(
                                model.EmpCode);

                        string pdfFileName =
                            $"Payslip_{safeEmpCode}_{monthName}_{year}.pdf";


                        // ----------------------------------------------------
                        // 4. Generate PDF WITH PUPPETEERSHARP
                        // ----------------------------------------------------

                        byte[] pdfBytes =
                            pdfService
                                .GeneratePayslipPdf(
                                    controllerContext,
                                    viewName,
                                    model,
                                    pdfFileName);


                        if (pdfBytes == null ||
                            pdfBytes.Length == 0)
                        {
                            throw new Exception(
                                "PuppeteerSharp generated empty PDF.");
                        }


                        LogBulkPayslipDebug(
                            "BulkPayslipPdfGenerated",
                            $"EmployeeId={empId}; Email={email}; EmpCode={model.EmpCode}; PdfBytes={pdfBytes.Length}");


                        // ----------------------------------------------------
                        // 5. Prepare email
                        // ----------------------------------------------------

                        string subject =
                            GetPayslipMailSubject(
                                model,
                                firstName,
                                email,
                                monthName,
                                year);


                        string bodyHtml =
                            GetPayslipMailBody(
                                model,
                                firstName,
                                email,
                                monthName,
                                year);


                        // ----------------------------------------------------
                        // 6. Send email
                        // ----------------------------------------------------

                        bool sent =
                            mailService.SendMails(
                                toEmail: email,
                                fromEmail: "",
                                bodyHtml: bodyHtml,
                                subject: subject,
                                attachmentBytes: pdfBytes,
                                fileNameWithoutExt:
                                    $"Payslip_{monthName}_{year}",
                                tenantId:
                                    resolvedTenantId,
                                mailPurpose:
                                    "Payslip",
                                profileName:
                                    "Payroll",
                                templateValues:
                                    new Dictionary<string, string>
                                    {
                                {
                                    "FirstName",
                                    string.IsNullOrWhiteSpace(firstName)
                                        ? model.EmpName
                                        : firstName
                                },
                                {
                                    "EmployeeName",
                                    model.EmpName ?? string.Empty
                                },
                                {
                                    "EmployeeCode",
                                    model.EmpCode ?? string.Empty
                                },
                                {
                                    "Email",
                                    email ?? string.Empty
                                },
                                {
                                    "MonthName",
                                    monthName ?? string.Empty
                                },
                                {
                                    "Year",
                                    year.ToString(
                                        CultureInfo.InvariantCulture)
                                },
                                {
                                    "MonthYear",
                                    $"{monthName} {year}"
                                }
                                    });


                        // ----------------------------------------------------
                        // 7. Handle send result
                        // ----------------------------------------------------

                        if (!sent)
                        {
                            MarkPayslipMailFailed(
                                resolvedTenantId,
                                empId,
                                month,
                                year,
                                "MailService.SendMails returned false.");

                            LogBulkPayslipDebug(
                                "BulkPayslipMailFailed",
                                $"EmployeeId={empId}; Email={email}; Subject={subject}; PdfBytes={pdfBytes.Length}");

                            continue;
                        }


                        MarkPayslipMailSent(
                            resolvedTenantId,
                            empId,
                            month,
                            year);


                        LogBulkPayslipDebug(
                            "BulkPayslipMailSent",
                            $"EmployeeId={empId}; Email={email}; Subject={subject}; PdfBytes={pdfBytes.Length}");
                    }
                    catch (Exception ex)
                    {
                        if (mailReserved &&
                            empId > 0)
                        {
                            MarkPayslipMailFailed(
                                resolvedTenantId,
                                empId,
                                month,
                                year,
                                ex.Message);
                        }


                        clsDatabase.fnErrorLog(
                            "BulkPayslipLoopError",
                            $"EmployeeId={empId}; " +
                            $"Email={email}; " +
                            $"Month={monthName}; " +
                            $"Year={year}; " +
                            $"Error={ex}");
                    }
                }


                LogBulkPayslipDebug(
                    "BulkPayslipCompleted",
                    $"Ids={payrollProcessEmployeeIds}; Month={monthName}; Year={year}; TenantId={resolvedTenantId}");
            }
            catch (Exception ex)
            {
                clsDatabase.fnErrorLog(
                    "BulkPayslipMainError",
                    ex.ToString());
            }
        }

        private string MakeSafePayslipFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Employee";

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '_');
            }

            return value.Trim();
        }

    }
}
