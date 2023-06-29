using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using MendinePayroll.UI.Utility;
using Newtonsoft.Json;
using MendinePayroll.Models;


namespace MendinePayroll.UI.Controllers
{
    public class SalaryComponentController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();
        // GET: SalaryComponent
        public ActionResult Index()
        {

            #region bind Paygroup
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
            SalaryComponentModel salaryComponentModel = new SalaryComponentModel();
            salaryComponentModel.mastermodel.selectListItems = HRList;
            #endregion

            #region bind Paygroup
            List<SalaryComponentTypeModel> datalist = new List<SalaryComponentTypeModel>();
            SalaryComponentTypeModel salaryComponentTypeModel = new SalaryComponentTypeModel();
            List<SelectListItem> List = new List<SelectListItem>();
            List.Add(new SelectListItem { Text = "Select Salary component Type", Value = "0" });
            salaryComponentTypeModel.Id = Convert.ToInt32(0);
            string content = JsonConvert.SerializeObject(salaryComponentTypeModel);
            HttpResponseMessage responsees = ObjAPI.CallAPI("api/SalaryComponent/GetAllSalaryComponentType", content);
            if (responsees.IsSuccessStatusCode)
            {
                string responseString = responsees.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    datalist = JsonConvert.DeserializeObject<List<SalaryComponentTypeModel>>(responseString); ;
                }
            }
            foreach (var items in datalist)
            {
                List.Add(new SelectListItem
                {
                    Text = items.SalaryComponentType,
                    Value = items.Id.ToString()
                });
            }
           
            salaryComponentModel.MasterModel.selectListItems = List;
            #endregion



            //SalaryComponentModel salaryComponentModel = new SalaryComponentModel();
            salaryComponentModel.SalaryComponentId = 0;

            string contents = JsonConvert.SerializeObject(salaryComponentModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryComponent/GetAllSalaryComponent", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    salaryComponentModel.SalaryComponentList = JsonConvert.DeserializeObject<List<SalaryComponentModel>>(responseString);
                }
            }
            return View(salaryComponentModel);
        }
        #region Get Salary Component By Id
        public JsonResult GetSalaryComponent(string SalaryComponentId)
        {
            List<SalaryComponentModel> data = new List<SalaryComponentModel>();
            SalaryComponentModel salaryComponentModel = new SalaryComponentModel();
            salaryComponentModel.SalaryComponentId = Convert.ToInt32(SalaryComponentId);
            string contents = JsonConvert.SerializeObject(salaryComponentModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryComponent/SalaryComponentListById", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<SalaryComponentModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Delete Salary Component
        public JsonResult DeleteSalaryComponent(string SalaryComponentId)
        {
            var data = "";
            SalaryComponentModel salaryComponentModel = new SalaryComponentModel();
            salaryComponentModel.SalaryComponentId = Convert.ToInt32(SalaryComponentId);
            string contents = JsonConvert.SerializeObject(salaryComponentModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/DeteteSalaryComponent", contents);
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
        #region Save Salary Component
        public JsonResult SaveSalaryComponent(string SalaryComponent,string SalaryComponentCode,string SalaryComponentTypeID,string PayGroupsID,string Description)
        {
            var data = "";
            SalaryComponentModel salaryComponentModel = new SalaryComponentModel();
            salaryComponentModel.SalaryComponentId = 0;
            salaryComponentModel.SalaryComponent = SalaryComponent;
            salaryComponentModel.SalaryComponentCode =Convert.ToInt32(SalaryComponentCode);
            salaryComponentModel.SalaryComponentTypeID =Convert.ToInt32(SalaryComponentTypeID);
            salaryComponentModel.PayGroupsID =Convert.ToInt32( PayGroupsID);
            salaryComponentModel.Description = Description;
            string contents = JsonConvert.SerializeObject(salaryComponentModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SaveSalaryComponent", contents);
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
        #region Update Salary Component
        public JsonResult UpdateSalaryComponent(string SalaryComponentID, string SalaryComponent, string SalaryComponentCode, string SalaryComponentTypeID, string PayGroupsID, string Description)
        {
            var data = "";
            SalaryComponentModel salaryComponentModel = new SalaryComponentModel();
            salaryComponentModel.SalaryComponentId = Convert.ToInt32(SalaryComponentID);
            salaryComponentModel.SalaryComponent = SalaryComponent;
            salaryComponentModel.SalaryComponentCode = Convert.ToInt32(SalaryComponentCode);
            salaryComponentModel.SalaryComponentTypeID = Convert.ToInt32(SalaryComponentTypeID);
            salaryComponentModel.PayGroupsID = Convert.ToInt32(PayGroupsID);
            salaryComponentModel.Description = Description;
            string contents = JsonConvert.SerializeObject(salaryComponentModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SaveSalaryComponent", contents);
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