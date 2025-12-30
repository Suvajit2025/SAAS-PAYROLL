using Common.Utility;
using DocumentFormat.OpenXml.Spreadsheet;
using MendinePayroll.UI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using UI.Models;

namespace UI.BLL
{
    public class clsLoan
    {
		public static String Payroll_Loan_Autono(String Type)
		{
			String LoanNo = "";
			DataTable DT = clsDatabase.fnDataTable("PRCAutoLoanNo", Type);
			if (DT.Rows.Count >0 )
            {
				LoanNo = DT.Rows[0][0].ToString();
			}
			return LoanNo;
		}
		public static DataTable Payroll_Employee_Auto_Complete()
		{
			return clsDatabase.fnDataTable("PRC_EmpDetails_AutoComplete");
		}
		public static String Payroll_Loan_Add_Edit(clsLoanInfo Info)
		{
			string UserName = HttpContext.Current.Session["username"].ToString();
			return clsDatabase.fnDBOperation("PRC_Employee_Loan",
									Info.IDLoan, Info.LoanNo, Info.LoanType,
									Info.EmpNo, Info.LoanDate,Info.EndDate,
                                    Info.RefNo,Info.RefDate,
                                    Info.LoanAmount, Info.LoanInterestPcn, Info.LoanTenure, 
                                    Info.MonthlyInstallmentAmount, Info.MonthlyInterestAmount,Info.MonthlyLoan,
                                    Info.TotalInterestAmount, Info.TotalLoanAmount,UserName);
        }
        //public static String Payroll_Loan_Processed(List<clsLoanProcessedInfo> Info)
        //{
        //    string UserName = HttpContext.Current.Session["username"].ToString();
        //    DataTable dtDetail = SaveTable();
        //    if (Info.Count > 0)
        //    {
        //        foreach (var item in Info)
        //        {
        //            DataRow DR = dtDetail.NewRow();
        //            DR["SRL"] = item.SRL;
        //            DR["IDDetail"] = item.IDDetail;
        //            DR["IDLoan"] = item.IDLoan;
        //            DR["ReceivedAmount"] = item.ReceivedAmount;
        //            dtDetail.Rows.Add(DR);
        //        }
        //    }
        //    return clsDatabase.fnDBOperation("Payroll_Proc_Loan_Procesed_New", dtDetail, UserName);
        //}
        public static string Payroll_Loan_Processed(List<clsLoanProcessedInfo> info)
        {
            if (info == null || info.Count == 0)
                return "No records to process.";

            // ✅ Get username safely from session (fallback if missing)
            string EntryUser = HttpContext.Current?.Session?["username"]?.ToString() ?? "system";

            try
            {
                foreach (var item in info)
                {
                    if (item == null)
                        continue;

                    // ✅ Map model fields directly
                    long IDLoan = item.IDLoan;
                    string InstallmentMonth = item.Month ?? string.Empty;
                    int InstallmentYear = Convert.ToInt32(item.Year);
                    decimal ReceivedAmount = item.ReceivedAmount;  // Already decimal
                    decimal OpeningBalance = Convert.ToDecimal(item.ClosingBalance);  // Passed from SP, used as next opening

                    // ✅ Ensure minimal validation before DB call
                    if (IDLoan <= 0 || string.IsNullOrEmpty(InstallmentMonth) || InstallmentYear <= 0)
                        continue;

                    var dbResult = clsDatabase.fnDBOperation("PRC_UpdateOrInsert_LoanProcess", IDLoan, InstallmentMonth, InstallmentYear, ReceivedAmount, OpeningBalance, EntryUser);

                    
                }

                return "OK"; // or return aggregated DB result as needed
            }
            catch (Exception ex)
            {
                // Log exception as appropriate and return message
                // e.g. Log.Error(ex);
                return $"Error: {ex.Message}";
            }
        }

