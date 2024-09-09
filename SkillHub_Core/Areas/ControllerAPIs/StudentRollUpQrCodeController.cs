using LMSCore.Areas.Request;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/StudentRollUpQrCode")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class StudentRollUpQrCodeController : BaseController
    {
        ///// <summary>
        ///// tạo mã Qr
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route()]
        //public async Task<IActionResult> Insert([FromBody]StudentRollUpQrCodeCreate request)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var result = await StudentRollUpQrCodeService.Insert(request);
        //            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", result });
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //        }
        //    }
        //    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //}
        /// <summary>
        /// xem mã Qr
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="scheduleId"></param>
        /// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> GetQrCodeByStudent(int studentId, int scheduleId)
        //{
        //    try
        //    {
        //        var data = await StudentRollUpQrCodeService.GetQrCodeByStudent(studentId, scheduleId);
        //        if (data == null)
        //            return StatusCode((int)HttpStatusCode.NoContent);
        //        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //    }
           
        //}
        /// <summary>
        /// điểm danh bằng mã Qr
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="scheduleId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("attendance-by-qr-code")]
        public async Task<IActionResult> AttendanceByQrCode(StudentRollUpQrCodeCreate request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentRollUpQrCodeService.AttendanceByQrCode(request);
                    if (data == null)
                        return StatusCode((int)HttpStatusCode.NoContent);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });

        }
    }
}
