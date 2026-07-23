using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using QuestPDF.Infrastructure;

namespace MendinePayroll.UI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        //protected void Application_Start()
        //{
        //    AreaRegistration.RegisterAllAreas();
        //    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        //    RouteConfig.RegisterRoutes(RouteTable.Routes);
        //    BundleConfig.RegisterBundles(BundleTable.Bundles);
        //}
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            // ✅ ADD THIS LINE to enable your filters/authorization
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // ✅ QuestPDF license configuration — required once at startup
            QuestPDF.Settings.License = LicenseType.Community;

            // 🔑 JSON VALUE PROVIDER
            if (!ValueProviderFactories.Factories.Any(f => f is JsonValueProviderFactory))
            {
                ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
            }
        }
    }
}