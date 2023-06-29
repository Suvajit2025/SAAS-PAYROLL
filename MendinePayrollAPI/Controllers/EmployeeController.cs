using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MendinePayroll.Core.Implementation;
using MendinePayroll.Data;
using MendinePayroll.Models;
namespace MendinePayrollAPI.Controllers
{
    public class EmployeeController : ApiController
    {
        EmployeeCore employeeCore = new EmployeeCore();
        [Route("api/Employee/GetAllEmployeeList")]
        [HttpPost]
        public HttpResponseMessage GetAllEmployeeList()
        {
            try
            {
                var Employeelist = employeeCore.Empllist();
                if (Employeelist != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Employeelist);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/GetAllEmployeeByPage")]
        [HttpPost]
        public HttpResponseMessage GetAllEmployeeByPage([FromBody] EmployeeListModel employeeListModel)
        {
            try
            {
                var Employeelist = employeeCore.GetEmplist(employeeListModel);
                if (Employeelist != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Employeelist);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/GetAllEmployeeByCount")]
        [HttpPost]
        public HttpResponseMessage GetAllEmployeeCount([FromBody] EmployeeListModel employeeListModel)
        {
            try
            {
                var Employeelist = employeeCore.GetEmplist_Count(employeeListModel);
                if (Employeelist != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Employeelist);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }


        [Route("api/Employee/AllEmployeeSalaryview_Count")]
        [HttpPost]
        public HttpResponseMessage AllEmployeeSalaryview_Count([FromBody] EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {
                var Employeelist = employeeCore.AllEmployeeSalaryview_Count(employeeSalaryModel);
                if (Employeelist != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Employeelist);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }




        [Route("api/Employee/GetAllEmployeeListbyid")]
        [HttpPost]
        public HttpResponseMessage GetAllEmployeeListbyid([FromBody] EmployeeListModel employeeListModel)
        {
            try
            {
                var list = employeeCore.GetEmplistById(employeeListModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/GetEmployeeLeave")]
        [HttpPost]
        public HttpResponseMessage GetEmployeeLeave([FromBody] EmployeeLeaveModel employeeLeaveModel)
        {
            try
            {
                var list = employeeCore.GetEmpleave(employeeLeaveModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/GetEmployeeDetailsbyid")]
        [HttpPost]
        public HttpResponseMessage GetEmployeeDetailsbyid([FromBody] EmpbasicModel empbasicModel)
        {
            try
            {
                var list = employeeCore.GEetEmployeeDetails(empbasicModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/GetEmployeeSalaryDetails")]
        [HttpPost]
        public HttpResponseMessage GetEmployeeSalaryDetails([FromBody] EmployeeSalaryConfigModel employeeSalaryConfigModel)
        {
            try
            {
                var list = employeeCore.GEetEmployeeSalaryDetails(employeeSalaryConfigModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/SaveEmployeeSalary")]
        [HttpPost]
        public HttpResponseMessage SaveEmployeeSalary([FromBody] EmployeeSalaryConfigModel employeeSalaryConfigModel)
        {
            try
            {
                int SavePayConfigdata = employeeCore.SaveEmpSalary(employeeSalaryConfigModel);
                int sMessage;
                if (SavePayConfigdata != 0)
                {
                    sMessage = SavePayConfigdata;
                }
                else
                {
                    sMessage = 0;
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/SaveEmployeeLoan")]
        [HttpPost]
        public HttpResponseMessage SaveEmployeeLoan([FromBody] EmployeeLoanModel employeeLoanModel)
        {
            try
            {
                int SavePayConfigdata = employeeCore.SaveEmpLoan(employeeLoanModel);
                string sMessage = "";
                if (SavePayConfigdata != 0)
                {
                    sMessage = "Saved Successfully";
                }
                else
                {
                    sMessage = "Some Error Occured";
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/GetEmployeeLoanDetails")]
        [HttpPost]
        public HttpResponseMessage GetEmployeeLoanDetails([FromBody] EmployeeLoanModel employeeLoanModel)
        {
            try
            {
                var list = employeeCore.GEetEmployeeLoanDetails(employeeLoanModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/SaveLoantransaction")]
        [HttpPost]
        public HttpResponseMessage SaveLoantransaction([FromBody] LoanTransactionModel loanTransactionModel)
        {
            try
            {
                int SavePayConfigdata = employeeCore.SaveLoanTransaction(loanTransactionModel);
                string sMessage = "";
                if (SavePayConfigdata != 0)
                {
                    sMessage = "Saved Successfully";
                }
                else
                {
                    sMessage = "Some Error Occured";
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/GetEmployeeLoanTDetails")]
        [HttpPost]
        public HttpResponseMessage GetEmployeeLoanTDetails([FromBody] LoanTransactionModel loanTransactionModel)
        {
            try
            {
                var list = employeeCore.GetEmployeeLoanTDetails(loanTransactionModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/SaveallempSalary")]
        [HttpPost]
        public HttpResponseMessage SaveallempSalary([FromBody] EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {
                int SavePayConfigdata = employeeCore.SaveallempSalary(employeeSalaryModel);
                string sMessage = "";
                if (SavePayConfigdata != 0)
                {
                    sMessage = "Saved Successfully";
                }
                else
                {
                    sMessage = "Some Error Occured";
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/EmployeeDuplicateSalarycheck")]
        [HttpPost]
        public HttpResponseMessage EmployeeDuplicateSalarycheck([FromBody] EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {
                var list = employeeCore.EmployeeDuplicateSalarycheck(employeeSalaryModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/EmployeeTotalinsertedinamonth")]
        [HttpPost]
        public HttpResponseMessage EmployeeTotalinsertedinamonth([FromBody] EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {
                int SavePayConfigdata = employeeCore.EmployeeTotalinsertedinamonth(employeeSalaryModel);
                string sMessage = "";
                if (SavePayConfigdata != 0)
                {
                    sMessage = SavePayConfigdata.ToString();
                }
                else
                {
                    sMessage = SavePayConfigdata.ToString();
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/AllEmployeeSalaryview")]
        [HttpPost]
        public HttpResponseMessage AllEmployeeSalaryview([FromBody] EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {
                var list = employeeCore.AllEmployeeSalaryview(employeeSalaryModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/EmployeeSalaryview_ByPageWise")]
        [HttpPost]
        public HttpResponseMessage EmployeeSalaryview_ByPageWise([FromBody] EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {
                var list = employeeCore.EmployeeSalaryview_ByPageWise(employeeSalaryModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/GetEmployeeSalaryDetailsforexcel")]
        [HttpPost]
        public HttpResponseMessage GetEmployeeSalaryDetailsforexcel([FromBody] ExportExcelModel exportExcelModel)
        {
            try
            {
                var list = employeeCore.EmployeeSalaryforexcel(exportExcelModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/Employeelogin")]
        [HttpPost]
        public HttpResponseMessage Employeelogin([FromBody] EmpbasicModel empbasicModel)
        {
            try
            {
                var list = employeeCore.Employeelogin(empbasicModel);
                if (list != null)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, list);


                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }


        [Route("api/Employee/SaveEmployeeManualSalary")]
        [HttpPost]
        public HttpResponseMessage SaveEmployeeManualSalary([FromBody] EmpoloyeeSalaryConfigManualValueModel empoloyeeSalaryConfigManualValueModel)
        {
            try
            {
                int SavePayConfigdata = employeeCore.SaveEmployeeManualSalary(empoloyeeSalaryConfigManualValueModel);
                string sMessage = "";
                if (SavePayConfigdata != 0)
                {
                    sMessage = "Saved Successfully";
                }
                else
                {
                    sMessage = "Some Error Occured";
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }



        [Route("api/Employee/GetAllEmployeeInActiveList")]
        [HttpPost]
        public HttpResponseMessage GetAllEmployeeInActiveList()
        {
            try
            {
                var Employeelist = employeeCore.EmpInActivelist();
                if (Employeelist != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Employeelist);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }


        [Route("api/Employee/GetIndividualEmployeesalary")]
        [HttpPost]
        public HttpResponseMessage GetIndividualEmployeesalary([FromBody] IndividualSalryModel individualSalryModel)
        {
            try
            {
                var list = employeeCore.ViewIndividualSalary(individualSalryModel);
                
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }


        [Route("api/Employee/DeleteAllEmployeeSalaryByMonth")]
        [HttpPost]
        public HttpResponseMessage DeleteAllEmployeeSalaryByMonth([FromBody] EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {
                int DeleteSalaryData = employeeCore.DeleteAllEmployeeSalaryByMonth(employeeSalaryModel);
                int sMessage;
                if (DeleteSalaryData != 0)
                {
                    sMessage = DeleteSalaryData;
                }
                else
                {
                    sMessage = 0;
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/SaveEmployeeBonus")]
        [HttpPost]
        public HttpResponseMessage SaveEmployeeBonus([FromBody] EmployeeBonusModel employeeBonusModel)
        {
            try
            {
                int savePayGroup = employeeCore.SaveEmployeeBonus(employeeBonusModel);
                string sMessage = "";
                if (savePayGroup != 0)
                {
                    if (savePayGroup == 1)
                    {
                        sMessage = "saved successfully";
                    }
                    else if (savePayGroup == 2)
                    {
                        sMessage = "This month data of the year is alreay exist";
                    }
                    else
                    {
                        sMessage = "some error occured try again";
                    }
                }
                else
                {
                    sMessage = "some error occured try again";
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }



        [Route("api/Employee/GetEmployeeBonus")]
        [HttpPost]
        public HttpResponseMessage GetEmployeeBonus([FromBody] EmployeeBonusModel employeeBonusModel)
        {
            try
            {
                List<EmployeeBonusModel> list = employeeCore.GetAllEmployeeOvertime(employeeBonusModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }


        [Route("api/Employee/GetEmployeeBonusByMonth")]
        [HttpPost]
        public HttpResponseMessage GetEmployeeBonusByMonth([FromBody] EmployeeBonusModel employeeBonusModel)
        {
            try
            {
                List<EmployeeBonusModel> list = employeeCore.GetAllEmployeeOvertimeByMonth(employeeBonusModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/UpdateSalaryPayment")]
        [HttpPost]
        public HttpResponseMessage UpdateSalaryPayment([FromBody] EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {
                int UpdatedSalaryData = employeeCore.UpdateEmployeeSalryPaid(employeeSalaryModel);
                int sMessage;
                if (UpdatedSalaryData != 0)
                {
                    sMessage = UpdatedSalaryData;
                }
                else
                {
                    sMessage = 0;
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/SaveLeavedetails")]
        [HttpPost]
        public HttpResponseMessage SaveEmployeeLeaveDetails([FromBody] LeaveDetailsModel leaveDetailsModel)
        {
            try
            {
                int SaveLeaveData = employeeCore.SaveLeavedetails(leaveDetailsModel);
                int sMessage;
                if (SaveLeaveData != 0)
                {
                    sMessage = SaveLeaveData;
                }
                else
                {
                    sMessage = 0;
                }
                return Request.CreateResponse(HttpStatusCode.OK, sMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/Employee/GetEditedLeaveDetails")]
        [HttpPost]
        public HttpResponseMessage GetEditedLeaveDetails([FromBody] LeaveDetailsModel leaveDetailsModel)
        {
            try
            {
                List<LeaveDetailsModel> list = employeeCore.GetLeaveDetails(leaveDetailsModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }


        [Route("api/Employee/GETTDSCHECK")]
        [HttpPost]
        public HttpResponseMessage CheckTDS()
        {
            try
            {
                List<EmployeeSalaryConfigModel> response = employeeCore.CheckTDSEligiblePaygroup();
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

    }
}
