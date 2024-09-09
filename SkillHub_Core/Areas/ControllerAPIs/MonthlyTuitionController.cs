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
using static LMSCore.Services.MonthlyTuitionService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [ValidateModelState]
    public class MonthlyTuitionController : BaseController
    {
        private lmsDbContext dbContext;
        private MonthlyTuitionService MonthlyTuitionService;
        public MonthlyTuitionController()
        {
            this.dbContext = new lmsDbContext();
            this.MonthlyTuitionService = new MonthlyTuitionService(this.dbContext);
        }
        [HttpGet]
        [Route("api/MonthlyTuition/{id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var data = await MonthlyTuitionService.GetById(Id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPost]
        [Route("api/MonthlyTuition/items")]
        public async Task<IActionResult> AddItems([FromBody] AddMonthlyTuitionModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await MonthlyTuitionService.AddItems(model, GetCurrentUser());
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
        [Route("api/MonthlyTuition")]
        public async Task<IActionResult> GetAll([FromQuery] MonthlyTuitionSearch baseSearch)
        {
            var data = await MonthlyTuitionService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
