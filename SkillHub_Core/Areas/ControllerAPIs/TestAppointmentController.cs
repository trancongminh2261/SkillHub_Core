using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using LMSCore.Utilities;


namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/TestAppointment")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class TestAppointmentController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await TestAppointmentService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        //[HttpPost]
        //[Route("")]
        //public async Task<IActionResult> Insert([FromBody]TestAppointmentCreate model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var data = await TestAppointmentService.Insert(model, GetCurrentUser());
        //            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //        }
        //    }
        //    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //}
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody]TestAppointmentUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await TestAppointmentService.Update(model, GetCurrentUser());
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
                await TestAppointmentService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        /// <summary>
        /// Nhân viên thuộc trung nào chỉ thấy học viên ở trung tâm đó, Tư vấn viên chỉ thấy khách hàng của chính mình
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] TestAppointmentSearch baseSearch)
        {
            var data = await TestAppointmentService.GetAll(baseSearch,GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        /// <summary>
        /// upload file
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadFile")]
        public IActionResult UploadFile()
        {
            try
            {
                var file = HttpContext.Request.Form.Files.GetFile("File");
                string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                var upload = UploadConfig.UploadFile(file, baseUrl, "Upload/FileInTestAppointment/");
                if (upload.Success)
                {
                    return StatusCode((int)HttpStatusCode.OK, new { data = upload.Link, message = upload.Message });
                }
                else
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = upload.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //public IActionResult UploadFile()
        //{
        //    try
        //    {
        //        string link = "";
        //        var httpContext = HttpContext.Current;
        //        var file = httpContext.Request.Files.Get("File");
        //        if (file != null)
        //        {
        //            string ext = Path.GetExtension(file.FileName).ToLower();
        //            string fileName = Guid.NewGuid() + ext; // getting File Name
        //            string fileExtension = Path.GetExtension(fileName).ToLower();
        //            //var result = AssetCRM.IsValidDocument(ext); // Validate Header
        //            var result = AssetCRM.isValIdDocument(ext);
        //            if (result)
        //            {
        //                fileName = Guid.NewGuid() + ext;
        //                var path = Path.Combine(httpContext.Server.MapPath("~/Upload/FileInTestAppointment/"), fileName);
        //                string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
        //                string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
        //                link = strUrl + "Upload/FileInTestAppointment/" + fileName;
        //                file.SaveAs(path);
        //                if (!link.Contains("https"))
        //                    link = link.Replace("http", "https");
        //                return StatusCode((int)HttpStatusCode.OK, new { data = link, message = ApiMessage.SAVE_SUCCESS });
        //            }
        //            else
        //            {
        //                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.INVALID_FILE });
        //            }
        //        }
        //        else
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.NOT_FOUND });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
        //    }
        //}

        /// <summary>
        /// đồng bộ dữ liệu ( bùa chú )
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SyncData")]
        public async Task<IActionResult> SyncData()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await TestAppointmentService.SyncData();
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!" });

                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
    }
}
