using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using LMSCore.Utilities;

namespace LMSCore.Services
{
    public class AutoNotiService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        public static async Task AutoNotiClassComing()
        {
            using (var db = new lmsDbContext())
            {
                var env = WebHostEnvironment.Environment;
                string content = "";
                string notificationContent = "";
                string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
                var pathViews = Path.Combine(env.ContentRootPath, "Views");
                content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Class/ClassComming.cshtml");

                //var timeNow = DateTime.Now;
                var timeAfter1Hour = DateTime.Now.AddHours(1);
                //danh sách schedule 1 tiếng nữa vô học
                List<tbl_Schedule> schedule = await db.tbl_Schedule.Where(x => x.Enable == true && x.Announced == false && x.StartTime <= timeAfter1Hour).ToListAsync();
                if (schedule.Any())
                {
                    string title = configuration.GetSection("MySettings:ProjectName").Value.ToString();

                    tbl_UserInformation user = new tbl_UserInformation
                    {
                        FullName = "Tự động"
                    };
                    int scheduleCount = schedule.Count;
                    for (int i = 0; i < scheduleCount; i++)
                    {
                        string startTime = (schedule[i].StartTime ?? timeAfter1Hour).ToString("dd/MM/yyyy HH:mm");
                        string startDay = (schedule[i].StartTime ?? timeAfter1Hour).ToString("dd");
                        string startMonth = (schedule[i].StartTime ?? timeAfter1Hour).ToString("MM");
                        string startYear = (schedule[i].StartTime ?? timeAfter1Hour).ToString("yyyy");
                        string startHour = (schedule[i].StartTime ?? timeAfter1Hour).ToString("hh:mm tt");
                        string endtHour = (schedule[i].EndTime ?? timeAfter1Hour).ToString("hh:mm tt");
                        int classId = schedule[i].ClassId ?? 0;
                        int teacherId = schedule[i].TeacherId;
                        int roomId = schedule[i].RoomId;
                        var room = await db.tbl_Room.FirstOrDefaultAsync(x => x.Id == roomId);
                        tbl_Class _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == classId);
                        if (_class == null)
                            continue;
                        string className = _class.Name;
                        //thông báo cho giáo viên của lớp
                        //int teacherId = _class.TeacherId ?? 0;
                        var teacher = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == teacherId);
                        string dayOfWeek = (schedule[i].StartTime ?? timeAfter1Hour).ToString("dddd", CultureInfo.GetCultureInfo("vi-VN"));
                        // Gắn thông tin vào file mẫu gửi mail
                        UrlNotificationModels urlNotification = new UrlNotificationModels();
                        string url = "class=" + _class.Id + "&curriculum=" + _class.CurriculumId + "&branch=" + _class.BranchId + "&scoreBoardTemplateId=" + _class.ScoreboardTemplateId;
                        string urlEmail = urlNotification.url + urlNotification.urlDetailClass + url;

                        content = content.Replace("{item1}", className);
                        if (teacher == null) content = content.Replace("{item2}", "");
                        else content = content.Replace("{item2}", teacher.FullName);
                        if (room == null) content = content.Replace("{room}", "Online");
                        else content = content.Replace("{room}", room.Name);
                        content = content.Replace("{item3}", dayOfWeek);
                        content = content.Replace("{item4}", startDay);
                        content = content.Replace("{item5}", startMonth);
                        content = content.Replace("{item6}", startYear);
                        content = content.Replace("{item7}", startHour);
                        content = content.Replace("{item8}", endtHour);
                        content = content.Replace("{item9}", projectName);
                        content = content.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");
                        notificationContent = @"<div>" + content + @"</div>";

                        // Thông báo cho giáo viên
                        //if (teacherId > 0)
                        //{
                        //    Thread sendTeacher = new Thread(async () =>
                        //    {
                        //        tbl_Notification notiTeacher = new tbl_Notification();
                        //        notiTeacher.Title = title + " thông báo lịch dạy";
                        //        notiTeacher.Content = "Bạn có lịch dạy lớp " + className + " vào lúc " + startTime + ". Vui lòng kiểm tra.";
                        //        notiTeacher.ContentEmail = notificationContent;
                        //        notiTeacher.UserId = teacherId;
                        //        notiTeacher.Type = 2;
                        //        notiTeacher.Category = 0;
                        //        notiTeacher.Url = urlNotification.urlDetailClass + url;
                        //        notiTeacher.AvailableId = _class.Id;
                        //        ScheduleParam param = new ScheduleParam { TeacherIds = teacherId.ToString(), ClassId = _class.Id };
                        //        notiTeacher.ParamString = JsonConvert.SerializeObject(param);
                        //        await NotificationService.Send(notiTeacher, user, true);
                        //    });
                        //    sendTeacher.Start();
                        //}

                        //thông báo cho học viên của mỗi schedule
                        List<tbl_UserInformation> studentInClasses = await (from sic in db.tbl_StudentInClass
                                                                            join u in db.tbl_UserInformation on sic.StudentId equals u.UserInformationId into list
                                                                            from l in list
                                                                            where sic.Enable == true && l.Enable == true && classId == sic.ClassId
                                                                            select l).ToListAsync();

                        int studentCount = studentInClasses.Count;
                        if (studentInClasses.Any())
                        {
                            //Thread sendStudent = new Thread(async () =>
                            //{
                            //    for (int j = 0; j < studentCount; j++)
                            //    {
                            //        tbl_Notification notiStudent = new tbl_Notification();
                            //        notiStudent.Title = title + " thông báo lịch học";
                            //        notiStudent.Content = "Bạn có lịch học lớp " + className + " vào lúc " + startTime + ". Vui lòng kiểm tra.";
                            //        notiStudent.ContentEmail = notificationContent;
                            //        int studentId = studentInClasses[j].UserInformationId;
                            //        notiStudent.UserId = studentId;
                            //        notiStudent.Category = 0;
                            //        notiStudent.Url = urlNotification.urlDetailClass + url;
                            //        notiStudent.AvailableId = _class.Id;
                            //        ScheduleParam param = new ScheduleParam { StudentId = studentId, ClassId = _class.Id };
                            //        notiStudent.ParamString = JsonConvert.SerializeObject(param);
                            //        await NotificationService.Send(notiStudent, user, true);
                            //        //nếu học viên này có phụ huynh thì thông báo cho phụ huynh
                            //        if (studentInClasses[j].ParentId != null)
                            //        {
                            //            tbl_Notification notiParent = new tbl_Notification();
                            //            notiParent.Title = title + " thông báo lịch học cho học viên " + studentInClasses[j].FullName;
                            //            notiParent.Content = "Học viên " + studentInClasses[j].FullName + " có lịch học lớp " + className + " vào lúc " + startTime + ". Vui lòng kiểm tra.";
                            //            notiParent.ContentEmail = notificationContent;
                            //            notiParent.UserId = studentInClasses[j].ParentId;
                            //            notiParent.Category = 0;
                            //            notiParent.Url = urlNotification.urlDetailClass + url;
                            //            notiParent.AvailableId = _class.Id;
                            //            await NotificationService.Send(notiParent, user, true);
                            //        }
                            //    }
                            //});
                            //sendStudent.Start();
                        }
                        schedule[i].Announced = true;
                    }
                    await db.SaveChangesAsync();
                }
            }
        }

    }
}