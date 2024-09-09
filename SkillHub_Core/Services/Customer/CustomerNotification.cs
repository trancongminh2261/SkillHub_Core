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

namespace LMSCore.Services.Customer
{
    public class CustomerNotification
    {
        /// <summary>
        /// Gửi thông báo cho tư vấn viên khi có khách hàng mới
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifySaleNewCustomer(NotifySaleNewCustomerRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_sale_new_customer;
                var customer = await dbContext.tbl_Customer.SingleOrDefaultAsync(x => x.Id == request.CustomerId);
                if (customer == null)
                    return;
                var sale = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == customer.SaleId);
                if (sale == null)
                    return;
                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", sale.FullName);
                token.Add("[CustomerName]", customer.FullName);
                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = customer.Id,
                    Token = token,
                    UserId = sale.UserInformationId
                }, request.CurrentUser);
            }
        }
        /// <summary>
        /// Gửi thông báo cho tư vấn viên khi khách hàng của họ bị người khác thay đổi trạng thái
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task NotifySaleChangeCustomerStatus(NotifySaleChangeCustomerStatusRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_sale_change_customer_status;
                var customer = await dbContext.tbl_Customer.SingleOrDefaultAsync(x => x.Id == request.CustomerId);
                if (customer == null)
                    return;
                var sale = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == customer.SaleId);
                if (sale == null)
                    return;
                var status = await dbContext.tbl_CustomerStatus.SingleOrDefaultAsync(x => x.Id == customer.CustomerStatusId);
                if (status == null)
                    return;

                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", sale.FullName);
                token.Add("[CustomerName]", customer.FullName);
                token.Add("[StatusName]", status.Name);

                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = customer.Id,
                    Token = token,
                    UserId = sale.UserInformationId
                }, request.CurrentUser);
            }
        }
        /// <summary>
        /// Gửi thông báo cho tư vấn viên khi nhận được danh sách khách hàng
        /// </summary>
        /// <returns></returns>
        public static async Task NotifySaleReceivedTheCustomerList(NotifySaleReceivedTheCustomerListRequest request)
        {
            using (var dbContext = new lmsDbContext())
            {
                string code = NotificationCode.notify_sale_received_the_customer_list;
                var sale = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == request.SaleId);
                if (sale == null)
                    return;

                if (request.Amount == 0)
                    return;

                Hashtable token = new Hashtable();
                token.Add("[ReceiverName]", sale.FullName);
                token.Add("[Amount]", request.Amount);

                await NotificationService.SendAllMethodsByCode(new NotificationRequest
                {
                    Code = code,
                    AvailableId = 0,
                    Token = token,
                    UserId = sale.UserInformationId
                }, request.CurrentUser);
            }
        }
    }
    public class CustomerNotificationRequest
    {
        public class NotifySaleChangeCustomerStatusRequest
        {
            public int CustomerId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifySaleNewCustomerRequest
        {
            public int CustomerId { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
        public class NotifySaleReceivedTheCustomerListRequest
        {
            public int SaleId { get; set; }
            public int Amount { get; set; }
            public tbl_UserInformation CurrentUser { get; set; }
        }
    }
}
