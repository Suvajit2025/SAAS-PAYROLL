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
    public class PayConfigController : ApiController
    {
        PayConfigCore groupCore = new PayConfigCore();
        [Route("api/PayConfig/GetAllPayconfig")]
        [HttpPost]
        public HttpResponseMessage GetAllPayconfigList([FromBody]PayConfigModel payConfigModel)
        {
            try
            {
                var list = groupCore.Payconfiglist();
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

        [Route("api/PayConfig/GetAllPayconfigbyid")]
        [HttpPost]
        public HttpResponseMessage GetAllPayconfigbyid([FromBody]PayConfigModel payConfigModel)
        {
            try
            {
                var list = groupCore.GetPayConfigById(payConfigModel);
                if (list != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
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

        [Route("api/PayConfig/GetAllPayconfigbyType")]
        [HttpPost]
        public HttpResponseMessage GetAllPayconfigbyType([FromBody]PayConfigModel payConfigModel)
        {
            try
            {
                var list = groupCore.GetPayConfigByType(payConfigModel);
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

        [Route("api/PayConfig/SavePayconfig")]
        [HttpPost]
        public HttpResponseMessage SavePayconfig([FromBody]PayConfigModel payConfigModel)
        {
            try
            {
                int SavePayConfigdata = groupCore.SavePayConfig(payConfigModel);
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
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }

        [Route("api/PayConfig/DeletePayConfig")]
        [HttpPost]
        public HttpResponseMessage DeletePayConfig([FromBody]PayConfigModel payConfigModel)
        {
            try
            {
                int SavePayConfigdata = groupCore.DeletePayconfig(payConfigModel);
                
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
