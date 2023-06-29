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
   public class ConfigureSalaryComponentCore: IConfigureSalaryComponentCore
    {
        esspEntities esspEntities = new esspEntities();
        ConfigureSalaryComponentModel model = new ConfigureSalaryComponentModel();
        public  int SaveConfigureSalaryComponent(ConfigureSalaryComponentModel configureSalaryComponentModel)
      {
            try
            {
                int SaveConfigureSalary = esspEntities.ConfigureSalaryComponent_Save(configureSalaryComponentModel.ConfigureSalaryID, configureSalaryComponentModel.PayConfigId, configureSalaryComponentModel.CalculationFormula, configureSalaryComponentModel.ManualRate, configureSalaryComponentModel.MasterPayConfigID, configureSalaryComponentModel.ISPercentage);
                if (SaveConfigureSalary != 0)
                {
                    return SaveConfigureSalary;
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

        public List<ConfigureSalaryComponentModel> ConfigureSalarylist()
        {
            List<ConfigureSalaryComponentModel> configureSalaryComponentlist = new List<ConfigureSalaryComponentModel>();
            try
            {
                List<ConfigureSalaryComponent_GetAll_Result> configureSalarylist = esspEntities.ConfigureSalaryComponent_GetAll().ToList();
                configureSalaryComponentlist = configureSalarylist.Select(X =>
                 {
                     return new ConfigureSalaryComponentModel()
                     {
                         ConfigureSalaryID=X.ConfigureSalaryID,
                         PayConfigId=X.PayConfigId,
                         CalculationFormula=X.CalculationFormula,
                         ManualRate=X.ManualRate,
                         MasterPayConfigID = X.MasterPayConfigID,
                         ISPercentage = X.ISPercentage
                         
                     };
                 }).ToList();
                
            }
            catch(Exception ex)
            {

            }
            return configureSalaryComponentlist;
        }




        public List<ConfigureSalaryComponentModel> GetConfigureSalaryComponentById(ConfigureSalaryComponentModel configureSalaryComponentModel)
        {
            List<ConfigureSalaryComponentModel> ConfigureSalaryComponentList = new List<ConfigureSalaryComponentModel>();
            try
            {
                List<ConfigureSalaryComponent_GetBYId_Result> listmodel = esspEntities.ConfigureSalaryComponent_GetBYId(configureSalaryComponentModel.ConfigureSalaryID).ToList();
                ConfigureSalaryComponentList = listmodel.Select(X =>
                {
                    return new ConfigureSalaryComponentModel
                    {
                        ConfigureSalaryID = X.ConfigureSalaryID,
                        PayConfigId = X.PayConfigId,
                        CalculationFormula = X.CalculationFormula,
                        ManualRate = X.ManualRate,
                        PayConfigType=X.PayConfigType,
                        MasterPayConfigID=X.MasterPayConfigID,
                        ISPercentage=X.ISPercentage,
                        PayConfigName=X.PayConfigName,
                        IScalculative=X.IScalculative
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ConfigureSalaryComponentList;

        }



        public int UpdateConfigureSalaryComponent(ConfigureSalaryComponentModel configureSalaryComponentModel)
        {
            try
            {
                int SaveConfigureSalary = esspEntities.ConfigureSalaryComponent_Update(configureSalaryComponentModel.ConfigureSalaryID, configureSalaryComponentModel.PayConfigId, configureSalaryComponentModel.CalculationFormula, configureSalaryComponentModel.ManualRate, configureSalaryComponentModel.MasterPayConfigID, configureSalaryComponentModel.ISPercentage);
                if (SaveConfigureSalary != 0)
                {
                    return SaveConfigureSalary;
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

        public int SaveManualSalaryConfigure(ManualSalaryConfigModel manualSalaryConfigModel)
        {
            try
            {
                int SaveConfigureSalary = esspEntities.ManualSalaryConfig_save(manualSalaryConfigModel.PayGroupID, manualSalaryConfigModel.ManualPayConfigId, manualSalaryConfigModel.ISActive);
                if (SaveConfigureSalary != 0)
                {
                    return SaveConfigureSalary;
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

        public List<ManualSalaryConfigModel> GetManualConfigureSalaryComponentById(ManualSalaryConfigModel manualSalaryConfigModel)
        {
            List<ManualSalaryConfigModel> ManualConfigureSalaryComponentList = new List<ManualSalaryConfigModel>();
            try
            {
                List<ManualSalaryConfig_GetByPayGroupID_Result> listmodel = esspEntities.ManualSalaryConfig_GetByPayGroupID(manualSalaryConfigModel.PayGroupID).ToList();
                ManualConfigureSalaryComponentList = listmodel.Select(X =>
                {
                    return new ManualSalaryConfigModel
                    {
                        ID= X.ID,
                        PayGroupID=X.PayGroupID,
                        ManualPayConfigId=X.ManualPayConfigId,
                        ISActive=X.ISActive,
                        PayConfigName=X.PayConfigName
                        
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ManualConfigureSalaryComponentList;

        }
    }
}
