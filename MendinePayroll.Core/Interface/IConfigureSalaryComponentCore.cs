using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MendinePayroll.Models;
using MendinePayroll.Data;

namespace MendinePayroll.Core.Interface
{
  public  interface IConfigureSalaryComponentCore
    {
        int SaveConfigureSalaryComponent(ConfigureSalaryComponentModel configureSalaryComponentModel);
        List<ConfigureSalaryComponentModel> ConfigureSalarylist();
        List<ConfigureSalaryComponentModel> GetConfigureSalaryComponentById(ConfigureSalaryComponentModel configureSalaryComponentModel);
    }
}
