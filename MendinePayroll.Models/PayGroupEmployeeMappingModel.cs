using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class PayGroupEmployeeMappingModel
    {
        //  public int Id { get; set; }
    
       public PayGroupEmployeeMappingModel()
        {
            PayGroupEmployeeMappingList = new List<PayGroupEmployeeMappingModel>();
        }
        public int PayGroupEmployeeID { get; set; }
        public int PayGroupID { get; set; }
        public int EmployeeID { get; set; }
        public string PayGroupName { get; set; }
        public string Description { get; set; }
        public string PayGroupMasterCode { get; set; }

        public int? ConcernHRPersonnel { get; set; }
        public List<PayGroupEmployeeMappingModel> PayGroupEmployeeMappingList { get; set; }
    }
}
