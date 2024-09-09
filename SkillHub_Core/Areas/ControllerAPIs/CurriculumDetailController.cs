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
    [Route("api/CurriculumDetail")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class CurriculumDetailController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await CurriculumDetailService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] CurriculumDetailCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CurriculumDetailService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody] CurriculumDetailUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CurriculumDetailService.Update(model, GetCurrentUser());
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
                await CurriculumDetailService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] CurriculumDetailSearch baseSearch)
        {
            var data = await CurriculumDetailService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalCurriculum = data.TotalRow, totalFile = data.Total, data = data.Data });
        }
        //[HttpPost]
        //[Route("file/{curriculumDetailId}")]
        //[ClaimsAuthorize]
        //public async Task<IActionResult> AddFile(int curriculumDetailId)
        //{
        //    try
        //    {
        //        var curriculumDetail = await CurriculumDetailService.GetById(curriculumDetailId);
        //        if (curriculumDetail == null)
        //            throw new Exception("Không tìm thấy danh mục");
        //        //Kiểm tra có tồn tại file nào của chương chưa thì gắn index
        //        string link = "";
        //        var httpContext = HttpContext.Current;
        //        var file = httpContext.Request.Files.Get("File");
        //        if (file != null)
        //        {
        //            string ext = Path.GetExtension(file.FileName).ToLower();
        //            string fileName = Guid.NewGuid() + ext; // getting File Name
        //            string fileExtension = Path.GetExtension(fileName).ToLower();
        //            var result = AssetCRM.isValIdDocument(ext); // ValIdate Header
        //            if (result)
        //            {
        //                fileName = Guid.NewGuid() + ext;
        //                var path = Path.Combine(httpContext.Server.MapPath("~/Upload/FileInCurriculum/"), fileName);
        //                string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
        //                string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
        //                link = strUrl + "Upload/FileInCurriculum/" + fileName;

        //                using (var stream = new FileStream(path, FileMode.Create))
        //                {
        //                    file.InputStream.CopyTo(stream);
        //                }

        //                //file.SaveAs(path);
        //                var model = new FileInCurriculumDetailCreate { CurriculumDetailId = curriculumDetailId, FileName = file.FileName, FileUrl = link};
        //                var data = await FileInCurriculumDetailService.Insert(model,GetCurrentUser());
        //                return StatusCode((int)HttpStatusCode.OK, new { data = data, message = ApiMessage.SAVE_SUCCESS });
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
        [HttpPost]
        [Route("file/{curriculumDetailId}")]
        [ClaimsAuthorize]
        public async Task<IActionResult> AddFile(int curriculumDetailId)
        {
            try
            {
                var curriculumDetail = await CurriculumDetailService.GetById(curriculumDetailId);
                if (curriculumDetail == null)
                    throw new Exception("Không tìm thấy danh mục");
                var file = Request.Form.Files.GetFile("File");
                string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                var upload = UploadConfig.UploadFile(file, baseUrl, "Upload/FileInCurriculum/");
                if (upload.Success)
                {
                    var model = new FileInCurriculumDetailCreate { CurriculumDetailId = curriculumDetailId, FileName = file.FileName, FileUrl = upload.Link };
                    var data = await FileInCurriculumDetailService.Insert(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { data = data, message = upload.Message });
                }
                else
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = upload.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPut]
        [Route("file")]
        public async Task<IActionResult> UpdateFile([FromBody] FileInCurriculumDetailUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await FileInCurriculumDetailService.Update(model, GetCurrentUser());
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
        [Route("file/{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                await FileInCurriculumDetailService.Delete(fileId);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("file/{curriculumDetailId}")]
        public async Task<IActionResult> GetFile(int curriculumDetailId)
        {
            var data = await FileInCurriculumDetailService.GetByCurriculumDetail(curriculumDetailId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPut]
        [Route("CurriculumDetailIndex")]
        public async Task<IActionResult> UpdateCurriculumDetailIndex([FromBody] List<CurriculumDetailUpdate> request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CurriculumDetailService.UpdateCurriculumDetailIndex(request, GetCurrentUser());
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
        [Route("FileCurriculumDetailIndex")]
        public async Task<IActionResult> FileUpdateCurriculumDetailIndex([FromBody] List<FileInCurriculumDetailUpdate> request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await FileInCurriculumDetailService.FileUpdateCurriculumDetailIndex(request, GetCurrentUser());
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
    }
}
