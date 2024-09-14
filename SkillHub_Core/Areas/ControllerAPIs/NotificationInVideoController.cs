using ExcelDataReader;
using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.Models;
using LMS_Project.Services;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.Users;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class NotificationInVideoController : BaseController
    {
        [HttpGet]
        [Route("api/NotificationInVideo/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await NotificationInVideoService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("api/NotificationInVideo")]
        public async Task<IActionResult> Insert([FromBody] NotificationInVideoCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await NotificationInVideoService.Insert(model, GetCurrentUser());
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
        [HttpGet]
        [Route("api/NotificationInVideo")]
        public async Task<IActionResult> GetAll([FromQuery] NotificationInVideoSearch search)
        {
            var data = await NotificationInVideoService.GetAll(search);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
