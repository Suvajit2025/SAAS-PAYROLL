using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using UI.BLL;
using MendinePayroll.UI.Models;
using System.Data;
using Common.Utility;

namespace MendinePayroll.UI.Controllers
{
    public class SalaryConfigurationController : Controller
    {

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
        // GET: SalaryConfiguration
        public ActionResult Index()
        {
            return View();
        }

        private static string DataTableToJSONWithJSONNet(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }
        // Configuration Detail
        [HttpGet]
        public JsonResult ConfigurationDetail(long IDConfiguration)
        {
            DataTable DT = clsSalaryConfiguration.Salary_Configuration_Detail(IDConfiguration);
            string employees = DataTableToJSONWithJSONNet(DT);
            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        // Configuration Save
        [HttpPost]
        public ActionResult ConfigurationSave(clsSalaryConfigurationInfo obj)
        {
            String Result = clsSalaryConfiguration.Salary_Configuration_Save(obj);
            if (Result == "")
                return Json(new { success = "" }, JsonRequestBehavior.AllowGet);
            else
            {
                return Json(new { error = "Error" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult SalaryAuthentication(String UserName, String Password)
        {
            Boolean valid = clsSalaryConfiguration.Salary_Authentication(UserName, Password);
            return Json(valid, JsonRequestBehavior.AllowGet);
        }
        #region Employee Access
        public ActionResult PayrollAccess()
        {
            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_PayrollAccessEmployees", TenantId);

            var model = new PayrollAccessViewModel();

            // ✅ Bind employee list
            if (dt != null)
            {
                model.EmployeeAccessList = dt.AsEnumerable()
                    .Select(x => new PayrollEmployeeVM
                    {
                        EmployeeNo = x["EmployeeNo"].ToString(),
                        EmployeeName = x["EmployeeName"].ToString(),
                        Department = x["Department"].ToString(),
                        IsActive = Convert.ToBoolean(x["IsActive"])
                    }).ToList();
            }

            // ✅ Load department list (for modal dropdown)
            DataTable dept = clsDatabase.fnDataTable("SP_SAAS_GET_Department", TenantId);

            model.DepartmentList = dept.AsEnumerable()
                .Select(x => new SelectListItem
                {
                    Value = x["DepartmentId"].ToString(),
                    Text = x["Department"].ToString()
                }).ToList();

            return View(model); // ✅ PASS MODEL
        }

        public JsonResult GetEmployeesByDepartment(string departmentId)
        {


            // ✅ Call SP
            DataTable dt = clsDatabase.fnDataTable("SP_GET_EMP_BY_DEPT", departmentId, TenantId);

            // ✅ Convert to JSON-friendly object
            var list = new List<object>();

            if (dt != null)
            {
                list = dt.AsEnumerable()
                    .Select(x => new
                    {
                        EmployeeNo = x["EmployeeNo"].ToString(),
                        EmployeeName = x["EmployeeName"].ToString()
                    })
                    .ToList<object>();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveEmployees(List<string> employees)
        {
            try
            {
                string UserId = Session["UserName"]?.ToString();

                if (employees == null || employees.Count == 0)
                {
                    return Json(new { success = false, message = "No employees selected" });
                }

                // ✅ Convert list to JSON
                string JsonData = Newtonsoft.Json.JsonConvert.SerializeObject(employees);

                clsDatabase.fnDBOperation("SP_SAAS_INSERT_PayrollAccessEmployee_JSON", TenantId, UserId, JsonData);

                return Json(new { success = true, message = "Saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult UpdateAccessStatus(string employeeNo, int status)
        {
            try
            {
                // 1. Get TenantID from Session
                string UserId = Session["UserEmail"]?.ToString();

                if (string.IsNullOrEmpty(TenantId))
                {
                    return Json(new { success = false, message = "Session expired. Please login again." });
                }

                // 2. Execute the Database Update
                // We pass: EmployeeNo, the Status (1 or 0), and TenantId for security
                // ✅ Call the DB utility (Returns a string)
                string result = clsDatabase.fnDBOperation(
                    "SP_UPDATE_PAYROLL_ACCESS_STATUS",
                    employeeNo,
                    status,
                    TenantId,
                    UserId
                );

                // ✅ Validation: Check if the string indicates success
                // Adjust "1" to whatever your SP/Utility returns (e.g., "Success")
                if (result == "1" || string.IsNullOrEmpty(result))
                {
                    return Json(new { success = true });
                }
                else
                {
                    // If the string contains an error message, pass it to the UI
                    return Json(new { success = false, message = result });
                }
            }
            catch (Exception ex)
            {
                // Log the error here if you have a logging utility
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion
    }
}
public class PayrollAccessViewModel
{
    public List<PayrollEmployeeVM> EmployeeAccessList { get; set; }
    public List<SelectListItem> DepartmentList { get; set; }
}

public class PayrollEmployeeVM
{
    public string EmployeeNo { get; set; }
    public string EmployeeName { get; set; }
    public string Department { get; set; }
    public bool IsActive { get; set; }
}