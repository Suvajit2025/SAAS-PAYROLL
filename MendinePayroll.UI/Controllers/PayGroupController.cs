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
    public class PayGroupController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();
        // GET: PayGroup
        public ActionResult Index()
        {
            if (Session["UserName"] == null && Session["TenantID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "PAYGROUP-ENTRY";
            clsAccessLog.AccessLog_Save(info);


            #region bind HR
            List<EmpbasicModel> data = new List<EmpbasicModel>();
            EmpbasicModel empbasicModel = new EmpbasicModel();
            List<SelectListItem> HRList = new List<SelectListItem>();
            HRList.Add(new SelectListItem { Text = "Select HR Name", Value = "0" });
            //empbasicModel.empdesignation = Convert.ToInt32(9);
            empbasicModel.TenantID = Session["TenantID"].ToString();
            string contentss = JsonConvert.SerializeObject(empbasicModel);
            HttpResponseMessage responses = ObjAPI.CallAPI("api/PayGroup/GetAllEmployeeByDesignation", contentss);
            if (responses.IsSuccessStatusCode)
            {
                string responseString = responses.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmpbasicModel>>(responseString); ;
                }
            }
            foreach (var items in data)
            {
                HRList.Add(new SelectListItem
                {
                    Text = items.empfirstname + " " + items.empmiddlename + " "+items.emplastname,
                    Value = items.empid.ToString()
                });
            }
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.masterModel.selectListItems = HRList;
            #endregion

            //tenderModel = new TenderModel() { TenderID = 0 };
            //PayGroupModel payGroupModel   = new PayGroupModel();
            payGroupModel.PayGroupID = 0;
            List<PayGroupModel> PaygroupList = new List<PayGroupModel>();
            //tenderModel.TenderID = 0;
            payGroupModel.TenantID = Session["TenantID"].ToString();
            //tenderModel.ProsumersID = Convert.ToInt32(Session["ProsumersID"]);

            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGroup/GetAllPayGroupList", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    payGroupModel.PayGroupList = JsonConvert.DeserializeObject<List<PayGroupModel>>(responseString);
                }
            }
            
            //tenderModel.TenderList = tenderList;
            return View(payGroupModel);
        }
        #region Save PayGroup
        public JsonResult SavePayGroup(string PayGroupName,string Description,string PayGroupMasterCode,string ConcernHRPersonnel)
        {
            var data="";
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.PayGroupID = 0;
            payGroupModel.PayGroupName = PayGroupName;
            payGroupModel.Description = Description;
            payGroupModel.PayGroupMasterCode = PayGroupMasterCode;
            payGroupModel.ConcernHRPersonnel =Convert.ToInt32(ConcernHRPersonnel);
            payGroupModel.TenantID = Session["TenantID"].ToString();
            payGroupModel.EntryUser = Session["UserEmail"].ToString();

            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SavePayGroup", contents);
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

        #region Get PayGroup
        public JsonResult GetPayGroup(string PayGroupID)
        {
            List<PayGroupModel> data = new List<PayGroupModel>();
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.PayGroupID =Convert.ToInt32(PayGroupID);
            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGroup/PayGroupListById", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayGroupModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Update PayGroup
        public JsonResult UpdatePayGroup(string PayGroupId, string PayGroupName, string Description, string PayGroupMasterCode, string ConcernHRPersonnel)
        {
            var data = "";
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.PayGroupID = Convert.ToInt32(PayGroupId);
            payGroupModel.PayGroupName = PayGroupName;
            payGroupModel.Description = Description;
            payGroupModel.PayGroupMasterCode = PayGroupMasterCode;
            payGroupModel.ConcernHRPersonnel = Convert.ToInt32(ConcernHRPersonnel);
            payGroupModel.TenantID = Session["TenantID"].ToString();
            payGroupModel.EntryUser = Session["UserEmail"].ToString();

            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SavePayGroup", contents);
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

        #region SavePayGroupBonus 
        public JsonResult SavePayGroupBonus(string ID, string PayGroupId, string Month, string Year, string OverTime, string ProductionBonus,string TargetAchieved)
        {
            var data = "";
            PayGroupBonusModel payGroupBonusModel = new PayGroupBonusModel();
            payGroupBonusModel.ID = Convert.ToInt32(ID);
            payGroupBonusModel.PayGroupId = Convert.ToInt32(PayGroupId);
            payGroupBonusModel.Month = Month;
            payGroupBonusModel.Year = Convert.ToInt32(Year);
            payGroupBonusModel.OverTimeHours = Convert.ToDouble(OverTime);
            payGroupBonusModel.ProductionBonus = Convert.ToDouble(ProductionBonus);
            payGroupBonusModel.TargetAchieved = Convert.ToDouble(TargetAchieved);

            string contents = JsonConvert.SerializeObject(payGroupBonusModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SavePayGroupBonus", contents);
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

        #region Delete Pay Group
        public JsonResult DeletePayGroup(string PayGroupID)
        {
            var data = "";
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.PayGroupID = Convert.ToInt32(PayGroupID);
            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/DetetePayGroup", contents);
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


        #region paygroup bonus
        public ActionResult PayGroupBonusDetails(string cal)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            PayGroupBonusModel payGroupBonusModel = new PayGroupBonusModel();
            List<PayGroupBonusModel> PaygroupList = new List<PayGroupBonusModel>();
            payGroupBonusModel.PayGroupId = Convert.ToInt32(cal);
            string contents = JsonConvert.SerializeObject(payGroupBonusModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGroup/GetAllPayGroupBonusById", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    payGroupBonusModel.PayGroupBonusList = JsonConvert.DeserializeObject<List<PayGroupBonusModel>>(responseString);
                }
            }

            return View(payGroupBonusModel);
            
        }

        #endregion

        #region Get PayGroupBonusByID
        public JsonResult GetPayGroupBonusById(string ID)
        {
            List<PayGroupBonusModel> data = new List<PayGroupBonusModel>();
            PayGroupBonusModel payGroupBonusModel = new PayGroupBonusModel();
            payGroupBonusModel.ID = Convert.ToInt32(ID);
            string contents = JsonConvert.SerializeObject(payGroupBonusModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGroup/GetPayGroupBonusById", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayGroupBonusModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion


    }
}