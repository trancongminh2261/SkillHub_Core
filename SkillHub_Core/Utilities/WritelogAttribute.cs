using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using System.Net;
using System.Web.Http.ModelBinding;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using RestSharp;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LMSCore.Utilities
{
    public class WritelogAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var body = context.ActionArguments;
            context.HttpContext.Items["RequestBody"] = body;
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                StringValues token = "";
                context.HttpContext.Request.Headers.TryGetValue("token", out token);

                var response = context.Result.GetPropValue("Value") ?? "500";
                var statusCode = context.Result.GetPropValue("StatusCode") ?? "500";
                if (statusCode.ToString() == "200" || statusCode.ToString() == "204")
                    return;
                string userAgent = context.HttpContext.Request.Headers.UserAgent;
                context.HttpContext.Items.TryGetValue("RequestBody", out var bodyObj);

                var writelog = new MonaWritelogModel
                {
                    Token = token,
                    HttpCode = statusCode.ToString(),
                    Body = JsonConvert.SerializeObject(bodyObj),
                    CreateOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Domain = context.HttpContext.Request.Headers.Host,
                    Method = context.HttpContext.Request.Method,
                    EndPoint = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString,
                    Response = JsonConvert.SerializeObject(response),
                    UserAgent = userAgent,
                };

                var client = new RestClient("https://log.monamedia.net/api/writelog");
                var request = new RestRequest(Method.POST);
                request.RequestFormat = DataFormat.Json;

                request.AddJsonBody(new
                {
                    Items = new List<MonaWritelogModel> { writelog }
                });
                request.AddHeader("Key", "mona-write-log-from-daicaminh");
                request.AddHeader("Content-Type", "application/json");

                IRestResponse restResponse = client.Execute(request);
            }
            catch
            {
                return;
            }
        }
    }
}
