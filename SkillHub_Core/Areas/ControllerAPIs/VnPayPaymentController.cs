//using LMSCore.Areas.Models;
//using LMSCore.Areas.Request;
//using LMSCore.Models;
//using LMSCore.Services;
//using LMSCore.Users; 
//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

//namespace LMSCore.Areas.ControllerAPIs
//{
//    [Route("api/VnPayPayment")]
//    [ClaimsAuthorize]
//    public class VnPayPaymentController : BaseController
//    {
//        private lmsDbContext dbContext;
//        private VnPayService domainService;
//        public VnPayPaymentController()
//        {
//            this.dbContext = new lmsDbContext();
//            this.domainService = new VnPayService(this.dbContext);
//        }
//        [HttpPost]
//        [Route("")]
//        public async Task<IActionResult> Pay(VnPayPayment model)
//        {
//            var data = await domainService.Pay(model);
//            if (data != null)
//                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
//            return StatusCode((int)HttpStatusCode.NoContent);
//        }
//        [HttpPost]
//        [Route("xu-ly-thanh-toan")]
//        public async Task<IActionResult> GetResultPayMent(VnPayPaymentCofirm cofirm)
//        {
//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    var data = await domainService.GetResultPayMent(cofirm);
//                    return StatusCode((int)HttpStatusCode.OK, new { message = data.ResultMessage, data.Data });
//                }
//                catch (Exception e)
//                {
//                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
//                }
//            }
//            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
//            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
//        }
//    }
//}