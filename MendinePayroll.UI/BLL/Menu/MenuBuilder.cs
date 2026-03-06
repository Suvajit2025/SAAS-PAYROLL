using System;
using System.Collections.Generic;
using System.Data;
using System.Linq; 

namespace MendinePayroll.UI.BLL.Menu
{
    public static class MenuBuilder
    {
        public static List<MenuVM> BuildMenuTree(DataSet ds)
        {
            if (ds == null || ds.Tables.Count < 2)
                return new List<MenuVM>();

            var mainMenus = ds.Tables[0].AsEnumerable()
                .Select(r => new MenuVM
                {
                    MainMenu = r["MainMenu"].ToString(),
                    MainMenuOrder = Convert.ToInt32(r["Main_Menu_Order"])
                })
                .OrderBy(x => x.MainMenuOrder)
                .ToList();

            var subMenus = ds.Tables[1].AsEnumerable()
                .Select(r => new SubMenuVM
                {
                    IDMenu = Convert.ToInt64(r["IDMenu"]),
                    MainMenu = r["MainMenu"].ToString(),
                    SubMenu = r["SubMenu"].ToString(),
                    URL = r["URL"]?.ToString(),
                    SaveYN = Convert.ToInt32(r["SaveYN"]) == 1,
                    EditYN = Convert.ToInt32(r["EditYN"]) == 1,
                    DeleteYN = Convert.ToInt32(r["DeleteYN"]) == 1,
                    ReportYN = Convert.ToInt32(r["ReportYN"]) == 1,
                    SubMenuOrder = Convert.ToInt32(r["Sub_Menu_Order"])
                })
                .ToList();

            foreach (var menu in mainMenus)
            {
                menu.Children = subMenus
                    .Where(s => s.MainMenu == menu.MainMenu)
                    .OrderBy(s => s.SubMenuOrder)
                    .ToList();
            }

            return mainMenus;
        }
    }
}