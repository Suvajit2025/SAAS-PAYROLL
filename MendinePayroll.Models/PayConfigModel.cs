using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class PayConfigModel
    {
        public PayConfigModel()
        {
            PayConfigList = new List<PayConfigModel>();
        }
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public bool? IScalculative { get; set; }
        public string EntryType { get; set; }
        public List<PayConfigModel> PayConfigList { get; set; }
    }
}
