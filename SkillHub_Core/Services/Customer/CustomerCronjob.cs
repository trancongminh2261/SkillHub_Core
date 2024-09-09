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
namespace LMSCore.Services.Customer
{
    public class CustomerCronjob
    {
        //Rảnh tối ưu lại
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        //public static async Task CheckVisitors()
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        DateTime today = DateTime.Now;
        //        DateTime tomorrow = today.AddDays(1);
        //        DateTime yesterday = today.AddDays(-1);
        //        string nullItem = "Chưa có";
        //        string title = "";
        //        int countToday = 1;
        //        int countYesterday = 1;
        //        var row = "";
        //        // Tìm khách sẽ đến hôm nay
        //        var customersVisitedToday = await db.tbl_TestAppointment.Where(x => x.Type == 1 && x.Enable == true).ToListAsync();
        //        customersVisitedToday = customersVisitedToday.Where(x => x.Time?.Date == today.Date).ToList();
        //        // Tìm khách đã đến hôm qua
        //        var customersVisitedYesterday = await db.tbl_TestAppointment.Where(x => x.Type == 1 && x.Enable == true).ToListAsync();
        //        customersVisitedYesterday = customersVisitedYesterday.Where(x => x.Time?.Date == yesterday.Date).ToList();
        //        List<tbl_UserInformation> admins = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.admin).ToListAsync();

        //        var dataCustomersVisitedToday = new List<CustomerVistor>();
        //        var dataCustomersVisitedYesterday = new List<CustomerVistor>();

        //        string contentToday = "";
        //        string contentYesterday = "";
        //        string noneContentToday = "";
        //        string noneContentYesterday = "";
        //        string notificationContentToday = "";
        //        string notificationContentYesterday = "";
        //        string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
        //        var appRootPath = WebHostEnvironment.Environment;
        //        var pathViews = Path.Combine(appRootPath.ContentRootPath, "Views");
        //        contentToday = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Customer/CustomerVisitor.cshtml");
        //        contentYesterday = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Customer/CustomerVisitor.cshtml");
        //        noneContentToday = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Customer/NoneCustomerVisitor.cshtml");
        //        noneContentYesterday = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Customer/NoneCustomerVisitor.cshtml");
        //        var builderToday = new StringBuilder();
        //        var builderYesterday = new StringBuilder();
        //        UrlNotificationModels urlNotification = new UrlNotificationModels();
        //        string url = urlNotification.urlTestAppointment;
        //        string urlEmail = urlNotification.url + url;
        //        // Tìm thông tin khách đến hôm nay
        //        if (customersVisitedToday != null && customersVisitedToday.Count != 0)
        //        {
        //            foreach (var customerToday in customersVisitedToday)
        //            {
        //                var c = new CustomerVistor();

        //                var customer = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == customerToday.StudentId && x.Enable == true);
        //                var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == customerToday.BranchId && x.Enable == true);
        //                var teacher = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == customerToday.TeacherId && x.Enable == true);
        //                c.FullName = customer.FullName;
        //                c.Code = customer.UserCode;
        //                c.BranchName = branch.Name;
        //                c.Time = (DateTime)customerToday.Time;
        //                c.LearningStatusName = customerToday.LearningStatusName;
        //                if (teacher == null) c.TeacherName = nullItem;
        //                else c.TeacherName = teacher.FullName;
        //                c.CreatedOn = customerToday.CreatedOn;
        //                c.CreatedBy = customerToday.CreatedBy;
        //                dataCustomersVisitedToday.Add(c);
        //            }


