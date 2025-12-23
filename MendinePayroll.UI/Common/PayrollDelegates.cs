using MendinePayroll.UI.Models;

namespace MendinePayroll.UI.Common
{
    public delegate decimal ComponentValueResolver(
        int employeeId,
        int payConfigId,
        int month,
        int year,
        SalaryComponentResult component
    );
}
