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
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [ValidateModelState]
    public class TuitionPackageController : BaseController
    {
        private lmsDbContext dbContext;
        private TuitionPackageService TuitionPackageService;
        public TuitionPackageController()
        {
            this.dbContext = new lmsDbContext();
            this.TuitionPackageService = new TuitionPackageService(this.dbContext);
        }
        [HttpGet]
        [Route("api/TuitionPackage/{id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var data = await TuitionPackageService.GetById(Id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPut]
        [Route("api/TuitionPackage")]
        public async Task<IActionResult> Update([FromBody]TuitionPackageUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await TuitionPackageService.Update(model, GetCurrentUser());
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
        [Route("api/TuitionPackage")]
        public async Task<IActionResult> Insert([FromBody]TuitionPackageCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await TuitionPackageService.Insert(model, GetCurrentUser());
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
        [Route("api/TuitionPackage/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await TuitionPackageService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/TuitionPackage")]
        public async Task<IActionResult> GetAll([FromQuery] SearchOptions baseSearch)
        {
            var data = await TuitionPackageService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