        //            if (dataCustomersVisitedToday != null && dataCustomersVisitedToday.Count != 0)
        //            {
        //                foreach (var customer in dataCustomersVisitedToday)
        //                {
        //                    if (countToday == 1)
        //                    {
        //                        contentToday = contentToday.Replace("{item0}", "DANH SÁCH KHÁCH HÀNG SẼ ĐẾN HÔM NAY");
        //                        contentToday = contentToday.Replace("{item1}", today.ToString("dd/MM/yyyy"));
        //                        contentToday = contentToday.Replace("{item2}", customer.Code);
        //                        contentToday = contentToday.Replace("{item3}", customer.FullName);
        //                        contentToday = contentToday.Replace("{item4}", customer.BranchName);
        //                        contentToday = contentToday.Replace("{item5}", customer.Time.ToString("dd/MM/yyyy HH:mm tt"));
        //                        if (customer.TeacherName != null) contentToday = contentToday.Replace("{item6}", customer.TeacherName.ToString());
        //                        else contentToday = contentToday.Replace("{item6}", nullItem);
        //                        contentToday = contentToday.Replace("{item7}", customer.CreatedBy);
        //                    }
        //                    else
        //                    {
        //                        row = "<tr>";
        //                        row += $"<td class='cusTableData'>{customer.Code}</td>";
        //                        row += $"<td class='cusTableData'>{customer.FullName}</td>";
        //                        row += $"<td class='cusTableData'>{customer.BranchName}</td>";
        //                        row += $"<td class='cusTableData'>{customer.Time.ToString("dd/MM/yyyy HH:mm tt")}</td>";
        //                        if (customer.TeacherName != null)
        //                        {
        //                            row += $"<td class='cusTableData'>{customer.TeacherName}</td>";
        //                        }
        //                        else
        //                        {
        //                            row += $"<td class='cusTableData'>{nullItem}</td>";
        //                        }
        //                        row += $"<td class='cusTableData'>{customer.CreatedBy}</td>";
        //                        row += "</tr>";

        //                        builderToday.Append(row);
        //                    }
        //                    countToday++;
        //                }

        //                contentToday = contentToday.Replace("{RL}", builderToday.ToString());
        //                //Chữ kí
        //                contentToday = contentToday.Replace("{item8}", projectName);
        //                contentToday = contentToday.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

        //                notificationContentToday = @"<div>" + contentToday + @"</div>";

        //                // Gửi thông báo của hôm nay nếu có khách hàng
        //                Thread sendToday = new Thread(async () =>
        //                {
        //                    foreach (var ad in admins)
        //                    {
        //                        tbl_Notification notification = new tbl_Notification();

        //                        notification.Title = "THÔNG BÁO DANH SÁCH KHÁCH HÀNG ĐẾN TRUNG TÂM NGÀY " + today.ToString("dd/MM/yyyy");
        //                        notification.ContentEmail = notificationContentToday;
        //                        notification.Content = "Hôm nay có " + dataCustomersVisitedToday.Count + " khách hàng đến trung tâm";
        //                        notification.Type = 1;
        //                        notification.Category = 2;
        //                        notification.Url = url;
        //                        notification.UserId = ad.UserInformationId;
        //                        await NotificationService.Send(notification, ad, true);
        //                    }
        //                });
        //                sendToday.Start();
        //            }
        //        }
        //        else
        //        {
        //            // Gửi thông báo của hôm nay nếu có không khách hàng
        //            noneContentToday = noneContentToday.Replace("{Title}", "Thông Báo Về Sự Vắng Mặt Của Khách Hàng");
        //            noneContentToday = noneContentToday.Replace("{Day}", "hôm nay");
        //            noneContentToday = noneContentToday.Replace("{ProjectName}", projectName);
        //            noneContentToday = noneContentToday.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

        //            notificationContentToday = @"<div>" + noneContentToday + @"</div>";

        //            Thread sendToday = new Thread(async () =>
        //            {
        //                foreach (var ad in admins)
        //                {
        //                    tbl_Notification notification = new tbl_Notification();

        //                    notification.Title = "THÔNG BÁO VỀ SỰ VẮNG MẶT CỦA KHÁCH HÀNG NGÀY " + today.ToString("dd/MM/yyyy");
        //                    notification.ContentEmail = notificationContentToday;
        //                    notification.Content = "Hôm nay không có khách hàng đến trung tâm";
        //                    notification.Type = 1;
        //                    notification.Category = 2;
        //                    notification.Url = url;
        //                    notification.UserId = ad.UserInformationId;
        //                    await NotificationService.Send(notification, ad, true);
        //                }
        //            });
        //            sendToday.Start();
        //        }

        //        // Tìm thông tin khách hàng đến hôm qua
        //        if (customersVisitedYesterday != null && customersVisitedYesterday.Count != 0)
        //        {
        //            foreach (var customerYesterday in customersVisitedYesterday)
        //            {
        //                var c = new CustomerVistor();

        //                var customer = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == customerYesterday.StudentId && x.Enable == true);
        //                var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == customerYesterday.BranchId && x.Enable == true);
        //                var teacher = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == customerYesterday.TeacherId && x.Enable == true);
        //                c.FullName = customer.FullName;
        //                c.Code = customer.UserCode;
        //                c.BranchName = branch.Name;
        //                c.Time = (DateTime)customerYesterday.Time;
        //                c.LearningStatusName = customerYesterday.LearningStatusName;
        //                if (teacher == null) c.TeacherName = nullItem;
        //                else c.TeacherName = teacher.FullName;
        //                c.CreatedOn = customerYesterday.CreatedOn;
        //                c.CreatedBy = customerYesterday.CreatedBy;
        //                dataCustomersVisitedYesterday.Add(c);
        //            }

