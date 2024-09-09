using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.Class.ClassService;
using Microsoft.AspNetCore.Hosting;
using System.Web.Http.Description;
using Microsoft.AspNetCore.Http;
using LMSCore.Services.Class;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Class")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ClassController : BaseController
    {
        private lmsDbContext dbContext;
        private ClassService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public ClassController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new ClassService(this.dbContext, _hostingEnvironment);
        }
        /// <summary>
        /// Tạo lớp cho chọn bảng điểm mẫu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpGet]
        [Route("old-class")]
        public async Task<IActionResult> OldClass([FromQuery] OldClassSearch request)
        {

            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            using (var db = new lmsDbContext())
            {
                var data = await ClassChangeService.GetOldClass(request, db);
                if (data == null)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
            }
        }

        /// <summary>
        /// Lấy danh sách lớp học cho biểu đồ Gantt
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("gantt")]
        public async Task<IActionResult> GetAllGantt([FromQuery] ClassSearch baseSearch)
        {
            var data = await ClassService.GetAllGantt(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                totalRow = data.TotalRow,
                upcoming = data.Upcoming,
                opening = data.Opening,
                closing = data.Closing,
                data = data.Data
            });
        }

        /// <summary>
        /// Tải lịch học khi tạo lớp
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("lesson-when-create")]
        public async Task<IActionResult> GetLessonWhenCreate([FromBody] LessonSearch itemModel)
        {
            try
            {
                var data = await ClassService.GetLessonWhenCreate(itemModel);
                if (!data.Any())
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await ClassService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("teacher-when-create")]
        public async Task<IActionResult> GetTeacherWhenCreate([FromQuery] TeacherSearch itemModel)
        {
            var data = await ClassService.GetTeacherWhenCreate(itemModel);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        /// <summary>
        /// true - lớp đang học (có học viện trong lớp)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("check-exist-student-in-class/{id}")]
        public async Task<IActionResult> CheckExistStudentInClass(int id)
        {
            var data = await ClassService.CheckExistStudentInClass(id);
            //if (data == null)
            //    return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        /// <summary>
        /// lấy danh sách giáo viên khi tạo lịch
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("teacher-available")]
        public async Task<IActionResult> GetTeacherAvailable([FromQuery] TeacherAvailableSearch baseSearch)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassService.GetTeacherAvailable(baseSearch);
                    if (!data.Any())
                        return StatusCode((int)HttpStatusCode.NoContent);
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
        [Route("tutor-available")]
        public async Task<IActionResult> GetTutorAvailable([FromQuery] TeacherAvailableSearch baseSearch)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassService.GetTutorAvailable(baseSearch);
                    if (!data.Any())
                        return StatusCode((int)HttpStatusCode.NoContent);
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
        /// lấy danh sách phòng khi tạo lịch
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("room-available")]
        public async Task<IActionResult> GetRoomAvailable([FromQuery] RoomAvailableSearch baseSearch)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassService.GetRoomAvailable(baseSearch);
                    if (!data.Any())
                        return StatusCode((int)HttpStatusCode.NoContent);
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
        /// Tạo lớp
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] ClassCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody] ClassUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassService.Update(model, GetCurrentUser());
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
                await ClassService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] ClassSearch baseSearch)
        {
            var data = await ClassService.GetAll(baseSearch, GetCurrentUser());
            //if (data.TotalRow == 0)
            //    return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                totalRow = data.TotalRow,
                upcoming = data.Upcoming,
                opening = data.Opening,
                closing = data.Closing,
                data = data.TotalRow == 0 ? new List<tbl_Class>() : data.Data
            });
        }
        [HttpGet]
        [Route("total-row")]
        public async Task<IActionResult> GetTotalRow([FromQuery] ClassSearch baseSearch)
        {
            var data = await ClassService.GetTotalRow(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                totalRow = data.TotalRow,
                upcoming = data.Upcoming,
                opening = data.Opening,
                closing = data.Closing
            });
        }
        [HttpGet]
        [Route("roll-up-teacher")]
        public async Task<IActionResult> GetRollUpTeacher([FromQuery] RollUpTeacherSearch baseSearch)
        {
            var data = await ClassService.GetRollUpTeacher(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpPost]
        [Route("roll-up-teacher/{scheduleId}")]
        public async Task<IActionResult> RollUpTeacher(int scheduleId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ClassService.RollUpTeacher(scheduleId);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
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
        [Route("schedule-in-date-now/{branchId}")]
        public async Task<IActionResult> GetScheduleInDateNow(int branchId)
        {
            var data = await ClassService.GetScheduleInDateNow(branchId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data });
        }


        /// <summary>
        /// lấy danh sách giáo viên khi đăng ký lịch lớp dạy kèm
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("teacher-tutoring-available")]
        public async Task<IActionResult> GetTeacherTutoringAvailable([FromQuery] TeacherTutoringAvailableSearch baseSearch)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassService.GetTeacherTutoringAvailable(baseSearch);
                    if (data.TotalRow == 0)
                        return StatusCode((int)HttpStatusCode.NoContent);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data.Data, totalRow = data.TotalRow });
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
        [Route("tutoring-config")]
        public async Task<IActionResult> TutoringConfig([FromBody] TutoringConfigModel itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await ClassService.TutoringConfig(itemModel);
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
        [Route("tutoring-config")]
        public async Task<IActionResult> GetTutoringConfig()
        {
            var data = await ClassService.GetTutoringConfig();
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("tutoring-curriculum")]
        public async Task<IActionResult> TutoringCurriculum(int classId)
        {
            var data = await ClassService.TutoringCurriculum(classId);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        #region Phần giáo trình trong lớp

        [HttpPost]
        [Route("curriculum-detail-in-class/complete/{curriculumDetailInClassId}")]
        public async Task<IActionResult> CurriculumDetailInClassComplete(int curriculumDetailInClassId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await CurriculumDetailInClassService.Complete(curriculumDetailInClassId, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
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
        [Route("file-curriculum-in-class/complete/{fileCurriculumInClassId}")]
        public async Task<IActionResult> FileCurriculumInClassComplete(int fileCurriculumInClassId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await FileCurriculumInClassSerivce.Complete(fileCurriculumInClassId, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
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
        [Route("curriculum-in-class/{classId}")]
        public async Task<IActionResult> GetCurriculumInClass(int classId)
        {
            var data = await CurriculumInClassService.GetCurriculumInClass(classId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("curriculum-details-in-class")]
        public async Task<IActionResult> GetCurriculumDetailInClass([FromQuery] CurriculumDetailInClassSearch baseSearch)
        {
            var data = await CurriculumDetailInClassService.GetCurriculumDetailInClass(baseSearch, GetCurrentUser());
            if (data.Data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalCurriculum = data.TotalRow, totalFile = data.Total, data = data.Data });
        }
        [HttpGet]
        [Route("file-curriculum-in-class")]
        public async Task<IActionResult> GetFileCurriculumInClass([FromQuery] FilesCurriculumInClassSearch baseSearch)
        {
            var data = await FileCurriculumInClassSerivce.GetFileCurriculumInClass(baseSearch, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPut]
        [Route("curriculum-in-class")]
        public async Task<IActionResult> UpdateCurriculumInClass([FromBody] CurriculumInClassUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CurriculumInClassService.Update(model, GetCurrentUser());
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
        [Route("curriculum-detail-in-class")]
        public async Task<IActionResult> UpdateCurriculumDetailInClass([FromBody] CurriculumDetailInClassUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CurriculumDetailInClassService.Update(model, GetCurrentUser());
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
        [Route("curriculum-detail-in-class")]
        public async Task<IActionResult> AddCurriculumDetailInClass([FromBody] CurriculumDetailInClassCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CurriculumDetailInClassService.Insert(model, GetCurrentUser());
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
        [Route("curriculum-detail-in-class/{id}")]
        public async Task<IActionResult> DeleteCurriculumDetailInClass(int id)
        {
            try
            {
                await CurriculumDetailInClassService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("curriculum-detail-in-class-index")]
        public async Task<IActionResult> UpdateCurriculumDetailIndex([FromBody] List<CurriculumDetailInClassUpdate> request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CurriculumDetailInClassService.UpdateCurriculumDetailIndex(request, GetCurrentUser());
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
        [Route("file-curriculum-in-class/{curriculumInClassDetailId}")]
        public async Task<IActionResult> AddFileCurriculumInClass(int curriculumInClassDetailId)
        {
            try
            {
                var curriculumDetail = await CurriculumDetailInClassService.GetById(curriculumInClassDetailId);
                if (curriculumDetail == null)
                    throw new Exception("Không tìm thấy danh mục");

                // Check if a file exists for the curriculum, if not, assign an index
                var file = Request.Form.Files.GetFile("File");
                string baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                var upload = UploadConfig.UploadFile(file, baseUrl, "Upload/FileInCurriculum/", UploadType.Document);
                if (upload.Success)
                {
                    var model = new FileInCurriculumDetailCreate { CurriculumDetailId = curriculumInClassDetailId, FileName = file.FileName, FileUrl = upload.Link, FileUrlRead = upload.LinkReadFile };
                    var data = await FileCurriculumInClassSerivce.Insert(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { data = data, message = upload.Message });
                }
                else
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = upload.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        //public async Task<IActionResult> AddFileCurriculumInClass(int curriculumInClassDetailId)
        //{
        //    try
        //    {
        //        var curriculumDetail = await CurriculumDetailInClassService.GetById(curriculumInClassDetailId);
        //        if (curriculumDetail == null)
        //            throw new Exception("Không tìm thấy danh mục");
        //        //Kiểm tra có tồn tại file nào của chương chưa thì gắn index
        //        string link = "";
        //        var httpContext = HttpContext.Current;
        //        var file = httpContext.Request.Files.Get("File");
        //        if (file != null)
        //        {
        //            string ext = Path.GetExtension(file.FileName).ToLower();
        //            string fileName = Guid.NewGuid() + ext; // getting File Name
        //            string fileExtension = Path.GetExtension(fileName).ToLower();
        //            var result = AssetCRM.isValIdDocument(ext); // ValIdate Header
        //            if (result)
        //            {
        //                fileName = Guid.NewGuid() + ext;
        //                var path = Path.Combine(httpContext.Server.MapPath("~/Upload/FileInCurriculum/"), fileName);
        //                string strPathAndQuery = httpContext.Request.Url.PathAndQuery;
        //                string strUrl = httpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
        //                link = strUrl + "Upload/FileInCurriculum/" + fileName;

        //                using (var stream = new FileStream(path, FileMode.Create))
        //                {
        //                    file.InputStream.CopyTo(stream);
        //                }

        //                //file.SaveAs(path);
        //                var model = new FileInCurriculumDetailCreate { CurriculumDetailId = curriculumInClassDetailId, FileName = file.FileName, FileUrl = link };
        //                var data = await FileCurriculumInClassSerivce.Insert(model, GetCurrentUser());
        //                return StatusCode((int)HttpStatusCode.OK, new { data = data, message = ApiMessage.SAVE_SUCCESS });
        //            }
        //            else
        //            {
        //                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.INVALID_FILE });
        //            }
        //        }
        //        else
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.NOT_FOUND });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
        //    }
        //}
        [HttpPut]
        [Route("file-curriculum-in-class")]
        public async Task<IActionResult> UpdateFileCurriculumInClass([FromBody] FileCurriculumInClassUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await FileCurriculumInClassSerivce.Update(model, GetCurrentUser());
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
        [Route("file-curriculum-in-class/{fileId}")]
        public async Task<IActionResult> DeleteFileCurriculumInClass(int fileId)
        {
            try
            {
                await FileCurriculumInClassSerivce.Delete(fileId);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("hide-file-curriculum-in-class/{id}")]
        public async Task<IActionResult> HideFileCurriculumInClass(int id)
        {
            try
            {
                await FileCurriculumInClassSerivce.Hide(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpPut]
        [Route("hide-curriculum-detail-in-class/{id}")]
        public async Task<IActionResult> HideCurriculumDetailInClass(int id)
        {
            try
            {
                await CurriculumDetailInClassService.Hide(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpPut]
        [Route("file-curriculum-in-class-index")]
        public async Task<IActionResult> FileCurriculumInClassIndex([FromBody] List<FileCurriculumInClassUpdate> request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await FileCurriculumInClassSerivce.FileCurriculumInClassIndex(request, GetCurrentUser());
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
        #endregion

        [HttpGet]
        [Route("attendance")]
        [ProducesResponseType(typeof(IList<StudentInClassWhenAttendanceDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAttendanceInClass([FromQuery] AttendanceSearch baseSearch)
        {
            var data = await ClassService.GetAttendanceInClass(baseSearch, GetCurrentUser());
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                totalRow = data.TotalRow,
                scheduleAttendances = data.ScheduleAttendances,
                data = data.Data
            });
        }
        [HttpPut]
        [Route("attendances")]
        public async Task<IActionResult> UpdateAttendances([FromBody] AttendancesForm itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ClassService.UpdateAttendances(itemModel, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
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
        [Route("attendance")]
        public async Task<IActionResult> UpdateAttendance([FromBody] AttendanceForm itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ClassService.UpdateAttendance(itemModel, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
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
        [Route("appropriate-teacher")]
        public async Task<IActionResult> AppropriateTeacher([FromQuery] AppropriateSearch itemModel)
        {
            try
            {
                var data = await ClassService.AppropriateTeacher(itemModel);
                if (!data.Any())
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
