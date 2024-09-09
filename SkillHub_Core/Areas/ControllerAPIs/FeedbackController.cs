using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using LMSCore.Models;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Feedback")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class FeedbackController : BaseController
    {
        private lmsDbContext dbContext;
        private FeedbackService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public FeedbackController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new FeedbackService(this.dbContext, _hostingEnvironment);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] FeedbackCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await FeedbackService.Insert(model, GetCurrentUser());
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
        [Route("")]
        public async Task<IActionResult> Update([FromBody] FeedbackUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await FeedbackService.Update(model, GetCurrentUser());
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
        public async Task<IActionResult> GetAll([FromQuery] FeedbackSearch baseSearch)
        {
            var data = await FeedbackService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpGet]
        [Route("v2")]
        public async Task<IActionResult> GetAllV2([FromQuery] FeedbackV2Search baseSearch)
        {
            var data = await FeedbackService.GetAllV2(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await FeedbackService.GetById(id, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data = data });
        }

        [HttpPut]
        [Route("rating-feedback/{id}")]
        public async Task<IActionResult> RatingFeedback(int id, int rating)
        {
            try
            {
                await FeedbackService.RatingFeedBack(id, rating, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("feedback-inprocess")]
        public async Task<IActionResult> GetFeedbackInProcess(string branchIds)
        {
            try
            {
                var data = await FeedbackService.GetFeedbackInProcess(branchIds, GetCurrentUser());
                if (data == 0)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });

            }
        }
    }
}
