using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
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
using static LMSCore.Services.PythonService;
using Microsoft.AspNetCore.Hosting;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Python")]
    [ValidateModelState]
    public class PythonController : BaseController
    {
        private static IWebHostEnvironment _hostingEnvironment;
        public PythonController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpPost]
        [Route("convert-text-to-speech")]
        public IActionResult ConvertTextToSpeech([FromBody] ReadTextModel itemModel)
        {
            try
            {
                //var httpContext = HttpContext.Current;
                ////var pathDist = Path.Combine(httpContext.Server.MapPath("~/dist"));
                ////var pathUpload = Path.Combine(httpContext.Server.MapPath("~/Upload"));
                //string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
                //string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                var strUrl = Path.Combine(_hostingEnvironment.ContentRootPath, "");
                //var data = PythonService.ConvertTextToSpeech(itemModel, pathDist, pathUpload, strUrl);
                var data = PythonService.ConvertTextToSpeech(itemModel, strUrl);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data.Item1, resultPython = data.Item2 });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
