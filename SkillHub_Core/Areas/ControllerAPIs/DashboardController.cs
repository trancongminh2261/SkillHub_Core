using LMS_Project.Areas.Models;
using LMS_Project.Models;
using LMS_Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Areas.ControllerAPIs;
using LMSCore.Users;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class DashboardController : BaseController
    {
        #region V2
        /*[HttpGet]
        [Route("api/Dashboard/OverviewV2")]
        public async Task<IActionResult> OverviewModelV2()
        {
            try
            {
                var data = await DashboardService.OverviewModelV2();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }*/

        /*[HttpGet]
        [Route("api/Dashboard/TotalDevice")]
        public async Task<IActionResult> TotalDevice()
        {
            try
            {
                var data = await DashboardService.TotalDevice();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }*/

        [HttpGet]
        [Route("api/Dashboard/TotalStaffCompleteAndNotCompleteCourse")]
        public async Task<IActionResult> TotalStaffCompleteAndNotCompleteCourse()
        {
            try
            {
                var data = await DashboardService.TotalStaffCompleteAndNotCompleteCourse();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }

        [HttpGet]
        [Route("api/Dashboard/TotalStaffStudyMoreOrLessThan5Hour")]
        public async Task<IActionResult> TotalStaffStudyMoreOrLessThan5Hour()
        {
            try
            {
                var data = await DashboardService.TotalStaffStudyMoreOrLessThan5Hour();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }
        [HttpGet]
        [Route("api/Dashboard/AverageVideoViewingTime")]
        public async Task<IActionResult> AverageVideoViewingTime()
        {
            try
            {
                var data = await DashboardService.AverageVideoViewingTime();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }

        [HttpGet]
        [Route("api/Dashboard/AverageVideoViewingViews")]
        public async Task<IActionResult> AverageVideoViewingViews()
        {
            try
            {
                var data = await DashboardService.AverageVideoViewingViews();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }

        [HttpGet]
        [Route("api/Dashboard/CountViewOfVideo")]
        public async Task<IActionResult> CountViewOfVideo()
        {
            try
            {
                var data = await DashboardService.CountViewOfVideo();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        #endregion


        [HttpGet]
        [Route("api/Dashboard/Overview")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await DashboardService.OverviewModel();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
            
        }
        [HttpGet]
        [Route("api/Dashboard/StatisticGetInMonth")]
        public async Task<IActionResult> GetAllInMonth()
        {
            try
            {
                var data = await DashboardService.OverviewModelInMonth();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }
        [HttpGet]
        [Route("api/Dashboard/StatisticAgeStudent")]
        public async Task<IActionResult> GetAllAgeStudent()
        {
            try
            {
                var data = await DashboardService.StatisticForAge();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }
        [HttpGet]
        [Route("api/Dashboard/StatisticTopCourse")]
        public async Task<IActionResult> StatisticTopCourse()
        {
            try
            {
                var data = await DashboardService.StatisticTopCourse();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }
        [HttpGet]
        [Route("api/Dashboard/OverviewTeacher")]
        public async Task<IActionResult> OverviewTeacher()
        {
            try
            {
                var data = await DashboardService.OverviewModelForTeacher(GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }
        [HttpGet]
        [Route("api/Dashboard/OverviewStudent")]
        public async Task<IActionResult> OverviewStudent()
        {
            try
            {
                var data = await DashboardService.StatisticCourseStudent(GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/Dashboard/Student/LearningDetails")]
        public async Task<IActionResult> LearningDetails()
        {
            try
            {
                var data = await DashboardService.LearningDetails(GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        /// <summary>
        /// Thống kê học tập của người dùng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Dashboard/OverviewLearning")]
        public async Task<IActionResult> OverviewLearning([FromQuery] OverviewSearch search)
        {
            try
            {
                var data = await DashboardService.OverviewLearning(search);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        /// <summary>
        /// Thống kê khoá học của hệ thống
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Dashboard/OverviewVideoCourse")]
        public async Task<IActionResult> OverviewVideoCourse([FromQuery] OverviewSearch search)
        {
            try
            {
                var data = await DashboardService.OverviewVideoCourse(search);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        /// <summary>
        /// Thống kê bài tập
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Dashboard/OverviewExam")]
        public async Task<IActionResult> OverviewExam([FromQuery] OverviewSearch search)
        {
            try
            {
                var data = await DashboardService.OverviewExam(search);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        //[HttpGet]
        //[ClaimsAuthorize(new RoleEnum[] {
        //    RoleEnum.admin,
        //    RoleEnum.teacher,
        //})]
        //[Route("api/Dashboard/OverviewUserInformation")]
        //public async Task<IActionResult> OverviewUserInformation()
        //{
        //    try
        //    {
        //        var data = await DashboardService.OverviewUserInformation();
        //        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //    }
        //}
        #region Dashboard For Student
        /// <summary>
        /// Chứng chỉ của học viên
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Dashboard/certificate")]
        public async Task<IActionResult> GetCertificateInDashboard()
        {
            try
            {
                var data = await DashboardService.GetCertificateInDashboard(GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        /// <summary>
        /// Đang học dở
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Dashboard/learning")]
        public async Task<IActionResult> GetLearningInDashboard()
        {
            try
            {
                var data = await DashboardService.GetLearningInDashboard(GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        #endregion
        [HttpGet]
        [Route("api/Dashboard/unfinished-grading")]
        public async Task<IActionResult> GetUnfinishedGrading()
        {
            try
            {
                var data = await DashboardService.GetUnfinishedGrading();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/Dashboard/statistical-certificate")]
        public async Task<IActionResult> GetStatisticalCertificate()
        {
            try
            {
                var data = await DashboardService.GetStatisticalCertificate();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("api/Dashboard/statistical-exam-result")]
        public async Task<IActionResult> GetStatisticalExamResult()
        {
            try
            {
                var data = await DashboardService.GetStatisticalExamResult();
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }



}
