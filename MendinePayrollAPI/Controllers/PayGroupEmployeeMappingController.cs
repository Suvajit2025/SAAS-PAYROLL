using MendinePayroll.Core.Implementation;
using MendinePayroll.Core.Interface;
using MendinePayroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;   



namespace MendinePayrollAPI.Controllers
{
    public class PayGroupEmployeeMappingController : ApiController
    {
        PayGroupEmployeeMappingCore payGroupEmployeeMapping = new PayGroupEmployeeMappingCore();

        [Route("api/PayGroup/GetAllPayGroupEmployeeMapping")]
        [HttpPost]
        public HttpResponseMessage GetAllPayGroupEmployeeMapping(PayGroupEmployeeMappingModel payGroupEmployeeMappingModel)
        {
            try
            {
                var list = payGroupEmployeeMapping.PayGroupEmployeeMappingList(payGroupEmployeeMappingModel.PayGroupID).ToList();

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

     

        [Route("api/PayGoup/SavePayGroupEmployeeMapping")]  
        [HttpPost]
        public HttpResponseMessage SavePayGroupEmployeeMapping([FromBody]PayGroupEmployeeMappingModel payGroupEmployeeMap)
        {
            
            try
            {
                int savePayGroupEmployee = payGroupEmployeeMapping.SavePayGroupEmployeeMapping(payGroupEmployeeMap);
                string sMessage = "";
                if (savePayGroupEmployee != 0)
                {
                    if(savePayGroupEmployee==1)
                    {
                        sMessage = "saved successfully";
                    }
                    if (savePayGroupEmployee == 2)
                    {
                        sMessage = "This Employeeid Already taken , please try with different Employeeid";
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
                // WriteToEventLog("PartnerLink", "NBV", (((ex.InnerException == null) ? "" : ex.InnerException.ToString()) + ((ex.Message == null) ? "" : ex.Message.ToString())), EventLogEntryType.Error);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }

        }

        [Route("api/PayGoup/DetetePayGroupEployeeMapping")]
        [HttpPost]
        public HttpResponseMessage DeletePayGroupEployeeMapping(PayGroupEmployeeMappingModel payGroupEmployeeMappingModel)
        {
            try
            {
                var deletePayGroup = payGroupEmployeeMapping.DeletePayGroupEmployeeMapping(payGroupEmployeeMappingModel.PayGroupID,payGroupEmployeeMappingModel.EmployeeID);
                if (deletePayGroup != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,"Delete Successful");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Employeeid of this paygroup does not exist");
                }

            }
            catch (Exception ex)
            {

                //  APILogError("Company", "Error", ex);
                //WriteToEventLog("PartnerLink", "NBV", (((ex.InnerException == null) ? "" : ex.InnerException.ToString()) + ((ex.Message == null) ? "" : ex.Message.ToString())), EventLogEntryType.Error);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }
       

        //Create MendinePayroll.Model class library Projec 
    }
}