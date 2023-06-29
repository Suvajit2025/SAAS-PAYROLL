using MendinePayroll.Data;
using MendinePayroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Core.Implementation
{
   public class SalaryComponentTypeTypeCore
    {
        esspEntities esspEntities = new esspEntities();

        SalaryComponentTypeModel model = new SalaryComponentTypeModel();

        public List<SalaryComponentTypeModel> SalaryComponentTypeList()
        {

            List<SalaryComponentTypeModel> listModel = new List<SalaryComponentTypeModel>();

            try
            {
                List<SalaryComponentType_GetAll_Result> SalaryComponentTypeList = esspEntities.SalaryComponentType_GetAll().ToList();

                listModel = SalaryComponentTypeList.Select(x =>
                {
                    return new SalaryComponentTypeModel()
                    {
                        Id = x.SalaryComponentTypeID,
                        SalaryComponentType = x.SalaryComponentType
                    };
                }).ToList();

            }
            catch (Exception ex)
            {

            }

            return listModel;
        }

        public List<SalaryComponentTypeModel> GetSalaryComponentTypeById(int Id)
        {
            List<SalaryComponentTypeModel> listModel = new List<SalaryComponentTypeModel>();

            try
            {
                List<SalaryComponentType_GetById_Result> listById = esspEntities.SalaryComponentType_GetById(Id).ToList();

                listModel = listById.Select(x =>
                {
                    return new SalaryComponentTypeModel()
                    {
                        Id = x.SalaryComponentTypeID,
                        SalaryComponentType = x.SalaryComponentType,
                        
                    };
                }).ToList();
            }
            catch (Exception ex)
            {

            }

            return listModel;
        }

        public int SaveSalaryComponentType(SalaryComponentTypeModel SalaryComponentTypeModel)
        {

            try
            {
                int saveSalaryComponentType = esspEntities.SalaryComponentType_Save(SalaryComponentTypeModel.Id, SalaryComponentTypeModel.SalaryComponentType);

                if (saveSalaryComponentType != 0)
                {
                    return saveSalaryComponentType;
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

        public int DeleteSalaryComponentType(int Id)
        {
            try
            {
                var deleteSalaryComponentType = esspEntities.SalaryComponentType_Delete(Id);

                if (deleteSalaryComponentType != 0)
                {
                    return deleteSalaryComponentType;
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
