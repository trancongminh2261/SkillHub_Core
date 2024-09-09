using ExcelDataReader;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using System.Net;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using LMSCore.Services.SampleTranscript;
using LMSCore.Services.InitData;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.PermissionService;

namespace LMSCore.Areas.ControllerAPIs.InitData
{
    [Route("api/InitData")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class InitDataController : ControllerBase
    {
        private lmsDbContext dbContext;
        private InitDataService domainService;
        public InitDataController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new InitDataService(this.dbContext);
        }
        [HttpPost]
        [Route("import-data")]
        public async Task<IActionResult> ImportData()
        {
            try
            {
                var httpRequest = HttpContext.Request;
                DataSet dsexcelRecords = new DataSet();
                IExcelDataReader reader = null;
                IFormFile inputFile = null;
                Stream fileStream = null;
                string datenow = DateTime.Now.ToString("yyyy-MM-dd");
                var defaultProp = $"Enable, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy";
                var defaultValues = $"1, '{datenow}', N'Hệ thống', '{datenow}', N'Hệ thống'";
                var listRole = new List<RoleModel>()
                    {
                        new RoleModel { Id = ((int)RoleEnum.admin), Name = "Admin" },
                        new RoleModel { Id = ((int)RoleEnum.teacher), Name = "Giáo viên" },
                        new RoleModel { Id = ((int)RoleEnum.student), Name = "Học viên" },
                        new RoleModel { Id = ((int)RoleEnum.manager), Name = "Quản lý" },
                        new RoleModel { Id = ((int)RoleEnum.sale), Name = "Tư vấn viên" },
                        new RoleModel { Id = ((int)RoleEnum.accountant), Name = "Kế toán" },
                        new RoleModel { Id = ((int)RoleEnum.academic), Name = "Học vụ" },
                        new RoleModel { Id = ((int)RoleEnum.parents), Name = "Phụ huynh" },
                        new RoleModel { Id = ((int)RoleEnum.tutor), Name = "Trợ giảng" },
                    };

                using (var db = new lmsDbContext())
                {
                    if (httpRequest.Form.Files.Count > 0)
                    {
                        inputFile = httpRequest.Form.Files["File"];
                        fileStream = inputFile.OpenReadStream();
                        if (inputFile != null && fileStream != null)
                        {
                            if (inputFile.FileName.EndsWith(".xls"))
                                reader = ExcelReaderFactory.CreateBinaryReader(fileStream);
                            else if (inputFile.FileName.EndsWith(".xlsx"))
                                reader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                            else
                                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không đúng định dạng." });

                            dsexcelRecords = reader.AsDataSet();
                            reader.Close();                           

                            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                            {
                                //danh sách tỉnh/ thành
                                var listArea = await domainService.GetArea();
                                //danh sách quận/ huyện
                                var listDistrict = await domainService.GetDistrict();
                                //danh sách xã/ phường
                                var listWard = await domainService.GetWard();

                                var dtSheet = dsexcelRecords.Tables;
                                var totalSheet = dsexcelRecords.Tables.Count;
                                //danh sách câu lệnh insert cho 1 bảng
                                var listQuery = "";
                                //câu lệnh insert 1 dữ liệu trong 1 bảng
                                var query = "";

                                #region trung tâm
                                var branchSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("trung tâm"));
                                if(branchSheet != null)
                                {
                                    var branchQuery = $"Insert Into tbl_Branch( Code, Name, Address, AreaId, DistrictId, WardId, Mobile, Email, {defaultProp}) Values (N'[Code]', N'[Name]', N'[Address]', [AreaId], [DistrictId], [WardId], N'[Mobile]', N'[Email]', {defaultValues}); ";
                                    for (int i = 2; i < branchSheet.Rows.Count; i++)
                                    {
                                        var canImport = await domainService.ValidBranch(branchSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = branchQuery;
                                            query = query.Replace("[Code]", branchSheet.Rows[i][0].ToString());
                                            query = query.Replace("[Name]", branchSheet.Rows[i][1].ToString());
                                            query = query.Replace("[Address]", branchSheet.Rows[i][2].ToString());
                                            var area = listArea.FirstOrDefault(x => x.Name.ToLower() == branchSheet.Rows[i][3].ToString().ToLower());
                                            query = query.Replace("[AreaId]", area == null ? "NULL" : area.Id.ToString());
                                            var district = listDistrict.FirstOrDefault(x => x.Name.ToLower() == branchSheet.Rows[i][4].ToString().ToLower());
                                            query = query.Replace("[DistrictId]", district == null ? "NULL" : district.Id.ToString());
                                            var ward = listWard.FirstOrDefault(x => x.Name.ToLower() == branchSheet.Rows[i][5].ToString().ToLower());
                                            query = query.Replace("[WardId]", ward == null ? "NULL" : ward.Id.ToString());
                                            query = query.Replace("[Mobile]", branchSheet.Rows[i][6].ToString());
                                            query = query.Replace("[Email]", branchSheet.Rows[i][7].ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Trung tâm", listQuery);                                 
                                }
                                var listBranch = await domainService.GetBranch();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region phòng học
                                var roomSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("phòng học"));
                                if(roomSheet != null)
                                {
                                    var roomQuery = $"Insert Into tbl_Room( BranchId, Code, Name, {defaultProp}) Values ( [BranchId], N'[Code]', N'[Name]', {defaultValues}); ";
                                    for (int i = 2; i < roomSheet.Rows.Count; i++)
                                    {
                                        var branch = listBranch.FirstOrDefault(x => x.Code.Trim().ToLower() == roomSheet.Rows[i][0].ToString().Trim().ToLower());
                                        if (branch == null)
                                            throw new Exception($"Danh sách phòng học không tìm thấy thông tin phòng ban có mã {roomSheet.Rows[i][0].ToString()}");
                                        var canImport = await domainService.ValidRoom(branch.Id, roomSheet.Rows[i][1].ToString());
                                        if (canImport)
                                        {
                                            query = roomQuery;
                                            query = query.Replace("[BranchId]", branch.Id.ToString());
                                            query = query.Replace("[Code]", roomSheet.Rows[i][1].ToString());
                                            query = query.Replace("[Name]", roomSheet.Rows[i][2].ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Phòng học", listQuery);
                                }                               
                                var listRoom = await domainService.GetRoom();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region câu hỏi thường gặp
                                var frequentlyQuestionSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("câu hỏi thường gặp"));
                                if (frequentlyQuestionSheet != null)
                                {
                                    var frequentlyQuestionQuery = $"Insert Into tbl_FrequentlyQuestion( Question, Answer, RoleIds, {defaultProp}) Values ( N'[Question]', N'[Answer]', N'[RoleIds]', {defaultValues}); ";
                                    for (int i = 2; i < frequentlyQuestionSheet.Rows.Count; i++)
                                    {
                                        var canImport = await domainService.ValidFrequentlyQuestion(frequentlyQuestionSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = frequentlyQuestionQuery;
                                            query = query.Replace("[Question]", frequentlyQuestionSheet.Rows[i][0].ToString());
                                            query = query.Replace("[Answer]", frequentlyQuestionSheet.Rows[i][1].ToString());                                        
                                            var roleNameSplit = frequentlyQuestionSheet.Rows[i][2].ToString().Split(',');
                                            var roles = listRole.Where(x => roleNameSplit.Any(y => y.Trim().ToLower() == x.Name.Trim().ToLower())).Select(x => x.Id.ToString());
                                            var roleIds = String.Join(",", roles);
                                            query = query.Replace("[RoleIds]", roleIds);

                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Câu hỏi thường gặp", listQuery);
                                }                                
                                listQuery = "";
                                query = "";
                                #endregion

                                #region nhu cầu học
                                var learningNeedSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("nhu cầu học"));
                                if(learningNeedSheet != null)
                                {
                                    var learningNeedQuery = $"Insert Into tbl_LearningNeed( Name, {defaultProp}) Values ( N'[Name]', {defaultValues}); ";
                                    for (int i = 2; i < learningNeedSheet.Rows.Count; i++)
                                    {
                                        var canImport = await domainService.ValidLearningNeed(learningNeedSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = learningNeedQuery;
                                            query = query.Replace("[Name]", learningNeedSheet.Rows[i][0].ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Nhu cầu học", listQuery);
                                }                               
                                var listLearningNeed = await domainService.GetLearningNeed();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region nguồn khách hàng
                                var sourceSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("nguồn khách hàng"));
                                if(sourceSheet != null)
                                {
                                    var sourceQuery = $"Insert Into tbl_Source( Name, {defaultProp}) Values ( N'[Name]', {defaultValues}); ";
                                    for (int i = 2; i < sourceSheet.Rows.Count; i++)
                                    {
                                        var canImport = await domainService.ValidSource(sourceSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = sourceQuery;
                                            query = query.Replace("[Name]", sourceSheet.Rows[i][0].ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Nguồn khách hàng", listQuery);
                                }                              
                                var listSource = await domainService.GetSource();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region công việc
                                var jobSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("công việc"));
                                if(jobSheet != null)
                                {
                                    var jobQuery = $"Insert Into tbl_Job( Name, {defaultProp}) Values ( N'[Name]', {defaultValues}); ";
                                    for (int i = 2; i < jobSheet.Rows.Count; i++)
                                    {
                                        var canImport = await domainService.ValidJob(jobSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = jobQuery;
                                            query = query.Replace("[Name]", jobSheet.Rows[i][0].ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Công việc", listQuery);
                                }                     
                                var listJob = await domainService.GetJob();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region trạng thái khách hàng
                                var customerStatusSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("trạng thái khách hàng"));
                                if (customerStatusSheet != null)
                                {
                                    var customerStatusQuery = $"Insert Into tbl_CustomerStatus( Type, Name, ColorCode, {defaultProp}) Values ( 2, N'[Name]', '[ColorCode]', {defaultValues}); ";
                                    for (int i = 2; i < customerStatusSheet.Rows.Count; i++)
                                    {
                                        var canImport = await domainService.ValidCustomerStatus(customerStatusSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = customerStatusQuery;
                                            query = query.Replace("[Name]", customerStatusSheet.Rows[i][0].ToString());
                                            query = query.Replace("[ColorCode]", await domainService.GenColorCode());
                                            listQuery = listQuery + query;
                                        }

                                    }
                                    await domainService.ExecuteQuery("Trạng thái khách hàng", listQuery);
                                }                             
                                var listCustomerStatus = await domainService.GetCustomerStatus();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region mục đích học
                                var purposeSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("mục đích học"));
                                if(purposeSheet != null)
                                {
                                    var purposeQuery = $"Insert Into tbl_Purpose( Name, {defaultProp}) Values ( N'[Name]', {defaultValues}); ";
                                    for (int i = 2; i < purposeSheet.Rows.Count; i++)
                                    {
                                        var canImport = await domainService.ValidPurpose(purposeSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = purposeQuery;
                                            query = query.Replace("[Name]", purposeSheet.Rows[i][0].ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Công việc", listQuery);
                                }
                               
                                var listPurpose = await domainService.GetPurpose();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region chuyên môn
                                var gradeSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("chuyên môn"));
                                if(gradeSheet != null)
                                {
                                    var gradeQuery = $"Insert Into tbl_Grade( Code, Name, {defaultProp}) Values ( N'[Code]', N'[Name]', {defaultValues}); ";
                                    for (int i = 2; i < gradeSheet.Rows.Count; i++)
                                    {
                                        var canImport = await domainService.ValidGrade(gradeSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = gradeQuery;
                                            query = query.Replace("[Code]", gradeSheet.Rows[i][0].ToString());
                                            query = query.Replace("[Name]", gradeSheet.Rows[i][1].ToString());
                                            listQuery = listQuery + query;
                                        }

                                    }
                                    await domainService.ExecuteQuery("Chuyên môn", listQuery);
                                }                            
                                var listGrade = await domainService.GetGrade();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region chương trình học
                                var programSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("chương trình học"));
                                if(programSheet != null)
                                {
                                    var programQuery = $"Insert Into tbl_Program( Code, Name, Price, GradeId, Description, [Index], {defaultProp}) Values ( N'[Code]', N'[Name]', [Price], [GradeId], N'[Description]', [Index_Value], {defaultValues}); ";
                                    for (int i = 2; i < programSheet.Rows.Count; i++)
                                    {
                                        var grade = listGrade.FirstOrDefault(x => x.Code.Trim().ToLower() == programSheet.Rows[i][3].ToString().Trim().ToLower());
                                        if (grade == null)
                                            throw new Exception($"Danh sách chương trình học không tìm thấy thông tin chuyên môn có mã {programSheet.Rows[i][3].ToString()}");
                                        var canImport = await domainService.ValidProgram(grade.Id, programSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = programQuery;
                                            query = query.Replace("[Code]", programSheet.Rows[i][0].ToString());
                                            query = query.Replace("[Name]", programSheet.Rows[i][1].ToString());
                                            query = query.Replace("[Price]", programSheet.Rows[i][2].ToString());
                                            query = query.Replace("[GradeId]", grade.Id.ToString());
                                            query = query.Replace("[Description]", programSheet.Rows[i][4].ToString());
                                            query = query.Replace("[Index_Value]", await domainService.ProgramNewIndex(grade.Id));
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Chương trình học", listQuery);
                                }
                                
                                var listProgram = await domainService.GetProgram();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region giáo trình
                                var curriculumSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("giáo trình"));
                                if(curriculumSheet != null)
                                {
                                    var curriculumQuery = $"Insert Into tbl_Curriculum( ProgramId, Name, Lesson, Time, {defaultProp}) Values ( [ProgramId], N'[Name]', [Lesson], [Time], {defaultValues}); ";
                                    for (int i = 2; i < curriculumSheet.Rows.Count; i++)
                                    {
                                        query = curriculumQuery;
                                        var program = listProgram.FirstOrDefault(x => x.Code.Trim().ToLower() == curriculumSheet.Rows[i][0].ToString().Trim().ToLower());
                                        if (program == null)
                                            throw new Exception($"Danh sách giáo trình học không tìm thấy thông tin chương trình học có mã {curriculumSheet.Rows[i][0].ToString()}");
                                        var canImport = await domainService.ValidCurriculum(program.Id, curriculumSheet.Rows[i][1].ToString());
                                        if (canImport)
                                        {
                                            query = query.Replace("[ProgramId]", program.Id.ToString());
                                            query = query.Replace("[Name]", curriculumSheet.Rows[i][1].ToString());
                                            query = query.Replace("[Lesson]", curriculumSheet.Rows[i][2].ToString());
                                            query = query.Replace("[Time]", curriculumSheet.Rows[i][3].ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Giáo trình", listQuery);
                                }
                               
                                var listCurriculum = await domainService.GetCurriculum();
                                listQuery = "";
                                query = "";
                                #endregion

                                /*#region nhân viên
                                var staffSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("nhân viên"));
                                if(staffSheet != null)
                                {
                                    var staffQuery = $"Insert Into tbl_UserInformation( UserCode, FullName, UserName, Password, RoleId, RoleName, Gender, Email, Mobile, BranchIds, StatusId, ActiveDate, {defaultProp}) Values ( N'[UserCode]', N'[FullName]', N'[UserName]', '[Password]', [RoleId], N'[RoleName]', [Gender], N'[Email]', N'[Mobile]', '[BranchIds]', 0, {datenow}, {defaultValues}); ";
                                    for (int i = 2; i < staffSheet.Rows.Count; i++)
                                    {
                                        query = staffQuery;
                                        var canImport = await domainService.ValidUser(staffSheet.Rows[i][3].ToString(), staffSheet.Rows[i][4].ToString().Trim().Replace("'", ""));
                                        if (canImport)
                                        {
                                            var roleId = 0;
                                            if (staffSheet.Rows[i][1].ToString() == "Admin")
                                            {
                                                roleId = (int)RoleEnum.admin;
                                                query = query.Replace("[RoleId]", roleId.ToString());

                                            }
                                            else if (staffSheet.Rows[i][1].ToString() == "Quản lý")
                                            {
                                                roleId = (int)RoleEnum.manager;
                                                query = query.Replace("[RoleId]", roleId.ToString());
                                            }
                                            else if (staffSheet.Rows[i][1].ToString() == "Giáo viên" || staffSheet.Rows[i][1].ToString() == "Giảng viên")
                                            {
                                                roleId = (int)RoleEnum.teacher;
                                                query = query.Replace("[RoleId]", roleId.ToString());
                                            }
                                            else if (staffSheet.Rows[i][1].ToString() == "Kế toán")
                                            {
                                                roleId = (int)RoleEnum.accountant;
                                                query = query.Replace("[RoleId]", roleId.ToString());
                                            }
                                            else if (staffSheet.Rows[i][1].ToString() == "Trợ giảng")
                                            {
                                                roleId = (int)RoleEnum.tutor;
                                                query = query.Replace("[RoleId]", roleId.ToString());
                                            }
                                            else if (staffSheet.Rows[i][1].ToString() == "Học vụ")
                                            {
                                                roleId = (int)RoleEnum.academic;
                                                query = query.Replace("[RoleId]", roleId.ToString());
                                            }
                                            else
                                            {
                                                roleId = (int)RoleEnum.sale;
                                                query = query.Replace("[RoleId]", roleId.ToString());
                                            }
                                            query = query.Replace("[RoleName]", staffSheet.Rows[i][1].ToString());

                                            string baseCode = roleId == ((int)RoleEnum.admin) ? "QTV"
                                                : roleId == ((int)RoleEnum.student) ? "HV"
                                                : roleId == ((int)RoleEnum.parents) ? "PH" : "NV";
                                            int count = await db.tbl_UserInformation.CountAsync(x => ((x.RoleId == roleId && baseCode != "NV")
                                                        || (x.RoleId != ((int)RoleEnum.admin) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && baseCode == "NV"))
                                                        && x.CreatedOn.Value.Year == DateTime.Now.Year
                                                        && x.CreatedOn.Value.Month == DateTime.Now.Month
                                                        && x.CreatedOn.Value.Day == DateTime.Now.Day);

                                            query = query.Replace("[UserCode]", AssetCRM.InitCode(baseCode, DateTime.Now, count + 1));
                                            query = query.Replace("[FullName]", staffSheet.Rows[i][0].ToString());

                                            if (staffSheet.Rows[i][2].ToString() == "Nữ")
                                            {
                                                query = query.Replace("[Gender]", "2");

                                            }
                                            else if (staffSheet.Rows[i][2].ToString() == "Nam")
                                            {
                                                query = query.Replace("[Gender]", "1");
                                            }
                                            else
                                            {
                                                query = query.Replace("[Gender]", "0");
                                            }

                                            query = query.Replace("[UserName]", staffSheet.Rows[i][3].ToString());
                                            query = query.Replace("[Password]", Encryptor.Encrypt(staffSheet.Rows[i][3].ToString()));
                                            query = query.Replace("[Email]", staffSheet.Rows[i][3].ToString());
                                            query = query.Replace("[Mobile]", staffSheet.Rows[i][4].ToString().Trim().Replace("'", ""));

                                            var branchCodeSplit = staffSheet.Rows[i][5].ToString().Split(',');
                                            var branchs = listBranch.Where(x => branchCodeSplit.Any(y => y == x.Code)).Select(x => x.Id.ToString());
                                            var branchIds = String.Join(",", branchs);
                                            query = query.Replace("[BranchIds]", branchIds);

                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Nhân viên", listQuery);
                                }
                                
                                var listStaff = await domainService.GetStaff();
                                listQuery = "";
                                query = "";
                                #endregion*/

                                /*#region giáo viên dạy chương trình
                                var teacherInProgramSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("giáo viên dạy chương trình"));
                                if(teacherInProgramSheet != null)
                                {
                                    var teacherInProgramQuery = $"Insert Into tbl_TeacherInProgram( ProgramId, TeacherId, TeachingFee, {defaultProp}) Values ( [ProgramId], [TeacherId], [TeachingFee], {defaultValues}); ";
                                    for (int i = 2; i < teacherInProgramSheet.Rows.Count; i++)
                                    {
                                        query = teacherInProgramQuery;
                                        var canImport = await domainService.ValidTeacherInProgram(teacherInProgramSheet.Rows[i][0].ToString(), teacherInProgramSheet.Rows[i][1].ToString());
                                        if (canImport)
                                        {
                                            var program = listProgram.FirstOrDefault(x => x.Code.ToLower() == teacherInProgramSheet.Rows[i][0].ToString().ToLower());
                                            if (program == null)
                                                throw new Exception($"Danh sách giáo viên dạy chương trình không tìm thấy thông tin chương trình học có mã {teacherInProgramSheet.Rows[i][0].ToString()}");
                                            query = query.Replace("[ProgramId]", program.Id.ToString());
                                            var teacher = listStaff.FirstOrDefault(x => x.Code.ToLower() == teacherInProgramSheet.Rows[i][1].ToString().ToLower());
                                            if (teacher == null)
                                                throw new Exception($"Danh sách giáo viên dạy chương trình không tìm thấy thông tin giáo viên có mã {teacherInProgramSheet.Rows[i][1].ToString()}");
                                            query = query.Replace("[TeacherId]", teacher.Id.ToString());
                                            query = query.Replace("[TeachingFee]", teacherInProgramSheet.Rows[i][2].ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Giáo viên dạy chương trình", listQuery);
                                }                           
                                listQuery = "";
                                query = "";
                                #endregion*/

                                #region ca học
                                var studyTimeSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("ca học"));
                                if(studyTimeSheet != null)
                                {                               
                                    var studyTimeQuery = $"Insert Into tbl_StudyTime( Name, StartTime, EndTime, Time, {defaultProp}) Values ( N'[Name]', N'[StartTime]', N'[EndTime]', [Time], {defaultValues}); ";
                                    for (int i = 2; i < studyTimeSheet.Rows.Count; i++)
                                    {
                                        query = studyTimeQuery;
                                        var canImport = await domainService.ValidStudyTime(studyTimeSheet.Rows[i][1].ToString(), studyTimeSheet.Rows[i][2].ToString());
                                        if (canImport)
                                        {
                                            query = query.Replace("[Name]", $"{studyTimeSheet.Rows[i][1].ToString()} - {studyTimeSheet.Rows[i][2].ToString()}");
                                            query = query.Replace("[Time]", studyTimeSheet.Rows[i][0].ToString());
                                            query = query.Replace("[StartTime]", studyTimeSheet.Rows[i][1].ToString());
                                            query = query.Replace("[EndTime]", studyTimeSheet.Rows[i][2].ToString());
                                            listQuery = listQuery + query;
                                        }

                                    }
                                    await domainService.ExecuteQuery("Ca học", listQuery);
                                }
                                
                                var listStudyTime = await domainService.GetStudyTime();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region gói học phí
                                var tuitionPackageSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("gói học phí"));
                                if(tuitionPackageSheet != null)
                                {
                                    var studyTimeQuery = $"Insert Into tbl_TuitionPackage( Code, Months, DiscountType, DiscountTypeName, Discount, MaxDiscount, Description, {defaultProp}) Values ( N'[Code]', [Months], [DiscountType], N'[DiscountTypeName]', [Discount], [MaxDiscount], N'[Description]', {defaultValues}); ";
                                    for (int i = 2; i < tuitionPackageSheet.Rows.Count; i++)
                                    {
                                        query = studyTimeQuery;
                                        var canImport = await domainService.ValidTuitionPackage(tuitionPackageSheet.Rows[i][1].ToString());
                                        if (canImport)
                                        {
                                            query = query.Replace("[Code]", tuitionPackageSheet.Rows[i][0].ToString());
                                            query = query.Replace("[Months]", tuitionPackageSheet.Rows[i][1].ToString());
                                            if (tuitionPackageSheet.Rows[i][2].ToString() == "Giảm theo số tiền")
                                            {
                                                query = query.Replace("[DiscountType]", "1");
                                                query = query.Replace("[DiscountType]", tuitionPackageSheet.Rows[i][2].ToString());
                                            }
                                            else
                                            {
                                                query = query.Replace("[DiscountType]", "2");
                                                query = query.Replace("[DiscountType]", tuitionPackageSheet.Rows[i][2].ToString());
                                            }
                                            query = query.Replace("[Discount]", tuitionPackageSheet.Rows[i][3].ToString());
                                            query = query.Replace("[MaxDiscount]", tuitionPackageSheet.Rows[i][4].ToString());
                                            query = query.Replace("[Description]", tuitionPackageSheet.Rows[i][5].ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Gói học phí", listQuery);
                                }
                                
                                var listTuitionPackage = await domainService.GetStudyTime();
                                listQuery = "";
                                query = "";
                                #endregion

                                /*#region cấu hình lương
                                var salaryConfigSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("cấu hình lương"));
                                if(salaryConfigSheet != null)
                                {
                                    var studyTimeQuery = $"Insert Into tbl_SalaryConfig( UserId, Value, Note, {defaultProp}) Values ( [UserId], [Value], [Note], {defaultValues}); ";
                                    for (int i = 2; i < salaryConfigSheet.Rows.Count; i++)
                                    {
                                        query = studyTimeQuery;
                                        var staff = listStaff.FirstOrDefault(x => x.Code.ToLower() == salaryConfigSheet.Rows[i][1].ToString().ToLower());
                                        if (staff == null)
                                            throw new Exception($"Danh sách cấu hình lương không tìm thấy thông tin nhân viên có mã {salaryConfigSheet.Rows[i][0].ToString()}");
                                        query = query.Replace("[UserId]", staff.Id.ToString());
                                        query = query.Replace("[Value]", salaryConfigSheet.Rows[i][1].ToString());
                                        query = query.Replace("[Note]", salaryConfigSheet.Rows[i][2].ToString());
                                        listQuery = listQuery + query;
                                    }
                                }
                                await domainService.ExecuteQuery("Cấu hình lương", listQuery);
                                listQuery = "";
                                query = "";
                                #endregion  */                              

                                /*#region phụ huynh
                                var parentSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("phụ huynh"));
                                if(parentSheet != null)
                                {
                                    var studentQuery = $"Insert Into tbl_UserInformation( UserCode, FullName, UserName, Password, RoleId, RoleName, Gender, Email, Mobile, BranchIds, StatusId, ActiveDate, JobId, {defaultProp}) Values ( N'[UserCode]', N'[FullName]', N'[UserName]', '[Password]', [RoleId], N'[RoleName]', [Gender], N'[Email]', N'[Mobile]', '[BranchIds]', 0, {datenow}, [JobId], {defaultValues}); ";
                                    for (int i = 2; i < parentSheet.Rows.Count; i++)
                                    {
                                        query = studentQuery;
                                        var canImport = await domainService.ValidUser(parentSheet.Rows[i][3].ToString(), parentSheet.Rows[i][2].ToString().Trim().Replace("'", ""));
                                        if (canImport)
                                        {
                                            query = query.Replace("[RoleId]", ((int)RoleEnum.parents).ToString());
                                            query = query.Replace("[RoleName]", "Phụ huynh");

                                            string baseCode = "PH";
                                            int count = await db.tbl_UserInformation.CountAsync(x => ((x.RoleId == (int)RoleEnum.student && baseCode != "NV")
                                                        || (x.RoleId != ((int)RoleEnum.admin) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && baseCode == "NV"))
                                                        && x.CreatedOn.Value.Year == DateTime.Now.Year
                                                        && x.CreatedOn.Value.Month == DateTime.Now.Month
                                                        && x.CreatedOn.Value.Day == DateTime.Now.Day);

                                            query = query.Replace("[UserCode]", AssetCRM.InitCode(baseCode, DateTime.Now, count + 1));
                                            query = query.Replace("[FullName]", parentSheet.Rows[i][0].ToString());

                                            if (parentSheet.Rows[i][1].ToString() == "Nữ")
                                            {
                                                query = query.Replace("[Gender]", "2");

                                            }
                                            else if (parentSheet.Rows[i][1].ToString() == "Nam")
                                            {
                                                query = query.Replace("[Gender]", "1");
                                            }
                                            else
                                            {
                                                query = query.Replace("[Gender]", "0");
                                            }

                                            query = query.Replace("[UserName]", parentSheet.Rows[i][3].ToString());
                                            query = query.Replace("[Password]", Encryptor.Encrypt(parentSheet.Rows[i][3].ToString()));
                                            query = query.Replace("[Email]", parentSheet.Rows[i][3].ToString());
                                            query = query.Replace("[Mobile]", parentSheet.Rows[i][2].ToString().Trim().Replace("'", ""));

                                            var job = listJob.FirstOrDefault(x => x.Name.ToLower() == parentSheet.Rows[i][4].ToString().ToLower());
                                            if (job == null)
                                                throw new Exception($"Danh sách phụ huynh không tìm thấy thông tin công việc: {parentSheet.Rows[i][4].ToString()}");
                                            query = query.Replace("[JobId]", job.Id.ToString());

                                            var branch = listBranch.FirstOrDefault(x => x.Code.ToLower() == parentSheet.Rows[i][4].ToString().ToLower());
                                            if (branch == null)
                                                throw new Exception($"Danh sách phụ huỳnh không tìm thấy thông tin phòng ban có mã {parentSheet.Rows[i][4].ToString()}");
                                            query = query.Replace("[BranchIds]", branch.Id.ToString());

                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Phụ huynh", listQuery);
                                }
                                
                                var listParent = await domainService.GetParent();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region học viên
                                var studentSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("học viên"));
                                if(studentSheet != null)
                                {
                                    var studentQuery = $"Insert Into tbl_UserInformation( UserCode, FullName, UserName, Password, RoleId, RoleName, Gender, Email, Mobile, BranchIds, StatusId, ActiveDate, LearningStatus, LearningStatusName, ParentId, {defaultProp}) Values ( N'[UserCode]', N'[FullName]', N'[UserName]', '[Password]', [RoleId], N'[RoleName]', [Gender], N'[Email]', N'[Mobile]', '[BranchIds]', 0, {datenow}, 5, N'Đang học', [ParentId], {defaultValues}); ";
                                    for (int i = 2; i < studentSheet.Rows.Count; i++)
                                    {
                                        query = studentQuery;
                                        var canImport = await domainService.ValidUser(studentSheet.Rows[i][3].ToString(), studentSheet.Rows[i][2].ToString().Trim().Replace("'", ""));
                                        if (canImport)
                                        {
                                            query = query.Replace("[RoleId]", ((int)RoleEnum.student).ToString());
                                            query = query.Replace("[RoleName]", "Học viên");

                                            string baseCode = "HV";
                                            int count = await db.tbl_UserInformation.CountAsync(x => ((x.RoleId == (int)RoleEnum.student && baseCode != "NV")
                                                        || (x.RoleId != ((int)RoleEnum.admin) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && baseCode == "NV"))
                                                        && x.CreatedOn.Value.Year == DateTime.Now.Year
                                                        && x.CreatedOn.Value.Month == DateTime.Now.Month
                                                        && x.CreatedOn.Value.Day == DateTime.Now.Day);

                                            query = query.Replace("[UserCode]", AssetCRM.InitCode(baseCode, DateTime.Now, count + 1));
                                            query = query.Replace("[FullName]", studentSheet.Rows[i][0].ToString());

                                            if (studentSheet.Rows[i][1].ToString() == "Nữ")
                                            {
                                                query = query.Replace("[Gender]", "2");

                                            }
                                            else if (studentSheet.Rows[i][1].ToString() == "Nam")
                                            {
                                                query = query.Replace("[Gender]", "1");
                                            }
                                            else
                                            {
                                                query = query.Replace("[Gender]", "0");
                                            }

                                            query = query.Replace("[UserName]", studentSheet.Rows[i][3].ToString());
                                            query = query.Replace("[Password]", Encryptor.Encrypt(studentSheet.Rows[i][3].ToString()));
                                            query = query.Replace("[Email]", studentSheet.Rows[i][3].ToString());
                                            query = query.Replace("[Mobile]", studentSheet.Rows[i][2].ToString().Trim().Replace("'", ""));

                                            var parent = listParent.FirstOrDefault(x => x.Mobile.ToLower() == studentSheet.Rows[i][4].ToString().ToLower());
                                            if (parent == null)
                                                throw new Exception($"Danh sách học viên không tìm thấy thông tin phụ huynh có số điện thoại {studentSheet.Rows[i][4].ToString()}");
                                            query = query.Replace("[ParentId]", parent.Id.ToString());

                                            listQuery = listQuery + query;
                                        }
                                    }
                                    await domainService.ExecuteQuery("Học viên", listQuery);
                                }
                                
                                var listStudent = await domainService.GetStudent();
                                listQuery = "";
                                query = "";
                                #endregion*/

                                #region lớp học
                                //var listStudent = await domainService.GetStudent();
                                #endregion

                                #region học viên trong lớp
                                //var listStudent = await domainService.GetStudent();
                                #endregion

                                #region khách hàng
                                var customerSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("Leads"));
                                if(customerSheet != null)
                                {
                                    var studentQuery = $"Insert Into tbl_Customer( Code, FullName, Mobile, Email, BranchId, CustomerStatusId, PurposeId, SourceId, LearningNeedId, JobId, SaleId, {defaultProp}) Values ( N'[Code]', N'[FullName]', N'[Email]', N'[Mobile]', [BranchId], [CustomerStatusId], [PurposeId], [SourceId], [LearningNeedId], [JobId], [SaleId], {defaultValues}); ";
                                    for (int i = 2; i < customerSheet.Rows.Count; i++)
                                    {
                                        query = studentQuery;
                                        var canImport = await domainService.ValidCustomer(customerSheet.Rows[i][2].ToString(), customerSheet.Rows[i][1].ToString().Trim().Replace(" ", ""));
                                        if (canImport)
                                        {
                                            string baseCode = "KH";
                                            int count = await db.tbl_Customer.CountAsync(x =>
                                                        x.CreatedOn.Value.Year == DateTime.Now.Year
                                                        && x.CreatedOn.Value.Month == DateTime.Now.Month
                                                        && x.CreatedOn.Value.Day == DateTime.Now.Day);
                                            query = query.Replace("[Code]", AssetCRM.InitCode(baseCode, DateTime.Now, count + 1));
                                            query = query.Replace("[FullName]", customerSheet.Rows[i][0].ToString());
                                            query = query.Replace("[Mobile]", customerSheet.Rows[i][1].ToString().Replace(" ", ""));
                                            query = query.Replace("[Email]", customerSheet.Rows[i][2].ToString());

                                            var branch = listBranch.FirstOrDefault(x => x.Code.Trim().ToLower() == customerSheet.Rows[i][3].ToString().Trim().ToLower());
                                            if (branch == null)
                                                throw new Exception($"Danh sách khách hàng không tìm thấy thông tin trung tâm có mã {customerSheet.Rows[i][3].ToString()}");
                                            query = query.Replace("[BranchId]", branch.Id.ToString());

                                            var customerStatus = listCustomerStatus.FirstOrDefault(x => x.Name.Trim().ToLower() == customerSheet.Rows[i][4].ToString().Trim().ToLower());
                                            if (customerStatus == null)
                                                throw new Exception($"Danh sách khách hàng không tìm thấy thông tin trạng thái khách hàng: {customerSheet.Rows[i][4].ToString()}");
                                            query = query.Replace("[CustomerStatusId]", customerStatus.Id.ToString());

                                            var purpose = listPurpose.FirstOrDefault(x => x.Name.Trim().ToLower() == customerSheet.Rows[i][5].ToString().Trim().ToLower());
                                            if (purpose == null)
                                                throw new Exception($"Danh sách khách hàng không tìm thấy thông tin mục đích học {customerSheet.Rows[i][5].ToString()}");
                                            query = query.Replace("[PurposeId]", purpose.Id.ToString());

                                            var source = listSource.FirstOrDefault(x => x.Name.Trim().ToLower() == customerSheet.Rows[i][6].ToString().Trim().ToLower());
                                            if (source == null)
                                                throw new Exception($"Danh sách khách hàng không tìm thấy thông tin nguồn khách hàng: {customerSheet.Rows[i][6].ToString()}");
                                            query = query.Replace("[SourceId]", source.Id.ToString());

                                            var learningNeed = listLearningNeed.FirstOrDefault(x => x.Name.Trim().ToLower() == customerSheet.Rows[i][7].ToString().Trim().ToLower());
                                            if (learningNeed == null)
                                                throw new Exception($"Danh sách khách hàng không tìm thấy thông tin nhu cầu học: {customerSheet.Rows[i][7].ToString()}");
                                            query = query.Replace("[LearningNeedId]", learningNeed.Id.ToString());

                                            var job = listJob.FirstOrDefault(x => x.Name.Trim().ToLower() == customerSheet.Rows[i][8].ToString().Trim().ToLower());
                                            if (job == null)
                                                throw new Exception($"Danh sách khách hàng không tìm thấy thông tin công việc: {customerSheet.Rows[i][8].ToString()}");
                                            query = query.Replace("[JobId]", job.Id.ToString());

                                            //xử lý trường hợp tư vấn viên
                                            query = query.Replace("[SaleId]", "0");

                                            listQuery = listQuery + query;
                                        }
                                    }
                                }
                                await domainService.ExecuteQuery("Khách hàng", listQuery);
                                var listCustomer = await domainService.GetCustomer();
                                listQuery = "";
                                query = "";
                                #endregion

                                #region hẹn test
                                //var listStudent = await domainService.GetStudent();
                                #endregion

                                #region mã khuyến mãi
                                var discountSheet = dtSheet.Cast<DataTable>().FirstOrDefault(table => table.TableName.Trim().ToLower().Contains("makhuyenmai"));
                                if (discountSheet != null)
                                {
                                    var discountQuery = $"Insert Into tbl_Discount( Code, Type, TypeName, PackageType, PackageTypeName, Value, Status, StatusName, Note, Expired, Quantity, UsedQuantity, MaxDiscount, BranchIds, {defaultProp}) Values ( N'[Code]', [Type], N'[TypeName]', [PackageType], N'[PackageTypeName]', [Value], [Status], N'[StatusName]', N'[Note]', '[Expired]', '[Quantity]', 0, [MaxDiscount], '[BranchIds]', {defaultValues}); ";
                                    for (int i = 2; i < discountSheet.Rows.Count; i++)
                                    {
                                        query = discountQuery;
                                        var canImport = await domainService.ValidDiscount(discountSheet.Rows[i][0].ToString());
                                        if (canImport)
                                        {
                                            query = query.Replace("[Code]", discountSheet.Rows[i][0].ToString());
                                            if (discountSheet.Rows[i][1].ToString() == "Giảm tiền")
                                            {
                                                query = query.Replace("[Type]", "1");
                                                query = query.Replace("[TypeName]", discountSheet.Rows[i][1].ToString());
                                            }
                                            else
                                            {
                                                query = query.Replace("[Type]", "2");
                                                query = query.Replace("[TypeName]", discountSheet.Rows[i][1].ToString());
                                            }
                                            if (discountSheet.Rows[i][2].ToString() == "Gói lẻ")
                                            {
                                                query = query.Replace("[PackageType]", "1");
                                                query = query.Replace("[PackageTypeName]", discountSheet.Rows[i][2].ToString());
                                            }
                                            else
                                            {
                                                query = query.Replace("[PackageType]", "2");
                                                query = query.Replace("[PackageTypeName]", discountSheet.Rows[i][2].ToString());
                                            }
                                            query = query.Replace("[Value]", discountSheet.Rows[i][3].ToString());
                                            if (discountSheet.Rows[i][4].ToString() == "Đang diễn ra")
                                            {
                                                query = query.Replace("[Status]", "1");
                                                query = query.Replace("[StatusName]", discountSheet.Rows[i][4].ToString());
                                            }
                                            else
                                            {
                                                query = query.Replace("[Status]", "2");
                                                query = query.Replace("[StatusName]", discountSheet.Rows[i][4].ToString());
                                            }
                                            query = query.Replace("[Note]", discountSheet.Rows[i][5].ToString());
                                            query = query.Replace("[Expired]", discountSheet.Rows[i][6].ToString());
                                            query = query.Replace("[Quantity]", discountSheet.Rows[i][7].ToString());
                                            query = query.Replace("[MaxDiscount]", discountSheet.Rows[i][8].ToString());
                                            var branch = listBranch.FirstOrDefault(x => x.Code.ToLower() == discountSheet.Rows[i][9].ToString().ToLower());
                                            if (branch == null)
                                                throw new Exception($"Danh sách mã khuyến mãi không tìm thấy thông tin phòng ban có mã {discountSheet.Rows[i][9].ToString()}");
                                            query = query.Replace("[BranchIds]", branch.Id.ToString());
                                            listQuery = listQuery + query;
                                        }
                                    }
                                }
                                await domainService.ExecuteQuery("Mã khuyến mãi", listQuery);
                                listQuery = "";
                                query = "";
                                #endregion
                            }
                            else
                                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không có dữ liệu." });
                        }
                        else
                            return StatusCode((int)HttpStatusCode.BadRequest, new { message = "File lỗi." });
                        return StatusCode((int)HttpStatusCode.OK, new { message = "Thêm thành công" });
                    }
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không tìm thấy file" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("import-data-v2")]
        public async Task<IActionResult> ImportDataV2()
        {
            try
            {
                var httpRequest = HttpContext.Request;
                DataSet dsexcelRecords = new DataSet();
                IExcelDataReader reader = null;
                IFormFile inputFile = null;
                Stream fileStream = null;
                string datenow = DateTime.Now.ToString("yyyy-MM-dd");
                var defaultProp = $" Enable, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy";
                var defaultValues = $" 1, {datenow}, Hệ thống, {datenow}, Hệ thống";
                var a = new List<string>();
                var result = "";
                using (var db = new lmsDbContext())
                {
                    if (httpRequest.Form.Files.Count > 0)
                    {
                        inputFile = httpRequest.Form.Files["File"];
                        fileStream = inputFile.OpenReadStream();
                        if (inputFile != null && fileStream != null)
                        {
                            if (inputFile.FileName.EndsWith(".xls"))
                                reader = ExcelReaderFactory.CreateBinaryReader(fileStream);
                            else if (inputFile.FileName.EndsWith(".xlsx"))
                                reader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                            else
                                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không đúng định dạng." });

                            dsexcelRecords = reader.AsDataSet();
                            reader.Close();

                            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                            {
                                var tableInit = await db.tbl_SaveTableInit.Where(x => x.Enable == true).ToListAsync();
                                var tableInitDetail = await db.tbl_SaveTableInitDetail.Where(x => x.Enable == true).ToListAsync();

                                var dtSheet = dsexcelRecords.Tables;
                                var totalSheet = dsexcelRecords.Tables.Count;
                                for (int index = 1; index <= totalSheet; index++)
                                {
                                    result = "";
                                    var tempTableInit = tableInit.FirstOrDefault(x => x.Index == index);
                                    var temptableInitDetail = tableInitDetail.Where(x => x.SaveTableInitId == tempTableInit.Id).ToList();
                                    // Chuyển đổi dữ liệu sheet thành câu lệnh INSERT INTO
                                    var insertStatements = new List<string>();

                                    for (int i = 2; i < dtSheet[index - 1].Rows.Count; i++)
                                    {
                                        
                                        //var insertStatement = $"INSERT INTO {importConfig.TableName} (";
                                        var insertProp = $"INSERT INTO {tempTableInit.TableName} (";
                                        var insertValue = ") VALUES (";
                                        Type tableType = Type.GetType($"{tempTableInit.NameSpace}{tempTableInit.TableName}");
                                        // Lấy các thuộc tính và kiểu dữ liệu của lớp mà không có [NotMapped]
                                        /*var mappedProperties = tableType.GetProperties()
                                            .Where(prop => prop.GetCustomAttribute<NotMappedAttribute>() == null)
                                            .Select(prop => new
                                            {
                                                Name = prop.Name,
                                                DataType = GetFriendlyTypeName(prop.PropertyType)
                                            })
                                            .ToList();*/

                                        var indexProp = 0;
                                        for (int j = 0; j < temptableInitDetail.Count; j++)
                                        {
                                            insertProp = insertProp + temptableInitDetail[j].FieldName + ", ";
                                            var value = "";
                                            //xử lý data nếu đó là khóa ngoại
                                            if (temptableInitDetail[j].IsForeignKey == true)
                                            {
                                                var query = $"Select Id from {temptableInitDetail[j].ForeignTable} where {temptableInitDetail[j].ForeignFieldName} = N'{dtSheet[index - 1].Rows[i][indexProp].ToString()}'";
                                                var id = await domainService.GetIdByQuery(query);
                                                value = id.ToString();
                                            }
                                            //xử lý data nếu đó không phải là khóa ngoại
                                            else
                                            {
                                                value = $"{dtSheet[index - 1].Rows[i][indexProp].ToString()}";
                                            }
                                            
                                            if (!temptableInitDetail[j].FieldType.ToLower().Contains("int") && !temptableInitDetail[j].FieldType.ToLower().Contains("bool"))
                                                value = $"N'{value}'";
                                            insertValue = insertValue + value + ", ";
                                            indexProp++;
                                        }

                                        //insertStatement = $"INSERT INTO {importConfig.TableName} (FullName, UserName, Email, Mobile, Password) VALUES ('{fullName}', '{userName}', '{email}', '{mobile}', '{password}');";
                                        insertProp = insertProp + defaultProp + insertValue + defaultValues + ");";
                                        result = result + insertProp + " ";
                                        //insertStatements.Add(insertStatement);
                                        /*}*/                                     
                                    }
                                    a.Add(result);

                                    /*// Thực thi các câu lệnh INSERT INTO cho sheet hiện tại
                                    foreach (var statement in insertStatements)
                                    {
                                        // Sử dụng Dapper để thực thi câu lệnh SQL
                                        db.Database.ExecuteSqlCommand(statement);
                                    }*/

                                }
                            }
                            else
                                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không có dữ liệu." });
                        }
                        else
                            return StatusCode((int)HttpStatusCode.BadRequest, new { message = "File lỗi." });
                        return StatusCode((int)HttpStatusCode.OK, new { message = "Thêm thành công", data = a });
                    }
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không tìm thấy file" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("api/Test/GetPropertyOfTable")]
        public async Task<IActionResult> GetPropertyOfTable()
        {
            // Tên của lớp mà bạn muốn lấy thuộc tính
            var tableName = "LMS_Project.Models.tbl_Cart"; // Thay thế "Namespace" bằng namespace thực tế của lớp tbl_Cart

            // Lấy đối tượng Type từ tên lớp
            Type tableType = Type.GetType(tableName);

            if (tableType != null)
            {
                // Lấy các thuộc tính của lớp mà không có [NotMapped]
                var mappedProperties = tableType.GetProperties()
                    .Where(prop => prop.GetCustomAttribute<NotMappedAttribute>() == null)
                    .Select(prop => new
                    {
                        Name = prop.Name,
                        DataType = GetFriendlyTypeName(prop.PropertyType)
                    })
                    .ToList();

                var result = "";
                // In ra tên của các thuộc tính
                foreach (var prop in mappedProperties)
                {
                    result = result + " || " + prop.Name + " - " + prop.DataType;
                }
                return StatusCode((int)HttpStatusCode.OK, new { data = result, message = "Thành công" });
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = $"Không tìm thấy lớp với tên '{tableName}'" });
            }
        }
        // Hàm để lấy tên kiểu dữ liệu thân thiện hơn
        private static string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Nếu là kiểu nullable, lấy kiểu cơ bản (underlying type)
                return $"{Nullable.GetUnderlyingType(type).Name}?";
            }
            else
            {
                // Trả về tên kiểu dữ liệu thông thường
                return type.Name;
            }
        }
    }
}
