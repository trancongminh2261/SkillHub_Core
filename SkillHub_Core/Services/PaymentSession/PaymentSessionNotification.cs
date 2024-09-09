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
namespace LMSCore.Services.PaymentSession
{
    public class PaymentSessionNotification
    {
        /// <summary>
        /// Tự động thông báo cho học viên khi đến hẹn thanh toán
        /// </summary>
        /// <returns></returns>
        public static async Task NotifyStudentPaymentDue()
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_student_payment_due;
                var today = DateTime.Now.Date;
                var bills = await dbContext.tbl_Bill
                    .Where(x => x.PaymentAppointmentDate.HasValue && x.Enable == true && x.Debt > 0
                    && x.PaymentAppointmentDate.Value.Date == today).ToListAsync();
                if (bills.Any())
                {
                    foreach (var item in bills)
                    {
                        var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                        if (student == null)
                            continue;
                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", student.FullName);
                        token.Add("[Debt]", String.Format("{0:0,0}", item.Debt));
                        token.Add("[PaymentAppointmentDate]", item.PaymentAppointmentDate.Value.ToString("HH:mm dd/MM/yyyy"));
                        token.Add("[StudentId]", student.UserInformationId);

                        var currentUser = new tbl_UserInformation
                        {
                            FullName = "Tự động"
                        };
                        await NotificationService.SendAllMethodsByCode(new NotificationRequest
                        {
                            Code = code,
                            AvailableId = item.Id,
                            Token = token,
                            UserId = student.UserInformationId
                        }, currentUser);
                    }
                }

            }
        }
        /// <summary>
        /// Gửi thông báo cho phụ huynh khi có khoản thu mới
        /// </summary>
        /// <returns></returns>
        public static async Task NotifyParentsPaymentDue()
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_parents_payment_due;
                var today = DateTime.Now.Date;
                var bills = await dbContext.tbl_Bill
                    .Where(x => x.PaymentAppointmentDate.HasValue && x.Enable == true && x.Debt > 0
                    && x.PaymentAppointmentDate.Value.Date == today).ToListAsync();
                if (bills.Any())
                {
                    foreach (var item in bills)
                    {
                        var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                        if (student == null)
                            continue;
                        var parent = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.ParentId);
                        if (parent == null)
                            continue;
                        Hashtable token = new Hashtable();
                        token.Add("[ReceiverName]", parent.FullName);
                        token.Add("[Debt]", String.Format("{0:0,0}", item.Debt));
                        token.Add("[PaymentAppointmentDate]", item.PaymentAppointmentDate.Value.ToString("HH:mm dd/MM/yyyy"));
                        token.Add("[StudentId]", student.UserInformationId);
                        token.Add("[StudentName]", student.FullName);

                        var currentUser = new tbl_UserInformation
                        {
                            FullName = "Tự động"
                        };
                        await NotificationService.SendAllMethodsByCode(new NotificationRequest
                        {
                            Code = code,
                            AvailableId = item.Id,
                            Token = token,
                            UserId = parent.UserInformationId
                        }, currentUser);
                    }
                }
            }
        }
    }
}
