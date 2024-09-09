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
using static LMSCore.Services.SalaryService;
using Microsoft.AspNetCore.Http;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Salary")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class SalaryController : BaseController
    {

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await SalaryService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]SalaryCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await SalaryService.Insert(model, GetCurrentUser());
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

        [HttpPost]
        [Route("salary-closing")]
        public async Task<IActionResult> SalaryClosing()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await SalaryService.SalaryClosing(GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !"});
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
        public async Task<IActionResult> Update([FromBody]SalaryUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await SalaryService.Update(model, GetCurrentUser());
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
        [Route("multiple")]
        public async Task<IActionResult> MultipleUpdate([FromBody] SalaryMultipleUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await SalaryService.MultipleUpdate(model, GetCurrentUser());
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
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] SalarySearch baseSearch)
        {
            var data = await SalaryService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new 
            { 
                message = "Thành công !",
                totalRow = data.TotalRow,
                data = data.Data,
                AllState = data.AllState,
                Unfinished = data.Unfinished, 
                Finished = data.Finished, 
                Paid = data.Paid });
        }

        [HttpGet]
        [Route("v2")]
        public async Task<IActionResult> GetAllV2([FromQuery] SalaryV2Search baseSearch)
        {
            var data = await SalaryService.GetAllV2(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                totalRow = data.TotalRow,
                data = data.Data,
                AllState = data.AllState,
                Unfinished = data.Unfinished,
                Finished = data.Finished,
                Paid = data.Paid
            });
        }

        [HttpGet]
        [Route("excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] SalaryV2Search baseSearch)
        {
            var data = await SalaryService.RepairDataToExport(baseSearch, GetCurrentUser());
            string folder = "Salary";
            string template = "Export_Salary.xlsx";
            string fileNameToSave = "DanhSachBangLuong";
            string baseUrl = Request.Scheme + "://" + Request.Host;
            var result = ExcelExportService.ExportV3(data, template, fileNameToSave, folder, baseUrl);
            return StatusCode((int)HttpStatusCode.OK, new { statusCode = (int)HttpStatusCode.OK, data = result });
        }

        [HttpGet]
        [Route("teaching-detail")]
        public async Task<IActionResult> GetTeachingDetail([FromQuery] TeachingDetailSearch baseSearch)
        {
            var data = await SalaryService.GetTeachingDetail(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }


        [HttpGet]
        [Route("user-available")]
        public async Task<IActionResult> GetUserAvailable([FromQuery] UserAvailableSearch baseSearch)
        {
            var data = await SalaryService.GetUserAvailable(baseSearch);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("user-available/v2")]
        public async Task<IActionResult> GetUserAvailableV2([FromQuery] UserAvailableV2Search baseSearch)
        {
            var data = await SalaryService.GetUserAvailableV2(baseSearch);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPost]
        [Route("salary-closing/v2")]
        public async Task<IActionResult> SalaryClosingV2()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await SalaryService.SalaryClosingV2(GetCurrentUser());
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
        [Route("salary-expected")]
        public async Task<IActionResult> GetAllSalaryExpected([FromQuery] ExpectedSalarySearch baseSearch)
        {
            var data = await SalaryService.GetAllSalaryExpected(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data});
        }
    }
}
