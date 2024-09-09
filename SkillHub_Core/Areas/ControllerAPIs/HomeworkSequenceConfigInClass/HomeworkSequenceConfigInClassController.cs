using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services.HomeworkConfigInClass;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LMSCore.Areas.ControllerAPIs.HomeworkSequenceConfigInClass
{
    [Route("api/HomeworkSequenceConfigInClass")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class HomeworkSequenceConfigInClassController : BaseController
    {
        private lmsDbContext dbContext;
        private HomeworkSequenceConfigInClassService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public HomeworkSequenceConfigInClassController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new HomeworkSequenceConfigInClassService(this.dbContext, _hostingEnvironment);
        }

        /// <summary>
        /// Lấy data config bài tập tuần tự trong lớp học id = classId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]  
        public async Task<IActionResult> GetHomeworkSequenceConfigInClass(int id)
        {
            var data = await domainService.GetHomeworkSequenceConfigInClass(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data});
        }

        /// <summary>
        /// Cập nhật cho phép làm bài tập tuần tự id = classId
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("allow-homework-sequence")]
        public async Task<IActionResult> AllowHomeworkSequenceInClass([FromBody] AllowHomeworkSequenceInClassUpdate itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.AllowHomeworkSequenceInClass(itemModel, GetCurrentUser());
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
