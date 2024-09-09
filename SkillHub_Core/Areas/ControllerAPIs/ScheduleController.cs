using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
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
using static LMSCore.Services.Schedule.ScheduleService;
using Microsoft.AspNetCore.Hosting;
using LMSCore.Services.Schedule;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Schedule")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ScheduleController : BaseController
    {
        private lmsDbContext dbContext;
        private ScheduleService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public ScheduleController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new ScheduleService(this.dbContext, _hostingEnvironment);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await ScheduleService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] ScheduleCreate model)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var data = await ScheduleService.Insert(model, GetCurrentUser(), db);
                        tran.Commit();
                        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                    }
                }
            }
        }

        [HttpPost]
        [Route("validate")]
        public async Task<IActionResult> Validate([FromBody] ScheduleCreate model)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        await ScheduleService.Validate(model, GetCurrentUser(), db);
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

        [HttpPost]
        [Route("generate-schedule")]
        public async Task<IActionResult> GenerateSchedule([FromBody] GenerateScheduleCreate model)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await ScheduleService.GenerateSchedule(model);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
        }

        [HttpPost]
        [Route("multiple")]
        public async Task<IActionResult> MultipleInsert([FromBody] MultipleScheduleCreate request)
        {

            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        await ScheduleService.MultipleInsert(request, GetCurrentUser(), db);
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

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] ScheduleUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ScheduleService.Update(model, GetCurrentUser());
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
        [Route("v2")]
        public async Task<IActionResult> UpdateV2([FromBody] ScheduleUpdateV2 model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ScheduleService.UpdateV2(model, GetCurrentUser());
                    return StatusCode(((int)HttpStatusCode.OK), new { message = "Thành công !", data });
                }
                catch (Exception e)
                {
                    return StatusCode(((int)HttpStatusCode.BadRequest), new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode(((int)HttpStatusCode.BadRequest), new { message = message });
        }
        /// <summary>
        /// Hủy lịch dạy kèm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("tutoring-cancel/{id}")]
        public async Task<IActionResult> TutoringCancel(int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ScheduleService.TutoringCancel(id, GetCurrentUser());
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
                await ScheduleService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpDelete]
        [Route("ByClass/{classId}")]
        public async Task<IActionResult> DeleteByClass(int classId)
        {
            try
            {
                await ScheduleService.DeleteByClass(classId);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        /// <summary>
        /// Xem lịch
        /// </summary>
        /// <remarks>
        /// Xem lịch theo lớp truyền classId <br></br>
        /// Xem lịch học của học viên và lịch dạy của giáo viên api tự bắt theo token  <br></br>
        /// Kiểm tra lịch trung tâm và giáo viên BranchIds, TeacherIds  <br></br>
        /// </remarks>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] ScheduleSearch baseSearch)
        {
            var data = await ScheduleService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpPut]
        [Route("rate-teacher")]
        public async Task<IActionResult> RateTeacher([FromBody] RateTeacherModel itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ScheduleService.RateTeacher(itemModel, GetCurrentUser());
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
        [Route("schedule-expected")]
        public async Task<IActionResult> GetAllScheduleExpected([FromQuery] ExpectedSheduleSearch baseSearch)
        {
            var data = await ScheduleService.GetAllScheduleExpected(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            if (data.IsActive == true)
                // Data là danh sách buổi học giáo viên đã điểm danh
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, totalDayTeacherAttendance = data.TotalRow, data = data.Data });
            // Data là danh sách tất cả buổi học của giáo viên
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, totalScheduleOfTeacher = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// Lấy danh sách giáo viên có lịch trống theo từng ca học và từng ngày
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("available-teacher-studyTime")]
        public async Task<IActionResult> GetAvailableTeachersFromStudyTime([FromQuery] TeacherAvailableScheduleSearch baseSearch)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await ScheduleService.GetAvailableTeachersFromStudyTime(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Lấy danh sách giáo viên có lịch trống theo 1 ca học ở 1 ngày
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("detail-available-teacher-studyTime")]
        public async Task<IActionResult> GetDetailAvailableTeachersFromStudyTime([FromQuery] DetailTeacherAvailableScheduleSearch baseSearch)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await ScheduleService.GetDetailAvailableTeachersFromStudyTime(baseSearch);
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// Lấy danh sách phòng học có lịch trống theo từng ca học và từng ngày
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("available-room-studyTime")]
        public async Task<IActionResult> GetAvailableRoomFromStudyTime([FromQuery] RoomAvailableScheduleSearch baseSearch)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await ScheduleService.GetAvailableRoomFromStudyTime(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Lấy danh sách phòng học có lịch trống theo 1 ca học ở 1 ngày
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("detail-available-room-studyTime")]
        public async Task<IActionResult> GetDetailAvailableRoomFromStudyTime([FromQuery] DetailRoomAvailableScheduleSearch baseSearch)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await ScheduleService.GetDetailAvailableRoomFromStudyTime(baseSearch);
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        #region phần zoom

        [HttpPost]
        [Route("create-zoom/{scheduleId}")]
        public async Task<IActionResult> CreateZoom(int scheduleId)
        {
            try
            {
                var data = await ScheduleService.CreateZoom(scheduleId, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpPut]
        [Route("close-zoom/{scheduleId}")]
        public async Task<IActionResult> CloseZoom(int scheduleId)
        {
            try
            {
                await ScheduleService.CloseZoom(scheduleId, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("recording/{scheduleId}")]
        public async Task<IActionResult> GetRecording(int scheduleId)
        {
            try
            {
                var data = await ScheduleService.GetRecording(scheduleId);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("zoom-room")]
        public async Task<IActionResult> GetZoomRoom([FromQuery] GetZoomSearch baseSearch)
        {
            var data = await ScheduleService.GetZoomRoom(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        #endregion
    }
}
