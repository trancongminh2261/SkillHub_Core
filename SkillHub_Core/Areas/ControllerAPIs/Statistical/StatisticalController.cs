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
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Services.DashboardService;
using LMSCore.Utilities;
using LMSCore.Services.Statistical;

namespace LMSCore.Areas.ControllerAPIs.Statistical
{
    /// <summary>
    /// thống kê
    /// </summary>
    [ClaimsAuthorize]
    [Route("api/Statistical")]
    [ValidateModelState]
    public class StatisticalController : BaseController
    {
        private lmsDbContext dbContext;
        private StatisticalService domainService;
        public StatisticalController()
        {
            dbContext = new lmsDbContext();
            domainService = new StatisticalService(dbContext);
        }
        #region báo cáo thống khách hàng
        /// <summary>
        /// thống kê số liệu khách hàng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerOverview")]
        public async Task<IActionResult> CustomerOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.CustomerOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// thống kê số liệu khách hàng - so sánh
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerCompareOverview")]
        public async Task<IActionResult> CustomerCompareOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.CustomerCompareOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// tỷ lệ chuyển đổi khách hàng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ConversionRateStatistics")]
        public async Task<IActionResult> ConversionRateStatistics([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ConversionRateStatistics(baseSearch, GetCurrentUser());
            if (data == null || data.Count <= 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// khách hàng mới trong 12 tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("NewCustomer12Month")]
        public async Task<IActionResult> NewCustomer12Month([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.NewCustomer12Month(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// biểu đồ tỷ lệ nhu cầu học
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerByLearningNeed")]
        public async Task<IActionResult> CustomerByLearningNeed([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.CustomerByLearningNeed(baseSearch, GetCurrentUser());
            if (data == null || data.Count <= 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Biểu đồ tỷ lệ mục đích học
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerByLearningPurpose")]
        public async Task<IActionResult> CustomerByLearningPurpose([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.CustomerByLearningPurpose(baseSearch, GetCurrentUser());
            if (data == null || data.Count <= 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Biểu đồ tỷ lệ khách hàng theo nguồn khách hàng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerBySource")]
        public async Task<IActionResult> CustomerBySource([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.CustomerBySource(baseSearch, GetCurrentUser());
            if (data == null || data.Count <= 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        #endregion

        #region báo cáo thống kê học viên
        /// <summary>
        /// thống kê số liệu học viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StudentOverview")]
        public async Task<IActionResult> StudentOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.StudentOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// thống kê số liệu học viên - so sánh
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StudentCompareOverview")]
        public async Task<IActionResult> StudentCompareOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.StudentCompareOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// biểu đồ tỷ lệ nhu cầu học
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StudentByLearningNeed")]
        public async Task<IActionResult> StudentByLearningNeed([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.StudentByLearningNeed(baseSearch, GetCurrentUser());
            if (data == null || data.Count <= 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Biểu đồ tỷ lệ mục đích học
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StudentByLearningPurpose")]
        public async Task<IActionResult> StudentByLearningPurpose([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.StudentByLearningPurpose(baseSearch, GetCurrentUser());
            if (data == null || data.Count <= 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Biểu đồ tỷ lệ học viên theo nguồn khách hàng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StudentBySource")]
        public async Task<IActionResult> StudentBySource([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.StudentBySource(baseSearch, GetCurrentUser());
            if (data == null || data.Count <= 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Biểu đồ thống kê độ tuổi
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StudentByAge")]
        public async Task<IActionResult> StudentByAge([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.StudentByAge(baseSearch, GetCurrentUser());
            if (data == null || data.Count <= 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// học viên mới trong 12 tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("NewStudent12Month")]
        public async Task<IActionResult> NewStudent12Month([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.NewStudent12Month(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        #endregion

        #region báo cáo thống kê ngân hàng đề thi
        /// <summary>
        /// thống kê số liệu ngân hàng đề thi
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExamSetOverview")]
        public async Task<IActionResult> ExamSetOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ExamSetOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// thống kê số liệu ngân hàng đề thi - so sánh
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExamSetCompareOverview")]
        public async Task<IActionResult> ExamSetCompareOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ExamSetCompareOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// báo cáo học viên mua bộ đề
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StudentBuyExamSet")]
        public async Task<IActionResult> ReportStudentBuyExamSet([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await StatisticalService.ReportStudentBuyExamSet(baseSearch);
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// báo cáo chi tiết học viên mua bộ đề
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DetailStudentBuyExamSet")]
        public async Task<IActionResult> ReportDetailStudentBuyExamSet([FromQuery] ReportDetailSearch baseSearch)
        {
            var data = await domainService.ReportDetailStudentBuyExamSet(baseSearch);
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// báo cáo kết quả bài tập về nhà
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ReportHomeworkResult")]
        public async Task<IActionResult> ReportHomeworkResult([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ReportHomeworkResult(baseSearch, GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// báo cáo kết quả bài thi
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ReportTestResult")]
        public async Task<IActionResult> ReportTestResult([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ReportTestResult(baseSearch, GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        #endregion

        #region báo cáo thống kê lớp học
        /// <summary>
        /// thống kê số liệu lớp học
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClassOverview")]
        public async Task<IActionResult> ClassOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ClassOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// thống kê số liệu lớp học - so sánh
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClassCompareOverview")]
        public async Task<IActionResult> ClassCompareOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ClassCompareOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// tiến trình lớp học
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClassProgress")]
        public async Task<IActionResult> ClassProgress([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ClassProgress(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// lớp mới trong 12 tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("NewClass12Month")]
        public async Task<IActionResult> NewClass12Month([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.NewClass12Month(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// báo cáo thông tin chung từng lớp
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ReportClass")]
        public async Task<IActionResult> ReportClass([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ReportClass(baseSearch, GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// số buổi dạy / học trong 12 tháng - dùng cho giáo viên, học viên, phụ huynh
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Schedule12Month")]
        public async Task<IActionResult> Schedule12Month([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.Schedule12Month(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        #endregion

        #region báo cáo thống kê tài chính
        /// <summary>
        /// thống kê số liệu tài chính - so sánh
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("FinanceCompareOverview")]
        public async Task<IActionResult> FinanceCompareOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.FinanceCompareOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        [HttpGet]
        [Route("FinanceCompareOverview/v2")]
        public async Task<IActionResult> FinanceCompareOverviewV2([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.FinanceCompareOverviewV2(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// báo cáo doanh thu theo nguồn khách hàng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        /*[HttpGet]
        [Route("RevenueBySource")]
        public async Task<IActionResult> RevenueBySource([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.RevenueBySource(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }*/
        [HttpGet]
        [Route("RevenueBySource/v2")]
        public async Task<IActionResult> RevenueBySourceV2([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.RevenueBySourceV2(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// báo cáo doanh thu trong 12 tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Revenue12Month")]
        public async Task<IActionResult> Revenue12Month([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.Revenue12Month(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        [HttpGet]
        [Route("Revenue12Month/v2")]
        public async Task<IActionResult> Revenue12MonthV2([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.Revenue12MonthV2(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// báo cáo chi phí trong 12 tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Expense12Month")]
        public async Task<IActionResult> Expense12Month([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.Expense12Month(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        [HttpGet]
        [Route("Expense12Month/v2")]
        public async Task<IActionResult> Expense12MonthV2([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.Expense12MonthV2(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// báo cáo doanh thu theo hoạt động của trung tâm
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        /*[HttpGet]
        [Route("RevenueByCenterActivities")]
        public async Task<IActionResult> RevenueByCenterActivities([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.RevenueByCenterActivities(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }*/
        [HttpGet]
        [Route("RevenueByCenterActivities/v2")]
        public async Task<IActionResult> RevenueByCenterActivitiesV2([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.RevenueByCenterActivitiesV2(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        #endregion

        #region báo cáo thống kê nhân viên
        /// <summary>
        /// thống kê số liệu nhân viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StaffOverview")]
        public async Task<IActionResult> StaffOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.StaffOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// thống kê số liệu nhân viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StaffCompareOverview")]
        public async Task<IActionResult> StaffCompareOverview([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.StaffCompareOverview(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// tỷ lệ chuyển đổi khách hàng sang hẹn test trong 12 tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("TestAppointment12Month")]
        public async Task<IActionResult> TestAppointment12Month([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.TestAppointment12Month(baseSearch, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// thống kê doanh thu của tư vấn viên - xếp hạng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ConsultingRevenue")]
        public async Task<IActionResult> ConsultingRevenue([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.ConsultingRevenue(baseSearch, GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// thống kê doanh thu của từng học viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        /*[HttpGet]
        [Route("RevenueByStudent")]
        public async Task<IActionResult> RevenueByStudent([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.RevenueByStudent(baseSearch, GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }*/

        [HttpGet]
        [Route("RevenueByStudent/v2")]
        public async Task<IActionResult> RevenueByStudentV2([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.RevenueByStudentV2(baseSearch, GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// thống kê doanh thu của từng học viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CustomerConversion")]
        public async Task<IActionResult> CustomerConversion([FromQuery] StatisticalSearch baseSearch)
        {
            var data = await domainService.CustomerConversion(baseSearch, GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        #endregion

        /// <summary>
        /// thống kê hoa hồng của tư vấn viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("commission-sale")]
        public async Task<IActionResult> StatisticalCommissionOfSale([FromQuery] CommissionOfSaleSearch baseSearch)
        {
            var data = await StatisticalService.StatisticalCommissionOfSale(baseSearch, GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// export thông tin thống kê hoa hồng của tư vấn viên
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ExportCommissionSale")]
        public async Task<IActionResult> ExportExcel([FromQuery] CommissionOfSaleSearch baseSearch)
        {
            try
            {
                //data đổ vào file
                var data = await StatisticalService.ExportCommissionOfSale(baseSearch, GetCurrentUser());
                //thư mục chứa file sau khi export
                string folder = "StatisticalExport";
                //tên file mẫu
                string template = "TemplateExport.xlsx";
                //tên file sau khi export
                string fileExportName = "BaoCaoHoaHong";
                List<string> listTitle = new List<string>
                {
                    "Mã nhân viên",
                    "Họ và tên",
                    "Số học viên đóng cọc",
                    "Số học viên đóng 100% học phí",
                    "Số học viên được mở lớp",
                    "Tỷ lệ mở lớp so với tổng",
                };
                string baseUrl = Request.Scheme + "://" + Request.Host;
                var result = ExcelExportService.ExportV2(template, folder, data, listTitle, fileExportName, baseUrl);
                return StatusCode((int)HttpStatusCode.OK, new { statusCode = (int)HttpStatusCode.OK, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        #region thống kê cũ
        /*/// <summary>
        /// mấy cái ô thống kê
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("overview")]
        public async Task<IActionResult> GetStatisticalOverview([FromQuery] OverviewFilter search)
        {
            var data = await StatisticalService.GetStatisticalOverview(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// tuổi của học viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("student-age")]
        public async Task<IActionResult> GetStatisticalAge([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalAge(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// tỉ lệ đánh giá phản hồi
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("feedback-rating")]
        public async Task<IActionResult> GetStatisticalFeedbackRating([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalFeedbackRating(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// lớp mới mỗi tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("new-class")]
        public async Task<IActionResult> GetStatisticalNewClass([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalNewClass(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// khách mới mỗi tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("new-customer")]
        public async Task<IActionResult> GetStatisticalNewCustomer([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalNewCustomer(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// doanh thu
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("revenue")]
        public async Task<IActionResult> GetStatisticalPayment([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalPayment(search);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// top 5 nhu cầu học
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("top-learning-need")]
        public async Task<IActionResult> GetStatisticalTopLearningNeed([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalTopLearningNeed(search);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// top 5 mục đích học
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("top-purpose")]
        public async Task<IActionResult> GetStatisticalTopPurpose([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalTopPurpose(search);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// top 5 nguồn khách hàng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns> 
        [HttpGet]
        [Route("top-source")]
        public async Task<IActionResult> GetStatisticalTopSource([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalTopSource(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// top 5 công việc của học viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("top-job")]
        public async Task<IActionResult> GetStatisticalTopJob([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalTopJob(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// mấy cái block thống kê của giáo viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>

        /// <summary>
        /// số buổi dạy của giáo viên mỗi tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("total-schedule-teacher")]
        public async Task<IActionResult> GetStatisticalTotalScheduleTeacher([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalTotalScheduleTeacher(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// số buổi dạy của học viên mỗi tháng
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("total-schedule-student")]
        public async Task<IActionResult> GetStatisticalTotalScheduleStudent([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalTotalScheduleStudent(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// đánh giá của học viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("rate-teacher")]
        public async Task<IActionResult> GetStatisticalRateTeacher([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalRateTeacher(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// khách mới mỗi tháng theo từng sales
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("new-customerofsales")]
        public async Task<IActionResult> GetStatisticalNewCustomerOfSales([FromQuery] ChartSearch search)
        {
            var data = await StatisticalService.GetStatisticalNewCustomerOfSalesId(search, GetCurrentUser());
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        /// <summary>
        /// Thống kê tất cả khách hàng hẹn test theo năm theo sales
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Dashboard/StatisticialTestAppointment")]
        public async Task<IActionResult> StatisticialTestAppointment([FromQuery] StatisticialCustomerInYearSearch baseSearch)
        {
            try
            {
                var data = await StatisticalService.StatisticialTestAppointment(baseSearch, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }*/
        #endregion

        #region Thống kê trong lớp học
        /// <summary>
        /// Thống kê điểm danh
        /// </summary>
        [HttpGet]
        [Route("class/roll-up")]
        public async Task<IActionResult> StatisticalRollUp(int classId)
        {
            var data = await StatisticalService.StatisticalRollUp(classId);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Thống kê bài tập
        /// </summary>
        [HttpGet]
        [Route("class/homework")]
        public async Task<IActionResult> StatisticalHomework(int classId)
        {
            var data = await StatisticalService.StatisticalHomework(classId);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Thống kê thông tin học viên
        /// </summary>
        [HttpGet]
        [Route("class/student")]
        public async Task<IActionResult> StatisticalStudentInClass([FromQuery] StatisticalStudentInClassSearch baseSearch)
        {
            var data = await StatisticalService.StatisticalStudentInClass(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Thống kê thông tin bài tập của học viên theo từ khóa
        /// </summary>
        [HttpGet]
        [Route("class/homework/keywords")]
        public async Task<IActionResult> StatisticalHomeworkKeywords([FromQuery] StatisticalHomeworkKeywordsSearch baseSearch)
        {
            var data = await StatisticalService.StatisticalHomeworkKeywords(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }

        /// <summary>
        /// Thống kê tổng thông tin bài tập của học viên theo từ khóa
        /// </summary>
        [HttpGet]
        [Route("class/total/homework/keywords")]
        public async Task<IActionResult> StatisticalTotalHomeworkKeywords([FromQuery] StatisticalHomeworkKeywordsSearch baseSearch)
        {
            var data = await StatisticalService.StatisticalTotalHomeworkKeywords(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        }
        #endregion
    }
}