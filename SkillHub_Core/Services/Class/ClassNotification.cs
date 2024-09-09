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
using LMSCore.DTO.ClassDTO;
using Microsoft.AspNetCore.Http;

namespace LMSCore.Services.Class
{
    public class ClassNotification
    {
        /// <summary>
        /// Thông báo cho giáo viên khi 30 phút nữa buổi học diễn ra
        /// </summary>
        /// <returns></returns>
        public static async Task NotifyTeacherAlmostTimeForClass()
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_teacher_almost_time_for_class;
                var checkTime = DateTime.Now.AddMinutes(30);
                var schedules = await dbContext.tbl_Schedule
                    .Where(x => x.Enable == true && !x.SentNotificationTeacher && x.StartTime <= checkTime)
                    .ToListAsync();
                if (schedules.Any())
                {
                    foreach (var schedule in schedules)
                    {
                        var teacher = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == schedule.TeacherId);
                        if (teacher == null)
                        {
                            schedule.SentNotificationTeacher = true;
                            await dbContext.SaveChangesAsync();
                            continue;
                        }
                        var _Class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                        if (_Class == null)
                        {
                            schedule.SentNotificationTeacher = true;
                            await dbContext.SaveChangesAsync();
                            continue;
                        }
                        if (!schedule.StartTime.HasValue)
                        {
                            schedule.SentNotificationTeacher = true;
                            await dbContext.SaveChangesAsync();
                            continue;
                        }
                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", teacher.FullName);
                        token.Add("[ClassName]", _Class.Name);
                        token.Add("[StartTime]", schedule.StartTime.Value.ToString("HH:mm dd/MM/yyyy"));
                        token.Add("[ClassId]", _Class.Id);
                        token.Add("[CurriculumId]", _Class.CurriculumId);
                        token.Add("[BranchId]", _Class.BranchId);
                        token.Add("[ScoreBoardTemplateId]", _Class.ScoreboardTemplateId);

                        var currentUser = new tbl_UserInformation
                        {
                            FullName = "Tự động"
                        };
                        await NotificationService.SendAllMethodsByCode(new NotificationRequest
                        {
                            Code = code,
                            AvailableId = schedule.Id,
                            Token = token,
                            UserId = teacher.UserInformationId
                        }, currentUser);
                        schedule.SentNotificationTeacher = true;
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
        /// <summary>
        /// Thông báo cho học viên khi 30 phút nữa buổi học diễn ra
        /// </summary>
        /// <returns></returns>
        public static async Task NotifyStudentAlmostTimeForClass()
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_student_almost_time_for_class;
                var checkTime = DateTime.Now.AddMinutes(30);
                var schedules = await dbContext.tbl_Schedule
                    .Where(x => x.Enable == true && !x.SentNotificationStudent && x.StartTime <= checkTime)
                    .ToListAsync();
                if (schedules.Any())
                {
                    foreach (var schedule in schedules)
                    {
                        var _Class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                        if (_Class == null)
                        {
                            schedule.SentNotificationStudent = true;
                            await dbContext.SaveChangesAsync();
                            continue;
                        }
                        if (!schedule.StartTime.HasValue)
                        {
                            schedule.SentNotificationStudent = true;
                            await dbContext.SaveChangesAsync();
                            continue;
                        }
                        var studentInClasses = await dbContext.tbl_StudentInClass
                            .Where(x => x.ClassId == _Class.Id && x.Enable == true)
                            .Select(x => x.StudentId).ToListAsync();
                        var currentUser = new tbl_UserInformation
                        {
                            FullName = "Tự động"
                        };
                        if (studentInClasses.Any())
                        {
                            foreach (var studentId in studentInClasses)
                            {
                                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentId);
                                Hashtable token = new Hashtable();
                                token.Add("[ReceiverName]", student.FullName);
                                token.Add("[ClassName]", _Class.Name);
                                token.Add("[StartTime]", schedule.StartTime.Value.ToString("HH:mm dd/MM/yyyy"));
                                token.Add("[ClassId]", _Class.Id);
                                token.Add("[CurriculumId]", _Class.CurriculumId);
                                token.Add("[BranchId]", _Class.BranchId);
                                token.Add("[ScoreBoardTemplateId]", _Class.ScoreboardTemplateId);


                                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                                {
                                    Code = code,
                                    AvailableId = schedule.Id,
                                    Token = token,
                                    UserId = student.UserInformationId
                                }, currentUser);
                            }
                        }

                        schedule.SentNotificationStudent = true;
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
        /// <summary>
        /// Thông báo khi học viên được thêm vào lớp mới
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyStudentAddedToTheClass(NotifyStudentAddedToTheClassRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                if (!request.Items.Any())
                    return;

                string code = NotificationCode.notify_student_added_to_the_class;
                foreach (var item in request.Items)
                {
                    var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                    if (student == null)
                        return;

                    var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == item.ClassId);
                    if (_class == null)
                        return;
                    Hashtable token = new Hashtable();
                    token.Add("[ReceiverName]", student.FullName);
                    token.Add("[ClassName]", _class.Name);
                    token.Add("[ClassId]", _class.Id);
                    token.Add("[CurriculumId]", _class.CurriculumId);
                    token.Add("[BranchId]", _class.BranchId);
                    token.Add("[ScoreBoardTemplateId]", _class.ScoreboardTemplateId);

                    await NotificationService.SendAllMethodsByCode(new NotificationRequest
                    {
                        Code = code,
                        AvailableId = _class.Id,
                        Token = token,
                        UserId = student.UserInformationId
                    }, request.CurrentUser);
                }
            }
        }
        /// <summary>
        /// Thông báo cho phụ huynh khi học viên được thêm vào lớp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyParentsAddedToTheClass(NotifyParentsAddedToTheClassRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_parents_added_to_the_class;

