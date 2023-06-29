using Common.Utility;
using MendinePayroll.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.BLL
{
    public class clsMenu
    {
        public static List<clsMenuInfo> Employee_Wise_Menu_List(String Con, String EmployeeNo)
        {
            var empNo = Convert.ToInt64(EmployeeNo);
            List<clsMainMenuInfo> mainmenu = new List<clsMainMenuInfo>();
            List<clsSubMenuInfo> submenu = new List<clsSubMenuInfo>();
            List<clsMenuInfo> menus = new List<clsMenuInfo>();
            DataSet DS = clsDatabase.fnDataSet("PRC_Payroll_Menu", empNo);

            foreach (DataRow Dr in DS.Tables[0].Rows)
            {
                clsMainMenuInfo mobj = new clsMainMenuInfo();
                mobj.MenuSRL = clsHelper.fnConvert2Long(Dr["MenuSRL"]);
                mobj.MainMenu = Dr["MainMenu"].ToString();
                mainmenu.Add(mobj);
            }
            foreach (DataRow Dr in DS.Tables[1].Rows)
            {
                clsSubMenuInfo sobj = new clsSubMenuInfo();
                sobj.MenuSRL = clsHelper.fnConvert2Long(Dr["MenuSRL"]);
                sobj.MainMenu = Dr["MainMenu"].ToString();
                sobj.SubMenu = Dr["SubMenu"].ToString();
                sobj.MenuURL = Dr["MenuURL"].ToString();
                sobj.MenuIcon = Dr["MenuIcon"].ToString();
                submenu.Add(sobj);
            }
            menus.Add(new clsMenuInfo { MainMenu = mainmenu, SubMenu = submenu });
            return menus;
        }
    }
}