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
using System.Threading;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using LMSCore.Services.PaymentSession;
using static LMSCore.DTO.UserInformationDTO.TeacherScheduleExpectedDTO;

namespace LMSCore.Services
{
    public class SalaryService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public SalaryService(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public static async Task<tbl_Salary> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Salary.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_Salary> Insert(SalaryCreate itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var user = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.UserId);
                if (user == null)
                    throw new Exception("Không tìm thấy nhân viên");
                var checkExist = await db.tbl_Salary
                    .AnyAsync(x => x.Year == itemModel.Year && x.Month == itemModel.Month && x.UserId == itemModel.UserId && x.Enable == true);
                if (checkExist)
                    throw new Exception($"Đã tính lương cho nhân viên {user.FullName}");

                var model = new tbl_Salary(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_Salary.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        /// <summary>
        /// Tính lương
        /// </summary>
        /// <returns></returns>
        public static async Task SalaryClosing(tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        DateTime time = DateTime.Now.AddMonths(-1);
                        int year = time.Year;
                        int month = time.Month;
                        //var check = await db.tbl_Salary.AnyAsync(x => x.Year == year && x.Month == month && x.Enable == true);
                        //if (check)
                        //    throw new Exception($"Đã tính lương tháng {month} năm {year}");
                        var staffs = await db.tbl_UserInformation
                            .Where(x => x.Enable == true && x.StatusId == ((int)AccountStatus.active) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && x.RoleId != ((int)RoleEnum.admin))
                            .Select(x => new { x.UserInformationId, x.RoleId })
                            .ToListAsync();
                        if (staffs.Any())
                        {
                            foreach (var item in staffs)
                            {
                                var check = await db.tbl_Salary.AnyAsync(x => x.Year == year && x.Month == month && x.Enable == true && x.UserId == item.UserInformationId);
                                if (check)
                                    continue; // Đã tính lương rồi thì không tính nữa
                                var salaryConfig = await db.tbl_SalaryConfig.FirstOrDefaultAsync(x => x.UserId == item.UserInformationId && x.Enable == true);
                                var salary = new tbl_Salary
                                {
                                    BasicSalary = salaryConfig?.Value ?? 0,
                                    Bonus = 0,
                                    CreatedBy = user.FullName,
                                    CreatedOn = DateTime.Now,
                                    Deduction = 0,
                                    Enable = true,
                                    ModifiedBy = user.FullName,
                                    ModifiedOn = DateTime.Now,
                                    Month = month,
                                    Note = "",
                                    Status = 1,
                                    StatusName = "Chưa chốt",
                                    TotalSalary = salaryConfig?.Value ?? 0,
                                    TeachingSalary = 0,
                                    UserId = item.UserInformationId,
                                    Year = year,
                                };
                                db.tbl_Salary.Add(salary);
                                await db.SaveChangesAsync();
                                double teachingSalary = 0;
                                var schedules = await db.tbl_Schedule
                                    .Where(x => x.TeacherAttendanceId == item.UserInformationId && x.Enable == true && x.StartTime.Value.Month == month && x.StartTime.Value.Year == year)
                                    .ToListAsync();
                                if (schedules.Any())
                                {
                                    foreach (var schedule in schedules)
                                    {
                                        schedule.SalaryId = salary.Id;
                                        teachingSalary += item.RoleId == (int)RoleEnum.tutor ? schedule.TutorFee ?? 0 : schedule.TeachingFee ?? 0;
                                        await db.SaveChangesAsync();
                                    }
                                }
                                salary.TeachingSalary = teachingSalary;
                                salary.TotalSalary += teachingSalary;
                                await db.SaveChangesAsync();
                            }
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
        public static async Task<tbl_Salary> Update(SalaryUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Salary.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (entity.Status == 3)
                    throw new Exception("Đã thanh toán lương, không thể chỉnh sửa");
                entity.Deduction = itemModel.Deduction ?? entity.Deduction;
                entity.Bonus = itemModel.Bonus ?? entity.Bonus;
                entity.Note = itemModel.Note ?? entity.Note;
                entity.TotalSalary = (entity.BasicSalary + entity.TeachingSalary + entity.Bonus) - entity.Deduction;
                entity.Status = itemModel.Status ?? entity.Status;
                entity.StatusName = itemModel.StatusName ?? entity.StatusName;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                if (entity.TotalSalary < 0)
                    throw new Exception("Lương không phù hợp");
                if (entity.Status == 3 && entity.TotalSalary > 0)
                {
                    var staff = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.UserId);
                    var payment = await db.tbl_PaymentMethod.FirstOrDefaultAsync(x => x.Code == "transfer");
                    db.tbl_PaymentSession.Add(new tbl_PaymentSession
                    {
                        BranchId = 0,
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        PaymentMethodId = payment?.Id,
                        Reason = $"Thanh toán lương nhân viên {staff?.FullName}",
                        UserId = entity.UserId,
                        Note = itemModel.Note,
                        Type = 2,
                        TypeName = "Chi",
                        Value = entity.TotalSalary,
                        PrintContent = Task.Run(() => PaymentSessionService.GetPrintContent(
                            2,
                            entity.UserId,
                            $"Thanh toán lương nhân viên",
                            entity.TotalSalary,
                            user.FullName
                            )).Result,
                    });
                    ////thông báo nhận lương

                    //Thread sendNoti = new Thread(async () =>
                    //{
                    //    SalaryParam param = new SalaryParam { Month = entity.Month, Year = entity.Year };
                    //    string paramString = JsonConvert.SerializeObject(param);
                    //    await NotificationService.Send(new tbl_Notification
                    //    {
                    //        Title = configuration.GetSection("MySettings:ProjectName").Value.ToString() + " thông báo lương tháng " + entity.Month + " năm " + entity.Year,
                    //        Content = configuration.GetSection("MySettings:ProjectName").Value.ToString() + " đã gửi lương tháng " + entity.Month + " năm " + entity.Year + " là " + entity.TotalSalary + " đến bạn. Vui lòng kiểm tra.",
                    //        ContentEmail = configuration.GetSection("MySettings:ProjectName").Value.ToString() + " đã gửi lương tháng " + entity.Month + " năm " + entity.Year + " là " + entity.TotalSalary + " đến bạn. Vui lòng kiểm tra.",
                    //        UserId = entity.UserId
                    //    }, user);
                    //});
                    //sendNoti.Start();
                }
                await db.SaveChangesAsync();
                return entity;
            }
        }

        public static async Task MultipleUpdate(SalaryMultipleUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    if (itemModel.Ids == null || itemModel.Ids.Count == 0)
                        throw new Exception("Vui lòng chọn nhân viên");
                    var staffIds = itemModel.Ids;
                    var entity = await db.tbl_Salary.Where(x => staffIds.Contains(x.Id) && x.Enable == true).ToListAsync();
                    if (entity == null || entity.Count == 0)
                        throw new Exception("Không tìm thấy dữ liệu");
                    var isCompleted = entity.Any(x => x.Status == 3);
                    if (isCompleted)
                        throw new Exception("Vui lòng chọn các nhân viên chưa được thanh toán lương");

                    foreach (var item in entity)
                    {
                        await Update(new SalaryUpdate { Status = itemModel.Status, Id = item.Id }, user);
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static async Task<SalaryResult> GetAll(SalarySearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SalarySearch();
                int userId = 0;
                if (user.RoleId != ((int)RoleEnum.admin) && user.RoleId != ((int)RoleEnum.accountant))
                    userId = user.UserInformationId;

                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;

                string sql = $"Get_Salary @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@Status = {baseSearch.Status ?? 0}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Year = N'{baseSearch.Year}'," +
                    $"@Month = N'{baseSearch.Month}'," +
                    $"@UserId = N'{userId}'," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds}'," +
                    $"@Roles = N'{baseSearch.Roles ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'";
                var data = await db.SqlQuery<Get_Salary>(sql);
                if (!data.Any()) return new SalaryResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                int allState = data[0].AllState;
                int unfinished = data[0].Unfinished;
                int finished = data[0].Finished;
                int paid = data[0].Paid;
                var result = data.Select(i => new tbl_Salary(i)).ToList();
                return new SalaryResult { TotalRow = totalRow, Data = result, AllState = allState, Unfinished = unfinished, Finished = finished, Paid = paid };
            }
        }

        public static async Task<SalaryResult> GetAllV2(SalaryV2Search baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SalaryV2Search();
                int userId = 0;
                if (user.RoleId != ((int)RoleEnum.admin) && user.RoleId != ((int)RoleEnum.accountant))
                    userId = user.UserInformationId;

                string myBranchIds = baseSearch.BranchIds ?? "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;

                string sql = $"Get_Salary @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@Status = {baseSearch.Status ?? 0}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Year = N'{baseSearch.Year}'," +
                    $"@Month = N'{baseSearch.Month}'," +
                    $"@UserId = N'{userId}'," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds}'," +
                    $"@Roles = N'{baseSearch.Roles ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'";
                var data = await db.SqlQuery<Get_Salary>(sql);
                if (!data.Any()) return new SalaryResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                int allState = data[0].AllState;
                int unfinished = data[0].Unfinished;
                int finished = data[0].Finished;
                int paid = data[0].Paid;
                var result = data.Select(i => new tbl_Salary(i)).ToList();
                return new SalaryResult { TotalRow = totalRow, Data = result, AllState = allState, Unfinished = unfinished, Finished = finished, Paid = paid };
            }
        }

