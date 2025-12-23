using MendinePayroll.Data;
using MendinePayroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Core.Interface
{
    public interface IPayGroupCore
    {
        List<PayGroupModel> PayGroupList(string tenantID);
        List<PayGroupModel> GetPayGropById(PayGroupModel payGroupModel);
        int SavePayGroup(PayGroupModel payGroupModel);
        int DeletePayGroup(PayGroupModel payGroupModel);
        List<EmpbasicModel> GetEmployeeByDesignationId(EmpbasicModel empbasicModel);
    }
}
