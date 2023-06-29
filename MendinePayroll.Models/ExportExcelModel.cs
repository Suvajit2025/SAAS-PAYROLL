using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class ExportExcelModel
    {

        public int Empid { get; set; }
        public DateTime? Prefixfromdate { get; set; }
        public DateTime? suffixtodate { get; set; }
        public string Month { get; set; }
        public int TotalDays { get; set; }
        public int empno { get; set; }
        public int year { get; set; }



        public int SLNO { get; set; }
        public string EMPLOYEECODE { get; set; }
        public string NAME_OF_EMPLOYEES { get; set; }
        public string SITE { get; set; }
        public string W { get; set; }
        public string O { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string E { get; set; }
        public string T_D { get; set; }
        public double? BASIC { get; set; }
        public double? STIPEND { get; set; }
        public int? WORKING_DAYS { get; set; }
        public double? DAY_S_ABSENT { get; set; }
        public double? DEDUCTION_AGAINST_ABSENSE { get; set; }
        public double? E_L_ { get; set; }
        public double? PAYMENT_AGAINST_E_L_ { get; set; }
        public double? GROSS_AMOUNT { get; set; }
        public double? HRA { get; set; }
        public double? LODGING_ALLOWANCE { get; set; }
        public double? Attire_Allowance { get; set; }
        public double? TRAINING_ALLOWANCE_ACHIEVED { get; set; }
        public double? MOBILE { get; set; }
        public double? BOOKS_AND_PERIODICALS_ALLOWANCE { get; set; }
        public double? INTERNET_ALLOWANCE { get; set; }
        public double? CONINUOUS_ATTENDENCE_ALLOWANCE { get; set; }
        public double? Conveyence_Allowance { get; set; }
        public double? PRODUCTION_BONUS { get; set; }
        public double? PRODUCTION_BONUS_ACHIEVED { get; set; }
        public double? OVERTIME { get; set; }
        public double? OVERTIME_AMOUNT { get; set; }
        public double? MEDICAL_ALLOWANCES { get; set; }
        public double? LTA { get; set; }
        public double? EDUCATION_ALLOWANCES { get; set; }
        public double? TOTAL_ALLOWANCES { get; set; }
        public double? PF { get; set; }
        public double? PTAX { get; set; }
        public double? E_S_I { get; set; }
        public double? TOTAL_DEDUCTION { get; set; }
        public decimal? OPENING_BALANCE { get; set; }
        public double? DEDUCTION { get; set; }
        public decimal? CLOSING_BALANCE { get; set; }
        public double? NET_AMOUNT { get; set; }
        public String PAID_DATE { get; set; }
    }
}
