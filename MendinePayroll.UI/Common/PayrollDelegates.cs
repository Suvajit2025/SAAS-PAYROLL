using MendinePayroll.UI.Models;

namespace MendinePayroll.UI.Common
{
    public delegate decimal ComponentValueResolver(
        int employeeId,
        int payGroupId,
        string payGroupName,
        int payConfigId,
        int month,
        int year,
        SalaryComponentResult component
    );
}
