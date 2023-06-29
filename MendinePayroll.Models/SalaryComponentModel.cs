using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MendinePayroll.Models
{
   public class SalaryComponentModel
    {
        public SalaryComponentModel()
        {
            SalaryComponentList = new List<SalaryComponentModel>();
            mastermodel = new PayGroupModel();
            MasterModel = new SalaryComponentTypeModel();
        }
        public int SalaryComponentId { get; set; }
        // public int PayGroupEmployeeID { get; set; }
        public string SalaryComponent { get; set; }
        public int SalaryComponentCode { get; set; }

        public int? SalaryComponentTypeID { get; set; }

        public int PayGroupsID { get; set; }
        public List<SalaryComponentModel> SalaryComponentList { get; set; }
        public string SalaryComponentType { get; set; }
        public string PayGroupName { get; set; }
        public string Description { get; set; }
        public PayGroupModel mastermodel { get; set; }
        public SalaryComponentTypeModel MasterModel { get; set; }
    }
}
