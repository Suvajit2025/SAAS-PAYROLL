using Common.Utility;
using MendinePayroll.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    }
}