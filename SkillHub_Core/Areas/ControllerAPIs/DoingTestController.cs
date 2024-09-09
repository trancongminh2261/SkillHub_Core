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
using static LMSCore.Services.DoingTestService;
using static LMSCore.Services.IeltsExamService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/DoingTest")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class DoingTestController : BaseController
    {
        private lmsDbContext dbContext;
        private DoingTestService domainService;
        public DoingTestController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new DoingTestService(this.dbContext);
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
        public async Task<IActionResult> Insert([FromBody] DoingTestCreate model)
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

        [HttpGet]
        [Route("total-question-uncompleted/{id}")]
        public async Task<IActionResult> GetTotalQuestionUncompleted(int id)
        {
            var data = await domainService.GetTotalQuestionUncompleted(id);
            return StatusCode(((int)HttpStatusCode.OK), new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("total-question-uncompleted-skill")]
        public async Task<IActionResult> GetTotalQuestionUncompletedSkill([FromQuery] TotalQuestionUncompletedSkillSearch baseSearch)
        {
            var data = await domainService.GetTotalQuestionUncompletedSkill(baseSearch);
            return StatusCode(((int)HttpStatusCode.OK), new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("current-section")]
        public async Task<IActionResult> InsertCurrentSection([FromBody] DoingTestCurrentSection model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await domainService.InsertCurrentSection(model);
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
        [Route("draft")]
        public async Task<IActionResult> GetDraft([FromQuery] DoingTestDraftSearch baseSearch)
        {
            var data = await domainService.GetDraft(baseSearch,GetCurrentUser());
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPost]
        [Route("insert-or-update-details")]
        public async Task<IActionResult> InsertOrUpdateDetails([FromBody] DoingTestDetailInsertOrUpdates model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.InsertOrUpdateDetails(model, GetCurrentUser());
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
        [Route("ielts-question-group")]
        public async Task<IActionResult> GetIeltsQuestionGroup([FromQuery] DoingTestIeltsQuestionGroupSearch baseSearch)
        {
            var data = await domainService.GetIeltsQuestionGroup(baseSearch);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPost]
        [Route("save-time")]
        public async Task<IActionResult> SaveTime([FromBody] SaveTimeModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await domainService.SaveTime(model);
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
