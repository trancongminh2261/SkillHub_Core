using LMSCore.Areas.Models;
using LMSCore.Models;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using LMSCore.Users;
using LMSCore.Utilities;
using LMSCore.Services.SampleTranscript;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using LMSCore.DTO.SampleTranscript;
using LMSCore.Areas.Request;
using LMSCore.DTO.ClassTranscript;

namespace LMSCore.Areas.ControllerAPIs.SampleTranscript
{
    [Route("api/SampleTranscript")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class SampleTranscriptController : BaseController
    {
        private lmsDbContext dbContext;
        private SampleTranscriptService domainService;
        public SampleTranscriptController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new SampleTranscriptService(this.dbContext);
        }
        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType(typeof(IList<StudentInClassWhenAttendanceDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPut]
        [Route("")]
        [ProducesResponseType(typeof(SampleTranscriptDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody]SampleTranscriptPut model)
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
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(SampleTranscriptDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Insert([FromBody] SampleTranscriptPost model)
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
        [Route("")]
        [ProducesResponseType(typeof(AppDomainResult<SampleTranscriptDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] SearchOptions baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
