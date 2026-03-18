using Common.Utility;
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
            string action = routeData["action"].ToString().ToLower();

            // ✅ 1. ALLOW: Skip check for the Login page so it can set the Session
            if (controller == "login")
            {
                return;
            }

            // ✅ 2. SESSION CHECK: Check if the user is logged in
            var session = filterContext.HttpContext.Session;
            string tenantId = session["TenantID"]?.ToString();
            string empId = session["EmployeeNo"]?.ToString();

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(empId))
            {
                filterContext.Result = new RedirectResult("https://iehrms.iecsl.in/Account/ChoosePortal.aspx");
                return;
            }

            // ✅ 3. AUTHORIZATION: Check Payroll access via SP
            // Using your existing Database utility
            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_VALIDATE_PayrollEmployeeAccess", tenantId, empId);

            if (dt == null || dt.Rows.Count == 0)
            {
                // Return the professional Card-style unauthorized View
                // Note: Ensure the view is in Views/Shared
                filterContext.Result = new ViewResult
                {
                    ViewName = "_UnauthorizedPayroll"
                };
            }

            base.OnActionExecuting(filterContext);
        }
    }
}