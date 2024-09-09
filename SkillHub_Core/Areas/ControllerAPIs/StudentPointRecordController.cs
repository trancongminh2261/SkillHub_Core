using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/StudentPointRecord")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class StudentPointRecordController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        [Description("Lấy 1 record")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await StudentPointRecordService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("")]
        [Description("Lấy bảng điểm nhiều học viên")]
        public async Task<IActionResult> GetAll([FromQuery] StudentPointRecordSearch baseSearch)
        {
            var data = await StudentPointRecordService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent, new { message = "Không có dữ liệu!" });
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, ResultMessage = data.ResultMessage, data = data.Data });
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]StudentPointRecordCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentPointRecordService.Insert(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.Count(), data });
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
        public async Task<IActionResult> Update([FromBody]StudentPointRecordUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentPointRecordService.Update(model, GetCurrentUser());
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
        [Route("{recordId}")]
        [Description("Xóa 1 record")]
        public async Task<IActionResult> Delete(int recordId)
        {
            try
            {
                await StudentPointRecordService.Delete(recordId);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }


        [HttpPut]
        [ClaimsAuthorize()]
        [Route("{StudentPointRecordId}")]
        public async Task<IActionResult> StudentPointRecordToPdf(int StudentPointRecordId)
        {
            try
            {
                var httpContext = HttpContext;
                var _hostingEnvironment = WebHostEnvironment.Environment;
                var path = Path.Combine(_hostingEnvironment.ContentRootPath, "Upload");
                var pathViews = Path.Combine(_hostingEnvironment.ContentRootPath, "Views");
                string strPathAndQuery = httpContext.Request.Path + httpContext.Request.QueryString;
                var fileUrl = $"{Request.Scheme}://{Request.Host}";
                if (fileUrl.IndexOf("https") == -1)
                    fileUrl = fileUrl.Replace("http", "https");
                var data = await StudentPointRecordService.StudentReportHtmlToPdf(StudentPointRecordId, path, pathViews, fileUrl);
                return StatusCode((int)HttpStatusCode.OK, new
                {
                    message = "Thành công",
                    data
                });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