        public static String Payroll_Loan_Processed_All(List<clsLoanProcessedInfo> Info)
		{
			string UserName = HttpContext.Current.Session["username"].ToString();
			DataTable dtDetail = SaveTable();
			if (Info.Count > 0)
			{
				foreach (var item in Info)
				{
					DataRow DR = dtDetail.NewRow();
					DR["SRL"] = item.SRL;
					DR["IDDetail"] = item.IDDetail;
					DR["IDLoan"] = item.IDLoan;
					DR["ReceivedAmount"] = item.ReceivedAmount;
					dtDetail.Rows.Add(DR);
				}
			}
			return clsDatabase.fnDBOperation("Payroll_Proc_Loan_Procesed_New", dtDetail, UserName);
		}
		private static DataTable SaveTable()
		{
			DataTable DT = new DataTable("Data");
			DT.Columns.Add("SRL", typeof(System.Int32));
			DT.Columns.Add("IDDetail", typeof(System.Int32));
			DT.Columns.Add("IDLoan", typeof(System.Int32));
			DT.Columns.Add("ReceivedAmount", typeof(System.Decimal));
			return DT;
		}
		public static DataTable Loan_Process_Data(long IDEmployee, int Year, string MonthName)
		{
            //return clsDatabase.fnDataTable("Payroll_Proc_Get_All_Emp_Loan_Process_Data  ", IDEmployee);
            return clsDatabase.fnDataTable("PRC_Loan_Installment_ByMonth", Year, MonthName, IDEmployee);
        }
		public static DataTable  Loan_List(long Employeeno)
		{
			return clsDatabase.fnDataTable("Payroll_PRC_Employee_loan_List", Employeeno);
		}
		public static DataTable Loan_Detail(String LoanNo)
		{
			return clsDatabase.fnDataTable("Payroll_Proc_Get_Emp_Loan_Details", LoanNo);
		}
		public static DataTable Loan_Revised_Detail(String LoanNo)
		{
			return clsDatabase.fnDataSet("Payroll_Proc_Revised_Loan_Details", LoanNo).Tables[0];
		}
		public static DataTable Loan_Revised_Month_List(String LoanNo)
		{
			return clsDatabase.fnDataSet("Payroll_Proc_Revised_Loan_Details", LoanNo).Tables[1];
		}
		public static DataTable Change_Loan(String LoanNo,Decimal InsAmount, String Month,String Year)
		{
			return clsDatabase.fnDataTable("Payroll_PRC_Change_Loan_Installment_New", LoanNo, InsAmount, Month,Year);
		}

		public static DataTable Sanctioned_Realised_Due_Loan(long EmpId)
		{
			return clsDatabase.fnDataTable("PRC_GET_Loan_Sanctioned_Realised_Due_Details", EmpId);
		}

		public static double  Employee_Loan_Amount(long IDEmployeeNo)
        {
			double Value = 0; 
			DataTable dt = clsDatabase.fnDataTable("PRC_Employee_Loan_Installment", IDEmployeeNo);
			Value = clsHelper.fnConvert2Double(dt.AsEnumerable().Sum(x => x.Field<Decimal>("Amount")));
			return Value;
        }
		public static DataTable Employee_Loan_Amount_Detail(long IDEmployeeNo)
		{
			return clsDatabase.fnDataTable("PRC_Employee_Loan_Installment", IDEmployeeNo);
		}
		public static DataTable Employee_Loan_Outstanding_Installment(String Month, String Year,long IDEmployee)
		{
			return clsDatabase.fnDataTable("PRC_Loan_Outstanding", Month,Year, IDEmployee);
		}

		public static DataTable Loan_Application_List(long idemployee)
		{
			return clsDatabase.fnDataTable("PRC_Employee_Loan_Application_List", idemployee);
		}
		public static DataTable Loan_Application_Detail(long idapplication)
		{
			return clsDatabase.fnDataTable("PRC_Employee_Loan_Application_Detail", idapplication);
		}
		public static String Loan_Application_Save(clsLoanApplicationInfo info)
		{
			return clsDatabase.fnDBOperation("PRC_Employee_Loan_Application_Add_Edit", 
											info.IDApplicatioon,info.AppDate,info.EmployeeNo, 
											info.LoanAmount,info.Installment,info.Reason,info.User);
		}
		public static String Loan_Application_Self_Approval(long IDApplication, long Levelno)
		{
			return clsDatabase.fnDBOperation("PRC_Employee_Loan_Application_Approval", Levelno);
		}
		public static DataTable Loan_Application_No()
		{
			return clsDatabase.fnDataTable("Loan_Application_No");
		}
		public static DataTable Loan_Approval_List()
		{
			return clsDatabase.fnDataTable("PRC_Employee_Loan_Approval_List");
		}
		public static String Loan_Approval(clsLoanApprovalInfo info)
		{
			return clsDatabase.fnDBOperation("PRC_Employee_Loan_Approval",
										info.IDApplication, info.Approval, info.ApprovedAmount,
										info.Rejected, info.RejectReason, info.User);
		}

		public static DataTable Employee_Having_Loan_List()
		{
			return clsDatabase.fnDataTable("PRC_Get_Employee_Having_Loan");
		}

	}
}