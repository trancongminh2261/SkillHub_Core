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
using LMSCore.Areas.ControllerAPIs;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class SeminarRecordController : BaseController
    {
        [HttpGet]
        [Route("api/SeminarRecord/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await SeminarRecordService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("api/SeminarRecord")]
        public async Task<IActionResult> Insert([FromBody] SeminarRecordCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await SeminarRecordService.Insert(model, GetCurrentUser());
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
        [Route("api/SeminarRecord")]
        public async Task<IActionResult> Update([FromBody] SeminarRecordUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await SeminarRecordService.Update(model, GetCurrentUser());
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
        [Route("api/SeminarRecord/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await SeminarRecordService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/SeminarRecord/BySeminar/{seminarId}")]
        public async Task<IActionResult> GetBySeminar(int seminarId)
        {
            var data = await SeminarRecordService.GetBySeminar(seminarId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !",data = data });
        }
    }
}
