using LMS_Project.Areas.Models;
using LMS_Project.Models;
using LMS_Project.Services;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static LMSCore.Models.lmsEnum;

namespace LMS_Project.Areas.ControllerAPIs
{
    [Route("api/ChangeInfo")]
    [ClaimsAuthorize]
    public class ChangeInfoController : BaseController
    {
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await ChangeInfoService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("{id}/Status/{status}")]
        public async Task<IActionResult> Approve(int id, ChangeInfoStatus status)
        {
            try
            {
                await ChangeInfoService.Approve(id,status,GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] ChangeInfo search)
        {
            var data = await ChangeInfoService.GetAll(search);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
