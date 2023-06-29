using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class LeaveDetailsModel
    {
        public int ID { get; set; }
        public int? Empno { get; set; }
        public double? LeaveDays { get; set; }
        public double? LateLeaveDays { get; set; }
        public int? CreatedBy { get; set; }
        public string Month { get; set; }
        public int? Year { get; set; }
        public double? Noofdaysabsent { get; set; }
        public string Type { get; set; }
    }
}
