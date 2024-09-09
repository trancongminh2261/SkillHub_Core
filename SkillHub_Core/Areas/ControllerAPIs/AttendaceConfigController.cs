using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
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
    [ValidateModelState]
    [Route("api/AttendaceConfig")]
    public class AttendaceConfigController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await AttendaceConfigService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("active")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetById()
        {
            var data = await AttendaceConfigService.GetAttendaceConfigActive();
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Insert([FromBody] AttendaceConfigCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await AttendaceConfigService.Insert(model, GetCurrentUser());
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
        [ClaimsAuthorize]
        public async Task<IActionResult> Update([FromBody] AttendaceConfigUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await AttendaceConfigService.Update(model, GetCurrentUser());
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
        [ClaimsAuthorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await AttendaceConfigService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetAll([FromQuery] AttendaceConfigSearch baseSearch)
        {
            var data = await AttendaceConfigService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("type")]
        public IActionResult GetAttendaceTypeOption()
        {
            var data = AttendaceConfigService.GetAttendaceTypeOption();
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
