
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MendinePayroll.Core;
using MendinePayroll.Core.Interface;
using MendinePayroll.Models;

namespace MendinePayrollAPI.Controllers
{
   
    public class PayGroupController : ApiController
    {
       PayGroupCore groupCore = new PayGroupCore();
        [Route("api/PayGroup/GetAllPayGroupList")]
        [HttpPost]
        public HttpResponseMessage GetAllPayGroupList([FromBody]PayGroupModel payGroup)
        {
          // List<PayGroupModel> list = new List<PayGroupModel>();

            try
            {            
              var list   = groupCore.PayGroupList(payGroup.TenantID);

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

        [Route("api/PayGroup/PayGroupListById")]
        [HttpPost]
        public HttpResponseMessage PayGroupListById([FromBody]PayGroupModel payGroup)
        {
            try
            {
                    var q = groupCore.GetPayGropById(payGroup);

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

        [Route("api/PayGoup/SavePayGroup")]
        [HttpPost]
        public HttpResponseMessage SavePayGroup([FromBody]PayGroupModel payGroup)
        {
            
            try
            {
                int savePayGroup = groupCore.SavePayGroup(payGroup);


                string sMessage = "";
                if (savePayGroup != 0)
                {
                    if(savePayGroup==1)
                    {
                        sMessage = "saved successfully";
                    }
                    else if(savePayGroup == 2)
                    {
                        sMessage = "PayGroup Already taken , please try with different name";
                    }
                    else if (savePayGroup == 3)
                    {
                        sMessage = "PayGroup Master Code Already taken , please try with different Code";
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

        [Route("api/PayGoup/SavePayGroupBonus")]
        [HttpPost]
        public HttpResponseMessage SavePayGroupBonus([FromBody]PayGroupBonusModel payGroupBonusModel)
        {

            try
            {
                int savePayGroup = groupCore.SavePayGroupBonus(payGroupBonusModel);


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
                // WriteToEventLog("PartnerLink", "NBV", (((ex.InnerException == null) ? "" : ex.InnerException.ToString()) + ((ex.Message == null) ? "" : ex.Message.ToString())), EventLogEntryType.Error);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }

        }

        [Route("api/PayGroup/GetAllPayGroupBonusById")]
        [HttpPost]
        public HttpResponseMessage GetAllPayGroupBonusById([FromBody]PayGroupBonusModel payGroupBonusModel)
        {
            try
            {
                var q = groupCore.GetAllPayGropBonusById(payGroupBonusModel);

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

        [Route("api/PayGroup/GetPayGroupBonusById")]
        [HttpPost]
        public HttpResponseMessage GetPayGroupBonusById([FromBody]PayGroupBonusModel payGroupBonusModel)
        {
            try
            {
                var q = groupCore.GetPayGropBonusById(payGroupBonusModel);

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

        [Route("api/PayGroup/GetPayGropBonusByMonth")]
        [HttpPost]
        public HttpResponseMessage GetPayGropBonusByMonth([FromBody]PayGroupBonusModel payGroupBonusModel)
        {
            try
            {
                var q = groupCore.GetPayGropBonusByMonth(payGroupBonusModel);

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

        [Route("api/PayGoup/DetetePayGroup")]
        [HttpPost]
        public HttpResponseMessage DeletePayGroup([FromBody]PayGroupModel payGroup)
        {
            try
            {
                var deletePayGroup = groupCore.DeletePayGroup(payGroup);

                if (deletePayGroup != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,"Delete Successfull");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,"PayGroup Does not exist");
                }

            }
            catch (Exception ex)
            {

                //  APILogError("Company", "Error", ex);
                //WriteToEventLog("PartnerLink", "NBV", (((ex.InnerException == null) ? "" : ex.InnerException.ToString()) + ((ex.Message == null) ? "" : ex.Message.ToString())), EventLogEntryType.Error);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }
        [Route("api/PayGroup/GetAllEmployeeByDesignation")]
        [HttpPost]
        public HttpResponseMessage GetAllEmployeeByDesignation([FromBody]EmpbasicModel empbasicModel)
        {
            try
            {
                var q = groupCore.GetEmployeeByDesignationId(empbasicModel);
                if (q != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, q);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "No Data found");
                }
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "ID not found");
            }
        }


    }
}
