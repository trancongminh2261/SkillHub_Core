using LMS_Project.Areas.Models;
using LMS_Project.Models;
using LMS_Project.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using LMSCore.Users;
using Microsoft.AspNetCore.Mvc;
using static LMS_Project.Services.WriteLogService;
using LMSCore.Areas.ControllerAPIs;

namespace LMS_Project.Areas.ControllerAPIs
{
    public class WriteLogController : BaseController
    {
        [HttpPost]
        [Route("api/WriteLog")]
        public async Task<IActionResult> Insert([FromBody] WriteLogCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await WriteLogService.Insert(model);
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
        [Route("api/WriteLog")]
        public async Task<IActionResult> GetAll()
        {
            var data = await WriteLogService.GetAll();
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
