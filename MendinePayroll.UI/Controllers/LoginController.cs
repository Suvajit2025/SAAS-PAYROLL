using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using MendinePayroll.UI.Utility;
using Newtonsoft.Json;
using MendinePayroll.Models;
using System.Data;
using System.Reflection;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using System.Configuration;
using UI.BLL;
using MendinePayroll.UI.Models;

namespace MendinePayroll.UI.Controllers
{
    public class LoginController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();
        // OLD  Login 
        // GET: Login
        //public ActionResult Index()
        //{
        //    return View();
        //}
        //[HttpPost]
        // Deplyment
        //public ActionResult Index(String UserName, String UserID, String UserEmail)
        //{
        //    if (UserName != "" && UserID != "" && UserEmail != "")
        //    {
        //        Session["UserName"] = Request.QueryString["UserName"];
        //        Session["UserId"] = Request.QueryString["UserID"];
        //        Session["UserEmail"] = Request.QueryString["UserEmail"];
        //        Session["ModuleURL"] = ConfigurationManager.AppSettings["ModuleURL"].ToString();
        //        return RedirectToAction("Index", "Dashboard");
        //    }
        //    return View();
        //}

        // Development
        public ActionResult Index()
        {
            //Session["UserName"] = "suvajit.das@iecsl.co.in";
            //Session["UserId"] = "3299";
            //Session["UserEmail"] = "suvajit.das@iecsl.co.in";

            // Development 
            Session["UserName"] = "lit@mindtree.co.in";
            Session["UserId"] = "229";
            Session["UserEmail"] = "lit@mindtree.co.in";
            Session["TenantID"] = "9CBC5D8B-589C-4144-8AE9-1937A39FC89D";
            //Session["TenantID"] = "DB9DE705-8097-41C6-A8F9-8175DD6DF064";

            //Session["CompanyId"] = "55";
            //Session["UserName"] = "subrata.mukherjee@mendine.co.in";
            //Session["UserId"] = "1114";
            //Session["UserEmail"] = "subrata.mukherjee@mendine.co.in";

            //Session["UserName"] = "arunima.saha@mendine.com";
            //Session["UserId"] = "1051";
            //Session["UserEmail"] = "arunima.saha@mendine.com";

            //Session["UserName"] = "ankur.banerjee@mendine.com";
            //Session["UserId"] = "946";
            //Session["UserEmail"] = "ankur.banerjee@mendine.com";

            //Session["UserName"] = "saikat.manna@mendine.com";
            //Session["UserId"] = "1744";
            //Session["UserEmail"] = "saikat.manna@mendine.com";

            //Session["UserName"] = "atreyee.das@mendine.com";
            //Session["UserId"] = "3114";
            //Session["UserEmail"] = "atreyee.das@mendine.com";

            // Development
            //Session["UserName"] = "rimpa.das@iecsl.co.in";
            //Session["UserId"] = "2993";
            //Session["UserEmail"] = "rimpa.das@iecsl.co.in";

            //Session["UserName"] = "md.mendinepharma@gmail.com";
            //Session["UserId"] = "1260";
            //Session["UserEmail"] = "md.mendinepharma@gmail.com";

            //Production
            //Session["UserName"] = Request.QueryString["UserName"];
            //Session["UserId"] = Request.QueryString["UserID"];
            //Session["UserEmail"] = Request.QueryString["UserEmail"];
            //Session["TenantID"] = Request.QueryString["TenantID"];

            Session["ModuleURL"] = ConfigurationManager.AppSettings["ModuleURL"].ToString();
            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "PAYROLL-LOGIN";
            clsAccessLog.AccessLog_Save(info);
            return RedirectToAction("Index", "Dashboard");
        }
        public JsonResult EmployeeLogin(string Email,string Password)
        {
            LoginWebService.logincontrol obj = new LoginWebService.logincontrol();
            var userlist = obj.Userlogin(Email, Password);
            int i = 0;
            List<LoginModel> LoginModel = new List<LoginModel>();
            LoginModel = JsonConvert.DeserializeObject<List<LoginModel>>(userlist);
            if (LoginModel.Count > 0)
            {
                if (LoginModel[0].Truefalse == "False" && LoginModel[0].Empname == null)
                {
                    i = 0;
                }
                else
                {
                    i = 1;
                    Session["UserEmail"] = LoginModel[0].empemail;
                    Session["UserName"] = LoginModel[0].Empname;
                    Session["Empno"] = LoginModel[0].empno;
                }
            }
            else
            {
                i = 0;
            }

            //int i = 1;

            //Session["UserName"] = "Chinmoy";
            //Session["UserId"] = 1;
            //Session["UserEmail"] = "Chinmoy";




            return Json(i, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LogOut()
        {
            HttpContext.Session.RemoveAll();
            FormsAuthentication.SignOut();
            Session.Abandon();

            // return RedirectToAction("Index", "Login");
            String logoutUrl=ConfigurationManager.AppSettings["PayrollLogout"].ToString();
            return Redirect(logoutUrl);
        }
    }
}