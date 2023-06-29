using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Common.Utility;
using UI.Models;
using System.Globalization;

namespace UI.BLL
{
    public class clsSalaryModification
    {
		public static List<clsMiscInfo> Salary_Months_List()
		{
			List<clsMiscInfo> mlist = new List<clsMiscInfo>();
			for(int index=1;index<=12;index++)
            {
				clsMiscInfo obj = new clsMiscInfo();
				obj.Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index);
				mlist.Add(obj);
			}
			return mlist;
		}
		public static List<clsMiscInfo> Salary_Months_List(String LoanNo)
		{
			List<clsMiscInfo> mlist = new List<clsMiscInfo>();
			DataTable dt = clsDatabase.fnDataTable("PRC_Loan_Months", LoanNo);
			foreach(DataRow dr in dt.Rows)
			{
				clsMiscInfo obj = new clsMiscInfo();
				obj.Name = dr["months"].ToString();
				mlist.Add(obj);
			}
			return mlist;
		}

		public static List<clsMiscInfo> Salary_Years_List()
		{
			List<clsMiscInfo> mlist = new List<clsMiscInfo>();
			DataTable dt = clsDatabase.fnDataTable("PRC_Salary_Years");
			foreach (DataRow dr in dt.Rows )
			{
				clsMiscInfo obj = new clsMiscInfo();
				obj.Name = dr["Years"].ToString();
				mlist.Add(obj);
			}
			return mlist;
		}
		public static List<clsMiscInfo> Salary_Years_List(String LoanNo)
		{
			List<clsMiscInfo> mlist = new List<clsMiscInfo>();
			DataTable dt = clsDatabase.fnDataTable("PRC_Loan_Years",LoanNo);
			foreach (DataRow dr in dt.Rows)
			{
				clsMiscInfo obj = new clsMiscInfo();
				obj.Name = dr["years"].ToString();
				mlist.Add(obj);
			}
			return mlist;
		}
		public static Decimal Revised_Value_Checking (String LoanNo, String Month , long Year)
		{
			Decimal Result = 0;
			DataTable dt = clsDatabase.fnDataTable("PRC_Revised_Value_Checking", LoanNo, Month, Year);
			if (dt.Rows.Count>0)
            {
				Result = clsHelper.fnConvert2Decimal(dt.Rows[0][0]);
            }
			return Result;
		}
		public static List<clsSalaryModificationInfo> Salary_Detail(String EmployeeNo, String Monthname, String Year )
		{
			List<clsSalaryModificationInfo> mlist = new List<clsSalaryModificationInfo>();
			DataTable dt = clsDatabase.fnDataTable("PRC_Salary_Detail", EmployeeNo, Monthname, Year);
			foreach (DataRow dr in dt.Rows)
            {
				clsSalaryModificationInfo obj = new clsSalaryModificationInfo();
				obj.RowID = clsHelper.fnConvert2Long(dr["RowID"]);
				obj.DESI = clsHelper.fnConvert2Decimal(dr["ESI"]);
				mlist.Add(obj);
			}

			return mlist;
		}
		public static String Salary_ESI_Modificaiton(clsSalaryModificationInfo info)
		{
			return clsDatabase.fnDBOperation("PRC_Salary_ESI_Modification", info.RowID, info.EmpNo, info.DESI);
				//info.ABasic,info.AHRA, info.AAttire,info.AInternet,
				//info.AMobile,info.AEducation,info.AMedical, info.ALta,
				//info.AContinuos,info.AOvertime,info.ALoadging,info.AProduction,info.ALeaveAmount,
				//info.AStripned,info.APerformance,info.ASpecial,
				//info.AArrear,info.ABooks,info.ATainning,info.AConvenyence,
				//info.AVehicle,info.AFood, info.AGross,
				//info.LWPDays,info.TotalAllowence,
				//info.TotalDeduction,info.NetPayable,
				//info.DESI,info.DPTax,info.DPF,info.DTDS,info.DOther,info.DLoan
				//);
		}
	}
}