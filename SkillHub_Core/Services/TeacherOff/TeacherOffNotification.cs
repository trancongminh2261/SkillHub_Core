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
namespace LMSCore.Services.TeacherOff
{
    public class TeacherOffNotification
    {
        /// <summary>
        /// Gửi thông báo cho học vụ và quản lý khi giáo viên đăng ký nghỉ
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyManagerTeacherRequestsTimeOff(TeacherOffNotificationRequest.NotifyManagerTeacherRequestsTimeOffRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_manager_teacher_requests_time_off;
                var teacherOff = await dbContext.tbl_TeacherOff.SingleOrDefaultAsync(x => x.Id == request.TeacherOffId);
                if (teacherOff == null)
                    return;
                var teacher = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == teacherOff.TeacherId);
                if (teacher == null)
                    return;

                string sql = $"Get_UserOption @RoleIds = N'{((int)RoleEnum.academic)},{((int)RoleEnum.manager)}', @BranchIds = N'{teacher.BranchIds}'";
                var arrives = await dbContext.SqlQuery<UserOption>(sql);
                if (!arrives.Any())
                    return;

                foreach (var item in arrives)
                {
                    var receiver = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.UserInformationId);
                    if (receiver == null)
                        continue;
                    Hashtable token = new Hashtable();
                    token.Add("[ReceiverName]", receiver.FullName);
                    token.Add("[TeacherName]", teacher.FullName);
                    token.Add("[StartDate]", teacherOff.StartTime.HasValue ? teacherOff.StartTime.Value.ToString("dd/MM/yyyy") : "");
                    await NotificationService.SendAllMethodsByCode(new NotificationRequest
                    {
                        Code = code,
                        AvailableId = teacherOff.Id,
                        Token = token,
                        UserId = receiver.UserInformationId
                    }, request.CurrentUser);
                }
            }
        }
        /// <summary>
        /// Gửi thông báo cho giáo viên khi đơn nghỉ được duyệt
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyTeacherLeaveRequestHasBeenApproved(TeacherOffNotificationRequest.NotifyTeacherLeaveRequestHasBeenApprovedRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_teacher_leave_request_has_been_approved;
                var teacherOff = await dbContext.tbl_TeacherOff.SingleOrDefaultAsync(x => x.Id == request.TeacherOffId);
                if (teacherOff == null)
                    return;
                var teacher = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == teacherOff.TeacherId);
                if (teacher == null)
                    return;

                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", teacher.FullName);
                token.Add("[StartDate]", teacherOff.StartTime.HasValue ? teacherOff.StartTime.Value.ToString("dd/MM/yyyy") : "");
                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = teacherOff.Id,
                    Token = token,
                    UserId = teacher.UserInformationId
                }, request.CurrentUser);
            }
        }
        /// <summary>
        /// Gửi thông báo khi đơn xin nghỉ bị hủy
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyTeacherLeaveRequestHasBeenCanceled(TeacherOffNotificationRequest.NotifyTeacherLeaveRequestHasBeenCanceledRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_teacher_leave_request_has_been_canceled;
                var teacherOff = await dbContext.tbl_TeacherOff.SingleOrDefaultAsync(x => x.Id == request.TeacherOffId);
                if (teacherOff == null)
                    return;
                var teacher = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == teacherOff.TeacherId);
                if (teacher == null)
                    return;

                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", teacher.FullName);
                token.Add("[StartDate]", teacherOff.StartTime.HasValue ? teacherOff.StartTime.Value.ToString("dd/MM/yyyy") : "");
                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = teacherOff.Id,
                    Token = token,
                    UserId = teacher.UserInformationId
                }, request.CurrentUser);
            }
        }
    }
    public class TeacherOffNotificationRequest
    {
        public class NotifyTeacherOffRequest
        { 
            public int TeacherOffId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifyManagerTeacherRequestsTimeOffRequest : NotifyTeacherOffRequest
        { 
        }
        public class NotifyTeacherLeaveRequestHasBeenApprovedRequest : NotifyTeacherOffRequest
        { 
        }
        public class NotifyTeacherLeaveRequestHasBeenCanceledRequest : NotifyTeacherOffRequest
        { 
        }
    }
}
