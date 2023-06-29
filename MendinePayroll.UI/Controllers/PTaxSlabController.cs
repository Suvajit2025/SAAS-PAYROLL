using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MendinePayroll.UI.BLL;
using Common.Utility;

namespace MendinePayroll.UI.Controllers
{
    public class PTaxSlabController : Controller
    {
        // GET: PTaxSlab
        public ActionResult Index()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        [HttpGet]
        public JsonResult State_List()
        {
            var d = clsState.StateList();
            return Json(new { d }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PTax_Save(List<clsPTaxSlabInfo> Info)
        {
            var d = clsPTaxSlab.Ptax_Save(Info);
            return Json(new { d }, JsonRequestBehavior.AllowGet);
        }
      
        [HttpGet]
        public JsonResult PTax_Detail(long IDState, long Year)
        {
            var d = clsPTaxSlab.PTax_ListDetails(IDState, Year);
            return Json(new { d }, JsonRequestBehavior.AllowGet);
        }


    }
}