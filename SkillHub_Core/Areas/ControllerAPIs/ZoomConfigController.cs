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
using LMSCore.Areas.ControllerAPIs;


namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class ZoomConfigController : BaseController
    {
        [HttpPost]
        [Route("api/ZoomConfig")]
        public async Task<IActionResult> Insert([FromBody] ZoomConfigCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ZoomConfigService.Insert(model, GetCurrentUser());
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
        [Route("api/ZoomConfig")]
        public async Task<IActionResult> Update([FromBody] ZoomConfigUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ZoomConfigService.Update(model, GetCurrentUser());
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
        [Route("api/ZoomConfig/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await ZoomConfigService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/ZoomConfig/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await ZoomConfigService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("api/ZoomConfig")]
        public async Task<IActionResult> GetAll()
        {
            var data = await ZoomConfigService.GetAll();
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !",data = data });
        }
    }
}
