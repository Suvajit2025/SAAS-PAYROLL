using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Utility;
using System.Data;

namespace MendinePayroll.UI.BLL
{
    public class clsState
    {
        public static  List<clsStateInfo> StateList()
        {
            List<clsStateInfo> mlist = new List<clsStateInfo>();
            DataTable DT = clsDatabase.fnDataTable("PRC_HRMS_ESSP_TstateMaster");
            foreach(DataRow DR  in DT.Rows)
            {
                clsStateInfo obj = new clsStateInfo();
                obj.IDState = clsHelper.fnConvert2Long(DR["stateid"]);
                obj.Name = DR["statename"].ToString();
                mlist.Add(obj);
            }
            return mlist;
        }


    }
    public class clsStateInfo
    {
        public long IDState { get; set; } = 0;
        public String Name { get; set; } = "";
    }
}