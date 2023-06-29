using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
     public class EmpoloyeeSalaryConfigManualValueModel
    {
        public int ID { get; set; }
        public int? EmployeeSalaryConfigid { get; set; }
        public int? PayConfigID { get; set; }
        public string Values { get; set; }
        public string PayConfigIDS { get; set; }
    }
}
