using MendinePayroll.Data;
using MendinePayroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Core.Implementation
{
   public class PayGroupEmployeeMappingCore
    {
        esspEntities esspEntities = new esspEntities();

        PayGroupEmployeeMappingModel model = new PayGroupEmployeeMappingModel();

        public List<PayGroupEmployeeMappingModel> PayGroupEmployeeMappingList(int PaygroupId)
        {

            List<PayGroupEmployeeMappingModel> listModel = new List<PayGroupEmployeeMappingModel>();

            try
            {
                List<PayGroupEmployeeMapping_GetAll_Result> PayGroupEmployeeMappingList = esspEntities.PayGroupEmployeeMapping_GetAll(PaygroupId).ToList();

                listModel = PayGroupEmployeeMappingList.Select(x =>
                {
                    return new PayGroupEmployeeMappingModel()
                    {
                       PayGroupID=x.PayGroupID,
                       EmployeeID=x.EmployeeID,
                       PayGroupName=x.PayGroupName,
                       Description=x.Description,
                       PayGroupMasterCode=x.PayGroupMasterCode,
                       ConcernHRPersonnel=x.ConcernHRPersonnel

                    };
                }).ToList();

            }
            catch (Exception ex)
            {

            }

            return listModel;
        }

        

        public int SavePayGroupEmployeeMapping(PayGroupEmployeeMappingModel payGroupEmployeeMappingModel)
        {

            try
            {
                List<PayGroupEmployeeMapping_Save_Result> savePayGroupEmployeeMapping = esspEntities.PayGroupEmployeeMapping_Save(payGroupEmployeeMappingModel.PayGroupID,payGroupEmployeeMappingModel.EmployeeID).ToList();

                if (savePayGroupEmployeeMapping != null)
                {
                    return Convert.ToInt32(savePayGroupEmployeeMapping[0].ReturnStatus);
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

        public int DeletePayGroupEmployeeMapping(int Id,int Employeeid)
        {
            try
            {
                var deletePayGroupEmployeeMapping = esspEntities.PayGroupEmployeeMapping_Delete(Id, Employeeid);

                if (deletePayGroupEmployeeMapping != 0)
                {
                    return deletePayGroupEmployeeMapping;
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
        //public PayGroupEmployeeMappingModel Result(PayGroupEmployeeMapping_GetAll_Result list)
        //{
        //    model = new PayGroupEmployeeMappingModel()
        //    {
        //        PayGroupEmployeeMappingID = list.PayGroupEmployeeMappingID,
        //        PayGroupEmployeeMappingName = list.PayGroupEmployeeMappingName,
        //        PayGroupEmployeeMappingMasterCode = list.PayGroupEmployeeMappingMasterCode,
        //        ConcernHRPersonnel = list.ConcernHRPersonnel
        //    };

        //    return model;
        //}

        //public PayGroupEmployeeMappingModel ResultById(List<PayGroupEmployeeMapping_GetById_Result> list)
        //{
        //    model = new PayGroupEmployeeMappingModel()
        //    {
        //        PayGroupEmployeeMappingID = list[0].PayGroupEmployeeMappingID,
        //        PayGroupEmployeeMappingName = list[0].PayGroupEmployeeMappingName,
        //        PayGroupEmployeeMappingMasterCode = list[0].PayGroupEmployeeMappingMasterCode,
        //        ConcernHRPersonnel = list[0].ConcernHRPersonnel
        //    };

        //    return model;
        //}
    }
}
