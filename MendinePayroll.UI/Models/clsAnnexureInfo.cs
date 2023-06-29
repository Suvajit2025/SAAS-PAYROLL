using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class clsAnnexure92BInfo
    {
        public long IDAnnexure{ get; set; } = 0;
        public String EmpMonth { get; set; } = "";
        public int EmpYear { get; set; } = 0;
        public long IDEmployee { get; set; } = 0;
        public String EmployeeCode { get; set; } = "";
        public String EmployeeName{ get; set; } = "";


        public String PANNCard { get; set; } = "";
        public String ChallanNo { get; set; } = "";
        public String SectionCode { get; set; } = "";
        
        public Decimal PaymentAmount { get; set; } = 0;

        public String TaxDeductionDate { get; set; } = "";
        public String AmountPaidDate { get; set; } = "";

        public Decimal TDSAmount { get; set; } = 0;
        public Decimal SurchargeAmount { get; set; } = 0;
        public Decimal HealthECAmount { get; set; } = 0;
        public Decimal SHECAmount { get; set; } = 0;
        public Decimal TotalAmount { get; set; } = 0;

        public String Reason { get; set; } = "";
        public String Certificateno { get; set; } = "";
        public String EntryUser { get; set; } = "";

    }

    public class clsForm16Info
    {
        public long IDForm { get; set; } = 0;
        public clsEmployeeInfo Employee  { get; set; } = new clsEmployeeInfo();
        public int EmpYear { get; set; } = 0;
        public String Company { get; set; } = "";
        public String StatusA { get; set; } = "";
        public String FilePathA { get; set; } = "";
        public String FileNameA { get; set; } = "";
        public String StatusB { get; set; } = "";
        public String FilePathB { get; set; } = "";
        public String FileNameB { get; set; } = "";
        public String EntryUser { get; set; } = "";

    }
    public class clsForm16UPLoadInfo
    {
        public int EmpYear { get; set; } = 0;
        public String Empno { get; set; } = "";
        public String Filename { get; set; } = "";
        public HttpPostedFileBase PostedFile { get; set; }
    }
}