using ExcelDataReader;
using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
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

using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.ExamService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ExamController : BaseController
    {
        [HttpGet]
        [Route("api/Exam/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await ExamService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("api/Exam")]
        public async Task<IActionResult> Insert([FromBody]ExamCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ExamService.Insert(model, GetCurrentUser());
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
        [Route("api/Exam")]
        public async Task<IActionResult> Update([FromBody]ExamUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ExamService.Update(model, GetCurrentUser());
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
        [Route("api/Exam/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await ExamService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/Exam")]
        public async Task<IActionResult> GetAll([FromQuery] ExamSearch baseSearch)
        {
            var data = await ExamService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("api/Exam/Detail/{examId}")]
        public async Task<IActionResult> GetDetail(int examId)
        {
            var data = await ExamService.GetDetail(examId,GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            double totalPoint = await ExamService.GetTotalPoint(examId);
            var exam = await ExamService.GetById(examId);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data.Data, totalPoint, time = exam.Time });
        }
        [HttpPost]
        [Route("api/Exam/AddExerciseGroup")]
        public async Task<IActionResult> AddExerciseGroup(AddExerciseGroupModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ExamService.AddExerciseGroup(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        /// <summary>
        /// Thêm câu hỏi Tự động
        /// </summary>
        /// <remarks>
        /// type 1 - Đơn, 2 - Nhóm
        /// </remarks>
        /// <param name="examSectionId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Exam/{examSectionId}/AddRandom/{amount}/Type/{type}")]
        public async Task<IActionResult> AddRandom(int examSectionId, int amount,int type)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ExamService.AddRandom(examSectionId,amount,type, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
    }
}
