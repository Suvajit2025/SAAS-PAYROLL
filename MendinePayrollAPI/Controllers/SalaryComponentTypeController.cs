using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MendinePayroll.Core.Implementation;
using MendinePayroll.Models;



namespace MendinePayrollAPI.Controllers
{
    public class SalaryComponentTypeTypeController : ApiController
    {
        SalaryComponentTypeTypeCore salaryComponentTypeTypeCore = new SalaryComponentTypeTypeCore();

        [Route("api/SalaryComponent/GetAllSalaryComponentType")]
        [HttpPost]
        public HttpResponseMessage GetAllSalaryComponentType()
        {
            try
            {
               // esspEntities esspEntities = new esspEntities();

                var list = salaryComponentTypeTypeCore.SalaryComponentTypeList().ToList();

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

        [Route("api/SalaryComponent/SalaryComponentTypeListById")]
        [HttpPost]
        public HttpResponseMessage GetSalaryComponentTypeById(SalaryComponentTypeModel salaryComponentTypeModel)
        {
            try
            {
              
                    var q = salaryComponentTypeTypeCore.GetSalaryComponentTypeById(salaryComponentTypeModel.Id);

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

        [Route("api/PayGoup/SaveSalaryComponentType")]
        [HttpPost]
        public HttpResponseMessage SaveSalaryComponentType([FromBody]SalaryComponentTypeModel salaryComponentTypes)
        {
           // esspEntities esspEntities = new esspEntities();
            try
            {
                int saveSalaryComponent = salaryComponentTypeTypeCore.SaveSalaryComponentType(salaryComponentTypes);

                if (saveSalaryComponent != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,"Save Succesfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                // WriteToEventLog("PartnerLink", "NBV", (((ex.InnerException == null) ? "" : ex.InnerException.ToString()) + ((ex.Message == null) ? "" : ex.Message.ToString())), EventLogEntryType.Error);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }

        }

        [Route("api/PayGoup/DeteteSalaryComponentType")]
        [HttpPost]
        public HttpResponseMessage DeleteSalaryComponentType(SalaryComponentTypeModel salaryComponentTypeModel)
        {
            try
            {
                //esspEntities mendineDBEntity = new esspEntities();

                var deleteSalaryComponent = salaryComponentTypeTypeCore.DeleteSalaryComponentType(salaryComponentTypeModel.Id);
                if (deleteSalaryComponent != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,"Delete Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "This record does not exist");
                }

            }
            catch (Exception ex)
            {

                //  APILogError("Company", "Error", ex);
                //WriteToEventLog("PartnerLink", "NBV", (((ex.InnerException == null) ? "" : ex.InnerException.ToString()) + ((ex.Message == null) ? "" : ex.Message.ToString())), EventLogEntryType.Error);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }
        public class SalaryComponentTypes
        {
            public int Id { get; set; }
            public string SalaryComponentType { get; set; }
        }
    }
}