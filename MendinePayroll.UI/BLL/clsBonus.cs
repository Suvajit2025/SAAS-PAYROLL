using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MendinePayroll.UI.Models;
using System.Configuration;
namespace UI.BLL
{
    public class clsBonus
    {
		
		public static String Bonus_Add_Edit(clsBonusInfo Info)
		{
			string UserName = HttpContext.Current.Session["username"].ToString();
			return clsDatabase.fnDBOperation("PRC_Bonus_Add_Edit",
									Info.IDBonus, Info.IDEmployee, Info.BonusDate,
									Info.BonusMonth, Info.BonusYear,Info.Amount,UserName);
        }
		public static DataTable  Bonus_List()
		{
			return clsDatabase.fnDataTable("PRC_Bonus_List");
		}
		public static DataTable Bonus_Detail(long IDBonus)
		{
			return clsDatabase.fnDataTable("PRC_Bonus_Detail", IDBonus);
		}

	}
}