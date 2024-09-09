using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services.StudentDevice;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LMSCore.Areas.ControllerAPIs.StudentDevice
{
    [ValidateModelState]
    [Route("api/StudentDevice")]
    [ClaimsAuthorize]
    public class StudentDeviceController : BaseController
    {
        private lmsDbContext dbContext;
        private StudentDeviceService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public StudentDeviceController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new StudentDeviceService(this.dbContext, _hostingEnvironment);
        }

        [HttpGet]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetAll([FromQuery] GetStudentDeviceLimitSearch baseSearch)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
                var data = await StudentDeviceService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpGet]
        [Route("device-config")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetDeviceConfig()
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await StudentDeviceService.GetDeviceConfig();
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("device-config-status")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetDeviceConfigStatus()
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await StudentDeviceService.GetDeviceConfigStatus();
            if (data.Count == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPut]
        [Route("device-config")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Update([FromBody] DeviceConfigUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentDeviceService.UpdateDeviceConfig(model);
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

        [HttpPost]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Insert([FromBody] StudentDeviceCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await StudentDeviceService.Insert(model);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !"});
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
        [ClaimsAuthorize]
        public async Task<IActionResult> Update([FromBody] StudentDeviceUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentDeviceService.Update(model, GetCurrentUser());
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
        [ClaimsAuthorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await StudentDeviceService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