        //            if (dataCustomersVisitedYesterday != null && dataCustomersVisitedYesterday.Count != 0)
        //            {
        //                foreach (var customer in dataCustomersVisitedYesterday)
        //                {
        //                    if (countYesterday == 1)
        //                    {
        //                        contentYesterday = contentYesterday.Replace("{item0}", "DANH SÁCH KHÁCH HÀNG ĐÃ ĐẾN HÔM QUA");
        //                        contentYesterday = contentYesterday.Replace("{item1}", today.ToString("dd/MM/yyyy"));
        //                        contentYesterday = contentYesterday.Replace("{item2}", customer.Code);
        //                        contentYesterday = contentYesterday.Replace("{item3}", customer.FullName);
        //                        contentYesterday = contentYesterday.Replace("{item4}", customer.BranchName);
        //                        contentYesterday = contentYesterday.Replace("{item5}", customer.Time.ToString("dd/MM/yyyy HH:mm tt"));
        //                        if (customer.TeacherName != null) contentYesterday = contentYesterday.Replace("{item6}", customer.TeacherName.ToString());
        //                        else contentYesterday = contentYesterday.Replace("{item6}", nullItem);
        //                        contentYesterday = contentYesterday.Replace("{item7}", customer.CreatedBy);
        //                    }
        //                    else
        //                    {
        //                        row = "<tr>";
        //                        row += $"<td class='cusTableData'>{customer.Code}</td>";
        //                        row += $"<td class='cusTableData'>{customer.FullName}</td>";
        //                        row += $"<td class='cusTableData'>{customer.BranchName}</td>";
        //                        row += $"<td class='cusTableData'>{customer.Time.ToString("dd/MM/yyyy HH:mm tt")}</td>";
        //                        if (customer.TeacherName != null)
        //                        {
        //                            row += $"<td class='cusTableData'>{customer.TeacherName}</td>";
        //                        }
        //                        else
        //                        {
        //                            row += $"<td class='cusTableData'>{nullItem}</td>";
        //                        }
        //                        row += $"<td class='cusTableData'>{customer.CreatedBy}</td>";
        //                        row += "</tr>";

        //                        builderYesterday.Append(row);
        //                    }
        //                    countYesterday++;
        //                }
        //                contentYesterday = contentYesterday.Replace("{RL}", builderYesterday.ToString());
        //                //Chữ kí
        //                contentYesterday = contentYesterday.Replace("{item8}", projectName);
        //                contentYesterday = contentYesterday.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");
        //                notificationContentYesterday = @"<div>" + contentYesterday + @"</div>";

        //                // Gửi thông báo danh sách khách hàng đến hôm qua và nếu có khách hàng
        //                Thread sendYesterday = new Thread(async () =>
        //                {
        //                    tbl_Notification notification = new tbl_Notification();

        //                    notification.Title = "THÔNG BÁO DANH SÁCH KHÁCH HÀNG ĐẾN TRUNG TÂM NGÀY " + yesterday.ToString("dd/MM/yyyy");
        //                    notification.ContentEmail = notificationContentYesterday;
        //                    notification.Content = "Hôm qua có " + dataCustomersVisitedYesterday.Count + " khách hàng đến trung tâm";
        //                    notification.Type = 1;
        //                    notification.Category = 2;
        //                    notification.Url = url;
        //                    foreach (var ad in admins)
        //                    {
        //                        notification.UserId = ad.UserInformationId;
        //                        await NotificationService.Send(notification, ad, true);
        //                    }
        //                });
        //                sendYesterday.Start();
        //            }
        //        }
        //        else
        //        {
        //            // Gửi thông báo danh sách khách hàng đến hôm qua và nếu không có khách hàng
        //            noneContentYesterday = noneContentYesterday.Replace("{Title}", "Thông Báo Về Sự Vắng Mặt Của Khách Hàng");
        //            noneContentYesterday = noneContentYesterday.Replace("{Day}", "hôm qua");
        //            noneContentYesterday = noneContentYesterday.Replace("{ProjectName}", projectName);
        //            noneContentYesterday = noneContentYesterday.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");
        //            notificationContentYesterday = @"<div>" + noneContentYesterday + @"</div>";

