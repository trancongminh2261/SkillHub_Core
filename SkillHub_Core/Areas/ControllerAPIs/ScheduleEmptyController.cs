using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Services.ScheduleEmptyService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/ScheduleEmpty")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ScheduleEmptyController : BaseController
    {
        private lmsDbContext dbContext;
        private ScheduleEmptyService domainService;
        public ScheduleEmptyController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new ScheduleEmptyService(this.dbContext);
        }
        [HttpGet]
        [Route("teacher-schedule-empty")]
        public async Task<IActionResult> GetTeacherScheduleEmpty([FromQuery] TeacherScheduleEmptySearch baseSearch  )
        {
            try { 
            var data = await domainService.GetTeacherScheduleEmpty(baseSearch);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
            }catch(Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message});
            }
        }
        [HttpGet]
        [Route("room-schedule-empty")]
        public async Task<IActionResult> GetRoomScheduleEmpty([FromQuery] RoomScheduleEmptySearch baseSearch)
        {
            var data = await domainService.GetRoomScheduleEmpty(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("teacher-schedule-statistic")]
        public async Task<IActionResult> GetStatisticTeacherSchedule([FromQuery] TeacherScheduleStatistic baseSearch)
        {
            var data = await domainService.GetStatisticTeacherSchedule(baseSearch);
            if (data.TotalRow==0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !",totalSchedule= data.TotalRow, data = data.Data });
        }
    }
}