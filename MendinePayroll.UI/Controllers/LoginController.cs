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
        private const string DefaultPortalUrl = "https://iehrms.iecsl.in/Account/ChoosePortal.aspx";
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

            if (!string.IsNullOrWhiteSpace(Request.QueryString["TenantID"]))
            {
                SetPayrollSession(
                    Request.QueryString["RoleID"],
                    Request.QueryString["UserName"],
                    Request.QueryString["UserID"],
                    Request.QueryString["EmployeeNo"],
                    Request.QueryString["UserEmail"],
                    Request.QueryString["TenantID"]);
            }
            else
            {
                //return Redirect("https://iehrms.iecsl.in/Account/ChoosePortal.aspx");
                //Development
                //SetPayrollSession(
                //    "10195",
                //    "suvajit.das@iecsl.co.in",
                //    "476",
                //    "101450",
                //    "suvajit.das@iecsl.co.in",
                //    "6B1B6590-C5CA-4FD6-A0BB-FEBA6DB8FB14");

                //SetPayrollSession(
                //   "10198",
                //   "ankur.banerjee@mendine.com",
                //   "593",
                //   "100606",
                //   "ankur.banerjee@mendine.com",
                //   "6B1B6590-C5CA-4FD6-A0BB-FEBA6DB8FB14");

                ////Development
                SetPayrollSession(
                    "10345",
                    "iniciolab@iecsl.co.in",
                    "3542",
                    "13500001",
                    "iniciolab@iecsl.co.in",
                    "808E0AC6-94CE-4887-AD9D-85AF318ED335");

                //SetPayrollSession(
                //    "10198",
                //    "saikat.manna@mendine.com",
                //    "968",
                //    "101007",
                //    "saikat.manna@mendine.com",
                //    "6B1B6590-C5CA-4FD6-A0BB-FEBA6DB8FB14");

                //SetPayrollSession(
                //    "10152",
                //    "demo@corporation.in",
                //    "348",
                //    "37",
                //    "demo@corporation.in",
                //    "24A72122-DD46-4386-873D-758E450E252A");
            }



            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "PAYROLL-LOGIN";
            clsAccessLog.AccessLog_Save(info);
            return RedirectToAction("Index", "Dashboard");
        }

        public ActionResult GenerateMachineKeys()
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] validationKey = new byte[64];
                byte[] decryptionKey = new byte[24];
                rng.GetBytes(validationKey);
                rng.GetBytes(decryptionKey);

                string vKey = BitConverter.ToString(validationKey).Replace("-", "");
                string dKey = BitConverter.ToString(decryptionKey).Replace("-", "");

                string result = $@"<machineKey 
                    validationKey=""{vKey}"" 
                    decryptionKey=""{dKey}"" 
                    validation=""SHA1"" 
                    decryption=""AES"" />";

                return Content(result, "text/plain");
            }
        }

        public ActionResult RestoreSession(string returnUrl)
        {
            var payslipReturnUrl = GetExternalPayslipReturnUrl(returnUrl);
            if (!string.IsNullOrWhiteSpace(payslipReturnUrl))
                return Redirect(payslipReturnUrl);

            return Redirect(GetPortalUrl());
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
            PayrollSessionCookie.Clear(Response); // ✅ clear cookie
            HttpContext.Session.RemoveAll();
            FormsAuthentication.SignOut();
            Session.Abandon();

            // return RedirectToAction("Index", "Login");
            return Redirect(GetPortalUrl());
        }

        private void SetPayrollSession(string roleId, string userName, string userId, string employeeNo, string userEmail, string tenantId)
        {
            Session["RoleID"] = roleId;
            Session["UserName"] = userName;
            Session["UserId"] = userId;
            Session["EmployeeNo"] = employeeNo;
            Session["UserEmail"] = userEmail;
            Session["TenantID"] = tenantId;
            Session["ModuleURL"] = ConfigurationManager.AppSettings["ModuleURL"].ToString();

            // ✅ Persist to cookie so session can be restored after expiry
            PayrollSessionCookie.Write(Response, roleId, userName, userId,
                employeeNo, userEmail, tenantId);
        }

        private string GetPortalUrl()
        {
            string logoutUrl = ConfigurationManager.AppSettings["PayrollLogout"];
            return string.IsNullOrWhiteSpace(logoutUrl) ? DefaultPortalUrl : logoutUrl;
        }

        private string GetExternalPayslipReturnUrl(string returnUrl)
        {
            for (int i = 0; i < 25; i++)
            {
                if (string.IsNullOrWhiteSpace(returnUrl))
                    return null;

                returnUrl = HttpUtility.UrlDecode(returnUrl);

                Uri uri;
                if (!Uri.TryCreate(returnUrl, UriKind.Relative, out uri))
                    return null;

                var path = uri.GetLeftPart(UriPartial.Path).TrimEnd('/').ToLowerInvariant();
                var query = HttpUtility.ParseQueryString(uri.Query);

                if (path == "/login/restoresession")
                {
                    returnUrl = query["returnUrl"];
                    continue;
                }

                if ((path == "/payslip" || path == "/payslip/index" || path == "/payslip/downloadpdf") &&
                        !string.IsNullOrWhiteSpace(query["tenantId"]) &&
                        !string.IsNullOrWhiteSpace(query["empId"]) &&
                        !string.IsNullOrWhiteSpace(query["month"]) &&
                        !string.IsNullOrWhiteSpace(query["year"]))
                {
                    return returnUrl;
                }

                return null;
            }

            return null;
        }
    }

}
