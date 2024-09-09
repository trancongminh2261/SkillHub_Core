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
    [Route("api/StudentExpectation")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class StudentExpectationController : BaseController
    {
        [HttpGet]
        [Route("{classRegistrationId}")]
        public async Task<IActionResult> GetByClassRegistrationId(int classRegistrationId)
        {
            var data = await StudentExpectationService.GetByClassRegistrationId(classRegistrationId);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
