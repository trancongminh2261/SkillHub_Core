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
using static LMS_Project.Services.ExamResultService;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.LMS;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class ExamResultController : BaseController
    {
        [HttpPost]
        [Route("api/ExamResult/Submit")]
        public async Task<IActionResult> Submit(ExamSubmit model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ExamResultService.Submit(model, GetCurrentUser());
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
        [Route("api/ExamResult")]
        public async Task<IActionResult> GetAll([FromQuery] ExamResultSearch search)
        {
            var data = await ExamResultService.GetAll(search,GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("api/ExamResult/Detail/{examResultId}")]
        public async Task<IActionResult> GetDetail(int examResultId)
        {
            var data = await ExamResultService.GetDetail(examResultId);
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", 
                data = data.Data,
                totalPoint = data.TotalPoint,
                myPoint = data.MyPoint,
                examCode = data.ExamCode,
                examName = data.ExamName,
                isPass = data.IsPass,
                lessonVideoId = data.LessonVideoId,
                passPoint = data.PassPoint,
                type = data.Type,
                status = data.Status,
                statusName = data.StatusName,
                createdOn = data.CreatedOn,
                createdBy = data.CreatedBy,
                videoCourseId = data.VideoCourseId,
            });
        }
        [HttpPost]
        [Route("api/ExamResult/upload-video")]
        public IActionResult UploadVideo()
        {
            try
            {
                string link = "";
                string baseUrl = $"{Request.Scheme}://{Request.Host}";

                // Lấy file từ Request
                var file = Request.Form.Files.FirstOrDefault();

                if (file != null)
                {
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    string fileName = Guid.NewGuid() + ext; // Tạo tên file mới
                    var result = AssetCRM.isValIdImageAndVIdeo(ext); // Validate file loại video hoặc ảnh

                    if (result)
                    {
                        fileName = Guid.NewGuid() + ext;
                        link = $"{baseUrl}/Upload/ExamResult/{fileName}";

                        // Lưu file vào đường dẫn vật lý trên máy chủ
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", "ExamResult");
                        Directory.CreateDirectory(uploadPath); // Tạo thư mục nếu chưa tồn tại
                        var filePath = Path.Combine(uploadPath, fileName);

                        // Lưu file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        // Thay thế http bằng https nếu cần
                        if (!link.Contains("https"))
                            link = link.Replace("http", "https");

                        return StatusCode((int)HttpStatusCode.OK, new { data = link, message = ApiMessage.SAVE_SUCCESS });
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.INVALID_FILE });
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.NOT_FOUND });
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Chấm bài tự luận
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/ExamResult/grading-essay")]
        public async Task<IActionResult> CreateGradingEssay([FromBody] GradingEssayModel itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ExamResultService.CreateGradingEssay(itemModel, GetCurrentUser());
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
        /// <summary>
        /// Xem chi tiết chấm bài tự luận
        /// </summary>
        /// <param name="examResultId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ExamResult/grading-essay/{examResultId}")]
        public async Task<IActionResult> GetGradingEssay(int examResultId)
        {
            var data = await ExamResultService.GetGradingEssay(examResultId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                data = data,
            });
        }
        [HttpPost]
        [Route("api/ExamResult/add-teachers")]
        public async Task<IActionResult> AddTeachers([FromBody] AddTeacherModel itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ExamResultService.AddTeachers(itemModel, GetCurrentUser());
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
        [HttpGet]
        [Route("api/ExamResult/knowledge-exam-completed/{videoCourseId}")]
        public async Task<IActionResult> KnowledgeExamCompleted(int videoCourseId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ExamResultService.KnowledgeExamCompleted(videoCourseId, GetCurrentUser());
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
    }
}
