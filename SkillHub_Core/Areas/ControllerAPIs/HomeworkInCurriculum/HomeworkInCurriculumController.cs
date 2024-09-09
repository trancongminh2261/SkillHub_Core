using LMSCore.Areas.ControllerAPIs;
using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services.HomeworkInCurriculum;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utilities;

namespace LMSCore.Areas.HomeworkInCurriculum
{
    [Route("api/HomeworkInCurriculum")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class HomeworkInCurriculumController : BaseController
    {
        private lmsDbContext dbContext;
        private HomeworkInCurriculumService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public HomeworkInCurriculumController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new HomeworkInCurriculumService(this.dbContext, _hostingEnvironment);
        }

        /// <summary>
        /// Lấy tất cả bài tập theo giáo trình
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] HomeworkSearchInCurriculum baseSearch)
        {
            var data = await domainService.GetAll(baseSearch, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data[0].TotalRow, data = data, });
        }

        /// <summary>
        /// Lấy tất cả các buổi trong giáo trình
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("session-number-curriculum")]
        public async Task<IActionResult> GetSessionNumberCurriculum(int id)
        {
            try
            {
                var data = await HomeworkInCurriculumService.GetSessionNumberCurriculum(id);
                if (data == null)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new
                {
                    message = e.Message
                });
            }
        }

        /// <summary>
        /// Thêm bài tập và bài thi cho giáo trình
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] HomeworkInCurriculumCreate itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Insert(itemModel, GetCurrentUser());
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
        /// Cập nhật bài tập trong giáo trình   
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] HomeworkInCurriculumUpdate itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Update(itemModel, GetCurrentUser());
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
        /// Xóa bài tập trong giáo trình
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await domainService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        /// <summary>
        /// Upload tài liệu trong bài tập trong giáo trình
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        [ClaimsAuthorize]
        public IActionResult Upload(IFormFile file)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            string fileName = string.Empty;
            if (file != null)
                fileName = StringRemoveUnicode.RemoveUnicode(file.FileName.Split(".")[0].ToLower())
                    .Replace(" ", "_").Trim()
                    .Replace("/", "")
                    .Replace("\\", "")
                    .Replace("-", "")
                    + "-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var data = UploadConfig.UploadFile(file, baseUrl, "Upload/HomeworkInCurriculum/", lmsEnum.UploadType.Document, fileName);
            if (data.Success)
                return Ok(new { data = data.Link, dataReSize = data.LinkResize, message = data.Message });
            else
                return BadRequest(new { message = data.Message });
        }

        /// <summary>
        /// Cập nhật vị trí bài tập trong giáo trình   
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update-index")]
        public async Task<IActionResult> UpdateIndexHomeworkInCurriculum([FromBody] List<IndexHomeworkInCurriculum> itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.UpdateIndexHomeworkInCurriculum(itemModel, GetCurrentUser());
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
