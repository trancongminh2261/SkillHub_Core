using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Services.IeltsExamResultService;
using static LMSCore.Services.IeltsExamService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/IeltsExamResult")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class IeltsExamResultController : BaseController
    {
        private lmsDbContext dbContext;
        private IeltsExamResultService domainService;
        public IeltsExamResultController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new IeltsExamResultService(this.dbContext);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]IeltsExamResultCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Insert(model, GetCurrentUser());
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
        [Route("choose-teacher")]
        public async Task<IActionResult> ChooseTeacher([FromBody] ChooseTeacherRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await domainService.ChooseTeacher(model);
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
        [HttpPut]
        [Route("review")]
        public async Task<IActionResult> Review([FromBody] IeltsExamResultReview model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await domainService.Review(model,GetCurrentUser());
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
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] IeltsExamResultSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("ielts-exam-result-overview")]
        public async Task<IActionResult> GetIeltsExamResultOverview([FromQuery] IeltsExamResultOverviewSearch baseSearch)
        {
            var data = await domainService.GetIeltsExamResultOverview(baseSearch);
            if(data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data});
        }
        [HttpGet]
        [Route("ielts-question-in-section-result")]
        public async Task<IActionResult> GetIeltsQuestionInSectionResult([FromQuery] IeltsQuestionInSectionResultSearch baseSearch)
        {
            var data = await domainService.GetIeltsQuestionInSectionResult(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        /// <summary>
        /// Lấy chi tiết nội dung kỹ năng
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ielts-skill-result-detail")]
        public async Task<IActionResult> GetIeltsSkillResultDetail([FromQuery] IeltsSkillResultSearch baseSearch)
        {
            var data = await domainService.GetIeltsSkillResultDetail(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
