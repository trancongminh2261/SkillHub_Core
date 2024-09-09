using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;


using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using LMSCore.Services.WarningHistory;
using Hangfire;
using LMSCore.Services.Class;
using static LMSCore.Services.Class.ClassNotificationRequest;
using LMSCore.Services.PaymentApprove;

namespace LMSCore.Services.StudentInClass
{
    public class StudentInClassService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public StudentInClassService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public class StudentInClassBySaleModel
        {
            public int SaleId { get; set; }
            public int StudentId { get; set; }
            public string StudentName { get; set; }
            public string StudentCode { get; set; }
            public string StudentAvatar { get; set; }
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            public DateTime? CreatedOn { get; set; }
        }
        //hàm này dùng trong báo cáo hoa hồng của tư vấn viên
        public static async Task<AppDomainResult> GetBySale(StudentInClassBySaleSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StudentInClassBySaleSearch();
                var studentInClass = await db.tbl_StudentInClass.Where(x => x.Enable == true
                                    && (baseSearch.Month == 0 || baseSearch.Month != 0 && x.CreatedOn.Value.Month == baseSearch.Month)
                                    && (baseSearch.Year == 0 || baseSearch.Year != 0 && x.CreatedOn.Value.Year == baseSearch.Year)).ToListAsync();
                if (baseSearch.SaleId != null)
                    studentInClass = studentInClass.Where(x => baseSearch.SaleId == 0 || baseSearch.SaleId != 0
                    && GetSaleByStudent(x.StudentId, baseSearch.SaleId, db)).ToList();
                var result = (from x in studentInClass
                              join u in db.tbl_UserInformation on x.StudentId equals u.UserInformationId into userGroup
                              from user in userGroup.DefaultIfEmpty()
                              join c in db.tbl_Class on x.ClassId equals c.Id into classGroup
                              from _class in classGroup.DefaultIfEmpty()
                              select new StudentInClassBySaleModel
                              {
                                  SaleId = baseSearch.SaleId ?? 0,
                                  StudentId = x.StudentId ?? 0,
                                  StudentName = user.FullName,
                                  StudentCode = user.UserCode,
                                  StudentAvatar = user.Avatar,
                                  ClassId = x.ClassId ?? 0,
                                  ClassName = _class.Name,
                                  CreatedOn = x.CreatedOn
                              }).ToList();

                if (result.Count <= 0) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = result.Count;
                // Phân trang
                int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
                result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static bool GetSaleByStudent(int? studentId, int? saleId, lmsDbContext db)
        {
            return db.tbl_UserInformation.Any(y => y.UserInformationId == studentId && y.SaleId == saleId);
        }
        public static async Task<tbl_StudentInClass> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_StudentInClass.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }

        /// <summary>
        /// Thêm nhiều học viên
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<bool> AppendStudent(StudentInClassAppend itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.ClassId);

                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");
                if (_class.Type == 3)
                    throw new Exception("Không thể thêm học viên vào lớp dạy kèm");
                if (itemModel.StudentIds == null || itemModel.StudentIds.Count == 0)
                    throw new Exception("Vui lòng chọn học viên");
                var studentIds = itemModel.StudentIds;
                var students = await db.tbl_UserInformation.Where(x => studentIds.Contains(x.UserInformationId) && x.RoleId == 3).ToListAsync();
                if (students == null || students.Count == 0)
                    throw new Exception("Không tìm thấy học viên");

                var countStudent = await db.tbl_StudentInClass.CountAsync(x => x.ClassId == _class.Id && x.Enable == true);
                // Lấy thông tin giáo viên
                var teacherInClass = await db.tbl_Schedule.Where(x => x.ClassId == _class.Id && x.Enable == true)
                                    .Select(x => x.TeacherId).Distinct().ToListAsync();
                var teachers = await db.tbl_UserInformation.Where(x => x.RoleId == 2 && x.Enable == true && teacherInClass.Contains(x.UserInformationId))
                    .Select(x => new TeacherAvailable
                    {
                        Id = x.UserInformationId,
                        TeacherCode = x.UserCode,
                        TeacherName = x.FullName
                    }).ToListAsync();

                if (countStudent + itemModel.StudentIds.Count() > _class.MaxQuantity)
                    throw new Exception($"Lớp đã có {countStudent}/{_class.MaxQuantity} học viên");
                var models = new List<tbl_StudentInClass>();
                var studentInClass = await db.tbl_StudentInClass.Where(x => x.Enable == true && x.ClassId == itemModel.ClassId).ToListAsync();
                var notifyAddedToTheClassItem = new List<NotifyAddedToTheClassItem>();

