using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using UI.BLL;
using MendinePayroll.UI.Models;
using System.Data;

namespace MendinePayroll.UI.Controllers
{
    public class SalaryConfigurationController : Controller
    {
        // GET: SalaryConfiguration
        public ActionResult Index()
        {
            return View();
        }

        private static string DataTableToJSONWithJSONNet(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }
        // Configuration Detail
        [HttpGet]
        public JsonResult ConfigurationDetail(long IDConfiguration)
        {
            DataTable DT = clsSalaryConfiguration.Salary_Configuration_Detail(IDConfiguration);
            string employees = DataTableToJSONWithJSONNet(DT);
            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        // Configuration Save
        [HttpPost]
        public ActionResult ConfigurationSave(clsSalaryConfigurationInfo obj)
        {
            String Result = clsSalaryConfiguration.Salary_Configuration_Save(obj);
            if (Result == "")
                return Json(new { success = "" }, JsonRequestBehavior.AllowGet);
            else
            {
                return Json(new { error = "Error" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult SalaryAuthentication(String UserName, String Password)
        {
            Boolean valid = clsSalaryConfiguration.Salary_Authentication(UserName,Password);
            return Json(valid, JsonRequestBehavior.AllowGet);
        }

    }
}