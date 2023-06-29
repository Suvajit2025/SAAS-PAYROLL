using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class LoanTransactionModel
    {
        public LoanTransactionModel()
        {
            LoanList = new List<LoanTransactionModel>();
        }
        public int LoanTransactionId { get; set; }
        public int? LoanId { get; set; }
        public int? Empid { get; set; }
        public decimal? InstallmentAmount { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? ClosingBalance { get; set; }
        public System.DateTime? DateOfInStallment { get; set; }
        public List<LoanTransactionModel> LoanList { get; set; }
    }
}
