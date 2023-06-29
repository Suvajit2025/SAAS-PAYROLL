using MendinePayroll.Core.Implementation;
using MendinePayroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;




namespace MendinePayrollAPI.Controllers
{
    public class SalaryComponentController : ApiController
    {
        SalaryComponentCore salaryComponentCore = new SalaryComponentCore();

        [Route("api/SalaryComponent/GetAllSalaryComponent")]
        [HttpPost]
        public HttpResponseMessage GetAllSalaryComponent()
        {
            try
            {
               

                var list = salaryComponentCore.SalaryComponentList().ToList();

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

        [Route("api/SalaryComponent/SalaryComponentListById")]
        [HttpPost]
        public HttpResponseMessage GetSalaryComponentById(SalaryComponentModel salaryComponentModel)
        {
            try
            {
                
                    var q = salaryComponentCore.GetSalaryComponentById(salaryComponentModel.SalaryComponentId);

                    if (q != null)
                    {
                        // APILogActivity("Survey", "Success", "Get User List");
                        return Request.CreateResponse(HttpStatusCode.OK, q);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "No Data found");
                    }
                
            }
            catch (Exception ex)
            {
                //WriteToEventLog("PartnerLink", "NBV", (((ex.InnerException == null) ? "" : ex.InnerException.ToString()) + ((ex.Message == null) ? "" : ex.Message.ToString())), EventLogEntryType.Error);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "ID not found");
            }
        }

        [Route("api/PayGoup/SaveSalaryComponent")]
        [HttpPost]
        public HttpResponseMessage SaveSalaryComponent([FromBody]SalaryComponentModel salaryComponents)
        {
           // esspEntities esspEntities = new esspEntities();
            try
            {
                int saveSalaryComponent = salaryComponentCore.SaveSalaryComponent(salaryComponents);

                if (saveSalaryComponent != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,"Save Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound,"Some Error Occured");
                }
            }
            catch (Exception ex)
            {
                // WriteToEventLog("PartnerLink", "NBV", (((ex.InnerException == null) ? "" : ex.InnerException.ToString()) + ((ex.Message == null) ? "" : ex.Message.ToString())), EventLogEntryType.Error);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }

        }

        [Route("api/PayGoup/DeteteSalaryComponent")]
        [HttpPost]
        public HttpResponseMessage DeleteSalaryComponent(SalaryComponentModel salaryComponentModel)
        {
            try
            {
               // esspEntities mendineDBEntity = new esspEntities();

                var deleteSalaryComponent = salaryComponentCore.DeleteSalaryComponent(salaryComponentModel.SalaryComponentId);
                if (deleteSalaryComponent != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,"Delete Successfull");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,"This record does not exist ");
                }

            }
            catch (Exception ex)
            {

                //  APILogError("Company", "Error", ex);
                //WriteToEventLog("PartnerLink", "NBV", (((ex.InnerException == null) ? "" : ex.InnerException.ToString()) + ((ex.Message == null) ? "" : ex.Message.ToString())), EventLogEntryType.Error);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }
        public class SalaryComponents
        {
            public int Id { get; set; }
            // public int SalaryComponentEmployeeID { get; set; }
            public string SalaryComponent { get; set; }
            public int SalaryComponentCode { get; set; }

            public int SalaryComponentTypeID { get; set; }

            public int SalaryComponentsID { get; set; }

            public string Description { get; set; }

            
        }
    }
}