        //            Thread sendYesterday = new Thread(async () =>
        //            {
        //                foreach (var ad in admins)
        //                {
        //                    tbl_Notification notification = new tbl_Notification();

        //                    notification.Title = "THÔNG BÁO VỀ SỰ VẮNG MẶT CỦA KHÁCH HÀNG NGÀY " + yesterday.ToString("dd/MM/yyyy");
        //                    notification.ContentEmail = notificationContentYesterday;
        //                    notification.Content = "Hôm qua không có khách hàng đến trung tâm";
        //                    notification.Type = 1;
        //                    notification.Category = 2;
        //                    notification.Url = url;
        //                    notification.UserId = ad.UserInformationId;
        //                    await NotificationService.Send(notification, ad, true);
        //                }
        //            });
        //            sendYesterday.Start();
        //        }
        //    }
        //}
        //public static async Task CheckNewLeads()
        //{
        //    DateTime today = DateTime.Now;
        //    DateTime yesterday = today.AddDays(-1);

        //    string content = "";
        //    string noneContent = "";
        //    string notificationContent = "";
        //    string nullItem = "Chưa có";
        //    int count = 1;
        //    var a = new StringBuilder();
        //    string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
        //    var appRootPath = WebHostEnvironment.Environment.ContentRootPath;
        //    var pathViews = Path.Combine(appRootPath, "Views");
        //    content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Customer/NewLeads.cshtml");
        //    noneContent = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Customer/NoneCustomerVisitor.cshtml");

        //    using (var db = new lmsDbContext())
        //    {
        //        var leads = await db.tbl_Customer.Where(x => x.Enable == true).ToListAsync();
        //        leads = leads.Where(x => x.CreatedOn?.Date == yesterday.Date).ToList();
        //        List<tbl_UserInformation> admins = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.admin).ToListAsync();
        //        UrlNotificationModels urlNotification = new UrlNotificationModels();
        //        string url = urlNotification.urlLeads;
        //        string urlEmail = urlNotification.url + url;
        //        if (leads != null && leads.Count != 0)
        //        {
        //            foreach (var data in leads)
        //            {
        //                // Thông tin chi nhánh
        //                var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == data.BranchId);
        //                // Thông tin tư vấn viên
        //                var sale = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == data.SaleId);
        //                // Thông tin nguồn
        //                var source = await db.tbl_Source.FirstOrDefaultAsync(x => x.Id == data.SourceId);
        //                // Thông tin trạng thái khách hàng
        //                var customerStatus = await db.tbl_CustomerStatus.FirstOrDefaultAsync(x => x.Id == data.CustomerStatusId);
        //                if (count == 1)
        //                {
        //                    content = content.Replace("{Yesterday}", yesterday.ToString("dd/MM/yyyy"));
        //                    if (branch == null) content = content.Replace("{BranchName}", nullItem);
        //                    else content = content.Replace("{BranchName}", branch.Name);
        //                    content = content.Replace("{UserCode}", data.Code);
        //                    content = content.Replace("{UserName}", data.FullName);
        //                    content = content.Replace("{Phone}", data.Mobile);
        //                    content = content.Replace("{Email}", data.Email);
        //                    content = content.Replace("{CustomerStatus}", customerStatus.Name);
        //                    if (sale == null) content = content.Replace("{SaleName}", nullItem);
        //                    else content = content.Replace("{SaleName}", sale.FullName);
        //                    if (source == null) content = content.Replace("{SourceName}", nullItem);
        //                    else content = content.Replace("{SourceName}", source.Name);
        //                    content = content.Replace("{CreatedBy}", data.CreatedBy);
        //                    content = content.Replace("{CreatedOn}", data.CreatedOn.Value.ToString("dd/MM/yyyy"));
        //                }
        //                else
        //                {
        //                    string row = "<tr>";
        //                    if (branch == null) row += $"<td class='data-cell'>{nullItem}</td>";
        //                    else row += $"<td class='data-cell'>{branch.Name}</td>";
        //                    row += $"<td class='data-cell'> <div>Mã: <strong>{data.Code}</strong></div> <div>Tên KH: {data.FullName}</div> </td>";
        //                    row += $"<td class='data-cell'> <div>Số điện thoại: {data.Mobile}</div> <div>Email: {data.Email}</td>";
        //                    row += $"<td class='data-cell'>{customerStatus.Name}</td>";
        //                    if (sale == null) row += $"<td class='data-cell'>{nullItem}</td>";
        //                    else row += $"<td class='data-cell'>{sale.FullName}</td>";
        //                    if (source == null) row += $"<td class='data-cell'>{nullItem}</td>";
        //                    else row += $"<td class='data-cell'>{source.Name}</td>";
        //                    row += $"<td class='data-cell'> <div>Người tạo: {data.CreatedBy}</div> <div>Ngày tạo: {data.CreatedOn.Value.ToString("dd/MM/yyyy")}</div> </td>";
        //                    row += "</tr>";

