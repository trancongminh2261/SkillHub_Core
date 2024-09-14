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
    public class AnswerInVideoController : BaseController
    {
        [HttpPost]
        [Route("api/AnswerInVideo")]
        public async Task<IActionResult> Insert([FromBody] AnswerInVideoCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await AnswerInVideoService.Insert(model, GetCurrentUser());
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
        [Route("api/AnswerInVideo/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await AnswerInVideoService.Delete(id, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/AnswerInVideo/{id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var data = await AnswerInVideoService.GetById(Id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpGet]
        [Route("api/AnswerInVideo/GetByQuestion/{questionInVideoId}")]
        public async Task<IActionResult> GetByQuestion(int questionInVideoId)
        {
            var data = await AnswerInVideoService.GetByQuestion(questionInVideoId);
            if (data.Any())
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
    }
}
