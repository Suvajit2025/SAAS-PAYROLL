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
    public class SalaryComponentTypeController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();
        // GET: SalaryComponentType
        public ActionResult Index()
        {
            SalaryComponentTypeModel salaryComponentTypeModel = new SalaryComponentTypeModel();
            salaryComponentTypeModel.Id = 0;
            string contents = JsonConvert.SerializeObject(salaryComponentTypeModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryComponent/GetAllSalaryComponentType", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    salaryComponentTypeModel.SalaryComponentTypeList = JsonConvert.DeserializeObject<List<SalaryComponentTypeModel>>(responseString);
                }
            }
            return View(salaryComponentTypeModel);
        }
                
        #region Save SalaryComponentType
        public JsonResult SaveSalaryComponentType(string SalaryComponentType)
        {
            var data = "";
            SalaryComponentTypeModel salaryComponentTypeModel = new SalaryComponentTypeModel();
            salaryComponentTypeModel.Id = 0;
            salaryComponentTypeModel.SalaryComponentType = SalaryComponentType
;
            


            string contents = JsonConvert.SerializeObject(salaryComponentTypeModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SaveSalaryComponentType", contents);
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

        #region Get SalaryComponentType
        public JsonResult GetSalaryComponentType(string SalaryComponentTypeID)
        {
            List<SalaryComponentTypeModel> data = new List<SalaryComponentTypeModel>();
            SalaryComponentTypeModel salaryComponentTypeModel = new SalaryComponentTypeModel();
            salaryComponentTypeModel.Id = Convert.ToInt32(SalaryComponentTypeID);
            string contents = JsonConvert.SerializeObject(salaryComponentTypeModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/SalaryComponent/SalaryComponentTypeListById", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<SalaryComponentTypeModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
                
        #region Update SalaryComponentType
        public JsonResult UpdateSalaryComponentType(string SalaryComponentTypeID, string SalaryComponentType)
        {
            var data = "";
            SalaryComponentTypeModel salaryComponentTypeModel = new SalaryComponentTypeModel();
            salaryComponentTypeModel.Id = Convert.ToInt32(SalaryComponentTypeID);
            salaryComponentTypeModel.SalaryComponentType = SalaryComponentType;



            string contents = JsonConvert.SerializeObject(salaryComponentTypeModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SaveSalaryComponentType", contents);
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

        #region Delete SalaryComponentType
        public JsonResult DeleteSalaryComponentType(string SalaryComponentTypeID)
        {
            var data = "";
            SalaryComponentTypeModel salaryComponentTypeModel = new SalaryComponentTypeModel();
            salaryComponentTypeModel.Id = Convert.ToInt32(SalaryComponentTypeID);
            string contents = JsonConvert.SerializeObject(salaryComponentTypeModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/DeteteSalaryComponentType", contents);
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