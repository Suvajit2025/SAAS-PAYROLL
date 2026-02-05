using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MendinePayroll.UI.Controllers
{
    public class PayrollreportController : Controller
    {
        public ActionResult EpfEcrChallanReport()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }
}