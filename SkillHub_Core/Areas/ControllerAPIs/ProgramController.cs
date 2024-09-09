using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Services;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Program")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ProgramController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await ProgramService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] ProgramCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ProgramService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody] ProgramUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ProgramService.Update(model, GetCurrentUser());
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
        [Route("change-index")]
        public async Task<IActionResult> ChangeIndex([FromBody] ChangeIndexItem model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ProgramService.SwapRollIndex(model, GetCurrentUser());
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
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await ProgramService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] ProgramSearch baseSearch)
        {
            var data = await ProgramService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        /// <summary>
        /// Lấy danh sách giáo viên được phép dậy chương trình này
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("teacher-in-program")]
        public async Task<IActionResult> GetTeacherInProgram([FromQuery] TeacherInProgramSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new TeacherInProgramSearch();
            var validateBranch = await BranchService.ValidateBranch(GetCurrentUser(), baseSearch.BranchIds);
            if (!validateBranch)
                return StatusCode(403, new { message = "Bạn không có quyền truy cập chi nhánh này !" });
            var data = await ProgramService.GetTeacherInProgram(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// Lấy danh sách trợ giảng được phép dậy chương trình này
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("tutor-in-program")]
        public async Task<IActionResult> GetTutorInProgram([FromQuery] TeacherInProgramSearch baseSearch)
        {
            var data = await ProgramService.GetTutorInProgram(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpPut]
        [Route("{programId}/allow-teacher/{teacherId}")]
        public async Task<IActionResult> AllowTeacher(int teacherId, int programId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ProgramService.AllowTeacher(teacherId, programId, GetCurrentUser());
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
        [Route("allow-teacher-v2")]
        public async Task<IActionResult> TeacherInProgram([FromBody] TeacherInProgramUpdate baseUpdate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ProgramService.AllowTeacherV2(baseUpdate, GetCurrentUser());
                    return StatusCode(((int)HttpStatusCode.OK), new { message = "Thành công !" });
                }
                catch (Exception e)
                {
                    return StatusCode(((int)HttpStatusCode.BadRequest), new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode(((int)HttpStatusCode.BadRequest), new { message = message });
        }

        [HttpPut]
        [Route("allow-list-teacher/{programId}")]
        public async Task<IActionResult> AllowListTeacher([FromBody] List<ListTeacherInProgram> baseUpdate, int programId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ProgramService.AllowListTeacher(baseUpdate, programId, GetCurrentUser());
                    return StatusCode(((int)HttpStatusCode.OK), new { message = "Thành công !" });
                }
                catch (Exception e)
                {
                    return StatusCode(((int)HttpStatusCode.BadRequest), new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode(((int)HttpStatusCode.BadRequest), new { message = message });
        }
        [HttpGet]
        [Route("dropdown/{gradeId}")]
        public async Task<IActionResult> GetDropdown(int gradeId)
        {
            var data = await ProgramService.GetDropdown(gradeId);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
