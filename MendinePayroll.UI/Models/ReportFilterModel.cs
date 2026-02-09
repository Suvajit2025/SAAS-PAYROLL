using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class ReportFilterModel
    {
        // 1 & 2: Time Filters
        public int year { get; set; }
        public int month { get; set; }

        // 3: Company (Multiselect)
        public List<int> companyIds { get; set; }

        // 4: Department (Multiselect)
        public List<int> deptIds { get; set; }

        // 5: Employment Type / Category (Multiselect)
        public List<int> categoryIds { get; set; }

        // 6: Pay Group (Multiselect)
        public List<int> paygroupIds { get; set; }

        // 7: Status (Single Select: "ACTIVE", "INACTIVE", or "All")
        public string status { get; set; }

        // 8: Employee (Multiselect)
        public List<int> empIds { get; set; }

        /// <summary>
        /// Constructor to prevent null reference errors during string.Join
        /// </summary>
        public ReportFilterModel()
        {
            companyIds = new List<int>();
            deptIds = new List<int>();
            categoryIds = new List<int>();
            paygroupIds = new List<int>();
            empIds = new List<int>();
        }
    }
}