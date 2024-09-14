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
using static LMS_Project.Services.ExamService;
using LMSCore.Areas.ControllerAPIs;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class TagController : BaseController
    {
        [HttpPost]
        [Route("api/Tag")]
        public async Task<IActionResult> Insert([FromBody] TagCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await TagService.Insert(model, GetCurrentUser());
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
        [Route("api/Tag/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await TagService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/Tag")]
        public async Task<IActionResult> GetAll()
        {
            var data = await TagService.GetAll();
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
