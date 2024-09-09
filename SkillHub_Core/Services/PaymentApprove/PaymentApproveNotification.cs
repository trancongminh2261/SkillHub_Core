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
using static LMSCore.Services.PaymentApprove.PaymentApproveNotificationRequest;

namespace LMSCore.Services.PaymentApprove
{
    public class PaymentApproveNotification
    {
        /// <summary>
        /// Gửi thông báo khi tới mới đơn duyệt thanh toán
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifySendAPaymentApprovalRequest(NotifySendAPaymentApprovalRequestRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_send_a_payment_approval_request;
                var paymentApprove = await dbContext.tbl_PaymentApprove.SingleOrDefaultAsync(x => x.Id == request.PaymentApproveId);
                if (paymentApprove == null)
                    return;
                var bill = await dbContext.tbl_Bill.SingleOrDefaultAsync(x => x.Id == paymentApprove.BillId);
                if (bill == null)
                    return;
                string sql = $"Get_UserOption @RoleIds = N'{((int)RoleEnum.academic)},{((int)RoleEnum.manager)}', @BranchIds = N'{bill.BranchId}'";

                var arrives = await dbContext.SqlQuery<UserOption>(sql);
                var admins = await dbContext.tbl_UserInformation.Where(x => x.RoleId == ((int)RoleEnum.admin) && x.Enable == true)
                    .Select(x => new UserOption
                    {
                        UserInformationId = x.UserInformationId,
                        FullName = x.FullName,
                        UserCode = x.UserCode
                    }).ToListAsync();
                if (admins.Any())
                {
                    arrives.AddRange(admins);
                }

                foreach (var item in arrives)
                {
                    var arrive = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.UserInformationId);
                    if (arrive == null)
                        continue;
                    Hashtable token = new Hashtable();
                    token.Add("[ReceiverName]", arrive.FullName);
                    token.Add("[Paid]", String.Format("{0:0,0}", paymentApprove.Money));
                    token.Add("[StaffName]", paymentApprove.CreatedBy);

                    await NotificationService.SendAllMethodsByCode(new NotificationRequest
                    {
                        Code = code,
                        AvailableId = paymentApprove.Id,
                        Token = token,
                        UserId = arrive.UserInformationId
                    }, request.CurrentUser);
                }
            }
        }
        /// <summary>
        /// Gửi thông báo khi đơn thanh toán được duyệt
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyPaymentRequestHasBeenApproved(NotifyPaymentRequestHasBeenApprovedRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_payment_request_has_been_approved;
                var paymentApprove = await dbContext.tbl_PaymentApprove.SingleOrDefaultAsync(x => x.Id == request.PaymentApproveId);
                if (paymentApprove == null)
                    return;
                var receiver = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == paymentApprove.CreateById);

                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", receiver.FullName);
                token.Add("[Paid]", String.Format("{0:0,0}", paymentApprove.Money));
                token.Add("[CreatedOn]", paymentApprove.CreatedOn.HasValue ? "" : paymentApprove.CreatedOn.Value.ToString("HH:mm dd/MM/yyyy"));

                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = paymentApprove.Id,
                    Token = token,
                    UserId = receiver.UserInformationId
                }, request.CurrentUser);
            }
        }
        /// <summary>
        /// Gửi thông báo khi đơn thanh toán bị hủy
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifyPaymentRequestHasBeenCanceled(NotifyPaymentRequestHasBeenCanceledRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_payment_request_has_been_canceled;
                var paymentApprove = await dbContext.tbl_PaymentApprove.SingleOrDefaultAsync(x => x.Id == request.PaymentApproveId);
                if (paymentApprove == null)
                    return;
                var receiver = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == paymentApprove.CreateById);

                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", receiver.FullName);
                token.Add("[Paid]", String.Format("{0:0,0}", paymentApprove.Money));
                token.Add("[CreatedOn]", paymentApprove.CreatedOn.HasValue ? "" : paymentApprove.CreatedOn.Value.ToString("HH:mm dd/MM/yyyy"));

                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = paymentApprove.Id,
                    Token = token,
                    UserId = receiver.UserInformationId
                }, request.CurrentUser);
            }
        }
    }
    public class PaymentApproveNotificationRequest
    {
        public class NotifyPaymentApprove
        {
            public int PaymentApproveId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifySendAPaymentApprovalRequestRequest : NotifyPaymentApprove
        { 
        }
        public class NotifyPaymentRequestHasBeenApprovedRequest : NotifyPaymentApprove
        {
        }
        public class NotifyPaymentRequestHasBeenCanceledRequest : NotifyPaymentApprove
        { 
        }
    }
}
