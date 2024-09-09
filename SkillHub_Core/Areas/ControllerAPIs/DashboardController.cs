using LMSCore.Areas.Models;
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
using static LMSCore.Services.DashboardService;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [ValidateModelState]
    public class DashboardController : BaseController
    {
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
        /// <summary>
        /// Thống kê khách hàng và học viên theo tháng dành cho tư vấn viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Dashboard/StatisticialCustomerInMonth")]
        public async Task<IActionResult> StatisticialCustomerInMonth([FromQuery] StatisticialCustomerSearch baseSearch)
        {
            try
            {
                var data = await DashboardService.StatisticialCustomerInMonth(baseSearch);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        /// <summary>
        /// Thống kê tất cả khách hàng và học viên dành cho tư vấn viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Dashboard/StatisticialAllCustomer")]
        public async Task<IActionResult> StatisticialAllCustomer([FromQuery] StatisticialCustomerSearch baseSearch)
        {
            try
            {
                var data = await DashboardService.StatisticialAllCustomer(baseSearch);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        
    }



}
