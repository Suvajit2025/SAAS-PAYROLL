using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.BLL;
using MendinePayroll.UI.Models;
namespace MendinePayroll.UI.Controllers
{
    public class BonusController : Controller
    {
        // GET: Bonus
        public ActionResult Index()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "BONUS-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }
        // Month List
        [HttpGet]
        public JsonResult Monthlist()
        {
            var Result = Enumerable.Range(1, 12).Select(x => new { month = DateTimeFormatInfo.CurrentInfo.GetMonthName(x) });
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        // Month List
        [HttpGet]
        public JsonResult CurrentYear()
        {
            var Result = DateTime.Now.Year;
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        // Employee List
        [HttpGet]
        public JsonResult EmployeeList()
        {
            DataTable DT = clsLoan.Payroll_Employee_Auto_Complete();
            string employees = DataTableToJSONWithJSONNet(DT);
            return Json(employees , JsonRequestBehavior.AllowGet);
        }
        private static string DataTableToJSONWithJSONNet(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }
        // Bonus Save
        [HttpPost]
        public ActionResult BonusSave(clsBonusInfo obj)
        {
            String Result = clsBonus.Bonus_Add_Edit (obj);
            if (Result == "")
                return Json(new { success = "" }, JsonRequestBehavior.AllowGet);
            else
            {
                return Json(new { error = "Error" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult BonusList()
        {
            DataTable DT = clsBonus.Bonus_List();
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult BonusDetail(long IDBonus)
        {
            DataTable DT = clsBonus.Bonus_Detail(IDBonus);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }

    }
}