
using Common.Utility;
using DocumentFormat.OpenXml.Bibliography;
using MendinePayroll.Models;
using MendinePayroll.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.BLL;

namespace MendinePayroll.UI.Controllers
{
    public class SalaryCTCController : Controller
    {
        private string tenantID
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
        // GET: SalaryCTC
        public ActionResult Index()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "EMPLOYEE-CTC-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        public JsonResult GetCompanyList()
        {

            DataTable dt = clsDatabase.fnDataTable("Proc_Saas_CompanyMst", tenantID);
            var list = dt.AsEnumerable().Select(r => new
            {
                CompanyId = r["CompanyID"].ToString(),
                CompanyName = r["CompanyName"].ToString() 
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDepartmentList()
        {

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_Department", tenantID);
            var list = dt.AsEnumerable().Select(r => new
            {
                DepartmentID = r["DepartmentId"].ToString(),
                DepartmentName = r["Department"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCategoryList()
        {

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_EMPCATEGORY", tenantID);
            var list = dt.AsEnumerable().Select(r => new
            {
                CategoryID = r["ID"].ToString(),
                CategoryName = r["CategoryName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPayGroupList()
        {
    
            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_PAYGROUP", tenantID);

            var list = dt.AsEnumerable().Select(r => new
            {
                PayGroupID = r["PayGroupID"].ToString(),
                PayGroupName = r["PayGroupName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPayGroupListConfigWise()
        {

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_PAYGROUP_CONFIG", tenantID);

            var list = dt.AsEnumerable().Select(r => new
            {
                PayGroupID = r["PayGroupID"].ToString(),
                PayGroupName = r["PayGroupName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEmployeeList(List<string> companyIds, List<string> deptIds, List<string> categoryIds)
        {

            string companyFilter = NormalizeMultiSelect(companyIds);
            string deptFilter = NormalizeMultiSelect(deptIds);
            string categoryFilter = NormalizeMultiSelect(categoryIds);

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_EMPLOYEE_CTC",tenantID,companyFilter,deptFilter,categoryFilter);

            var list = dt.AsEnumerable().Select(r => new
            {
                EmpID = r["EmpID"]?.ToString(),
                EmpSalaryConfigID = r["EmpSalaryConfigID"]?.ToString(),

                EmpName = r["EmpName"]?.ToString(),
                EmpCode = r["EmpCode"]?.ToString(),

                Designation = r["Designation"]?.ToString(),
                Post = r["Postname"]?.ToString(),
                Department = r["Department"]?.ToString(),

                PayGroupName = r["PayGroupName"]?.ToString(),

                Status = r["CTCStatus"]?.ToString(),   // Configured / Pending
                Empstatus = r["Status"]?.ToString(),   // ACTIVE / INACTIVE

                /* 🔑 REQUIRED FOR JS FILTERING */
                CompanyID = r["CompanyID"]?.ToString(),
                DepartmentID = r["DepartmentID"]?.ToString(),
                CategoryID = r["CategoryID"]?.ToString(),
                PayGroupID = r["PayGroupID"]?.ToString()
            }).ToList();


            return Json(list, JsonRequestBehavior.AllowGet);
        }
        private string NormalizeMultiSelect(List<string> items)
        {
            if (items == null || items.Count == 0)
                return "";

            // Remove "multiselect-all"
            items = items
                .Where(x => !x.Equals("multiselect-all", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // If after removing "multiselect-all" nothing left → treat as All selected
            if (items.Count == 0)
                return "";

            return string.Join(",", items);
        }
        [HttpGet]
        public JsonResult GetSalaryConfigByPayGroup(int payGroupId)
        {
            DataTable dt = clsDatabase.fnDataTable("PRC_GetSalaryConfigByPayGroup", payGroupId,tenantID);
            // Convert DataTable to List<Dictionary<string, object>>
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }
                list.Add(dict);
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveEmployeeSalaryStructure(EmpSalaryConfigModel model)
        {
            try
            {
                
                string UserName = Session["UserName"].ToString();

                string JsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(model);

                DataTable dt = clsDatabase.fnDataTable("PRC_Save_EmployeeSalaryConfig",tenantID, JsonPayload,UserName);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int status = Convert.ToInt32(dt.Rows[0][0]);        // 1 or 0
                    string message = dt.Rows[0][1].ToString();          // success message
                    long configId = Convert.ToInt64(dt.Rows[0][2]);     // EmpSalaryConfigID

                    return Json(new
                    {
                        Success = status == 1,
                        Message = message,
                        EmpSalaryConfigID = configId
                    });
                }

                return Json(new { Success = false, Message = "No response from server." });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetEmployeeSalaryConfig(long empSalaryConfigID, long EmployeeID)
        {
            try
            { 
                // Step 2: Call your SP EXACTLY as required
                DataSet ds = clsDatabase.fnDataSet("PRC_Get_EmployeeSalaryConfig",tenantID, empSalaryConfigID,EmployeeID);

                // Safety Check
                if (ds == null || ds.Tables.Count < 3)
                {
                    return Json(new { Success = false, Message = "Invalid salary config response." },
                                JsonRequestBehavior.AllowGet);
                }

                // ----------------  HEADER  ----------------
                var h = ds.Tables[0].Rows[0];

                var header = new
                {
                    EmpSalaryConfigID = h["EmpSalaryConfigID"],
                    EmployeeID = h["EmployeeID"],
                    PayGroupID = h["PayGroupID"],
                    IsPF_Applicable = h["IsPF"],
                    IsESIC_Applicable = h["IsESIC"],
                    IsPTAX_Applicable = h["IsPTAX"],
                    MonthlyGross = h["MonthlyGross"],
                    TotalAllowance = h["TotalAllowance"],
                    TotalDeduction = h["TotalDeduction"],
                    TotalContribution = h["TotalContribution"],
                    NetPay = h["NetPay"],
                    MonthlyCTC = h["MonthlyCTC"],
                    AnnualCTC = h["AnnualCTC"]
                };

                // ----------------  DETAIL LIST  ----------------
                var detailList = ds.Tables[1].AsEnumerable().Select(r => new
                {
                    PayConfigId = r["PayConfigId"],
                    PayConfigName = r["PayConfigName"],
                    PayType = r["PayType"],
                    MonthlyAmount = r["MonthlyAmount"],
                    YearlyAmount = r["YearlyAmount"],
                    TimeBasis = r["TimeBasis"],
                    LogicType = r["LogicType"],
                    MappedColumn = r["MappedColumn"],
                    CalculationFormula = r["CalculationFormula"],
                    ManualRate = r["ManualRate"],
                    ISPercentage = r["ISPercentage"],
                    IsBasicComponent = r["IsBasicComponent"],
                    IsGrossComponent = r["IsGrossComponent"],
                    IsStatutory = r["IsStatutory"],
                    StatutoryType = r["StatutoryType"],
                    IsOther = r["IsOther"],
                    OtherType = r["OtherType"],
                    MaxLimit = r["MaxLimit"],
                    RoundingType = r["RoundingType"],
                    PTaxSlabsJson = r["PTaxSlabsJson"]
                }).ToList();

                // ----------------  TEMPLATE (MASTER PAYGROUP RULES) ----------------
                var template = ds.Tables[2].AsEnumerable().Select(r => new
                {
                    PayConfigId = r["PayConfigId"],
                    PayConfigName = r["PayConfigName"],
                    PayType = r["PayType"],
                    LogicType = r["LogicType"],
                    TimeBasis = r["TimeBasis"],
                    ManualRate = r["ManualRate"],
                    ISPercentage = r["ISPercentage"],

                    IsBasicComponent = r["IsBasicComponent"],
                    IsGrossComponent = r["IsGrossComponent"],
                    IsLoanComponent = r["IsLoanComponent"],
                    IsCalculative = r["IScalculative"],
                    IsVariableAmt = r["IsVariableAmt"],

                    IsStatutory = r["IsStatutory"],
                    StatutoryType = r["StatutoryType"],
                    MaxLimit = r["MaxLimit"],
                    RoundingType = r["RoundingType"],
                    IsOther = r["IsOther"],
                    OtherType = r["OtherType"],
                    MappedColumn = r["MappedColumn"],
                    CalculationFormula = r["CalculationFormula"],
                    PTAXSlabs = r["ptaxSlabs"]?.ToString()
                }).ToList();

                return Json(new
                {
                    Success = true,
                    Data = new
                    {
                        Header = header,
                        DetailList = detailList,
                        Template = template
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SalaryRevision()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "EMPLOYEE-CTC-REVISIED";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        public JsonResult GetEmployeeSalaryList(List<string> companyIds, List<string> deptIds, List<string> categoryIds)
        {

            string companyFilter = NormalizeMultiSelect(companyIds);
            string deptFilter = NormalizeMultiSelect(deptIds);
            string categoryFilter = NormalizeMultiSelect(categoryIds);

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_EMPLOYEE_SALARY_STRUCTURE", tenantID, companyFilter, deptFilter, categoryFilter);

            var list = dt.AsEnumerable().Select(r => new
            {
                EmpID = r["EmpID"]?.ToString(),
                EmpSalaryConfigID = r["EmpSalaryConfigID"]?.ToString(),

                EmpName = r["EmpName"]?.ToString(),
                EmpCode = r["EmpCode"]?.ToString(),

                Designation = r["Designation"]?.ToString(),
                Post = r["Postname"]?.ToString(),
                Department = r["Department"]?.ToString(),

                PayGroupName = r["PayGroupName"]?.ToString(),

                Gross = r["Gross"]?.ToString(),
                Deduction = r["Deduction"]?.ToString(),
                NetPay = r["Net Pay"]?.ToString(),

                Status = r["CTCStatus"]?.ToString(),   // Configured / Pending
                Empstatus = r["Status"]?.ToString(),   // ACTIVE / INACTIVE

                /* 🔑 REQUIRED FOR JS FILTERING */
                CompanyID = r["CompanyID"]?.ToString(),
                DepartmentID = r["DepartmentID"]?.ToString(),
                CategoryID = r["CategoryID"]?.ToString(),
                PayGroupID = r["PayGroupID"]?.ToString()
            }).ToList();


            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateEmployeeSalaryStructure(EmpSalaryConfigModel model)
        {
            try
            {

                string UserName = Session["UserName"].ToString();

                string JsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(model);

                DataTable dt = clsDatabase.fnDataTable("PRC_Update_EmployeeSalaryConfig", tenantID, JsonPayload, UserName);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int status = Convert.ToInt32(dt.Rows[0][0]);        // 1 or 0
                    string message = dt.Rows[0][1].ToString();          // success message
                    long configId = Convert.ToInt64(dt.Rows[0][2]);     // EmpSalaryConfigID

                    return Json(new
                    {
                        Success = status == 1,
                        Message = message,
                        EmpSalaryConfigID = configId
                    });
                }

                return Json(new { Success = false, Message = "No response from server." });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }
    }
}

    