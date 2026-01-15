using MendinePayroll.UI.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MendinePayroll.UI.Controllers
{

    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            Session["Connection"] = System.Configuration.ConfigurationManager.ConnectionStrings["Admin"].ToString();
            return View();
        }

        // Menu List
        public JsonResult MenuList()
        {
            var con = System.Configuration.ConfigurationManager.ConnectionStrings["Admin"].ToString();
            var d = clsMenu.Employee_Wise_Menu_List(con, Session["UserId"].ToString());
            return Json(d, JsonRequestBehavior.AllowGet);
        }
    }
}