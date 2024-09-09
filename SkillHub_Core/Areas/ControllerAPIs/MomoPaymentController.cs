using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Services.MomoPaymentService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/MomoPayment")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class MomoPaymentController : BaseController
    {
        private lmsDbContext dbContext;
        private MomoPaymentService domainService;
        public MomoPaymentController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new MomoPaymentService(this.dbContext);
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Pay([FromBody] MomoPayment model)
        {
            var data = await domainService.PaymentByMomo(model);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPost]
        [Route("xu-ly-thanh-toan")]
        public async Task<AppDomainResult> GetResultPayMent([FromBody] MomoPaymentResult result)
        {
            var success= await domainService.ConfirmPaymentClient(result);
            return new AppDomainResult()
            {
                ResultCode = (int)HttpStatusCode.OK,
                ResultMessage = "Lấy Thông tin sau khi thanh toán của momo thành công!",
                Data = result,
                Success = success// true
            };
        }
    }
}