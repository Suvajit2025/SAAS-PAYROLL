using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using MendinePayroll.UI.Utility;
using Newtonsoft.Json;
using MendinePayroll.Models;
using UI.BLL;
using MendinePayroll.UI.Models;
namespace MendinePayroll.UI.Controllers
{
    public class PayConfigController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();
        // GET: PayConfig
        public ActionResult Index()
        {
           
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "PAYCONFIG-ENTRY";
            clsAccessLog.AccessLog_Save(info);


            PayConfigModel payConfigModel = new PayConfigModel();
           
            payConfigModel.PayConfigId = 0;
            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/GetAllPayconfig", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    payConfigModel.PayConfigList = JsonConvert.DeserializeObject<List<PayConfigModel>>(responseString);
                }
            }
            return View(payConfigModel);
        }
        #region Save PayGroup
        public JsonResult SavePayConfig(string PayConfigName, string PayConfigType,string IScalculative)
        {
            var data = "";
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigId = 0;
            payConfigModel.PayConfigName = PayConfigName;
            payConfigModel.PayConfigType = PayConfigType;
            payConfigModel.IScalculative =Convert.ToBoolean(IScalculative);


            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/SavePayconfig", contents);
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

        #region Update PayGroup
        public JsonResult UpdatePayConfig(string PayConfigId,  string PayConfigName, string PayConfigType, string IScalculative)
        {
            var data = "";
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigId = Convert.ToInt32(PayConfigId);
            payConfigModel.PayConfigName = PayConfigName;
            payConfigModel.PayConfigType = PayConfigType;
            payConfigModel.IScalculative = Convert.ToBoolean(IScalculative);


            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/SavePayconfig", contents);
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

        #region Get PayConfig
        public JsonResult GetPayConfig(string PayConfigID)
        {
            List<PayConfigModel> data = new List<PayConfigModel>();
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigId = Convert.ToInt32(PayConfigID);
            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/GetAllPayconfigbyid", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayConfigModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete Pay Config
        public JsonResult DeletePayConfig(string PayConfigID)
        {
            var data = "";
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigId = Convert.ToInt32(PayConfigID);
            string contents = JsonConvert.SerializeObject(payConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/DeletePayConfig", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString() ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}