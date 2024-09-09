using LMSCore.Areas.Models;
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

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Area")]
    [ClaimsAuthorize]
    public class AreaController : BaseController
    {
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            tbl_Area data = await Area.GetById(Id);
            if (data != null)
            {
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" , data });
            }
            return StatusCode((int)HttpStatusCode.NoContent);

        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] AreaSearch search)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await Area.GetAll(search);
            int totalRow = data.TotalRow;
            if (totalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            if (data != null)
            {
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow, data = data.obj });
            }
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Thất bại !" });
        }
    }
}
