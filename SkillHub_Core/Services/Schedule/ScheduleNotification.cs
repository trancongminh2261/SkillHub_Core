using Hangfire;
using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Globalization;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using Microsoft.Extensions.Primitives;
using LMSCore.NotificationConfig;
using System.Collections;
using static LMSCore.Services.Schedule.ScheduleNotificationRequest;

namespace LMSCore.Services.Schedule
{
    public class ScheduleNotification
    {
        /// <summary>
        /// Gửi thông báo cho học viên khi lớp có buổi học mới
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyStudentClassHasANewSchedule(NotifyStudentClassHasANewScheduleRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_student_class_has_a_new_schedule;
                var schedule = await dbContext.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == request.ScheduleId);
                if (schedule == null)
                    return;
                var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                if (_class == null)
                    return;
                var studentInClasses = await dbContext.tbl_StudentInClass.Where(x => x.ClassId == _class.Id && x.Enable == true).ToListAsync();
                if (!studentInClasses.Any())
                    return;
                foreach (var studentInClass in studentInClasses)
                {
                    var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentInClass.StudentId);
                    if (student == null)
                        return;

                    Hashtable token = new Hashtable();
                    token.Add("[ReceiverName]", student.FullName);
                    token.Add("[ClassName]", _class.Name);
                    token.Add("[StartTime]", schedule.StartTime.Value.ToString("HH:mm dd/MM/yyyy"));
                    token.Add("[ClassId]", _class.Id);
                    token.Add("[CurriculumId]", _class.CurriculumId);
                    token.Add("[BranchId]", _class.BranchId);
                    token.Add("[ScoreBoardTemplateId]", _class.ScoreboardTemplateId);
                    await NotificationService.SendAllMethodsByCode(new NotificationRequest
                    {
                        Code = code,
                        AvailableId = schedule.Id,
                        Token = token,
                        UserId = student.UserInformationId
                    }, request.CurrentUser);
                }
            }
        }
        /// <summary>
        /// Gửi thông báo cho giáo viên khi có lịch mới
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyTeacherClassHasANewSchedule(NotifyTeacherClassHasANewScheduleRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_teacher_class_has_a_new_schedule;
                var schedule = await dbContext.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == request.ScheduleId);
                if (schedule == null)
                    return;
                var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                if (_class == null)
                    return;
                var teacher = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == _class.TeacherId);
                if (teacher == null)
                    return;

                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", teacher.FullName);
                token.Add("[ClassName]", _class.Name);
                token.Add("[StartTime]", schedule.StartTime.Value.ToString("HH:mm dd/MM/yyyy"));
                token.Add("[ClassId]", _class.Id);
                token.Add("[CurriculumId]", _class.CurriculumId);
                token.Add("[BranchId]", _class.BranchId);
                token.Add("[ScoreBoardTemplateId]", _class.ScoreboardTemplateId);
                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = schedule.Id,
                    Token = token,
                    UserId = teacher.UserInformationId
                }, request.CurrentUser);
            }
        }
    }
    public class ScheduleNotificationRequest
    {
        public class NotifyNewSchedule
        {
            public int ScheduleId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifyStudentClassHasANewScheduleRequest : NotifyNewSchedule
        { 
        }
        public class NotifyTeacherClassHasANewScheduleRequest : NotifyNewSchedule
        { 
        }
    }
}
