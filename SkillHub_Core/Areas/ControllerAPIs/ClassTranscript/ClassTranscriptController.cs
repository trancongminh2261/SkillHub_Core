using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.ClassTranscript;
using LMSCore.Models;
using LMSCore.Services.ClassTranscript;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading.Tasks;
using System;
using LMSCore.Users;
using LMSCore.Utilities;
using LMSCore.DTO.ClassTranscriptDetail;
using System.Collections.Generic;

namespace LMSCore.Areas.ControllerAPIs.ClassTranscript
{
    [Route("api/ClassTranscript")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ClassTranscriptController : BaseController
    {
        private lmsDbContext dbContext;
        private ClassTranscriptService domainService;
        public ClassTranscriptController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new ClassTranscriptService(this.dbContext);
        }
        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType(typeof(ClassTranscriptDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(ClassTranscriptDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Insert([FromBody] ClassTranscriptPost model)
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
        [HttpPut]
        [Route("")]
        [ProducesResponseType(typeof(ClassTranscriptDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] ClassTranscriptPut model)
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
        [ProducesResponseType(typeof(AppDomainResult<ClassTranscriptDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ClassTranscriptSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
