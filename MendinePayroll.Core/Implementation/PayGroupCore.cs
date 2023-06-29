using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MendinePayroll.Models;
using MendinePayroll.Data;
using MendinePayroll.Core.Interface;

namespace MendinePayroll.Core
{
    public class PayGroupCore:IPayGroupCore
    {
        esspEntities esspEntities = new esspEntities();

        PayGroupModel model = new PayGroupModel();

        public List<PayGroupModel> PayGroupList()
        {

            List<PayGroupModel> listModel = new List<PayGroupModel>();

            try
            {
                List<PayGroup_GetAll_Result> payGroupList = esspEntities.PayGroup_GetAll().ToList();

                listModel = payGroupList.Select(x =>
                {
                    return new PayGroupModel()
                    {
                        PayGroupID = x.PayGroupID,
                        PayGroupName = x.PayGroupName,
                        PayGroupMasterCode = x.PayGroupMasterCode,
                        Description=x.Description,
                        ConcernHRPersonnel = x.ConcernHRPersonnel

                    };
                }).ToList();

            }
            catch (Exception ex)
            {
                
            }

            return listModel;
        }

        public List<PayGroupModel> GetPayGropById(PayGroupModel payGroupModel)
        {
            List<PayGroupModel> listModel = new List<PayGroupModel>();

            try
            {
                List<PayGroup_GetById_Result> listById = esspEntities.PayGroup_GetById(payGroupModel.PayGroupID).ToList();

                listModel = listById.Select(x =>
                {
                    return new PayGroupModel()
                    {
                        PayGroupID = x.PayGroupID,
                        PayGroupName = x.PayGroupName,
                        PayGroupMasterCode = x.PayGroupMasterCode,
                        Description=x.Description,
                        ConcernHRPersonnel = x.ConcernHRPersonnel,
                        EMPLOYEENAME=x.EMPLOYEENAME

                    };
                }).ToList();
            }
            catch (Exception ex)
            {
               
            }

            return listModel;
        }

