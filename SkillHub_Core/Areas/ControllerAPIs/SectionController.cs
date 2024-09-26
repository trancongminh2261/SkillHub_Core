using ExcelDataReader;
using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.Models;
using LMS_Project.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using LMSCore.Users;
using Microsoft.AspNetCore.Mvc;
using static LMS_Project.Services.SectionService;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class SectionController : BaseController
    {
        private lmsDbContext dbContext;
        private SectionService domainService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public SectionController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.dbContext = new lmsDbContext();
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            this.domainService = new SectionService(this.dbContext, _httpContextAccessor, _configuration);
        }

        [HttpPost]
        [Route("api/Section")]
        public async Task<IActionResult> Insert([FromBody] SectionCreate model)
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
        [HttpPut]
        [Route("api/Section")]
        public async Task<IActionResult> Update([FromBody] SectionUpdate model)
        {
            if (ModelState.IsValid)
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
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        [HttpDelete]
        [Route("api/Section/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await domainService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("api/Section/ChangeIndex")]
        public async Task<IActionResult> ChangeIndex([FromBody] ChangeIndexModel model)
        {
            try
            {
                await domainService.ChangeIndex(model);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/Section/GetByVideoCourse/{videoCourseId}")]
        public async Task<IActionResult> GetByVideoCourse(int videoCourseId)
        {
            try
            {
                var data = await domainService.GetByVideoCourse(videoCourseId, GetCurrentUser());
                if (!data.Any())
                    return StatusCode((int)HttpStatusCode.NoContent);
                double complete = await domainService.GetComplete(videoCourseId,GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data, complete = complete });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
