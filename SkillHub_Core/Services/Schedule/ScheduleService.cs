using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static LMSCore.Models.lmsEnum;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using static LMSCore.Services.Class.ClassService;
using LMSCore.Services.Class;
using Hangfire;
using static LMSCore.DTO.ScheduleDTO.ScheduleExpectedDTO;
using static LMSCore.DTO.ScheduleDTO.TeacherAvailableScheduleDTO;
using static LMSCore.DTO.ScheduleDTO.RoomAvailableScheduleDTO;

namespace LMSCore.Services.Schedule
{
    public class ScheduleService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public ScheduleService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public static async Task<tbl_Schedule> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                {
                    var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == data.ClassId);
                    var teacher = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.TeacherId);
                    data.ClassName = _class == null ? "" : _class.Name;
                    data.TeacherName = teacher == null ? "" : teacher.FullName;
                }
                return data;
            }
        }

        public static async Task<bool> Validate(ScheduleCreate itemModel, tbl_UserInformation user, lmsDbContext db)
        {
            var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.ClassId);
            if (_class == null)
                throw new Exception("Không tìm thấy lớp học");
            if (itemModel.EndTime <= itemModel.StartTime)
                throw new Exception("Thời gian không phù hợp");
            var model = new tbl_Schedule(itemModel);
            if (_class.Type == 3)
            {
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == _class.CurriculumId);
                if (curriculum == null)
                    throw new Exception("Không tìm thấy giáo trình");
                var countSchedule = await db
                    .tbl_Schedule.CountAsync(x => x.ClassId == itemModel.ClassId
                    && x.Enable == true
                    && x.StatusTutoring != 2
                    && x.StatusTutoring != 4
                    && x.StatusTutoring != 5);
                if (countSchedule >= curriculum.Lesson)
                    throw new Exception($"Bạn đã học hết số buổi theo giáo trình {curriculum.Name}");

                int openTime = 0;
                var openTutoring = await db.tbl_Config.FirstOrDefaultAsync(x => x.Code == "OpenTutoring");
                if (openTutoring != null)
                    openTime = int.Parse(openTutoring.Value);

                var timeNow = DateTime.Now.AddHours(-openTime);
                if (itemModel.StartTime < timeNow)
                    throw new Exception($"Vui lòng đặt lịch trước {openTime} tiếng");

                model.StatusTutoring = 1;
                model.StatusTutoringName = "Mới đặt";
                var checkScheduleAvailable = await db.tbl_ScheduleAvailable
                    .AnyAsync(x => x.StartTime <= itemModel.StartTime && x.EndTime >= itemModel.EndTime && x.Enable == true);
                if (!checkScheduleAvailable)
                    throw new Exception("Giáo viên không mở lịch thời gian này");

                var tutoringFee = await db.tbl_TutoringFee
                    .FirstOrDefaultAsync(x => x.TeacherId == model.TeacherId && x.Enable == true);
                if (tutoringFee == null)
                    model.TeachingFee = 0;
                else model.TeachingFee = tutoringFee.Fee;

                //Kiểm tra lịch học của học viên khi tạo lịch
                var myClass = await db.tbl_StudentInClass.Where(x => x.StudentId == user.UserInformationId && x.Enable == true).Select(x => x.ClassId).ToListAsync();
                var checkMySchedule = await db.tbl_Schedule
                    .FirstOrDefaultAsync(x => x.Enable == true && x.StartTime < itemModel.EndTime && x.EndTime > itemModel.StartTime && myClass.Contains(x.ClassId)
                        && x.StatusTutoring != 2 //Lớp dạy kèm bỏ qua buổi đã hủy
                        && x.StatusTutoring != 4
                        && x.StatusTutoring != 5);
                if (checkMySchedule != null)
                    throw new Exception($"Bạn đã có lịch học từ {checkMySchedule.StartTime} đến {checkMySchedule.EndTime}");
            }

            if (model.RoomId != 0)
            {
                var checkRoom = await ClassService.CheckRoom(db, 0, model.RoomId, model.StartTime.Value, model.EndTime.Value);
                if (checkRoom.Value == false)
                    throw new Exception(checkRoom.Note);
            }
            var checkTeacher = await ClassService.CheckTeacher(db, 0, model.TeacherId, model.StartTime.Value, model.EndTime.Value);
            if (checkTeacher.Value == false)
                throw new Exception(checkTeacher.Note);

            var checkSchedule = await db.tbl_Schedule
                .FirstOrDefaultAsync(x => x.Enable == true && x.StartTime < itemModel.EndTime && x.EndTime > itemModel.StartTime && x.ClassId == model.ClassId);
            if (checkSchedule != null)
                throw new Exception($"Trùng lịch từ {checkSchedule.StartTime} đến {checkSchedule.EndTime}");

            return true;
        }

        public static async Task<tbl_Schedule> Insert(ScheduleCreate itemModel, tbl_UserInformation user, lmsDbContext db)
        {
            await Validate(itemModel, user, db);
            var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.ClassId);
            var model = new tbl_Schedule(itemModel);
            model.BranchId = _class.BranchId ?? 0;
            model.CreatedBy = model.ModifiedBy = user.FullName;
            db.tbl_Schedule.Add(model);
            await db.SaveChangesAsync();
            await UpdateDayClass(db, model.ClassId.Value);


            BackgroundJob.Schedule(() => ScheduleNotification.NotifyStudentClassHasANewSchedule(new ScheduleNotificationRequest.NotifyStudentClassHasANewScheduleRequest
            {
                ScheduleId = model.Id,
                CurrentUser = user,
            }), TimeSpan.FromSeconds(2));

            BackgroundJob.Schedule(() => ScheduleNotification.NotifyTeacherClassHasANewSchedule(new ScheduleNotificationRequest.NotifyTeacherClassHasANewScheduleRequest
            {
                ScheduleId = model.Id,
                CurrentUser = user,
            }), TimeSpan.FromSeconds(2));
            return model;
        }
        public static async Task MultipleInsert(MultipleScheduleCreate itemModel, tbl_UserInformation user, lmsDbContext db)
        {
            if (itemModel.schedules != null && itemModel.schedules.Count > 0)
            {
                var schedules = itemModel.schedules;
                foreach (var schedule in schedules)
                {
                    await Insert(schedule, user, db);
                }
                var classId = schedules.FirstOrDefault()?.ClassId;
                if (classId != null)
                    await UpdateDayClass(db, classId.Value);
            }
        }
        public static async Task UpdateDayClass(lmsDbContext db, int classId)
        {
            var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == classId);
            if (_class != null)
            {
                var startSchedule = await db.tbl_Schedule
                    .Where(x => x.ClassId == classId && x.Enable == true).OrderBy(x => x.StartTime).FirstOrDefaultAsync();
                _class.StartDay = startSchedule?.StartTime;

                var lastSchedule = await db.tbl_Schedule
                    .Where(x => x.ClassId == classId && x.Enable == true).OrderByDescending(x => x.EndTime).FirstOrDefaultAsync();
                _class.EndDay = lastSchedule?.EndTime;
                if (lastSchedule != null)
                {
                    if (_class.EndDay < GetDateTime.Now)
                    {
                        _class.Status = 3;
                        _class.StatusName = "Đã kết thúc";
                    }
                    else if (_class.StartDay > GetDateTime.Now)
                    {
                        _class.Status = 1;
                        _class.StatusName = "Sắp diễn ra";
                    }
                    else
                    {
                        _class.Status = 2;
                        _class.StatusName = "Đang diễn ra";
                    }
                }
                else
                {
                    _class.Status = 1;
                    _class.StatusName = "Sắp diễn ra";
                }
                await db.SaveChangesAsync();
            }
        }
        public static async Task<tbl_Schedule> Update(ScheduleUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);

                // Dữ liệu lịch học trước và sau khi thay đổi
                string oldStartTime = entity.StartTime.Value.ToString("dddd, dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));
                string newStartTime = itemModel.StartTime.Value.ToString("dddd, dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));
                string oldEndTime = entity.EndTime.Value.ToString("dddd, dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));
                string newEndTime = itemModel.EndTime.Value.ToString("dddd, dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));

                // Dữ liệu giáo viên trước và sau khi thay đổi
                var oldTeacher = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == entity.TeacherId);
                string oldTeacherName = oldTeacher.FullName;
                var newTeacher = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == itemModel.TeacherId);
                string newTeacherName = "";
                if (newTeacher != null)
                    newTeacherName = newTeacher.FullName;

                // Cấu hình gửi thông báo
                string content = "";
                string contentToTeacher = "";
                string notificationContent = "";
                string notificationContentToTeacher = "";
                string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
                var pathViews = Path.Combine(_hostingEnvironment.ContentRootPath, "Views");

                content = File.ReadAllText($"{pathViews}/Base/Mail/Class/UpdateSchedule.cshtml");
                contentToTeacher = File.ReadAllText($"{pathViews}/Base/Mail/Class/UpdateSchedule.cshtml");

                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (entity.TeacherAttendanceId != 0)
                    throw new Exception("Đã học không thể chỉnh sửa");
                if (entity.StatusTutoring == 2)
                    throw new Exception("Buổi học đã hủy, không thể cập nhật");
                if (entity.StatusTutoring == 4 || entity.StatusTutoring == 5)
                    throw new Exception("Không thể cập nhật buổi này");
                if (entity.SalaryId.HasValue && entity.SalaryId != 0 && entity.TeachingFee != itemModel.TeachingFee && itemModel.TeachingFee.HasValue)
                    throw new Exception("Buổi học này đã được tính lương, không thể cập nhật lương giảng dạy");
                entity.StatusTutoring = itemModel.StatusTutoring ?? entity.StatusTutoring;
                entity.StatusTutoringName = itemModel.StatusTutoringName ?? entity.StatusTutoringName;
                entity.RoomId = itemModel.RoomId ?? entity.RoomId;
                entity.StartTime = itemModel.StartTime ?? entity.StartTime;
                entity.EndTime = itemModel.EndTime ?? entity.EndTime;
                entity.TeacherId = itemModel.TeacherId ?? entity.TeacherId;
                entity.Note = itemModel.Note ?? entity.Note;
                entity.TeachingFee = itemModel.TeachingFee ?? entity.TeachingFee;
                entity.TutorFee = itemModel.TutorFee ?? entity.TutorFee;

                if (entity.RoomId != 0)
                {
                    var checkRoom = await ClassService.CheckRoom(db, entity.Id, entity.RoomId, entity.StartTime.Value, entity.EndTime.Value);
                    if (checkRoom.Value == false)
                        throw new Exception(checkRoom.Note);
                }
                var checkTeacher = await ClassService.CheckTeacher(db, entity.Id, entity.TeacherId, entity.StartTime.Value, entity.EndTime.Value);
                if (checkTeacher.Value == false)
                    throw new Exception(checkTeacher.Note);

                var checkSchedule = await db.tbl_Schedule
                    .FirstOrDefaultAsync(x => x.Enable == true && x.StartTime < entity.EndTime && x.EndTime > entity.StartTime && x.Id != entity.Id && x.ClassId == entity.ClassId);
                if (checkSchedule != null)
                    throw new Exception($"Trùng lịch từ {checkSchedule.StartTime} đến {checkSchedule.EndTime}");

                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                await UpdateDayClass(db, entity.ClassId.Value);

                // Chuẩn bị nội dung mail
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == entity.ClassId);
                string contentTeacher = $"Lịch dạy lớp {_class?.Name} có thay đổi: Thời gian cũ ( " + oldStartTime + " - " + oldEndTime + " ), thời gian mới ( " + newStartTime + " - " + newEndTime + " ), vui lòng kiểm tra.";
                string contentStudent = $"Lịch dạy lớp {_class?.Name} có thay đổi: Thời gian cũ ( " + oldStartTime + " - " + oldEndTime + " ), thời gian mới ( " + newStartTime + " - " + newEndTime + " ), vui lòng kiểm tra.";
                string title = "Thay Đổi Lịch Học";
                var studentInClass = await db.tbl_StudentInClass.Where(x => x.ClassId == entity.ClassId && x.Enable == true).ToListAsync();
                ScheduleParam param = new ScheduleParam { TeacherIds = entity.TeacherId.ToString(), ClassId = _class.Id };
                string paramString = JsonConvert.SerializeObject(param);
                List<tbl_UserInformation> parents = new List<tbl_UserInformation>();
                foreach (var item in studentInClass)
                {
                    tbl_UserInformation student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                    parents.Add(student);
                }

                UrlNotificationModels urlNotification = new UrlNotificationModels();
                string url = "class=" + _class.Id + "&curriculum=" + _class.CurriculumId + "&branch=" + _class.BranchId + "&scoreBoardTemplateId=" + _class.ScoreboardTemplateId;
                string urlEmail = urlNotification.url + urlNotification.urlDetailClass + url;

                content = content.Replace("{TitleName}", "Phụ huynh / Học sinh");
                content = content.Replace("{ClassName}", _class?.Name);
                content = content.Replace("{OldTeacher}", oldTeacherName);
                content = content.Replace("{NewTeacher}", newTeacherName);
                content = content.Replace("{OldStartDate}", oldStartTime);
                content = content.Replace("{OldEndDate}", oldEndTime);
                content = content.Replace("{NewStartDate}", newStartTime);
                content = content.Replace("{NewEndDate}", newEndTime);
                content = content.Replace("{ProjectName}", projectName);
                content = content.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

                notificationContent = @"<div>" + content + @"</div>";

                contentToTeacher = contentToTeacher.Replace("{TitleName}", "Giáo Viên phụ trách lớp " + _class?.Name);
                contentToTeacher = contentToTeacher.Replace("{ClassName}", _class?.Name);
                contentToTeacher = contentToTeacher.Replace("{OldTeacher}", oldTeacherName);
                contentToTeacher = contentToTeacher.Replace("{NewTeacher}", newTeacherName);
                contentToTeacher = contentToTeacher.Replace("{OldStartDate}", oldStartTime);
                contentToTeacher = contentToTeacher.Replace("{OldEndDate}", oldEndTime);
                contentToTeacher = contentToTeacher.Replace("{NewStartDate}", newStartTime);
                contentToTeacher = contentToTeacher.Replace("{NewEndDate}", newEndTime);
                contentToTeacher = contentToTeacher.Replace("{ProjectName}", projectName);
                contentToTeacher = contentToTeacher.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

                notificationContentToTeacher = @"<div>" + contentToTeacher + @"</div>";

                //// Gửi giáo viên
                //if (newTeacher.UserInformationId != oldTeacher.UserInformationId)
                //{
                //    // Thông báo giáo viên cũ nếu đổi giáo viên
                //    Thread sendNotiToOldTeacher = new Thread(async () =>
                //    {
                //        await NotificationService.Send(new tbl_Notification
                //        {
                //            Title = title,
                //            Content = contentTeacher,
                //            ContentEmail = notificationContentToTeacher,
                //            UserId = oldTeacher.UserInformationId,
                //            Type = 2,
                //            ParamString = paramString,
                //            Category = 0,
                //            Url = urlNotification.urlDetailClass + url,
                //            AvailableId = _class.Id
                //        }, user);
                //    });
                //    sendNotiToOldTeacher.Start();

                //    // Thông báo giáo viên mới nếu đổi giáo viên
                //    Thread sendNotiToNewTeacher = new Thread(async () =>
                //    {
                //        await NotificationService.Send(new tbl_Notification
                //        {
                //            Title = title,
                //            Content = contentTeacher,
                //            ContentEmail = notificationContentToTeacher,
                //            UserId = newTeacher.UserInformationId,
                //            Type = 2,
                //            ParamString = paramString,
                //            Category = 0,
                //            Url = urlNotification.urlDetailClass + url,
                //            AvailableId = _class.Id
                //        }, user);
                //    });
                //    sendNotiToNewTeacher.Start();
                //}
                //else
                //{
                //    // Thông báo giáo viên cũ nếu không đổi giáo viên
                //    Thread sendNotiToOldTeacher = new Thread(async () =>
                //    {
                //        await NotificationService.Send(new tbl_Notification
                //        {
                //            Title = title,
                //            Content = contentTeacher,
                //            ContentEmail = notificationContentToTeacher,
                //            UserId = oldTeacher.UserInformationId,
                //            Type = 2,
                //            ParamString = paramString,
                //            Category = 0,
                //            Url = urlNotification.urlDetailClass + url,
                //            AvailableId = _class.Id
                //        }, user);
                //    });
                //    sendNotiToOldTeacher.Start();
                //}


                //// Gửi học sinh
                //Thread sendNotiToStudent = new Thread(async () =>
                //{
                //    if (studentInClass.Any())
                //    {
                //        foreach (var itemStudent in studentInClass)
                //        {
                //            await NotificationService.Send(new tbl_Notification
                //            {
                //                Title = title,
                //                Content = contentStudent,
                //                ContentEmail = notificationContent,
                //                UserId = itemStudent.StudentId,
                //                Type = 2,
                //                ParamString = paramString,
                //                Category = 0,
                //                Url = urlNotification.urlDetailClass + url,
                //                AvailableId = _class.Id
                //            }, user, true);
                //        }
                //    }
                //});
                //sendNotiToStudent.Start();

                //// Gửi phụ huynh
                //Thread sendNotiToParent = new Thread(async () =>
                //{
                //    if (studentInClass.Any())
                //    {
                //        foreach (var itemParent in parents)
                //        {
                //            if (itemParent.ParentId.HasValue)
                //            {
                //                await NotificationService.Send(new tbl_Notification
                //                {
                //                    Title = title,
                //                    Content = contentStudent,
                //                    ContentEmail = notificationContent,
                //                    UserId = itemParent.ParentId,
                //                    Type = 2,
                //                    ParamString = paramString,
                //                    Category = 0,
                //                    Url = urlNotification.urlDetailClass + url,
                //                    AvailableId = _class.Id
                //                }, user, true);
                //            }
                //        }
                //    }
                //});
                //sendNotiToParent.Start();
                return entity;
            }
        }

        public static async Task<tbl_Schedule> UpdateV2(ScheduleUpdateV2 itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);

                // Dữ liệu lịch học trước và sau khi thay đổi
                string oldStartTime = entity.StartTime.Value.ToString("dddd, dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));
                string newStartTime = itemModel.StartTime.Value.ToString("dddd, dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));
                string oldEndTime = entity.EndTime.Value.ToString("dddd, dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));
                string newEndTime = itemModel.EndTime.Value.ToString("dddd, dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN"));

                // Dữ liệu giáo viên trước và sau khi thay đổi
                var oldTeacher = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == entity.TeacherId);
                string oldTeacherName = oldTeacher.FullName;
                var newTeacher = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == itemModel.TeacherId);
                string newTeacherName = newTeacher.FullName;

                // Cấu hình gửi thông báo
                string content = "";
                string contentToTeacher = "";
                string notificationContent = "";
                string notificationContentToTeacher = "";
                string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
                var pathViews = Path.Combine(_hostingEnvironment.ContentRootPath, "Views");

                content = File.ReadAllText($"{pathViews}/Base/Mail/Class/UpdateSchedule.cshtml");
                contentToTeacher = File.ReadAllText($"{pathViews}/Base/Mail/Class/UpdateSchedule.cshtml");

                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (entity.TeacherAttendanceId != 0)
                    throw new Exception("Đã học không thể chỉnh sửa");
                if (entity.StatusTutoring == 2)
                    throw new Exception("Buổi học đã hủy, không thể cập nhật");
                if (entity.StatusTutoring == 4 || entity.StatusTutoring == 5)
                    throw new Exception("Không thể cập nhật buổi này");
                //if (entity.SalaryId.HasValue && entity.SalaryId != 0 && entity.TeachingFee != itemModel.TeachingFee && itemModel.TeachingFee.HasValue)
                //    throw new Exception("Buổi học này đã được tính lương, không thể cập nhật lương giảng dạy");
                entity.StatusTutoring = itemModel.StatusTutoring ?? entity.StatusTutoring;
                entity.StatusTutoringName = itemModel.StatusTutoringName ?? entity.StatusTutoringName;
                entity.RoomId = itemModel.RoomId ?? entity.RoomId;
                entity.StartTime = itemModel.StartTime ?? entity.StartTime;
                entity.EndTime = itemModel.EndTime ?? entity.EndTime;
                entity.TeacherId = itemModel.TeacherId ?? entity.TeacherId;
                entity.Note = itemModel.Note ?? entity.Note;
                // Logic mới thì ở đây sẽ không còn cập nhật lương nữa
                //entity.TeachingFee = itemModel.TeachingFee ?? entity.TeachingFee;
                entity.TutorFee = itemModel.TutorFee ?? entity.TutorFee;

                if (entity.RoomId != 0)
                {
                    var checkRoom = await ClassService.CheckRoom(db, entity.Id, entity.RoomId, entity.StartTime.Value, entity.EndTime.Value);
                    if (checkRoom.Value == false)
                        throw new Exception(checkRoom.Note);
                }
                var checkTeacher = await ClassService.CheckTeacher(db, entity.Id, entity.TeacherId, entity.StartTime.Value, entity.EndTime.Value);
                if (checkTeacher.Value == false)
                    throw new Exception(checkTeacher.Note);

                var checkSchedule = await db.tbl_Schedule
                    .FirstOrDefaultAsync(x => x.Enable == true && x.StartTime < entity.EndTime && x.EndTime > entity.StartTime && x.Id != entity.Id && x.ClassId == entity.ClassId);
                if (checkSchedule != null)
                    throw new Exception($"Trùng lịch từ {checkSchedule.StartTime} đến {checkSchedule.EndTime}");

                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                await UpdateDayClass(db, entity.ClassId.Value);

                // Chuẩn bị nội dung mail
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == entity.ClassId);
                string contentTeacher = $"Lịch dạy lớp {_class?.Name} có thay đổi: thời gian cũ ( " + oldStartTime + " - " + oldStartTime + " ), thời gian mới " + newStartTime + " - " + newEndTime + " ), vui lòng kiểm tra.";
                string contentStudent = $"Lịch dạy lớp {_class?.Name} có thay đổi: thời gian cũ ( " + oldStartTime + " - " + oldStartTime + " ), thời gian mới " + newStartTime + " - " + newEndTime + " ), vui lòng kiểm tra.";
                string title = "Thay Đổi Lịch Học";
                var studentInClass = await db.tbl_StudentInClass.Where(x => x.ClassId == entity.ClassId && x.Enable == true).ToListAsync();
                ScheduleParam param = new ScheduleParam { TeacherIds = entity.TeacherId.ToString(), ClassId = _class.Id };
                string paramString = JsonConvert.SerializeObject(param);
                List<tbl_UserInformation> parents = new List<tbl_UserInformation>();
                foreach (var item in studentInClass)
                {
                    tbl_UserInformation student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                    parents.Add(student);
                }

                UrlNotificationModels urlNotification = new UrlNotificationModels();
                string url = "class=" + _class.Id + "&curriculum=" + _class.CurriculumId + "&branch=" + _class.BranchId + "&scoreBoardTemplateId=" + _class.ScoreboardTemplateId;
                string urlEmail = urlNotification.url + urlNotification.urlDetailClass + url;

                content = content.Replace("{TitleName}", "Phụ huynh / Học sinh");
                content = content.Replace("{ClassName}", _class?.Name);
                content = content.Replace("{OldTeacher}", oldTeacherName);
                content = content.Replace("{NewTeacher}", newTeacherName);
                content = content.Replace("{OldStartDate}", oldStartTime);
                content = content.Replace("{OldEndDate}", oldEndTime);
                content = content.Replace("{NewStartDate}", newStartTime);
                content = content.Replace("{NewEndDate}", newEndTime);
                content = content.Replace("{ProjectName}", projectName);
                content = content.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

                notificationContent = @"<div>" + content + @"</div>";

                contentToTeacher = contentToTeacher.Replace("{TitleName}", "Giáo Viên phụ trách lớp " + _class?.Name);
                contentToTeacher = contentToTeacher.Replace("{ClassName}", _class?.Name);
                contentToTeacher = contentToTeacher.Replace("{OldTeacher}", oldTeacherName);
                contentToTeacher = contentToTeacher.Replace("{NewTeacher}", newTeacherName);
                contentToTeacher = contentToTeacher.Replace("{OldStartDate}", oldStartTime);
                contentToTeacher = contentToTeacher.Replace("{OldEndDate}", oldEndTime);
                contentToTeacher = contentToTeacher.Replace("{NewStartDate}", newStartTime);
                contentToTeacher = contentToTeacher.Replace("{NewEndDate}", newEndTime);
                contentToTeacher = contentToTeacher.Replace("{ProjectName}", projectName);
                contentToTeacher = contentToTeacher.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

                notificationContentToTeacher = @"<div>" + contentToTeacher + @"</div>";


                //// Gửi giáo viên
                //if (newTeacher.UserInformationId != oldTeacher.UserInformationId)
                //{
                //    // Thông báo giáo viên cũ nếu đổi giáo viên
                //    Thread sendNotiToOldTeacher = new Thread(async () =>
                //    {
                //        await NotificationService.Send(new tbl_Notification
                //        {
                //            Title = title,
                //            Content = contentTeacher,
                //            ContentEmail = notificationContentToTeacher,
                //            UserId = oldTeacher.UserInformationId,
                //            Type = 2,
                //            ParamString = paramString,
                //            Category = 0,
                //            Url = urlNotification.urlDetailClass + url,
                //            AvailableId = _class.Id
                //        }, user);
                //    });
                //    sendNotiToOldTeacher.Start();

                //    // Thông báo giáo viên mới nếu đổi giáo viên
                //    Thread sendNotiToNewTeacher = new Thread(async () =>
                //    {
                //        await NotificationService.Send(new tbl_Notification
                //        {
                //            Title = title,
                //            Content = contentTeacher,
                //            ContentEmail = notificationContentToTeacher,
                //            UserId = newTeacher.UserInformationId,
                //            Type = 2,
                //            ParamString = paramString,
                //            Category = 0,
                //            Url = urlNotification.urlDetailClass + url,
                //            AvailableId = _class.Id
                //        }, user);
                //    });
                //    sendNotiToNewTeacher.Start();
                //}
                //else
                //{
                //    // Thông báo giáo viên cũ nếu không đổi giáo viên
                //    Thread sendNotiToOldTeacher = new Thread(async () =>
                //    {
                //        await NotificationService.Send(new tbl_Notification
                //        {
                //            Title = title,
                //            Content = contentTeacher,
                //            ContentEmail = notificationContentToTeacher,
                //            UserId = oldTeacher.UserInformationId,
                //            Type = 2,
                //            ParamString = paramString,
                //            Category = 0,
                //            Url = urlNotification.urlDetailClass + url,
                //            AvailableId = _class.Id
                //        }, user);
                //    });
                //    sendNotiToOldTeacher.Start();
                //}


                //// Gửi học sinh
                //Thread sendNotiToStudent = new Thread(async () =>
                //{
                //    if (studentInClass.Any())
                //    {
                //        foreach (var itemStudent in studentInClass)
                //        {
                //            await NotificationService.Send(new tbl_Notification
                //            {
                //                Title = title,
                //                Content = contentStudent,
                //                ContentEmail = notificationContent,
                //                UserId = itemStudent.StudentId,
                //                Type = 2,
                //                ParamString = paramString,
                //                Category = 0,
                //                Url = urlNotification.urlDetailClass + url,
                //                AvailableId = _class.Id
                //            }, user, true);
                //        }
                //    }
                //});
                //sendNotiToStudent.Start();

                //// Gửi phụ huynh
                //Thread sendNotiToParent = new Thread(async () =>
                //{
                //    if (studentInClass.Any())
                //    {
                //        foreach (var itemParent in parents)
                //        {
                //            if (itemParent.ParentId.HasValue)
                //            {
                //                await NotificationService.Send(new tbl_Notification
                //                {
                //                    Title = title,
                //                    Content = contentStudent,
                //                    ContentEmail = notificationContent,
                //                    UserId = itemParent.ParentId,
                //                    Type = 2,
                //                    ParamString = paramString,
                //                    Category = 0,
                //                    Url = urlNotification.urlDetailClass + url,
                //                    AvailableId = _class.Id
                //                }, user, true);
                //            }
                //        }
                //    }
                //});
                //sendNotiToParent.Start();
                return entity;
            }
        }

        /// <summary>
        /// Hủy lịch
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <returns></returns>
        public static async Task<tbl_Schedule> TutoringCancel(int scheduleId, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == scheduleId);
                if (schedule == null)
                    throw new Exception("Không tìm thấy buổi học này");
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp học");
                if (_class.Type != 3)
                    throw new Exception("Chỉ có thể hủy lịch lớp dạy kèm");

                int cancelTime = 0;
                var cancelTutoring = await db.tbl_Config.FirstOrDefaultAsync(x => x.Code == "CancelTutoring");
                if (cancelTutoring != null)
                    cancelTime = int.Parse(cancelTutoring.Value);

                var timeNow = DateTime.Now.AddHours(-cancelTime);
                if (schedule.StartTime < timeNow)
                    throw new Exception($"Vui lòng hủy lịch trước {cancelTime} tiếng");

                schedule.StatusTutoring = 2;
                schedule.StatusTutoringName = "Hủy";
                schedule.ModifiedBy = userLog.FullName;
                schedule.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();

                string content = $"Buổi học vào lúc {schedule.StartTime.Value.ToString("dd/MM/yyyy HH:mm")} lớp {_class.Name} đã hủy " +
                    $"vui lòng kiểm tra lịch.";
                string title = "Hủy lịch";
                var studentInClass = await db.tbl_StudentInClass.Where(x => x.ClassId == schedule.ClassId && x.Enable == true).ToListAsync();
                ScheduleParam param = new ScheduleParam { TeacherIds = schedule.TeacherId.ToString(), ClassId = _class.Id };
                string paramString = JsonConvert.SerializeObject(param);
                //Thread sendNoti = new Thread(async () =>
                //{
                //    await NotificationService.Send(new tbl_Notification
                //    {
                //        Title = title,
                //        Content = content,
                //        ContentEmail = content,
                //        UserId = schedule.TeacherId,
                //        Type = 2,
                //        ParamString = paramString
                //    }, userLog);
                //    if (studentInClass.Any())
                //    {
                //        foreach (var item in studentInClass)
                //        {
                //            await NotificationService.Send(new tbl_Notification
                //            {
                //                Title = title,
                //                Content = content,
                //                ContentEmail = content,
                //                UserId = item.StudentId,
                //                Type = 2,
                //                ParamString = paramString
                //            }, userLog);
                //        }
                //    }
                //});
                //sendNoti.Start();
                return schedule;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy lịch học");
                if (entity.TeacherAttendanceId != 0)
                    throw new Exception("Đã học không thể xóa");
                entity.Enable = false;
                await db.SaveChangesAsync();
                await UpdateDayClass(db, entity.ClassId.Value);
            }
        }
        public static async Task<AppDomainResult> GetAll(ScheduleSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ScheduleSearch();
                if (user.RoleId == (int)RoleEnum.student)
                    baseSearch.StudentId = user.UserInformationId;
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;
                string sql = $"Get_Schedule @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = {baseSearch.ClassId}," +
                    $"@StudentId = {baseSearch.StudentId}," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds}'," +
                    $"@ParentId = N'{baseSearch.ParentId}'," +
                    $"@From = N'{(!baseSearch.From.HasValue ? "" : baseSearch.From.Value.ToString("yyyy-MM-dd HH:mm:ss"))}'," +
                    $"@To = N'{(!baseSearch.To.HasValue ? "" : baseSearch.To.Value.ToString("yyyy-MM-dd HH:mm:ss"))}'," +
                    $"@TeacherIds = N'{baseSearch.TeacherIds ?? ""}'";
                var data = await db.SqlQuery<Get_Schedule>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Schedule(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public class RateTeacherModel
        {
            [Required(ErrorMessage = "Vui lòng chọn buổi học")]
            public int ScheduleId { get; set; }
            /// <summary>
            /// Đánh giá từ 1 đến 5 sao
            /// </summary>
            public int? RateTeacher { get; set; }
            public string RateTeacherComment { get; set; }
        }
        public static async Task RateTeacher(RateTeacherModel itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == itemModel.ScheduleId);
                if (data == null)
                    throw new Exception("Không tìm thấy buổi học");
                data.RateTeacher = itemModel.RateTeacher ?? data.RateTeacher;
                data.RateTeacherComment = itemModel.RateTeacherComment ?? data.RateTeacherComment;
                data.ModifiedBy = user.FullName;
                data.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
            }
        }
        /*public static async Task<List<ScheduleCreates>> GenerateSchedule(GenerateScheduleCreate itemModel)
        {
            using (var db = new lmsDbContext())
            {
                if (!itemModel.TimeModels.Any())
                    throw new Exception("Không tìm thấy ngày học");
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.ClassId);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == _class.CurriculumId);
                if (curriculum == null)
                    throw new Exception("Không tìm thấy giáo trình");
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == _class.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                if (itemModel.TimeModels == null || itemModel.TimeModels.Count == 0)
                    throw new Exception("Vui lòng chọn ca học");
                var teacherIds = itemModel.TimeModels.Select(x => x.TeacherId).Distinct().ToList();
                var teachers = await db.tbl_UserInformation.Where(x => x.Enable == true && teacherIds.Contains(x.UserInformationId) && x.RoleId == (int)RoleEnum.teacher).ToListAsync();
                if (teachers == null || teachers.Count != teacherIds.Count())
                    throw new Exception("Giáo viên không tồn tại");

                //var allTutorIds = itemModel.TimeModels.Where(x => x.TutorIds != null && x.TutorIds.Count > 0).SelectMany(x => x.TutorIds).Distinct().ToList();
                //if (allTutorIds != null && allTutorIds.Count > 0)
                //{
                //    var tutors = await db.tbl_UserInformation.Where(x => allTutorIds.Contains(x.UserInformationId) && x.RoleId == (int)RoleEnum.tutor).ToListAsync();
                //    if (tutors == null || tutors.Count != allTutorIds.Count())
                //        throw new Exception("Trợ giảng không tồn tại");
                //}

                var teacherInProgram = await db.tbl_TeacherInProgram
                        .Where(x => x.ProgramId == curriculum.ProgramId && x.Enable == true).ToListAsync();

                foreach (var teacher in teachers)
                {
                    var branchIds = teacher.BranchIds.Split(',');
                    if (!branchIds.Any(x => x == branch.Id.ToString()))
                        throw new Exception("Giáo viên không thuộc trung tâm này");
                    var isInProgram = teacherInProgram.Any(x => x.TeacherId == teacher.UserInformationId);
                    if (!isInProgram)
                        throw new Exception($"Giáo viên {teacher.FullName} không được phép dạy chương trình này");
                }

                var room = new tbl_Room { Id = 0, Name = "" };
                if (itemModel.RoomId != 0)
                {
                    room = await db.tbl_Room.SingleOrDefaultAsync(x => x.Id == itemModel.RoomId);
                    if (room == null)
                        throw new Exception("Không tìm thấy phòng");
                }
                var result = new List<ScheduleCreates>();
                var date = itemModel.StartDay.Value.AddDays(-1).Date;
                do
                {
                    date = date.AddDays(1);
                    var dayOff = await db.tbl_DayOff
                        .AnyAsync(x => x.Enable == true && x.sDate <= date && x.eDate >= date);
                    if (dayOff)
                        continue;
                    foreach (var item in itemModel.TimeModels)
                    {
                        if (item.DayOfWeek == ((int)date.Date.DayOfWeek))
                        {
                            var studyTime = await db.tbl_StudyTime.SingleOrDefaultAsync(x => x.Id == item.StudyTimeId);
                            if (studyTime == null)
                                continue;

                            //var tutorIds = item.TutorIds;
                            //if (tutorIds == null) tutorIds = new List<int>();
                            //check xem giáo viên hoặc trợ giảng có xin nghỉ không
                            var teacherOff = await db.tbl_TeacherOff.Where(x => x.Enable == true && x.StartTime <= date && x.EndTime >= date && x.Status == 2 && x.TeacherId.HasValue).ToListAsync();
                            bool isTeacherOff = teacherOff.Any(x => x.TeacherId == item.TeacherId);
                            if (isTeacherOff)
                                continue;
                            //bool isTutorOff = teacherOff.Any(x => tutorIds.Contains(x.TeacherId.Value));
                            //if (isTutorOff)
                            //    continue;

                            var stimes = studyTime.StartTime.Split(':');
                            DateTime st = new DateTime(date.Year, date.Month, date.Day, int.Parse(stimes[0]), int.Parse(stimes[1]), 0);
                            var etimes = studyTime.EndTime.Split(':');
                            DateTime et = new DateTime(date.Year, date.Month, date.Day, int.Parse(etimes[0]), int.Parse(etimes[1]), 0);

                            var checkRoom = await db.tbl_Schedule
                                .AnyAsync(x => x.Enable == true && x.RoomId == itemModel.RoomId
                                    && x.StartTime < et && x.EndTime > st);

                            var schedules = await db.tbl_Schedule
                                .Where(x => x.Enable == true && x.StartTime < et && x.EndTime > st).ToListAsync();
                            bool checkTeacher = schedules.Any(x => x.TeacherId == item.TeacherId);
                            //foreach (var tutorId in tutorIds.ToList())
                            //{
                            //    if (schedules.Any(x => !string.IsNullOrEmpty(x.TutorIds) && x.TutorIds.Contains(tutorId.ToString())))
                            //        tutorIds.Remove(tutorId);
                            //}
                            var teacher = teachers.FirstOrDefault(x => x.UserInformationId == item.TeacherId);
                            result.Add(new ScheduleCreates
                            {
                                ClassId = _class.Id,
                                StartTime = st,
                                EndTime = et,
                                RoomId = checkRoom ? 0 : room.Id,
                                RoomName = checkRoom ? "" : room.Name,
                                RoomCode = checkRoom ? "" : room.Code,
                                TeacherId = checkTeacher ? 0 : teacher.UserInformationId,
                                TeacherName = checkTeacher ? "" : teacher.FullName,
                                TeacherCode = checkTeacher ? "" : teacher.UserCode,
                                //TutorIds = string.Join(",", tutorIds),
                                //TutorNames = Task.Run(() => UserInformation.GetTutorNames(tutorIds)).Result,
                                Note = ""
                            });

                        }
                    }
                } while (result.Count() < curriculum.Lesson);
                var schedule = await db.tbl_Schedule.Where(x => x.Enable == true && x.ClassId == _class.Id).ToListAsync();
                if (schedule.Any())
                {
                    foreach (var item in schedule)
                    {
                        var data = result.FirstOrDefault(x => x.StartTime == item.StartTime && x.EndTime == item.EndTime);
                        if (data != null)
                            result.Remove(data);
                    }
                }
                return result;
            }
        }*/

        #region Chức năng zoom
        /// Đến giờ dậy (trước mà sau thời gian ca học 15p) FE sẽ hiện nút Tạo phòng
        /// Nếu đã tạo rồi sẽ hiện nút Tạo lại, có IsRoomStart = true thì thêm tham gia và kết thúc
        public class ZoomReponse
        {
            public int ScheduleId { get; set; }
            public string ZoomId { get; set; }
            public string ZoomPass { get; set; }
            public string StartUrl { get; set; }
            public string JoinUrl { get; set; }
        }
        /// <summary>
        /// Nếu phòng đang hoạt động mà vẫn muốn tạo thì lấy tài khoản đang hoạt động tạo lại phòng mới
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="userLog"></param>
        /// <returns></returns>
        public static async Task<ZoomReponse> CreateZoom(int scheduleId, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var result = new ZoomReponse();
                        var schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == scheduleId);
                        if (schedule == null)
                            throw new Exception("Không tìm thấy buổi học");

                        var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                        if (_class == null)
                            throw new Exception("Không tìm thấy lớp học");

                        var zoomConfig = await db.tbl_ZoomConfig
                            .Where(x => x.Active == false && x.Enable == true)
                            .FirstOrDefaultAsync();

                        if (schedule.IsOpenZoom == true)
                        {
                            zoomConfig = await db.tbl_ZoomConfig.SingleOrDefaultAsync(x => x.Id == schedule.ZoomConfigId);
                        }

                        if (zoomConfig == null)
                            throw new Exception("Không có tài khoản Zoom nào đang trống, vui lòng thêm tài khoản");

                        var tokenOject = JObject.Parse(AccessTokenWithServerToServer(zoomConfig.AccountId, zoomConfig.ClientId, zoomConfig.ClientSecret));

                        string tokenString = tokenOject["access_token"].ToString();
                        var client = new RestClient("https://api.zoom.us/v2/users/me/meetings");
                        var request = new RestRequest(Method.POST);
                        request.RequestFormat = DataFormat.Json;

                        var dataBody = new
                        {
                            topic = _class.Name,
                            type = "1",
                            settings = new
                            {
                                waiting_room = false
                            }
                        };

                        request.AddJsonBody(dataBody);
                        request.AddHeader("Authorization", string.Format("Bearer {0}", tokenString));
                        request.AddHeader("Content-Type", "application/json");

                        IRestResponse restResponse = client.Execute(request);
                        HttpStatusCode statusCode = restResponse.StatusCode;
                        int numericStatusCode = (int)statusCode;
                        var jObject = JObject.Parse(restResponse.Content);


                        if (numericStatusCode == 201)
                        {
                            if (string.IsNullOrEmpty(jObject["id"].ToString()) || string.IsNullOrEmpty(jObject["encrypted_password"].ToString()))
                            {
                                throw new Exception("Tạo phòng không thành công");
                            }
                            else
                            {
                                result.ZoomId = jObject["id"].ToString();
                                result.ZoomPass = jObject["password"].ToString();
                                result.StartUrl = jObject["start_url"].ToString();
                                result.JoinUrl = jObject["join_url"].ToString();
                            }
                        }
                        else
                            throw new Exception("Tạo phòng không thành công");

                        zoomConfig.Active = true;
                        schedule.ZoomId = result.ZoomId;
                        schedule.ZoomPass = result.ZoomPass;
                        schedule.StartUrl = result.StartUrl;
                        schedule.JoinUrl = result.JoinUrl;
                        schedule.IsOpenZoom = true;
                        schedule.TeacherAttendanceId = schedule.TeacherId;
                        schedule.ModifiedBy = userLog.FullName;
                        schedule.ModifiedOn = DateTime.Now;
                        schedule.ZoomConfigId = zoomConfig.Id;
                        schedule.HostId = userLog.UserInformationId;
                        result.ScheduleId = schedule.Id;
                        await db.SaveChangesAsync();
                        tran.Commit();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
            }
        }
        /// <summary>
        /// AccessToken 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        public static string AccessTokenWithServerToServer(string acountId, string clientId, string clientSecret)
        {
            var client = new RestClient("https://zoom.us/oauth/token?grant_type=account_credentials&account_id=" + acountId);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            var authenticationString = $"{clientId.Trim()}:{clientSecret.Trim()}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
            request.AddHeader("Authorization", "Basic " + base64EncodedAuthenticationString);

            IRestResponse response = client.Execute(request);
            return response.Content;
        }
        /// <summary>
        /// Kết thúc phòng học
        /// </summary>
        /// <param name="courseScheduleId"></param>
        /// <param name="userLog"></param>
        /// <returns></returns>
        public static async Task CloseZoom(int scheduleId, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (userLog.RoleId == (int)RoleEnum.student || userLog.RoleId == (int)RoleEnum.parents)
                            throw new Exception("Bạn không thể đóng phòng học");

                        var schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == scheduleId);
                        if (schedule == null)
                            throw new Exception("Không tìm thấy phòng học");
                        if (schedule.IsOpenZoom == false)
                            throw new Exception("Phòng học này đã đóng");

                        if (!string.IsNullOrEmpty(schedule.ZoomId))
                        {
                            var zoomConfig = await db.tbl_ZoomConfig.SingleOrDefaultAsync(x => x.Id == schedule.ZoomConfigId);
                            if (zoomConfig != null)
                            {
                                zoomConfig.Active = false;
                                await db.SaveChangesAsync();
                            }

                            var tokenOject = JObject.Parse(AccessTokenWithServerToServer(zoomConfig.AccountId, zoomConfig.ClientId, zoomConfig.ClientSecret));
                            string tokenString = tokenOject["access_token"].ToString();

                            var client = new RestClient($"https://api.zoom.us/v2/meetings/{schedule.ZoomId}/status");
                            var request = new RestRequest(Method.PUT);
                            request.AddHeader("authorization", string.Format("Bearer {0}", tokenString));
                            request.AddParameter("application/json", "{\"action\":\"end\"}", ParameterType.RequestBody);
                            IRestResponse response = client.Execute(request);
                            HttpStatusCode statusCode = response.StatusCode;
                            int numericStatusCode = (int)statusCode;

                        }

                        schedule.IsOpenZoom = false;
                        schedule.ModifiedBy = userLog.FullName;
                        schedule.ModifiedOn = DateTime.Now;

                        await db.SaveChangesAsync();

                        //Nếu chưa điểm danh hệ thống sẽ Tự động đánh giá vắng không phép
                        var studentInClass = await db.tbl_StudentInClass.Where(x => x.ClassId == schedule.ClassId && x.Enable == true)
                            .Select(x => x.StudentId).ToListAsync();
                        if (studentInClass.Any())
                        {
                            foreach (var item in studentInClass)
                            {
                                var checkRollUp = await db.tbl_RollUp
                                    .AnyAsync(x => x.ClassId == schedule.ClassId && x.ScheduleId == schedule.Id && x.StudentId == item && x.Enable == true);
                                if (!checkRollUp)
                                {
                                    db.tbl_RollUp.Add(new tbl_RollUp
                                    {
                                        ClassId = schedule.ClassId,
                                        CreatedBy = userLog.FullName,
                                        CreatedOn = DateTime.Now,
                                        Enable = true,
                                        LearningStatus = 8,
                                        LearningStatusName = "Không nhận xét",
                                        ModifiedBy = userLog.FullName,
                                        ModifiedOn = DateTime.Now,
                                        Note = "",
                                        ScheduleId = schedule.Id,
                                        Status = 3,
                                        StatusName = "Vắng không phép",
                                        StudentId = item
                                    });
                                    await db.SaveChangesAsync();
                                }
                            }
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
            }
        }

        public class recording_files
        {
            public string id { get; set; }
            public string meeting_id { get; set; }
            public string download_url { get; set; }
            public string file_type { get; set; }
            public string play_url { get; set; }
            public string recording_start { get; set; }
            public string recording_end { get; set; }
            public string file_size { get; set; }
            public string file_extension { get; set; }
            public string recording_type { get; set; }
        }
        public static async Task<List<recording_files>> GetRecording(int scheduleId)
        {
            try
            {

                using (var db = new lmsDbContext())
                {
                    var schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == scheduleId);
                    if (schedule == null) throw new Exception("Không tìm thấy buổi học");

                    var zoomConfig = await db.tbl_ZoomConfig.SingleOrDefaultAsync(x => x.Id == schedule.ZoomConfigId);
                    if (zoomConfig == null) throw new Exception("Không tìm thấy tài khoản zoom");

                    List<recording_files> rFile = new List<recording_files>();
                    var tokenOject = JObject.Parse(AccessTokenWithServerToServer(zoomConfig.AccountId, zoomConfig.ClientId, zoomConfig.ClientSecret));
                    string tokenString = tokenOject["access_token"].ToString();
                    var client = new RestClient($"https://api.zoom.us/v2/meetings/{schedule.ZoomId}/recordings");
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("authorization", string.Format("Bearer {0}", tokenString));
                    IRestResponse response = await client.ExecuteAsync(request);
                    HttpStatusCode statusCode = response.StatusCode;
                    int numericStatusCode = (int)statusCode;

                    //chỗ này hứng data nè
                    if (numericStatusCode == 200)
                    {
                        var jObject = JObject.Parse(response.Content);
                        if (jObject != null)
                        {
                            rFile = JsonConvert.DeserializeObject<List<recording_files>>(jObject["recording_files"].ToString());
                        }
                    }
                    if (rFile.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return rFile;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public static async Task AutoCloseZoom()
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var timeEnd = DateTime.Now.AddMinutes(-15);
                    var schedules = await db.tbl_Schedule
                        .Where(x => x.Enable == true && x.IsOpenZoom == true && x.EndTime <= timeEnd)
                        .ToListAsync();
                    if (schedules.Any())
                        foreach (var item in schedules)
                        {
                            ///Tắt mấy cái phòng còn chạy khi quá giờ 15p
                            await CloseZoom(item.Id, new tbl_UserInformation { RoleId = (int)RoleEnum.admin, FullName = "Tự động" });
                        }
                }
                catch (Exception e)
                {
                    AssetCRM.Writelog("Schedule", "AutoCloseZoom", 1, e.Message + e.InnerException);
                }
            }
        }
        public class Get_ZoomRoom
        {
            public int Id { get; set; }
            public int? ClassId { get; set; }
            public string ClassName { get; set; }
            public int? BranchId { get; set; }
            public string BranchName { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string ZoomId { get; set; }
            public string ZoomPass { get; set; }
            public int? ZoomConfigId { get; set; }
            public bool IsOpenZoom { get; set; }
            public string StartUrl { get; set; }
            public string JoinUrl { get; set; }
            public int? TeacherId { get; set; }
            public string TeacherName { get; set; }
            public int TotalRow { get; set; }

        }
        public static async Task<AppDomainResult> GetZoomRoom(GetZoomSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new GetZoomSearch();

                string sql = $"Get_ZoomRoom @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Search = N'{baseSearch.Search ?? ""}'," +
                    $"@ClassId = {baseSearch.ClassId ?? 0}," +
                    $"@BranchId = {baseSearch.BranchId ?? 0}," +
                    $"@TeacherId = {baseSearch.TeacherId ?? 0}";

                var data = await db.SqlQuery<Get_ZoomRoom>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new
                {
                    i.Id,
                    i.ClassId,
                    i.ClassName,
                    i.BranchId,
                    i.BranchName,
                    i.StartTime,
                    i.EndTime,
                    i.ZoomId,
                    i.ZoomPass,
                    i.ZoomConfigId,
                    i.IsOpenZoom,
                    i.StartUrl,
                    i.JoinUrl,
                    i.TeacherId,
                    i.TeacherName
                }).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        #endregion
        public static async Task<CheckModel> CheckRoom(lmsDbContext db, int scheduleId, int roomId, DateTime stime, DateTime etime)
        {
            var schedule = await db.tbl_Schedule
                .FirstOrDefaultAsync(x => x.Enable == true && x.RoomId == roomId
                && x.Id != scheduleId && x.StartTime < etime && x.EndTime > stime
                    && x.StatusTutoring != 2  //Lớp dạy kèm bỏ qua buổi đã hủy
                    && x.StatusTutoring != 4
                    && x.StatusTutoring != 5
                    );
            var room = await db.tbl_Room.SingleOrDefaultAsync(x => x.Id == roomId);
            if (schedule != null)
            {
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                return new CheckModel
                {
                    Value = false,
                    Note = $"Phòng học trùng lịch với lớp {_class?.Name} "
                };
            }
            return new CheckModel
            {
                Value = true,
                Note = ""
            };
        }
        public static async Task<CheckModel> CheckTeacher(lmsDbContext db, int scheduleId, int teacherId, DateTime stime, DateTime etime)
        {
            var schedule = await db.tbl_Schedule
                .FirstOrDefaultAsync(x => x.Enable == true && x.TeacherId == teacherId
                && x.Id != scheduleId && x.StartTime < etime && x.EndTime > stime
                    && x.StatusTutoring != 2 //Lớp dạy kèm bỏ qua buổi đã hủy
                    && x.StatusTutoring != 4
                    && x.StatusTutoring != 5
                    );
            var teacher = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == teacherId);
            if (schedule != null)
            {
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                if (_class != null && _class.Type == 3)// lớp dạy kèm kiểm tra giáo viên mở lịch
                {
                    var checkScheduleAvailable = await db.tbl_ScheduleAvailable
                        .AnyAsync(x => x.StartTime <= stime && x.EndTime >= etime && x.Enable == true);
                    if (!checkScheduleAvailable)
                        return new CheckModel
                        {
                            Value = false,
                            Note = "Giáo viên không mở lịch thời gian này "
                        };
                }
                return new CheckModel
                {
                    Value = false,
                    Note = $"Giáo viên trùng lịch với lớp {_class?.Name} "
                };
            }
            return new CheckModel
            {
                Value = true,
                Note = ""
            };
        }

        public static async Task<GenerateScheduleResponse> GenerateSchedule(GenerateScheduleCreate itemModel)
        {
            using (var db = new lmsDbContext())
            {
                var result = new GenerateScheduleResponse();
                var datas = new List<ScheduleCreatesV2>();
                if (!itemModel.TimeModels.Any())
                    throw new Exception("Không tìm thấy ngày học");
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.ClassId);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");
                var classSchedules = await db.tbl_Schedule.Where(x => x.Enable == true && x.ClassId == _class.Id).ToListAsync();
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == _class.CurriculumId);
                if (curriculum == null)
                    throw new Exception("Không tìm thấy giáo trình");
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == _class.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                if (itemModel.TimeModels == null || itemModel.TimeModels.Count == 0)
                    throw new Exception("Vui lòng chọn ca học");
                var teacherIds = itemModel.TimeModels.Select(x => x.TeacherId).Distinct().ToList();
                var teachers = await db.tbl_UserInformation.Where(x => x.Enable == true && teacherIds.Contains(x.UserInformationId) && x.RoleId == (int)RoleEnum.teacher).ToListAsync();
                if (teachers == null || teachers.Count != teacherIds.Count())
                    throw new Exception("Giáo viên không tồn tại");

                var teacherInProgram = await db.tbl_TeacherInProgram
                        .Where(x => x.ProgramId == curriculum.ProgramId && x.Enable == true).ToListAsync();

                foreach (var teacher in teachers)
                {
                    var branchIds = teacher.BranchIds.Split(',');
                    if (!branchIds.Any(x => x == branch.Id.ToString()))
                        throw new Exception("Giáo viên không thuộc trung tâm này");
                    var isInProgram = teacherInProgram.Any(x => x.TeacherId == teacher.UserInformationId);
                    if (!isInProgram)
                        throw new Exception($"Giáo viên {teacher.FullName} không được phép dạy chương trình này");
                }

                var room = new tbl_Room { Id = 0, Name = "" };
                if (itemModel.RoomId != 0)
                {
                    room = await db.tbl_Room.SingleOrDefaultAsync(x => x.Id == itemModel.RoomId);
                    if (room == null)
                        throw new Exception("Không tìm thấy phòng");
                }
                var date = itemModel.StartDay.Value.AddDays(-1).Date;
                do
                {
                    date = date.AddDays(1);
                    var dayOff = await db.tbl_DayOff
                        .AnyAsync(x => x.Enable == true && x.sDate <= date && x.eDate >= date);
                    if (dayOff)
                        continue;
                    foreach (var item in itemModel.TimeModels)
                    {
                        if (item.DayOfWeek == (int)date.Date.DayOfWeek)
                        {
                            var studyTime = await db.tbl_StudyTime.SingleOrDefaultAsync(x => x.Id == item.StudyTimeId);
                            if (studyTime == null)
                                continue;

                            //check xem giáo viên hoặc trợ giảng có xin nghỉ không
                            var teacherOff = await db.tbl_TeacherOff.Where(x => x.Enable == true && x.StartTime <= date && x.EndTime >= date && x.Status == 2 && x.TeacherId.HasValue).ToListAsync();
                            bool isTeacherOff = teacherOff.Any(x => x.TeacherId == item.TeacherId);
                            if (isTeacherOff)
                                continue;

                            /*var stimes = studyTime.StartTime.Split(':');
                            DateTime st = new DateTime(date.Year, date.Month, date.Day, int.Parse(stimes[0]), int.Parse(stimes[1]), 0);
                            var etimes = studyTime.EndTime.Split(':');
                            DateTime et = new DateTime(date.Year, date.Month, date.Day, int.Parse(etimes[0]), int.Parse(etimes[1]), 0);

                            var checkRoom = await db.tbl_Schedule
                                .AnyAsync(x => x.Enable == true && x.RoomId == itemModel.RoomId
                                    && x.StartTime < et && x.EndTime > st);

                            var schedules = await db.tbl_Schedule
                                .Where(x => x.Enable == true && x.StartTime < et && x.EndTime > st).ToListAsync();
                            bool checkTeacher = schedules.Any(x => x.TeacherId == item.TeacherId);

                            var teacher = teachers.FirstOrDefault(x => x.UserInformationId == item.TeacherId);
                            result.Add(new ScheduleCreatesV2
                            {
                                StartTime = st,
                                EndTime = et,
                                RoomId = checkRoom ? 0 : room.Id,
                                RoomName = checkRoom ? "" : room.Name,
                                RoomCode = checkRoom ? "" : room.Code,
                                TeacherId = checkTeacher ? 0 : teacher.UserInformationId,
                                TeacherName = checkTeacher ? "" : teacher.FullName,
                                TeacherCode = checkTeacher ? "" : teacher.UserCode,
                                Note = "",
                                Status = 1
                            });*/

                            var stimes = studyTime.StartTime.Split(':');
                            DateTime st = new DateTime(date.Year, date.Month, date.Day, int.Parse(stimes[0]), int.Parse(stimes[1]), 0);
                            var etimes = studyTime.EndTime.Split(':');
                            DateTime et = new DateTime(date.Year, date.Month, date.Day, int.Parse(etimes[0]), int.Parse(etimes[1]), 0);

                            var teacher = teachers.FirstOrDefault(x => x.UserInformationId == item.TeacherId);
                            datas.Add(new ScheduleCreatesV2
                            {
                                ClassId = _class.Id,
                                StartTime = st,
                                EndTime = et,
                                RoomId = room.Id,
                                RoomName = room.Name,
                                RoomCode = room.Code,
                                TeacherId = teacher.UserInformationId,
                                TeacherName = teacher.FullName,
                                TeacherCode = teacher.UserCode,
                                Note = "",
                                Types = "1"
                            });
                        }
                        if (classSchedules.Any())
                        {
                            foreach (var schedule in classSchedules)
                            {
                                var data = datas.FirstOrDefault(x => x.StartTime == schedule.StartTime && x.EndTime == schedule.EndTime);
                                if (data != null)
                                    datas.Remove(data);
                            }
                        }
                    }
                } while (classSchedules.Count + datas.Count < curriculum.Lesson);

                if (datas.Any())
                {
                    DateTime endDay = itemModel.StartDay.Value;
                    foreach (var item in datas)
                    {
                        /*if(item.TeacherId.Value == 0)
                        {
                            item.Status = 2;
                            item.Note = "Chưa chọn giảng viên ";
                        }

                        //lớp offline phải có phòng học
                        if (_class.Type == 1 && item.RoomId.Value == 0)
                        {
                            item.Status = 2;
                            item.Note = "Chưa chọn phòng học ";
                        }*/

                        var checkRoom = await CheckRoom(db, 0, item.RoomId.Value, item.StartTime, item.EndTime);
                        if (!checkRoom.Value && item.RoomId.Value != 0)
                        {
                            if (item.Types == "1")
                                item.Types = "";
                            item.Types = item.Types + "2,";
                            //item.Note = item.Note + checkRoom.Note + "\n";
                        }

                        var checkTeacher = await CheckTeacher(db, 0, item.TeacherId.Value, item.StartTime, item.EndTime);
                        if (!checkTeacher.Value)
                        {
                            if (item.Types == "1")
                                item.Types = "";
                            item.Types = item.Types + "3,";
                            //item.Note = item.Note + checkTeacher.Note + "\n";
                        }

                        /*var itemTeacher = await UserInformation.GetById(item.TeacherId.Value);
                        if (itemTeacher == null)
                        {
                            item.Status = 2;
                            item.Note = "Không tìm thấy thông tin giáo viên ";
                        }*/
                    }
                }
                result.TotalCurriculumLesson = curriculum.Lesson ?? 0;
                result.TotalClassSchedule = classSchedules.Count;
                result.Datas = datas;
                return result;
            }
        }
        public static async Task DeleteByClass(int classId)
        {
            using (var db = new lmsDbContext())
            {
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == classId);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");

                var schedules = await db.tbl_Schedule.Where(x => x.ClassId == _class.Id).ToListAsync();
                if (schedules.Count > 0)
                {
                    foreach (var schedule in schedules)
                    {
                        //đã học rồi thì bỏ qua
                        if (schedule.TeacherAttendanceId != 0)
                            continue;
                        schedule.Enable = false;
                    }
                }
                await db.SaveChangesAsync();
                await ScheduleService.UpdateDayClass(db, classId);
            }
        }
        /// <summary>
        /// Lấy tất cả các lịch dạy của giáo viên
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public static async Task<AppDomainResult> GetAllScheduleExpected(ExpectedSheduleSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ExpectedSheduleSearch();
                if (!baseSearch.FromDate.HasValue)
                    baseSearch.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                if (!baseSearch.ToDate.HasValue)
                {
                    var lastDayOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                    baseSearch.ToDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, lastDayOfMonth);
                }
                string sql = $"Get_Schedule_Expected @Search = N'{baseSearch.Search ?? ""}', " +
                     $"@PageIndex = {baseSearch.PageIndex}, " +
                     $"@PageSize = {baseSearch.PageSize}, " +
                     $"@FromDate = N'{baseSearch.FromDate?.ToString("yyyy/MM/dd") ?? ""}', " +
                     $"@ToDate = N'{baseSearch.ToDate?.ToString("yyyy/MM/dd") ?? ""}', " +
                     $"@TeacherId = N'{baseSearch.TeacherId}', " +
                     $"@IsOnlyAtendence = {baseSearch.IsOnlyAtendence}";
                var data = await db.SqlQuery<Get_Schedule_Expected>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                return new AppDomainResult { TotalRow = totalRow, IsActive = baseSearch.IsOnlyAtendence, Data = data };
            }
        }

        public static async Task<TeacherAvailableScheduleModel> GetAvailableTeachersFromStudyTime(TeacherAvailableScheduleSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new TeacherAvailableScheduleSearch();
                if (baseSearch.FromDate == null)
                    baseSearch.FromDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
                if (baseSearch.ToDate == null)
                    baseSearch.ToDate = baseSearch.FromDate.Value.AddDays(6);
                var daysList = new List<DateTime>();
                // Lấy các ngày từ FromDate đến ToDate
                for (var date = baseSearch.FromDate.Value; date <= baseSearch.ToDate.Value; date = date.AddDays(1))
                {
                    daysList.Add(date);
                }
                var studyTimeData = await db.tbl_StudyTime
                    .Where(x => x.Enable == true)
                    .OrderBy(x => x.Name)
                    .Select(x => new { x.Id, x.Name, x.StartTime, x.EndTime }).ToListAsync();
                //var scheduleData = await db.tbl_Schedule.Where(x => x.Enable == true && (baseSearch.BranchId == 0 || x.BranchId == baseSearch.BranchId))
                //    .Select(x => new { x.Id, x.TeacherId, x.StartTime, x.EndTime }).ToListAsync();
                var scheduleData = await (from s in db.tbl_Schedule
                                          join c in db.tbl_Class on s.ClassId equals c.Id
                                          where s.Enable == true && c.Enable == true
                                                && (baseSearch.BranchId == 0 || s.BranchId == baseSearch.BranchId)
                                                && (c == null || c.Status != 3)
                                          select new
                                          {
                                              s.Id,
                                              s.TeacherId,
                                              s.StartTime,
                                              s.EndTime,
                                          }).ToListAsync();
                var teacherData = await db.tbl_UserInformation
                    .Where(x => x.Enable == true
                    && !string.IsNullOrEmpty(x.BranchIds)
                    && (baseSearch.BranchId == 0 || x.BranchIds.Contains(baseSearch.BranchId.ToString()))
                    && x.RoleId == (int)RoleEnum.teacher
                    && (string.IsNullOrEmpty(baseSearch.TeacherIds) || baseSearch.TeacherIds.Contains(x.UserInformationId.ToString())))
                    .Select(x => new { x.UserInformationId, x.FullName, x.Avatar, x.UserCode })
                    .ToListAsync();

                var result = new TeacherAvailableScheduleModel();
                var studyTimeModels = new List<StudyTimeModel>();
                if (teacherData.Count != 0)
                {
                    foreach (var st in studyTimeData)
                    {
                        var studyTimeModel = new StudyTimeModel();
                        var dayOfWeekModels = new List<DayOfWeekModel>();
                        foreach (var dl in daysList)
                        {
                            var dayOfWeek = new DayOfWeekModel();
                            var teacherInforModels = new List<TeacherInforModel>();
                            var date = dl.ToString("dd/MM/yyyy");
                            var startDateTime = DateTime.ParseExact($"{date} {st.StartTime}", "dd/MM/yyyy HH:mm", null);
                            var endDateTime = DateTime.ParseExact($"{date} {st.EndTime}", "dd/MM/yyyy HH:mm", null);
                            foreach (var t in teacherData)
                            {
                                var teacherSchedule = scheduleData
                                    .Any(x => x.TeacherId == t.UserInformationId
                                    && x.StartTime.Value.ToString("dd/MM/yyyy HH:mm") == startDateTime.ToString("dd/MM/yyyy HH:mm")
                                    && x.EndTime.Value.ToString("dd/MM/yyyy HH:mm") == endDateTime.ToString("dd/MM/yyyy HH:mm"));
                                if (!teacherSchedule)
                                {
                                    var teacher = new TeacherInforModel
                                    {
                                        TeacherId = t.UserInformationId,
                                        TeacherName = t.FullName,
                                        TeacheCode = t.UserCode,
                                        TeacherAvatar = t.Avatar
                                    };
                                    teacherInforModels.Add(teacher);
                                }
                            }
                            dayOfWeek.DateTime = date;
                            // Mặc định danh sách sẽ chỉ chứa 5 giáo viên mà thôi
                            if (teacherInforModels.Count > 5)
                            {
                                dayOfWeek.SeeMore = true;
                                // Còn lại bao nhiêu giáo viên chưa show ra
                                dayOfWeek.Remains = teacherInforModels.Count - 5;
                            }
                            else
                            {
                                dayOfWeek.SeeMore = false;
                                dayOfWeek.Remains = 0;
                            }
                            dayOfWeek.Teachers = teacherInforModels.Take(5).ToList();
                            dayOfWeekModels.Add(dayOfWeek);
                        }
                        studyTimeModel.StudyTimeId = st.Id;
                        studyTimeModel.StudyTimeName = st.Name;
                        studyTimeModel.DayOfWeek = dayOfWeekModels;
                        studyTimeModels.Add(studyTimeModel);
                    }
                    result.FromDate = baseSearch.FromDate.Value.ToString("dd/MM/yyyy");
                    result.ToDate = baseSearch.ToDate.Value.ToString("dd/MM/yyyy");
                    result.StudyTimes = studyTimeModels;
                }
                return result;
            }
        }

        public static async Task<AppDomainResult> GetDetailAvailableTeachersFromStudyTime(DetailTeacherAvailableScheduleSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new DetailTeacherAvailableScheduleSearch();
                var studyTimeData = await db.tbl_StudyTime
                    .Where(x => x.Enable == true && x.Id == baseSearch.StudyTimeId)
                    .Select(x => new { x.Id, x.Name, x.StartTime, x.EndTime })
                    .SingleOrDefaultAsync();
                if (studyTimeData == null)
                    throw new Exception("Không tìm thấy thông tin ca học");
                //var scheduleData = await db.tbl_Schedule.Where(x => x.Enable == true && (baseSearch.BranchId == 0 || x.BranchId == baseSearch.BranchId))
                //    .Select(x => new { x.Id, x.TeacherId, x.StartTime, x.EndTime }).ToListAsync();
                var scheduleData = await (from s in db.tbl_Schedule
                                          join c in db.tbl_Class on s.ClassId equals c.Id
                                          where s.Enable == true && c.Enable == true
                                                && (baseSearch.BranchId == 0 || s.BranchId == baseSearch.BranchId)
                                                && (c == null || c.Status != 3)
                                          select new
                                          {
                                              s.Id,
                                              s.TeacherId,
                                              s.StartTime,
                                              s.EndTime,
                                          }).ToListAsync();
                var teacherData = await db.tbl_UserInformation
                    .Where(x => x.Enable == true
                        && !string.IsNullOrEmpty(x.BranchIds)
                        && (baseSearch.BranchId == 0 || x.BranchIds.Contains(baseSearch.BranchId.ToString()))
                        && x.RoleId == (int)RoleEnum.teacher
                        && (string.IsNullOrEmpty(baseSearch.TeacherIds) || baseSearch.TeacherIds.Contains(x.UserInformationId.ToString())))
                    .Select(x => new { x.UserInformationId, x.FullName, x.Avatar, x.UserCode })
                    .ToListAsync();
                int totalRow = 0;
                var result = new List<TeacherInforModel>();
                if (teacherData.Count != 0)
                {
                    var dayOfWeek = new DayOfWeekModel();
                    var date = baseSearch.DateTime.ToString("dd/MM/yyyy");
                    var startDateTime = DateTime.ParseExact($"{date} {studyTimeData.StartTime}", "dd/MM/yyyy HH:mm", null);
                    var endDateTime = DateTime.ParseExact($"{date} {studyTimeData.EndTime}", "dd/MM/yyyy HH:mm", null);
                    foreach (var t in teacherData)
                    {
                        var teacherSchedule = scheduleData
                            .Any(x => x.TeacherId == t.UserInformationId
                            && x.StartTime.Value.ToString("dd/MM/yyyy HH:mm") == startDateTime.ToString("dd/MM/yyyy HH:mm")
                            && x.EndTime.Value.ToString("dd/MM/yyyy HH:mm") == endDateTime.ToString("dd/MM/yyyy HH:mm"));
                        if (!teacherSchedule)
                        {
                            var teacher = new TeacherInforModel
                            {
                                TeacherId = t.UserInformationId,
                                TeacherName = t.FullName,
                                TeacheCode = t.UserCode,
                                TeacherAvatar = t.Avatar
                            };
                            result.Add(teacher);
                        }
                    }
                    totalRow = result.Count;
                    result = result.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                }
                else return new AppDomainResult() { Data = null, TotalRow = 0 };
                return new AppDomainResult() { Data = result, TotalRow = totalRow };
            }
        }

        public static async Task<RoomAvailableScheduleModel> GetAvailableRoomFromStudyTime(RoomAvailableScheduleSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new RoomAvailableScheduleSearch();
                if (baseSearch.FromDate == null)
                    baseSearch.FromDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
                if (baseSearch.ToDate == null)
                    baseSearch.ToDate = baseSearch.FromDate.Value.AddDays(6);
                var daysList = new List<DateTime>();
                for (var date = baseSearch.FromDate.Value; date <= baseSearch.ToDate.Value; date = date.AddDays(1))
                {
                    daysList.Add(date);
                }
                var studyTimeData = await db.tbl_StudyTime
                    .Where(x => x.Enable == true)
                    .OrderBy(x => x.Name)
                    .Select(x => new { x.Id, x.Name, x.StartTime, x.EndTime }).ToListAsync();
                //var scheduleData = await db.tbl_Schedule.Where(x => x.Enable == true && (baseSearch.BranchId == 0 || x.BranchId == baseSearch.BranchId))
                //    .Select(x => new { x.Id, x.RoomId, x.StartTime, x.EndTime }).ToListAsync();
                var scheduleData = await (from s in db.tbl_Schedule
                                          join c in db.tbl_Class on s.ClassId equals c.Id
                                          where s.Enable == true && c.Enable == true
                                                && (baseSearch.BranchId == 0 || s.BranchId == baseSearch.BranchId)
                                                && (c == null || c.Status != 3)
                                          select new
                                          {
                                              s.Id,
                                              s.RoomId,
                                              s.StartTime,
                                              s.EndTime,
                                          }).ToListAsync();

                var roomData = await db.tbl_Room
                    .Where(x => x.Enable == true
                        && (baseSearch.BranchId == 0 || x.BranchId == baseSearch.BranchId)
                        && (string.IsNullOrEmpty(baseSearch.RoomIds) || baseSearch.RoomIds.Contains(x.Id.ToString())))
                    .Select(x => new { x.Id, x.Name, x.Code })
                    .ToListAsync();
                var result = new RoomAvailableScheduleModel();
                var studyTimeModels = new List<RoomOfStudyTimeModel>();
                if (roomData.Count != 0)
                {
                    foreach (var st in studyTimeData)
                    {
                        var studyTimeModel = new RoomOfStudyTimeModel();
                        var dayOfWeekModels = new List<RoomOfDayOfWeekModel>();
                        foreach (var dl in daysList)
                        {
                            var dayOfWeek = new RoomOfDayOfWeekModel();
                            var roomModels = new List<RoomModel>();
                            var date = dl.ToString("dd/MM/yyyy");
                            var startDateTime = DateTime.ParseExact($"{date} {st.StartTime}", "dd/MM/yyyy HH:mm", null);
                            var endDateTime = DateTime.ParseExact($"{date} {st.EndTime}", "dd/MM/yyyy HH:mm", null);
                            foreach (var r in roomData)
                            {
                                var roomSchedule = scheduleData
                                    .Any(x => x.RoomId == r.Id
                                    && x.StartTime.Value.ToString("dd/MM/yyyy HH:mm") == startDateTime.ToString("dd/MM/yyyy HH:mm")
                                    && x.EndTime.Value.ToString("dd/MM/yyyy HH:mm") == endDateTime.ToString("dd/MM/yyyy HH:mm"));
                                if (!roomSchedule)
                                {
                                    var room = new RoomModel
                                    {
                                        RoomId = r.Id,
                                        RoomCode = r.Code,
                                        RoomName = r.Name
                                    };
                                    roomModels.Add(room);
                                }
                            }
                            dayOfWeek.DateTime = date;
                            if (roomModels.Count > 5)
                            {
                                dayOfWeek.SeeMore = true;
                                dayOfWeek.Remains = roomModels.Count - 5;
                            }
                            else
                            {
                                dayOfWeek.SeeMore = false;
                                dayOfWeek.Remains = 0;
                            }
                            dayOfWeek.Teachers = roomModels.Take(5).ToList();
                            dayOfWeekModels.Add(dayOfWeek);
                        }
                        studyTimeModel.StudyTimeId = st.Id;
                        studyTimeModel.StudyTimeName = st.Name;
                        studyTimeModel.DayOfWeek = dayOfWeekModels;
                        studyTimeModels.Add(studyTimeModel);
                    }
                    result.FromDate = baseSearch.FromDate.Value.ToString("dd/MM/yyyy");
                    result.ToDate = baseSearch.ToDate.Value.ToString("dd/MM/yyyy");
                    result.StudyTimes = studyTimeModels;
                }
                return result;
            }
        }

        public static async Task<AppDomainResult> GetDetailAvailableRoomFromStudyTime(DetailRoomAvailableScheduleSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new DetailRoomAvailableScheduleSearch();
                var studyTimeData = await db.tbl_StudyTime
                    .Where(x => x.Enable == true && x.Id == baseSearch.StudyTimeId)
                    .Select(x => new { x.Id, x.Name, x.StartTime, x.EndTime })
                    .SingleOrDefaultAsync();
                if (studyTimeData == null)
                    throw new Exception("Không tìm thấy thông tin ca học");
                //var scheduleData = await db.tbl_Schedule.Where(x => x.Enable == true && (baseSearch.BranchId == 0 || x.BranchId == baseSearch.BranchId))
                //    .Select(x => new { x.Id, x.RoomId, x.StartTime, x.EndTime }).ToListAsync();
                var scheduleData = await (from s in db.tbl_Schedule
                                          join c in db.tbl_Class on s.ClassId equals c.Id
                                          where s.Enable == true && c.Enable == true
                                                && (baseSearch.BranchId == 0 || s.BranchId == baseSearch.BranchId)
                                                && (c == null || c.Status != 3)
                                          select new
                                          {
                                              s.Id,
                                              s.RoomId,
                                              s.StartTime,
                                              s.EndTime,
                                          }).ToListAsync();
                var roomData = await db.tbl_Room
                    .Where(x => x.Enable == true
                        && (baseSearch.BranchId == 0 || x.BranchId == baseSearch.BranchId)
                        && (string.IsNullOrEmpty(baseSearch.RoomIds) || baseSearch.RoomIds.Contains(x.Id.ToString())))
                    .Select(x => new { x.Id, x.Name, x.Code })
                    .ToListAsync();
                int totalRow = 0;
                var result = new List<RoomModel>();
                if (roomData.Count != 0)
                {
                    var dayOfWeek = new RoomOfDayOfWeekModel();
                    var date = baseSearch.DateTime.ToString("dd/MM/yyyy");
                    var startDateTime = DateTime.ParseExact($"{date} {studyTimeData.StartTime}", "dd/MM/yyyy HH:mm", null);
                    var endDateTime = DateTime.ParseExact($"{date} {studyTimeData.EndTime}", "dd/MM/yyyy HH:mm", null);
                    foreach (var r in roomData)
                    {
                        var teacherSchedule = scheduleData
                            .Any(x => x.RoomId == r.Id
                            && x.StartTime.Value.ToString("dd/MM/yyyy HH:mm") == startDateTime.ToString("dd/MM/yyyy HH:mm")
                            && x.EndTime.Value.ToString("dd/MM/yyyy HH:mm") == endDateTime.ToString("dd/MM/yyyy HH:mm"));
                        if (!teacherSchedule)
                        {
                            var room = new RoomModel
                            {
                                RoomId = r.Id,
                                RoomCode = r.Code,
                                RoomName = r.Name
                            };
                            result.Add(room);
                        }
                    }
                    totalRow = result.Count;
                    result = result.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                }
                else return new AppDomainResult() { Data = null, TotalRow = 0 };
                return new AppDomainResult() { Data = result, TotalRow = totalRow };
            }
        }
    }
}