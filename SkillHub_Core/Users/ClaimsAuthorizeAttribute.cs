using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Users;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using LMS_Project.Services;

namespace LMSCore.Users
{
    public class ClaimsAuthorizeAttribute : TypeFilterAttribute
    {
        public ClaimsAuthorizeAttribute(params lmsEnum.RoleEnum[] allowRoles) : base(typeof(ClaimsAuthorizeFilter))
        {
            Arguments = new object[] { allowRoles };
        }
    }

    public class ClaimsAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly lmsEnum.RoleEnum[] _allowRoles;

        public ClaimsAuthorizeFilter(params lmsEnum.RoleEnum[] allowRoles)
        {
            _allowRoles = allowRoles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            try
            {
                StringValues token;
                if (!request.Headers.TryGetValue("token", out token))
                {
                    context.Result = new ContentResult
                    {
                        StatusCode = 401,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(new { message = "Hết hạn đăng nhập" })
                    };
                    return;
                }
                var controllerName = context.ActionDescriptor.RouteValues["controller"];
                var actionName = context.ActionDescriptor.RouteValues["action"];
                var principal = JWTManager.GetPrincipal(token);
                if (principal == null || !principal.Identity.IsAuthenticated)
                {
                    context.Result = new ContentResult
                    {
                        StatusCode = 401,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(new { message = "Hết hạn đăng nhập" })
                    };
                    return;
                }
                int roleId = int.Parse(principal.FindFirst("RoleId").Value);
                var allow = await Account.HasPermission(roleId, controllerName, actionName);
                if (!allow)
                {
                    context.Result = new ContentResult
                    {
                        StatusCode = 403,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(new { message = ApiMessage.UNAUTHORIZED })
                    };
                    return;
                }
                // Authentication succeeded
                context.HttpContext.User = principal;
            }
            catch
            {
                context.Result = new ContentResult
                {
                    StatusCode = 401,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(new { message = "Hết hạn đăng nhập" })
                };
                return;
            }
        }
    }
}