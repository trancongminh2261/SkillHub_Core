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
using System.Web.Http.Description;
using static LMS_Project.Services.ExerciseGroupService;
using LMSCore.Areas.ControllerAPIs;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class ExerciseGroupController : BaseController
    {
        [HttpPost]
        [Route("api/ExerciseGroup")]
        public async Task<IActionResult> Insert([FromBody] ExerciseGroupCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ExerciseGroupService.Insert(model, GetCurrentUser());
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
        [HttpPut]
        [Route("api/ExerciseGroup")]
        public async Task<IActionResult> Update([FromBody] ExerciseGroupUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ExerciseGroupService.Update(model, GetCurrentUser());
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
        [HttpGet]
        [Route("api/ExerciseGroup")]
        [ResponseType(typeof(List<ExerciseGroupModel>))]
        public async Task<IActionResult> GetAll([FromQuery] ExerciseGroupSearch search)
        {
            var data = await ExerciseGroupService.GetAll(search);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpDelete]
        [Route("api/ExerciseGroup/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await ExerciseGroupService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("api/ExerciseGroup/ChangeIndex")]
        public async Task<IActionResult> ChangeIndex([FromBody] ExerciseGroupIndexModel model)
        {
            try
            {
                await ExerciseGroupService.ChangeIndex(model);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
