using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using MendinePayroll.UI.Utility;
using Newtonsoft.Json;
using MendinePayroll.Models;
using Common.Utility;
using System.Data;
using UI.BLL;
using MendinePayroll.UI.Models;

namespace MendinePayroll.UI.Controllers
{
    public class ConfigureSalaryComponentController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();
        // GET: ConfigureSalaryComponent
        public ActionResult Index()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "CONFIGURE-SALARY-COMPONENT-ENTRY";
            clsAccessLog.AccessLog_Save(info);

            SalaryConfigureModel salaryConfigureModel = new SalaryConfigureModel();
            salaryConfigureModel.SalaryConfigureID = 0;

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

        #region Save Configure Salary Component
        public JsonResult SaveConfigureSalaryComponent(List<ConfigureSalaryComponentModel> InvoiceDList)
        {
            var data = "";
            foreach (ConfigureSalaryComponentModel configureSalaryComponentModel in InvoiceDList)
            {
                string contents = JsonConvert.SerializeObject(configureSalaryComponentModel);
                HttpResponseMessage response = ObjAPI.CallAPI("api/ConfigureSalaryComponent/SaveConfigureSalarycomponent", contents);
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


        #region bind Allowances
        public JsonResult GetPayConfig()
        {
            string message = "All";
            List<PayConfigModel> data = new List<PayConfigModel>();
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigType = message;
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

        #region Update Salary Configure
        public ActionResult UpdateSalaryConfigure()
        {
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
        public JsonResult GetConfigureSalaryComponentByID(string ConfigureSalaryID)
        {
            List<ConfigureSalaryComponentModel> data = new List<ConfigureSalaryComponentModel>();
            ConfigureSalaryComponentModel configureSalaryComponentModel = new ConfigureSalaryComponentModel();
            configureSalaryComponentModel.ConfigureSalaryID = Convert.ToInt32(ConfigureSalaryID);
            string contents = JsonConvert.SerializeObject(configureSalaryComponentModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/ConfigureSalaryComponent/GetAllConfigureSalaryComponentbyid", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<ConfigureSalaryComponentModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
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

            DataTable dt = clsDatabase.fnDataTable("PayConfig_GetAll_New");
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
            DataTable dt = clsDatabase.fnDataTable("ManualSalaryConfig_GetByPayGroupID_New", PayGroupID);
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