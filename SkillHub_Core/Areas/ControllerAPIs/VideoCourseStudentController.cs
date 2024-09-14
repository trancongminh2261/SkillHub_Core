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
using static LMS_Project.Services.VideoCourseStudentService;
using LMSCore.Areas.ControllerAPIs;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class VideoCourseStudentController : BaseController
    {
        [HttpPost]
        [Route("api/VideoCourseStudent/AddVideoCourse/{videoCourseId}")]
        public async Task<IActionResult> AddVideoCourse(int videoCourseId)
        {
            try
            {
                await VideoCourseStudentService.AddVideoCourse(videoCourseId, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
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
        /// <summary>
        /// Cấp chứng chỉ
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
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
