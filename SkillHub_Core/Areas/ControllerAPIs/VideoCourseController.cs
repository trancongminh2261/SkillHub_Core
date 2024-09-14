using ExcelDataReader;
using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.Models;
using LMS_Project.Services;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.Users;
using Microsoft.AspNetCore.Mvc;
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

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class VideoCourseController : BaseController
    {
        [HttpPost]
        [Route("api/VideoCourse")]
        public async Task<IActionResult> Insert([FromBody] VideoCourseCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await VideoCourseService.Insert(model, GetCurrentUser());
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
        [Route("api/VideoCourse")]
        public async Task<IActionResult> Update([FromBody] VideoCourseUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await VideoCourseService.Update(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !",data });
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
        [Route("api/VideoCourse/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await VideoCourseService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/VideoCourse")]
        public async Task<IActionResult> GetAll([FromQuery] VideoCourseSearch search)
        {
            var data = await VideoCourseService.GetAll(search,GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("api/VideoCourse/v2")]
        public async Task<IActionResult> GetAllV2([FromQuery] VideoCourseSearchV2 search)
        {
            var data = await VideoCourseService.GetAllV2(search, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("api/VideoCourse/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await VideoCourseService.GetById(id,GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("api/VideoCourse/Overview/{videoCourseId}")]
        public async Task<IActionResult> GetVideoCourseOverview(int videoCourseId)
        {
            var data = await VideoCourseService.GetVideoCourseOverview(videoCourseId);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("api/VideoCourse/list-id-detail/{videoCourseId}")]
        public async Task<IActionResult> GetListIdDetail(int videoCourseId)
        {
            var data = await VideoCourseService.GetListIdDetail(videoCourseId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("api/VideoCourse/status")]
        public async Task<IActionResult> GetStatus([FromQuery] VideoCourseSearchV2 search)
        {
            var data = await VideoCourseService.GetStatus(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
