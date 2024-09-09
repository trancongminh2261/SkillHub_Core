using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Users;
using System;

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using LMSCore.Utilities;
using LMSCore.Services.Homework;
using Microsoft.AspNetCore.Http;
using Utilities;
using System.Collections.Generic;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Homework")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class HomeworkController : BaseController
    {
        private lmsDbContext dbContext;
        private HomeworkService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public HomeworkController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new HomeworkService(this.dbContext, _hostingEnvironment);
        }
        /// <summary>
        /// thêm btvn / bài thi
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] HomeworkCreate itemModel)
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
        /// cập nhật btvn
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] HomeworkUpdate itemModel)
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
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] HomeworkSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data, totalRow = data[0].TotalRow, });
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("teacher-available")]
        public async Task<IActionResult> GetTeacherAvailable()
        {
            var data = await domainService.GetTeacherAvailable();
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("teacher-available/{classId}")]
        public async Task<IActionResult> GetTeacherAvailableByClass(int classId)
        {
            var data = await domainService.GetTeacherAvailable(classId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
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
            var data = UploadConfig.UploadFile(file, baseUrl, "Upload/Homework/", lmsEnum.UploadType.Document, fileName);
            if (data.Success)
                return Ok(new { data = data.Link, dataReSize = data.LinkResize, message = data.Message });
            else
                return BadRequest(new { message = data.Message });
        }

        /// <summary>
        /// Cập nhật vị trí bài tập trong lớp  
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update-index")]
        public async Task<IActionResult> UpdateIndexHomework([FromBody] List<IndexHomeworkUpdate> itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.UpdateIndexHomework(itemModel, GetCurrentUser());
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
        /// Cập nhật nhiều bài tập cho giáo viên
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update-teacher")]
        public async Task<IActionResult> UpdateTeacher([FromBody] HomeworkForTeacherUpdate itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await domainService.UpdateTeacher(itemModel);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !"});
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
