using LMS_Project.Areas.ControllerAPIs;
using LMS_Project.Areas.Models;
using LMS_Project.Models;
using LMS_Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using LMSCore.Users;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.Filters;
using static LMS_Project.Services.UserInformation;
using LMS_Project.Areas.Request;
using static LMS_Project.Services.Account;
using System.IO;
using LMSCore.Areas.ControllerAPIs;
using static LMSCore.Models.lmsEnum;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using LMSCore.Models;
using Microsoft.Extensions.Configuration;

namespace LMS_Project.ControllerAPIs
{
    public class AccountController : BaseController
    {
        private lmsDbContext dbContext;
        private Account domainService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.dbContext = new lmsDbContext();
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            this.domainService = new Account(this.dbContext, _httpContextAccessor);
        }
        [HttpPost]
        [Route("api/Account/Login")]
        public async Task<IActionResult> Login()
        {
            string username = Request.Form["username"];
            string password = Request.Form["password"];
            TokenResult appDomainResult = await domainService.Login(username, password);
            if (appDomainResult.ResultCode != ((int)HttpStatusCode.OK))
                return StatusCode((int)(HttpStatusCode)appDomainResult.ResultCode, new
                {
                    message = appDomainResult.ResultMessage,
                });
            return StatusCode((int)(HttpStatusCode)appDomainResult.ResultCode, new
            {
                message = appDomainResult.ResultMessage,
                token = appDomainResult.GenerateTokenModel.Token,
                refreshToken = appDomainResult.GenerateTokenModel.RefreshToken,
                refreshTokenExpires = appDomainResult.GenerateTokenModel.RefreshTokenExpires,
            });
        }

        [HttpPost]
        [Route("api/Account/LoginTest")]
        public async Task<IActionResult> LoginTest(string username, string password)
        {
            TokenResult appDomainResult = await domainService.Login(username, password);
            if (appDomainResult.ResultCode != ((int)HttpStatusCode.OK))
                return StatusCode((int)(HttpStatusCode)appDomainResult.ResultCode, new
                {
                    message = appDomainResult.ResultMessage,
                });
            return StatusCode((int)(HttpStatusCode)appDomainResult.ResultCode, new
            {
                message = appDomainResult.ResultMessage,
                token = appDomainResult.GenerateTokenModel.Token,
                refreshToken = appDomainResult.GenerateTokenModel.RefreshToken,
                refreshTokenExpires = appDomainResult.GenerateTokenModel.RefreshTokenExpires,
            });
        }
        public class LoginDevModel
        { 
            public int Id { get; set; }
            public string PassDev { get; set; }
        }
        [HttpPost]
        [Route("api/LoginDev")]
        public async Task<IActionResult> LoginByDev([FromBody] LoginDevModel model)
        {

            TokenResult appDomainResult = await domainService.LoginByDev(model);
            if (appDomainResult.ResultCode != ((int)HttpStatusCode.OK))
                return StatusCode((int)(HttpStatusCode)appDomainResult.ResultCode, new
                {
                    message = appDomainResult.ResultMessage,
                });
            return StatusCode((int)(HttpStatusCode)appDomainResult.ResultCode, new
            {
                message = appDomainResult.ResultMessage,
                Token = appDomainResult.GenerateTokenModel.Token,
                refreshToken = appDomainResult.GenerateTokenModel.RefreshToken,
                refreshTokenExpires = appDomainResult.GenerateTokenModel.RefreshTokenExpires,
            });
        }

        [HttpPost]
        [Route("api/RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest itemModel)
        {
            TokenResult appDomainResult = await domainService.RefreshToken(itemModel);
            if (appDomainResult.ResultCode == ((int)HttpStatusCode.Unauthorized))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, new
                {
                    message = appDomainResult.ResultMessage,
                });
            }
            return StatusCode((int)(HttpStatusCode)appDomainResult.ResultCode, new
            {
                message = appDomainResult.ResultMessage,
                token = appDomainResult.GenerateTokenModel.Token,
                refreshToken = appDomainResult.GenerateTokenModel.RefreshToken,
                refreshTokenExpires = appDomainResult.GenerateTokenModel.RefreshTokenExpires,
            });
        }
        [HttpGet]
        [Route("api/GetAccount")]
        public async Task<IActionResult> GetAccount()
        {
            try
            {
                var data = await domainService.GetAccount();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công", data });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message + "||" + ex.InnerException });
            }
        }
        [HttpPost]
        [ClaimsAuthorize]
        [Route("api/ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await domainService.ChangePassword(model,GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        [HttpPost]
        [Route("api/Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if(await domainService.GetAllowRegister() == AllowRegister.UnAllow)
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Chức năng này đã tắt" });
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Register(model);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        /// <summary>
        /// Tạo token mới
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ClaimsAuthorize]
        [Route("api/NewToken")]
        public async Task<IActionResult> NewToken()
        {

            TokenResult appDomainResult = await domainService.NewToken(GetCurrentUser());
            return StatusCode((int)(HttpStatusCode)appDomainResult.ResultCode, new
            {
                message = appDomainResult.ResultMessage,
                Token = appDomainResult.GenerateTokenModel.Token,
                refreshToken = appDomainResult.GenerateTokenModel.RefreshToken,
                refreshTokenExpires = appDomainResult.GenerateTokenModel.RefreshTokenExpires,
            });
        }
        [HttpPost]
        [ClaimsAuthorize]
        [Route("api/ChangeRegister/{value}")]
        public async Task<IActionResult> ChangeRegister(AllowRegister value)
        {
            try
            {
                await domainService.ChangeRegister(value);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/AllowRegister")]
        public async Task<IActionResult> GetAllowRegister()
        {
            var result = await domainService.GetAllowRegister();
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = result.ToString() });
        }
        [HttpPost]
        [Route("api/KeyForgotPassword")]
        public async Task<IActionResult> KeyForgotPassword([FromBody] KeyForgotPasswordModel model)
        {
            try
            {
                await domainService.KeyForgotPassword(model);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !"});
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPost]
        [Route("api/ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            
            if (ModelState.IsValid)
            {
                try
                {
                    await domainService.ResetPassword(model);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }

        [HttpPost("api/Base/Upload")]
        [ClaimsAuthorize]
        public IActionResult Upload(IFormFile file)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            var data = UploadConfig.UploadFile(file, baseUrl, "Upload/Images/");
            if (data.Success)
                return Ok(new { data = data.Link, dataReSize = data.LinkResize, message = data.Message });
            else
                return BadRequest(new { message = data.Message });
        }

        [HttpGet]
        [Route("api/Base/Upload/File")]
        public IActionResult GetFileBaseUpload()
        {
            return StatusCode((int)HttpStatusCode.OK, new { data = "jpg,jpeg,png,bmp", message = "Thành công" });
        }
    }
}