        public int SavePayGroup(PayGroupModel payGroupModel)
        {
            
            try
            {
                
                List<PayGroup_Save_Result> SavePayGroup =esspEntities.PayGroup_Save(payGroupModel.PayGroupID, payGroupModel.PayGroupName, payGroupModel.Description, payGroupModel.PayGroupMasterCode, payGroupModel.ConcernHRPersonnel).ToList();
                
                if (SavePayGroup != null)
                {
                    int savepaygroup = Convert.ToInt32(SavePayGroup[0].ReturnStatus);
                    //int savepaygroup = Convert.ToInt32(SavePayGroup.ToString());
                    return savepaygroup;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public int SavePayGroupBonus(PayGroupBonusModel payGroupBonusModel)
        {

            try
            {

                List<PayGroupBonus_Save_Result> SavePayGroup = esspEntities.PayGroupBonus_Save(payGroupBonusModel.ID,payGroupBonusModel.PayGroupId, payGroupBonusModel.Month, payGroupBonusModel.Year, payGroupBonusModel.OverTimeHours, payGroupBonusModel.ProductionBonus, payGroupBonusModel.TargetAchieved).ToList();

                if (SavePayGroup != null)
                {
                    int savepaygroupbonus = Convert.ToInt32(SavePayGroup[0].ReturnStatus);
                    return savepaygroupbonus;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public int DeletePayGroup(PayGroupModel payGroupModel)
        {
            try
            {
                var deletePayGroup = esspEntities.PayGroup_Delete(payGroupModel.PayGroupID);

                if (deletePayGroup != 0)
                {
                    return deletePayGroup;
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

        public List<PayGroupBonusModel> GetAllPayGropBonusById(PayGroupBonusModel  payGroupBonusModel)
        {
            List<PayGroupBonusModel> listModel = new List<PayGroupBonusModel>();

            try
            {
                List<PayGroupBonus_GetAllByID_Result> listById = esspEntities.PayGroupBonus_GetAllByID(payGroupBonusModel.PayGroupId).ToList();

                listModel = listById.Select(x =>
                {
                    return new PayGroupBonusModel()
                    {
                        ID=x.ID,
                        PayGroupId = x.PayGroupId,
                        Month = x.Month,
                        Year = x.Year,
                        OverTimeHours = x.OverTimeHours,
                        ProductionBonus = x.ProductionBonus,
                        TargetAchieved=x.TargetAchieved
                        
                    };
                }).ToList();
            }
            catch (Exception ex)
            {

            }

            return listModel;
        }

        public List<PayGroupBonusModel> GetPayGropBonusById(PayGroupBonusModel payGroupBonusModel)
        {
            List<PayGroupBonusModel> listModel = new List<PayGroupBonusModel>();

            try
            {
                List<PayGroupBonus_GetById_Result> listById = esspEntities.PayGroupBonus_GetById(payGroupBonusModel.ID).ToList();

                listModel = listById.Select(x =>
                {
                    return new PayGroupBonusModel()
                    {
                        ID = x.ID,
                        PayGroupId = x.PayGroupId,
                        Month = x.Month,
                        Year = x.Year,
                        OverTimeHours = x.OverTimeHours,
                        ProductionBonus = x.ProductionBonus,
                        TargetAchieved = x.TargetAchieved

                    };
                }).ToList();
            }
            catch (Exception ex)
            {

            }

            return listModel;
        }

        public List<PayGroupBonusModel> GetPayGropBonusByMonth(PayGroupBonusModel payGroupBonusModel)
        {
            List<PayGroupBonusModel> listModel = new List<PayGroupBonusModel>();

            try
            {
                List<PayGroupBonus_GetByMonthandPaygroupId_Result> listById = esspEntities.PayGroupBonus_GetByMonthandPaygroupId(payGroupBonusModel.PayGroupId, payGroupBonusModel.Month, payGroupBonusModel.Year).ToList();

                listModel = listById.Select(x =>
                {
                    return new PayGroupBonusModel()
                    {
                        ID = x.ID,
                        PayGroupId = x.PayGroupId,
                        Month = x.Month,
                        Year = x.Year,
                        OverTimeHours = x.OverTimeHours,
                        ProductionBonus = x.ProductionBonus,
                        TargetAchieved = x.TargetAchieved

                    };
                }).ToList();
            }
            catch (Exception ex)
            {

            }

            return listModel;
        }

        public List<EmpbasicModel> GetEmployeeByDesignationId(EmpbasicModel empbasicModel)
        {
            List<EmpbasicModel> listModel = new List<EmpbasicModel>();

            try
            {
                List<Employee_GetByDesignationID_Result> listById = esspEntities.Employee_GetByDesignationID(empbasicModel.empdesignation).ToList();

                listModel = listById.Select(x =>
                {
                    return new EmpbasicModel()
                    {
                        empfirstname=x.empfirstname,
                        empmiddlename=x.empmiddlename,
                        emplastname=x.emplastname,
                        empid=x.empid,
                        empdesignation=x.empdesignation
                    };
                }).ToList();
            }
            catch (Exception ex)
            {

            }

            return listModel;
        }



        //public PayGroupModel Result(PayGroup_GetAll_Result list)
        //{
        //     model = new PayGroupModel()
        //    {
        //         PayGroupID = list.PayGroupID,
        //        PayGroupName = list.PayGroupName,
        //        PayGroupMasterCode = list.PayGroupMasterCode,
        //        ConcernHRPersonnel = list.ConcernHRPersonnel
        //    };

        //    return model;
        //}

        //public PayGroupModel ResultById(List<PayGroup_GetById_Result> list)
        //{
        //    model = new PayGroupModel()
        //    {
        //        PayGroupID = list[0].PayGroupID,
        //        PayGroupName = list[0].PayGroupName,
        //        PayGroupMasterCode = list[0].PayGroupMasterCode,
        //        ConcernHRPersonnel = list[0].ConcernHRPersonnel
        //    };

        //    return model;
        //}
    }
}
