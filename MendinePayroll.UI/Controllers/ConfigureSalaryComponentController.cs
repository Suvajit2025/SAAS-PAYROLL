using Common.Utility;
using MendinePayroll.Models;
using MendinePayroll.UI.Models;
using MendinePayroll.UI.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using UI.BLL;

namespace MendinePayroll.UI.Controllers
{
    public class ConfigureSalaryComponentController : Controller
    {
        private string TenantID
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

        ApiCommon ObjAPI = new ApiCommon();

        // Common API Call Wrapper
        private T SafeApiCall<T>(string apiEndpoint, object model)
        {
            try
            {
                string contents = JsonConvert.SerializeObject(model);
                HttpResponseMessage response = ObjAPI.CallAPI(apiEndpoint, contents);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"API call failed: {apiEndpoint} - {response.StatusCode}");

                string responseString = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseString))
                    return default(T);

                return JsonConvert.DeserializeObject<T>(responseString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calling {apiEndpoint}: {ex.Message}");
                return default(T);
            }
        }

        // GET: ConfigureSalaryComponent
        public ActionResult Index()
        {
            if (Session["UserName"] == null && Session["TenantID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "CONFIGURE-SALARY-COMPONENT-ENTRY";
            clsAccessLog.AccessLog_Save(info);

            SalaryConfigureModel salaryConfigureModel = new SalaryConfigureModel();
            salaryConfigureModel.SalaryConfigureID = 0;
            salaryConfigureModel.TenantID = TenantID;

            string contents = JsonConvert.SerializeObject(salaryConfigureModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryConfigure/GetAllSalaryConfigureList", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    salaryConfigureModel.SalaryConfigureList = JsonConvert.DeserializeObject<List<SalaryConfigureModel>>(responseString);
                }
            }
            return View(salaryConfigureModel);
        }
        public ActionResult NewConfigureSalary()
        {
            return View();
        }

        public ActionResult dynamicConfigureSalary()
        {
            return View();
        }

        #region Save Configure Salary Component
        //public JsonResult SaveConfigureSalaryComponent(List<ConfigureSalaryComponentModel> InvoiceDList)
        //{
        //    var data = "";
        //    foreach (ConfigureSalaryComponentModel configureSalaryComponentModel in InvoiceDList)
        //    {
        //        string contents = JsonConvert.SerializeObject(configureSalaryComponentModel);
        //        HttpResponseMessage response = ObjAPI.CallAPI("api/ConfigureSalaryComponent/SaveConfigureSalarycomponent", contents);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            string responseString = response.Content.ReadAsStringAsync().Result;

        //            if (!string.IsNullOrEmpty(responseString))
        //            {
        //                data = JsonConvert.DeserializeObject(responseString).ToString();
        //            }
        //        }
        //    }

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult SaveConfigureSalaryComponent(SalaryConfigureVM model)
        {
            try
            {
                var head = model.Head;
                head.TenantID = TenantID;
                head.EntryUser = Session["UserEmail"].ToString();
                head.ActiveFlag = 1;

                // 1. SAVE HEADER
                var HeadDT = clsDatabase.fnDataTable("SP_SalaryConfigure_Save",
                              head.SalaryConfigureID, head.SalaryConfigureName,
                              head.PayGroupID, head.SalaryConfigureType,
                              head.TenantID, head.EntryUser);

                if (HeadDT.Rows.Count == 0)
                    return Json(new { Success = false, Message = "Invalid SP response." });

                // Read returned values
                int code = Convert.ToInt32(HeadDT.Rows[0]["Code"]);
                string message = HeadDT.Rows[0]["Message"].ToString();

                // Pass SP message directly
                if (code <= 0)
                {
                    return Json(new
                    {
                        Success = false,
                        Code = code,
                        Message = message
                    });
                }

                int newHeadId = code; // On success, 'code' holds ID

                // 2. SAVE DETAILS
                foreach (var d in model.Detail)
                {
                    d.TenantID = TenantID;
                    d.EntryUser = head.EntryUser;
                }

                string jsonDetail = JsonConvert.SerializeObject(model.Detail);

                var DetailDT = clsDatabase.fnDataTable("SP_ConfigureSalaryComponent_Save",
                                newHeadId.ToString(), jsonDetail);

                int detailCode = Convert.ToInt32(DetailDT.Rows[0]["Code"]);
                string detailMsg = DetailDT.Rows[0]["Message"].ToString();

                if (detailCode <= 0)
                {
                    return Json(new
                    {
                        Success = false,
                        Code = detailCode,
                        Message = detailMsg
                    });
                }

                // SUCCESS
                return Json(new
                {
                    Success = true,
                    Code = 1,
                    Message = detailMsg,
                    SalaryConfigureID = newHeadId
                });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }



        #endregion

        #region Save  Salary Configure
        public JsonResult SaveSalaryConfigure(string SalaryConfigureName, string PayGroupID,string SalaryType)
        {
            var data = "";
            SalaryConfigureModel salaryConfigureModel = new SalaryConfigureModel();
            salaryConfigureModel.SalaryConfigureID = 0;
            salaryConfigureModel.SalaryConfigureName = SalaryConfigureName;
            salaryConfigureModel.PayGroupID = Convert.ToInt32(PayGroupID);
            salaryConfigureModel.SalaryConfigureType = Convert.ToInt32(SalaryType);
            string contents = JsonConvert.SerializeObject(salaryConfigureModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryConfigure/SaveSalaryConfigure", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region bind Allowances+Company Contribution
        public JsonResult GetPayConfig()
        {
            string message = "All";
            List<PayConfigModel> data = new List<PayConfigModel>();
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigType = message;
            payConfigModel.TenantID = TenantID;
            payConfigModel.EntryUser = Session["UserEmail"].ToString();
            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/GetAllPayconfigbyType", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayConfigModel>>(responseString);
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPayConfigbyTypeAllowances()
        {
            string message = "Allowances";
            List<PayConfigModel> data = new List<PayConfigModel>();
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigType = message;
            payConfigModel.TenantID = TenantID;
            payConfigModel.EntryUser = Session["UserEmail"].ToString();
            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/GetAllPayconfigbyType", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayConfigModel>>(responseString);
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPayConfigbyTypeCC()
        {
            string message = "CC";
            List<PayConfigModel> data = new List<PayConfigModel>();
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigType = message;
            payConfigModel.TenantID = TenantID;
            payConfigModel.EntryUser = Session["UserEmail"].ToString();
            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/GetAllPayconfigbyType", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayConfigModel>>(responseString);
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region bind Deducation
        public JsonResult GetPayConfigbyTypeDeducation()
        {
            string message = "Deduction";
            List<PayConfigModel> data = new List<PayConfigModel>();
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigType = message;
            payConfigModel.TenantID = TenantID;
            payConfigModel.EntryUser = Session["UserEmail"].ToString();
            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/GetAllPayconfigbyType", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayConfigModel>>(responseString);
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //#region Update Salary Configure
        //public ActionResult UpdateSalaryConfigure()
        //{
        //    return View();
        //}
        //#endregion
        #region Update Salary Configure
        public ActionResult NewUpdateSalaryConfigure(int id)
        {
            if (id <= 0)
            {
                // If ID is missing or invalid redirect back
                return RedirectToAction("Index", "ConfigureSalaryComponent");
            }

            // Pass ID to View (so JS can load the record)
            ViewBag.SalaryConfigureID = id;

            return View();
        }
        #endregion

        #region Get SalaryConfig
        public JsonResult GetSalaryConfigureByID(string SalaryConfigureID)
        {
            List<SalaryConfigureModel> data = new List<SalaryConfigureModel>();
            SalaryConfigureModel salaryConfigureModel = new SalaryConfigureModel();
            salaryConfigureModel.SalaryConfigureID = Convert.ToInt32(SalaryConfigureID);
            string contents = JsonConvert.SerializeObject(salaryConfigureModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryConfigure/GetAllSalaryConfigurebyid", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<SalaryConfigureModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Salary Component By id
        //public JsonResult GetConfigureSalaryComponentByID(string ConfigureSalaryID)
        //{
        //    List<ConfigureSalaryComponentModel> data = new List<ConfigureSalaryComponentModel>();
        //    ConfigureSalaryComponentModel configureSalaryComponentModel = new ConfigureSalaryComponentModel();
        //    configureSalaryComponentModel.ConfigureSalaryID = Convert.ToInt32(ConfigureSalaryID);
        //    string contents = JsonConvert.SerializeObject(configureSalaryComponentModel);
        //    HttpResponseMessage response = ObjAPI.CallAPI("api/ConfigureSalaryComponent/GetAllConfigureSalaryComponentbyid", contents);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        string responseString = response.Content.ReadAsStringAsync().Result;
        //        if (!string.IsNullOrEmpty(responseString))
        //        {
        //            data = JsonConvert.DeserializeObject<List<ConfigureSalaryComponentModel>>(responseString); ;
        //        }
        //    }

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        [HttpPost]
        public JsonResult GetConfigureSalaryComponentData(int? SalaryConfigureID)
        {
            try
            {
                if (SalaryConfigureID == null || SalaryConfigureID == 0)
                {
                    return Json(new { Success = false, Message = "SalaryConfigureID is missing" });
                }

                DataSet ds = clsDatabase.fnDataSet("SP_GetSalaryConfigureFullData", SalaryConfigureID.Value);

                if (ds == null || ds.Tables.Count < 2)
                {
                    return Json(new { Success = false, Message = "No data found" });
                }

                // HEADER
                var head = ds.Tables[0].AsEnumerable().Select(r => new
                {
                    SalaryConfigureID = Convert.ToInt32(r["SalaryConfigureID"]),
                    SalaryConfigureName = r["SalaryConfigureName"].ToString(),
                    PayGroupID = Convert.ToInt32(r["PayGroupID"]),
                    SalaryConfigureType = r["SalaryConfigureType"].ToString(),
                    TenantID = r["TenantID"].ToString()
                }).FirstOrDefault();

                // DATA
                DataTable allData = ds.Tables[1];
                var allList = ConvertToList(allData);

                // Check column
                bool hasTypeColumn = allData.Columns.Contains("PayConfigType");

                if (!hasTypeColumn)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "PayConfigType column missing in result set"
                    });
                }

                // Extract distinct types
                var availableTypes = allList
                    .Select(x => x["PayConfigType"].ToString())
                    .Distinct()
                    .ToList();

                // Safe splitting
                var manual = availableTypes.Contains("Manual")
                                ? allList.Where(x => x["PayConfigType"].ToString() == "Manual").ToList()
                                : new List<Dictionary<string, object>>();

                var allowances = availableTypes.Contains("Addition")
                                ? allList.Where(x => x["PayConfigType"].ToString() == "Addition").ToList()
                                : new List<Dictionary<string, object>>();

                var deduction = availableTypes.Contains("Deduction")
                                ? allList.Where(x => x["PayConfigType"].ToString() == "Deduction").ToList()
                                : new List<Dictionary<string, object>>();

                var companyContribution = availableTypes.Contains("CompanyContribution")
                                ? allList.Where(x => x["PayConfigType"].ToString() == "CompanyContribution").ToList()
                                : new List<Dictionary<string, object>>();

                // Final return
                return Json(new
                {
                    Success = true,
                    Manual = manual,
                    Allowances = allowances,
                    Deduction = deduction,
                    CompanyContribution = companyContribution
                });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }



        private List<Dictionary<string, object>> ConvertToList(DataTable dt)
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


        #endregion

        #region Update Configure Salary Component
        public JsonResult UpdateConfigureSalaryComponent(List<ConfigureSalaryComponentModel> InvoiceDList)
        {
            var data = "";
            
            foreach (ConfigureSalaryComponentModel configureSalaryComponentModel in InvoiceDList)
            {
                string contents = JsonConvert.SerializeObject(configureSalaryComponentModel);
                HttpResponseMessage response = ObjAPI.CallAPI("api/ConfigureSalaryComponent/UpdateConfigureSalarycomponent", contents);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(responseString))
                    {
                        data = JsonConvert.DeserializeObject(responseString).ToString();
                    }
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Update  Salary Configure
        public JsonResult UpdateSalaryConfigureModel(string SalaryConfigureID, string SalaryConfigureName,string PayGroupID,string SalaryType)
        {
            var data = "";
            SalaryConfigureModel salaryConfigureModel = new SalaryConfigureModel();
            salaryConfigureModel.SalaryConfigureID = Convert.ToInt32(SalaryConfigureID);
            salaryConfigureModel.SalaryConfigureName = SalaryConfigureName;
            salaryConfigureModel.PayGroupID = Convert.ToInt32(PayGroupID);
            salaryConfigureModel.SalaryConfigureType = Convert.ToInt32(SalaryType);
            string contents = JsonConvert.SerializeObject(salaryConfigureModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryConfigure/SaveSalaryConfigure", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region bind PayGroup Name
        public JsonResult GetPayGroup()
        {
            List<SelectListItem> partyList = new List<SelectListItem>();

            // partyList.Add(new SelectListItem { Text = "Select PartyName", Value = "0" });

            List<PayGroupModel> data = new List<PayGroupModel>();
            PayGroupModel payGroupModel = new PayGroupModel();
            List<SelectListItem> HRList = new List<SelectListItem>();
            HRList.Add(new SelectListItem { Text = "Select PayGroup Name", Value = "0" });
            payGroupModel.PayGroupID = Convert.ToInt32(0);
            payGroupModel.TenantID = Session["TenantID"].ToString();
            string contentss = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage responses = ObjAPI.CallAPI("api/PayGroup/GetAllPayGroupList", contentss);
            if (responses.IsSuccessStatusCode)
            {
                string responseString = responses.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayGroupModel>>(responseString); ;
                }
            }
            foreach (var items in data)
            {
                HRList.Add(new SelectListItem
                {
                    Text = items.PayGroupName,
                    Value = items.PayGroupID.ToString()
                });
            }
            return Json(HRList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region bind payconfig

        public JsonResult GetAllPayConfig()
        {
            //string message = "Deduction";
            List<PayConfigModel> data = new List<PayConfigModel>();
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigId = 0;
            payConfigModel.TenantID = TenantID;
            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/GetAllPayconfig", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayConfigModel>>(responseString);
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllPayConfigNew()
        {
             
            DataTable dt = clsDatabase.fnDataTable("PayConfig_GetAll_New", TenantID);
            List<PayConfigModel> data = new List<PayConfigModel>();
            foreach (DataRow DR in dt.Rows )
            {
                PayConfigModel obj = new PayConfigModel();
                obj.PayConfigId = clsHelper.fnConvert2Int(DR["PayConfigId"]);
                obj.PayConfigName = DR["PayConfigName"].ToString();
                obj.PayConfigType = DR["PayConfigType"].ToString();
                obj.IScalculative = DR["ISCalculative"].ToString() == "0" ? true : false;
                obj.EntryType = DR["EntryType"].ToString();
                data.Add(obj);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetManualConfigureSalaryNew(string PayGroupID)
        {

            DataTable dt = clsDatabase.fnDataTable("ManualSalaryConfig_GetByPayGroupID_New", PayGroupID,TenantID);
            List<ManualSalaryConfigModel> data = new List<ManualSalaryConfigModel>();
            foreach (DataRow dr in dt.Rows  )
            {
                ManualSalaryConfigModel obj = new ManualSalaryConfigModel();
                obj.ID = clsHelper.fnConvert2Int(dr["ID"]);
                obj.ISActive = dr["ISActive"].ToString() == "0" ? false : true;
                obj.PayGroupID=clsHelper.fnConvert2Int( dr["PayGroupId"]); 
                obj.ManualPayConfigId = clsHelper.fnConvert2Int(dr["ManualPayConfigId"]);
                obj.PayConfigName = dr["PayConfigName"].ToString();
                data.Add(obj);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Save Manual  Salary Configure
        [HttpPost]
        public JsonResult SaveManualSalaryConfigure(List<ManualSalaryConfigModel> ManualSalarylist)
        {
            var data = "";
            foreach (ManualSalaryConfigModel manualSalaryConfigModel in ManualSalarylist)
            {
                string contents = JsonConvert.SerializeObject(manualSalaryConfigModel);
                HttpResponseMessage response = ObjAPI.CallAPI("api/ConfigureSalaryComponent/SaveManualSalaryConfigure", contents);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        data = JsonConvert.DeserializeObject(responseString).ToString();
                    }
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Manual Salary Component By id
        public JsonResult GetManualConfigureSalary(string PayGroupID)
        {
            List<ManualSalaryConfigModel> data = new List<ManualSalaryConfigModel>();
            ManualSalaryConfigModel manualSalaryConfigModel = new ManualSalaryConfigModel();
            manualSalaryConfigModel.PayGroupID = Convert.ToInt32(PayGroupID);
            string contents = JsonConvert.SerializeObject(manualSalaryConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/ConfigureSalaryComponent/GetManualConfigureSalary", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<ManualSalaryConfigModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        
        #endregion



        #region DuplicatePaygroupCheck
        public JsonResult DuplicatePaygroupCheck(string PayGroupID)
        {
            List<SalaryConfigureModel> data = new List<SalaryConfigureModel>();
            SalaryConfigureModel salaryConfigureModel = new SalaryConfigureModel();
            salaryConfigureModel.PayGroupID = Convert.ToInt32(PayGroupID);
            salaryConfigureModel.TenantID = TenantID;
            string contents = JsonConvert.SerializeObject(salaryConfigureModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryConfigure/DuplicatePaygroupCheck", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<SalaryConfigureModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Delete Pay Config
        public JsonResult DeleteConfigureSalaryComponent(string SalaryConfigureID)
        {
            var data = "";
            SalaryConfigureModel salaryConfigureModel = new SalaryConfigureModel();
            salaryConfigureModel.SalaryConfigureID = Convert.ToInt32(SalaryConfigureID);
            string contents = JsonConvert.SerializeObject(salaryConfigureModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryConfigure/DeleteSalaryConfigure", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}