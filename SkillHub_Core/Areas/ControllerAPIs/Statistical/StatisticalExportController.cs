using LMSCore.Areas.Models;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Services.Statistical;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace LMSCore.Areas.ControllerAPIs.Statistical
{
    /// <summary>
    /// export thống kê
    /// </summary>
    [ClaimsAuthorize]
    [Route("api/StatisticalExport")]
    [ValidateModelState]
    public class StatisticalExportController : BaseController
    {
        private lmsDbContext dbContext;
        private StatisticalExport domainService;
        public StatisticalExportController()
        {
            dbContext = new lmsDbContext();
            domainService = new StatisticalExport(dbContext);
        }

        /// <summary>
        /// export excel
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ExportRevenueByStudent")]
        public async Task<IActionResult> ExportRevenueByStudent([FromQuery] ExportStatisticalSearch baseSearch)
        {
            try
            {
                var data = await domainService.RevenueByStudent(baseSearch, GetCurrentUser());
                string baseUrl = Request.Scheme + "://" + Request.Host;
                string folderToSave = "StatisticalExport";
                string template = "TemplateExport.xlsx";
                string fileExportName = "DanhSachHocVien";
                List<string> listTitle = new List<string>
                    {                      
                        "MSNV",
                        "Họ và tên",
                        "Số tiền",
                        "Lý do",
                        "Phương thức",
                        "Ngày thanh toán",
                        "Người duyệt"
                    };
                var result = ExcelExportService.ExportV2(template, folderToSave, data, listTitle, fileExportName, baseUrl);
                return StatusCode((int)HttpStatusCode.OK, new { statusCode = (int)HttpStatusCode.OK, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
    }
}
