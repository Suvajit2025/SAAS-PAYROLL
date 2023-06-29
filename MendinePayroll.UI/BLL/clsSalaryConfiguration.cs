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
    public class clsSalaryConfiguration
	{
		
		public static String Salary_Configuration_Save(clsSalaryConfigurationInfo Info)
		{
			string UserName = HttpContext.Current.Session["username"].ToString();
			return clsDatabase.fnDBOperation("PRC_Payroll_Configuration_Save",
									Info.IDConfiguration, Info.UserName, Info.Password);
        }
		public static DataTable Salary_Configuration_Detail(long IDConfiguration)
		{
			return clsDatabase.fnDataTable("PRC_Payroll_Configuration_Detail", IDConfiguration);
		}
		public static Boolean Salary_Authentication(String UserName, String Password)
		{
			Boolean Value = true;
			DataTable  DT =clsDatabase.fnDataTable("PRC_Payroll_Configuration_Detail_By_Name_Password", UserName, Password);
			if (DT.Rows.Count <=0) { Value =false ; }
			return Value;
		}
	}
}