using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MendinePayroll.Models;
using MendinePayroll.Core.Implementation;

namespace MendinePayrollAPI.Controllers
{
    public class SalaryConfigureController : ApiController
    {
        SalaryConfigureCore groupCore = new SalaryConfigureCore();
        [Route("api/SalaryConfigure/GetAllSalaryConfigureList")]
        [HttpPost]
        public HttpResponseMessage GetAllSalaryConfigureList([FromBody]SalaryConfigureModel salaryConfigureModel)
        {
            try
            {
                var list = groupCore.Salaryconfiglist(salaryConfigureModel.TenantID);

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

        [Route("api/SalaryConfigure/GetAllSalaryConfigurebyid")]
        [HttpPost]
        public HttpResponseMessage GetAllSalaryConfigurebyid([FromBody]SalaryConfigureModel salaryConfigureModel)
        {
            try
            {
                var list = groupCore.GetSalaryConfigById(salaryConfigureModel);
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

        [Route("api/SalaryConfigure/SaveSalaryConfigure")]
        [HttpPost]
        public HttpResponseMessage SaveSalaryConfigure([FromBody]SalaryConfigureModel salaryConfigureModel)
        {
            try
            {
                int SavePayConfigdata = groupCore.SavesalaryConfigure(salaryConfigureModel);
                string sMessage = "";
                if (SavePayConfigdata != 0)
                {
                    sMessage = SavePayConfigdata.ToString();
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

        [Route("api/SalaryConfigure/DuplicatePaygroupCheck")]
        [HttpPost]
        public HttpResponseMessage DuplicatePaygroupCheck([FromBody]SalaryConfigureModel salaryConfigureModel)
        {
            try
            {
                var list = groupCore.CheckDuplicatePaygroup(salaryConfigureModel);
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

        [Route("api/SalaryConfigure/DeleteSalaryConfigure")]
        [HttpPost]
        public HttpResponseMessage DeletePayConfig([FromBody]SalaryConfigureModel salaryConfigureModel)
        {
            try
            {
                int SavePayConfigdata = groupCore.DeleteSalaryConfigure(salaryConfigureModel);

                if (SavePayConfigdata != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Delete Successfull");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "PayConfig Does not exist");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }
    }
}
