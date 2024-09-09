using LMSCore.Areas.Models;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Ward")]
    [ClaimsAuthorize]
    public class WardController : BaseController
    {
       
        [HttpGet]
        [Route("{Id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
            tbl_Ward data = await Ward.GetbyId(Id);
            if (data == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Thất bại !" });
            }
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" ,data});
        }
        [HttpGet]
        [Route("")]
        //Lấy danh sách quận huyện
        public async Task<IActionResult> GetAll([FromQuery] WardSearch search)
        {
            var data = await Ward.GetAll(search);
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
