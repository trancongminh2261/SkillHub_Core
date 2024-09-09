using LMSCore.Areas.Request;
using LMSCore.DTO.ClassTranscriptDetail;
using LMSCore.Models;
using LMSCore.Services.ClassTranscriptDetail;
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

namespace LMSCore.Areas.ControllerAPIs.ClassTranscriptDetail
{
    [Route("api/ClassTranscriptDetail")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ClassTranscriptDetailController : BaseController
    {
        private lmsDbContext dbContext;
        private ClassTranscriptDetailService domainService;
        public ClassTranscriptDetailController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new ClassTranscriptDetailService(this.dbContext);
        }

        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType(typeof(ClassTranscriptDetailDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
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
        [HttpPut]
        [Route("")]
        [ProducesResponseType(typeof(ClassTranscriptDetailDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] ClassTranscriptDetailPut model)
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
        [ProducesResponseType(typeof(ClassTranscriptDetailDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Insert([FromBody] ClassTranscriptDetailPost model)
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
        [Route("by-class-transcript/{classTranscriptId}")]
        [ProducesResponseType(typeof(IList<ClassTranscriptDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByClassTranscript(int classTranscriptId)
        {
            var data = await domainService.GetByClassTranscript(classTranscriptId);
            if (data.Any())
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
    }
}
