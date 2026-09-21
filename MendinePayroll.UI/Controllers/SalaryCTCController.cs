
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
            string UserId = Session["UserId"].ToString();
            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_PAYGROUP", tenantID, UserId);
            var list = dt.AsEnumerable().Select(r => new
            {
                PayGroupID = r["PayGroupID"].ToString(),
                PayGroupName = r["PayGroupName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPayGroupListConfigWise()
        {
            string UserId = Session["UserId"].ToString();
            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_PAYGROUP_CONFIG", tenantID, UserId);

            var list = dt.AsEnumerable().Select(r => new
            {
                PayGroupID = r["PayGroupID"].ToString(),
                PayGroupName = r["PayGroupName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEmployeeList(List<string> companyIds, List<string> deptIds, List<string> categoryIds)
        {
            string UserId = Session["UserId"].ToString();
            string companyFilter = NormalizeMultiSelect(companyIds);
            string deptFilter = NormalizeMultiSelect(deptIds);
            string categoryFilter = NormalizeMultiSelect(categoryIds);

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_EMPLOYEE_CTC",tenantID,companyFilter,deptFilter,categoryFilter, UserId);

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
        public JsonResult SaveEmployeeSalaryStructure(EmpSalaryConfigRevisionModel model)
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
                DataSet ds = clsDatabase.fnDataSet(
                    "PRC_Get_EmployeeSalaryConfig",
                    tenantID,
                    empSalaryConfigID,
                    EmployeeID
                );

                if (ds == null || ds.Tables.Count < 3 || ds.Tables[0].Rows.Count == 0)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Invalid salary config response."
                    }, JsonRequestBehavior.AllowGet);
                }

                // ================= HEADER =================
                var h = ds.Tables[0].Rows[0];

                var header = new
                {
                    EmpSalaryConfigID = Convert.ToInt64(h["EmpSalaryConfigID"]),
                    EmployeeID = Convert.ToInt64(h["EmployeeID"]),
                    PayGroupID = Convert.ToInt64(h["PayGroupID"]),

                    IsPF_Applicable = Convert.ToBoolean(h["IsPF"]),
                    IsESIC_Applicable = Convert.ToBoolean(h["IsESIC"]),
                    IsPTAX_Applicable = Convert.ToBoolean(h["IsPTAX"]),
                    EffectiveFrom = Convert.ToDateTime(h["EffectiveDate"]),
                    MonthlyGross = Convert.ToDecimal(h["MonthlyGross"]),
                    TotalAllowance = Convert.ToDecimal(h["TotalAllowance"]),
                    TotalDeduction = Convert.ToDecimal(h["TotalDeduction"]),
                    TotalContribution = Convert.ToDecimal(h["TotalContribution"]),
                    NetPay = Convert.ToDecimal(h["NetPay"]),
                    MonthlyCTC = Convert.ToDecimal(h["MonthlyCTC"]),
                    AnnualCTC = Convert.ToDecimal(h["AnnualCTC"])
                };

                // ================= DETAIL =================
                var detailList = ds.Tables[1].AsEnumerable().Select(r => new
                {
                    PayConfigId = Convert.ToInt64(r["PayConfigId"]),
                    PayConfigName = Convert.ToString(r["PayConfigName"]),
                    PayType = Convert.ToString(r["PayType"]),

                    MonthlyAmount = Convert.ToDecimal(r["MonthlyAmount"]),
                    YearlyAmount = Convert.ToDecimal(r["YearlyAmount"]),

                    TimeBasis = Convert.ToString(r["TimeBasis"]),
                    LogicType = Convert.ToString(r["LogicType"]),
                    MappedColumn = Convert.ToString(r["MappedColumn"]),
                    CalculationFormula = Convert.ToString(r["CalculationFormula"]),

                    ManualRate = Convert.ToDecimal(r["ManualRate"]),
                    ISPercentage = Convert.ToBoolean(r["ISPercentage"]),

                    IsBasicComponent = Convert.ToBoolean(r["IsBasicComponent"]),
                    IsGrossComponent = Convert.ToBoolean(r["IsGrossComponent"]),
                    IsTaxable = (ds.Tables[1].Columns.Contains("IsTaxable") && r["IsTaxable"] != DBNull.Value && Convert.ToBoolean(r["IsTaxable"])) || (ds.Tables[1].Columns.Contains("isTaxable") && r["isTaxable"] != DBNull.Value && Convert.ToBoolean(r["isTaxable"])),
                    IsStatutory = Convert.ToBoolean(r["IsStatutory"]),
                    StatutoryType = Convert.ToString(r["StatutoryType"]),

                    IsOther = Convert.ToBoolean(r["IsOther"]),
                    OtherType = Convert.ToString(r["OtherType"]),

                    MaxLimit = r["MaxLimit"] == DBNull.Value ? 0 : Convert.ToDecimal(r["MaxLimit"]),
                    RoundingType = Convert.ToString(r["RoundingType"]),

                    PTaxSlabsJson = r["PTaxSlabsJson"]?.ToString()
                }).ToList();

                // ================= TEMPLATE =================
                var template = ds.Tables[2].AsEnumerable().Select(r => new
                {
                    PayConfigId = Convert.ToInt64(r["PayConfigId"]),
                    PayConfigName = Convert.ToString(r["PayConfigName"]),
                    PayType = Convert.ToString(r["PayType"]), // 🔴 IMPORTANT FIX

                    LogicType = Convert.ToString(r["LogicType"]),
                    TimeBasis = Convert.ToString(r["TimeBasis"]),

                    ManualRate = Convert.ToDecimal(r["ManualRate"]),
                    ISPercentage = Convert.ToBoolean(r["ISPercentage"]),

                    IsBasicComponent = Convert.ToBoolean(r["IsBasicComponent"]),
                    IsGrossComponent = Convert.ToBoolean(r["IsGrossComponent"]),
                    IsTaxable = (ds.Tables[2].Columns.Contains("IsTaxable") && r["IsTaxable"] != DBNull.Value && Convert.ToBoolean(r["IsTaxable"])) || (ds.Tables[2].Columns.Contains("isTaxable") && r["isTaxable"] != DBNull.Value && Convert.ToBoolean(r["isTaxable"])),
                    IsLoanComponent = Convert.ToBoolean(r["IsLoanComponent"]),
                    IsCalculative = Convert.ToBoolean(r["IScalculative"]),
                    IsVariableAmt = Convert.ToBoolean(r["IsVariableAmt"]),

                    IsStatutory = Convert.ToBoolean(r["IsStatutory"]),
                    StatutoryType = Convert.ToString(r["StatutoryType"]),

                    MaxLimit = r["MaxLimit"] == DBNull.Value ? 0 : Convert.ToDecimal(r["MaxLimit"]),
                    RoundingType = Convert.ToString(r["RoundingType"]),

                    IsOther = Convert.ToBoolean(r["IsOther"]),
                    OtherType = Convert.ToString(r["OtherType"]),

                    MappedColumn = Convert.ToString(r["MappedColumn"]),
                    CalculationFormula = Convert.ToString(r["CalculationFormula"]),

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
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
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
            string UserId = Session["UserId"].ToString();
            string companyFilter = NormalizeMultiSelect(companyIds);
            string deptFilter = NormalizeMultiSelect(deptIds);
            string categoryFilter = NormalizeMultiSelect(categoryIds);

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_EMPLOYEE_SALARY_STRUCTURE", tenantID, companyFilter, deptFilter, categoryFilter, UserId);

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
        public JsonResult UpdateEmployeeSalaryStructure(EmpSalaryConfigRevisionModel model)
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

    