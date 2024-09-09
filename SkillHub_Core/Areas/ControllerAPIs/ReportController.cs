using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Report")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ReportController : BaseController
    {
        #region Thống kê sinh viên
        //[HttpDelete]
        //[Route("student-report")]
        //public async Task<IActionResult> StudentReport()
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //    }
        //    var data = await domainService.StudentReport();
        //    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        //}
        #endregion

        #region Thống kê học viên
        /// <summary>
        /// Tỉ lệ chuyển đổi từ khách hàng tiềm năng thành học viên mỗi tháng
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-conversion-rate")]
        public async Task<IActionResult> GetStatisticsConversionRate(int Year)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticsConversionRate(Year);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Tỉ lệ chuyển đổi từ khách hàng tiềm năng thành học viên mỗi tháng", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }

        /// <summary>
        /// Số học viên chuyển lớp mỗi tháng
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-student-change-class")]
        public async Task<IActionResult> GetStatisticsStudentChangeClass(int Year)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticsStudentChangeClass(Year);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Số học viên chuyển lớp mỗi tháng", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }

        /// <summary>
        /// Số học viên đăng ký lớp mỗi tháng
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-student-registration-class")]
        public async Task<IActionResult> GetStatisticsStudentRegistrationClass(int Year)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticsStudentRegistrationClass(Year);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Số học viên đăng ký lớp mỗi tháng", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }

        /// <summary>
        /// Thống kê điểm trung bình theo năm
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-student-medium-score")]
        public async Task<IActionResult> GetStatisticsStudentMediumScore(int Year)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticsStudentMediumScore(Year);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thống kê điểm trung bình của học viên theo năm", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }
        #endregion

        #region Thống kê nhân viên
        /// <summary>
        /// Tỉ lệ vai trò nhân viên
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-staff-role")]
        public async Task<IActionResult> GetStatisticsStaffRole()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticsStaffRole();
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Tỉ lệ vai trò nhân viên", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }
        #endregion

        #region Thống kê tài chính
        /// <summary>
        /// Thống kê quản lý thanh toán
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-manage-payment")]
        public async Task<IActionResult> GetStatisticsManagePayment(int Year)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticsManagePayment(Year);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thống kê quản lý thanh toán", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }

        /// <summary>
        /// Thống kê doanh thu và tiền nợ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-payment-by-category")]
        public async Task<IActionResult> GetStatisticsPaymentByCategory()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticsPaymentByCategory();
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thống kê doanh thu và tiền nợ", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }
        #endregion

        #region Thống kê lớp
        /// <summary>
        /// Số lớp học được tạo trong tháng
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-class-created-compare-last-month")]
        public async Task<IActionResult> GetStatisticalClassCreatedCompareLastMonth()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticalClassCreatedCompareLastMonth();
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Số lớp học được tạo trong tháng", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }
        #endregion

        #region Thống kê khóa học video
        /// <summary>
        /// Tổng số khóa học video
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-video-course-available")]
        public async Task<IActionResult> GetStatisticsVideoCoursesAvailable()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticsVideoCoursesAvailable();
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Số khóa học video", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }
        #endregion

        #region Thống kê số buổi test được tạo mỗi tháng
        /// <summary>
        /// Số buổi test mới mỗi tháng
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-statistical-test-appointment")]
        public async Task<IActionResult> GetStatisticsTestAppointment(int Year)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await ReportService.GetStatisticsTestAppointment(Year);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Số buổi test mới mỗi tháng", data = result });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }
        #endregion
    }
}