using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/StudentInTraining")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class StudentInTrainingController : BaseController
    {
        [HttpGet]
        [Route("doing-test")]
        public async Task<IActionResult> GetTrainingDoingTest([FromQuery] TrainingDoingTestSearch baseSearch)
        {
            var data = await StudentInTrainingService.GetTrainingDoingTest(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data.Data, totalRow = data.TotalRow });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]StudentInTrainingCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentInTrainingService.Insert(model, GetCurrentUser());
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
                await StudentInTrainingService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] StudentInTrainingSearch baseSearch)
        {
            var data = await StudentInTrainingService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
