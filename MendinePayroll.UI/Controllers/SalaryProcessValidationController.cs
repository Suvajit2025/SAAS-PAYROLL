using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.Models;
using UI.BLL;
using Common.Utility;

namespace MendinePayroll.UI.Controllers
{
    public class SalaryProcessValidationController : Controller
    {
        // GET: SalaryProcessValidation
        public ActionResult Index()
        {
            return View();
        }
        // Pay Group
        [HttpGet]
        public JsonResult PayGroupList()
        {
            long IDEmployee = clsHelper.fnConvert2Long(Session["UserId"]);
            var Result = clsSalaryProcessValidation.Pay_Group_List(IDEmployee);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        // Month List
        [HttpGet]
        public JsonResult PayrollMonthList()
        {
            var Result = clsSalaryProcessValidation.Payroll_Month();
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        // Year List
        [HttpGet]
        public JsonResult PayrollYearList()
        {
            var Result = clsSalaryProcessValidation.Payroll_Year();
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        // Year List
        [HttpGet]
        public JsonResult PayrollProcessList(long IDPayGroup, String Month, long Year)
        {
            var Result = clsSalaryProcessValidation.Payroll_Proces_List(IDPayGroup, Month, Year);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        //Validation
        [HttpGet]
        public JsonResult PayrollProcessValidation (long IDPayroll,long IDEmployee,String Month, String Year)
        {
            var Result = clsSalaryProcessValidation.Payroll_Proces_Validation(IDPayroll, IDEmployee, Month, Year);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
    }
}