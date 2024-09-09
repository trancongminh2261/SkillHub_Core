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
using static LMSCore.Services.VideoCourseStudentService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [ValidateModelState]
    public class VideoCourseStudentController : BaseController
    {
        [HttpPost]
        [Route("api/VideoCourseStudent/active")]
        public async Task<IActionResult> ActiveVideoCourse([FromBody] ActiveVideoCourseModel itemModel)
        {
            try
            {
                await VideoCourseStudentService.ActiveVideoCourse(itemModel, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPost]
        [Route("api/VideoCourseStudent")]
        public async Task<IActionResult> Insert([FromBody]VideoCourseStudentCreate itemModel)
        {
            try
            {
                var data = await VideoCourseStudentService.Insert(itemModel, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        ///// <summary>
        ///// Danh sách khoá học của mình
        ///// </summary>
        ///// <param name="search"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[ClaimsAuthorize(new RoleEnum[] {
        //    RoleEnum.admin,
        //    RoleEnum.student,
        //    RoleEnum.teacher
        //})]
        //[Route("api/VideoCourseStudent")]
        //public async Task<IActionResult> GetAll([FromQuery] VideoCourseStudentSearch search)
        //{
        //    var data = await VideoCourseStudentService.GetAll(search, GetCurrentUser());
        //    if (data.TotalRow == 0)
        //        return StatusCode((int)HttpStatusCode.NoContent);
        //    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        //}
        [HttpGet]
        [Route("api/VideoCourseStudent/GetStudent")]
        public async Task<IActionResult> GetStudentInVideoCourse([FromQuery] StudentInVideoCourseSearch search)
        {
            var data = await VideoCourseStudentService.GetStudentInVideoCourse(search);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("api/VideoCourseStudent/{videoCourseId}/GetLearningDetail/{userId}")]
        public async Task<IActionResult> GetLearningDetail(int videoCourseId, int userId)
        {
            var data = await VideoCourseStudentService.GetLearningDetail(videoCourseId,userId);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        /// <summary>
        /// Đánh giá 
        /// myRate: từ 1 => 5
        /// </summary>
        /// <param name="videoCourseId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/VideoCourseStudent/AddRate")]
        public async Task<IActionResult> AddRate([FromBody] AddRateModel model)
        {
            try
            {
                await VideoCourseStudentService.AddRate(model, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/VideoCourseStudent/Rate/{videoCourseId}")]
        public async Task<IActionResult> GetRate(int videoCourseId)
        {
            var data = await VideoCourseStudentService.GetRate(videoCourseId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        ///// <summary>
        ///// Cấp chứng chỉ
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("api/VideoCourseStudent/CreateCertificate")]
        //public async Task<IActionResult> CreateCertificate()
        //{
        //    try
        //    {
        //        await CertificateService.CreateCertificate(GetCurrentUser());
        //        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //    }
        //}
    }
}
