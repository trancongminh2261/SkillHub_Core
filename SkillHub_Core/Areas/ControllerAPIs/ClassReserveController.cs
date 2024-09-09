using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.ClassRegistrationService;
using static LMSCore.Services.ClassReserve.ClassReserveService;
using LMSCore.Utilities;
using LMSCore.Services.ClassReserve;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/ClassReserve")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ClassReserveController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await ClassReserveService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("paid")]
        public async Task<IActionResult> PaidCalc([FromQuery] OldClassSearch search)
        {
            var data = await ClassReserveService.PaidCalc(search);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]ClassReserveCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassReserveService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody]ClassReserveUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassReserveService.Update(model, GetCurrentUser());
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
                await ClassReserveService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] ClassReserveSearch baseSearch)
        {
            var data = await ClassReserveService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        /// <summary>
        /// Chuyển học viên vào lớp học
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("add-to-class")]
        //public async Task<IActionResult> AddToClassFromReserve(AddToClassFromReserveModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (var db = new lmsDbContext())
        //        {
        //            using (var tran = db.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    await ClassReserveService.AddToClassFromReserve(model, GetCurrentUser(), db);
        //                    tran.Commit();
        //                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
        //                }
        //                catch (Exception e)
        //                {
        //                    tran.Rollback();
        //                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //                }
        //            }
        //        }
        //    }
        //    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //}

        [HttpGet]
        [Route("review-reserve/{studentInClassId}")]
        public async Task<IActionResult> GetReviewReserve(int studentInClassId)
        {
            var data = await ClassReserveService.GetReviewReserve(studentInClassId);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
