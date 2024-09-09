using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.Class.ClassService;
using LMSCore.Migrations;


namespace LMSCore.Services
{
    public class ClassRegistrationService
    {
        public static async Task<tbl_ClassRegistration> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_ClassRegistration.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public class AddToClassModel
        {
            public List<int> ClassRegistrationIds { get; set; }
            public int classId { get; set; }
        }
        public static async Task AddToClass(AddToClassModel model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == model.classId);
                        if (_class == null)
                            throw new Exception("Không tìm thấy lớp");
                        if (_class.Status == 3)
                            throw new Exception("Lớp học đã kết thúc");
                        var countStudentInClass = await db.tbl_StudentInClass.CountAsync(x => x.ClassId == _class.Id && x.Enable == true);
                        if (_class.MaxQuantity <= (countStudentInClass + model.ClassRegistrationIds.Count()))
                            throw new Exception($"Lớp đã có {countStudentInClass}/{_class.MaxQuantity} học viên");

                        if (!model.ClassRegistrationIds.Any())
                            throw new Exception("Không tìm thấy dữ liệu");

                        foreach (var item in model.ClassRegistrationIds)
                        {
                            var classRegistration = await db.tbl_ClassRegistration.SingleOrDefaultAsync(x => x.Id == item);
                            if (classRegistration == null)
                                throw new Exception("Không tìm thấy học viên chờ xếp lớp");
                            var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == classRegistration.StudentId);
                            if (student == null)
                                throw new Exception("Không tìm thấy học viên");

                            if (classRegistration.Status == 2)
                                throw new Exception($"Học viên {student.FullName} đã xếp lớp");
                            if (classRegistration.Status == 3)
                                throw new Exception($"Học viên {student.FullName} đã hoàn tiền");

                            var checkExist = await db.tbl_StudentInClass.AnyAsync(x => x.StudentId == classRegistration.StudentId && x.ClassId == model.classId && x.Enable == true);
                            if (checkExist)
                                throw new Exception($"Học viên {student.FullName} đã có trong lớp {_class.Name}");

                            var newStudentInClass = new tbl_StudentInClass//thêm vào lớp mới
                            {
                                BranchId = classRegistration.BranchId,
                                ClassId = _class.Id,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                Note = "Học viên chờ xếp lớp chuyển vào lớp",
                                StudentId = classRegistration.StudentId,
                                Enable = true,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                Type = 1,
                                TypeName = "Chính thức",
                                Warning = false

                            };
                            db.tbl_StudentInClass.Add(newStudentInClass);
                            student.LearningStatus = 5;
                            student.LearningStatusName = "Đang học";

                            classRegistration.Status = 2;
                            classRegistration.StatusName = "Đã xếp lớp";

