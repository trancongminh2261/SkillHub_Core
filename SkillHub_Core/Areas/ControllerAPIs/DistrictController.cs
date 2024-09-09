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
    [Route("api/District")]
    [ClaimsAuthorize]
    public class DistrictController : BaseController
    {
        
        [HttpGet]
        [Route("{Id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
            tbl_District data = await District.GetById(Id);
            if (data != null)
            {
                return StatusCode((int)HttpStatusCode.OK, new {  message = "Thành công !" , data });

            }
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Thất bại !" });
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] DistrictSearch search)
        {
           
            var data = await District.GetAll(search);
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

