using ExcelDataReader;
using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
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
using System.Web;

using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [ValidateModelState]
    public class NotificationController : BaseController
    {
        [HttpGet]
        [Route("api/Notification")]
        public async Task<IActionResult> GetAll([FromQuery] SearchOptions search)
        {
            var data = await NotificationService.GetAll(search, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            int unreadNotificationCount = await NotificationService.UnreadNotificationCount(GetCurrentUser());
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data, unreadNotificationCount = unreadNotificationCount });
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
    }
}
