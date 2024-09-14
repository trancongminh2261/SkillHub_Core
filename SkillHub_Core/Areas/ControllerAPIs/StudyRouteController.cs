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
using LMSCore.Areas.ControllerAPIs;
using LMSCore.Models;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [Route("api/StudyRoute")]
    public class StudyRouteController : BaseController
    {
        private lmsDbContext dbContext;
        private StudyRouteService domainService;
        public StudyRouteController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new StudyRouteService(this.dbContext);
        }

        /// <summary>
        /// Lấy danh sách lộ trình học
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] StudyRouteSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thàng công", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// Tìm kiếm lộ trình học theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await domainService.GetById(id);
                if (data == null)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công", data = data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        /// <summary>
        /// Thêm mới lộ trình học
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] StudyRouteCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Insert(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công", data });
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
        /// chỉnh sửa lộ trình học
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] StudyCourseUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Update(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công", data });
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
        /// xóa lộ trình học
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await domainService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
