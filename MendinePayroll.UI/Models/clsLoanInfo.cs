using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.Models
{
    public class clsLoanInfo
    {
        public long IDLoan { get; set; } = 0;
        public String LoanNo { get; set; } = "";
        public String LoanDate { get; set; } = "";
        public String LoanType { get; set; } = "";
        public String RefDate { get; set; } = "";
        public String RefNo { get; set; } = "";
        public long EmpNo { get; set; } = 0;
       
        public String EndDate { get; set; } = "";
        public Decimal LoanAmount { get; set; } = 0;
        public Decimal LoanInterestPcn { get; set; } = 0;
        public int LoanTenure { get; set; } = 0;
        public Decimal MonthlyInstallmentAmount { get; set; } = 0;
        public Decimal MonthlyInterestAmount { get; set; } = 0;
        public Decimal MonthlyLoan { get; set; } = 0;
        public Decimal TotalInterestAmount  { get; set; } = 0;
        public Decimal TotalLoanAmount { get; set; } = 0;
    }
}