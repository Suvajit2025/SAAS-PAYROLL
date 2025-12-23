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
    public class LoanModel
    {
        public string LoanType { get; set; }
        public string LoanNo { get; set; }
        public int IDEmployee { get; set; }
        public string EmpCode { get; set; }
        public string LoanDate { get; set; }
        public string EndDate { get; set; }

        public string RefNo { get; set; }
        public string RefDate { get; set; }

        public decimal LoanAmount { get; set; }
        public int Tenure { get; set; }
        public decimal InterestRate { get; set; }
        public int GracePeriod { get; set; }

        public string MonthlyPrincipal { get; set; }
        public string MonthlyInterest { get; set; }
        public string TotalEMI { get; set; }
        public string TotalInterest { get; set; }
        public string TotalLoan { get; set; }

        public int RepaymentStartMonth { get; set; }
        public int RepaymentStartYear { get; set; }

        public string Remarks { get; set; }
    }
    public class ProcessBulkLoanModel
    {
        public string JsonData { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string UserName { get; set; }
    }
    public class LoanScheduleViewModel
    {
        public int EMI_No { get; set; }
        public string MonthName { get; set; }
        public int MonthNo { get; set; }
        public int YearNo { get; set; }

        public decimal BalanceBefore { get; set; }
        public decimal PrincipalDue { get; set; }
        public decimal InterestDue { get; set; }
        public decimal TotalDue { get; set; }
        public decimal BalanceAfter { get; set; }

        public decimal? AmountPaid { get; set; }
        public decimal? InterestPaid { get; set; }
        public int? SalaryProcessed { get; set; }
        public string ProcessSource { get; set; }
        public string Status { get; set; }
    }
    public class LoanAdjustmentRequest
    {
        public int LoanId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentMode { get; set; }
        public string RefNo { get; set; }
        public string Remarks { get; set; }

        public decimal NewEMI { get; set; }
        public int NewTenure { get; set; }
        public decimal NewBalance { get; set; }
        public string Strategy { get; set; }
    }

    public class LoanEditViewModel
    {
        public int IDLoan { get; set; }
        public string LoanType { get; set; }
        public string LoanNo { get; set; }
        public int IDEmployee { get; set; }
        public string EmployeeName { get; set; }
        public string EmpCode { get; set; }

        public string LoanDate { get; set; }
        public string EndDate { get; set; }
        public string RefNo { get; set; }
        public string RefDate { get; set; }

        public decimal LoanAmount { get; set; }
        public int LoanTenure { get; set; }
        public decimal LoanInterestPcn { get; set; }
        public decimal MonthlyInstallment { get; set; }

        public int GracePeriodMonths { get; set; }
        public int RepaymentStartMonth { get; set; }
        public int RepaymentStartYear { get; set; }

        public decimal BalanceAmount { get; set; }

        public int? RemainingTenure { get; set; }
        public decimal? RevisedEMI { get; set; }
        public DateTime? RevisedDate { get; set; }

        public string Remarks { get; set; }
    }

}