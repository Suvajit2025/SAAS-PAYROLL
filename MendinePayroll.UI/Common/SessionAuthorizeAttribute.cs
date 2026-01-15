using Microsoft.Reporting.Map.WebForms.BingMaps;
using System.Web;
using System.Web.Mvc;

public class SessionAuthorizeAttribute : AuthorizeAttribute
{
    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        var session = httpContext.Session;

        if (session == null)
            return false;

        return session["UserId"] != null
            && session["UserName"] != null
            && session["UserEmail"] != null
            && session["TenantID"] != null;
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        filterContext.HttpContext.Session.Clear();
        filterContext.HttpContext.Session.Abandon();

        filterContext.Result = new RedirectResult("https://iehrms.iecsl.in/Account/ChoosePortal.aspx"); 
    }
}
