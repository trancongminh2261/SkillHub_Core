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
namespace LMSCore.Services.StudentInClass
{
    public class StudentInClassNotification
    {
        //notify_sale_student_is_on_warning
        /// <summary>
        /// Gửi thông báo cho tư vấn viên khi học viên bị cảnh báo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifySaleStudentIsOnWarning(StudentInClassNotificationRequest.NotifySaleStudentIsOnWarningRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_sale_student_is_on_warning;
                var studentInClass = await dbContext.tbl_StudentInClass.SingleOrDefaultAsync(x => x.Id == request.StudentInClassId);
                if (studentInClass == null)
                    return;
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentInClass.StudentId);
                if (student == null)
                    return;
                var sale = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.SaleId);
                if (sale == null)
                    return;
                var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == studentInClass.ClassId);
                if (_class == null)
                    return;

                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", sale.FullName);
                token.Add("[StudentName]", student.FullName);
                token.Add("[ClassName]", _class.Name);

                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = studentInClass.Id,
                    Token = token,
                    UserId = sale.UserInformationId
                }, request.CurrentUser);
            }
        }
        /// <summary>
        /// Gửi thông báo cho phụ huynh khi học viên bị cảnh báo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyParentsStudentIsOnWarning(StudentInClassNotificationRequest.NotifyParentsStudentIsOnWarningRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_parents_student_is_on_warning;
                var studentInClass = await dbContext.tbl_StudentInClass.SingleOrDefaultAsync(x => x.Id == request.StudentInClassId);
                if (studentInClass == null)
                    return;
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentInClass.StudentId);
                if (student == null)
                    return;
                var parent = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.ParentId);
                if (parent == null)
                    return;
                var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == studentInClass.ClassId);
                if (_class == null)
                    return;

                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", parent.FullName);
                token.Add("[StudentName]", student.FullName);
                token.Add("[ClassName]", _class.Name);

                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = studentInClass.Id,
                    Token = token,
                    UserId = parent.UserInformationId
                }, request.CurrentUser);
            }
        }
    }
    public class StudentInClassNotificationRequest
    {
        public class NotifyWarningRequest
        { 
            public int StudentInClassId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifySaleStudentIsOnWarningRequest : NotifyWarningRequest
        {

        }
        public class NotifyParentsStudentIsOnWarningRequest : NotifyWarningRequest
        {

        }
    }
}
