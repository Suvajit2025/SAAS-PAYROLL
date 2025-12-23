using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MendinePayroll.Data;
using MendinePayroll.Models;

namespace MendinePayroll.Core.Interface
{
    public interface ISalaryConfigureCore
    {
        List<SalaryConfigureModel> Salaryconfiglist(string tenantID);
        List<SalaryConfigureModel> GetSalaryConfigById(SalaryConfigureModel salaryConfigureModel);
        int SavesalaryConfigure(SalaryConfigureModel salaryConfigureModel);
    }
}
