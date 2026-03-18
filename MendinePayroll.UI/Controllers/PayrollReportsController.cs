using Common.Utility;
using MendinePayroll.Models;
using MendinePayroll.UI.BLL;
using MendinePayroll.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MendinePayroll.UI.Controllers
{
    public class PayrollReportsController : Controller
    {
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
        // GET: PayrollReports
        public ActionResult Index()
        {
            return View();
        }
        
        //ALL Filter Data
        public JsonResult GetCompanyList()
        {

            DataTable dt = clsDatabase.fnDataTable("Proc_Saas_CompanyMst", TenantId);
            var list = dt.AsEnumerable().Select(r => new
            {
                CompanyId = r["CompanyID"].ToString(),
                CompanyName = r["CompanyName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDepartmentList()
        {

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_Department", TenantId);
            var list = dt.AsEnumerable().Select(r => new
            {
                DepartmentID = r["DepartmentId"].ToString(),
                DepartmentName = r["Department"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCategoryList()
        {

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_EMPCATEGORY", TenantId);
            var list = dt.AsEnumerable().Select(r => new
            {
                CategoryID = r["ID"].ToString(),
                CategoryName = r["CategoryName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPayGroupList()
        {

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_PAYGROUP", TenantId);

            var list = dt.AsEnumerable().Select(r => new
            {
                PayGroupID = r["PayGroupID"].ToString(),
                PayGroupName = r["PayGroupName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmployeeList(string companyIds, string deptIds, string categoryIds, string status)
        {
            // Note: If you pass multiple IDs, your SP should handle comma-separated strings
            // Or you can filter the DataTable in C# as shown below
            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_EMPLOYEES", companyIds, deptIds, categoryIds, status,TenantId);

            var list = dt.AsEnumerable().Select(r => new
            {
                EmployeeID = r["EmployeeID"].ToString(),
                EmployeeName = r["EmployeeName"].ToString() + " - " + r["EmployeeNo"].ToString(),
                CompanyID = r["CompanyID"].ToString(),
                DepartmentID = r["DepartmentID"].ToString(),
                CategoryID = r["CategoryID"].ToString(),
                Status = r["Status"].ToString() // ACTIVE/INACTIVE
            }); 
            return Json(list.ToList(), JsonRequestBehavior.AllowGet);
        }
        // --- SALARY REGISTER ---
        public ActionResult SalaryRegister()
        {
            ViewBag.Title = "Salary Register Report";

            // Define visibility settings for this specific report
            ViewBag.ShowExcel = true;
            ViewBag.ShowPDF = false;   // Usually too wide for PDF
            ViewBag.ShowCSV = true;
            ViewBag.ShowPrint = false;

            return View(); // Looks for SalaryRegister.cshtml
        }
        [HttpPost]
        public JsonResult SalaryRegister(ReportFilterModel filters)
        {
            try
            {
                // 1. Convert List<int> to Comma-Separated Strings for your SP
                string CompanyIds = filters.companyIds != null ? string.Join(",", filters.companyIds) : "";
                string DeptIds = filters.deptIds != null ? string.Join(",", filters.deptIds) : "";
                string CategoryIds = filters.categoryIds != null ? string.Join(",", filters.categoryIds) : "";
                string PayGroupIds = filters.paygroupIds != null ? string.Join(",", filters.paygroupIds) : "";
                string EmpIds = filters.empIds != null ? string.Join(",", filters.empIds) : "";
                int Year = filters.year;
                int Month = filters.month;
                string Status = filters.status;
                // 2. Execute ADO.NET SP
                // Order: Year, Month, Company, Dept, Category, PayGroup, Employee, Status, Tenant
                DataTable dt = clsDatabase.fnDataTable("SP_PAYROLL_SALARY_REGISTER",
                    Year, Month, CompanyIds, DeptIds, CategoryIds, PayGroupIds, EmpIds, Status, TenantId);

                if (dt == null || dt.Rows.Count == 0)
                    return Json(new { success = false, message = "No records found." });

                // 3. Extract Headers dynamically from the SQL Result Columns
                var finalHeaders = new List<string>();
                foreach (DataColumn column in dt.Columns)
                {
                    finalHeaders.Add(column.ColumnName);
                }

                // 4. Convert DataTable Rows to List of Dictionaries
                var finalRows = new List<Dictionary<string, object>>();
                foreach (DataRow dr in dt.Rows)
                {
                    var row = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        var value = dr[col];

                        // Format numeric values to 2 decimal places for the report
                        if (value != DBNull.Value && (col.DataType == typeof(decimal) || col.DataType == typeof(double)))
                        {
                            row[col.ColumnName] = Convert.ToDecimal(value).ToString("N2");
                        }
                        else
                        {
                            row[col.ColumnName] = value != DBNull.Value ? value.ToString() : "";
                        }
                    }
                    finalRows.Add(row);
                }

                return Json(new { success = true, Headers = finalHeaders, Data = finalRows });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server Error: " + ex.Message });
            }
        }

        #region Loan Register
        public ActionResult LoanRegister()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Index", "Login");

            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel(); 
            // Month dropdown
            employeeSalaryModel.MonthList = GetMonthList();

            DataTable fyTable = clsSalary.FinancialYears(); // Must return columns: FinancialYear, Year

            List<SelectListItem> fyList = new List<SelectListItem>();

            foreach (DataRow row in fyTable.Rows)
            {
                SelectListItem item = new SelectListItem();
                item.Text = row["FinancialYear"].ToString(); // e.g., "2024-2025"
                item.Value = row["Year"].ToString();         // e.g., "2025"
                fyList.Add(item);
            }

            employeeSalaryModel.YearList = fyList;
             
            // Optional: select current FY
            var currentFY = employeeSalaryModel.YearList
            .FirstOrDefault(y => y.Value == (DateTime.Now.Year).ToString());
            if (currentFY != null)
                currentFY.Selected = true;


            return View(employeeSalaryModel);
        }
        [HttpPost]
        public JsonResult GetAllEmployeeLoanRegister(string FinancialYear, int? empParam, int numberofRow, int pagenumber)
        {
            try
            {
                int? IDEmployee = (empParam == 0 ? (int?)null : empParam);

                // 1. Fetch full SP result
                DataTable dt = clsDatabase.fnDataTable("PRC_Loan_Ledger_ByFY", TenantId, FinancialYear, IDEmployee, numberofRow, pagenumber);

                if (dt == null || dt.Rows.Count == 0)
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);

                // 2. Convert DataTable → list of dictionary (row-wise)
                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow dr in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                        dict[col.ColumnName] = dr[col] == DBNull.Value ? null : dr[col];
                    rows.Add(dict);
                }

                // 3. Department-wise subtotals
                var grouped = rows.GroupBy(r => r["Department"]?.ToString() ?? "Unknown").ToList();
                var final = new List<Dictionary<string, object>>();

                foreach (var grp in grouped)
                {
                    final.AddRange(grp); // add all department rows first

                    // subtotal row
                    var subtotal = new Dictionary<string, object>();
                    subtotal["Department"] = grp.Key;
                    subtotal["Employee Name"] = "Subtotal";

                    foreach (DataColumn col in dt.Columns)
                    {
                        string name = col.ColumnName;
                        if (IsNumericType(col.DataType))
                        {
                            decimal sum = grp.Sum(r =>
                            {
                                var val = r[name];
                                if (val == null) return 0;
                                decimal d;
                                return decimal.TryParse(val.ToString(), out d) ? d : 0;
                            });
                            subtotal[name] = sum;
                        }
                        else if (!subtotal.ContainsKey(name))
                        {
                            subtotal[name] = null;
                        }
                    }

                    final.Add(subtotal);
                }

                // 4. Grand total row
                var grand = new Dictionary<string, object>();
                grand["Department"] = "Grand Total";
                grand["Employee Name"] = "";

                foreach (DataColumn col in dt.Columns)
                {
                    string name = col.ColumnName;
                    if (IsNumericType(col.DataType))
                    {
                        decimal sum = final
                            .Where(r => r.ContainsKey("Employee Name") && (r["Employee Name"]?.ToString() == "Subtotal"))
                            .Sum(r =>
                            {
                                var val = r[name];
                                if (val == null) return 0;
                                decimal d;
                                return decimal.TryParse(val.ToString(), out d) ? d : 0;
                            });
                        grand[name] = sum;
                    }
                    else if (!grand.ContainsKey(name))
                    {
                        grand[name] = null;
                    }
                }

                final.Add(grand);

                // 5. Return full JSON list
                return Json(final, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(@"C:\Temp\LoanRegisterError.log",
                    DateTime.Now + " :: " + ex.ToString() + Environment.NewLine);
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private bool IsNumericType(Type t)
        {
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
        }
        [HttpPost]
        public JsonResult ExportLoanRegisterToCSV(string Year, int? empParam)
        {
            try
            {
                int? IDEmployee = (empParam == 0 ? (int?)null : empParam);
                int numberofRow = 9999; // fetch all rows
                int pagenumber = 1;

                // 1. Get data from SP
                DataTable dt = clsDatabase.fnDataTable("PRC_Loan_Ledger_ByFY", Year, IDEmployee, numberofRow, pagenumber);
                if (dt == null || dt.Rows.Count == 0)
                    return Json("NODATA", JsonRequestBehavior.AllowGet);
                // Remove TotalCount column if exists
                if (dt.Columns.Contains("TotalCount"))
                    dt.Columns.Remove("TotalCount");
                // Remove TotalCount column if exists
                if (dt.Columns.Contains("RN"))
                    dt.Columns.Remove("RN");
                // 2. Clone table for modifications (subtotal + grand total)
                DataTable grouped = dt.Clone();
                var deptGroups = dt.AsEnumerable().GroupBy(r => r["Department"].ToString());

                foreach (var group in deptGroups)
                {
                    // Add all rows for this department
                    foreach (var row in group)
                        grouped.ImportRow(row);

                    // Create subtotal row
                    DataRow subtotal = grouped.NewRow();

                    // Fill all string columns to avoid null issues
                    foreach (DataColumn col in grouped.Columns)
                        if (col.DataType == typeof(string))
                            subtotal[col.ColumnName] = "";

                    subtotal["Department"] = group.Key;
                    subtotal["Employee Name"] = "Subtotal";
                    subtotal["Financial Year"] = Year;

                    // Sum numeric columns
                    foreach (DataColumn col in grouped.Columns)
                    {
                        if (!new[] { "Department", "Employee Name", "Financial Year", "Loan No" }.Contains(col.ColumnName)
                            && (col.DataType == typeof(decimal) || col.DataType == typeof(double) ||
                                col.DataType == typeof(float) || col.DataType == typeof(int)))
                        {
                            decimal sum = group.Sum(r =>
                            {
                                decimal d;
                                return decimal.TryParse(r[col.ColumnName]?.ToString(), out d) ? d : 0;
                            });
                            subtotal[col.ColumnName] = sum;
                        }
                    }
                    grouped.Rows.Add(subtotal);
                }

                // 3. Add grand total row
                DataRow grand = grouped.NewRow();
                foreach (DataColumn col in grouped.Columns)
                    if (col.DataType == typeof(string))
                        grand[col.ColumnName] = "";

                grand["Department"] = "Grand Total";
                grand["Financial Year"] = Year;

                foreach (DataColumn col in grouped.Columns)
                {
                    if (!new[] { "Department", "Employee Name", "RN", "TotalCount", "Financial Year", "Loan No" }.Contains(col.ColumnName)
                        && (col.DataType == typeof(decimal) || col.DataType == typeof(double) ||
                            col.DataType == typeof(float) || col.DataType == typeof(int)))
                    {
                        decimal sum = grouped.AsEnumerable()
                            .Where(r => r["Employee Name"].ToString() == "Subtotal")
                            .Sum(r =>
                            {
                                decimal d;
                                return decimal.TryParse(r[col.ColumnName]?.ToString(), out d) ? d : 0;
                            });
                        grand[col.ColumnName] = sum;
                    }
                }
                grouped.Rows.Add(grand);

                // 4. Build CSV
                var csv = new StringBuilder();

                // Header
                for (int i = 0; i < grouped.Columns.Count; i++)
                {
                    csv.Append(grouped.Columns[i].ColumnName);
                    if (i < grouped.Columns.Count - 1)
                        csv.Append(",");
                }
                csv.AppendLine();

                // Rows
                foreach (DataRow row in grouped.Rows)
                {
                    for (int i = 0; i < grouped.Columns.Count; i++)
                    {
                        string val = row[i]?.ToString().Replace(",", " ") ?? "0";
                        if (string.IsNullOrWhiteSpace(val)) val = "0";
                        csv.Append(val);
                        if (i < grouped.Columns.Count - 1)
                            csv.Append(",");
                    }
                    csv.AppendLine();
                }

                // 5. Save file (same as ESIC)
                string fileName = $"LoanRegister_{Year}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                string folderPath = Server.MapPath("~/Downloads/");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);
                System.IO.File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

                // 6. Return filename to AJAX
                return Json(fileName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("ERROR", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Bind Month&Year
        public List<SelectListItem> GetMonthList()
        {
            List<SelectListItem> MonthList = new List<SelectListItem>();
            var months = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            DateTimeFormatInfo info = DateTimeFormatInfo.GetInstance(null);
            for (int i = 1; i < 13; i++)
            {
                MonthList.Add(new SelectListItem
                {
                    Text = info.GetMonthName(i),
                    Value = i.ToString(),
                    Selected = (i == DateTime.Now.AddMonths(-1).Month ? true : false)
                });

            }
            return MonthList;
        }
        public List<SelectListItem> GetYearList()
        {
            List<SelectListItem> YearList = new List<SelectListItem>();
            int year = DateTime.Now.Year;
            for (int j = year - 20; j < year + 20; j++)
            {
                YearList.Add(new SelectListItem
                {
                    Text = j.ToString(),
                    Value = j.ToString(),
                    Selected = (j == DateTime.Now.AddMonths(-1).Year ? true : false)
                });

            }
            return YearList;
        }

        #endregion

    }
}