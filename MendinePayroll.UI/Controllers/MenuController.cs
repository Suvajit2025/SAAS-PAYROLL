using MendinePayroll.UI.BLL.Menu;
using MendinePayroll.UI.BLL.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

public class MenuController : Controller
{
    [ChildActionOnly] // 🔒 Prevent direct URL access
    public ActionResult PayrollMenu()
    {
        // Session validation
        if (Session["TenantID"] == null || Session["RoleID"] == null)
        {
            return PartialView("_PayrollMenu", null);
        }

        long roleId = Convert.ToInt64(Session["RoleID"]);
        string tenantId = Session["TenantID"].ToString();

        // Call your SP
        var ds = clsSecurity.RoleModuleMenu(roleId,"PAYROLL",2,tenantId);

        var menuList = MenuBuilder.BuildMenuTree(ds);

        return PartialView("_PayrollMenu", menuList);
    }
}

public class MenuVM
{
    public string MainMenu { get; set; }
    public int MainMenuOrder { get; set; }
    public List<SubMenuVM> Children { get; set; } = new List<SubMenuVM>();
}

public class SubMenuVM
{
    public long IDMenu { get; set; }
    public string MainMenu { get; set; }
    public string SubMenu { get; set; }
    public string URL { get; set; }
    public bool SaveYN { get; set; }
    public bool EditYN { get; set; }
    public bool DeleteYN { get; set; }
    public bool ReportYN { get; set; }
    public int SubMenuOrder { get; set; }
}