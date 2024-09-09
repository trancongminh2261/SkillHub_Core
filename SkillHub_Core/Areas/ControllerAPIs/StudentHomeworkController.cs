using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/StudentHomework")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class StudentHomeworkController : BaseController
    {
        private lmsDbContext dbContext;
        private StudentHomeworkService domainService;
        public StudentHomeworkController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new StudentHomeworkService(this.dbContext);
        }
       
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] StudentHomeworkSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data, totalRow = data[0].TotalRow, });
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

    }
}
