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
//    [Route("api/zns")]
//    [ClaimsAuthorize]
//    public class ZnsController : BaseController
//    {
//        private lmsDbContext dbContext;
//        private ZaloNotificationService domainService;
//        public ZnsController()
//        {
//            this.dbContext = new lmsDbContext();
//            this.domainService = new ZaloNotificationService(this.dbContext);
//        }

//        [HttpGet]
//        [Route("zns-config/{id}")]
//        public async Task<IActionResult> GetZnsConfigById(int id)
//        {
//            var data = await domainService.GetZnsConfigById(id);
//            if (data == null)
//                return StatusCode((int)HttpStatusCode.NoContent);
//            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
//        }

//        [HttpGet]
//        [Route("zns-config")]
//        public async Task<IActionResult> GetZnsConfig()
//        {
//            var data = await domainService.GetZnsConfig();
//            if (!data.Any())
//                return StatusCode((int)HttpStatusCode.NoContent);
//            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
//        }

//        [HttpGet]
//        [Route("zns-template/{id}")]
//        public async Task<IActionResult> GetZnsTemplateById(int id)
//        {
//            var data = await domainService.GetZnsTemplateById(id);
//            if (data == null)
//                return StatusCode((int)HttpStatusCode.NoContent);
//            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
//        }

//        [HttpGet]
//        [Route("zns-template")]
//        public async Task<IActionResult> GetZnsTemplate()
//        {
//            var data = await domainService.GetZnsTemplate();
//            if (!data.Any())
//                return StatusCode((int)HttpStatusCode.NoContent);
//            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
//        }

//        [HttpPut]
//        [Route("zns-config")]
//        public async Task<IActionResult> UpdateZnsConfig(ZnsConfigUpdate request)
//        {
//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    var data = await domainService.ZnsConfigUpdate(request, GetCurrentUser());
//                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
//                }
//                catch (Exception e)
//                {
//                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
//                }
//            }
//            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
//            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
//        }
//        [HttpPut]
//        [Route("zns-template")]
//        public async Task<IActionResult> UpdateZnsTemplate(ZnsTemplateUpdate request)
//        {
//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    var data = await domainService.ZnsTemplateUpdate(request, GetCurrentUser());
//                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
//                }
//                catch (Exception e)
//                {
//                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
//                }
//            }
//            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
//            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
//        }
//        /// <summary>
//        /// lấy mẫu zns
//        /// 1 - mẫu thông báo học phí
//        /// 2 - mẫu thông báo kết quả bài thi
//        /// </summary>
//        /// <param name="templateType"></param>
//        /// <returns></returns>
//        [HttpGet]
//        [Route("zns-template-sample")]
//        public IActionResult GetZnsTemplateSample([FromQuery] int templateType)
//        {
//            var data = domainService.GetTemplateSample(templateType);
//            if (!data.Any())
//                return StatusCode((int)HttpStatusCode.NoContent);
//            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
//        }
//    }
//}
