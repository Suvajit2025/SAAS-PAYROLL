using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace MendinePayrollAPI
{
    public class ApplicationAuthenticationHandler:AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                creratelog();
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            else
            {
                string authenticationtoken = actionContext.Request.Headers.Authorization.Parameter;
                string decodeAuthenticationtoken = Encoding.UTF8.GetString(Convert.FromBase64String(authenticationtoken));
                string[] usaernamepassword = decodeAuthenticationtoken.Split(':');
                string username = usaernamepassword[0];
                string password = usaernamepassword[1];
                if (Authoriz(username, password))
                {
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(username), null);
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
        }

        public bool Authoriz(string Username, string Password)
        {
            bool Authoriz = false;
            string uname = System.Configuration.ConfigurationManager.AppSettings["APIKEY"];
            string pwd = System.Configuration.ConfigurationManager.AppSettings["APIPASSWORD"];
            if (Username == uname && Password == pwd)
            {
                Authoriz = true;
            }
            return Authoriz;
        }

        public int creratelog()
        {
            int terval = 0;
            string LogNumber = DateTime.Now.ToString("yyyyMMdd");
            string NEXTLINE = "\r\n";
            string name = Convert.ToString(DateTime.Now);
            string[] formats = { "dd/MM/yyyy" };

            // string fileLoc = @"c:\api\Logfiles_" + LogNumber + ".txt";
            string fileLoc = Convert.ToString(System.Web.HttpContext.Current.Server.MapPath("/log/" + "/Logfiles_" + LogNumber + ".txt"));
            FileStream fs = null;
            if (!File.Exists(fileLoc))
            {
                using (fs = File.Create(fileLoc))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write("--------------------" + DateTime.Now + "------------------------------" + NEXTLINE);
                        sw.Write("auathorrized");
                        sw.Write("bad request error");
                        sw.Write(NEXTLINE);
                        sw.Write("-------------------------------------------------------------------");
                    }
                }
            }
            else
            {

                string[] lines = File.ReadAllLines(fileLoc);
                using (StreamWriter sw = new StreamWriter(fileLoc))
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        sw.Write(lines[i]);
                        sw.Write(NEXTLINE);
                    }
                    sw.Write("--------------------" + DateTime.Now + "------------------------------" + NEXTLINE);
                    sw.Write("auathorrized");
                    sw.Write("bad request error");
                    sw.Write(NEXTLINE);
                    sw.Write("-------------------------------------------------------------------");
                }
            }
            return terval;
        }
    }
}