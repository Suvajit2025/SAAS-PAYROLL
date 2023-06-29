using MendinePayroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Core.Interface
{
    public interface IPayGroupEmployeeMappingCore
    {
        List<PayGroupEmployeeMappingModel> PayGroupEmployeeMappingList();
        List<PayGroupEmployeeMappingModel> GetPayGropById(int Id);
        int SavePayGroupEmployeeMapping(PayGroupEmployeeMappingModel payGroupEmployeeMappingModel);
        int DeletePayGroupEmployeeMapping(int Id);
    }
}
