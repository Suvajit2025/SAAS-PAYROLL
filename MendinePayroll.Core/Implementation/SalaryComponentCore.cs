using MendinePayroll.Data;
using MendinePayroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Core.Implementation
{
   public class SalaryComponentCore
    {
        esspEntities esspEntities = new esspEntities();

        SalaryComponentModel model = new SalaryComponentModel();

        public List<SalaryComponentModel> SalaryComponentList()
        {

            List<SalaryComponentModel> listModel = new List<SalaryComponentModel>();

            try
            {
                List<SalaryComponent_GetAll_Result> SalaryComponentList = esspEntities.SalaryComponent_GetAll().ToList();

                listModel = SalaryComponentList.Select(x =>
                {
                    return new SalaryComponentModel()
                    {
                        SalaryComponentId = x.SalaryComponentID,
                        SalaryComponent = x.SalaryComponent,
                        SalaryComponentCode = x.SalaryComponentCode,
                        SalaryComponentTypeID = x.SalaryComponentTypeID,
                        PayGroupsID = x.PayGroupID,
                        Description = x.Description,
                        PayGroupName=x.PayGroupName,
                        SalaryComponentType=x.SalaryComponentType
                    };
                }).ToList();

            }
            catch (Exception ex)
            {

            }

            return listModel;
        }

        public List<SalaryComponentModel> GetSalaryComponentById(int Id)
        {
            List<SalaryComponentModel> listModel = new List<SalaryComponentModel>();

            try
            {
                List<SalaryComponent_GetById_Result> listById = esspEntities.SalaryComponent_GetById(Id).ToList();

                listModel = listById.Select(x =>
                {
                    return new SalaryComponentModel()
                    {
                        SalaryComponentId = x.SalaryComponentID,
                        SalaryComponent = x.SalaryComponent,
                        SalaryComponentCode = x.SalaryComponentCode,
                        SalaryComponentTypeID = x.SalaryComponentTypeID,
                        PayGroupsID=x.PayGroupID,
                        Description=x.Description,
                        PayGroupName = x.PayGroupName,
                        SalaryComponentType = x.SalaryComponentType
                    };
                }).ToList();
            }
            catch (Exception ex)
            {

            }

            return listModel;
        }

        public int SaveSalaryComponent(SalaryComponentModel salaryComponentModel)
        {

            try
            {
                int saveSalaryComponent = esspEntities.SalaryComponent_Save(salaryComponentModel.SalaryComponentId, salaryComponentModel.SalaryComponent, salaryComponentModel.SalaryComponentCode, salaryComponentModel.SalaryComponentTypeID, salaryComponentModel.PayGroupsID, salaryComponentModel.Description);

                if (saveSalaryComponent != 0)
                {
                    return saveSalaryComponent;
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

        public int DeleteSalaryComponent(int Id)
        {
            try
            {
                var deleteSalaryComponent = esspEntities.SalaryComponent_Delete(Id);

                if (deleteSalaryComponent != 0)
                {
                    return deleteSalaryComponent;
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
