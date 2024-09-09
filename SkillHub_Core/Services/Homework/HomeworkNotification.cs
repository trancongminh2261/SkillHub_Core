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
using static LMSCore.Services.Class.ClassNotificationRequest;
using MimeKit.Cryptography;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static LMSCore.Services.Customer.CustomerNotificationRequest;
using static LMSCore.Services.Homework.HomeworkNotificationRequest;

namespace LMSCore.Services.Homework
{
    public class HomeworkNotification
    {
        /// <summary>
        /// Gửi thông báo cho học viên khi có bài tập mới
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyStudentHaveHomework(NotifyStudentHaveHomeworkRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_student_have_homework;
                var homework = await dbContext.tbl_Homework.SingleOrDefaultAsync(x => x.Id == request.HomeworkId);
                if (homework == null)
                    return;
                var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == homework.ClassId);
                if (_class == null)
                    return;
                var studentInClasses = await dbContext.tbl_StudentInClass
                    .Where(x => x.ClassId == _class.Id && x.Enable == true).Select(x => new { x.Id, x.ClassId, x.StudentId }).ToListAsync();
                if (studentInClasses.Any())
                {
                    foreach (var studentInClass in studentInClasses)
                    {
                        var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentInClass.StudentId);
                        if (student == null)
                            continue;
                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", student.FullName);
                        token.Add("[ClassName]", _class.Name);
                        token.Add("[HomeworkName]", homework.Name);
                        if (homework.ToDate.HasValue)
                        token.Add("[EndDate]", homework.ToDate.Value.ToString("dd/MM/yyyy"));
                        else token.Add("[EndDate]", "Cập nhật sau");
                        token.Add("[ClassId]", _class.Id);
                        token.Add("[CurriculumId]", _class.CurriculumId);
                        token.Add("[BranchId]", _class.BranchId);
                        token.Add("[ScoreBoardTemplateId]", _class.ScoreboardTemplateId);
                        await NotificationService.SendAllMethodsByCode(new NotificationRequest
                        {
                            Code = code,
                            AvailableId = homework.Id,
                            Token = token,
                            UserId = student.UserInformationId
                        }, request.CurrentUser);
                    }
                }
            }
        }
    }
    public class HomeworkNotificationRequest
    {
        public class NotifyStudentHaveHomeworkRequest
        {
            public int HomeworkId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
    }
}
