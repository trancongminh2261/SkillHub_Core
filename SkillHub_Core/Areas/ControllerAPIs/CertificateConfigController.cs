using ExcelDataReader;
using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.Models;
using LMS_Project.Services;
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
using LMSCore.Users;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Areas.ControllerAPIs;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class CertificateConfigController : BaseController
    {
        [HttpGet]
        [Route("api/CertificateConfig/GetGuide")]
        public async Task<IActionResult> GetGuide()
        {
            var data = await CertificateConfigService.GetGuide();
            if (data.Any())
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPost]
        [Route("api/CertificateConfig")]
        public async Task<IActionResult> Insert([FromBody] CertificateConfigCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CertificateConfigService.Insert(model, GetCurrentUser());
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
        [Route("api/CertificateConfig")]
        public async Task<IActionResult> Update([FromBody] CertificateConfigUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CertificateConfigService.Update(model, GetCurrentUser());
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
        [Route("api/CertificateConfig/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await CertificateConfigService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/CertificateConfig/{id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var data = await CertificateConfigService.GetById(Id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpGet]
        [Route("api/CertificateConfig")]
        public async Task<IActionResult> GetAll([FromQuery] SearchOptions search)
        {
            var data = await CertificateConfigService.GetAll(search);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
