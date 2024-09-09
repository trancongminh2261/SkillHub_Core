using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Areas.Models;
using static LMSCore.Services.UserInformation;
using System.Collections.Generic;
using System.Data;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using static LMSCore.Utilities.ValidateUserInput;
using LMSCore.Services.Customer;

namespace LMSCore.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    [ValidateModelState]
    public class UserInformationController : BaseController
    {
        private lmsDbContext dbContext;
        private UserInformation domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public UserInformationController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new UserInformation(this.dbContext, _hostingEnvironment);
        }

        [HttpPost]
        [Route("api/UserInformation")]
        public async Task<IActionResult> Insert([FromBody] UserCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await UserInformation.Insert(new tbl_UserInformation(model), GetCurrentUser(), false, model.ProgramIds);
                    if (data != null && model.CustomerId != 0 && model.CustomerId.HasValue)
                    {
                        //cập nhật trạng thái khách hàng 
                        await CustomerService.Complete(model.CustomerId.Value, GetCurrentUser());
                    }
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
        [Route("api/UserInformation/test-appointment")]
        public async Task<IActionResult> TestAppointment([FromBody] UserAndTestAppointmentCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await UserInformation.InsertUserAndTestAppointment(model.UserModel, GetCurrentUser(), model.TestAppointmentModel);
                    if (data.Item1 != null && model.UserModel.CustomerId != 0 && model.UserModel.CustomerId.HasValue)
                    {
                        //cập nhật trạng thái khách hàng 
                        await CustomerService.Complete(model.UserModel.CustomerId.Value, GetCurrentUser());
                    }
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message });
        }

        [HttpPut]
        [Route("api/UserInformation")]
        public async Task<IActionResult> Update([FromBody] UserUpdate model)
        {
            if (ModelState.IsValid)
            {
                var user = GetCurrentUser();
                try
                {
                    var data = await UserInformation.Update(new tbl_UserInformation(model), user);
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
        [HttpDelete]
        [Route("api/UserInformation/{userInformationId}")]
        public async Task<IActionResult> Delete(int userInformationId)
        {
            try
            {
                await UserInformation.Delete(userInformationId);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }
        [HttpGet]
        [Route("api/UserInformation")]
        public async Task<IActionResult> GetAll([FromQuery] UserSearch baseSearch)
        {
            var data = await UserInformation.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpGet]
        [Route("api/UserInformation/{userInformationId}")]
        public async Task<IActionResult> GetById(int userInformationId)
        {
            var data = await UserInformation.GetById(userInformationId);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = new UserInformationModel(data) });
        }
        [HttpPut]
        [Route("api/Update_OneSignal_DeviceId")]
        public async Task<IActionResult> Update_OneSignal_DeviceId(string oneSignal_deviceId)
        {
            var data = await UserInformation.Update_OneSignal_DeviceId(oneSignal_deviceId, GetCurrentUser());
            return StatusCode((int)HttpStatusCode.OK);
        }
        // chỗ này không hiểu vì sao phải truyền BranchId vô trong khi không xử lý gì @@
        [HttpPost]
        [Route("api/UserInformation/ImportStudent")]
        public async Task<IActionResult> ImportStudent([FromQuery] int BranchId)
        {
            try
            {
                var httpRequest = HttpContext.Request;
                DataSet dsexcelRecords = new DataSet();
                IExcelDataReader reader = null;
                IFormFile Inputfile = null;
                Stream FileStream = null;
                using (var db = new lmsDbContext())
                {
                    var model = new List<RegisterModel>();
                    if (httpRequest.Form.Files.Count > 0)
                    {
                        Inputfile = httpRequest.Form.Files["File"];
                        FileStream = Inputfile.OpenReadStream();
                        if (Inputfile != null && FileStream != null)
                        {
                            if (Inputfile.FileName.EndsWith(".xls"))
                                reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
                            else if (Inputfile.FileName.EndsWith(".xlsx"))
                                reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);
                            else
                                return BadRequest(new { message = "Không đúng định dạng." });

                            dsexcelRecords = reader.AsDataSet();
                            reader.Close();

                            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                            {
                                DataTable dtStudentRecords = dsexcelRecords.Tables[0];
                                for (int i = 2; i < dtStudentRecords.Rows.Count; i++)
                                {
                                    var item = new RegisterModel
                                    {
                                        FullName = dtStudentRecords.Rows[i][0].ToString(),
                                        UserName = dtStudentRecords.Rows[i][1].ToString(),
                                        Email = dtStudentRecords.Rows[i][2].ToString(),
                                        Mobile = dtStudentRecords.Rows[i][3].ToString(),
                                        Password = Encryptor.Encrypt(dtStudentRecords.Rows[i][4].ToString())
                                    };

                                    if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.UserName))
                                    {
                                        return BadRequest(new { message = "Vui lòng điền đầy đủ họ tên và tên đăng nhập" });
                                    }

                                    model.Add(item);
                                }
                            }
                            else
                            {
                                return BadRequest(new { message = "Không có dữ liệu." });
                            }
                        }
                        else
                        {
                            return BadRequest(new { message = "File lỗi." });
                        }
                    }
                    await UserInformation.ImportData(model, GetCurrentUser());
                    return Ok(new { message = "Thêm thành công" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("api/UserInformation/ImportStudent/v2")]
        public async Task<IActionResult> ImportStudentV2([FromQuery] string BranchIds)
        {
            try
            {
                var httpRequest = HttpContext.Request;
                DataSet dsexcelRecords = new DataSet();
                IExcelDataReader reader = null;
                IFormFile Inputfile = null;
                Stream FileStream = null;
                var data = new UserImport();
                using (var db = new lmsDbContext())
                {
                    var model = new List<RegisterModel>();
                    if (httpRequest.Form.Files.Count > 0)
                    {
                        Inputfile = httpRequest.Form.Files["File"];
                        FileStream = Inputfile.OpenReadStream();
                        if (Inputfile != null && FileStream != null)
                        {
                            if (Inputfile.FileName.EndsWith(".xls"))
                                reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
                            else if (Inputfile.FileName.EndsWith(".xlsx"))
                                reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);
                            else
                                return BadRequest(new { message = "Không đúng định dạng." });

                            dsexcelRecords = reader.AsDataSet();
                            reader.Close();

                            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                            {
                                DataTable dtStudentRecords = dsexcelRecords.Tables[0];
                                for (int i = 2; i < dtStudentRecords.Rows.Count; i++)
                                {
                                    var item = new RegisterModel
                                    {
                                        FullName = dtStudentRecords.Rows[i][0].ToString(),
                                        UserName = dtStudentRecords.Rows[i][1].ToString(),
                                        Email = dtStudentRecords.Rows[i][2].ToString(),
                                        Mobile = dtStudentRecords.Rows[i][3].ToString(),
                                        Password = Encryptor.Encrypt(dtStudentRecords.Rows[i][4].ToString())
                                    };

                                    if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.UserName))
                                    {
                                        return BadRequest(new { message = "Vui lòng điền đầy đủ họ tên và tên đăng nhập" });
                                    }

                                    model.Add(item);
                                }
                            }
                            else
                            {
                                return BadRequest(new { message = "Không có dữ liệu." });
                            }
                        }
                        else
                        {
                            return BadRequest(new { message = "File lỗi." });
                        }
                    }
                    data.BranchIds = BranchIds;
                    data.DataImports = model;
                    await UserInformation.ImportDataV2(data, GetCurrentUser());
                    return Ok(new { message = "Thêm thành công" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Route("api/UserInformation/user-available/{roleId}")]
        public async Task<IActionResult> GetUserAvailable(int roleId)
        {
            var data = await UserInformation.GetUserAvailable(new UserInformationAvailableSearch { RoleId = roleId }, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("api/UserInformation/user-available")]
        public async Task<IActionResult> GetUserAvailable([FromQuery] UserInformationAvailableSearch baseSearch)
        {
            var data = await UserInformation.GetUserAvailable(baseSearch, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("api/UserInformation/ResendOTP")]
        public async Task<IActionResult> ResendOTP([FromQuery] UserInformationAvailableSearch baseSearch)
        {
            var data = await UserInformation.GetUserAvailable(baseSearch, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPost]
        [Route("api/UserInformation/InsertParent")]
        public async Task<IActionResult> InsertParent([FromBody] ParentCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await UserInformation.InsertParent(new tbl_UserInformation(model), GetCurrentUser(), false, model.ProgramIds, model.studentId);
                    if (data != null && model.CustomerId != 0 && model.CustomerId.HasValue)
                    {
                        //cập nhật trạng thái khách hàng 
                        await CustomerService.Complete(model.CustomerId.Value, GetCurrentUser());
                    }
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
        [Route("api/UserInformation/get-parent-user/{userInformationId}")]
        public async Task<IActionResult> GetParentById(int userInformationId)
        {
            try
            {
                var data = await UserInformation.GetParentById(userInformationId);
                if (data == null)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }

        ///// <summary>
        ///// export excel
        ///// </summary>
        ///// <returns></returns>
        [HttpGet]
        [Route("api/UserInformation/ExportExcel")]
        public async Task<IActionResult> ExportStudentExcel([FromQuery] UserExportSearch baseSearch)
        {
            try
            {
                string baseUrl = Request.Scheme + "://" + Request.Host;
                var data = await UserInformation.PrepareDataToExport(baseSearch, GetCurrentUser());
                string result;
                //học viên
                if (baseSearch.Type == 1)
                {
                    var convertData = data.Select(i => new StudentExport(i)).ToList();
                    string folderToSave = "UserExport";
                    string template = "TemplateExport.xlsx";
                    string fileExportName = "DanhSachHocVien";
                    List<string> listTitle = new List<string>
                    {
                        "Họ và tên",
                        "MSNV",
                        "Tài khoản",
                        "Ngày sinh",
                        "Giới tính",
                        "Số điện thoại",
                        "Email",
                        "Trạng thái học",
                        "Tư vấn viên",
                        "Công việc"
                    };
                    result = ExcelExportService.ExportV2(template, folderToSave, convertData, listTitle, fileExportName, baseUrl);
                }
                //nhân viên
                else
                {
                    var convertData = data.Select(i => new StaffExport(i)).ToList();
                    string folderToSave = "UserExport";
                    string template = "TemplateExport.xlsx";
                    string fileExportName = "DanhSachNhanVien";
                    List<string> listTitle = new List<string>
                    {
                        "Họ và tên",
                        "MSNV",
                        "Tài khoản",
                        "Ngày sinh",
                        "Giới tính",
                        "Số điện thoại",
                        "Email",
                        "Chức vụ"
                    };
                    result = ExcelExportService.ExportV2(template, folderToSave, convertData, listTitle, fileExportName, baseUrl);
                }
                return StatusCode((int)HttpStatusCode.OK, new { statusCode = (int)HttpStatusCode.OK, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/UserInformation/export-student-template/{branchId}")]
        public async Task<IActionResult> ExportStudentTemplate(int branchId)
        {
            try
            {
                string baseUrl = Request.Scheme + "://" + Request.Host;
                var result = UserInformation.ExportExampleStudent(branchId, baseUrl, GetCurrentUser());
                if (result == null) return StatusCode((int)HttpStatusCode.BadRequest);
                return StatusCode((int)HttpStatusCode.OK, new { statusCode = (int)HttpStatusCode.OK, data = result });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpPost]
        [Route("api/UserInformation/view-student-excel")]
        public async Task<IActionResult> ViewStudentExcel()
        {
            try
            {
                var httpRequest = HttpContext.Request;
                DataSet dsexcelRecords = new DataSet();
                IExcelDataReader reader = null;
                IFormFile Inputfile = null;
                Stream FileStream = null;

                using (var db = new lmsDbContext())
                {
                    var data = new List<StudentModelExampleExport>();

                    if (httpRequest.Form.Files.Count > 0)
                    {
                        Inputfile = httpRequest.Form.Files.GetFile("File");
                        FileStream = Inputfile.OpenReadStream();

                        if (Inputfile != null && FileStream != null)
                        {
                            if (Inputfile.FileName.EndsWith(".xls"))
                                reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
                            else if (Inputfile.FileName.EndsWith(".xlsx"))
                                reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);
                            else
                                return BadRequest(new { message = "Không đúng định dạng." });

                            dsexcelRecords = reader.AsDataSet();
                            reader.Close();

                            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                            {
                                DataTable dtStudentRecords = dsexcelRecords.Tables[0];
                                FormartDate formart = new FormartDate();
                                for (int i = 3; i < dtStudentRecords.Rows.Count; i++)
                                {
                                    var item = new StudentModelExampleExport
                                    {
                                        FullName = dtStudentRecords.Rows[i][0].ToString(),
                                        Gender = dtStudentRecords.Rows[i][1].ToString(),
                                        UserName = dtStudentRecords.Rows[i][3].ToString(),
                                        Password = dtStudentRecords.Rows[i][4].ToString(),
                                        Email = dtStudentRecords.Rows[i][5].ToString(),
                                        Mobile = dtStudentRecords.Rows[i][6].ToString(),
                                        LearningNeedName = dtStudentRecords.Rows[i][7].ToString(),
                                        PurposeName = dtStudentRecords.Rows[i][8].ToString(),
                                        SourceName = dtStudentRecords.Rows[i][9].ToString(),
                                        SaleName = dtStudentRecords.Rows[i][10].ToString(),
                                    };
                                    string dobString = dtStudentRecords.Rows[i][2].ToString();
                                    DateTime dob;
                                    string[] formats = formart.formats;
                                    bool validDate = false;

                                    foreach (var format in formats)
                                    {
                                        if (DateTime.TryParseExact(dobString, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dob))
                                        {
                                            // Định dạng ngày theo chuẩn "dd/MM/yyyy"
                                            item.DOB = dob.ToString("dd/MM/yyyy");
                                            validDate = true;
                                            break;
                                        }
                                    }
                                    if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.UserName))
                                    {
                                        return BadRequest(new { message = "Vui lòng điền đầy đủ họ tên và tên đăng nhập" });
                                    }

                                    data.Add(item);
                                }
                            }
                            else
                            {
                                return BadRequest(new { message = "Không có dữ liệu." });
                            }
                        }
                        else
                        {
                            return BadRequest(new { message = "File lỗi." });
                        }
                    }
                    else
                    {
                        return BadRequest(new { message = "Không có file được tải lên." });
                    }

                    return Ok(new { message = "Thêm thành công", totalRow = data.Count, data = data });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("api/UserInformation/insert-student-excel/{branchId}")]
        public async Task<IActionResult> InsertStudentExcel([FromBody] List<InsertStudentExcel> model, int branchId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await UserInformation.InsertStudentExcel(model, branchId, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.Count, data = data });

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
        [Route("api/UserInformation/export-employee-template")]
        public async Task<IActionResult> ExportEmployeeTemplate()
        {
            try
            {
                string baseUrl = Request.Scheme + "://" + Request.Host;
                var result = UserInformation.ExportExampleEmployee(GetCurrentUser(), baseUrl);
                if (result.Result == null) return StatusCode((int)HttpStatusCode.BadRequest);
                return StatusCode((int)HttpStatusCode.OK, new { statusCode = (int)HttpStatusCode.OK, data = result });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpPost]
        [Route("api/UserInformation/view-employee-excel")]
        public async Task<IActionResult> ViewEmployeeExcel()
        {
            try
            {
                var httpRequest = HttpContext.Request;
                DataSet dsexcelRecords = new DataSet();
                IExcelDataReader reader = null;
                IFormFile Inputfile = null;
                Stream FileStream = null;

                if (httpRequest.Form.Files.Count > 0)
                {
                    Inputfile = httpRequest.Form.Files.GetFile("File");
                    FileStream = Inputfile.OpenReadStream();
                    if (Inputfile != null && FileStream != null)
                    {
                        if (Inputfile.FileName.EndsWith(".xls"))
                            reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
                        else if (Inputfile.FileName.EndsWith(".xlsx"))
                            reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);
                        else
                            return BadRequest(new { message = "Không đúng định dạng." });

                        dsexcelRecords = reader.AsDataSet();
                        reader.Close();

                        if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                        {
                            DataTable dtEmployeeRecords = dsexcelRecords.Tables[0];
                            var data = new List<EmployeeModelExampleExport>();
                            FormartDate formart = new FormartDate();
                            for (int i = 3; i < dtEmployeeRecords.Rows.Count; i++)
                            {
                                var item = new EmployeeModelExampleExport
                                {
                                    FullName = dtEmployeeRecords.Rows[i][0].ToString(),
                                    Gender = dtEmployeeRecords.Rows[i][1].ToString(),
                                    UserName = dtEmployeeRecords.Rows[i][3].ToString(),
                                    Password = dtEmployeeRecords.Rows[i][4].ToString(),
                                    RoleName = dtEmployeeRecords.Rows[i][5].ToString(),
                                    Email = dtEmployeeRecords.Rows[i][6].ToString(),
                                    Mobile = dtEmployeeRecords.Rows[i][7].ToString(),
                                };
                                string dobString = dtEmployeeRecords.Rows[i][2].ToString();
                                DateTime dob;
                                string[] formats = formart.formats;
                                bool validDate = false;

                                foreach (var format in formats)
                                {
                                    if (DateTime.TryParseExact(dobString, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dob))
                                    {
                                        // Định dạng ngày theo chuẩn "dd/MM/yyyy"
                                        item.DOB = dob.ToString("dd/MM/yyyy");
                                        validDate = true;
                                        break;
                                    }
                                }
                                if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.UserName))
                                {
                                    return BadRequest(new { message = "Vui lòng điền đầy đủ họ tên và tên đăng nhập" });
                                }

                                data.Add(item);
                            }

                            return Ok(new { message = "Thêm thành công", totalRow = data.Count, data = data });
                        }
                        else
                        {
                            return BadRequest(new { message = "Không có dữ liệu." });
                        }
                    }
                    else
                    {
                        return BadRequest(new { message = "File lỗi." });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Không có file được tải lên." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/UserInformation/insert-employee-excel/{branchId}")]
        public async Task<IActionResult> InsertEmployeeExcel([FromBody] List<InsertEmployeeExcel> model, int branchId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await UserInformation.InsertEmployeeExcel(model, branchId, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.Count, data = data });

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
        /// Lấy tất cả các tư vấn viên bao gồm các tư vấn viên bị khóa và đã vị xóa
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/UserInformation/get-all-saler")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetAllSaler([FromQuery] SalerSearch baseSearch)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await UserInformation.GetAllSaler(baseSearch, GetCurrentUser());
                    if (data.TotalRow == 0)
                        return StatusCode((int)HttpStatusCode.NoContent);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
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
        /// Thêm 1 tư vấn viên cho nhiều học sinh
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="branchId"></param>
        /// <param name="saleId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/UserInformation/students-for-saler")]
        [ClaimsAuthorize]
        public async Task<IActionResult> StudentsForSaler([FromBody] List<SetStudentForSaler> itemModel, int branchId, int saleId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await UserInformation.StudentsForSaler(itemModel, branchId, saleId, GetCurrentUser());
                    if (data == true) return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
                    else return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Thất bại !" });
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
        /// Lấy danh sách học sinh cho Popup thêm 1 tư vấn viên cho nhiều học sinh
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/UserInformation/student-search")]
        [ClaimsAuthorize]
        public async Task<IActionResult> StudentForSalerSearch([FromQuery] StudentForSalerSearch baseSearch)
        {
            try
            {
                var data = await UserInformation.GetStudentForSaler(baseSearch, GetCurrentUser());
                if (data.TotalRow == 0)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorize]
        [Route("api/UserInformation/employee-in-branch")]
        public async Task<IActionResult> GetEmployeeInBranch(int employeeId)
        {
            var data = await UserInformation.GetEmployeeInBranch(employeeId);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPut]
        [ClaimsAuthorize]
        [Route("api/UserInformation/update-employee-branch")]
        public async Task<IActionResult> UpdateEmployeeBranch(EmployeeBranchUpdate itemModel)
        {
            if (ModelState.IsValid)
            {
                var user = GetCurrentUser();
                try
                {
                    var data = await UserInformation.UpdateEmployeeBranch(itemModel, GetCurrentUser());
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
    }
}
