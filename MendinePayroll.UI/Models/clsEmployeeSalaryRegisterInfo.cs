using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class clsEmployeeSalaryRegisterInfo
    {

        public String EmployeeName { get; set; } = "";
        public String SalaryDate { get; set; } = "";
        public String SalaryMonth { get; set; } = "";
        public int SalaryYear { get; set; } = 0;
        public Decimal Basic { get; set; } = 0;
        public Decimal HRA { get; set; } = 0;
        public Decimal Attir { get; set; } = 0;
        public Decimal Internet { get; set; } = 0;
        public Decimal Mobile { get; set; } = 0;
        public Decimal Medical { get; set; } = 0;
        public Decimal Education { get; set; } = 0;
        public Decimal LTA { get; set; } = 0;
        public Decimal ContinousAttendance { get; set; } = 0;
        public Decimal Overtime { get; set; } = 0;
        public Decimal Lodging { get; set; } = 0;
        public Decimal Fooding { get; set; } = 0;
        public Decimal Trainning { get; set; } = 0;
        public Decimal BooksPeriodicals { get; set; } = 0;
        public Decimal Convenyence { get; set; } = 0;
        public Decimal Vehical { get; set; } = 0;
        public Decimal TDS { get; set; } = 0;
        public Decimal Stipned { get; set; } = 0;
        public Decimal Arrear { get; set; } = 0;
        public Decimal Production { get; set; } = 0;
        public Decimal PF { get; set; } = 0;
        public Decimal ESI { get; set; } = 0;
        public Decimal PTax { get; set; } = 0;
        public Decimal Loan { get; set; } = 0;

    }
}