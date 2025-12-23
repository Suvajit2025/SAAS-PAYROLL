using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MendinePayroll.Models
{
    public class EmpbasicModel
    {
        public EmpbasicModel()
        {
            EmpbasicList = new List<EmpbasicModel>();
            selectListItems = new List<SelectListItem>();
        }
        public int empid { get; set; }
        public string TenantID { get; set; }
        public string empinitialname { get; set; }
        public string empfirstname { get; set; }
        public string empmiddlename { get; set; }
        public string emplastname { get; set; }
        public DateTime? empdob { get; set; }
        public string empgender { get; set; }
        public string companycode { get; set; }
        public string empcategory { get; set; }
        public int empno { get; set; }
        public string empcode { get; set; }
        public string adharno { get; set; }
        public int? adahrcardimageid { get; set; }
        public string emppersonalcontactno { get; set; }
        public DateTime? joining { get; set; }
        public int? empdept { get; set; }
        public int? emppost { get; set; }
        public int? empdesignation { get; set; }
        public int? empreportto { get; set; }
        public int? empreporttoperson { get; set; }
        public string empstatus { get; set; }
        public DateTime? closeingdate { get; set; }
        public int? postingstateid { get; set; }
        public string emppersonalemail { get; set; }
        public string empofficialcontactno { get; set; }
        public string empemail { get; set; }
        public DateTime empaddon { get; set; }
        public string empaddby { get; set; }
        public DateTime? lastupdatedon { get; set; }
        public string lastupdatedby { get; set; }
        public string postname { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }

        public string Empdofb { get; set; }
        public string Closeingdateof { get; set; }
        public string Joiningdate { get; set; }

        public string EmployeeName { get; set; }
        public List<SelectListItem> selectListItems { get; set; }
        public List<EmpbasicModel> EmpbasicList { get; set; }
    }
}
