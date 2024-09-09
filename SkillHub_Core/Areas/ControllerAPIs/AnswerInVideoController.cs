using ExcelDataReader;
using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
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
using System.Web;

using static LMSCore.Models.lmsEnum;

namespace LMSCore.Areas.ControllerAPIs
{
    [ValidateModelState]
    [ClaimsAuthorize]
    public class AnswerInVideoController : BaseController
    {
        [HttpPost]
        [Route("api/AnswerInVideo")]
        public async Task<IActionResult> Insert([FromBody]AnswerInVideoCreate model)
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
                if (!ModelState.IsValid)
                {
                    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
                }
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
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await AnswerInVideoService.GetById(Id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpGet]
        [Route("api/AnswerInVideo/GetByQuestion/{questionInVideoId}")]
        public async Task<IActionResult> GetByQuestion(int questionInVideoId)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await AnswerInVideoService.GetByQuestion(questionInVideoId);
            if (data.Any())
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
    }
}
