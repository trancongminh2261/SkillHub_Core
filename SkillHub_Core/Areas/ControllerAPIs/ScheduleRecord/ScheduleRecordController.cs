using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System;
using LMSCore.Services.ScheduleRecord;

namespace LMSCore.Areas.ControllerAPIs.ScheduleRecord
{
    [Route("api/ScheduleRecord")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ScheduleRecordController : BaseController
    {
        private lmsDbContext dbContext;
        private ScheduleRecordService domainService;
        public ScheduleRecordController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new ScheduleRecordService(this.dbContext);
        }
        [HttpPost]
        [Route("UploadRecord")]
        public IActionResult UploadRecord(IFormFile file)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            var data = UploadConfig.UploadFile(file, baseUrl, "Upload/ScheduleRecord/");
            if (data.Success)
                return Ok(new { data = data.Link, message = data.Message });
            else
                return BadRequest(new { message = data.Message });
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
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] ScheduleRecordSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.Count <= 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] ScheduleRecordCreate baseCreate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Insert(baseCreate, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody] ScheduleRecordUpdate model)
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
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (ModelState.IsValid)
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
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
    }
}
