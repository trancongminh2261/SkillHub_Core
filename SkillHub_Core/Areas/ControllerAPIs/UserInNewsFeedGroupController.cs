using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/UserInNewsFeedGroup")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class UserInNewsFeedGroupController : BaseController
    {
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]UserInNFGroupCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await UserInNFGroupService.Insert(model, GetCurrentUser());
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
        [Route("")]
        public async Task<IActionResult> Update([FromBody]UserInNFGroupUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await UserInNFGroupService.Update(model, GetCurrentUser());
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

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await UserInNFGroupService.Delete(id, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }


        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] UserInNFGroupSearch baseSearch)
        {
            var data = await UserInNFGroupService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }




        [HttpGet]
        [Route("user-not-in-group/{groupId}")]
        public async Task<IActionResult> GetUserNotInGroup(int? groupId)
        {
            var data = await UserInNFGroupService.GetUserNotInGroup(groupId);
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data.Data });
        }
    }
}
