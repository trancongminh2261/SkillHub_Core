using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
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
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/elsa")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ElsaSpeakController : BaseController
    {
        private lmsDbContext dbContext;
        private readonly ElsaSpeakService elsaSpeakService;
        public ElsaSpeakController()
        {
            this.dbContext = new lmsDbContext();
            this.elsaSpeakService = new ElsaSpeakService(this.dbContext);
        }

        /// <summary>
        /// Định dạng file của document library
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("file")]
        public IActionResult UploadFile()
        {
            try
            {
                var file = HttpContext.Request.Form.Files.GetFile("file");
                string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                var upload = UploadConfig.UploadFile(file, baseUrl, "Upload/Audios/");
                if (upload.Success)
                {
                    return StatusCode((int)HttpStatusCode.OK, new { data = upload.Link, message = upload.Message });
                }
                else
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = upload.Message });
                //if (file == null)
                //{
                //    return BadRequest(new { message = ApiMessage.NOT_FOUND });
                //}

                //string fileName = file.FileName;
                //string ext = Path.GetExtension(fileName).ToLower();
                //string fileExtension = Path.GetExtension(fileName).ToLower();
                //var result = AssetCRM.IsValidAudio(fileExtension); // Validate Header                 

                //if (!result)
                //{
                //    return BadRequest(new { message = ApiMessage.INVALID_FILE });
                //}

                //fileName = Guid.NewGuid() + ext;
                //var path = Path.Combine(Directory.GetCurrentDirectory(), "Upload", "Audios", fileName);
                //string link = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Upload/Audios/{fileName}";

                //using (var stream = new FileStream(path, FileMode.Create))
                //{
                //    file.CopyTo(stream);
                //}

                //if (!link.Contains("https"))
                //{
                //    link = link.Replace("http", "https");
                //}

                //return Ok(new { data = link, message = ApiMessage.SAVE_SUCCESS });
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
        //        var file = httpContext.Request.Files.Get("file");
        //        if (file == null)
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.NOT_FOUND });
        //        string fileName = file.FileName;
        //        string ext = Path.GetExtension(file.FileName).ToLower();
        //        string fileExtension = Path.GetExtension(fileName).ToLower();
        //        var result = AssetCRM.IsValidAudio(fileExtension); // Validate Header                 
        //        if (!result)
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.INVALID_FILE });
        //        fileName = Guid.NewGuid() + ext;
        //        var path = Path.Combine(httpContext.Server.MapPath("~/Upload/Audios/"), fileName);
        //        string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
        //        string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
        //        link = strUrl + "Upload/Audios/" + fileName;

        //        using (var stream = new FileStream(path, FileMode.Create))
        //        {
        //            file.InputStream.CopyTo(stream);
        //        }
        //        if (!link.Contains("https"))
        //            link = link.Replace("http", "https");
        //        return StatusCode((int)HttpStatusCode.OK, new { data = link, message = ApiMessage.SAVE_SUCCESS });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
        //    }
        //}


        [HttpPost]
        [Route("scripted")]
        public async Task<ElsaApiResponse> Scripted([FromBody] ElsaSpeakRequest request)
        => await elsaSpeakService.ScriptedAsync(request);
    }
}
