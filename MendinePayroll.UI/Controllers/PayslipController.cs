using ClosedXML.Excel;
using Common.Utility;
using MendinePayroll.UI.Models;
using Newtonsoft.Json;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Mvc;
using UI.BLL;
//using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MendinePayroll.UI.Controllers
{
    public class PayslipController : Controller
    {
        // ✅ 1. ADD THIS LINE HERE
        public string ManualTenantId { get; set; }
        private string TenantId
        {
            get
            {
                // 1️⃣ Background jobs
                if (!string.IsNullOrEmpty(ManualTenantId))
                    return ManualTenantId;

                // 2️⃣ Web request via querystring
                if (System.Web.HttpContext.Current?.Request?.QueryString["tenantId"] != null)
                    return System.Web.HttpContext.Current.Request.QueryString["tenantId"];

                // 3️⃣ Normal logged-in web session
                if (System.Web.HttpContext.Current?.Session?["TenantID"] != null)
                    return System.Web.HttpContext.Current.Session["TenantID"].ToString();

                throw new Exception("TenantID not found in Manual property, QueryString, or Session.");
            }
        }


        // GET: Payslip
        public ActionResult Index(int empId, string month, int year)
        {
            try
            {
                //clsDatabase.fnErrorLog(
                //    "PayslipIndexStart",
                //    $"tenantId={Request?.QueryString["tenantId"]}; empId={empId}; month={month}; year={year}; url={Request?.RawUrl}");

                var model = GetPayslipData(empId, month, year, false, 0);

                string viewName = GetPayslipViewName(model.TenantId);

                return View(viewName, model);
            }
            catch (Exception ex)
            {
                clsDatabase.fnErrorLog(
                    "PayslipIndexError",
                    $"tenantId={Request?.QueryString["tenantId"]}; empId={empId}; month={month}; year={year}; url={Request?.RawUrl}; error={ex}");

                return Content("Payslip view failed. Please check server ErrorLog.txt for PayslipIndexError.");
            }
        }
        internal string GetPayslipViewName(string tenantId)
        {
            string viewName = clsDatabase.fnDBOperation(
                "SP_GetPayslipViewName",
                tenantId);

            if (string.IsNullOrWhiteSpace(viewName))
                viewName = "Index";

            return viewName;
        }
        public ActionResult HrPreview(int empId, string month, int year)
        {
            try
            {
                clsDatabase.fnErrorLog(
                    "PayslipHrPreviewStart",
                    $"tenantId={Request?.QueryString["tenantId"]}; empId={empId}; month={month}; year={year}; url={Request?.RawUrl}");

                var model = GetPayslipData(empId, month, year, false, 1);
                string viewName = GetPayslipViewName(model.TenantId);

                return View(viewName, model);
            }
            catch (Exception ex)
            {
                clsDatabase.fnErrorLog(
                    "PayslipHrPreviewError",
                    $"tenantId={Request?.QueryString["tenantId"]}; empId={empId}; month={month}; year={year}; url={Request?.RawUrl}; error={ex}");

                return Content("Payslip preview failed. Please check server ErrorLog.txt for PayslipHrPreviewError.");
            }
        }
 
        public static string NumberToWords(long number)
        {
            if (number == 0) return "Zero";

            if (number < 0)
                return "Minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                var unitsMap = new[]
                {
                "Zero","One","Two","Three","Four","Five","Six","Seven","Eight","Nine","Ten",
                "Eleven","Twelve","Thirteen","Fourteen","Fifteen","Sixteen","Seventeen","Eighteen","Nineteen"
            };

                var tensMap = new[]
                {
                "Zero","Ten","Twenty","Thirty","Forty","Fifty","Sixty","Seventy","Eighty","Ninety"
            };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }
        public ActionResult Downloadpdf(
    int empId,
    string month,
    int year)
        {
            PayslipViewModel model = null;

            try
            {
                clsDatabase.fnErrorLog(
                    "PayslipDownloadPdfStart",
                    $"tenantId={TenantId}; " +
                    $"empId={empId}; " +
                    $"month={month}; " +
                    $"year={year}; " +
                    $"url={Request?.RawUrl}"
                );

                // ---------------------------------------------
                // 1. Get payslip data
                // ---------------------------------------------
                model = GetPayslipData(
                    empId,
                    month,
                    year,
                    true,
                    1
                );

                if (model == null)
                    throw new Exception("Payslip data could not be loaded.");

                if (model.EmpId == 0)
                    throw new Exception("Employee payslip not found.");

                model.isPdf = true;

                // ---------------------------------------------
                // 2. Get tenant-specific Razor view
                // ---------------------------------------------
                string viewName =
                    GetPayslipViewName(model.TenantId);

                if (string.IsNullOrWhiteSpace(viewName))
                    viewName = "Index";

                // ---------------------------------------------
                // 3. File name
                // ---------------------------------------------
                string safeEmployeeName =
                    MakeSafeFileName(
                        string.IsNullOrWhiteSpace(model.EmpName)
                            ? model.EmpCode
                            : model.EmpName
                    );

                string fileName =
                    $"Payslip_{safeEmployeeName}_{month}_{year}.pdf";

                // ---------------------------------------------
                // 4. Generate PDF using PuppeteerSharp
                // ---------------------------------------------
                var pdfService =
                    new PayslipPdfService();

                byte[] pdfBytes =
                     pdfService.GeneratePayslipPdf(
                        this.ControllerContext,
                        viewName,
                        model,
                        fileName
                    );

                if (pdfBytes == null ||
                    pdfBytes.Length == 0)
                {
                    throw new Exception(
                        "PuppeteerSharp generated an empty PDF."
                    );
                }

                clsDatabase.fnErrorLog(
                    "PayslipDownloadPdfGenerated",
                    $"tenantId={model.TenantId}; " +
                    $"empId={empId}; " +
                    $"month={month}; " +
                    $"year={year}; " +
                    $"viewName={viewName}; " +
                    $"bytes={pdfBytes.Length}"
                );

                return File(
                    pdfBytes,
                    "application/pdf",
                    fileName
                );
            }
            catch (Exception ex)
            {
                clsDatabase.fnErrorLog(
                    "PayslipDownloadPdfError",
                    $"tenantId={model?.TenantId ?? TenantId}; " +
                    $"empId={empId}; " +
                    $"month={month}; " +
                    $"year={year}; " +
                    $"url={Request?.RawUrl}; " +
                    $"error={ex}"
                );

                return Content(
                    "Payslip PDF download failed. " +
                    "Please check server ErrorLog.txt for PayslipDownloadPdfError."
                );
            }
        }
        private string MakeSafeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "Employee";

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName.Trim();
        }

        //public ActionResult Downloadpdf(int empId, string month, int year)
        // {
        //     try
        //     {
        //         //clsDatabase.fnErrorLog(
        //         //    "PayslipDownloadPdfStart",
        //         //    $"tenantId={TenantId}; empId={empId}; month={month}; year={year}; url={Request?.RawUrl}");


        //         var model = GetPayslipData(empId, month, year, true, 1);
        //         string viewName = GetPayslipViewName(model.TenantId);

        //         var pdfService = new PayslipPdfService();

        //         byte[] pdfBytes = pdfService.GeneratePayslipPdf(
        //             this.ControllerContext,
        //             viewName,
        //             model,
        //             $"Payslip_{model.EmpName}_{month}_{year}.pdf"
        //         );
        //         if (pdfBytes == null || pdfBytes.Length == 0)
        //             throw new Exception("Payslip PDF generation failed.");

        //         clsDatabase.fnErrorLog(
        //             "PayslipDownloadPdfGenerated",
        //             $"tenantId={TenantId}; empId={empId}; month={month}; year={year}; bytes={pdfBytes.Length}");

        //         return File(pdfBytes, "application/pdf", $"Payslip_{model.EmpName}_{month}_{year}.pdf");
        //     }
        //     catch (Exception ex)
        //     {
        //         clsDatabase.fnErrorLog(
        //             "PayslipDownloadPdfError",
        //             $"tenantId={Request?.QueryString["tenantId"]}; empId={empId}; month={month}; year={year}; url={Request?.RawUrl}; error={ex}");
        //         return Content("Payslip PDF download failed. Please check server ErrorLog.txt for PayslipDownloadPdfError.");
        //     }
        // }

        private PayslipViewModel CreateUnavailablePayslipModel(string month, int year, bool isPdf, string message)
        {
            return new PayslipViewModel
            {
                Month = month,
                Year = year,
                isPdf = isPdf,
                UnavailableMessage = message
            };
        }

        private bool IsPayslipPaid(int empId, string month, int year)
        {
            int monthNumber = GetMonthNumber(month);
            DataTable dt = clsDatabase.fnDataTable(
                "SP_Payslip_CheckPaidStatus",
                TenantId,
                empId,
                monthNumber,
                year
            );

            if (dt == null || dt.Rows.Count == 0 || !dt.Columns.Contains("PaidStatus"))
                return false;

            object paidStatus = dt.Rows[0]["PaidStatus"];
            return paidStatus != DBNull.Value && Convert.ToInt32(paidStatus) == 1;
        }

        private int GetMonthNumber(string month)
        {
            if (int.TryParse(month, out int monthNumber))
                return monthNumber;

            return DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture).Month;
        }
    //internal PayslipViewModel GetPayslipData(int EmpId, string Month, int Year,bool ispdf)
        //{
        //    var model = new PayslipViewModel
        //    {
        //        Month = Month,
        //        Year = Year,
        //        isPdf = ispdf
        //    };
        //    // Convert Month Name to Month Number
        //    int monthNum = DateTime.ParseExact(Month,"MMMM",System.Globalization.CultureInfo.InvariantCulture).Month;

        //    string spName;

        //    // Cutoff logic
        //    if (Year <= 2025 || (Year == 2025 && monthNum >= 12))
        //        spName = "Essp.dbo.PRC_Get_PayslipFullData";
        //    DataSet ds1 = clsDatabase.fnDataSet("Essp.dbo.PRC_Get_PayslipFullData", EmpId, Month, Year, TenantId)
        //        if (ds1 != null && ds1.Tables.Count > 0)
        //    {
        //        // 1. Company Info
        //        if (ds1.Tables[0].Rows.Count > 0)
        //        {
        //            var row = ds1.Tables[0].Rows[0];
        //            model.CompanyName = row["Company"].ToString();
        //            model.CompanyLogoUrl = row["CompanyLogoUrl"].ToString();
        //            model.CompanyAddressLine1 = row["Address1"].ToString();
        //            model.CompanyAddressLine2 = row["Address2"].ToString();
        //        }

        //        // 2. Employee Info
        //        if (ds1.Tables.Count > 1 && ds1.Tables[1].Rows.Count > 0)
        //        {
        //            var row = ds1.Tables[1].Rows[0];
        //            model.EmpId = Convert.ToInt32(row["empid"]);
        //            model.EmpNo = row["empno"].ToString();
        //            model.EmpCode = row["empcode"].ToString();
        //            model.EmpName = row["EmployeeName"].ToString();
        //            model.Designation = row["Designation"].ToString();
        //            model.Department = row["Department"].ToString();
        //            model.DOJ = Convert.ToDateTime(row["joining"]);
        //            model.PAN = row["emppancardno"].ToString();
        //            model.UAN = row["empuanno"].ToString();
        //            model.PFNo = row["empepfno"].ToString();
        //            model.ESICNo = row["empesicno"].ToString();
        //            model.BankAccount = row["empaccountno"].ToString();
        //            model.BankName = row["bankname"].ToString();
        //        }
        //        // 3. Attendance Summary
        //        var SalesEmpNo = clsDatabase.fnDBOperation("PROC_GETEMPNO_SALES", EmpId);
        //        if (!string.IsNullOrEmpty(SalesEmpNo) && SalesEmpNo != "0")
        //        {
        //            DataTable officeTable = clsDatabase.fnDataTable("proc_dailyattendance", Month, Year, SalesEmpNo, "");
        //            // Step 2: Fetch API data (Sales)
        //            List<AttendanceLog> apiLogs = new List<AttendanceLog>();
        //            if (!string.IsNullOrEmpty(SalesEmpNo) && SalesEmpNo != "0")
        //            {
        //                using (var client = new HttpClient())
        //                {
        //                    string url = $"https://crmfieldforceapi.mendine.co.in/api/crm/HRMSApi/EmployeeAttendanceSheet?Businessid=MEND-PVTL-890&Employeeno={SalesEmpNo}&Month={Month}&Year={Year}";
        //                    var response = client.GetStringAsync(url).Result;



        //                    //apiLogs = JsonSerializer.Deserialize<List<AttendanceLog>>(response,
        //                    //    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        //                    apiLogs = JsonConvert.DeserializeObject<List<AttendanceLog>>(response);

        //                    // Log response
        //                    System.IO.File.WriteAllText(
        //                         System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AttendanceApiLog.txt"),
        //                         JsonConvert.SerializeObject(apiLogs, Formatting.Indented)
        //                     );

        //                }

        //            }
        //            // Step 3: Merge sources if Hybrid
        //            int totalDays = 0, presentDays = 0, leaveDays = 0;

        //            int monthNumber = DateTime.ParseExact(Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;
        //            int yearNumber = Convert.ToInt32(Year);

        //            var calendarDays = Enumerable.Range(1, DateTime.DaysInMonth(yearNumber, monthNumber))
        //                                         .Select(d => new DateTime(yearNumber, monthNumber, d));

        //            foreach (var day in calendarDays)
        //            {
        //                totalDays++;

        //                // ✅ check from proc_dailyattendance (office punches)
        //                bool officePresent = officeTable?.AsEnumerable().Any(r =>
        //                    Convert.ToDateTime(r["LOGDATE"]).Date == day.Date &&
        //                    !string.IsNullOrWhiteSpace(r["INTIME"]?.ToString()) &&
        //                    !string.IsNullOrWhiteSpace(r["OUTTIME"]?.ToString()) &&
        //                    r["DURATION"] != DBNull.Value &&
        //                    r["DURATION"].ToString() != "00:00:00"
        //                ) ?? false;

        //                var apiDay = apiLogs.FirstOrDefault(l =>
        //                {
        //                    if (string.IsNullOrWhiteSpace(l.LogDate))
        //                        return false;

        //                    DateTime parsed;
        //                    if (!DateTime.TryParseExact(l.LogDate,
        //                        new[] { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy HH:mm:ss" },
        //                        System.Globalization.CultureInfo.InvariantCulture,
        //                        System.Globalization.DateTimeStyles.None,
        //                        out parsed))
        //                        return false;

        //                    return parsed.Date == day.Date;
        //                });

        //                bool apiPresent = apiDay != null && !string.IsNullOrWhiteSpace(apiDay.CheckIn);
        //                bool isLeave = apiDay != null && apiDay.WorkType?.Trim().ToUpper() == "LEAVE";

        //                if (isLeave)
        //                    leaveDays++;
        //                else if (officePresent || apiPresent)
        //                    presentDays++;
        //                else
        //                    leaveDays++;
        //            }
        //            // Assign
        //            model.TotalDays = totalDays;
        //            model.WorkedDays = presentDays;  // or adjust with policy
        //            model.LeaveTaken = leaveDays;
        //            if (ds1.Tables.Count > 2 && ds1.Tables[2].Rows.Count > 0)
        //            {
        //                var dsRow = ds1.Tables[2].Rows[0];
        //                model.TotalDays = Convert.ToInt32(dsRow["TotalDays"]);
        //                model.PayableDays = Convert.ToInt32(dsRow["TotalDays"]);
        //                model.LeaveTaken = Convert.ToInt32(dsRow["TotalLeave"]);
        //            }

        //        }
        //        else
        //        {
        //            // 3. Attendance Summary For except Sales Employee
        //            if (ds1.Tables.Count > 2 && ds1.Tables[2].Rows.Count > 0)
        //            {
        //                var row = ds1.Tables[2].Rows[0];
        //                model.TotalDays = Convert.ToInt32(row["TotalDays"]);
        //                model.WorkedDays = Convert.ToInt32(row["WorkedDays"]);
        //                model.LOPDays = 0;
        //                model.PayableDays = Convert.ToInt32(row["TotalDays"]); // or DaysPayable
        //                model.LeaveTaken = Convert.ToInt32(row["TotalLeave"]);
        //            }
        //        }
        //        // Step 1: Fetch office data (DB)
        //        //DataTable officeTable = ds.Tables.Count > 2 ? ds.Tables[2] : null;

        //        // 4. Leave Balances
        //        if (ds1.Tables.Count > 3)
        //        {
        //            foreach (DataRow row in ds1.Tables[3].Rows)
        //            {
        //                model.LeaveBalances.Add(new LeaveBalance
        //                {
        //                    CodeType = row["CodeType"].ToString(),
        //                    Balance = Convert.ToDecimal(row["LeaveBalance"])
        //                });
        //            }
        //        }
        //        // 5. Earnings (Table 4) → skip 0 and Gross Amount
        //        if (ds1.Tables.Count > 4)
        //        {
        //            var earningsList = new List<SalaryHead>();

        //            foreach (DataRow row in ds1.Tables[4].Rows)
        //            {
        //                decimal amount = Convert.ToDecimal(row["Amount"]);
        //                string name = row["PayConfigName"].ToString();

        //                if (amount == 0) continue;
        //                if (name.Equals("Gross Amount", StringComparison.OrdinalIgnoreCase)) continue;

        //                earningsList.Add(new SalaryHead
        //                {
        //                    PayConfigName = ToTitleCase(name),
        //                    Amount = amount
        //                });
        //            }

        //            // ✅ Priority Order: Basic > Stipend > Others
        //            model.Earnings = earningsList
        //                .OrderBy(e =>
        //                    e.PayConfigName.IndexOf("BASIC", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 :
        //                    e.PayConfigName.IndexOf("STIPEND", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 2
        //                )
        //                .ThenBy(e => e.PayConfigName) // optional: alphabetical for the rest
        //                .ToList();
        //        }

        //        // 6. Deductions (Table 5) → skip 0 values
        //        if (ds1.Tables.Count > 5)
        //        {
        //            foreach (DataRow row in ds1.Tables[5].Rows)
        //            {
        //                decimal amount = Convert.ToDecimal(row["Amount"]);
        //                if (amount == 0) continue;

        //                model.Deductions.Add(new SalaryHead
        //                {
        //                    PayConfigName = ToTitleCase(row["PayConfigName"].ToString()),
        //                    Amount = amount
        //                });
        //            }
        //        }

        //        //7. Net Pay 
        //        if (ds1.Tables.Count > 5 && ds1.Tables[6].Rows.Count > 0)
        //        {
        //            var row = ds1.Tables[6].Rows[0];
        //            model.NetPay = Convert.ToDecimal(row["NetPay"]);
        //        }
        //        //7. Loan 
        //        // 7. Loan (only if SP returned a loan dataset with values)
        //        if (ds1.Tables.Count > 7 && ds1.Tables[7] != null && ds1.Tables[7].Rows.Count > 0)
        //        {
        //            var row = ds1.Tables[7].Rows[0];

        //            model.LoanInstallment = row["LoanInstallment"] == DBNull.Value ? 0
        //                                    : Math.Round(Convert.ToDecimal(row["LoanInstallment"]), 0);

        //            model.OpeningLoan = row["OpeningAmount"] == DBNull.Value ? 0
        //                                    : Math.Round(Convert.ToDecimal(row["OpeningAmount"]), 0);

        //            model.ClosingLoan = row["ClosingAmount"] == DBNull.Value ? 0
        //                                    : Math.Round(Convert.ToDecimal(row["ClosingAmount"]), 0);
        //        }

        //        // Totals
        //        var grossRow = model.Earnings
        //            .FirstOrDefault(x => x.PayConfigName.Equals("Gross Amount", StringComparison.OrdinalIgnoreCase));
        //        model.GrossEarnings = grossRow != null ? grossRow.Amount : model.Earnings.Sum(x => x.Amount);
        //        model.TotalDeductions = model.Deductions.Sum(x => x.Amount);
        //        //model.NetPay = model.GrossEarnings - model.TotalDeductions;
        //        model.NetPayWords = NumberToWords((long)model.NetPay);
        //    }

        //    return model;
        //    else
        //        spName = "PRC_Get_PayslipFullData"; 
        //        DataSet ds = clsDatabase.fnDataSet("PRC_Get_PayslipFullData", EmpId, Month, Year, TenantId);
        //    // DataSet ds = clsDatabase.fnDataSet("PRC_Get_PayslipFullData", EmpId, Month, Year, TenantId);

        //    if (ds != null && ds.Tables.Count > 0)
        //    {
        //        // 1. Company Info
        //        if (ds.Tables[0].Rows.Count > 0)
        //        {
        //            var row = ds.Tables[0].Rows[0];
        //            model.CompanyName = row["Company"].ToString();
        //            model.CompanyLogoUrl = row["CompanyLogoUrl"].ToString();
        //            model.CompanyAddressLine1 = row["Address1"].ToString();
        //            model.CompanyAddressLine2 = row["Address2"].ToString();
        //        }

        //        // 2. Employee Info
        //        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
        //        {
        //            var row = ds.Tables[1].Rows[0];
        //            model.EmpId = Convert.ToInt32(row["empid"]);
        //            model.EmpNo = row["empno"].ToString();
        //            model.EmpCode = row["empcode"].ToString();
        //            model.EmpName = row["EmployeeName"].ToString();
        //            model.Designation = row["Designation"].ToString();
        //            model.Department = row["Department"].ToString();
        //            model.DOJ = Convert.ToDateTime(row["joining"]);
        //            model.PAN = row["emppancardno"].ToString();
        //            model.UAN = row["empuanno"].ToString();
        //            model.PFNo = row["empepfno"].ToString();
        //            model.ESICNo = row["empesicno"].ToString();
        //            model.BankAccount = row["empaccountno"].ToString();
        //            model.BankName = row["bankname"].ToString();
        //        } 
        //        // 3. Attendance Summary
        //        var SalesEmpNo = clsDatabase.fnDBOperation("PROC_GETEMPNO_SALES", EmpId);

        //        // Define your fix TenantID here
        //        string targetTenantId = "6B1B6590-C5CA-4FD6-A0BB-FEBA6DB8FB14";

        //        if (TenantId == targetTenantId && !string.IsNullOrEmpty(SalesEmpNo) && SalesEmpNo != "0")
        //        {
        //            DataTable officeTable = clsDatabase.fnDataTable("proc_dailyattendance", Month, Year, SalesEmpNo, "");
        //            List<AttendanceLog> apiLogs = new List<AttendanceLog>();

        //            // Step 2: Fetch API data (Sales Force API)
        //            using (var client = new HttpClient())
        //            {
        //                string url = $"https://crmfieldforceapi.mendine.co.in/api/crm/HRMSApi/EmployeeAttendanceSheet?Businessid=MEND-PVTL-890&Employeeno={SalesEmpNo}&Month={Month}&Year={Year}";
        //                var response = client.GetStringAsync(url).Result;
        //                apiLogs = JsonConvert.DeserializeObject<List<AttendanceLog>>(response) ?? new List<AttendanceLog>();
        //            }

        //            // Step 3: Merge sources
        //            int totalDays = 0, presentDays = 0, leaveDays = 0;

        //            int monthNumber = DateTime.ParseExact(Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;
        //            int yearNumber = Convert.ToInt32(Year);

        //            var calendarDays = Enumerable.Range(1, DateTime.DaysInMonth(yearNumber, monthNumber))
        //                                 .Select(d => new DateTime(yearNumber, monthNumber, d));

        //            foreach (var day in calendarDays)
        //            {
        //                totalDays++;

        //                // Check Office DB Punches
        //                bool officePresent = officeTable?.AsEnumerable().Any(r =>
        //                    Convert.ToDateTime(r["LOGDATE"]).Date == day.Date &&
        //                    !string.IsNullOrWhiteSpace(r["INTIME"]?.ToString()) &&
        //                    !string.IsNullOrWhiteSpace(r["OUTTIME"]?.ToString())
        //                ) ?? false;

        //                // Check API Field Punches
        //                var apiDay = apiLogs.FirstOrDefault(l => {
        //                    DateTime parsed;
        //                    return DateTime.TryParse(l.LogDate, out parsed) && parsed.Date == day.Date;
        //                });

        //                bool apiPresent = apiDay != null && !string.IsNullOrWhiteSpace(apiDay.CheckIn);
        //                bool isLeave = apiDay != null && apiDay.WorkType?.Trim().ToUpper() == "LEAVE";

        //                if (isLeave)
        //                    leaveDays++;
        //                else if (officePresent || apiPresent)
        //                    presentDays++;
        //                // If neither present nor leave, it's counted as absent/leave depending on your policy
        //            }

        //            // Final Assignment for Sales Tenant
        //            model.TotalDays = totalDays;
        //            model.WorkedDays = presentDays;
        //            model.LeaveTaken = leaveDays;
        //            model.PayableDays = totalDays - (totalDays - presentDays - leaveDays); // Adjust based on your policy
        //        }
        //        else
        //        {
        //            // Default logic for all other Tenants / Non-Sales Employees
        //            if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
        //            {
        //                var row = ds.Tables[2].Rows[0];
        //                model.TotalDays = Convert.ToInt32(row["TotalDays"]);
        //                model.WorkedDays = Convert.ToInt32(row["WorkedDays"]);
        //                model.LOPDays = row.Table.Columns.Contains("LOPDays") ? Convert.ToInt32(row["LOPDays"]) : 0;
        //                model.PayableDays = Convert.ToInt32(row["TotalDays"]);
        //                model.LeaveTaken = Convert.ToInt32(row["TotalLeave"]);
        //            }
        //        }
        //        // Step 1: Fetch office data (DB)
        //        //DataTable officeTable = ds.Tables.Count > 2 ? ds.Tables[2] : null;

        //        // 4. Leave Balances
        //        if (ds.Tables.Count > 3)
        //        {
        //            foreach (DataRow row in ds.Tables[3].Rows)
        //            {
        //                model.LeaveBalances.Add(new LeaveBalance
        //                {
        //                    CodeType = row["CodeType"].ToString(),
        //                    Balance = Convert.ToDecimal(row["LeaveBalance"])
        //                });
        //            }
        //        }
        //        // 5. Earnings (Table 4) → Side-by-Side Actual vs Earned

        //        if (ds.Tables.Count > 4)
        //        {
        //            var earningsList = new List<SalaryHead>();

        //            foreach (DataRow row in ds.Tables[4].Rows)
        //            {
        //                // Adjust column names to match your new SP output
        //                decimal actual = row.Table.Columns.Contains("ActualAmount") ? Convert.ToDecimal(row["ActualAmount"]) : 0;
        //                decimal earned = row.Table.Columns.Contains("EarnedAmount") ? Convert.ToDecimal(row["EarnedAmount"]) : 0;
        //                string name = row["AllowanceName"].ToString();

        //                // Skip if both are zero or if it's the Total row
        //                if (actual == 0 && earned == 0) continue;
        //                if (name.Equals("Gross Amount", StringComparison.OrdinalIgnoreCase)) continue;

        //                earningsList.Add(new SalaryHead
        //                {
        //                    PayConfigName = ToTitleCase(name),
        //                    ActualAmount = actual,
        //                    Amount = earned
        //                });
        //            }

        //            // ✅ Priority Order: Basic > Stipend > Others
        //            model.Earnings = earningsList
        //                .OrderBy(e =>
        //                    e.PayConfigName.IndexOf("BASIC", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 :
        //                    e.PayConfigName.IndexOf("STIPEND", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 2
        //                )
        //                .ThenBy(e => e.PayConfigName)
        //                .ToList();
        //        }

        //        // 6. Deductions (Table 5) 
        //        if (ds.Tables.Count > 5)
        //        {
        //            foreach (DataRow row in ds.Tables[5].Rows)
        //            {
        //                // Adjust column names to match your new Deduction SP output
        //                decimal configDeduction = row.Table.Columns.Contains("ConfiguredDeduction") ? Convert.ToDecimal(row["ConfiguredDeduction"]) : 0;
        //                decimal actualDeduction = row.Table.Columns.Contains("ActualDeduction") ? Convert.ToDecimal(row["ActualDeduction"]) : 0;

        //                if (configDeduction == 0 && actualDeduction == 0) continue;

        //                model.Deductions.Add(new SalaryHead
        //                {
        //                    PayConfigName = ToTitleCase(row["DeductionName"].ToString()),
        //                    ActualAmount = configDeduction, // Standard deduction
        //                    Amount = actualDeduction     // Processed deduction
        //                });
        //            }
        //        }

        //        //7. Net Pay 
        //        if (ds.Tables.Count > 5 && ds.Tables[6].Rows.Count > 0)
        //        {
        //            var row = ds.Tables[6].Rows[0];
        //            model.NetPay = Convert.ToDecimal(row["NetPay"]);
        //        }
        //        //7. Loan 
        //        // 7. Loan (only if SP returned a loan dataset with values)
        //        if (ds.Tables.Count > 7 && ds.Tables[7]?.Rows.Count > 0)
        //        {
        //            var row = ds.Tables[7].Rows[0];
        //            var cols = ds.Tables[7].Columns;

        //            // Helper function to safely parse and round decimals
        //            decimal GetSafeDecimal(DataRow r, string colName)
        //            {
        //                if (!cols.Contains(colName) || r[colName] == DBNull.Value)
        //                    return 0;
        //                return Math.Round(Convert.ToDecimal(r[colName]), 0);
        //            }

        //            model.LoanInstallment = GetSafeDecimal(row, "EMIAmount");
        //            model.OpeningLoan = GetSafeDecimal(row, "OpeningBalance");
        //            model.ClosingLoan = GetSafeDecimal(row, "ClosingBalance");
        //        }

        //        // Totals
        //        var grossRow = model.Earnings
        //            .FirstOrDefault(x => x.PayConfigName.Equals("Gross Amount", StringComparison.OrdinalIgnoreCase));
        //        model.GrossEarnings = grossRow != null ? grossRow.Amount : model.Earnings.Sum(x => x.Amount);
        //        model.TotalDeductions = model.Deductions.Sum(x => x.Amount);
        //        //model.NetPay = model.GrossEarnings - model.TotalDeductions;
        //        model.NetPayWords = NumberToWords((long)model.NetPay);
        //    }

        //    return model;
        //}
        private static DataTable FindTableWithColumn(DataSet dataSet, string columnName, int startIndex)
        {
            if (dataSet == null || dataSet.Tables.Count <= startIndex)
                return null;

            for (int i = startIndex; i < dataSet.Tables.Count; i++)
            {
                if (dataSet.Tables[i].Columns.Contains(columnName))
                    return dataSet.Tables[i];
            }

            return null;
        }

        internal PayslipViewModel GetPayslipData(int EmpId, string Month, int Year, bool ispdf, int hrTag = 0)
    {
            var model = new PayslipViewModel
            {
                Month = Month,
                Year = Year,
                isPdf = ispdf,
                TenantId=TenantId
            };

            int monthNum = DateTime.ParseExact(
                Month,
                "MMMM",
                System.Globalization.CultureInfo.InvariantCulture
            ).Month;
             
            DataSet ds = clsDatabase.fnDataSet("PRC_Get_PayslipFullData",EmpId, Month, Year, TenantId, hrTag);

            if (ds == null || ds.Tables.Count == 0)
                return model;

            //Sortorder Pay component 
            DataTable dtPayConfigSortOrder =clsDatabase.fnDataTable("PRC_Get_PayConfig_SortOrder",TenantId);

            // Create lookup: PayConfigName -> SortOrder
            var payConfigSortOrder = dtPayConfigSortOrder
                .AsEnumerable()
                .Where(r => r["PayConfigName"] != DBNull.Value)
                .GroupBy(
                    r => r["PayConfigName"].ToString().Trim(),
                    StringComparer.OrdinalIgnoreCase
                )
                .ToDictionary(
                    g => g.Key,
                    g => g.First()["SortOrder"] == DBNull.Value
                        ? 9999
                        : Convert.ToInt32(g.First()["SortOrder"]),
                    StringComparer.OrdinalIgnoreCase
                );
            // Company
            if (ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];
                model.CompanyName = row["Company"].ToString();
                model.CompanyLogoUrl = row["CompanyLogoUrl"].ToString();
                model.CompanyAddressLine1 = row["Address1"].ToString();
                model.CompanyAddressLine2 = row["Address2"].ToString();
            }

            // Employee
            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
            {
                var row = ds.Tables[1].Rows[0];

                model.EmpId = Convert.ToInt32(row["empid"]);
                model.EmpNo = row["empno"].ToString();
                model.EmpCode = row["empcode"].ToString();
                model.EmpName = row["EmployeeName"].ToString();
                model.Designation = row["Designation"].ToString();
                model.Department = row["Department"].ToString();
                model.DOJ = Convert.ToDateTime(row["joining"]);
                model.PAN = row["emppancardno"].ToString();
                model.UAN = row["empuanno"].ToString();
                model.PFNo = row["empepfno"].ToString();
                model.ESICNo = row["empesicno"].ToString();
                model.BankAccount = row["empaccountno"].ToString();
                model.BankName = row["bankname"].ToString();
                model.IFSCCode= row["IFSCCode"].ToString();
            }

            // 3. Attendance Summary
            var SalesEmpNo = clsDatabase.fnDBOperation("PROC_GETEMPNO_SALES", EmpId);

            // Define your fix TenantID here
            string targetTenantId = "6B1B6590-C5CA-4FD6-A0BB-FEBA6DB8FB14";

            if (TenantId == targetTenantId && !string.IsNullOrEmpty(SalesEmpNo) && SalesEmpNo != "0")
            {
                DataTable officeTable = clsDatabase.fnDataTable("proc_dailyattendance", Month, Year, SalesEmpNo, "");
                List<AttendanceLog> apiLogs = new List<AttendanceLog>();

                // Step 2: Fetch API data (Sales Force API)
                using (var client = new HttpClient())
                {
                    string url = $"https://crmfieldforceapi.mendine.co.in/api/crm/HRMSApi/EmployeeAttendanceSheet?Businessid=MEND-PVTL-890&Employeeno={SalesEmpNo}&Month={Month}&Year={Year}";
                    var response = client.GetStringAsync(url).Result;
                    apiLogs = JsonConvert.DeserializeObject<List<AttendanceLog>>(response) ?? new List<AttendanceLog>();
                }

                // Step 3: Merge sources
                int totalDays = 0, presentDays = 0, leaveDays = 0;

                int monthNumber = DateTime.ParseExact(Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                int yearNumber = Convert.ToInt32(Year);

                var calendarDays = Enumerable.Range(1, DateTime.DaysInMonth(yearNumber, monthNumber))
                                        .Select(d => new DateTime(yearNumber, monthNumber, d));

                foreach (var day in calendarDays)
                {
                    totalDays++;

                    // Check Office DB Punches
                    bool officePresent = officeTable?.AsEnumerable().Any(r =>
                        Convert.ToDateTime(r["LOGDATE"]).Date == day.Date &&
                        !string.IsNullOrWhiteSpace(r["INTIME"]?.ToString()) &&
                        !string.IsNullOrWhiteSpace(r["OUTTIME"]?.ToString())
                    ) ?? false;

                    // Check API Field Punches
                    var apiDay = apiLogs.FirstOrDefault(l => {
                        DateTime parsed;
                        return DateTime.TryParse(l.LogDate, out parsed) && parsed.Date == day.Date;
                    });

                    bool apiPresent = apiDay != null && !string.IsNullOrWhiteSpace(apiDay.CheckIn);
                    bool isLeave = apiDay != null && apiDay.WorkType?.Trim().ToUpper() == "LEAVE";

                    if (isLeave)
                        leaveDays++;
                    else if (officePresent || apiPresent)
                        presentDays++;
                    // If neither present nor leave, it's counted as absent/leave depending on your policy
                }

                // Final Assignment for Sales Tenant
                model.TotalDays = totalDays;
                model.WorkedDays = presentDays;
                model.LeaveTaken = leaveDays;
                model.PayableDays = totalDays - (totalDays - presentDays - leaveDays); // Adjust based on your policy
            }
            else
            {
                // Default logic for all other Tenants / Non-Sales Employees
                if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                {
                    var row = ds.Tables[2].Rows[0];
                    model.TotalDays = Convert.ToInt32(row["TotalDays"]);
                    model.WorkedDays = Convert.ToInt32(row["WorkingDays"]);
                    model.WeeklyOffHoliday = Convert.ToInt32(row["WeeklyOffHoliday"]);
                    model.PayableDays = Convert.ToInt32(row["PayDays"]);
                    model.LeaveTaken = Convert.ToInt32(row["LeaveTaken"]);
                    model.LOPDays = row.Table.Columns.Contains("LWP") ? Convert.ToInt32(row["LWP"]) : 0;
                    
                   
                }
            }
            // Step 1: Fetch office data (DB)
            //DataTable officeTable = ds.Tables.Count > 2 ? ds.Tables[2] : null;

            // 4. Leave Balances
            if (ds.Tables.Count > 3)
            {
                foreach (DataRow row in ds.Tables[3].Rows)
                {
                    model.LeaveBalances.Add(new LeaveBalance
                    {
                        CodeType = row["CodeType"].ToString(),
                        Balance = Convert.ToDecimal(row["LeaveBalance"])
                    });
                }
            }
            // 5. Earnings (Table 4) → Side-by-Side Actual vs Earned

            //if (ds.Tables.Count > 4)
            //{
            //    var earningsList = new List<SalaryHead>();

            //    foreach (DataRow row in ds.Tables[4].Rows)
            //    {

            //        // Adjust column names to match your new SP output
            //        decimal actual = row.Table.Columns.Contains("ActualAmount") ? Convert.ToDecimal(row["ActualAmount"]) : 0;
            //        decimal earned = row.Table.Columns.Contains("EarnedAmount") ? Convert.ToDecimal(row["EarnedAmount"]) : 0;
            //        string name = row["AllowanceName"].ToString();

            //        // Skip if both are zero or if it's the Total row
            //        if (actual == 0 && earned == 0) continue;
            //        if (name.Equals("Gross Amount", StringComparison.OrdinalIgnoreCase)) continue;

            //        earningsList.Add(new SalaryHead
            //        {
            //            PayConfigName = ToTitleCase(name),
            //            ActualAmount = actual,
            //            Amount = earned
            //        });
            //    }

            //    // ✅ Priority Order: Basic > Stipend > Others
            //    model.Earnings = earningsList
            //        .OrderBy(e =>
            //            e.PayConfigName.IndexOf("BASIC", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 :
            //            e.PayConfigName.IndexOf("STIPEND", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 2
            //        )
            //        .ThenBy(e => e.PayConfigName)
            //        .ToList();
            //}
            //if (ds.Tables.Count > 4)
            //{
            //    var earnings = new List<SalaryHead>();

            //    foreach (DataRow row in ds.Tables[4].Rows)
            //    {
            //        bool isOld = row.Table.Columns.Contains("PayConfigName");

            //        string name = isOld
            //            ? row["PayConfigName"].ToString()
            //            : row["AllowanceName"].ToString();

            //        decimal actual = isOld
            //            ? Convert.ToDecimal(row["Amount"])
            //            : Convert.ToDecimal(row["ActualAmount"]);

            //        decimal earned = isOld
            //            ? Convert.ToDecimal(row["Amount"])
            //            : Convert.ToDecimal(row["EarnedAmount"]);

            //        // Skip components where earned amount is 0 (or both actual and earned are 0)
            //        if (earned == 0 && actual == 0)
            //            continue;

            //        if (earned == 0)
            //            continue;

            //        if (name.Equals("Gross Amount", StringComparison.OrdinalIgnoreCase))
            //            continue;

            //        earnings.Add(new SalaryHead
            //        {
            //            PayConfigName = ToTitleCase(name),
            //            ActualAmount = actual,
            //            Amount = earned
            //        });
            //    }

            //    model.Earnings = earnings
            //        .OrderBy(e =>
            //            e.PayConfigName.Equals("Basic", StringComparison.OrdinalIgnoreCase) ? 0 :
            //            e.PayConfigName.Equals("Stipend", StringComparison.OrdinalIgnoreCase) ? 1 : 2)
            //        .ThenBy(e => e.PayConfigName)
            //        .ToList();
            //}
            if (ds.Tables.Count > 4)
            {
                var earnings = new List<SalaryHead>();

                foreach (DataRow row in ds.Tables[4].Rows)
                {
                    bool isOld = row.Table.Columns.Contains("PayConfigName");

                    string name = isOld
                        ? row["PayConfigName"].ToString()
                        : row["AllowanceName"].ToString();

                    decimal actual = isOld
                        ? Convert.ToDecimal(row["Amount"])
                        : Convert.ToDecimal(row["ActualAmount"]);

                    decimal earned = isOld
                        ? Convert.ToDecimal(row["Amount"])
                        : Convert.ToDecimal(row["EarnedAmount"]);

                    if (earned == 0 && actual == 0)
                        continue;

                    if (earned == 0)
                        continue;

                    if (name.Equals(
                        "Gross Amount",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Example:
                    // "ACTIVITY INCENTIVE (June 2026)"
                    // becomes
                    // "ACTIVITY INCENTIVE"
                    string basePayConfigName = name.Trim();

                    int bracketIndex = basePayConfigName.LastIndexOf(
                        " (",
                        StringComparison.Ordinal
                    );

                    if (bracketIndex > 0 &&
                        basePayConfigName.EndsWith(")"))
                    {
                        basePayConfigName =
                            basePayConfigName
                                .Substring(0, bracketIndex)
                                .Trim();
                    }

                    int sortOrder = 9999;

                    if (payConfigSortOrder.TryGetValue(
                        basePayConfigName,
                        out int configSortOrder))
                    {
                        sortOrder = configSortOrder;
                    }

                    earnings.Add(new SalaryHead
                    {
                        PayConfigName = ToTitleCase(name),
                        ActualAmount = actual,
                        Amount = earned,
                        SortOrder = sortOrder
                    });
                }

                model.Earnings = earnings
                    .OrderBy(x => x.SortOrder)
                    .ToList();
            }
            // 6. Deductions (Table 5) 
            if (ds.Tables.Count > 5)
                {
                    foreach (DataRow row in ds.Tables[5].Rows)
                    {
                        bool isOld = row.Table.Columns.Contains("PayConfigName");

                        string name = isOld
                            ? row["PayConfigName"].ToString()
                            : row["DeductionName"].ToString();

                        decimal configured = isOld
                            ? Convert.ToDecimal(row["Amount"])
                            : Convert.ToDecimal(row["ConfiguredDeduction"]);

                        decimal actual = isOld
                            ? Convert.ToDecimal(row["Amount"])
                            : Convert.ToDecimal(row["ActualDeduction"]);

                        // Skip deduction components where actual deduction is 0 (or both configured and actual are 0)
                        if (configured == 0 && actual == 0)
                            continue;

                        if (actual == 0)
                            continue;

                        model.Deductions.Add(new SalaryHead
                        {
                            PayConfigName = ToTitleCase(name),
                            ActualAmount = configured,
                            Amount = actual
                        });
                    }
                }
                //7. Net Pay is optional. HRTag = 0 may intentionally skip this resultset.
                var netPayTable = FindTableWithColumn(ds, "NetPay", 6);
            if (netPayTable != null && netPayTable.Rows.Count > 0)
            {
                var row = netPayTable.Rows[0];
                model.NetPay = Convert.ToDecimal(row["NetPay"]);
                model.ShowNetPay = true;
            }
            //7. Loan 
            // 7. Loan (only if SP returned a loan dataset with values)
            var loanTable = FindTableWithColumn(ds, "EMIAmount", 6);
            if (loanTable != null && loanTable.Rows.Count > 0)
            {
                var row = loanTable.Rows[0];
                var cols = loanTable.Columns;

                // Helper function to safely parse and round decimals
                decimal GetSafeDecimal(DataRow r, string colName)
                {
                    if (!cols.Contains(colName) || r[colName] == DBNull.Value)
                        return 0;
                    return Math.Round(Convert.ToDecimal(r[colName]), 0);
                }

                model.LoanInstallment = GetSafeDecimal(row, "EMIAmount");
                model.OpeningLoan = GetSafeDecimal(row, "OpeningBalance");
                model.ClosingLoan = GetSafeDecimal(row, "ClosingBalance");
            }
            bool isOldPayroll = ds.Tables.Count > 4 &&
                    ds.Tables[4].Columns.Contains("PayConfigName");
            if (isOldPayroll && model.LoanInstallment > 0)
            {
                model.Deductions.Add(new SalaryHead
                {
                    PayConfigName = "Salary Advance",
                    ActualAmount = model.LoanInstallment,
                    Amount = model.LoanInstallment
                });
            }
            // Totals
            var grossRow = model.Earnings
                .FirstOrDefault(x => x.PayConfigName.Equals("Gross Amount", StringComparison.OrdinalIgnoreCase));
            model.GrossEarnings = grossRow != null ? grossRow.Amount : model.Earnings.Sum(x => x.Amount);
            model.TotalDeductions = model.Deductions.Sum(x => x.Amount);
            //model.NetPay = model.GrossEarnings - model.TotalDeductions;
            model.NetPayWords = NumberToWords((long)model.NetPay);
            // Remaining ds logic same as your previous block
        //}

        //-------------------------------------------------------
        // Final Totals
        //-------------------------------------------------------
        model.GrossEarnings = model.Earnings.Sum(x => x.Amount);
        model.TotalDeductions = model.Deductions.Sum(x => x.Amount);
        model.NetPayWords = NumberToWords((long)model.NetPay);

        return model;
    }
        [HttpPost]
        public ActionResult DownloadBulk(string empIds,string month,int year)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(empIds))
                    return Content("No employees selected.");

                var empList =
                    empIds
                        .Split(',')
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(int.Parse)
                        .ToList();

                using (var zipStream = new MemoryStream())
                {
                    var pdfService =
                        new PayslipPdfService();

                    using (var archive =
                        new ZipArchive(
                            zipStream,
                            ZipArchiveMode.Create,
                            true))
                    {
                        foreach (int empId in empList)
                        {
                            try
                            {
                                // -----------------------------------------
                                // 1. Get model
                                // -----------------------------------------
                                PayslipViewModel model =
                                    GetPayslipData(
                                        empId,
                                        month,
                                        year,
                                        true,
                                        1
                                    );

                                if (model == null ||
                                    model.EmpId == 0)
                                {
                                    continue;
                                }

                                model.isPdf = true;

                                // -----------------------------------------
                                // 2. Get view
                                // -----------------------------------------
                                string viewName =
                                    GetPayslipViewName(
                                        model.TenantId
                                    );

                                if (string.IsNullOrWhiteSpace(viewName))
                                    viewName = "Index";

                                // -----------------------------------------
                                // 3. Safe file name
                                // -----------------------------------------
                                string empCode =
                                    MakeSafeFileName(
                                        model.EmpCode
                                    );

                                string empName =
                                    MakeSafeFileName(
                                        model.EmpName
                                    );

                                string pdfFileName =
                                    $"Payslip_{empCode}_{empName}_{month}_{year}.pdf";

                                // -----------------------------------------
                                // 4. Generate using Puppeteer
                                // -----------------------------------------
                                byte[] pdfBytes =
                                    pdfService.GeneratePayslipPdf(
                                        ControllerContext,
                                        viewName,
                                        model,
                                        pdfFileName
                                    );

                                if (pdfBytes == null ||
                                    pdfBytes.Length == 0)
                                {
                                    clsDatabase.fnErrorLog(
                                        "BulkPayslipSkipped",
                                        $"EmpId={empId}; Reason=Empty PDF"
                                    );

                                    continue;
                                }

                                // -----------------------------------------
                                // 5. Add PDF to ZIP
                                // -----------------------------------------
                                var entry =
                                    archive.CreateEntry(
                                        pdfFileName,
                                        CompressionLevel.Fastest
                                    );

                                using (var entryStream =
                                    entry.Open())
                                {
                                    entryStream.Write(
                                        pdfBytes,
                                        0,
                                        pdfBytes.Length
                                    );
                                }
                            }
                            catch (Exception empEx)
                            {
                                clsDatabase.fnErrorLog(
                                    "BulkPayslipEmployeeError",
                                    $"EmpId={empId}; " +
                                    $"Month={month}; " +
                                    $"Year={year}; " +
                                    $"Error={empEx}"
                                );

                                // Continue with next employee
                            }
                        }
                    }

                    byte[] zipBytes =
                        zipStream.ToArray();

                    return File(
                        zipBytes,
                        "application/zip",
                        $"Payslips_{month}_{year}.zip"
                    );
                }
            }
            catch (Exception ex)
            {
                clsDatabase.fnErrorLog(
                    "BulkPayslipDownloadError",
                    $"EmpIds={empIds}; " +
                    $"Month={month}; " +
                    $"Year={year}; " +
                    $"Error={ex}"
                );

                return Content(
                    "Bulk payslip download failed."
                );
            }
        }

        public ActionResult SalaryReport()
        {
            // Get dataset from stored procedure
            DataSet ds = clsDatabase.fnDataSet("SP_MasterBonus_List");
            // Defensive check
            if (ds != null && ds.Tables.Count > 0)
            {
                // Table 0 = Departments
                var departments = (from DataRow dr in ds.Tables[0].Rows
                                   select new SelectListItem
                                   {
                                       Value = dr["DepartmentId"].ToString(),
                                       Text = dr["Department"].ToString()
                                   }).ToList();

                // Table 1 = Categories
                var categories = (from DataRow dr in ds.Tables[1].Rows
                                  select new SelectListItem
                                  {
                                      Value = dr["CategoryId"].ToString(),
                                      Text = dr["CategoryName"].ToString()
                                  }).ToList();

                ViewBag.Departments = departments;
                ViewBag.Categories = categories;
            }
            return View();
        }
        public ActionResult SalaryReportPartial(string month, int year,int category)
        {
            var vm = BuildSalaryReport(month, year, category); // your existing logic that builds SalaryReportViewModel
            return PartialView("_SalaryReportTable", vm);
        }
        // 🔹 Common builder for SalaryReportViewModel
        private SalaryReportViewModel BuildSalaryReport(string month, int year,int category)
        {
            DataTable dt = clsDatabase.fnDataTable("PRC_GetSalaryWithAttendance", month, year,category);

            var list = (from DataRow row in dt.Rows
                        select new EmployeeSalaryReportModel
                        {
                            EmployeeName = row["EmployeeName"].ToString(),
                            Department = row["Department"].ToString(),
                            PresentDays = Convert.ToInt32(row["PresentDays"]),
                            Basic = Convert.ToDecimal(row["Basic"]),
                            HRA = Convert.ToDecimal(row["HRA"]),
                            ConveyenceAllowance = Convert.ToDecimal(row["ConveyenceAllowance"]),
                            OtherAllowance = Convert.ToDecimal(row["OtherAllowance"]),
                            OtherEarning = Convert.ToDecimal(row["OtherEarning"]),
                            TotalEarning = Convert.ToDecimal(row["TotalEarning"]),
                            PF = Convert.ToDecimal(row["PF"]),
                            ESI = Convert.ToDecimal(row["ESI"]),
                            PT = Convert.ToDecimal(row["PT"]),
                            TDS = Convert.ToDecimal(row["TDS"]),
                            OtherDeduction= Convert.ToDecimal(row["Others"]),
                            LoanAmount = Convert.ToDecimal(row["Loan"]),
                            TotalDeductions = Convert.ToDecimal(row["TotalDeductions"]),
                            NetPayable = Convert.ToDecimal(row["NetPayable"]),
                            Month = row["Month"].ToString(),
                            Year = row["Year"].ToString(),
                            
                            Company = row.Table.Columns.Contains("Company")
                                        ? row["Company"].ToString()
                                        : "N/A"
                        }).ToList();

            // Grouping by Department
            var grouped = list
                .GroupBy(x => x.Department)
                .Select(g => new DepartmentSalaryGroup
                {
                    Department = g.Key,
                    Employees = g.ToList(),
                    SubTotal = new EmployeeSalaryReportModel
                    {
                        Department = g.Key,
                        PresentDays = g.Sum(x => x.PresentDays),
                        Basic = g.Sum(x => x.Basic),
                        HRA = g.Sum(x => x.HRA),
                        ConveyenceAllowance = g.Sum(x => x.ConveyenceAllowance),
                        OtherAllowance = g.Sum(x => x.OtherAllowance),
                        OtherEarning = g.Sum(x => x.OtherEarning),
                        TotalEarning = g.Sum(x => x.TotalEarning),
                        PF = g.Sum(x => x.PF),
                        ESI = g.Sum(x => x.ESI),
                        PT = g.Sum(x => x.PT),
                        TDS = g.Sum(x => x.TDS),
                        OtherDeduction=g.Sum(x => x.OtherDeduction),
                        LoanAmount= g.Sum(x => x.LoanAmount),
                        TotalDeductions = g.Sum(x => x.TotalDeductions),
                        NetPayable = g.Sum(x => x.NetPayable)
                    }
                }).ToList();

            // Grand total
            var grand = new EmployeeSalaryReportModel
            {
                PresentDays = list.Sum(x => x.PresentDays),
                Basic = list.Sum(x => x.Basic),
                HRA = list.Sum(x => x.HRA),
                ConveyenceAllowance = list.Sum(x => x.ConveyenceAllowance),
                OtherAllowance = list.Sum(x => x.OtherAllowance),
                OtherEarning = list.Sum(x => x.OtherEarning),
                TotalEarning = list.Sum(x => x.TotalEarning),
                PF = list.Sum(x => x.PF),
                ESI = list.Sum(x => x.ESI),
                PT = list.Sum(x => x.PT),
                TDS = list.Sum(x => x.TDS),
                OtherDeduction=list.Sum(x => x.OtherDeduction),
                LoanAmount = list.Sum(x => x.LoanAmount),
                TotalDeductions = list.Sum(x => x.TotalDeductions),
                NetPayable = list.Sum(x => x.NetPayable)
            };

            return new SalaryReportViewModel
            {
                CompanyName = list.FirstOrDefault()?.Company ?? "N/A",
                Month = month,
                Year = year,
                Departments = grouped,
                GrandTotal = grand
            };
        }
        // ----------------- Excel Export -----------------
         

        public ActionResult ExportSalaryExcel(string month, int year, int category)
        {
            var model = BuildSalaryReport(month, year, category); // your SalaryReportViewModel

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Salary Sheet");

                // Header
                ws.Cell(1, 1).Value = model.CompanyName;
                ws.Range(1, 1, 1, 18).Merge().Style
                    .Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Cell(2, 1).Value = $"Salary Sheet Report for the Month of {model.Month} {model.Year}";
                ws.Range(2, 1, 2, 18).Merge().Style
                    .Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Cell(3, 1).Value = $"Print Date: {DateTime.Now:dd-MMM-yyyy}";
                ws.Range(3, 1, 3, 18).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                // Column headers
                int row = 5;
                ws.Cell(row, 1).Value = "Ref. No.";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Present Days";
                ws.Range(row, 4, row, 8).Merge().Value = "Earnings";
                ws.Cell(row, 9).Value = "Total Earnings";
                ws.Range(row, 10, row, 15).Merge().Value = "Deductions";
                ws.Cell(row, 16).Value = "Total Deductions";
                ws.Cell(row, 17).Value = "Net Payable";
                ws.Cell(row, 18).Value = "Remarks";

                row++;

                ws.Cell(row, 4).Value = "BASIC";
                ws.Cell(row, 5).Value = "HRA";
                ws.Cell(row, 6).Value = "CONVEYANCE";
                ws.Cell(row, 7).Value = "Other Allow";
                ws.Cell(row, 8).Value = "Other Earn.";
                ws.Cell(row, 10).Value = "PF";
                ws.Cell(row, 11).Value = "ESI";
                ws.Cell(row, 12).Value = "PT";
                ws.Cell(row, 13).Value = "TDS";
                ws.Cell(row, 14).Value = "Other Ded.";
                ws.Cell(row, 15).Value = "Loan Amt.";

                // Body
                foreach (var dept in model.Departments)
                {
                    row++;
                    ws.Cell(row, 1).Value = dept.Department;
                    ws.Range(row, 1, row, 17).Merge().Style.Fill.BackgroundColor = XLColor.LightGray;

                    int refNo = 1;
                    foreach (var emp in dept.Employees)
                    {
                        row++;
                        ws.Cell(row, 1).Value = refNo++;
                        ws.Cell(row, 2).Value = emp.EmployeeName;
                        ws.Cell(row, 3).Value = emp.PresentDays;
                        ws.Cell(row, 4).Value = emp.Basic;
                        ws.Cell(row, 5).Value = emp.HRA;
                        ws.Cell(row, 6).Value = emp.ConveyenceAllowance;
                        ws.Cell(row, 7).Value = emp.OtherAllowance;
                        ws.Cell(row, 8).Value = 0;
                        ws.Cell(row, 9).Value = emp.TotalEarning;
                        ws.Cell(row, 10).Value = emp.PF;
                        ws.Cell(row, 11).Value = emp.ESI;
                        ws.Cell(row, 12).Value = emp.PT;
                        ws.Cell(row, 13).Value = emp.TDS;
                        ws.Cell(row, 14).Value = emp.OtherDeduction;
                        ws.Cell(row, 15).Value = emp.LoanAmount;
                        ws.Cell(row, 16).Value = emp.TotalDeductions;
                        ws.Cell(row, 17).Value = emp.NetPayable;
                    }

                    // Subtotal
                    row++;
                    ws.Cell(row, 1).Value = "Sub Total – " + dept.Department;
                    ws.Range(row, 1, row, 2).Merge().Style.Font.SetBold();
                    ws.Cell(row, 4).Value = dept.SubTotal.Basic;
                    ws.Cell(row, 5).Value = dept.SubTotal.HRA;
                    ws.Cell(row, 6).Value = dept.SubTotal.ConveyenceAllowance;
                    ws.Cell(row, 7).Value = dept.SubTotal.OtherAllowance;
                    ws.Cell(row, 9).Value = dept.SubTotal.TotalEarning;
                    ws.Cell(row, 10).Value = dept.SubTotal.PF;
                    ws.Cell(row, 11).Value = dept.SubTotal.ESI;
                    ws.Cell(row, 12).Value = dept.SubTotal.PT;
                    ws.Cell(row, 13).Value = dept.SubTotal.TDS;
                    ws.Cell(row, 14).Value = dept.SubTotal.OtherDeduction;
                    ws.Cell(row, 15).Value = dept.SubTotal.LoanAmount;
                    ws.Cell(row, 16).Value = dept.SubTotal.TotalDeductions;
                    ws.Cell(row, 17).Value = dept.SubTotal.NetPayable;
                }

                // Grand total
                row++;
                ws.Cell(row, 1).Value = "GRAND TOTAL";
                ws.Range(row, 1, row, 2).Merge().Style.Font.SetBold();
                ws.Cell(row, 4).Value = model.GrandTotal.Basic;
                ws.Cell(row, 5).Value = model.GrandTotal.HRA;
                ws.Cell(row, 6).Value = model.GrandTotal.ConveyenceAllowance;
                ws.Cell(row, 7).Value = model.GrandTotal.OtherAllowance;
                ws.Cell(row, 9).Value = model.GrandTotal.TotalEarning;
                ws.Cell(row, 10).Value = model.GrandTotal.PF;
                ws.Cell(row, 11).Value = model.GrandTotal.ESI;
                ws.Cell(row, 12).Value = model.GrandTotal.PT;
                ws.Cell(row, 13).Value = model.GrandTotal.TDS;
                ws.Cell(row, 14).Value = model.GrandTotal.OtherDeduction;
                ws.Cell(row, 15).Value = model.GrandTotal.LoanAmount;
                ws.Cell(row, 16).Value = model.GrandTotal.TotalDeductions;
                ws.Cell(row, 17).Value = model.GrandTotal.NetPayable;

                // Auto-fit columns
                ws.Columns().AdjustToContents();

                // Return file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"SalarySheet_{model.Month}_{model.Year}.xlsx");
                }
            }
        }
        // ----------------- PDF Export -----------------
        public ActionResult ExportSalaryPdf(string month, int year, int category)
        {
            var vm = BuildSalaryReport(month, year, category); // same builder

            // "SalaryReportPdf" should be a Razor view (cshtml) formatted for PDF
            return new Rotativa.ViewAsPdf("SalaryReportPdf", vm)
            {
                FileName = $"SalaryReport_{month}_{year}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Landscape
            };
        }
        private string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }
        
    }
}
