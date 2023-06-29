using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.Models
{
    public class clsSalaryModificationInfo
    {
        public long RowID { get; set; } = 0;
        public String EmpNo { get; set; } = "";
        public decimal ABasic { get; set; } = 0;
        public decimal AHRA { get; set; } = 0;
        public decimal AAttire { get; set; } = 0;
        public decimal AInternet { get; set; } = 0;
        public decimal AMobile { get; set; } = 0;
        public decimal AEducation { get; set; } = 0;
        public decimal AMedical { get; set; } = 0;
        public decimal ALta { get; set; } = 0;
        public decimal AGross { get; set; } = 0;
        public decimal AContinuos { get; set; } = 0;
        public decimal AOvertime { get; set; } = 0;
        public decimal ALoadging { get; set; } = 0;
        public decimal AProduction { get; set; } = 0;
        public decimal ALeaveAmount { get; set; } = 0;
        public decimal AStripned { get; set; } = 0;
        public decimal APerformance { get; set; } = 0;
        public decimal ASpecial { get; set; } = 0;
        public decimal AArrear { get; set; } = 0;
        public decimal ABooks { get; set; } = 0;
        public decimal ATainning { get; set; } = 0;
        public decimal AConvenyence { get; set; } = 0;
        public decimal AVehicle { get; set; } = 0;
        public decimal AFood { get; set; } = 0;

        public decimal DTDS { get; set; } = 0;
        public decimal DOther { get; set; } = 0;
        public decimal DPF { get; set; } = 0;
        public decimal DESI { get; set; } = 0;
        public decimal DPTax { get; set; } = 0;
        public decimal DLoan { get; set; } = 0;

        public decimal TotalAllowence { get; set; } = 0;
        public decimal TotalDeduction { get; set; } = 0;
        public decimal NetPayable { get; set; } = 0;
        public decimal LWPDays  { get; set; } = 0;
        public decimal ISPaid  { get; set; } = 0;
      
    }
}