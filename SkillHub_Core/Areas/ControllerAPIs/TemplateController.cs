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
    [Route("api/Template")]
    [ValidateModelState]
    public class TemplateController : BaseController
    {
        [HttpGet]
        [Route("guide/{type}")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetGuide(int type)
        {
            var data = await TemplateService.GetGuide(type);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPut]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Update([FromBody]TemplateUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await TemplateService.Update(model, GetCurrentUser());
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
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetAll()
        {
            var data = await TemplateService.GetAll();
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("by-type/{type}")]
        public async Task<IActionResult> GetByType(int type)
        {
            var data = await TemplateService.GetByType(type);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
