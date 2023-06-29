using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.Models
{
    public class clsLoanProcessedInfo
    {
        public long SRL { get; set; }
        public String EmployeeNo { get; set; }
        public String EmployeeName { get; set; }
        public long IDDetail { get; set; }
        public long IDLoan { get; set; }
        public Decimal ReceivedAmount { get; set; }
    }
    public class clsChangeLoanInfo
    {
        public String LoanNo { get; set; }
        public Decimal InsAmount { get; set; }
        public String ChangeMonth { get; set; }
        public String ChangeYear { get; set; }
    }

    public class clsLoanApplicationInfo
    {
        public long  IDApplicatioon { get; set; }
        public String Appno { get; set; }
        public String AppDate { get; set; }
        public long IDEmployee  { get; set; }
        public String EmployeeNo { get; set; }
        public String EmployeeName { get; set; }
        public Decimal LoanAmount { get; set; }
        public Decimal Installment { get; set; }
        public String Reason { get; set; }
        public String User { get; set; }
    }
}