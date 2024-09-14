using ExcelDataReader;
using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.Models;
using LMS_Project.Services;
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
using LMSCore.Users;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Areas.ControllerAPIs;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class NotificationController : BaseController
    {
        [HttpGet]
        [Route("api/Notification")]
        public async Task<IActionResult> GetAll([FromQuery] SearchOptions search)
        {
            var data = await NotificationService.GetAll(search,GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpPut]
        [Route("api/Notification/Seen/{id}")]
        public async Task<IActionResult> Seen(int id)
        {
            await NotificationService.Seen(id,GetCurrentUser());
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
        }
        [HttpPut]
        [Route("api/Notification/SeenAll")]
        public async Task<IActionResult> SeenAll()
        {
            await NotificationService.SeenAll(GetCurrentUser());
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
        }
        /// <summary>
        /// true - gửi thông báo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Notification/maintenance")]
        public async Task<IActionResult> MaintenanceNotice()
        {
            var data = await NotificationService.MaintenanceNotice();
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
    }
}
