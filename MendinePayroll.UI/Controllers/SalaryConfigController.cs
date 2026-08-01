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
using System.Globalization;
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


            return Json(new { Code = 0, Message = "Please enter Component Name" });
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
                    PayConfigId = GetInt(row["PayConfigId"]),
                    PayConfigName = GetString(row["PayConfigName"]),
                    PayConfigType = GetString(row["PayConfigType"]),

                    // ✅ SAFE BOOLEAN HANDLING (string/int/bit সব support করবে)
                    IScalculative = GetBool(row["IScalculative"]),
                    IsStatutory = GetNullableBool(row["IsStatutory"]),
                    IsGrossComponent = GetBool(row["IsGrossComponent"]),
                    IsBasicComponent = GetBool(row["IsBasicComponent"]),
                    IsVariableAmt = GetBool(row["IsVariableAmt"]),
                    IsLoanComponent = GetBool(row["IsLoanComponent"]),

                    StatutoryType = GetString(row["StatutoryType"]),
                    SortOrder = GetInt(row["SortOrder"]),

                    OtherType = GetString(row["OtherType"]),
                    IsOther = GetNullableBool(row["IsOther"]),

                    EntryType = GetString(row["EntryType"]),

                    PayConfigList = new List<object>() // future use
                }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
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
        public static string GetString(object value)
        {
            return value == DBNull.Value || value == null ? null : value.ToString();
        }

        public static int GetInt(object value)
        {
            if (value == DBNull.Value || value == null)
                return 0;

            int.TryParse(value.ToString(), out int result);
            return result;
        }

        public static bool GetBool(object value)
        {
            if (value == DBNull.Value || value == null)
                return false;

            var str = value.ToString().Trim().ToLower();

            // supports: true/false, 1/0, yes/no
            return str == "true" || str == "1" || str == "yes";
        }

        public static bool? GetNullableBool(object value)
        {
            if (value == DBNull.Value || value == null)
                return null;

            var str = value.ToString().Trim().ToLower();

            if (str == "true" || str == "1" || str == "yes")
                return true;

            if (str == "false" || str == "0" || str == "no")
                return false;

            return null;
        }
        [HttpPost]
        public JsonResult SaveSalaryConfig(SalaryConfigSaveRequest model)
        {
            try
            {
                string EntryUser = Session["UserName"].ToString();
                byte calculationMode = model.CalculationMode == 2 ? (byte)2 : (byte)1;

                //------------------------------------------------------
                // 1️⃣ SAVE HEADER (Insert / Update)
                //------------------------------------------------------
                DataTable dt = clsDatabase.fnDataTable(
                    "SP_SaveSalaryConfigureHeader",
                    model.SalaryConfigureID,
                    model.SalaryConfigureName,
                    model.SalaryConfigureType,
                    model.PayGroupID,
                    calculationMode,
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
        #region ESIC_SetUp
        public List<EmployeeListModel> GetAllEmployee()
        {
            //Saikat Mondal start
            List<EmployeeListModel> list = new List<EmployeeListModel>();
            DataTable dt = clsDatabase.fnDataTable("EmployeeList_Get", TenantId);

            if (dt != null && dt.Rows.Count > 0)
            {
                list = (from DataRow row in dt.Rows
                        select new EmployeeListModel
                        {
                            empid = Convert.ToInt32(row["empid"]),
                            empno = Convert.ToInt32(row["empno"]),
                            EmployeeName = row["EmployeeName"].ToString(),
                            empcode = row["empcode"].ToString(),
                            postname = row["postname"].ToString(),
                            Designation = row["Designation"].ToString(),
                            Department = row["Department"].ToString(),
                            //PayGroupName = row["PayGroupName"] == DBNull.Value ? null : row["PayGroupName"].ToString()
                        }).ToList();
            }

            return list;
            
        }
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
        public ActionResult ESIC_SetUp()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            // Esic Process
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "ESIC-EMPLOYEE-PROCESS";
            clsAccessLog.AccessLog_Save(info);
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            #region bind Employee
            EmployeeListModel employeeListModel = new EmployeeListModel();
            List<SelectListItem> EmpList = new List<SelectListItem>();
            EmpList.Add(new SelectListItem { Text = "Select Employee", Value = "0", Selected = true });
            employeeListModel.empid = 0;
            List<EmployeeListModel> employeedata = GetAllEmployee();
            foreach (var items in employeedata)
            {
                EmpList.Add(new SelectListItem
                {
                    Text = $"{items.EmployeeName} - {items.empno}",
                    Value = items.empid.ToString()
                });
            }
            #endregion
            employeeSalaryModel.masterModel.selectListItems = EmpList;
            employeeSalaryModel.MonthList = GetMonthList();
            employeeSalaryModel.YearList = GetYearList();
            return View(employeeSalaryModel);
        }
        [HttpPost]
        public JsonResult SaveEmployeeSlab(int EmployeeId, int SlabId, int IsEligible, string remark)
        {
            try
            {
                // Insert into DB (Payroll_ESIC_EmployeeSlab)
                Payroll_ESIC_EmployeeSlab model = new Payroll_ESIC_EmployeeSlab
                {
                    EmployeeId = EmployeeId,
                    SlabId = SlabId,
                    IsEligible = Convert.ToBoolean(IsEligible),
                    Remark = remark,
                    CreatedBy = Session["UserName"].ToString()
                };

                clsDatabase.fnDataTable("SP_Save_ESIC_EmployeeSlab", model.EmployeeId, model.SlabId, model.IsEligible, model.Remark, model.CreatedBy,TenantId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetEmployeeSlabs()
        {
            // Fetch from DB (Payroll_ESIC_EmployeeSlab with join on Employee and SlabMaster)
            DataTable DT = clsDatabase.fnDataTable("SP_Get_ESIC_EmployeeSlabs",TenantId);
            var data = DT.AsEnumerable().Select(row => new
            {
                EmployeeSlabId = row.Field<int>("EmployeeSlabId"),
                EmployeeName = row.Field<string>("EmployeeName"),
                SlabName = row.Field<string>("SlabName"),
                IsEligible = row.Field<bool>("IsEligible")
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteEmployeeSlab(int EmployeeSlabId)
        {
            try
            {
                DataTable dataTable = clsDatabase.fnDataTable("SP_Delete_ESIC_EmployeeSlab", EmployeeSlabId,TenantId);
                return Json(new { success = true, message = "Delete Successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Payroll_Cycle_Configaration
        [HttpGet]
        public ActionResult Payroll_Cycle_Configaration()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "PAYROLL-CYCLE-CONFIGURATION";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        [HttpGet]
        public JsonResult GetPayrollCycleConfigurations()
        {
            try
            {
                DataTable dt = clsDatabase.fnDataTable("SP_PayrollCycleConfiguration_List", GetPayrollTenantId());
                var list = BuildPayrollCycleConfigurationList(dt);

                return Json(new { Success = true, Data = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetPayrollCycleConfiguration(int id)
        {
            try
            {
                DataTable dt = clsDatabase.fnDataTable("SP_PayrollCycleConfiguration_GetById", id, GetPayrollTenantId());

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new { Success = false, Message = "Configuration not found." }, JsonRequestBehavior.AllowGet);
                }

                DataRow row = dt.Rows[0];
                var model = new
                {
                    PayrollConfigID = Convert.ToInt32(row["PayrollConfigID"]),
                    TenantID = row["TenantID"].ToString(),
                    TypeID = Convert.ToByte(row["TypeID"]),
                    PayrollCycleType = row["PayrollCycleType"] == DBNull.Value ? (byte?)null : Convert.ToByte(row["PayrollCycleType"]),
                    StartDay = row["StartDay"] == DBNull.Value ? (byte?)null : Convert.ToByte(row["StartDay"]),
                    EndDay = row["EndDay"] == DBNull.Value ? (byte?)null : Convert.ToByte(row["EndDay"]),
                    PaidDaysSource = row["PaidDaysSource"] == DBNull.Value ? (byte)1 : Convert.ToByte(row["PaidDaysSource"]),
                    IsActive = Convert.ToBoolean(row["IsActive"])
                };

                return Json(new { Success = true, Data = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SavePayrollCycleConfiguration(PayrollCycleConfigurationModel model)
        {
            try
            {
                ValidatePayrollCycleConfiguration(model);

                Guid tenantId = GetPayrollTenantId();
                string userName = GetPayrollUserName();
                DataTable dt = clsDatabase.fnDataTable(
                    "SP_PayrollCycleConfiguration_Save",
                    model.PayrollConfigID,
                    tenantId,
                    model.TypeID,
                    (object)model.PayrollCycleType ?? DBNull.Value,
                    (object)model.StartDay ?? DBNull.Value,
                    (object)model.EndDay ?? DBNull.Value,
                    model.PaidDaysSource,
                    userName
                );

                return BuildPayrollCycleProcedureResponse(dt, "Payroll cycle configuration saved successfully.");
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeletePayrollCycleConfiguration(int id)
        {
            try
            {
                DataTable dt = clsDatabase.fnDataTable(
                    "SP_PayrollCycleConfiguration_Delete",
                    id,
                    GetPayrollTenantId(),
                    GetPayrollUserName()
                );

                return BuildPayrollCycleProcedureResponse(dt, "Payroll cycle configuration deleted successfully.");
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }

        private void ValidatePayrollCycleConfiguration(PayrollCycleConfigurationModel model)
        {
            if (model == null)
                throw new Exception("Invalid payroll cycle configuration.");

            if (model.TypeID != 1 && model.TypeID != 2)
                throw new Exception("Select a valid payroll type.");

            if (model.PaidDaysSource != 1 && model.PaidDaysSource != 2)
                model.PaidDaysSource = 1;

            if (model.TypeID == 2)
            {
                model.PayrollCycleType = null;
                model.StartDay = null;
                model.EndDay = null;
                return;
            }

            if (!model.PayrollCycleType.HasValue || (model.PayrollCycleType.Value != 1 && model.PayrollCycleType.Value != 2))
                throw new Exception("Select a valid payroll cycle type.");

            if (model.PayrollCycleType.Value == 1)
            {
                model.StartDay = null;
                model.EndDay = null;
                return;
            }

            if (!model.StartDay.HasValue || model.StartDay.Value < 1 || model.StartDay.Value > 31)
                throw new Exception("Payroll Starts On must be between 1 and 31.");

            if (!model.EndDay.HasValue || model.EndDay.Value < 1 || model.EndDay.Value > 31)
                throw new Exception("Payroll Ends On must be between 1 and 31.");
        }

        private Guid GetPayrollTenantId()
        {
            Guid tenantId;
            if (Session["TenantID"] == null || !Guid.TryParse(Session["TenantID"].ToString(), out tenantId))
                throw new Exception("Invalid TenantID in session. Payroll_CycleConfiguration requires UNIQUEIDENTIFIER TenantID.");

            return tenantId;
        }

        private string GetPayrollUserName()
        {
            string userName = Session["UserName"] == null ? "" : Session["UserName"].ToString();

            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("Invalid UserName in session. Payroll_CycleConfiguration requires CreatedBy/ModifiedBy user name.");

            return userName.Length > 50 ? userName.Substring(0, 50) : userName;
        }

        private string GetPayrollTypeName(byte typeId)
        {
            return typeId == 1 ? "Monthly" : typeId == 2 ? "Contractual" : "-";
        }

        private string GetPayrollCycleTypeName(byte? payrollCycleType)
        {
            if (!payrollCycleType.HasValue)
                return "-";

            return payrollCycleType == 1 ? "Calendar Month" : payrollCycleType == 2 ? "Fixed Payroll Cycle" : "-";
        }

        private string GetPayrollPeriodText(byte typeId, byte? payrollCycleType, byte? startDay, byte? endDay)
        {
            if (typeId == 2)
                return "Manual From Date and To Date during salary processing";

            if (payrollCycleType == 1)
                return "1st day to last day of selected month";

            if (payrollCycleType == 2)
                return "Day " + startDay + " to Day " + endDay;

            return "-";
        }

        private string GetPaidDaysSourceName(byte paidDaysSource)
        {
            return paidDaysSource == 2 ? "Attendance Data" : "Month/Period Days";
        }

        private List<object> BuildPayrollCycleConfigurationList(DataTable dt)
        {
            var list = new List<object>();

            if (dt == null)
                return list;

            foreach (DataRow row in dt.Rows)
            {
                byte typeId = Convert.ToByte(row["TypeID"]);
                byte? cycleType = row["PayrollCycleType"] == DBNull.Value ? (byte?)null : Convert.ToByte(row["PayrollCycleType"]);
                byte? startDay = row["StartDay"] == DBNull.Value ? (byte?)null : Convert.ToByte(row["StartDay"]);
                byte? endDay = row["EndDay"] == DBNull.Value ? (byte?)null : Convert.ToByte(row["EndDay"]);
                byte paidDaysSource = row["PaidDaysSource"] == DBNull.Value ? (byte)1 : Convert.ToByte(row["PaidDaysSource"]);

                list.Add(new
                {
                    PayrollConfigID = Convert.ToInt32(row["PayrollConfigID"]),
                    TenantID = row["TenantID"].ToString(),
                    TypeID = typeId,
                    TypeName = GetPayrollTypeName(typeId),
                    PayrollCycleType = cycleType,
                    PayrollCycleTypeName = GetPayrollCycleTypeName(cycleType),
                    StartDay = startDay,
                    EndDay = endDay,
                    PeriodText = GetPayrollPeriodText(typeId, cycleType, startDay, endDay),
                    PaidDaysSource = paidDaysSource,
                    PaidDaysSourceName = GetPaidDaysSourceName(paidDaysSource),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedOn = Convert.ToDateTime(row["CreatedOn"]).ToString("dd-MMM-yyyy"),
                    ModifiedOn = row["ModifiedOn"] == DBNull.Value ? "" : Convert.ToDateTime(row["ModifiedOn"]).ToString("dd-MMM-yyyy")
                });
            }

            return list;
        }

        private JsonResult BuildPayrollCycleProcedureResponse(DataTable dt, string defaultSuccessMessage)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return Json(new { Success = true, Message = defaultSuccessMessage, PayrollConfigID = 0 });
            }

            DataRow row = dt.Rows[0];
            int code = dt.Columns.Contains("Code") ? Convert.ToInt32(row["Code"]) : 1;
            string message = dt.Columns.Contains("Message") ? row["Message"].ToString() : defaultSuccessMessage;
            int payrollConfigId = 0;

            if (dt.Columns.Contains("PayrollConfigID") && row["PayrollConfigID"] != DBNull.Value)
            {
                payrollConfigId = Convert.ToInt32(row["PayrollConfigID"]);
            }

            return Json(new
            {
                Success = code == 1,
                Message = message,
                PayrollConfigID = payrollConfigId
            });
        }

        #endregion

    }

    public class PayrollCycleConfigurationModel
    {
        public int PayrollConfigID { get; set; }
        public byte TypeID { get; set; }
        public byte? PayrollCycleType { get; set; }
        public byte? StartDay { get; set; }
        public byte? EndDay { get; set; }
        public byte PaidDaysSource { get; set; }
    }

    public class SalaryConfigSaveRequest
    {
        public int SalaryConfigureID { get; set; }
        public string SalaryConfigureName { get; set; }
        public int SalaryConfigureType { get; set; }
        public int PayGroupID { get; set; }
        public byte CalculationMode { get; set; } = 1;
        public string ComponentListJson { get; set; }
    }
}
