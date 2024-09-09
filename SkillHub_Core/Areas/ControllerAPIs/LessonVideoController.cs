using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Services.LessonVideoService;
using Microsoft.AspNetCore.Hosting;

namespace LMSCore.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [ValidateModelState]
    public class LessonVideoController : BaseController
    {
        private static IWebHostEnvironment _hostingEnvironment;
        public LessonVideoController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpPost]
        [Route("api/LessonVideo")]
        public async Task<IActionResult> Insert([FromBody]LessonVideoCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var pathViews = Path.Combine(_hostingEnvironment.ContentRootPath, "/Upload/FileInVideo/");
                    var data = await LessonVideoService.Insert(
                        model, 
                        GetCurrentUser(),
                        pathViews);
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
        [Route("api/LessonVideo")]
        public async Task<IActionResult> Update([FromBody]LessonVideoUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var pathViews = Path.Combine(_hostingEnvironment.ContentRootPath, "/Upload/FileInVideo/");
                    var data = await LessonVideoService.Update(
                        model, 
                        GetCurrentUser(),
                        pathViews);
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
        [Route("api/LessonVideo/{id}")] 
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await LessonVideoService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpPut]
        [Route("api/LessonVideo/ChangeIndex")]
        public async Task<IActionResult> LessonVideoChangeIndex([FromBody] ChangeLessonIndexModel model)
        {
            try
            {
                await LessonVideoService.ChangeIndex(model);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("api/LessonVideo/GetBySection/{sectionId}")]
        public async Task<IActionResult> GetBySection(int sectionId)
        {
            var data = await LessonVideoService.GetBySection(sectionId,GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("api/FileInVideo")]
        public async Task<IActionResult> InsertFileInVideo([FromBody] FileInVideoCreate fileInVideoCreate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await FileInVideoService.Insert(fileInVideoCreate, GetCurrentUser());
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
        [Route("api/FileInVideo/{id}")]
        public async Task<IActionResult> DeleteFileInVideo(int id)
        {
            try
            {
                await FileInVideoService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/FileInVideo/GetByLesson/{lessonVideoId}")]
        public async Task<IActionResult> GetByLesson(int lessonVideoId)
        {
            var data = await FileInVideoService.GetByLesson(lessonVideoId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("api/FileInVideo/UploadFile")]
        public IActionResult UploadFile()
        {
            try
            {
                var file = HttpContext.Request.Form.Files.GetFile("File");
                string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                var upload = UploadConfig.UploadFile(file, baseUrl, "Upload/FileInVideo/");
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
        //            var result = AssetCRM.IsValidDocument(ext); // Validate Header
        //            if (result)
        //            {
        //                fileName = Guid.NewGuid() + ext;
        //                var path = Path.Combine(httpContext.Server.MapPath("~/Upload/FileInVideo/"), fileName);
        //                string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
        //                string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
        //                link = strUrl + "Upload/FileInVideo/" + fileName;
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
        [HttpGet]
        [Route("api/FileInVideo/UploadFile/File")]
        public IActionResult GetFileInVideoUpload()
        {
            string result = "jpg,jpeg,png" +
                ",bmp" +
                ",mp4" +
                ",flv" +
                ",mpeg" +
                ",mov" +
                ",mp3" +
                ",doc" +
                ",docx" +
                ",pdf" +
                ",csv" +
                ",xlsx" +
                ",xls" +
                ",ppt" +
                ",pptx" +
                ",zip" +
                ",rar";
            return StatusCode((int)HttpStatusCode.OK, new { data = result, message = "Thành công" });
        }
        [HttpPost]
        [Route("api/LessonVideo/Completed/{lessonVideoId}")]
        public async Task<IActionResult> Completed(int lessonVideoId)
        {
            try
            {
                await LessonVideoService.Completed(lessonVideoId, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        //[HttpPost]
        //[Route("api/LessonVideo/test")]
        //public async Task<IActionResult> test()
        //{
        //    try
        //    {
        //        var httpContext = HttpContext.Current;
        //        double time = 0;
        //        var player = new WindowsMediaPlayer();
        //        var clip = player.newMedia($"{httpContext.Server.MapPath("~/Upload/Mau/")}/test.wav");
        //        time = clip.duration;
        //        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", time });
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //    }
        //}
    }
}
