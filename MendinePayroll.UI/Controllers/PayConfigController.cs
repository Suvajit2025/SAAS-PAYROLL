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
using System.Data;
using Common.Utility;
namespace MendinePayroll.UI.Controllers
{
    public class PayConfigController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();
        // GET: PayConfig
        public ActionResult Index()
        {
           
            if (Session["UserName"] == null && Session["TenantID"]== null)
            {
                return RedirectToAction("Index", "Login");
            }



            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "PAYCONFIG-ENTRY";
            clsAccessLog.AccessLog_Save(info);


            PayConfigModel payConfigModel = new PayConfigModel();
           
            payConfigModel.PayConfigId = 0;
            payConfigModel.TenantID = Session["TenantID"].ToString();
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
        #region Save PayConfig
        public JsonResult SavePayConfig(string PayConfigName, string PayConfigType,string IScalculative, bool IsStatutory)
        {
            var data = "";
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigId = 0;
            payConfigModel.PayConfigName = PayConfigName;
            payConfigModel.PayConfigType = PayConfigType;
            payConfigModel.IScalculative =Convert.ToBoolean(IScalculative);
            payConfigModel.IsStatutory=IsStatutory;
            payConfigModel.TenantID = Session["TenantID"].ToString();
            payConfigModel.EntryUser = Session["UserEmail"].ToString();

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

        #region Update PayConfig
        public JsonResult UpdatePayConfig(string PayConfigId,  string PayConfigName, string PayConfigType, string IScalculative, bool IsStatutory)
        {
            var data = "";
            PayConfigModel payConfigModel = new PayConfigModel();
            payConfigModel.PayConfigId = Convert.ToInt32(PayConfigId);
            payConfigModel.PayConfigName = PayConfigName;
            payConfigModel.PayConfigType = PayConfigType;
            payConfigModel.IScalculative = Convert.ToBoolean(IScalculative);
            payConfigModel.IsStatutory = IsStatutory;
            payConfigModel.TenantID = Session["TenantID"].ToString();
            payConfigModel.EntryUser = Session["UserEmail"].ToString();

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
            DataTable dt = clsDatabase.fnDataTable("PayConfig_GetById", PayConfigID);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    data.Add(new PayConfigModel
                    {
                        PayConfigId = Convert.ToInt32(dr["PayConfigId"]),
                        PayConfigName = dr["PayConfigName"].ToString(),
                        PayConfigType = dr["PayConfigType"].ToString(),
                        IScalculative = dr["IScalculative"] != DBNull.Value && Convert.ToBoolean(dr["IScalculative"]),
                        IsStatutory = dr["IsStatutory"] != DBNull.Value && Convert.ToBoolean(dr["IsStatutory"])
                    });
                }
            }


            //payConfigModel.PayConfigId = Convert.ToInt32(PayConfigID); 
            //string contents = JsonConvert.SerializeObject(payConfigModel);
            //HttpResponseMessage response = ObjAPI.CallAPI("api/PayConfig/GetAllPayconfigbyid", contents);
            //if (response.IsSuccessStatusCode)
            //{
            //    string responseString = response.Content.ReadAsStringAsync().Result;

            //    if (!string.IsNullOrEmpty(responseString))
            //    {
            //        data = JsonConvert.DeserializeObject<List<PayConfigModel>>(responseString); ;
            //    }
            //}

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