using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using Microsoft.AspNetCore.Hosting;
using LMSCore.Utilities;
using LMSCore.Services.PaymentApprove;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/PaymentApprove")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class PaymentApproveController : BaseController
    {
        private lmsDbContext dbContext;
        private PaymentApproveService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public PaymentApproveController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new PaymentApproveService(this.dbContext, _hostingEnvironment);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await PaymentApproveService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Đã duyệt
        /// 3 - Không duyệt
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/status/{status}")]
        public async Task<IActionResult> Approve(int id, int status)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string baseUrl = Request.Scheme + "://" + Request.Host;
                    await PaymentApproveService.Approve(id, status, baseUrl, GetCurrentUser());
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
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await PaymentApproveService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] PaymentApproveSearch baseSearch)
        {
            var data = await PaymentApproveService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", 
                totalRow = data.TotalRow, 
                data = data.Data,
                totalMoney = data.TotalMoney,
                AllState = data.AllState,
                Opened = data.Opened,
                Approved = data.Approved,
                Canceled = data.Canceled,
            });
        }

        [HttpGet]
        [Route("v2")]
        public async Task<IActionResult> GetAllV2([FromQuery] PaymentApproveV2Search baseSearch)
        {
            var data = await PaymentApproveService.GetAllV2(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                totalRow = data.TotalRow,
                data = data.Data,
                totalMoney = data.TotalMoney,
                AllState = data.AllState,
                Opened = data.Opened,
                Approved = data.Approved,
                Canceled = data.Canceled,
            });
        }
    }
}