        //                    a.Append(row);
        //                }
        //                count++;
        //            }
        //            content = content.Replace("{RL}", a.ToString());
        //            //Chữ kí
        //            content = content.Replace("{ProjectName}", projectName);
        //            content = content.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");
        //            notificationContent = @"<div>" + content + @"</div>";
        //        }
        //        else
        //        {
        //            noneContent = noneContent.Replace("{Title}", "Thông Báo Về Sự Vắng Mặt Của Khách Hàng Tiềm Năng");
        //            noneContent = noneContent.Replace("{Day}", "hôm qua");
        //            noneContent = noneContent.Replace("{ProjectName}", projectName);
        //            noneContent = noneContent.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");
        //            notificationContent = @"<div>" + noneContent + @"</div>";
        //        }

        //        Thread send = new Thread(async () =>
        //        {
        //            foreach (var ad in admins)
        //            {
        //                tbl_Notification notification = new tbl_Notification();

        //                notification.Title = "THÔNG BÁO DANH SÁCH KHÁCH HÀNG TIỀM NĂNG ĐƯỢC TẠO NGÀY " + yesterday.ToString("dd/MM/yyyy");
        //                notification.ContentEmail = notificationContent;
        //                if (leads != null && leads.Count != 0) notification.Content = "Hôm qua có " + leads.Count + " khách hàng tiềm năng được tạo!";
        //                else notification.Content = "Hôm qua không có khách hàng tiềm năng được tạo!";
        //                notification.Type = 1;
        //                notification.Category = 2;
        //                notification.Url = url;
        //                notification.UserId = ad.UserInformationId;
        //                await NotificationService.Send(notification, ad, true);
        //            }
        //        });
        //        send.Start();
        //    }
        //}

        public static async Task AutoUpdateCustomerStatus()
        {
            using (var db = new lmsDbContext())
            {
                var customers = await db.tbl_Customer.Where(x => x.Enable == true).ToListAsync();
                var userInformation = await db.tbl_UserInformation.Where(x => x.Enable == true && x.CustomerId != 0).ToListAsync();
                var studentInClass = await db.tbl_StudentInClass.Where(x => x.Enable == true).ToListAsync();
                var customerStatus = await db.tbl_CustomerStatus.Where(x => x.Enable == true).ToListAsync();
                foreach (var c in customers)
                {
                    var customer = userInformation.Where(x => x.CustomerId == c.Id).OrderByDescending(x => x.ModifiedOn).FirstOrDefault();
                    if (customerStatus.Count != 0)
                    {
                        if (customer != null)
                        {
                            var statusTest= customerStatus.FirstOrDefault(x => x.Type == 3);
                            var statusLearning = customerStatus.FirstOrDefault(x => x.Type == 4);
                            var statusStopLearning = customerStatus.FirstOrDefault(x => x.Type == 5);
                            if (customer.LearningStatus == 3) // Không học
                            {
                                c.CustomerStatusId = statusStopLearning.Id;
                                c.CustomerStatusName = statusStopLearning.Name;
                            }
                            else if (customer.LearningStatus == 5) // Đang học
                            {
                                c.CustomerStatusId = statusLearning.Id;
                                c.CustomerStatusName = statusLearning.Name;
                            }else if(customer.LearningStatus != 3 && customer.LearningStatus != 5 && customer.LearningStatus != 6)
                            {
                                c.CustomerStatusId = statusTest.Id;
                                c.CustomerStatusName = statusTest.Name;
                            }
                            else
                            {
                                var customerInClass = studentInClass.Any(x => x.StudentId == customer.UserInformationId && x.Enable == true);
                                if (customerInClass)
                                {
                                    c.CustomerStatusId = statusLearning.Id;
                                    c.CustomerStatusName = statusLearning.Name;
                                }
                            }
                        }
                    }
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
