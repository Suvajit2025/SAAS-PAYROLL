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
    public class clsAccessLog
    {
		public static String AccessLog_Save(clsAccessLogInfo info)
		{
			string UserName = HttpContext.Current.Session["username"].ToString();
			return clsDatabase.fnDBOperation("PRC_Payroll_Access_Log",UserName, info.AccessType);
        }
	}
}