        public static async Task<List<Get_SalaryExport>> RepairDataToExport(SalaryV2Search baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SalaryV2Search();
                int userId = 0;
                if (user.RoleId != ((int)RoleEnum.admin) && user.RoleId != ((int)RoleEnum.accountant))
                    userId = user.UserInformationId;

                string myBranchIds = baseSearch.BranchIds ?? "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;

                string sql = $"Get_Salary @Search = N'{baseSearch.Search ?? ""}', @PageIndex = 1," +
                    $"@Status = {baseSearch.Status ?? 0}," +
                    $"@PageSize = 999999," +
                    $"@Year = N'{baseSearch.Year}'," +
                    $"@Month = N'{baseSearch.Month}'," +
                    $"@UserId = N'{userId}'," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds}'," +
                    $"@Roles = N'{baseSearch.Roles ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'";
                var data = await db.SqlQuery<Get_SalaryExport>(sql);
                if (!data.Any())
                    throw new Exception("Không có dữ liệu!");
                return data;
            }
        }

        public class SalaryResult : AppDomainResult
        {
            public int AllState { get; set; }
            public int Unfinished { get; set; }
            public int Finished { get; set; }
            public int Paid { get; set; }
        }

        public static async Task<AppDomainResult> GetTeachingDetail(TeachingDetailSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TeachingDetailSearch();
                string sql = $"Get_TeachingDetail @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@SalaryId = N'{baseSearch.SalaryId}'";
                var data = await db.SqlQuery<Get_TeachingDetail>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                // Thêm OrderBy by Dery
                var result = data.Select(i => new TeachingDetailModel(i)).ToList().OrderBy(x => x.ClassName);
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        public class Get_UserAvailable_Salary
        {
            public int UserInformationId { get; set; }
            public string FullName { get; set; }
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public string UserCode { get; set; }
        }
        public class UserAvailableSearch
        {
            public int Year { get; set; }
            public int Month { get; set; }
        }
        public class UserAvailableV2Search
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string BranchIds { get; set; }
        }
        public static async Task<List<Get_UserAvailable_Salary>> GetUserAvailable(UserAvailableSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new UserAvailableSearch();
                string sql = $"Get_UserAvailable_Salary @year = {baseSearch.Year}, @month = {baseSearch.Month}";
                var data = await db.SqlQuery<Get_UserAvailable_Salary>(sql);
                return data;
            }
        }

        public static async Task<List<Get_UserAvailable_Salary>> GetUserAvailableV2(UserAvailableV2Search baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new UserAvailableV2Search();
                string sql = $"Get_UserAvailable_SalaryV2 @year = {baseSearch.Year}, @month = {baseSearch.Month}, @BranchIds = '{baseSearch.BranchIds ?? ""}'";
                var data = await db.SqlQuery<Get_UserAvailable_Salary>(sql);
                return data;
            }
        }

        /// <summary>
        /// Tính lương
        /// </summary>
        /// <returns></returns>
        public static async Task SalaryClosingV2(tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        DateTime time = DateTime.Now.AddMonths(-1);
                        int year = time.Year;
                        int month = time.Month;
                        //var check = await db.tbl_Salary.AnyAsync(x => x.Year == year && x.Month == month && x.Enable == true);
                        //if (check)
                        //    throw new Exception($"Đã tính lương tháng {month} năm {year}");
                        var staffs = await db.tbl_UserInformation
                            .Where(x => x.Enable == true && x.StatusId == ((int)AccountStatus.active) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && x.RoleId != ((int)RoleEnum.admin))
                            .Select(x => new { x.UserInformationId, x.RoleId })
                            .ToListAsync();
                        if (staffs.Any())
                        {
                            foreach (var item in staffs)
                            {
                                var check = await db.tbl_Salary.AnyAsync(x => x.Year == year && x.Month == month && x.Enable == true && x.UserId == item.UserInformationId);
                                if (check)
                                    continue; // Đã tính lương rồi thì không tính nữa
                                var salaryConfig = await db.tbl_SalaryConfig.FirstOrDefaultAsync(x => x.UserId == item.UserInformationId && x.Enable == true);
                                var salary = new tbl_Salary
                                {
                                    BasicSalary = salaryConfig?.Value ?? 0,
                                    Bonus = 0,
                                    CreatedBy = user.FullName,
                                    CreatedOn = DateTime.Now,
                                    Deduction = 0,
                                    Enable = true,
                                    ModifiedBy = user.FullName,
                                    ModifiedOn = DateTime.Now,
                                    Month = month,
                                    Note = "",
                                    Status = 1,
                                    StatusName = "Chưa chốt",
                                    TotalSalary = salaryConfig?.Value ?? 0,
                                    TeachingSalary = 0,
                                    UserId = item.UserInformationId,
                                    Year = year,
                                };
                                db.tbl_Salary.Add(salary);
                                await db.SaveChangesAsync();
                                double teachingSalary = 0;
                                var schedules = await db.tbl_Schedule
                                    .Where(x => x.TeacherAttendanceId == item.UserInformationId && x.Enable == true && x.StartTime.Value.Month == month && x.StartTime.Value.Year == year)
                                    .ToListAsync();

                                // Gắn lương/buổi cho những lịch học giáo viên đã được điểm danh theo tháng
                                var classId = schedules.Select(x => x.ClassId).Distinct().ToList();
                                foreach (var cl in classId)
                                {
                                    var _class = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == cl);
                                    var programOfTeacher = await db.tbl_TeacherInProgram.FirstOrDefaultAsync(x => x.TeacherId == item.UserInformationId && x.ProgramId == _class.ProgramId);
                                    if (programOfTeacher != null)
                                    {
                                        foreach (var sch in schedules)
                                        {
                                            if (sch.ClassId == _class.Id)
                                            {
                                                sch.TeachingFee = programOfTeacher.TeachingFee;
                                                await db.SaveChangesAsync();
                                            }
                                        }
                                    }
                                }


                                if (schedules.Any())
                                {
                                    foreach (var schedule in schedules)
                                    {
                                        schedule.SalaryId = salary.Id;
                                        teachingSalary += item.RoleId == (int)RoleEnum.tutor ? schedule.TutorFee ?? 0 : schedule.TeachingFee ?? 0;
                                        await db.SaveChangesAsync();
                                    }
                                }
                                salary.TeachingSalary = teachingSalary;
                                salary.TotalSalary += teachingSalary;
                                await db.SaveChangesAsync();
                            }
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

        /// <summary>
        /// Lấy tất cả lương dự kiến và thực tế của giáo viên
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<AppDomainResult> GetAllSalaryExpected(ExpectedSalarySearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    if (baseSearch == null)
                        baseSearch = new ExpectedSalarySearch();

                    //string myBranchIds = baseSearch.BranchIds ?? "";
                    //if (user.RoleId != ((int)RoleEnum.admin)
                    //    && user.RoleId != ((int)RoleEnum.academic)
                    //    && user.RoleId != ((int)RoleEnum.manager))
                    //{
                    //    myBranchIds = user.BranchIds;
                    //}
                    if (!baseSearch.FromDate.HasValue)
                        baseSearch.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    if (!baseSearch.ToDate.HasValue)
                    {
                        var lastDayOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                        baseSearch.ToDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, lastDayOfMonth);
                    }

                    string sql = $"Get_Teacher_Schedule @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                        $"@PageSize = {baseSearch.PageSize}," +
                        $"@FromDate = N'{baseSearch.FromDate?.ToString("yyyy/MM/dd") ?? ""}'," +
                        $"@ToDate = N'{baseSearch.ToDate?.ToString("yyyy/MM/dd") ?? ""}'," +
                        $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'";
                    var data = await db.SqlQuery<TeacherScheduleExpected>(sql);
                    if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                    var totalRow = data[0].TotalRow;
                    var _class = await db.tbl_Class.Where(x => x.Enable == true).ToListAsync();
                    var teacherInProgram = await db.tbl_TeacherInProgram.Where(x => x.Enable == true).ToListAsync();
                    var schedule = await db.tbl_Schedule.Where(x => x.Enable == true).ToListAsync();
                    schedule = schedule
                        .Where(x => (!baseSearch.FromDate.HasValue || x.StartTime.Value.Date >= baseSearch.FromDate.Value.Date) &&
                                    (!baseSearch.ToDate.HasValue || x.StartTime.Value.Date <= baseSearch.ToDate.Value.Date))
                        .ToList();
                    var salaryConfig = await db.tbl_SalaryConfig.Where(x => x.Enable == true).ToListAsync();
                    foreach (var t in data)
                    {
                        double totalSalaryAttendance = 0; // Tổng lương các buổi đã đi dạy + lương cứng
                        double totalSalaryExpected = 0; // Tổng lương dự kiến (tổng các buổi đi dạy và không đi dạy) + lương cứng
                        double salaryAttendance = 0; // Lương các buổi đã đi dạy
                        double salaryExpected = 0; // Lương dự kiến (tổng các buổi đi dạy và không đi dạy)
                        double teacherSalary = 0; // Lương cứng của giáo viên

                        // Lương cứng của giáo viên
                        var teacherSalaryConfig = salaryConfig.FirstOrDefault(x => x.UserId == t.TeacherId);
                        if (teacherSalaryConfig != null) teacherSalary = teacherSalaryConfig.Value;

                        if (schedule.Count != 0)
                        {
                            // Lấy danh sách ID của lớp học
                            var teacherInClass = schedule.Where(x => x.TeacherId == t.TeacherId).Select(x => x.ClassId).Distinct().ToList();
                            if (teacherInClass.Count != 0)
                            {
                                foreach (var c in teacherInClass)
                                {
                                    // Lấy thông tin lớp học
                                    var classInfor = _class.FirstOrDefault(x => x.Id == c);
                                    // Lấy thông tin lương
                                    var teachingFee = teacherInProgram.FirstOrDefault(x => x.TeacherId == t.TeacherId && x.ProgramId == classInfor.ProgramId);
                                    if (teachingFee != null)
                                    {
                                        // Lấy thông tin các lịch dạy giáo viên đã đi dạy khi giáo viên
                                        var scheduleAttended = schedule.Count(x => x.ClassId == classInfor.Id && x.TeacherId == t.TeacherId && x.TeacherAttendanceId == t.TeacherId);
                                        // Lấy thông tin các lịch dạy dự kiến của giáo viên
                                        var scheduleExpected = schedule.Count(x => x.ClassId == classInfor.Id && x.TeacherId == t.TeacherId);
                                        // Tính lương
                                        totalSalaryAttendance += (double)(scheduleAttended * teachingFee.TeachingFee ?? 0);
                                        totalSalaryExpected += (double)(scheduleExpected * teachingFee.TeachingFee ?? 0);
                                        salaryAttendance += (double)(scheduleAttended * teachingFee.TeachingFee ?? 0);
                                        salaryExpected += (double)(scheduleExpected * teachingFee.TeachingFee ?? 0);
                                    }
                                }
                            }
                        }
                        t.SalaryConfig = teacherSalary;
                        t.TotalSalaryAttendance = totalSalaryAttendance + teacherSalary;
                        t.TotalSalaryExpected = totalSalaryExpected + teacherSalary;
                        t.SalaryAttendance = salaryAttendance;
                        t.SalaryExpected = salaryExpected;
                    }
                    return new AppDomainResult { TotalRow = totalRow, Data = data };
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}