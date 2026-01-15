using System.Web;
using System.Web.Mvc;

namespace MendinePayroll.UI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new SessionAuthorizeAttribute()); // ✅ GLOBAL
        }
    }
}
