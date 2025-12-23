using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MendinePayroll.Models
{
    public class PayGroupModel
    {
        public PayGroupModel()
        {
            PayGroupList = new List<PayGroupModel>();
            masterModel = new EmpbasicModel();
            selectListItems = new List<SelectListItem>();
        }
            public List<PayGroupModel> PayGroupList { get; set; }
            public int PayGroupID { get; set; }
            public string PayGroupName { get; set; }
            public string Description { get; set; }
            public string PayGroupMasterCode { get; set; }
            public string EMPLOYEENAME { get; set; }
            public int? ConcernHRPersonnel { get; set; }
            public EmpbasicModel masterModel { get; set; }
            public List<SelectListItem> selectListItems { get; set; }
            public string TenantID { get; set; }=string.Empty;
            public string EntryUser { get; set; }=String.Empty;
    }
}
