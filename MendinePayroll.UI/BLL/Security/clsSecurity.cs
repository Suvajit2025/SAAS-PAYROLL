using Common.Utility;
using System.Data;

namespace MendinePayroll.UI.BLL.Security
{
    public static class clsSecurity
    {
        public static DataSet RoleModuleMenu(long roleId,string module,long applicationId,string tenantId)
        {
            return clsDatabase.fnDataSet("PRC_Role_Module_Menu",roleId,module,applicationId,tenantId);
        }
    }
}