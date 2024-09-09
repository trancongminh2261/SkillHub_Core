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
using static LMSCore.Services.Bill.BillNotificationRequest;

namespace LMSCore.Services.Bill
{
    public class BillNotification
    {
        /// <summary>
        /// Thông báo cho học viên khi đăng ký học thành công
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyStudentSuccessfulClassRegistration(NotifyStudentSuccessfulClassRegistrationRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_student_successful_class_registration;
                var bill = await dbContext.tbl_Bill.SingleOrDefaultAsync(x => x.Id == request.BillId);
                if (bill == null)
                    return;
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == bill.StudentId);
                if (student == null)
                    return;

                var details = await dbContext.tbl_BillDetail.Where(x => x.BillId == bill.Id && x.Enable == true).ToListAsync();
                if (details.Any())
                {
                    foreach (var detail in details)
                    {
                        if (!detail.ClassId.HasValue)
                            continue;
                        var _Class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == detail.ClassId);
                        if (_Class == null)
                            continue;

                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", student.FullName);
                        token.Add("[ClassName]", _Class.Name);
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
                            AvailableId = _Class.Id,
                            Token = token,
                            UserId = student.UserInformationId
                        }, currentUser);
                    }
                }
            }
        }
        /// <summary>
        /// Thông báo cho phụ huynh khi đăng ký học thành công
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyParentsSuccessfulClassRegistration(NotifyParentsSuccessfulClassRegistrationRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_parents_successful_class_registration;
                var bill = await dbContext.tbl_Bill.SingleOrDefaultAsync(x => x.Id == request.BillId);
                if (bill == null)
                    return;
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == bill.StudentId);
                if (student == null)
                    return;

                var parent = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.ParentId);
                if (parent == null)
                    return;
                var details = await dbContext.tbl_BillDetail.Where(x => x.BillId == bill.Id && x.Enable == true).ToListAsync();
                if (details.Any())
                {
                    foreach (var detail in details)
                    {
                        if (!detail.ClassId.HasValue)
                            continue;
                        var _Class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == detail.ClassId);
                        if (_Class == null)
                            continue;

                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", parent.FullName);
                        token.Add("[StudentName]", student.FullName);
                        token.Add("[ClassName]", _Class.Name);
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
                            AvailableId = _Class.Id,
                            Token = token,
                            UserId = parent.UserInformationId
                        }, currentUser);
                    }
                }
            }
        }
        /// <summary>
        /// Thông báo khi học viên đăng ký đặt chỗ thành công
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyStudentClassPlacementRegistration(NotifyStudentClassPlacementRegistrationRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_student_class_placement_registration;
                var bill = await dbContext.tbl_Bill.SingleOrDefaultAsync(x => x.Id == request.BillId);
                if (bill == null)
                    return;
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == bill.StudentId);
                if (student == null)
                    return;

                var details = await dbContext.tbl_BillDetail.Where(x => x.BillId == bill.Id && x.Enable == true).ToListAsync();
                if (details.Any())
                {
                    foreach (var detail in details)
                    {
                        if (detail.ClassId.HasValue)
                            continue;
                        if (!detail.ProgramId.HasValue)
                            continue;
                        var program = await dbContext.tbl_Program.SingleOrDefaultAsync(x => x.Id == detail.ProgramId);
                        if (program == null)
                            continue;

                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", student.FullName);
                        token.Add("[ProgramName]", program.Name);

                        var currentUser = new tbl_UserInformation
                        {
                            FullName = "Tự động"
                        };
                        await NotificationService.SendAllMethodsByCode(new NotificationRequest
                        {
                            Code = code,
                            AvailableId = bill.Id,
                            Token = token,
                            UserId = student.UserInformationId
                        }, currentUser);
                    }
                }
            }
        }
        /// <summary>
        /// Thông báo cho học viên khi mua khóa học combo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyStudentSuccessfulByCombo(NotifyStudentSuccessfulByComboRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_student_successful_by_combo;
                var bill = await dbContext.tbl_Bill.SingleOrDefaultAsync(x => x.Id == request.BillId);
                if (bill == null)
                    return;
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == bill.StudentId);
                if (student == null)
                    return;

                var details = await dbContext.tbl_BillDetail.Where(x => x.BillId == bill.Id && x.Enable == true).ToListAsync();
                if (details.Any())
                {
                    foreach (var detail in details)
                    {
                        if (!detail.ComboId.HasValue)
                            continue;
                        var combo = await dbContext.tbl_Combo.SingleOrDefaultAsync(x => x.Id == detail.ComboId);
                        if (combo == null)
                            continue;

                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", student.FullName);
                        token.Add("[ComboName]", combo.Name);
                        token.Add("[StudentId]", student.UserInformationId);
                        token.Add("[Tab]", 5); // Tab thanh toán, Màn hình chi tiết học viên

                        var currentUser = new tbl_UserInformation
                        {
                            FullName = "Tự động"
                        };
                        await NotificationService.SendAllMethodsByCode(new NotificationRequest
                        {
                            Code = code,
                            AvailableId = combo.Id,
                            Token = token,
                            UserId = student.UserInformationId
                        }, currentUser);
                    }
                }
            }
        }
        public static async Task NotifyParentsSuccessfulByCombo(NotifyParentsSuccessfulByComboRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_parents_successful_by_combo;
                var bill = await dbContext.tbl_Bill.SingleOrDefaultAsync(x => x.Id == request.BillId);
                if (bill == null)
                    return;
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == bill.StudentId);
                if (student == null)
                    return;

                var details = await dbContext.tbl_BillDetail.Where(x => x.BillId == bill.Id && x.Enable == true).ToListAsync();
                if (details.Any())
                {
                    foreach (var detail in details)
                    {
                        if (!detail.ComboId.HasValue)
                            continue;
                        var combo = await dbContext.tbl_Combo.SingleOrDefaultAsync(x => x.Id == detail.ComboId);
                        if (combo == null)
                            continue;

                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", student.FullName);
                        token.Add("[ComboName]", combo.Name);
                        token.Add("[StudentId]", student.UserInformationId);
                        token.Add("[Tab]", 5); // Tab thanh toán, Màn hình chi tiết học viên

                        var currentUser = new tbl_UserInformation
                        {
                            FullName = "Tự động"
                        };
                        await NotificationService.SendAllMethodsByCode(new NotificationRequest
                        {
                            Code = code,
                            AvailableId = combo.Id,
                            Token = token,
                            UserId = student.UserInformationId
                        }, currentUser);
                    }
                }
            }
        }

    }
    public class BillNotificationRequest
    {
        public class NotifyStudentClassPlacementRegistrationRequest
        {
            public int BillId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifyStudentSuccessfulClassRegistrationRequest
        { 
            public int BillId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifyParentsSuccessfulClassRegistrationRequest
        {
            public int BillId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifyStudentSuccessfulByComboRequest
        {
            public int BillId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifyParentsSuccessfulByComboRequest
        {
            public int BillId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
    }
}
