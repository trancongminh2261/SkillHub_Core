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
using static LMSCore.Services.TranscriptService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Transcript")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class TranscriptController : BaseController
    {
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]TranscriptCreate model)
        {
            try
            {
                var data = await TranscriptService.Insert(model, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await TranscriptService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("point-edit")]
        public async Task<IActionResult> PointEdit([FromBody] TranscriptModel model)
        {
            try
            {
                await TranscriptService.PointEdit(model, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("by-class/{classId}")]
        public async Task<IActionResult> GetByClass(int classId)
        {
            var data = await TranscriptService.GetByClass(classId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            else
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("get-point-by-student-class")]
        public async Task<IActionResult> GetByStudentClass(int studentId, int classId)
        {
            var data = await TranscriptService.GetByStudentClass(studentId, classId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            else
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("point/{transcriptId}")]
        public async Task<IActionResult> GetPoint(int transcriptId)
        {
            var data = await TranscriptService.GetPoint(transcriptId,GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            else
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("student-point")]
        public async Task<IActionResult> GetStudentPoint([FromQuery] PointSearch baseSearch)
        {
            var data = await TranscriptService.GetPointByStudent(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            else
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
