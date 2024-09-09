using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
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
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var message = context.ModelState.Values.FirstOrDefault(x => x.ValidationState == ModelValidationState.Invalid);
                if (message != null)
                {
                    context.Result = new BadRequestObjectResult(new { message = message.Errors.FirstOrDefault().ErrorMessage });
                }
            }
        }
    }
}
