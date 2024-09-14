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
using static LMS_Project.Services.LessonVideoService;
using static LMS_Project.Services.PermissionService;
using static LMSCore.Models.lmsEnum;

namespace LMS_Project.Areas.ControllerAPIs
{
    public class PermissionController : BaseController
    {
        [HttpPost]
        [Route("api/Permission")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Insert([FromBody] PermissionCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await PermissionService.Insert(model);
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

        [HttpPut]
        [Route("api/Permission")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Update([FromBody] PermissionUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await PermissionService.Update(model);
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
        [Route("api/Permission/role")]
        public async Task<IActionResult> GetRole()
        {
            var data = await PermissionService.GetRole();
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        [HttpGet]
        [Route("api/Permission/role-staff")]
        public async Task<IActionResult> GetRoleStaff()
        {
            var data = await PermissionService.GetRoleStaff();
            data = data.Where(x => x.Id != ((int)RoleEnum.student)).ToList();
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        [HttpGet]
        [Route("api/Permission")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetAll([FromQuery] PermissionSearch baseSearch)
        {
            var data = await PermissionService.GetAll(baseSearch);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
    }
}
