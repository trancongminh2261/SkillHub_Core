
using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.DTO;
using LMS_Project.Models;
using LMS_Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Users;
using LMSCore.Utilities;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [Route("api/VideoConfig")]
    [ValidateModelState]
    public class VideoConfigController : BaseController
    {
        private lmsDbContext dbContext;
        private VideoConfigService domainService;
        public VideoConfigController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new VideoConfigService(this.dbContext);
        }
        [HttpGet]
        [Route("{id:int}")]
        [SwaggerResponse(200, "OK", typeof(VideoConfigDTO))]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPut]
        [Route("")]
        [SwaggerResponse(200, "OK", typeof(VideoConfigDTO))]
        public async Task<IActionResult> Update([FromBody] VideoConfigUpdate model)
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
        [SwaggerResponse(200, "OK", typeof(VideoConfigDTO))]
        public async Task<IActionResult> Insert([FromBody] VideoConfigCreate model)
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
        [Route("by-lesson-video/{lessonVideoId}")]
        [SwaggerResponse(200, "OK", typeof(IList<VideoConfigDTO>))]
        public async Task<IActionResult> GetByLessonVideo(int lessonVideoId)
        {
            var data = await domainService.GetByLessonVideo(lessonVideoId);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        [HttpGet]
        [Route("detail/{id}")]
        [SwaggerResponse(200, "OK", typeof(VideoConfigDetailDTO))]
        public async Task<IActionResult> GetDetail(int id)
        {
            var data = await domainService.GetDetail(id);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
    }
}
