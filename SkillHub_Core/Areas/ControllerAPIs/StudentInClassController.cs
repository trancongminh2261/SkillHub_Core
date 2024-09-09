using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Users;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Services.StudentInClass.StudentInClassService;
using Microsoft.AspNetCore.Hosting;
using LMSCore.Services.StudentInClass;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/StudentInClass")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class StudentInClassController : BaseController
    {
        private lmsDbContext dbContext;
        private StudentInClassService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public StudentInClassController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new StudentInClassService(this.dbContext, _hostingEnvironment);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await StudentInClassService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        /// <summary>
        /// Thêm nhiều học viên
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("appends")]
        public async Task<IActionResult> AppendStudent([FromBody] StudentInClassAppend model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentInClassService.AppendStudent(model, GetCurrentUser());
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

        /// <summary>
        /// Thêm học thử
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]StudentInClassCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentInClassService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody]StudentInClassUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentInClassService.Update(model, GetCurrentUser());
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
                await StudentInClassService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] StudentInClassSearch baseSearch)
        {
            var data = await StudentInClassService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpGet]
        [Route("by-sale")]
        public async Task<IActionResult> GetBySale([FromQuery] StudentInClassBySaleSearch baseSearch)
        {
            var data = await StudentInClassService.GetBySale(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpGet]
        [Route("v2")]
        public async Task<IActionResult> GetAllV2([FromQuery] StudentInClassV2Search baseSearch)
        {
            var data = await StudentInClassService.GetAllV2(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        ///// <summary>
        ///// danh sách lớp học của học viên
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("class-of-student/{studentId}")]
        //public async Task<IActionResult> GetClassOfStudent(int studentId)
        //{
        //    List<tbl_Class> result = await StudentInClassService.GetClassOfStudent(studentId);
        //    if (result.Count == 0)
        //    {
        //        return StatusCode((int)HttpStatusCode.NoContent);
        //    }
        //    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = result });

        //}

        [HttpGet]
        [Route("student-available")]
        public async Task<IActionResult> GetStudentAvailable([FromQuery]AvailableStudentSearch request)
        {
            var data = await StudentInClassService.GetStudentAvailable(request);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("attendance-by-student")]
        public async Task<IActionResult> GetAttendanceByStudent([FromQuery] AttendanceByStudentSearch request)
        {
            var data = await StudentInClassService.GetAttendanceByStudent(request, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        /// <summary>
        /// Lấy danh sách học viên sắp học xong hoặc không học khóa nào
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("student-inregis")]
        public async Task<IActionResult> GetStudentInRegis([FromQuery] StudentInRegisSearch request)
        {
            var data = await StudentInClassService.GetStudentInRegis(request, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// v2 bổ sung admin tự lọc theo branch Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("student-inregis/v2")]
        public async Task<IActionResult> GetStudentInRegisV2([FromQuery] StudentInRegisV2Search request)
        {
            var data = await StudentInClassService.GetStudentInRegisV2(request, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        /// <summary>
        /// Lấy danh sách các lớp học sắp kết thúc theo học viên
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("student-inregis/{studentId}")]
        public async Task<IActionResult> GetStudentInRegis(int studentId)
        {
            var data = await StudentInClassService.GetUpcommingClassByStudentId(studentId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        #region Chứng chỉ
        [HttpPost]
        [Route("certificate")]
        public async Task<IActionResult> CreateCertificate([FromBody] CreateCertificateRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentInClassService.CreateCertificate(model, GetCurrentUser());
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
        [Route("certificate")]
        public async Task<IActionResult> UpdateCertificate([FromBody] UpdateCertificateRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await StudentInClassService.UpdateCertificate(model, GetCurrentUser());
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
        [Route("certificate/{id}")]
        public async Task<IActionResult> RemoveCertificate(int id)
        {
            try
            {
                await StudentInClassService.RemoveCertificate(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPost]
        [Route("export-certificate")]
        public async Task<IActionResult> ExportCertificate([FromBody] ExportCertificateRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var _hostingEnvironment = WebHostEnvironment.Environment;
                    var path = Path.Combine(_hostingEnvironment.ContentRootPath, "Upload");
                    var fileUrl = $"{Request.Scheme}://{Request.Host}";
                    if (fileUrl.IndexOf("https") == -1)
                        fileUrl = fileUrl.Replace("http", "https");
                    var data = await StudentInClassService.ExportCertificate(model, path, fileUrl);
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
        [Route("certificate")]
        public async Task<IActionResult> GetCertificate([FromQuery] StudentInClassService.CertificateSearch baseSearch)
        {
            var data = await StudentInClassService.GetCertificate(baseSearch);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        #endregion
    }
}
