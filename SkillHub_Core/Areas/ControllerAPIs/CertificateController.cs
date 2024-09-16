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
using static LMSCore.Models.lmsEnum;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class CertificateController : BaseController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CertificateController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Route("api/Certificate/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await CertificateService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPut]
        [Route("api/Certificate")]
        public async Task<IActionResult> Update(CertificateUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CertificateService.Update(model, GetCurrentUser());
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
        [HttpGet]
        [Route("api/Certificate")]
        public async Task<IActionResult> GetAll([FromQuery] CertificateSearch search)
        {
            var data = await CertificateService.GetAll(search, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpPost]
        [Route("api/Certificate/{videoCourseId}/for-student/{studentId}")]
        public async Task<IActionResult> CreateCertificate(int videoCourseId, int studentId)
        {
            try
            {
                using (var dbContext = new lmsDbContext())
                {
                    if (GetCurrentUser().RoleId == ((int)RoleEnum.student))
                        studentId = GetCurrentUser().UserInformationId;
                    var check = await dbContext.tbl_VideoCourseStudent
                        .AnyAsync(x => x.VideoCourseId == videoCourseId && x.UserId == studentId && x.Enable == true && x.Status == 3);
                    if (!check)
                        throw new Exception("Chưa hoàn thành chương trình học");

                    var hasCertificate = await dbContext.tbl_Certificate.AnyAsync(x => x.UserId == studentId && x.VideoCourseId == videoCourseId && x.Enable == true);
                    if (hasCertificate)
                        throw new Exception("Học viên đã được cấp chứng chỉ");

                    await CertificateService.CreateCertificate(dbContext,videoCourseId, studentId, _httpContextAccessor);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
                }
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        public class ExportPdf
        {
            public int Id { get; set; }
            public string Content { get; set; }
        }
        [Route("api/Certificate/export-pdf")]
        [HttpPost]
        public async Task<IActionResult> ExportPDF([FromBody] ExportPdf itemModel)
        {
            using (var dbContext = new lmsDbContext())
            {
                try
                {
                    // Xây dựng URL cơ bản và đường dẫn
                    string baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var uploadPath = Path.Combine(baseUrl, "Upload");
                    var pathViews = Path.Combine(baseUrl, "Views");

                    // Xây dựng đường dẫn URL đầy đủ
                    string strUrl = $"{baseUrl}/";

                    // Gọi phương thức ExportPDF từ CertificateService
                    var data = await CertificateService.ExportPDF(dbContext, itemModel.Id, itemModel.Content, uploadPath, strUrl);

                    // Trả về kết quả thành công
                    return StatusCode((int)HttpStatusCode.OK, new
                    {
                        message = "Thành công",
                        data = data
                    });
                }
                catch (Exception e)
                {
                    // Xử lý lỗi và trả về thông báo lỗi
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
        }

    }
}
