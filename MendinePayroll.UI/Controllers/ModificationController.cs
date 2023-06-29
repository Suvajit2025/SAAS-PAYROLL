using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.Models;
using UI.BLL;

namespace MendinePayroll.UI.Controllers
{
    public class ModificationController : Controller
    {
        // GET: SalaryModification
        public ActionResult Salary()
        {
            return View();
        }
        public ActionResult Loan()
        {
            return View();
        }

        // MOnth List
        [HttpGet]
        public JsonResult SalaryMonthsList(string Type)
        {
            var Result = clsSalaryModification.Salary_Months_List();
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        // Year List
        [HttpGet]
        public JsonResult SalaryYearList(string Type)
        {
            var Result = clsSalaryModification.Salary_Years_List();
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SalaryDetailList(String Employeeno, String MonthName, String Year )
        {
            var Result = clsSalaryModification.Salary_Detail(Employeeno, MonthName, Year);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SalaryESIModification(clsSalaryModificationInfo info)
        {
            var Result = clsSalaryModification.Salary_ESI_Modificaiton(info);
            if (Result == "")
                return Json(new { success = "" }, JsonRequestBehavior.AllowGet);
            else
            {
                return Json(new { success = Result }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}