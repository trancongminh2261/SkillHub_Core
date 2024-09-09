using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Refund")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class RefundController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await RefundService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]RefundCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await RefundService.Insert(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Bạn đã tạo yêu cầu hoàn tiền thành công, vui lòng đợi duyệt !", data });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody]RefundUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await RefundService.Update(model, GetCurrentUser());
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
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await RefundService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] RefundSearch baseSearch)
        {
            var data = await RefundService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", 
                totalRow = data.TotalRow, 
                data = data.Data,
                totalPrice = data.TotalPrice,
                AllState = data.AllState,
                Opened = data.Opened,
                Approved = data.Approved,
                Canceled = data.Canceled,
            });
        }
        [HttpGet]
        [Route("status")]
        [ProducesResponseType(typeof(RefundStatus), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatus([FromQuery] RefundStatusSearch baseSearch)
        {
            var data = await RefundService.GetRefundStatus(baseSearch, GetCurrentUser());
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                data = data
            });
        }
    }
}
