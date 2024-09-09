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
using LMSCore.Services.HomeworkResult;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/HomeworkResult")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class HomeworkResultController : BaseController
    {
        private lmsDbContext dbContext;
        private HomeworkResultService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public HomeworkResultController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new HomeworkResultService(this.dbContext, _hostingEnvironment);
        }
        /// <summary>
        /// thêm btvn / bài thi
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] HomeworkResultCreate itemModel)
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
        public async Task<IActionResult> Update([FromBody] StudentHomeworkResultUpdate itemModel)
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
        /// giáo viên chấm bài
        /// <returns></returns>
        [HttpPut]
        [Route("teacher-point")]
        public async Task<IActionResult> TeacherUpdate([FromBody] TeacherHomeworkResultUpdate itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.TeacherUpdate(itemModel, GetCurrentUser());
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
        public async Task<IActionResult> GetAll([FromQuery] HomeworkResultSearch baseSearch)
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
    }
}
