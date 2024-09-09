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
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.ClassRegistrationService;
using LMSCore.Utilities;
using Microsoft.EntityFrameworkCore;
using LMSCore.Services.Bill;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/ClassRegistration")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ClassRegistrationController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await ClassRegistrationService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("class-available")]
        public async Task<IActionResult> GetClassAvailable([FromQuery] ClassAvailableSearch baseSearch)
        {
            try
            {
                var data = await ClassRegistrationService.GetClassAvailable(baseSearch);
                if (!data.Any())
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        /// <summary>
        /// Chuyển học viên vào lớp học
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("add-to-class")]
        public async Task<IActionResult> AddToClass([FromBody] AddToClassModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ClassRegistrationService.AddToClass(model, GetCurrentUser());
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
        [Route("")]
        public async Task<IActionResult> Update([FromBody] ClassRegistrationUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassRegistrationService.Update(model, GetCurrentUser());
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
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] ClassRegistrationSearch baseSearch)
        {
            var data = await ClassRegistrationService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("program-registration")]
        public async Task<IActionResult> GetProgramRegistration([FromQuery] ProgramRegistrationSearch baseSearch)
        {
            var data = await ClassRegistrationService.GetProgramRegistration(baseSearch, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("schedule-registration")]
        public async Task<IActionResult> GetScheduleRegistration([FromQuery] ScheduleRegistrationSearch baseSearch)
        {
            var data = await ClassRegistrationService.GetScheduleRegistration(baseSearch, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("student-registration")]
        public async Task<IActionResult> GetStudentRegistration([FromQuery] StudentRegistrationSearch baseSearch)
        {
            var data = await ClassRegistrationService.GetStudentRegistration(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpPost]
        [Route("swap-expectations")]
        public async Task<IActionResult> SwapExpectations([FromBody] SwapExpectationCreate model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new lmsDbContext())
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            await ClassRegistrationService.SwapExpectations(model, GetCurrentUser(), db);
                            tran.Commit();
                            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
                        }
                        catch (Exception e)
                        {
                            tran.Rollback();
                            return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                        }
                    }
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
    }
}
