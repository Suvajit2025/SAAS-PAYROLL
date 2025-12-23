using Common.Utility;
using DocumentFormat.OpenXml.Office2010.Excel;
using MendinePayroll.Models;
using MendinePayroll.UI.Models;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.BLL;

namespace MendinePayroll.UI.Controllers
{
    public class SalaryConfigController : Controller
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
        // GET: SalaryConfig
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult StatutoryRuls()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "STATUTORY-RULES-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        [HttpPost]
        public JsonResult GetStatutoryComponents()
        { 
            DataTable dt = clsDatabase.fnDataTable("SP_GetPayrollStatutoryComponents", TenantId);

            var list = dt.AsEnumerable().Select(r => new
            {
                PayConfigId = r["PayConfigId"].ToString(),
                PayConfigName = r["PayConfigName"].ToString(),
                StatutoryType = r["StatutoryType"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetRoundingTypes()
        {
            string tenantId = Session["TenantID"].ToString();

            DataTable dt = clsDatabase.fnDataTable("SP_GetPayrollRoundingTypes");

            var list = dt.AsEnumerable().Select(r => new
            {
                RoundingTypeId = r["RoundingTypeId"].ToString(),
                RoundingType = r["RoundingType"].ToString() 
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetPTaxStates()
        {
            string tenantId = Session["TenantID"].ToString();

            DataTable dt = clsDatabase.fnDataTable("Proc_Getallstate");

            var list = dt.AsEnumerable().Select(r => new
            {
                StateId = r["stateid"].ToString(),
                StateName = r["statename"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveStatutoryRules(StatutoryRuleModel model)
        {
            try
            {
                string EntryUser = Session["UserName"].ToString(); 

                if (model == null)
                    return Json(new { success = false, message = "Invalid data." });

                object StatutoryRuleID = (model.StatutoryRuleId == null || model.StatutoryRuleId == 0)
                               ? null
                               : model.StatutoryRuleId;
                // Call Stored Procedure (Correct Parameter Order)
                DataTable dt = clsDatabase.fnDataTable( "SP_Save_PayrollStatutoryRules", StatutoryRuleID, TenantId,model.PayConfigId,model.StatutoryType,
                    model.MaxLimit,model.RoundingType,model.LWFMonth,model.LWFAmount,model.StateId,model.EffectiveDate,EntryUser,model.SlabJson);

                // ===== READ RETURNED VALUES =====
                if (dt.Rows.Count > 0)
                {
                    return Json(new
                    {
                        Code = Convert.ToInt32(dt.Rows[0]["Code"]),
                        Message = dt.Rows[0]["Message"].ToString(),
                        StatutoryRuleID = Convert.ToInt64(dt.Rows[0]["StatutoryRuleID"])
                    });
                }

                return Json(new { Code = 0, Message = "No response from SP" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GetStatutoryRules()
        {
            try
            {
                 
                // Load data from SP
                DataTable dt = clsDatabase.fnDataTable("SP_Get_PayrollStatutoryRules", TenantId);

                var list = dt.AsEnumerable().Select(r => new
                {
                    StatutoryRuleID = Convert.ToInt64(r["StatutoryRuleID"]),
                    PayConfigId = Convert.ToInt32(r["PayConfigId"]),
                    PayConfigName = r["PayConfigName"].ToString(),
                    StatutoryType = r["StatutoryType"].ToString(),
                    MaxLimit = r["MaxLimit"] == DBNull.Value ? null : r["MaxLimit"].ToString(),
                    RoundingType = r["RoundingType"].ToString(),
                    LWFMonth = r["LWFMonth"].ToString(),
                    LWFAmount = r["LWFAmount"].ToString(),
                    StateId = r["StateId"] == DBNull.Value ? null : r["StateId"].ToString(),
                    EffectiveDate = Convert.ToDateTime(r["EffectiveDate"]).ToString("dd-MMM-yyyy"),

                    // Display text (auto generated for grid)
                    DisplayValue = r["DisplayValue"].ToString()
                }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Code = 0, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetStatutoryRuleById(long statutoryRuleId)
        {
            try
            {
              
                // Call SP or query to get Rule
                DataSet ds = clsDatabase.fnDataSet("SP_Get_PayrollStatutoryRuleById",statutoryRuleId,TenantId);

                if (ds == null || ds.Tables.Count == 0)
                    return Json(new { success = false, message = "Rule not found." });

                DataTable dtRule = ds.Tables[0];   // single rule
                DataTable dtSlabs = (ds.Tables.Count > 1) ? ds.Tables[1] : null; // PTAX slabs

                if (dtRule.Rows.Count == 0)
                    return Json(new { success = false, message = "Invalid Rule ID." });

                var row = dtRule.Rows[0];

                var model = new
                {
                    StatutoryRuleId = Convert.ToInt64(row["StatutoryRuleID"]),
                    PayConfigId = Convert.ToInt32(row["PayConfigId"]),
                    StatutoryType = row["StatutoryType"].ToString(),

                    MaxLimit = row["MaxLimit"] != DBNull.Value ? Convert.ToDecimal(row["MaxLimit"]) : (decimal?)null,
                    RoundingType = row["RoundingType"]?.ToString(),

                    LWFMonth = row["LWFMonth"]?.ToString(),
                    LWFAmount = row["LWFAmount"] != DBNull.Value ? Convert.ToDecimal(row["LWFAmount"]) : (decimal?)null,

                    StateId = row["StateId"] != DBNull.Value ? Convert.ToInt32(row["StateId"]) : (int?)null,
                    EffectiveDate = Convert.ToDateTime(row["EffectiveDate"]).ToString("yyyy-MM-dd"),

                    Slabs = dtSlabs != null
                        ? dtSlabs.AsEnumerable()
                    .Select(s => new
                    {
                        Min = Convert.ToDecimal(s["MinSalary"]),
                        Max = Convert.ToDecimal(s["MaxSalary"]),
                        Value = Convert.ToDecimal(s["PTAXAmount"])
                    })
                    .Cast<object>()
                    .ToList()
                    : new List<object>()
                };


                return Json(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteStatutoryRule(long statutoryRuleId)
        {
            try
            {
                

                DataTable dt = clsDatabase.fnDataTable("SP_Delete_PayrollStatutoryRule",statutoryRuleId, TenantId);

                if (dt.Rows.Count > 0)
                {
                    int code = Convert.ToInt32(dt.Rows[0]["Code"]);
                    string msg = dt.Rows[0]["Message"].ToString();

                    return Json(new { Code = code, Message = msg });
                }

                return Json(new { Code = 0, Message = "No response from SP" });
            }
            catch (Exception ex)
            {
                return Json(new { Code = 0, Message = ex.Message });
            }
        }

        //Pay Component Master
        public ActionResult PayComponentMaster()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "PAY-COMPONENET-MASTER-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        [HttpGet]
        public JsonResult GetPayComponents()
        {
            try
            {
                 
                DataTable dt = clsDatabase.fnDataTable("SP_GetPayConfigMaster",TenantId);

                var list = dt.AsEnumerable().Select(r => new
                {
                    id = Convert.ToInt32(r["id"]),
                    name = r["name"].ToString(),
                    type = r["type"].ToString(),

                    isCalc = Convert.ToInt32(r["isCalc"]) == 1,
                    sort = Convert.ToInt32(r["sort"]),

                    isGross = Convert.ToInt32(r["isGross"]) == 1,
                    isBasic = Convert.ToInt32(r["isBasic"]) == 1,
                    //Statutory 
                    isStat = Convert.ToInt32(r["isStat"]) == 1, 
                    statKey = r["statKey"] == DBNull.Value ? "" : r["statKey"].ToString(),
                    //Others
                    isOther = Convert.ToInt32(r["isOther"]) == 1, 
                    otherKey = r["otherKey"] == DBNull.Value ? "" : r["otherKey"].ToString(),
                    //Allownces
                    isAllowance = Convert.ToInt32(r["isAllowance"]) == 1,
                    allowanceKey = r["allowanceKey"] == DBNull.Value ? "" : r["allowanceKey"].ToString(),

                    date = r["date"].ToString()
                }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetAllowanceKeys()
        {
            DataTable dt = clsDatabase.fnDataTable("Sp_GetAllowanceKeys");

            var list = dt.AsEnumerable().Select(r => new
            {
                key = r["AllowanceKey"].ToString(),
                name = r["AllowanceName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetStatutoryKeys()
        {
            DataTable dt = clsDatabase.fnDataTable("Sp_GetStatutorykey");

            var list = dt.AsEnumerable().Select(r => new
            {
                key = r["StatKey"].ToString(),
                name = r["StatName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetOtherKeys()
        {
            DataTable dt = clsDatabase.fnDataTable("Sp_GetOtherkey");

            var list = dt.AsEnumerable().Select(r => new
            {
                key = r["OtherKey"].ToString(),
                name = r["OtherName"].ToString()
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetPayComponentById(int Id)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_GetPayConfigByID", Id, TenantId);

            if (dt.Rows.Count == 0)
                return Json(null, JsonRequestBehavior.AllowGet);

            DataRow r = dt.Rows[0];

            var obj = new
            {
                id = Convert.ToInt32(r["PayConfigId"]),
                name = r["PayConfigName"].ToString(),
                type = r["PayConfigType"].ToString(),
                isCalc = Convert.ToInt32(r["IScalculative"]),
                sort = Convert.ToInt32(r["SortOrder"]),
                isBasic = Convert.ToInt32(r["IsBasicComponent"]) == 1,
                isGross = Convert.ToInt32(r["IsGrossComponent"]) == 1,
                isVar = Convert.ToInt32(r["IsVariableAmt"]) == 1,
                isLoan = Convert.ToInt32(r["IsLoanComponent"]) == 1,
                isStat = Convert.ToInt32(r["IsStatutory"]) == 1,
                statKey = r["StatutoryType"] == DBNull.Value ? "" : r["StatutoryType"].ToString(),
                isOthers = Convert.ToInt32(r["IsOthers"]) == 1,
                otherKey = r["OtherType"] == DBNull.Value ? "" : r["OtherType"].ToString(),
                isAllowance = Convert.ToInt32(r["IsAllowance"]) == 1,
                allowanceKey = r["AllowanceType"] == DBNull.Value ? "" : r["AllowanceType"].ToString()
            };

            return Json(obj, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SavePayComponent(PayComponentModel model)
        {
            
            string UserName = Session["UserName"].ToString();

            DataTable dt = clsDatabase.fnDataTable("SP_SavePayConfig", model.PayConfigId, TenantId,model.PayConfigName,model.PayConfigType,model.IsCalculative,
                model.SortOrder,model.IsBasicComponent, model.IsGrossComponent,model.IsVariableAmt,model.IsLoanComponent,model.IsStatutory, (object)model.StatutoryType ?? "", model.IsOther, (object)model.OtherType ?? ""
                , model.IsAllowance, (object)model.AllowanceType ?? "", UserName
            );

            // ===== READ RETURNED VALUES =====
            if (dt.Rows.Count > 0)
            {
                long? payConfigId = null;

                if (dt.Rows[0]["PayConfigID"] != DBNull.Value &&
                    !string.IsNullOrWhiteSpace(dt.Rows[0]["PayConfigID"].ToString()))
                {
                    payConfigId = Convert.ToInt64(dt.Rows[0]["PayConfigID"]);
                }

                return Json(new
                {
                    Code = Convert.ToInt32(dt.Rows[0]["Code"]),
                    Message = dt.Rows[0]["Message"].ToString(),
                    PayConfigID = payConfigId
                });
            }


            return Json(new { Code = 0, Message = "No response from SP" });
        }

        [HttpPost]
        public JsonResult DeletePayComponent(int id)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_DeletePayConfig", id, TenantId);

            return Json(new
            {
                Code = Convert.ToInt32(dt.Rows[0]["Code"]),
                Message = dt.Rows[0]["Message"].ToString()
            });
        }

        /// <summary>
        /// Build Salary Structure
        /// </summary> 
        public ActionResult SalaryStructureConfiguration()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "SALARY-TEMPLATE-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        [HttpPost]
        public JsonResult GetAllPayConfig()
        {
            try
            { 
                DataTable dt = clsDatabase.fnDataTable("SP_GetAllPayConfig", TenantId);

                var list = dt.AsEnumerable().Select(row => new
                {
                    PayConfigId = row.Field<int>("PayConfigId"),
                    PayConfigName = row.Field<string>("PayConfigName"),
                    PayConfigType = row.Field<string>("PayConfigType"),

                    // Nullable Boolean Conversion
                    IScalculative = row.Field<bool?>("IScalculative") ?? false,
                    IsStatutory = row.Field<bool?>("IsStatutory"),
                    IsGrossComponent = row.Field<bool?>("IsGrossComponent") ?? false,
                    IsBasicComponent = row.Field<bool?>("IsBasicComponent") ?? false,
                    IsVariableAmt = row.Field<bool?>("IsVariableAmt") ?? false,
                    IsLoanComponent = row.Field<bool?>("IsLoanComponent") ?? false,

                    StatutoryType = row["StatutoryType"]?.ToString(),
                    SortOrder = row.Field<int?>("SortOrder") ?? 0,

                    OtherType = row["OtherType"]?.ToString(),
                    IsOther = row.Field<bool?>("IsOther"),

                    EntryType = row.Field<string>("EntryType"),

                    PayConfigList = new List<object>()  // same as JSON structure
                }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SaveSalaryConfig(SalaryConfigModel model)
        {
            try
            {
                string EntryUser = Session["UserName"].ToString();

                //------------------------------------------------------
                // 1️⃣ SAVE HEADER (Insert / Update)
                //------------------------------------------------------
                DataTable dt = clsDatabase.fnDataTable(
                    "SP_SaveSalaryConfigureHeader",
                    model.SalaryConfigureID,
                    model.SalaryConfigureName,
                    model.SalaryConfigureType,
                    model.PayGroupID,
                    TenantId,
                    EntryUser
                );

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new { Success = false, Message = "Invalid response from Header SP." });
                }

                var row = dt.Rows[0];

                if (Convert.ToInt32(row["Success"]) == 0)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = row["Message"].ToString()
                    });
                }

                int SalaryConfigureID = Convert.ToInt32(row["SalaryConfigureID"]);


                //------------------------------------------------------
                // 2️⃣ SAVE DETAIL ROWS USING JSON (Delete + Insert)
                //------------------------------------------------------
                DataTable Detaildt = clsDatabase.fnDataTable(
                    "SP_SaveSalaryComponentDetail",
                    SalaryConfigureID,
                    TenantId,
                    EntryUser,
                    model.ComponentListJson
                );

                if (Detaildt == null || Detaildt.Rows.Count == 0)
                {
                    return Json(new { Success = false, Message = "Invalid response from Detail SP." });
                }

                var detailRow = Detaildt.Rows[0];

                if (Convert.ToInt32(detailRow["Success"]) == 0)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = detailRow["Message"].ToString()
                    });
                }


                //------------------------------------------------------
                // 3️⃣ FINAL SUCCESS RESPONSE
                //------------------------------------------------------
                return Json(new
                {
                    Success = true,
                    Message = "Salary Structure Saved Successfully!",
                    SalaryConfigureID = SalaryConfigureID
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult GetSalaryConfigList()
        {
            DataTable dt = clsDatabase.fnDataTable("SP_GetSalaryConfigList", TenantId);

            var list = dt.AsEnumerable().Select(r => new
            {
                SalaryConfigureID = r["SalaryConfigureID"].ToString(),
                SalaryConfigureName = r["SalaryConfigureName"].ToString(),
                SalaryConfigureType = r["SalaryConfigureType"].ToString(),
                PayGroupName = r["PayGroupName"].ToString(),
                TotalComponents = r["TotalComponents"].ToString()
            }).ToList();

            return Json(list);
        }
        [HttpPost]
        public JsonResult GetSalaryConfigById(int SalaryConfigureID)
        {
            try
            {
                  
                DataSet ds = clsDatabase.fnDataSet("SP_GetSalaryConfigurationFull", SalaryConfigureID, TenantId);

                // Convert DataTable to list
                var header = ConvertDataTableToList(ds.Tables[0]);
                var detail = ConvertDataTableToList(ds.Tables[1]);

                return Json(new
                {
                    Success = true,
                    Header = header,
                    Detail = detail
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // return the exact real error so UI can see it
                return Json(new
                {
                    Success = false,
                    Message = ex.Message,
                    Stack = ex.StackTrace
                });
            }
        }
        public static List<Dictionary<string, object>> ConvertDataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }

                list.Add(dict);
            }

            return list;
        }


        [HttpPost]
        public JsonResult DeleteSalaryConfig(int SalaryConfigureID)
        {
           
            string user = Session["UserName"].ToString();

            try
            {
                DataTable dt = clsDatabase.fnDataTable("SP_DeleteSalaryConfigure",SalaryConfigureID,TenantId);

                var row = dt.Rows[0];

                if (Convert.ToInt32(row["Success"]) == 0)
                {
                    return Json(new { Success = false, Message = row["Message"].ToString() });
                }

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }


    }
}