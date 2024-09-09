using LMSCore.Areas.Request;
using LMSCore.DTO.ClassTranscript;
using LMSCore.DTO.SampleTranscriptDetail;
using LMSCore.Models;
using LMSCore.Services.SampleTranscriptDetail;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LMSCore.Areas.ControllerAPIs.SampleTranscriptDetail
{
    [Route("api/SampleTranscriptDetail")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class SampleTranscriptDetailController : BaseController
    {
        private lmsDbContext dbContext;
        private SampleTranscriptDetailService domainService;
        public SampleTranscriptDetailController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new SampleTranscriptDetailService(this.dbContext);
        }

        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType(typeof(SampleTranscriptDetailDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPut]
        [Route("")]
        [ProducesResponseType(typeof(SampleTranscriptDetailDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] SampleTranscriptDetailPut model)
        {
            try
            {
                var data = await domainService.Update(model, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("change-index")]
        public async Task<IActionResult> ChangeIndex([FromBody] ChangeIndexRequest model)
        {
            try
            {
                await domainService.ChangeIndex(model, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(SampleTranscriptDetailDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Insert([FromBody] SampleTranscriptDetailPost model)
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
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await domainService.Delete(id, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("by-sample-transcript/{sampleTranscriptId}")]
        [ProducesResponseType(typeof(IList<SampleTranscriptDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBySampleTranscript(int sampleTranscriptId)
        {
            var data = await domainService.GetBySampleTranscript(sampleTranscriptId);
            if (data.Any())
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
    }
}
