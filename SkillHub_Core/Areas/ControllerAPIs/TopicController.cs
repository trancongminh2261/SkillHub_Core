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
    public class TopicController : BaseController
    {
        private lmsDbContext dbContext;
        private TopicService standardService;
        public TopicController()
        {
            this.dbContext = new lmsDbContext();
            this.standardService = new TopicService(this.dbContext);
        }

        [HttpGet]
        [Route("api/Topic/{id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var data = await standardService.GetById(Id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("api/Topic")]
        public async Task<IActionResult> GetAll([FromQuery] TopicSearch baseSearch)
        {
            var data = await standardService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpPut]
        [Route("api/Topic")]
        public async Task<IActionResult> Update([FromBody] TopicUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await standardService.Update(model, GetCurrentUser());
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
        [Route("api/Topic")]
        public async Task<IActionResult> Insert([FromBody] TopicCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await standardService.Insert(model, GetCurrentUser());
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
        [Route("api/Topic/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await standardService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

    }
}
