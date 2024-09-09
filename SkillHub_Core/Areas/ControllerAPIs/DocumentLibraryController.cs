using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
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
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/DocumentLibrary")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class DocumentLibraryController : BaseController
    {
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]DocumentLibraryCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await DocumentLibraryService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody]DocumentLibraryUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await DocumentLibraryService.Update(model, GetCurrentUser());
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

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] DocumentLibrarySearch baseSearch)
        {
            var data = await DocumentLibraryService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await DocumentLibraryService.GetById(id, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data = data });
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await DocumentLibraryService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        /// <summary>
        /// Định dạng file của document library
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FileInDocumentLibrary/UploadFile")]
        public IActionResult UploadFile()
        {
            try
            {
                var file = Request.Form.Files.GetFile("File");
                string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                var upload = UploadConfig.UploadFile(file, baseUrl, "Upload/DocumentLibrary/");
                if (upload.Success)
                {
                    var item = new ThumbnailModel();
                    item.FileName = file.FileName;
                    item.FileUrl = upload.Link;
                    item.FileUrlRead = upload.LinkReadFile;

                    return StatusCode((int)HttpStatusCode.OK, new { data = item, message = upload.Message });
                }
                else
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = upload.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        //public IActionResult UploadFile()
        //{
        //    try
        //    {
        //        ThumbnailModel item = new ThumbnailModel();
        //        var httpContext = HttpContext.Current;
        //        var file = httpContext.Request.Files.Get("File");
        //        if (file != null)
        //        {
        //            item.FileName = file.FileName;
        //            string ext = Path.GetExtension(file.FileName).ToLower();
        //            string fileName = Guid.NewGuid() + ext; // getting File Name
        //            string fileExtension = Path.GetExtension(fileName).ToLower();
        //            var result = AssetCRM.IsValidDocument(ext); // Validate Header                 
        //            if (result)
        //            {
        //                fileName = Guid.NewGuid() + ext;
        //                var path = Path.Combine(httpContext.Server.MapPath("~/Upload/DocumentLibrary/"), fileName);
        //                string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
        //                string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
        //                item.FileUrl = strUrl + "Upload/DocumentLibrary/" + fileName;

        //                using (var stream = new FileStream(path, FileMode.Create))
        //                {
        //                    file.InputStream.CopyTo(stream);
        //                }

        //                if (!item.FileUrl.Contains("https"))
        //                    item.FileUrl = item.FileUrl.Replace("http", "https");

        //                return StatusCode((int)HttpStatusCode.OK, new { data = item, message = ApiMessage.SAVE_SUCCESS });
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

    }
}
