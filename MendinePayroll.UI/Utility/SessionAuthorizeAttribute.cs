using Common.Utility;
using System;
using System.Data;
using System.Web;
using System.Web.Mvc;

namespace MendinePayroll.UI.Utility
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var routeData = filterContext.RouteData.Values;
            string controller = routeData["controller"].ToString().ToLower();

            if (controller == "login")
                return;

            var session = filterContext.HttpContext.Session;
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;


            string tenantId = session["TenantID"]?.ToString();
            string empId = session["EmployeeNo"]?.ToString();

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(empId))
            {
                var parts = PayrollSessionCookie.Read(request);
                if (parts != null && parts.Length == 6)
                {
                    // Restore all session keys
                    session["RoleID"] = parts[0];
                    session["UserName"] = parts[1];
                    session["UserId"] = parts[2];
                    session["EmployeeNo"] = parts[3];
                    session["UserEmail"] = parts[4];
                    session["TenantID"] = parts[5];
                    session["ModuleURL"] =
                        System.Configuration.ConfigurationManager
                            .AppSettings["ModuleURL"]?.ToString();

                    // Refresh the cookie expiry (sliding expiry)
                    PayrollSessionCookie.Write(response,
                        parts[0], parts[1], parts[2],
                        parts[3], parts[4], parts[5]);

                    tenantId = parts[5];
                    empId = parts[3];
                }
                else
                {
                    // Cookie also missing/expired — send back to portal
                    filterContext.Result = new RedirectResult(
                        "https://iehrms.iecsl.in/Account/ChoosePortal.aspx");
                    return;
                }
            }

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_VALIDATE_PayrollEmployeeAccess", tenantId, empId);

            if (dt == null || dt.Rows.Count == 0)
            {
                filterContext.Result = new ViewResult { ViewName = "_UnauthorizedPayroll" };
                return; // ✅ also return here so base isn't called on unauthorized
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
