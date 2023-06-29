using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class EmployeeLoanModel
    {
        public int LoanId { get; set; }
        public int? Empid { get; set; }
        public double? LoanAmount { get; set; }
        public double? MonthlyInstallment { get; set; }
        public bool? IsStart { get; set; }
        public System.DateTime? LoanDate { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? ClosingBalance { get; set; }
        public int? LoanTransactionId { get; set; }
    }
}
