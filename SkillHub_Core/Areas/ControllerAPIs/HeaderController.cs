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
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Header")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class HeaderController : BaseController
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetFullData([FromQuery] HeaderOptions baseSearch)
        {
            var data = await HeaderService.GetFullData(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("menu-number")]
        public async Task<IActionResult> GetMenuNumber([FromQuery] MenuNumberOptions baseSearch)
        {
            var data = await HeaderService.GetMenuNumber(baseSearch, GetCurrentUser());
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

    }
}
