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

using static LMS_Project.Services.ExamSectionService;
using LMSCore.Areas.ControllerAPIs;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class ExamSectionController : BaseController
    {
        [HttpPost]
        [Route("api/ExamSection")]
        public async Task<IActionResult> Insert([FromBody] ExamSectionCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ExamSectionService.Insert(model, GetCurrentUser());
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
        [Route("api/ExamSection")]
        public async Task<IActionResult> Update([FromBody] ExamSectionUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ExamSectionService.Update(model, GetCurrentUser());
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
        [Route("api/ExamSection/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await ExamSectionService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("api/ExamSection/ChangeSectionIndex")]
        public async Task<IActionResult> ChangeSectionIndex([FromBody] SectionIndexModel model)
        {
            try
            {
                await ExamSectionService.ChangeSectionIndex(model);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
