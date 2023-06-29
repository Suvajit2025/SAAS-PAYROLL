using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MendinePayroll.Core.Implementation;
using MendinePayroll.Models;

namespace MendinePayrollAPI.Controllers
{
    public class ConfigureSalaryComponentController : ApiController
    {
        ConfigureSalaryComponentCore configureSalaryComponentCore = new ConfigureSalaryComponentCore();
        [Route("api/ConfigureSalaryComponent/SaveConfigureSalarycomponent")]
        [HttpPost]
        public HttpResponseMessage SaveConfigureSalarycomponent([FromBody] ConfigureSalaryComponentModel configureSalaryComponentModel)
        {
            try
            {
                int saveconfiguresalary = configureSalaryComponentCore.SaveConfigureSalaryComponent(configureSalaryComponentModel);
                if(saveconfiguresalary!=0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Save Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Some Error Occured");
                }
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }


        [Route("api/ConfigureSalaryComponent/GetAllConfigureSalary")]
        [HttpPost]
        public HttpResponseMessage GetAllConfigureSalary()
        {
            try
            {
                var configuresalarylist = configureSalaryComponentCore.ConfigureSalarylist();
                if (configuresalarylist != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, configuresalarylist);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
                }
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }


        [Route("api/ConfigureSalaryComponent/GetAllConfigureSalaryComponentbyid")]
        [HttpPost]
        public HttpResponseMessage GetAllConfigureSalaryComponentbyid([FromBody]ConfigureSalaryComponentModel configureSalaryComponentModel)
         {
            try
            {
                var list = configureSalaryComponentCore.GetConfigureSalaryComponentById(configureSalaryComponentModel);
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

        [Route("api/ConfigureSalaryComponent/UpdateConfigureSalarycomponent")]
        [HttpPost]
        public HttpResponseMessage UpdateConfigureSalarycomponent([FromBody] ConfigureSalaryComponentModel configureSalaryComponentModel)
        {
            try
            {
                int saveconfiguresalary = configureSalaryComponentCore.UpdateConfigureSalaryComponent(configureSalaryComponentModel);
                if (saveconfiguresalary != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Save Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Some Error Occured");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/ConfigureSalaryComponent/SaveManualSalaryConfigure")]
        [HttpPost]
        public HttpResponseMessage SaveManualSalaryConfigure([FromBody] ManualSalaryConfigModel manualSalaryConfigModel)
        {
            try
            {
                int saveconfiguresalary = configureSalaryComponentCore.SaveManualSalaryConfigure(manualSalaryConfigModel);
                if (saveconfiguresalary != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Save Successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Some Error Occured");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/ConfigureSalaryComponent/GetManualConfigureSalary")]
        [HttpPost]
        public HttpResponseMessage GetManualConfigureSalary([FromBody] ManualSalaryConfigModel manualSalaryConfigModel)
        {
            try
            {
                var configuresalarylist = configureSalaryComponentCore.GetManualConfigureSalaryComponentById(manualSalaryConfigModel);
                if (configuresalarylist != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, configuresalarylist);
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
