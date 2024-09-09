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
using static LMSCore.Services.IeltsQuestionGroupResultService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/IeltsQuestionGroupResult")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class IeltsQuestionGroupResultController : BaseController
    {
        private lmsDbContext dbContext;
        private IeltsQuestionGroupResultService domainService;
        public IeltsQuestionGroupResultController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new IeltsQuestionGroupResultService(this.dbContext);
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
        /// <summary>
        /// Chấm câu hỏi tự luận
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("grading-essay")]
        public async Task<IActionResult> GradingEssay([FromBody] GradingEssayRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await domainService.GradingEssay(model, GetCurrentUser());
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
        [Route("answer-comment")]
        public async Task<IActionResult> GetAnswerComment([FromQuery] AnswerCommentSearch baseSearch)
        {
            var data = await domainService.GetAnswerComment(baseSearch);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
