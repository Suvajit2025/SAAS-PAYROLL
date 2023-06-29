using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using UI.Models;
using System.Configuration;

namespace UI.BLL
{
    public class clsSalaryProcessValidation
	{
		public static List<clsPayGroupInfo> Pay_Group_List(long IDEmployee)
		{
			List<clsPayGroupInfo> list = new List<clsPayGroupInfo>();
			DataTable DT = clsDatabase.fnDataTable("PRC_PayGroup_List", IDEmployee);
			foreach (DataRow dr in DT.Rows)
            {
				clsPayGroupInfo obj = new clsPayGroupInfo();
				obj.IDPaygroup = clsHelper.fnConvert2Long(dr["PayGroupID"]);
				obj.Name = dr["PayGroupName"].ToString();
				list.Add(obj);
			}

			return list;
		}
		public static List<clsPayrollInfo> Payroll_Year()
		{
			List<clsPayrollInfo> list = new List<clsPayrollInfo>();
			DataTable DT = clsDatabase.fnDataTable("PRC_Payroll_Year_List");
			foreach (DataRow dr in DT.Rows)
			{
				clsPayrollInfo obj = new clsPayrollInfo();
				obj.Year = clsHelper.fnConvert2Long(dr["Year"]);
				list.Add(obj);
			}
			return list;
		}
		public static List<clsPayrollInfo> Payroll_Month()
		{
			List<clsPayrollInfo> list = new List<clsPayrollInfo>();
			DataTable DT = clsDatabase.fnDataTable("PRC_Payroll_Month_List");
			foreach (DataRow dr in DT.Rows)
			{
				clsPayrollInfo obj = new clsPayrollInfo();
				obj.Month = dr["Month"].ToString();
				list.Add(obj);
			}
			return list;
		}
		public static List<clsSalaryProcessValidationInfo> Payroll_Proces_List(long IDPayGroup, String Month, long Year)
		{
			List<clsSalaryProcessValidationInfo> list = new List<clsSalaryProcessValidationInfo>();
			DataTable DT = clsDatabase.fnDataTable("PRC_Payroll_Proces_List", IDPayGroup, Month, Year);
			foreach (DataRow dr in DT.Rows)
			{
				clsSalaryProcessValidationInfo obj = new clsSalaryProcessValidationInfo();
				obj.IDPayroll = clsHelper.fnConvert2Long(dr["RowID"]);
				obj.IDEmployee= clsHelper.fnConvert2Long(dr["Empid"]);
				obj.EmployeeName = dr["Employee"].ToString();
				obj.EmployeeNo = dr["EmpNo"].ToString();
				obj.Department = dr["Department"].ToString();
				obj.PayGroup = dr["PayGroupName"].ToString();
				list.Add(obj);
			}
			return list;
		}

		public static String Payroll_Proces_Validation(long IDPayroll, long IDEmployee, String Month, String Year)
		{
			List<clsSalaryProcessValidationInfo> list = new List<clsSalaryProcessValidationInfo>();
			DataTable DT = clsDatabase.fnDataTable("PRC_Payroll_Proces_Validation", IDPayroll, IDEmployee, Month, Year);
			String Result = DT.Rows[0][0].ToString();
			return Result;
		}
	}
}