                foreach (var student in students)
                {
                    //bỏ qua nếu học sinh đã có trong lớp
                    bool isExsisted = studentInClass.Any(x => x.StudentId == student.UserInformationId);
                    if (isExsisted)
                        continue;
                    var model = new tbl_StudentInClass
                    {
                        ClassId = itemModel.ClassId,
                        StudentId = student.UserInformationId,
                        Warning = itemModel.Warning,
                        Note = itemModel.Note,
                        Type = itemModel.Type,
                        TypeName = itemModel.TypeName,
                        BranchId = _class.BranchId,
                        CreatedBy = user.FullName,
                        ModifiedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now,
                        Enable = true
                    };
                    models.Add(model);
                    student.LearningStatusName = "Đang học";
                    student.LearningStatus = 5;

                    List<tbl_StudyRoute> studyRoutes = await db.tbl_StudyRoute
                   .Where(x => x.Enable == true && x.StudentId == student.UserInformationId && x.ProgramId == _class.ProgramId && x.Status == (int)StudyRouteStatus.ChuaHoc).ToListAsync();
                    if (studyRoutes.Any())
                    {
                        foreach (var item in studyRoutes)
                        {
                            item.Status = (int)StudyRouteStatus.DangHoc;
                            item.StatusName = ListStudyRouteStatus().Where(x => x.Key == item.Status).FirstOrDefault().Value;
                        }
                        await db.SaveChangesAsync();
                    }
                    // thêm lịch sử học viên  
                    var learningHistoryService = new LearningHistoryService(db);
                    await learningHistoryService.Insert(new LearningHistoryCreate
                    {
                        StudentId = student.UserInformationId,
                        Content = $"Thêm vào lớp {_class.Name}"
                    });
                    notifyAddedToTheClassItem.Add(new NotifyAddedToTheClassItem
                    {
                        ClassId = model.ClassId.Value,
                        StudentId = model.StudentId.Value
                    });
                }
                db.tbl_StudentInClass.AddRange(models);
                await db.SaveChangesAsync();