                            // thêm lịch sử học viên  
                            var learningHistoryService = new LearningHistoryService(db);
                            await learningHistoryService.Insert(new LearningHistoryCreate
                            {
                                StudentId = student.UserInformationId,
                                Content = $"Từ danh sách hẹn đăng ký, chuyển vào lớp {_class.Name}"
                            }, user);
                            //
                            await db.SaveChangesAsync();
                        }
                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
        }

        public static async Task<tbl_ClassRegistration> Update(ClassRegistrationUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_ClassRegistration.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Note = itemModel.Note ?? entity.Note;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task<AppDomainResult> GetAll(ClassRegistrationSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ClassRegistrationSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                int mySaleId = 0;
                if (user.RoleId == ((int)RoleEnum.sale))
                    mySaleId = user.UserInformationId;
                string sql = $"Get_ClassRegistration @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranhIds = N'{baseSearch.BranchIds}'," +
                    $"@ProgramIds = N'{baseSearch.ProgramIds}'," +
                    $"@Status = N'{baseSearch.Status}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@SaleId = N'{mySaleId}'";
                var data = await db.SqlQuery<Get_ClassRegistration>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_ClassRegistration
                {
                    Id = i.Id,
                    Avatar = i.Avatar,
                    AvatarReSize = i.AvatarReSize,
                    BranchId = i.BranchId,
                    BranchName = i.BranchName,
                    CreatedBy = i.CreatedBy,
                    CreatedOn = i.CreatedOn,
                    Enable = i.Enable,
                    FullName = i.FullName,
                    ModifiedBy = i.ModifiedBy,
                    ModifiedOn = i.ModifiedOn,
                    Note = i.Note,
                    Price = i.Price,
                    ProgramId = i.ProgramId,
                    ProgramName = i.ProgramName,
                    Status = i.Status,
                    StatusName = i.StatusName,
                    StudentId = i.StudentId,
                    UserCode = i.UserCode,
                    SaleId = i.SaleId,
                    SalerName = i.SalerName,
                    SalerUserCode = i.SalerUserCode,
                    Expectations = Task.Run(() => GetExpectation(db, i.Id)).Result
                }).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<List<ExpectationModel>> GetExpectation(lmsDbContext db, int classRegistrationId)
        {
            var result = new List<ExpectationModel>();
            var studentExpectations = await db.tbl_StudentExpectation.Where(x => x.ClassRegistrationId == classRegistrationId && x.Enable == true)
                .Select(x => new
                {
                    x.ExectedDay,
                    x.StudyTimeId
                }).ToListAsync();
            if (!studentExpectations.Any())
                return result;
            foreach (var item in studentExpectations)
            {
                if (item.ExectedDay.HasValue)
                {
                    var expectation = new ExpectationModel();
                    expectation.ExectedDay = item.ExectedDay.Value;
                    var studyTime = await db.tbl_StudyTime.SingleOrDefaultAsync(x => x.Id == item.StudyTimeId);
                    if (studyTime != null)
                    {
                        expectation.StudyTimeId = studyTime.Id;
                        expectation.StudyTimeName = studyTime.Name;
                    }
                    result.Add(expectation);
                }
            }
            return result;
        }
        public class ClassAvailableModel
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            /// <summary>
            /// true - Lớp phù hợp
            /// </summary>
            public bool Fit { get; set; }
            public List<string> Notes { get; set; }
            public ClassAvailableModel()
            {
                Notes = new List<string>();
            }
        }
        public class ClassAvailableSearch
        {
            /// <summary>
            /// Chương trình
            /// </summary>
            public int ProgramId { get; set; }
            /// <summary>
            /// Chi nhánh
            /// </summary>
            public int BranchId { get; set; }
            /// <summary>
            /// Danh sách học viên 1,2,3
            /// </summary>
            public string StudentIds { get; set; }
        }
        public static async Task<List<ClassAvailableModel>> GetClassAvailable(ClassAvailableSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ClassAvailableSearch();
                if (string.IsNullOrEmpty(baseSearch.StudentIds))
                    throw new Exception("Vui lòng chọn học viên");
                var result = new List<ClassAvailableModel>();
                var studentIds = baseSearch.StudentIds.Split(',');
                if (!studentIds.Any())
                    throw new Exception("Vui lòng chọn học viên");
                var classes = await db.tbl_Class
                    .Where(x => x.BranchId == baseSearch.BranchId && x.ProgramId == baseSearch.ProgramId && x.Enable == true && x.Status != 3)
                    .Select(x => new { x.Id, x.Name, x.MaxQuantity }).ToListAsync();
                if (!classes.Any())
                    return result;

                foreach (var item in classes)
                {
                    var classAvailable = new ClassAvailableModel
                    {
                        ClassId = item.Id,
                        ClassName = item.Name,
                        Fit = true
                    };

                    var totalStudent = await db.tbl_StudentInClass.CountAsync(x => x.ClassId == item.Id && x.Enable == true);
                    if ((totalStudent + studentIds.Count()) > item.MaxQuantity)
                    {
                        classAvailable.Fit = false;
                        classAvailable.Notes.Add($"Lớp đã có {totalStudent}/{item.MaxQuantity} học viên");
                        continue;
                    }
                    foreach (var studentId in studentIds)
                    {
                        var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId.ToString() == studentId.Trim());
                        if (student != null)
                        {
                            var hasInClass = await db.tbl_StudentInClass.AnyAsync(x => x.ClassId == item.Id && x.StudentId.ToString() == studentId.Trim() && x.Enable == true);
                            if (hasInClass)
                            {
                                classAvailable.Fit = false;
                                classAvailable.Notes.Add($"Học viên {student.FullName} đang học lớp này");
                            }
                        }
                    }
                    result.Add(classAvailable);
                }
                return result;
            }
        }
        public static async Task<int> GetClassRegistrationNumber(string branchIds, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                string sql = $"Get_ProgramRegistration " +
                    $"@BranhIds = N'{branchIds}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'";
                var data = await db.SqlQuery<Get_ProgramRegistration>(sql);
                if (data.Any())
                {
                    return data.Sum(x => x.Quantity);
                }
                else
                    return 0;
            }
        }
        public static async Task<List<Get_ProgramRegistration>> GetProgramRegistration(ProgramRegistrationSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ProgramRegistrationSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                string sql = $"Get_ProgramRegistration " +
                    $"@BranhIds = N'{baseSearch.BranchIds}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'";
                var data = await db.SqlQuery<Get_ProgramRegistration>(sql);
                return data;
            }
        }
        public static async Task<List<Get_ScheduleRegistration>> GetScheduleRegistration(ScheduleRegistrationSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ScheduleRegistrationSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                string sql = $"Get_ScheduleRegistration " +
                    $"@BranhIds = N'{baseSearch.BranchIds}'," +
                    $"@ProgramId = {baseSearch.ProgramId}," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'";
                var data = await db.SqlQuery<Get_ScheduleRegistration>(sql);
                if (data.Any())
                {
                    data = data.Where(x => x.Quantity > 0).ToList();
                    var studyTime = await db.tbl_StudyTime.Where(x => x.Enable == true).ToListAsync();
                    foreach (var d in data)
                    {
                        d.Studys = new List<Study>();
                        if (string.IsNullOrEmpty(d.ExectedDays) || string.IsNullOrEmpty(d.StudyTimeIds))
                            continue;
                        var exectedDays = d.ExectedDays.Split(",").ToList();
                        var studyTimeIds = d.StudyTimeIds.Split(",").ToList();
                        for (int i = 0; i < exectedDays.Count; i++)
                        {
                            var study = new Study();
                            if (Int32.TryParse(exectedDays[i], out int exectedDay))
                                study.ExectedDay = exectedDay;
                            if (study.ExectedDay == 0)
                                study.ExectedDayName = "Chủ nhật";
                            else
                                study.ExectedDayName = "Thứ " + (study.ExectedDay + 1);
                            if (Int32.TryParse(studyTimeIds[i], out int studyTimeId))
                                study.StudyTimeId = studyTimeId;
                            study.StudyTimeName = studyTime.FirstOrDefault(x => x.Id == study.StudyTimeId)?.Name;
                            d.Studys.Add(study);
                        }
                    }
                }
                return data;
            }
        }
        public static async Task<AppDomainResult> GetStudentRegistration(StudentRegistrationSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StudentRegistrationSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                var listRejectES = new List<string>();
                if (!string.IsNullOrEmpty(baseSearch.RejectExectedDays) && !string.IsNullOrEmpty(baseSearch.RejectStudyTimeIds))
                {
                    var studyTimeIds = baseSearch.RejectStudyTimeIds.Split(',').ToList();
                    var exectedDays = baseSearch.RejectExectedDays.Split(',').ToList();
                    for (int i = 0; i < exectedDays.Count; i++)
                    {
                        listRejectES.Add(exectedDays[i] + "-" + studyTimeIds[i]);
                    }
                }
                var rejectES = string.Join(",", listRejectES);
                string sql = $"Get_StudentRegistration @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranhIds = N'{baseSearch.BranchIds}'," +
                    $"@ProgramId = N'{baseSearch.ProgramId}'," +
                    $"@ExectedDays = '{baseSearch.ExectedDays}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@IsUndetermined = '{baseSearch.IsUndetermined}'," +
                    $"@RejectUndetermined = '{baseSearch.RejectUndetermined}'," +
                    $"@RejectES = '{rejectES}'," +
                    $"@StudyTimeIds = '{baseSearch.StudyTimeIds}'";
                var data = await db.SqlQuery<Get_StudentRegistration>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new StudentRegistration(i)).ToList();

                if (result.Any())
                {
                    var studyTime = await db.tbl_StudyTime.Where(x => x.Enable == true).ToListAsync();
                    foreach (var d in result)
                    {
                        d.Studys = new List<Study>();
                        if (string.IsNullOrEmpty(d.ExectedDays) || string.IsNullOrEmpty(d.StudyTimeIds))
                            continue;
                        var exectedDays = d.ExectedDays.Split(",").ToList();
                        var studyTimeIds = d.StudyTimeIds.Split(",").ToList();
                        for (int i = 0; i < exectedDays.Count; i++)
                        {
                            var study = new Study();
                            if (Int32.TryParse(exectedDays[i], out int exectedDay))
                                study.ExectedDay = exectedDay;

                            if (study.ExectedDay == 0)
                                study.ExectedDayName = "Chủ nhật";
                            else
                                study.ExectedDayName = "Thứ " + (study.ExectedDay + 1);

                            if (Int32.TryParse(studyTimeIds[i], out int studyTimeId))
                                study.StudyTimeId = studyTimeId;

                            study.StudyTimeName = studyTime.FirstOrDefault(x => x.Id == study.StudyTimeId)?.Name;
                            d.Studys.Add(study);
                        }
                    }
                }
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task SwapExpectations(SwapExpectationCreate itemModel, tbl_UserInformation user, lmsDbContext db)
        {
            // Xóa ca học cũ
            var classRegistrationIds = itemModel.ClassRegistrationIds.Select(x => "," + x + ",").ToList();
            var clearStudentExpectations = await db.tbl_StudentExpectation.Where(x => x.Enable == true
            && classRegistrationIds.Contains("," + x.ClassRegistrationId + ",")).ToListAsync();
            if (clearStudentExpectations.Any())
            {
                clearStudentExpectations.ForEach(x => x.Enable = false);
            }
            //Thông tin chờ xếp lớp
            var classRegistration = await db.tbl_ClassRegistration.Where(x => x.Enable == true
            && classRegistrationIds.Contains("," + x.Id + ",")).ToListAsync();
            if (!classRegistration.Any())
                throw new Exception("Không tìm thấy thông tin");

            foreach (var item in itemModel.ClassRegistrationIds)
            {
                var infoCR = classRegistration.FirstOrDefault(x => x.Id == item);
                if (infoCR == null)
                    throw new Exception("Không tìm thấy thông tin");

                var expectations = new List<StudentExpectationCreate>();
                expectations = itemModel.Expectations;
                if (expectations != null)
                    foreach (var exp in expectations)
                    {
                        var expectation = new tbl_StudentExpectation(exp);
                        expectation.ClassRegistrationId = item;
                        expectation.StudentId = infoCR?.StudentId;
                        expectation.CreatedBy = expectation.ModifiedBy = user.FullName;
                        db.tbl_StudentExpectation.Add(expectation);
                    }
            }
            await db.SaveChangesAsync();
        }
    }
}