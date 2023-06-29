using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MendinePayroll.UI.Models;
using System.Data;
using Common.Utility;

namespace MendinePayroll.UI.BLL
{
    public class clsMaster
    {
        public static List<clsEmployeeInfo> EmployeeList()
        {
            List<clsEmployeeInfo> mlist = new List<clsEmployeeInfo>();
            DataTable DT = clsDatabase.fnDataTable("PRC_Employee_List");
            foreach (DataRow DR in DT.Rows)
            {
                mlist.Add(new clsEmployeeInfo()
                {
                    IDEmployee = clsHelper.fnConvert2Int(DR["Empid"]),
                    EmployeeNo = DR["EmpNo"].ToString(),
                    EmployeeName = DR["Employee"].ToString()
                });
            }
            return mlist;

        }

    }
}