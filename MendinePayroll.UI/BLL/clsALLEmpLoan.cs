using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.BLL.Report
{
    public class clsALLEmpLoan
    {
        public static DataTable Loan_List_Employee_Wise_All()
        {
            return clsDatabase.fnDataTable( "Payroll_PRC_EmployeeWiseLoan");
        }
        public static DataTable Employee_Wise_LoanDetails(long IDEmp)
        {
            return clsDatabase.fnDataTable("Payroll_PRC_Employee_Loan_Register", IDEmp);
        }
        public static DataTable Employee_Loan_Ledger(String IDEmp)
        {
            return clsDatabase.fnDataTable("PRC_Loan_Ledger", IDEmp);
        }
    }
}