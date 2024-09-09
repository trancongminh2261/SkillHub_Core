using ExcelDataReader;
using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
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
using System.Web;

using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.SectionService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [ValidateModelState]
    public class SectionController : BaseController
    {
        [HttpPost]
        [Route("api/Section")]
        public async Task<IActionResult> Insert([FromBody]SectionCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await SectionService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody]SectionUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await SectionService.Update(model, GetCurrentUser());
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
                await SectionService.Delete(id);
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
                await SectionService.ChangeIndex(model);
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
                var data = await SectionService.GetByVideoCourse(videoCourseId, GetCurrentUser());
                if (!data.Any())
                    return StatusCode((int)HttpStatusCode.NoContent);
                double complete = await SectionService.GetComplete(videoCourseId,GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data, complete = complete });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
            
        }
    }
}
