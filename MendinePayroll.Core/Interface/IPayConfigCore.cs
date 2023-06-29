using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MendinePayroll.Data;
using MendinePayroll.Models;

namespace MendinePayroll.Core.Interface
{
    public interface IPayConfigCore
    {
        List<PayConfigModel> Payconfiglist();
        List<PayConfigModel> GetPayConfigById(PayConfigModel payConfigModel);
        List<PayConfigModel> GetPayConfigByType(PayConfigModel payConfigModel);
        int SavePayConfig(PayConfigModel payConfigModel);
    }
}
