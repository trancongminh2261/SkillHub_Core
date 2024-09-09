using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using LMSCore.Areas.Request;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.Account;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.LMS;
using LMSCore.Utilities;
using System.Net.Http;
using LMSCore.NotificationConfig;
using System.Collections;
using LMSCore.Services.PaymentSession;
using UAParser;

namespace LMSCore.ControllerAPIs
{
    [ValidateModelState]
    public class AccountController : BaseController
    {
        [HttpPost]
        [Route("api/Account/Login")]
        public async Task<IActionResult> Login()
        {
            try
            {
                string username = Request.Form["username"];
                string password = Request.Form["password"];
                //string deviceName = Request.Form["deviceName"];
                //var userAgent = Request.Headers["User-Agent"].ToString();
                //var uaParser = Parser.GetDefault();
                //ClientInfo clientInfo = uaParser.Parse(userAgent);
                //string deviceName = clientInfo.Device.Family;
                var device = await GetDataConfig.GetDeviceName(HttpContext);
                TokenResult appDomainResult = await Account.Login(username, password, device);
                if (appDomainResult.ResultCode != ((int)HttpStatusCode.OK))
                    return StatusCode(appDomainResult.ResultCode, new
                    {
                        message = appDomainResult.ResultMessage,
                    });
                return StatusCode(appDomainResult.ResultCode, new
                {
                    message = appDomainResult.ResultMessage,
                    token = appDomainResult.GenerateTokenModel.Token,
                    refreshToken = appDomainResult.GenerateTokenModel.RefreshToken,
                    refreshTokenExpires = appDomainResult.GenerateTokenModel.RefreshTokenExpires,
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new
                {
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        [Route("api/Account/LoginTest")]
        public async Task<IActionResult> LoginTest(string username, string password)
        {
            try
            {
                //var userAgent = Request.Headers["User-Agent"].ToString();
                //var uaParser = Parser.GetDefault();
                //ClientInfo clientInfo = uaParser.Parse(userAgent);
                //string deviceName = clientInfo.Device.Family;
                var device = await GetDataConfig.GetDeviceName(HttpContext);
                TokenResult appDomainResult = await Account.Login(username, password, device);
                if (appDomainResult.ResultCode != ((int)HttpStatusCode.OK))
                    return StatusCode(appDomainResult.ResultCode, new
                    {
                        message = appDomainResult.ResultMessage,
                    });
                return StatusCode(appDomainResult.ResultCode, new
                {
                    message = appDomainResult.ResultMessage,
                    token = appDomainResult.GenerateTokenModel.Token,
                    refreshToken = appDomainResult.GenerateTokenModel.RefreshToken,
                    refreshTokenExpires = appDomainResult.GenerateTokenModel.RefreshTokenExpires,
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new
                {
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        [Route("api/LoginDev")]
        public async Task<IActionResult> LoginByDev([FromBody] LoginDevModel model)
        {
            TokenResult appDomainResult = await Account.LoginByDev(model);
            if (appDomainResult.ResultCode != ((int)HttpStatusCode.OK))
                return StatusCode(appDomainResult.ResultCode, new
                {
                    message = appDomainResult.ResultMessage,
                });
            return StatusCode(appDomainResult.ResultCode, new
            {
                message = appDomainResult.ResultMessage,
                token = appDomainResult.GenerateTokenModel.Token,
                refreshToken = appDomainResult.GenerateTokenModel.RefreshToken,
                refreshTokenExpires = appDomainResult.GenerateTokenModel.RefreshTokenExpires,
            });
        }
        [HttpPost]
        [Route("api/RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest itemModel)
        {
            TokenResult appDomainResult = await Account.RefreshToken(itemModel);
            if (appDomainResult.ResultCode == ((int)HttpStatusCode.Unauthorized))
            {
                return StatusCode(((int)HttpStatusCode.Unauthorized), new
                {
                    message = appDomainResult.ResultMessage,
                });
            }
            return StatusCode(appDomainResult.ResultCode, new
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
                var data = await Account.GetAccount();
                return StatusCode(((int)HttpStatusCode.OK), new { message = "Thành công", data });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message + "||" + ex.InnerException });
            }
        }
        [HttpGet]
        [Route("api/GetAccountTemplate")]
        public async Task<IActionResult> GetAccountTemplate()
        {
            try
            {
                var data = await Account.GetAccountTemplate();
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
                    await Account.ChangePassword(model, GetCurrentUser());
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
            if (await Account.GetAllowRegister() == AllowRegister.UnAllow)
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Chức năng này đã tắt" });
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await Account.Register(model);
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

        [HttpPost]
        [Route("api/ConfirmOTP")]
        public async Task<IActionResult> ConfirmOTP(int userId, string Otp)
        {
            if (await Account.GetAllowRegister() == AllowRegister.UnAllow)
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Chức năng này đã tắt" });
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await Account.CheckOTP(userId, Otp);
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

        [HttpPost]
        [Route("api/ReSendOTP")]
        public async Task<IActionResult> ReSendOTP(int userId)
        {
            if (await Account.GetAllowRegister() == AllowRegister.UnAllow)
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Chức năng này đã tắt" });
            if (ModelState.IsValid)
            {
                using (var db = new lmsDbContext())
                {
                    try
                    {
                        var user = db.tbl_UserInformation.SingleOrDefault(x => x.UserInformationId == userId && x.Enable == true); // ban đầu là false
                        if (user == null)
                            throw new Exception("Không tìm thấy tài khoản");
                        var data = await UserInformation.GenerateOTPAndSendMail(user, "MONA-LMS OTP", "");
                        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                    }
                    catch (Exception e)
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                    }
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
            TokenResult appDomainResult = await Account.NewToken(GetCurrentUser());
            return StatusCode(appDomainResult.ResultCode, new
            {
                message = appDomainResult.ResultMessage,
                token = appDomainResult.GenerateTokenModel.Token,
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
                await Account.ChangeRegister(value);
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
            var result = await Account.GetAllowRegister();
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = result.ToString() });
        }
        [HttpPost]
        [Route("api/KeyForgotPassword")]
        public async Task<IActionResult> KeyForgotPassword([FromBody] KeyForgotPasswordModel model)
        {
            try
            {
                await Account.KeyForgotPassword(model);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
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
                    await Account.ResetPassword(model);
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
        [HttpPost]
        [Route("api/Base/ClientWriteLog")]
        public IActionResult ClientWriteLog([FromBody] AssetCRM.ClientWriteLogModel itemModel)
        {
            var data = AssetCRM.ClientWriteLog(itemModel);
            return StatusCode(((int)HttpStatusCode.OK), new { data = data, message = ApiMessage.SAVE_SUCCESS });
        }
    }
}
