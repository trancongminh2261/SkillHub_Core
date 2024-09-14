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
using static LMS_Project.Services.LessonVideoService;
using System.Net.Http.Headers;
using System.Configuration;
using LMS_Project.DTO.ServerDownload;
using Newtonsoft.Json;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.LMS;
using LMSCore.Models;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class LessonVideoController : BaseController
    {
        public static string serverDownload_Api_Key = ConfigurationManager.AppSettings["ServerDownload_API_Key"].ToString();
        public static string serverDownload_Video_Protection_Id = ConfigurationManager.AppSettings["ServerDownload_Video_Protection_Id"].ToString();
        [HttpPost]
        [Route("api/LessonVideo/AntiDownload/upload-video")]
        public async Task<IActionResult> UploadVideo()
        {
            var protectedValue = "true";
            var httpClient = new HttpClient();
            var httpRequest = HttpContext.Current.Request;
            var serverDownloadInfor = await LessonVideoService.GetDiskUsage();
            using (var content = new MultipartFormDataContent())
            {
                if (httpRequest.Files.Count > 0)
                {
                    /*var videoUploadId = httpRequest.Form["videouploadid"];
                    if(videoUploadId != null)
                    {
                        // Xóa video cũ
                        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"https://app-api.mona.host/v1/videos/{videoUploadId}");
                        deleteRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        deleteRequest.Headers.Add("X-MONA-KEY", serverDownload_Api_Key);

                        var deleteResponse = await httpClient.SendAsync(deleteRequest);
                        if (!deleteResponse.IsSuccessStatusCode)
                        {
                            var deleteErrorContent = await deleteResponse.Content.ReadAsStringAsync();
                            return StatusCode((int)deleteResponse.StatusCode, new { message = "Xóa video cũ thất bại!", deleteErrorContent });
                        }
                    }*/

                    var file = httpRequest.Files["file"];
                    if (file != null)
                    {
                        var fileContent = new StreamContent(file.InputStream);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                        content.Add(fileContent, "file", file.FileName);
                    }

                    var thumbnail = httpRequest.Files["thumbnail"];
                    if (thumbnail != null)
                    {
                        var thumbnailContent = new StreamContent(thumbnail.InputStream);
                        thumbnailContent.Headers.ContentType = MediaTypeHeaderValue.Parse(thumbnail.ContentType);
                        content.Add(thumbnailContent, "thumbnail", thumbnail.FileName);
                    }
                }
                else
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Vui lòng chọn video cần tải lên!" });

                var title = httpRequest.Form["title"];
                if(title == null)
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Vui lòng nhập tên bài học trước!" });

                content.Add(new StringContent(serverDownload_Video_Protection_Id ?? ""), "video_protection_id");
                content.Add(new StringContent(protectedValue ?? ""), "protected");
                content.Add(new StringContent(title ?? ""), "title");

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("X-MONA-KEY", serverDownload_Api_Key);

                var response = await httpClient.PostAsync("https://app-api.mona.host/v1/videos", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseContentParse = JsonConvert.DeserializeObject<ResponseUploadVideoDTO>(responseContent);
                    var data = new ConvertUploadVideoDTO();
                    data.VideoUploadId = responseContentParse._id;
                    data.Minute = (int)Math.Ceiling((responseContentParse.duration ?? 0) / 60.0);
                    data.Thumbnail = responseContentParse.thumbnail;
                    data.VideoUrl = $"https://video.mona-cloud.com/api/video/?user={serverDownloadInfor.folder}&video={responseContentParse.filename}&protected=True&version=v2";
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { message = "Thất bại!", errorContent });
                }
            }
        }

        [HttpPost]
        [Route("api/LessonVideo/AntiDownload/edit-video")]
        public async Task<IActionResult> EditVideo()
        {
            //var protectedValue = "true";
            var httpClient = new HttpClient();
            var httpRequest = HttpContext.Current.Request;
            //var serverDownloadInfor = await LessonVideoService.GetDiskUsage();
            using (var content = new MultipartFormDataContent())
            {
                if (httpRequest.Files.Count > 0)
                {
                    var thumbnail = httpRequest.Files["thumbnail"];
                    if (thumbnail != null && thumbnail.ContentLength > 0)
                    {
                        var thumbnailContent = new StreamContent(thumbnail.InputStream);
                        thumbnailContent.Headers.ContentType = MediaTypeHeaderValue.Parse(thumbnail.ContentType);
                        content.Add(thumbnailContent, "thumbnail", thumbnail.FileName);
                    }
                }

                var title = httpRequest.Form["title"];
                if (title == null)
                    title = "";

                var videoUploadId = httpRequest.Form["videouploadid"];
                if (videoUploadId == null)
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Vui lòng truyền thông tin video cần cập nhật" });

                //content.Add(new StringContent(serverDownload_Video_Protection_Id ?? ""), "video_protection_id");
                content.Add(new StringContent(title ?? ""), "title");

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("X-MONA-KEY", serverDownload_Api_Key);

                var response = await httpClient.PutAsync($"https://app-api.mona.host/v1/videos/{videoUploadId}", content);

                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotModified)
                {
                    /*var responseContent = await response.Content.ReadAsStringAsync();
                    var responseContentParse = JsonConvert.DeserializeObject<ResponseUploadVideoDTO>(responseContent);
                    var data = new ConvertUploadVideoDTO();
                    data.VideoUploadId = responseContentParse._id;
                    data.Minute = (int)Math.Ceiling((responseContentParse.duration ?? 0) / 60.0);
                    data.Thumbnail = responseContentParse.thumbnail;
                    data.VideoUrl = $"https://video.mona-cloud.com/api/video/?user={serverDownloadInfor.folder}&video={responseContentParse.filename}&protected=True&version=v2";
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });*/
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!" });
                }
                else
                {
                    /*var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { message = "Thất bại!", errorContent });*/
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { message = "Thất bại!", errorContent });
                }
            }
        }


        [HttpPost]
        [Route("api/LessonVideo")]
        public async Task<IActionResult> Insert([FromBody] LessonVideoCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var httpContext = HttpContext.Current;
                    var pathViews = Path.Combine(httpContext.Server.MapPath("~/Views"));
                    var data = await LessonVideoService.Insert(
                        model,
                        GetCurrentUser(),
                        httpContext.Server.MapPath("~/Upload/FileInVideo/"),
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

        [HttpPost]
        [Route("api/LessonVideo/v2")]
        public async Task<IActionResult> InsertV2([FromBody] LessonVideoCreateV2 model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var httpContext = HttpContext.Current;
                    var pathViews = Path.Combine(httpContext.Server.MapPath("~/Views"));
                    var data = await LessonVideoService.InsertV2(
                        model,
                        GetCurrentUser(),
                        httpContext.Server.MapPath("~/Upload/FileInVideo/"),
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
        public async Task<IActionResult> Update([FromBody] LessonVideoUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var httpContext = HttpContext.Current;
                    var data = await LessonVideoService.Update(
                        model,
                        GetCurrentUser(),
                        httpContext.Server.MapPath("~/Upload/FileInVideo/"));
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
        [Route("api/LessonVideo/v2")]
        public async Task<IActionResult> UpdateV2([FromBody] LessonVideoUpdateV2 model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var httpContext = HttpContext.Current;
                    var data = await LessonVideoService.UpdateV2(
                        model,
                        GetCurrentUser(),
                        httpContext.Server.MapPath("~/Upload/FileInVideo/"));
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
            var data = await LessonVideoService.GetBySection(sectionId, GetCurrentUser());
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
                string link = "";
                var httpContext = HttpContext.Current;
                var file = httpContext.Request.Files.Get("File");
                if (file != null)
                {
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    string fileName = Guid.NewGuid() + ext; // getting File Name
                    string fileExtension = Path.GetExtension(fileName).ToLower();
                    var result = AssetCRM.IsValidDocument(ext); // Validate Header
                    if (result)
                    {
                        fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine(httpContext.Server.MapPath("~/Upload/FileInVideo/"), fileName);
                        string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
                        string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                        link = strUrl + "Upload/FileInVideo/" + fileName;
                        file.SaveAs(path);
                        //if (!link.Contains("https"))
                        //    link = link.Replace("http", "https");
                        return StatusCode((int)HttpStatusCode.OK, new { data = link, message = ApiMessage.SAVE_SUCCESS });
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.INVALID_FILE });
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.NOT_FOUND });
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
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
            using (var dbContext = new lmsDbContext())
            {
                using (var tran = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        await LessonVideoService.Completed(dbContext,lessonVideoId, GetCurrentUser());
                        tran.Commit();
                        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        return StatusCode((int)HttpStatusCode.OK);
                    }
                }
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

        //[HttpPost]
        //[Route("api/LessonVideo/test-completed")]
        //public async Task<IActionResult> TestCompleteVideo()
        //{
        //    try
        //    {
        //        await LessonVideoService.TestCompleteVideo();
        //        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //    }
        //}

        [HttpPost]
        [Route("api/LessonVideo/save-time-watching-video")]
        public async Task<IActionResult> SaveTimeWatchingVideo([FromBody] SaveTimeWatchingVideoModel itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await LessonVideoService.SaveTimeWatchingVideo(
                        itemModel,
                        GetCurrentUser());
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
        [Route("api/LessonVideo/time-watching-video/{lessonVideoId}")]
        public async Task<IActionResult> GetTimeWatchingVideo(int lessonVideoId)
        {
            var  data = await LessonVideoService.GetTimeWatchingVideo(lessonVideoId,GetCurrentUser());
            if (data != null)
            {
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            return StatusCode((int)HttpStatusCode.NoContent);

        }
        [HttpGet]
        [Route("api/LessonVideo/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await LessonVideoService.GetById(id);
            if (data != null)
            {
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            return StatusCode((int)HttpStatusCode.NoContent);

        }
    }
}
