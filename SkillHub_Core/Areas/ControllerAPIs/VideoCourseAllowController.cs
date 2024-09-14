using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.DTO;
using LMS_Project.DTO.OptionDTO;
using LMS_Project.Models;
using LMS_Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.Users;
using LMSCore.Utilities;
using LMSCore.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [Route("api/VideoCourseAllow")]
    [ValidateModelState]
    public class VideoCourseAllowController : BaseController
    {
        private lmsDbContext dbContext;
        private VideoCourseAllowService domainService;
        public VideoCourseAllowController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new VideoCourseAllowService(this.dbContext);
        }
        [HttpGet]
        [Route("list-allow")]
        [SwaggerResponse(200, "OK", typeof(IList<VideoCourseAllowAvailableDTO>))]
        public async Task<IActionResult> GetAllowAvailable([FromQuery] VideoCourseAllowAvaibleSearch baseSeach)
        {
            var data = await domainService.GetAllowAvailable(baseSeach);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPut]
        [Route("delete-multi")]
        public async Task<IActionResult> DeleteMulti([FromBody] List<int> ListId)
        {
            try
            {
                await domainService.DeleteMulti(ListId, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !"});
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPost]
        [Route("")]
        [SwaggerResponse(200, "OK", typeof(IList<VideoCourseAllowDTO>))]
        public async Task<IActionResult> Insert([FromBody] VideoCourseAllowCreate model)
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
        [HttpGet]
        [Route("")]
        [SwaggerResponse(200, "OK", typeof(AppDomainResult<VideoCourseAllowDTO>))]
        public async Task<IActionResult> GetAll([FromQuery] VideoCourseAllowSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
