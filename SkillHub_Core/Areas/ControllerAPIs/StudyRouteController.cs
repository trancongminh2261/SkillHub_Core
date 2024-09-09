using LMSCore.Areas.Request;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    /// <summary>
    /// lộ trình của học viên
    /// </summary>
    [Route("api/StudyRoute")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class StudyRouteController : BaseController
    {
        /// <summary>
        /// thêm lộ trình cho học viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] StudyRouteCreate request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await StudyRouteService.Insert(request, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        /// <summary>
        /// cập nhật lộ trình
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] StudyRouteUpdate request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await StudyRouteService.Update(request, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        /// <summary>
        /// thay đổi thứ tự lộ trình
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("update-index")]
        public async Task<IActionResult> UpdateIndex([FromBody] StudyRouteIndex update)
        {
            try
            {
                var result = await StudyRouteService.UpdateIndex(update.IdUp, update.IdDown, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = result });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        /// <summary>
        /// xem lộ trình của học viên
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="parentIds"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("student-study-route")]
        public async Task<IActionResult> StudentStudyRoute(string studentIds = null, string parentIds = null)
        {
            var result = await StudyRouteService.GetStudyRouteOfStudent(studentIds, parentIds);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = result });
        }
        /// <summary>
        /// xóa
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await StudyRouteService.Delete(id, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = result });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        /// <summary>
        /// gợi ý của học viên
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("suggest-study-route/{studentId}")]
        public async Task<IActionResult> SuggestStudyRoute(int studentId)
        {
            var result = await StudyRouteService.SuggestStudyRoute(studentId);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = result });
        }
    }
}
