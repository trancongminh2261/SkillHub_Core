using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.SampleTranscript;
using LMSCore.DTO.SaveGradesInClass;
using LMSCore.Models;
using LMSCore.Services.SaveGradesInClass;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LMSCore.Areas.ControllerAPIs.SaveGradesInClass
{
    [Route("api/SaveGradesInClass")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class SaveGradesInClassController : BaseController
    {
        private lmsDbContext dbContext;
        private SaveGradesInClassService domainService;
        public SaveGradesInClassController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new SaveGradesInClassService(this.dbContext);
        }
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(IList<SaveGradesInClassDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Insert([FromBody] List<SaveGradesInClassPost> model)
        {
            try
            {
                var data = await domainService.InsertOrUpdate(model, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(AppDomainResult<SaveGradesInClassDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] SaveGradesInClassSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