                if (!request.Items.Any())
                    return;
                foreach (var item in request.Items)
                {
                    var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                    if (student == null)
                        return;

                    var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == item.ClassId);
                    if (_class == null)
                        return;

                    var parent = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.ParentId);
                    if (parent == null)
                        return;

                    Hashtable token = new Hashtable();
                    token.Add("[ReceiverName]", parent.FullName);
                    token.Add("[StudentName]", student.FullName);
                    token.Add("[ClassName]", _class.Name);
                    token.Add("[ClassId]", _class.Id);
                    token.Add("[CurriculumId]", _class.CurriculumId);
                    token.Add("[BranchId]", _class.BranchId);
                    token.Add("[ScoreBoardTemplateId]", _class.ScoreboardTemplateId);

                    await NotificationService.SendAllMethodsByCode(new NotificationRequest
                    {
                        Code = code,
                        AvailableId = _class.Id,
                        Token = token,
                        UserId = parent.UserInformationId
                    }, request.CurrentUser);
                }
            }
        }
        /// <summary>
        /// Gửi thông báo cho tư vấn viên khi học viên hoàn thành khóa học
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifySaleStudentCompletesTheClass(NotifySaleStudentCompletesTheClassRequest request)
        {
            string code = NotificationCode.notify_sale_student_completes_the_class;
            using (var dbContext = new lmsDbContext())
            {
                if (request.StudentInClassIds.Any())
                {
                    foreach (var item in request.StudentInClassIds)
                    {
                        var studentInClass = await dbContext.tbl_StudentInClass.SingleOrDefaultAsync(x => x.Id == item);
                        if (studentInClass == null)
                            continue;
                        var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentInClass.StudentId);
                        if (student == null)
                            continue;
                        var sale = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.SaleId);
                        if (sale == null)
                            continue;

                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", sale.FullName);
                        token.Add("[StudentName]", student.FullName);
                        token.Add("[StudentId]", student.UserInformationId);

                        var currentUser = new tbl_UserInformation
                        {
                            FullName = "Tự động"
                        };
                        await NotificationService.SendAllMethodsByCode(new NotificationRequest
                        {
                            Code = code,
                            AvailableId = student.UserInformationId,
                            Token = token,
                            UserId = sale.UserInformationId
                        }, currentUser);
                    }
                }
            }
        }
        #region Tự động thông báo tới phụ huynh kết quả học tập của học sinh vào ngày 1 và ngày 15 mỗi tháng

        public static async Task NotifyStudentLearningOutcomes()
        {
            using (var dbContext = new lmsDbContext())
            {
                var currentDate = DateTime.Now;
                bool isFirstDay = currentDate.Day == 1;
                bool isFifteenthDay = currentDate.Day == 15;
                if (isFirstDay == true || isFifteenthDay == true)
                {
                    string code = NotificationCode.notify_parents_student_learning_outcomes;
                    var checkTime = DateTime.Now.AddMinutes(30);
                    var userInformation = await dbContext.tbl_UserInformation
                        .Where(x => x.Enable == true && (x.RoleId == (int)RoleEnum.parents || x.RoleId == (int)RoleEnum.student))
                        .Select(x => new { x.UserInformationId, x.BranchIds, x.ParentId, x.RoleId, x.FullName, x.Email })
                        .ToListAsync();
                    var parents = userInformation
                        .Where(x => x.RoleId == (int)RoleEnum.parents && !string.IsNullOrEmpty(x.Email))
                        .ToList();
                    var classes = await dbContext.tbl_Class
                        .Where(x => x.Enable == true)
                        .Select(x => new { x.Id, x.BranchId, x.Name, x.Status })
                        .ToListAsync();
                    var schedules = await dbContext.tbl_Schedule
                        .Where(x => x.Enable == true)
                        .Select(x => new { x.Id, x.ClassId })
                        .ToListAsync();
                    var studentInClass = await dbContext.tbl_StudentInClass
                        .Where(x => x.Enable == true)
                        .Select(x => new { x.Id, x.ClassId, x.StudentId, x.BranchId })
                        .ToListAsync();
                    var rollUp = await dbContext.tbl_RollUp
                        .Where(x => x.Enable == true && x.Status != 2 && x.Status != 3 && x.Status != 6)
                        .Select(x => new { x.Status, x.Id, x.ClassId, x.StudentId, x.ScheduleId })
                        .ToListAsync();
                    var classTranscripts = await dbContext.tbl_ClassTranscript.Where(x => x.Enable == true).ToListAsync();
                    var classTranscriptDetails = await dbContext.tbl_ClassTranscriptDetail.Where(x => x.Enable == true).ToListAsync();
                    var classGrades = await dbContext.tbl_SaveGradesInClass.Where(x => x.Enable == true).ToListAsync();
                    var tasks = new List<Task>();

                    foreach (var p in parents)
                    {
                        var task = Task.Run(async () =>
                        {
                            var students = userInformation.Where(x => x.ParentId == p.UserInformationId).ToList();
                            foreach (var s in students)
                            {
                                var classOfStudent = studentInClass.Where(x => x.StudentId == s.UserInformationId).ToList();
                                foreach (var c in classOfStudent)
                                {
                                    var result = new StudentLearningOutcomesDTO();
                                    var homeworkInClassData = new List<HomeworkInClassModel>();
                                    var _class = classes.FirstOrDefault(x => x.Id == c.ClassId && x.Status == 2 && s.BranchIds.Contains(x.BranchId.ToString()));
                                    if (_class != null)
                                    {
                                        string sql = $"Get_Homework " +
                                            $"@Search = N'{""}', " +
                                            $"@PageIndex = {1}," +
                                            $"@PageSize = {int.MaxValue}," +
                                            $"@ClassId = {_class.Id}";
                                        var data = await dbContext.SqlQuery<HomeworkInClassModel>(sql);
                                        homeworkInClassData = data;

                                        var rollUpData = rollUp.Where(x => x.ClassId == _class.Id && x.StudentId == s.UserInformationId).ToList();
                                        var scheduleData = schedules.Where(x => x.ClassId == _class.Id).ToList();
                                        var scoreInClassModel = new List<ScoreInClassModel>();
                                        var classTranscript = classTranscripts.Where(x => x.ClassId == _class.Id).OrderByDescending(x => x.CreatedOn).ToList();
                                        if (classTranscript.Count != 0)
                                        {
                                            foreach (var item in classTranscript)
                                            {
                                                var scoreInClassData = new ScoreInClassModel();
                                                var classTranscriptDetailModel = new List<ClassTranscriptDetailModel>();
                                                scoreInClassData.Name = item.Name;
                                                var classTranscriptDetail = classTranscriptDetails.Where(x => x.ClassTranscriptId == item.Id).ToList();
                                                if (classTranscriptDetail.Count != 0)
                                                {
                                                    foreach (var ctd in classTranscriptDetail)
                                                    {
                                                        var classGrade = classGrades.FirstOrDefault(x => x.Enable == true && x.ClassTranscriptId == item.Id && x.ClassTranscriptDetailId == ctd.Id);
                                                        if (classGrade != null)
                                                        {
                                                            var classTranscriptDetailData = new ClassTranscriptDetailModel
                                                            {
                                                                Name = ctd.Name,
                                                                Value = classGrade.Value
                                                            };
                                                            classTranscriptDetailModel.Add(classTranscriptDetailData);
                                                        }
                                                    }
                                                }
                                                scoreInClassData.ClassTranscriptDetailModel = classTranscriptDetailModel;
                                                scoreInClassModel.Add(scoreInClassData);
                                            }
                                        }
                                        result.ParentName = p.FullName;
                                        result.StudentName = s.FullName;
                                        result.ClassName = _class.Name;
                                        result.Attendance = rollUpData.Count + "/" + scheduleData.Count + " buổi";
                                        result.HomeworkInClassModel = homeworkInClassData;
                                        result.ScoreInClassModel = scoreInClassModel;
                                        if (result.HomeworkInClassModel.Count != 0 || result.ScoreInClassModel.Count != 0)
                                        {
                                            string title = "Thông báo kết quả học tập của học sinh " + result.StudentName + " ở lớp " + result.ClassName;
                                            string templateName = "StudentLearningOutcomes.xlsx";
                                            string fileNameToSave = "KetQuaHocTap_" + result.StudentName + "_" + result.ClassName;
                                            string folderToSave = "StudentLearningOutcomes";
                                            var path = await ExcelExportService.ExportExcelNotification(result, templateName, fileNameToSave, folderToSave);
                                            string content = "Hệ thống xin gửi kết quả học tập của học sinh " + result.StudentName + " tại lớp " + result.ClassName + ". Vui lòng xem đầy đủ ở file excel đính kèm.";
                                            AssetCRM.SendMail(p.Email, title, content, path);
                                        }
                                    }
                                }
                            }
                        });
                        tasks.Add(task);
                    }
                    await Task.WhenAll(tasks);
                }
            }
        }
        #endregion
    }
    #region Model
    public class ClassNotificationRequest
    {
        public class NotifyAddedToTheClassItem
        {
            public int StudentId { get; set; }
            public int ClassId { get; set; }
        }
        public class NotifySaleStudentCompletesTheClassRequest
        {
            public List<int> StudentInClassIds { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifyStudentAddedToTheClassRequest
        {
            public List<NotifyAddedToTheClassItem> Items { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifyParentsAddedToTheClassRequest
        {
            public List<NotifyAddedToTheClassItem> Items { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
    }
    #endregion
}
