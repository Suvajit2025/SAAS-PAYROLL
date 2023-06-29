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
    public class SalaryConfigureCore:ISalaryConfigureCore
    {
        esspEntities esspEntities = new esspEntities();
        SalaryConfigureModel salaryConfigureModel = new SalaryConfigureModel();
        public int SavesalaryConfigure(SalaryConfigureModel salaryConfigureModel)
        {
            try
            {
                List<SalaryConfigure_Save_Result> SavesalaryConfig = esspEntities.SalaryConfigure_Save(salaryConfigureModel.SalaryConfigureID, salaryConfigureModel.SalaryConfigureName,salaryConfigureModel.PayGroupID, salaryConfigureModel.SalaryConfigureType).ToList();
                if (SavesalaryConfig != null)
                {
                    int Savesalaryconfig = Convert.ToInt32(SavesalaryConfig[0].ReturnStatus);
                    //int savepaygroup = Convert.ToInt32(SavePayGroup.ToString());
                    return Savesalaryconfig;
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

        public List<SalaryConfigureModel> Salaryconfiglist()
        {
            List<SalaryConfigureModel> listmodel = new List<SalaryConfigureModel>();
            try
            {
                List<SalaryConfigure_GetAll_Result> payconfiglist = esspEntities.SalaryConfigure_GetAll().ToList();
                listmodel = payconfiglist.Select(X =>
                {
                    return new SalaryConfigureModel
                    {
                        SalaryConfigureID = X.SalaryConfigureID,
                        SalaryConfigureName=X.SalaryConfigureName,
                        PayGroupID = X.PayGroupID,PayGroupName=X.PayGroupName
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;
        }

        public List<SalaryConfigureModel> GetSalaryConfigById(SalaryConfigureModel salaryConfigureModel)
        {
            List<SalaryConfigureModel> payconfiglist = new List<SalaryConfigureModel>();
            try
            {
                List<SalaryConfigure_GetById_Result> listmodel = esspEntities.SalaryConfigure_GetById(salaryConfigureModel.SalaryConfigureID).ToList();
                payconfiglist = listmodel.Select(X =>
                {
                    return new SalaryConfigureModel
                    {
                        SalaryConfigureID = X.SalaryConfigureID,
                        SalaryConfigureName = X.SalaryConfigureName,
                        PayGroupID=X.PayGroupID,
                        SalaryConfigureType=X.SalaryConfigureType
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return payconfiglist;

        }

        public List<SalaryConfigureModel> CheckDuplicatePaygroup(SalaryConfigureModel salaryConfigureModel)
        {
            List<SalaryConfigureModel> payconfiglist = new List<SalaryConfigureModel>();
            try
            {
                List<SalaryConfigure_PaygroupDuplicatecheck_Result> listmodel = esspEntities.SalaryConfigure_PaygroupDuplicatecheck(salaryConfigureModel.PayGroupID).ToList();
                payconfiglist = listmodel.Select(X =>
                {
                    return new SalaryConfigureModel
                    {
                        SalaryConfigureID = X.SalaryConfigureID,
                        SalaryConfigureName = X.SalaryConfigureName,
                        PayGroupID = X.PayGroupID
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return payconfiglist;

        }

        public int DeleteSalaryConfigure(SalaryConfigureModel salaryConfigureModel)
        {
            try
            {
                int deletepayconfigdata = esspEntities.SalaryConfigure_Delete(salaryConfigureModel.SalaryConfigureID);
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
