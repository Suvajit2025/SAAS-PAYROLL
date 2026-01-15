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

    public class PayGroupEmployeeMappingController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();
        // GET: PayGroupEmployeeMapping
        public ActionResult Index()
        {
            PayGroupEmployeeMappingModel payGroupEmployeeMappingModel = new PayGroupEmployeeMappingModel();
            payGroupEmployeeMappingModel.PayGroupEmployeeID = 0;
            string contents = JsonConvert.SerializeObject(payGroupEmployeeMappingModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGroup/GetAllPayGroupEmployeeMapping", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    payGroupEmployeeMappingModel.PayGroupEmployeeMappingList = JsonConvert.DeserializeObject<List<PayGroupEmployeeMappingModel>>(responseString);
                }
            }
            return View();
        }
    }
}