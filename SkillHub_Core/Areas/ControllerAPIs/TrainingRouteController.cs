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
using static LMSCore.Services.TrainingRouteService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/TrainingRoute")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class TrainingRouteController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await TrainingRouteService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]TrainingRouteCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await TrainingRouteService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody]TrainingRouteUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await TrainingRouteService.Update(model, GetCurrentUser());
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
                await TrainingRouteService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] SearchOptions baseSearch)
        {
            var data = await TrainingRouteService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("form")]
        public async Task<IActionResult> GetTrainingRouteForm([FromQuery] TrainingRouteFormParam baseSearch)
        {
            var student = GetCurrentUser();
            var data = await TrainingRouteService.GetTrainingRouteForm(baseSearch,student);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            double percentComplete = await TrainingRouteService.PercentComplete(student.UserInformationId, baseSearch.TrainingRouteId);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data, percentComplete = percentComplete });
        }
    }
}
