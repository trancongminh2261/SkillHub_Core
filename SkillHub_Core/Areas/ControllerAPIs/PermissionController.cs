using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.PermissionService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [ValidateModelState]
    public class PermissionController : BaseController
    {
        [HttpPost]
        [Route("api/Permission")]
        [ClaimsAuthorize]
        public async Task<IActionResult> InsertOrUpdate([FromBody] PermissionInsertOrUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await PermissionService.InsertOrUpdate(model);
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
        //[HttpPost]
        //[Route("api/Permission")]
        //[ClaimsAuthorize]
        //public async Task<IActionResult> Insert([FromBody]PermissionCreate model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var data = await PermissionService.Insert(model);
        //            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //        }
        //    }
        //    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //}
    }
}
