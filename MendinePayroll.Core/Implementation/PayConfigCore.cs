using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MendinePayroll.Models;
using MendinePayroll.Data;
using MendinePayroll.Core.Interface;

namespace MendinePayroll.Core.Implementation
{
    public class PayConfigCore:IPayConfigCore
    {
        esspEntities esspEntities = new esspEntities();
        PayConfigModel payConfigModel = new PayConfigModel();
        public  List<PayConfigModel> Payconfiglist()
        {
            List<PayConfigModel> listmodel = new List<PayConfigModel>();
            try
            {
                List<PayConfig_GetAll_Result> payconfiglist = esspEntities.PayConfig_GetAll().ToList();
                listmodel = payconfiglist.Select(X =>
                {
                    return new PayConfigModel
                    {
                        PayConfigId = X.PayConfigId,
                        PayConfigName = X.PayConfigName,
                        PayConfigType = X.PayConfigType,
                        IScalculative = X.IScalculative,
                        EntryType=X.EntryType
                    };
                }).ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }
        public List<PayConfigModel> GetPayConfigById(PayConfigModel payConfigModel)
        {
            List<PayConfigModel> payconfiglist = new List<PayConfigModel>();
            try
            {
                List<PayConfig_GetById_Result> listmodel = esspEntities.PayConfig_GetById(payConfigModel.PayConfigId).ToList();
                payconfiglist = listmodel.Select(X =>
                {
                    return new PayConfigModel
                    {
                        PayConfigId = X.PayConfigId,
                        PayConfigName = X.PayConfigName,
                        PayConfigType = X.PayConfigType,
                        IScalculative=X.IScalculative
                    };
                }).ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return payconfiglist;

        }
        public List<PayConfigModel> GetPayConfigByType(PayConfigModel payConfigModel)
        {
            List<PayConfigModel> PayList = new List<PayConfigModel>();
            try
            {
                List<PayConfig_GetByType_Result> listmodel = esspEntities.PayConfig_GetByType(payConfigModel.PayConfigType).ToList();
                PayList = listmodel.Select(X =>
                {
                    return new PayConfigModel
                    {
                        PayConfigId = X.PayConfigId,
                        PayConfigName = X.PayConfigName,
                        PayConfigType = X.PayConfigType,
                        IScalculative = X.IScalculative
                    };
                }).ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return PayList;

        }
        public int SavePayConfig(PayConfigModel payConfigModel)
        {
            try
            {
                int Savepayconfig = esspEntities.PayConfig_Save(payConfigModel.PayConfigId, payConfigModel.PayConfigName, payConfigModel.PayConfigType,payConfigModel.IScalculative);
                if(Savepayconfig!=0)
                {
                    return Savepayconfig;
                }
                else
                {
                    return 0;
                }
            }
            catch(Exception ex)
            {
                return 0;
            }
        }

        public int DeletePayconfig(PayConfigModel payConfigModel)
        {
            try
            {
                int deletepayconfigdata = esspEntities.PayConfig_Delete(payConfigModel.PayConfigId);
                if (deletepayconfigdata != 0)
                {
                    return deletepayconfigdata;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