                BackgroundJob.Schedule(() => ClassNotification.NotifyStudentAddedToTheClass(new NotifyStudentAddedToTheClassRequest
                {
                    Items = notifyAddedToTheClassItem,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(2));

                BackgroundJob.Schedule(() => ClassNotification.NotifyParentsAddedToTheClass(new NotifyParentsAddedToTheClassRequest
                {
                    Items = notifyAddedToTheClassItem,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(4));

                return true;
            }
        }
        /// <summary>
        /// Thêm học thử
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_StudentInClass> Insert(StudentInClassCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.ClassId);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");
                if (_class.Type == 3)
                    throw new Exception("Không thể thêm học viên vào lớp dạy kèm");
                var countStudent = await db.tbl_StudentInClass.CountAsync(x => x.ClassId == _class.Id && x.Enable == true);
                if (countStudent >= _class.MaxQuantity)
                    throw new Exception("Lớp đã đủ học viên");
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId && x.RoleId == 3);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var model = new tbl_StudentInClass(itemModel);
                model.BranchId = _class.BranchId;
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_StudentInClass.Add(model);
                student.LearningStatus = 5;
                student.LearningStatusName = "Đang học";
                await db.SaveChangesAsync();
                List<tbl_StudyRoute> studyRoutes = await db.tbl_StudyRoute.Where(x => x.Enable == true && x.StudentId == student.UserInformationId && x.ProgramId == _class.ProgramId && x.Status == (int)StudyRouteStatus.ChuaHoc).ToListAsync();
                if (studyRoutes.Any())
                {
                    foreach (var item in studyRoutes)
                    {
                        item.Status = (int)StudyRouteStatus.DangHoc;
                        item.StatusName = ListStudyRouteStatus().Where(x => x.Key == item.Status).FirstOrDefault().Value;
                    }
                    await db.SaveChangesAsync();
                }
                // thêm lịch sử học viên  
                var learningHistoryService = new LearningHistoryService(db);
                await learningHistoryService.Insert(new LearningHistoryCreate
                {
                    StudentId = student.UserInformationId,
                    Content = $"Thêm vào lớp {_class.Name}"
                });

                var notifyAddedToTheClassItem = new List<NotifyAddedToTheClassItem>()
                {
                    new NotifyAddedToTheClassItem { ClassId = model.ClassId.Value, StudentId = model.StudentId.Value }
                };
                BackgroundJob.Schedule(() => ClassNotification.NotifyStudentAddedToTheClass(new NotifyStudentAddedToTheClassRequest
                {
                    Items = notifyAddedToTheClassItem,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(2));

                BackgroundJob.Schedule(() => ClassNotification.NotifyParentsAddedToTheClass(new NotifyParentsAddedToTheClassRequest
                {
                    Items = notifyAddedToTheClassItem,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(4));

                return model;
            }
        }
        public static async Task<tbl_StudentInClass> InsertForRegistration(StudentInClassCreate itemModel, tbl_UserInformation user, tbl_Class _class)
        {
            using (var db = new lmsDbContext())
            {
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");
                if (_class.Type == 3)
                    throw new Exception("Không thể thêm học viên vào lớp dạy kèm");
                var countStudent = await db.tbl_StudentInClass.CountAsync(x => x.ClassId == _class.Id && x.Enable == true);
                if (countStudent >= _class.MaxQuantity)
                    throw new Exception("Lớp đã đủ học viên");
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId && x.RoleId == 3);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var model = new tbl_StudentInClass(itemModel);
                model.BranchId = _class.BranchId;
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_StudentInClass.Add(model);
                student.LearningStatus = 5;
                student.LearningStatusName = "Đang học";
                await db.SaveChangesAsync();
                List<tbl_StudyRoute> studyRoutes = await db.tbl_StudyRoute.Where(x => x.Enable == true && x.StudentId == student.UserInformationId && x.ProgramId == _class.ProgramId && x.Status == (int)StudyRouteStatus.ChuaHoc).ToListAsync();
                if (studyRoutes.Any())
                {
                    foreach (var item in studyRoutes)
                    {
                        item.Status = (int)StudyRouteStatus.DangHoc;
                        item.StatusName = ListStudyRouteStatus().Where(x => x.Key == item.Status).FirstOrDefault().Value;
                    }
                    await db.SaveChangesAsync();
                }
                // thêm lịch sử học viên  
                var learningHistoryService = new LearningHistoryService(db);
                await learningHistoryService.Insert(new LearningHistoryCreate
                {
                    StudentId = student.UserInformationId,
                    Content = $"Thêm vào lớp {_class.Name}"
                });

                var notifyAddedToTheClassItem = new List<NotifyAddedToTheClassItem>()
                {
                    new NotifyAddedToTheClassItem { ClassId = model.ClassId.Value, StudentId = model.StudentId.Value }
                };
                BackgroundJob.Schedule(() => ClassNotification.NotifyStudentAddedToTheClass(new NotifyStudentAddedToTheClassRequest
                {
                    Items = notifyAddedToTheClassItem,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(2));

                BackgroundJob.Schedule(() => ClassNotification.NotifyParentsAddedToTheClass(new NotifyParentsAddedToTheClassRequest
                {
                    Items = notifyAddedToTheClassItem,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(4));

                return model;
            }
        }
        /// <summary>
        /// Ghi chú và cảnh báo
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_StudentInClass> Update(StudentInClassUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudentInClass.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                //lưu lại trước đó học viên này có bị cảnh báo hay không
                var oldWarning = entity.Warning;
                entity.Warning = itemModel.Warning ?? entity.Warning;
                entity.WarningContent = itemModel.WarningContent ?? entity.WarningContent;
                entity.Note = itemModel.Note ?? entity.Note;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                //học viên bị cảnh báo thì thông báo cho học vụ và tư vấn viên
                tbl_Class _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == entity.ClassId);
                //thêm lịch sử học viên  
                if (oldWarning != entity.Warning && entity.Warning == true)
                {

                    var learningHistoryService = new LearningHistoryService(db);
                    await learningHistoryService.Insert(new LearningHistoryCreate
                    {
                        StudentId = entity.StudentId,
                        ClassId = entity.ClassId,
                        Content = $"Bị cảnh báo trong lớp {_class.Name}, lý do: {entity.WarningContent}",
                    }, user);

                    //lưu thông tin này để thống kê
                    var warningHistoryService = new WarningHistoryService(db);
                    await warningHistoryService.Insert(new WarningHistoryCreate
                    {
                        StudentId = entity.StudentId ?? 0,
                        ClassId = entity.ClassId ?? 0,
                        Content = $"{entity.WarningContent}",
                        Type = 1
                    }, user);

                    BackgroundJob.Schedule(() => StudentInClassNotification.NotifySaleStudentIsOnWarning(new StudentInClassNotificationRequest.NotifySaleStudentIsOnWarningRequest
                    {
                        StudentInClassId = entity.Id,
                        CurrentUser = user
                    }), TimeSpan.FromSeconds(2));
                    BackgroundJob.Schedule(() => StudentInClassNotification.NotifyParentsStudentIsOnWarning(new StudentInClassNotificationRequest.NotifyParentsStudentIsOnWarningRequest
                    {
                        StudentInClassId = entity.Id,
                        CurrentUser = user
                    }), TimeSpan.FromSeconds(4));
                }
                //đối với trường hợp cảnh báo = false thì phải check trước đó oldWarning có bằng null hay không
                //vì có thể lúc khởi tạo oldWarning = null sau đó cập nhật laị = false hệ thống thông báo là được gỡ cảnh báo => sai logic
                if (oldWarning != null && oldWarning != entity.Warning && entity.Warning == false)
                {
                    var learningHistoryService = new LearningHistoryService(db);
                    await learningHistoryService.Insert(new LearningHistoryCreate
                    {
                        StudentId = entity.StudentId,
                        ClassId = entity.ClassId,
                        Content = $"Được gỡ cảnh báo trong lớp {_class.Name}",
                    }, user);

                    //lưu thông tin này để thống kê
                    var warningHistoryService = new WarningHistoryService(db);
                    await warningHistoryService.Insert(new WarningHistoryCreate
                    {
                        StudentId = entity.StudentId ?? 0,
                        ClassId = entity.ClassId ?? 0,
                        Content = $"",
                        Type = 2
                    }, user);
                }

                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudentInClass.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == entity.ClassId);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp học");
                if (_class.Type == 3)
                    throw new Exception("Không thể xóa học viên ở lớp dạy kèm");
                //Xóa
                entity.Enable = false;
                // thêm lịch sử học viên  
                var learningHistoryService = new LearningHistoryService(db);
                await learningHistoryService.Insert(new LearningHistoryCreate
                {
                    StudentId = entity.StudentId,
                    Content = $"Ngưng học tại lớp {_class.Name}"
                });
                //
                await db.SaveChangesAsync();
                List<tbl_StudyRoute> studyRoutes = await (from c in db.tbl_Class
                                                          join sr in db.tbl_StudyRoute on c.ProgramId equals sr.ProgramId into list
                                                          from l in list.DefaultIfEmpty()
                                                          where l.Enable == true && l.StudentId == entity.StudentId && l.Status == (int)StudyRouteStatus.DangHoc
                                                          select l).ToListAsync();
                if (studyRoutes.Any())
                {
                    foreach (var item in studyRoutes)
                    {
                        item.Status = (int)StudyRouteStatus.ChuaHoc;
                        item.StatusName = ListStudyRouteStatus().Where(x => x.Key == item.Status).FirstOrDefault().Value;
                    }
                    await db.SaveChangesAsync();
                }

                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.StudentId && x.RoleId == 3);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var classIds = await db.tbl_StudentInClass.Where(x => x.StudentId == student.UserInformationId && x.Enable == true && x.ClassId != entity.Id).Select(x => x.ClassId).ToListAsync();
                var checkClass = await db.tbl_Class.Where(x => classIds.Contains(x.Id) && x.Status != 3 && x.Enable == true).AnyAsync();
                if (!checkClass)
                {
                    student.LearningStatus = 6;
                    student.LearningStatusName = "Học xong";
                    await db.SaveChangesAsync();
                }
            }
        }
        public class NewClassModel
        {
            public int ClassId { get; set; }
            public bool Fit { get; set; }
            public string Note { get; set; }
        }
        public static async Task<AppDomainResult> GetAll(StudentInClassSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StudentInClassSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;
                int mySaleId = 0;
                if (user.RoleId == (int)RoleEnum.sale)
                    mySaleId = user.UserInformationId;
                if (user.RoleId == (int)RoleEnum.student)
                {
                    baseSearch.StudentIds = user.UserInformationId.ToString();
                    myBranchIds = "";
                }
                string warning = baseSearch.Warning.HasValue ? baseSearch.Warning.Value.ToString() : "null";
                string sql = $"Get_StudentInClass @Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = N'{baseSearch.ClassId}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@SaleId = N'{mySaleId}'," +
                    $"@Warning = {warning}," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@ParentIds = N'{baseSearch.ParentIds ?? ""}'," +
                    $"@StudentIds = N'{baseSearch.StudentIds ?? ""}'," +
                    $"@SortType = N'{(baseSearch.SortType ? 1 : 0)}'," +
                     $"@disable = N'{(baseSearch.disable ? 0 : 1)}'";
                var data = await db.SqlQuery<Get_StudentInClass>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_StudentInClass
                {
                    Id = i.Id,
                    Avatar = i.Avatar,
                    AvatarReSize = i.AvatarReSize,
                    BillDetailId = i.BillDetailId,
                    BranchId = i.BranchId,
                    ClassId = i.ClassId,
                    ClassName = i.ClassName,
                    ClassType = i.ClassType,
                    ClassTypeName = i.ClassTypeName,
                    CreatedBy = i.CreatedBy,
                    CreatedOn = i.CreatedOn,
                    Email = i.Email,
                    Enable = i.Enable,
                    FullName = i.FullName,
                    Mobile = i.Mobile,
                    ModifiedBy = i.ModifiedBy,
                    ModifiedOn = i.ModifiedOn,
                    Note = i.Note,
                    PaymentType = i.PaymentType,
                    PaymentTypeName = i.PaymentTypeName,
                    RemainingLesson = i.RemainingLesson,
                    RemainingMonth = i.RemainingMonth,
                    StudentId = i.StudentId,
                    TotalLesson = i.TotalLesson,
                    TotalMonth = i.TotalMonth,
                    Type = i.Type,
                    TypeName = i.TypeName,
                    UserCode = i.UserCode,
                    Warning = i.Warning,
                    HasCertificate = db.tbl_StudentCertificate.Any(x => x.StudentId == i.StudentId.Value && x.ClassId == i.ClassId.Value && x.Enable == true),
                    WarningContent = i.WarningContent
                }).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        //public static async Task<bool> HasCertificate(lmsDbContext db, int classId, int studentId)
        //{
        //    return await db.tbl_Certificate.AnyAsync(x => x.ClassId == classId && x.StudentId == studentId && x.Enable == true);
        //}
        public class Get_StudentAvailable_StudentInClass
        {
            public int UserInformationId { get; set; }
            public string FullName { get; set; }
            public string UserCode { get; set; }
            public string CurrentClassName { get; set; }
        }

        public static async Task<List<Get_StudentAvailable_StudentInClass>> GetStudentAvailable(AvailableStudentSearch request)
        {
            using (var db = new lmsDbContext())
            {
                if (request == null) request = new AvailableStudentSearch();
                string sql = $"Get_StudentAvailable_StudentInClass " +
                    $"@classId = {request.classId}";
                var data = await db.SqlQuery<Get_StudentAvailable_StudentInClass>(sql);
                return data;
            }
        }

        public static async Task<AppDomainResult> GetStudentInRegis(StudentInRegisSearch request, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    if (request == null) request = new StudentInRegisSearch();
                    string myBranchIds = "";
                    if (user.RoleId != (int)RoleEnum.admin)
                        myBranchIds = user.BranchIds;
                    int mySaleId = 0;
                    if (user.RoleId == (int)RoleEnum.sale)
                        mySaleId = user.UserInformationId;

                    DateTime endDate = request.EndDate ?? DateTime.Now.AddMonths(1);

                    //lấy danh sách học viên 
                    string sql = $"Get_StudentInRegis " +
                        $"@Search = N'{request.Search ?? ""}'," +
                        $"@PageIndex = {request.PageIndex}," +
                        $"@PageSize = {request.PageSize}," +
                        $"@ClassId = {request.ClassId}," +
                        $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                        $"@SaleId = {mySaleId}," +
                        $"@Sort = {request.Sort}," +
                        $"@ParentIds = N'{request.ParentIds ?? ""}'," +
                        $"@SortType = {(request.SortType ? 1 : 0)}," +
                        $"@Type = {request.Type}," +
                        $"@LessonRemaining = {request.LessonRemaining}," +
                        $"@EndDate = '{endDate.ToString("yyyy-MM-dd hh:mm:ss")}'";

                    var data = await db.SqlQuery<Get_StudentInRegis>(sql);
                    if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                    var totalRow = data[0].TotalRow;
                    var result = data.ToList();
                    return new AppDomainResult { TotalRow = totalRow, Data = result };
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static async Task<AppDomainResult> GetStudentInRegisV2(StudentInRegisV2Search request, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    if (request == null) request = new StudentInRegisV2Search();
                    string BranchIds = request.BranchIds ?? "";
                    if (user.RoleId != (int)RoleEnum.admin)
                        BranchIds = user.BranchIds;
                    int mySaleId = 0;
                    if (user.RoleId == (int)RoleEnum.sale)
                        mySaleId = user.UserInformationId;

                    DateTime endDate = request.EndDate ?? DateTime.Now.AddMonths(1);

                    //lấy danh sách học viên 
                    string sql = $"Get_StudentInRegisV2 " +
                        $"@Search = N'{request.Search ?? ""}'," +
                        $"@PageIndex = {request.PageIndex}," +
                        $"@PageSize = {request.PageSize}," +
                        $"@ClassId = {request.ClassId}," +
                        $"@BranchIds = '{BranchIds}'," +
                        $"@SaleId = {mySaleId}," +
                        $"@Sort = {request.Sort}," +
                        $"@ParentIds = N'{request.ParentIds ?? ""}'," +
                        $"@SortType = {(request.SortType ? 1 : 0)}," +
                        $"@Type = {request.Type}," +
                        $"@LessonRemaining = {request.LessonRemaining}," +
                        $"@EndDate = '{endDate.ToString("yyyy-MM-dd hh:mm:ss")}'";

                    var data = await db.SqlQuery<Get_StudentInRegis>(sql);
                    if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                    var totalRow = data[0].TotalRow;
                    var result = data.ToList();
                    return new AppDomainResult { TotalRow = totalRow, Data = result };
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static async Task<List<Get_UpcomingClass>> GetUpcommingClassByStudentId(int studentId)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    //lấy danh sách học viên 
                    string sql = $"Get_UpcommingClassByStudentId @studentId = '{studentId}'";
                    var data = await db.SqlQuery<Get_UpcomingClass>(sql);
                    return data;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public class AttendanceByStudentDTO
        {
            public int ClassId { get; set; }
            public int StudentId { get; set; }
            public int ScheduleId { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public int Status { get; set; }
            public string StatusName { get; set; }
            public string Note { get; set; }
        }
        public class AttendanceByStudentSearch
        {
            public int ClassId { get; set; }
            public int StudentId { get; set; }
        }
        public static async Task<List<AttendanceByStudentDTO>> GetAttendanceByStudent(AttendanceByStudentSearch baseSearch, tbl_UserInformation userlog)
        {
            using (var db = new lmsDbContext())
            {
                var result = new List<AttendanceByStudentDTO>();
                if (userlog.RoleId == (int)RoleEnum.student)
                    baseSearch.StudentId = userlog.UserInformationId;
                if (userlog.RoleId == (int)RoleEnum.parents)
                {
                    var hasStudent = await db.tbl_UserInformation
                        .AnyAsync(x => x.ParentId == userlog.UserInformationId && x.UserInformationId == baseSearch.StudentId);
                    if (!hasStudent)
                        return result;
                }

                var schedules = await db.tbl_Schedule
                    .Where(x => x.ClassId == baseSearch.ClassId && x.Enable == true).OrderBy(x => x.StartTime)
                    .Select(x => new { x.Id, x.StartTime, x.EndTime })
                    .ToListAsync();
                var rollUps = await db.tbl_RollUp
                    .Where(x => x.ClassId == baseSearch.ClassId && x.Enable == true && x.StudentId == baseSearch.StudentId)
                    .Select(x => new
                    {
                        x.ScheduleId,
                        x.Status,
                        x.StatusName,
                        x.Note
                    }).ToListAsync();
                result = (from s in schedules
                          join r in rollUps on s.Id equals r.ScheduleId
                          select new AttendanceByStudentDTO
                          {
                              ClassId = baseSearch.ClassId,
                              ScheduleId = s.Id,
                              EndTime = s.EndTime,
                              Note = r.Note,
                              StartTime = s.StartTime,
                              Status = r.Status ?? 0,
                              StatusName = r.StatusName,
                              StudentId = baseSearch.StudentId
                          }).ToList();
                return result;

            }
        }

        #region Chứng chỉ
        public class CreateCertificateRequest
        {
            public int StudentId { get; set; }
            public int ClassId { get; set; }
        }
        public static async Task<tbl_Certificate> CreateCertificate(CreateCertificateRequest itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var studentInClass = await db.tbl_StudentInClass.FirstOrDefaultAsync(x => x.StudentId == itemModel.StudentId && x.ClassId == itemModel.ClassId);
                if (studentInClass == null)
                    throw new Exception("Không tìm thấy học viện trong lớp này");
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == studentInClass.ClassId);
                if (_class == null)
                    throw new Exception("Không tìm lớp học");
                var certificateTemplate = await db.tbl_CertificateTemplate.SingleOrDefaultAsync(x => x.Id == _class.CertificateTemplateId);
                if (certificateTemplate == null)
                    throw new Exception("Không tìm thấy mẫu chứng chỉ");
                var hasCertificate = await db.tbl_Certificate.AnyAsync(x => x.StudentId == studentInClass.StudentId.Value && x.ClassId == _class.Id && x.Enable == true);
                if (hasCertificate)
                    throw new Exception("Không tìm thấy chứng chỉ");
                var certificateTemplateService = new CertificateTemplateService(db);
                string content = await certificateTemplateService.ReplaceContent(certificateTemplate.Content, studentInClass.StudentId.Value, _class.Id);
                var item = new tbl_Certificate
                {
                    Background = certificateTemplate.Background,
                    Backside = certificateTemplate.Backside,
                    ClassId = _class.Id,
                    Content = content,
                    Height = certificateTemplate.Height,
                    Width = certificateTemplate.Width,
                    StudentId = studentInClass.StudentId.Value,
                    Enable = true,
                    CreatedBy = userLog.FullName,
                    CreatedOn = DateTime.Now,
                    ModifiedBy = userLog.FullName,
                    ModifiedOn = DateTime.Now,
                };
                db.tbl_Certificate.Add(item);
                await db.SaveChangesAsync();
                return item;
            }
        }
        public static async Task RemoveCertificate(int studentInClassId)
        {
            using (var db = new lmsDbContext())
            {
                var studentInClass = await db.tbl_StudentInClass.SingleOrDefaultAsync(x => x.Id == studentInClassId);
                if (studentInClass == null)
                    throw new Exception("Không tìm thấy học viện trong lớp này");
                var entity = await db.tbl_Certificate.FirstOrDefaultAsync(x => x.StudentId == studentInClass.StudentId.Value && x.ClassId == studentInClass.ClassId && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy chứng chỉ");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public class UpdateCertificateRequest
        {
            public int StudentId { get; set; }
            public int ClassId { get; set; }
            public string Content { get; set; }
        }
        public static async Task<tbl_Certificate> UpdateCertificate(UpdateCertificateRequest itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var studentInClass = await db.tbl_StudentInClass.FirstOrDefaultAsync(x => x.StudentId == itemModel.StudentId && x.ClassId == itemModel.ClassId);
                if (studentInClass == null)
                    throw new Exception("Không tìm thấy học viện trong lớp này");
                var entity = await db.tbl_Certificate.FirstOrDefaultAsync(x => x.StudentId == studentInClass.StudentId.Value && x.ClassId == studentInClass.ClassId && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy chứng chỉ");
                entity.Content = itemModel.Content ?? entity.Content;
                entity.ModifiedBy = userLog.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public class ExportCertificateRequest
        {
            public int StudentId { get; set; }
            public int ClassId { get; set; }
            public string Content { get; set; }
        }
        public static async Task<tbl_Certificate> ExportCertificate(ExportCertificateRequest itemModel, string path, string domain)
        {
            using (var db = new lmsDbContext())
            {
                var studentInClass = await db.tbl_StudentInClass.FirstOrDefaultAsync(x => x.StudentId == itemModel.StudentId && x.ClassId == itemModel.ClassId);
                if (studentInClass == null)
                    throw new Exception("Không tìm thấy học viện trong lớp này");
                var entity = await db.tbl_Certificate.FirstOrDefaultAsync(x => x.StudentId == studentInClass.StudentId.Value && x.ClassId == studentInClass.ClassId && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy chứng chỉ");
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == studentInClass.ClassId);
                string classCode = AssetCRM.RemoveUnicodeAndSpace(_class.Name);
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentInClass.StudentId);

                string certificateUrl = $"Certificate/certificate{student.UserCode}{classCode.Replace("/", "_")}.pdf";
                string savePath = $"{path}/{certificateUrl}";
                if (!string.IsNullOrEmpty(entity.PDFUrl))
                {
                    try
                    {
                        File.Delete(savePath);
                    }
                    catch { }
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
                    {
                        Path = path
                    });
                    await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
                    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        ExecutablePath = browserFetcher.RevisionInfo(BrowserFetcher.DefaultRevision).ExecutablePath
                    });
                    var page = await browser.NewPageAsync();
                    await page.EmulateMediaTypeAsync(MediaType.Screen);
                    await page.SetContentAsync(itemModel.Content);
                    var pdfContent = await page.PdfStreamAsync(new PdfOptions
                    {
                        Width = entity.Width,
                        Height = entity.Height,
                        PrintBackground = true,
                        MarginOptions = new MarginOptions() { Top = "0px" },
                    });
                    await pdfContent.CopyToAsync(stream);

                    byte[] bytes = stream.ToArray();
                    stream.Close();
                    await browser.CloseAsync();

                    var newStream = new MemoryStream(bytes);

                    using (var fileStream = File.Create(savePath))
                    {
                        newStream.WriteTo(fileStream);
                    }
                }
                entity.PDFUrl = $"{domain}/Upload/{certificateUrl}";
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public class CertificateSearch
        {
            public int studentId { get; set; }
            public int classId { get; set; }
        }
        public static async Task<tbl_Certificate> GetCertificate(CertificateSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                return await db.tbl_Certificate.FirstOrDefaultAsync(x => x.StudentId == baseSearch.studentId && x.ClassId == baseSearch.classId && x.Enable == true);
            }
        }
        #endregion

        public static async Task<AppDomainResult> GetAllV2(StudentInClassV2Search baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StudentInClassV2Search();

                string myBranchIds = baseSearch.BranchIds ?? "";
                if (user.RoleId != (int)RoleEnum.admin)
                {
                    myBranchIds = user.BranchIds;
                }

                int mySaleId = 0;
                if (user.RoleId == (int)RoleEnum.sale)
                    mySaleId = user.UserInformationId;
                if (user.RoleId == (int)RoleEnum.student)
                {
                    baseSearch.StudentIds = user.UserInformationId.ToString();
                    myBranchIds = "";
                }
                if (user.RoleId == (int)RoleEnum.parents)
                {
                    myBranchIds = "";
                }
                string warning = baseSearch.Warning.HasValue ? baseSearch.Warning.Value.ToString() : "null";
                string sql = $"Get_StudentInClass @Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = N'{baseSearch.ClassId}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@SaleId = N'{mySaleId}'," +
                    $"@Warning = {warning}," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@ParentIds = N'{baseSearch.ParentIds ?? ""}'," +
                    $"@StudentIds = N'{baseSearch.StudentIds ?? ""}'," +
                    $"@SortType = N'{(baseSearch.SortType ? 1 : 0)}'," +
                     $"@disable = N'{(baseSearch.disable ? 0 : 1)}'";
                var data = await db.SqlQuery<Get_StudentInClass>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_StudentInClass
                {
                    Id = i.Id,
                    Avatar = i.Avatar,
                    AvatarReSize = i.AvatarReSize,
                    BillDetailId = i.BillDetailId,
                    BranchId = i.BranchId,
                    ClassId = i.ClassId,
                    ClassName = i.ClassName,
                    ClassType = i.ClassType,
                    ClassTypeName = i.ClassTypeName,
                    CreatedBy = i.CreatedBy,
                    CreatedOn = i.CreatedOn,
                    Email = i.Email,
                    Enable = i.Enable,
                    FullName = i.FullName,
                    Mobile = i.Mobile,
                    ModifiedBy = i.ModifiedBy,
                    ModifiedOn = i.ModifiedOn,
                    Note = i.Note,
                    PaymentType = i.PaymentType,
                    PaymentTypeName = i.PaymentTypeName,
                    RemainingLesson = i.RemainingLesson,
                    RemainingMonth = i.RemainingMonth,
                    StudentId = i.StudentId,
                    TotalLesson = i.TotalLesson,
                    TotalMonth = i.TotalMonth,
                    Type = i.Type,
                    TypeName = i.TypeName,
                    UserCode = i.UserCode,
                    Warning = i.Warning,
                    HasCertificate = db.tbl_StudentCertificate.Any(x => x.StudentId == i.StudentId.Value && x.ClassId == i.ClassId.Value && x.Enable == true),
                    WarningContent = i.WarningContent,
                }).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}