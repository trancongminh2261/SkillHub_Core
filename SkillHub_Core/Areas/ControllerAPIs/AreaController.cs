using LMS_Project.Areas.Models;
using LMS_Project.Models;
using LMS_Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Areas.ControllerAPIs;

namespace LMS_Project.Areas.ControllerAPIs
{
    [Route("api/Area")]
    public class AreaController : BaseController
    {
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
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
