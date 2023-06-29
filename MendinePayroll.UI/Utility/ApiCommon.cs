using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;

namespace MendinePayroll.UI.Utility
{
    public class ApiCommon
    {
        string ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];

        public HttpResponseMessage CallAPI(string ApiMethod, string contents)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            HttpResponseMessage response;

            using (var client = new System.Net.Http.HttpClient())
            {

                client.BaseAddress = new Uri(ApiUrl);
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "Y2JlOmNiZQ==");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                response = client.PostAsync(ApiMethod, new StringContent(contents, Encoding.UTF8, "application/json")).Result;

            }


            return response;
        }
    }
}