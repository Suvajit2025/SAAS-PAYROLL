using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using UI.Models;
using System.Configuration;
using MendinePayroll.UI.Models;

namespace MendinePayroll.UI.BLL
{
	public class clsAnnexure
	{

		public static List<clsAnnexure92BInfo> Annexure92B_Data(String Month, int Year, DataTable dtEmp)
		{
			List<clsAnnexure92BInfo> mlist = new List<clsAnnexure92BInfo>();
			DataTable dt = clsDatabase.fnDataTable("Payroll_Annexure_92B_List", Month, Year, dtEmp);
			foreach (DataRow dr in dt.Rows)
			{
				clsAnnexure92BInfo obj = new clsAnnexure92BInfo();
				obj.IDAnnexure = clsHelper.fnConvert2Long(dr["IDAnnexure"]);
				obj.IDEmployee = clsHelper.fnConvert2Long(dr["IDEmployee"]);
				obj.EmployeeCode = dr["EmployeeNo"].ToString();
				obj.EmployeeName = dr["EmployeeName"].ToString();
				obj.ChallanNo = dr["ChallanNo"].ToString();
				obj.PANNCard = dr["PanCard"].ToString();
				obj.SectionCode = dr["SectionCode"].ToString();
				obj.TaxDeductionDate = dr["TaxDeductionDate"].ToString();
				obj.PaymentAmount = clsHelper.fnConvert2Decimal(dr["PaymentAmount"]);
				obj.AmountPaidDate = dr["AmountPaidDate"].ToString();
				obj.TDSAmount = clsHelper.fnConvert2Long(dr["TDSAmount"]);
				obj.SurchargeAmount = clsHelper.fnConvert2Long(dr["SurchargeAmount"]);
				obj.HealthECAmount = clsHelper.fnConvert2Long(dr["HealthECAmount"]);
				obj.SHECAmount = clsHelper.fnConvert2Long(dr["SHECAmount"]);
				obj.TotalAmount = clsHelper.fnConvert2Long(dr["TotalAmount"]);
				obj.Reason = dr["Reason"].ToString();
				obj.Certificateno = dr["Certificateno"].ToString();
				obj.EmpMonth = dr["EmpMonth"].ToString();
				obj.EmpYear = clsHelper.fnConvert2Int(dr["EmpYear"].ToString());
				mlist.Add(obj);
			}
			return mlist;
		}
		public static String Annexure92B_Save(List<clsAnnexure92BInfo> info)
		{
			List<clsAnnexure92BInfo> mlist = new List<clsAnnexure92BInfo>();
			string UserName = HttpContext.Current.Session["username"].ToString();
			DataTable dt = fnSaveData();
			if (info.Count>0)
            {
				foreach (clsAnnexure92BInfo obj in info )
                {
					DataRow dr = dt.NewRow();
					dr["IDAnnexure"] = obj.IDAnnexure;
					dr["IDEmployee"] = obj.IDEmployee;
					dr["ChallanNo"] = obj.ChallanNo;
					dr["EmpPAN"] = obj.PANNCard;
					dr["EmpMonth"] = obj.EmpMonth;
					dr["EmpYear"] = obj.EmpYear;
					dr["SectionCode"] = obj.SectionCode;
					dr["PaymentAmount"] = obj.PaymentAmount;
					dr["TaxDate"] = obj.TaxDeductionDate;
					dr["PaidDate"] = obj.AmountPaidDate;
					dr["TDS"] = obj.TDSAmount;
					dr["Surcharge"] = obj.SurchargeAmount;
					dr["HealthEC"] = obj.HealthECAmount;
					dr["SHEC"] = obj.SHECAmount;
					dr["Total"] = obj.TotalAmount;
					dr["TotalTax"] = obj.TotalAmount;
					dr["Reason"] = obj.Reason;
					dr["Certificate"] = obj.Certificateno;
					dr["EntryUser"] = UserName;
					dt.Rows.Add(dr);
				}
            }
			return clsDatabase.fnDBOperation("PRC_Annexure_92B_Save", dt);
		}
		private static  DataTable  fnSaveData()
        {
			DataTable dt = new DataTable("Data");
			dt.Columns.Add("IDAnnexure", typeof(System.Int32));
			dt.Columns.Add("IDEmployee", typeof(System.Int32));
			dt.Columns.Add("Challanno", typeof(System.String));
			dt.Columns.Add("EmpPAN", typeof(System.String));
			dt.Columns.Add("EmpMonth", typeof(System.String));
			dt.Columns.Add("EmpYear", typeof(System.Int16));
			dt.Columns.Add("SectionCode", typeof(System.String));
			dt.Columns.Add("PaymentAmount", typeof(System.Decimal));
			dt.Columns.Add("TaxDate", typeof(System.String));
			dt.Columns.Add("PaidDate", typeof(System.String));
			dt.Columns.Add("TDS", typeof(System.Decimal));
			dt.Columns.Add("Surcharge", typeof(System.Decimal));
			dt.Columns.Add("HealthEC", typeof(System.Decimal));
			dt.Columns.Add("SHEC", typeof(System.Decimal));
			dt.Columns.Add("Total", typeof(System.Decimal));
			dt.Columns.Add("TotalTax", typeof(System.Decimal));
			dt.Columns.Add("Reason", typeof(System.String));
			dt.Columns.Add("Certificate", typeof(System.String));
			dt.Columns.Add("EntryUser", typeof(System.String));
			return dt;

        }

	}
}
		