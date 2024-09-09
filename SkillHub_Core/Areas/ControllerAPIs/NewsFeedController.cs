using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/NewsFeed")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class NewsFeedController : BaseController
    {
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] NewsFeedCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await NewsFeedService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody] NewsFeedUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await NewsFeedService.Update(model, GetCurrentUser());
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
            try
            {
                await NewsFeedService.Delete(id, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] NewsFeedSearch baseSearch)
        {
            var data = await NewsFeedService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await NewsFeedService.GetById(id, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data = data });
        }
        /// <summary>
        /// Định dạng file của NewsFeed : jpg,jpeg,png,bmp,mp4,flv,mpeg,mov,mp3
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("Upload")]
        public IActionResult UploadFile(IFormFile file)
        {
            try
            {
                string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                var upload = UploadConfig.UploadFile(file, baseUrl, "Upload/FileInNewsFeed/");
                if (upload.Success)
                {
                    return StatusCode((int)HttpStatusCode.OK, new {
                        data = upload.Link,
                        message = upload.Message });
                }
                else
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = upload.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("Pin")]
        public async Task<IActionResult> Pin(int id)
        {
            try
            {
                await NewsFeedService.Pin(id, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

    }
}
