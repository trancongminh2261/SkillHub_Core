using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;
using LMSCore.NotificationConfig;
using System.Collections;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using LMSCore.Utilities;

namespace LMSCore.Services
{
    public class NotificationService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        /// <summary>
        /// Xác nhận xem
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task Seen(int id, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_Notification.SingleOrDefaultAsync(x => x.Id == id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy thông báo");
                    entity.IsSeen = true;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        /// <summary>
        /// Xác nhận xem tất cả
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task SeenAll(tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var notifications = await db.tbl_Notification
                    .Where(x => x.UserId == user.UserInformationId && x.Enable == true && x.IsSeen == false)
                    .Select(x => x.Id).ToListAsync();
                if (notifications.Any())
                {
                    foreach (var item in notifications)
                    {
                        var notification = await db.tbl_Notification.SingleOrDefaultAsync(x => x.Id == item);
                        notification.IsSeen = true;
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
        public static async Task<AppDomainResult> GetAll(SearchOptions search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (search == null) return new AppDomainResult { TotalRow = 0, Data = null };
                string sql = $"Get_Notification @PageIndex = {search.PageIndex}," +
                    $"@PageSize = {search.PageSize}," +
                    $"@UserId = N'{user.UserInformationId}'";
                var data = await db.SqlQuery<Get_Notification>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Notification(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<int> UnreadNotificationCount(tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                return await db.tbl_Notification
                    .Where(x => x.UserId == user.UserInformationId && x.IsSeen == false)
                    .CountAsync();
            }
        }
        /// <summary>
        /// Gửi thông báo bằng tất cả dịch vụ nếu có cấu hình
        /// Lưu ý token là tất cả dữ liệu động của Title, Content,URL
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public static async Task SendAllMethodsByCode(NotificationRequest itemModel, tbl_UserInformation currentUser)
        {
            try
            {
                using (var dbContext = new lmsDbContext())
                {
                    var receiver = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.UserId);
                    if (receiver == null)
                        return;
                    string domain = configuration.GetSection("MySettings:DomainFE").Value.ToString();
                    string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
                    itemModel.Token.Add("[Domain]", domain);
                    itemModel.Token.Add("[ProjectName]", projectName);

                    var env = WebHostEnvironment.Environment;
                    var pathNotificationConfig = Path.Combine(env.ContentRootPath, "NotificationConfig");
                    string jsonString = System.IO.File.ReadAllText($"{pathNotificationConfig}/NotificationConfigVN.json");
                    var config = JsonSerializer.Deserialize<List<NotificationConfigModel>>(jsonString)
                        .FirstOrDefault(x=> x.Code == itemModel.Code);
                    if (config == null)
                        return;
                    if (!string.IsNullOrEmpty(config.Title))
                    {
                        dbContext.tbl_Notification.Add(new tbl_Notification
                        {
                            Code = itemModel.Code,
                            AvailableId = itemModel.AvailableId,
                            Category = config.Category,
                            Content = GenerateText(config.Content, itemModel.Token),
                            CreatedBy = currentUser.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true,
                            IsSeen = false,
                            ModifiedBy = currentUser.FullName,
                            ModifiedOn = DateTime.Now,
                            ParamString = null,
                            Title = GenerateText(config.Title, itemModel.Token),
                            Type = config.Type,
                            Url = GenerateText(config.Url, itemModel.Token),
                            UserId = receiver.UserInformationId
                        });
                        await dbContext.SaveChangesAsync();
                    }
                    if (!string.IsNullOrEmpty(config.OnesignalTitle))
                    {
                        config.OnesignalTitle = GenerateText(config.OnesignalTitle, itemModel.Token);
                        config.OnesignalContent = GenerateText(config.OnesignalContent, itemModel.Token);
                        AssetCRM.OneSignalPushNotifications( new OneSignalRequest
                        { 
                            Headings = config.OnesignalTitle,
                            Content = config.OnesignalContent,
                            PlayerId = receiver.OneSignal_DeviceId,
                            Url = config.Url
                        });
                    }
                    if (!string.IsNullOrEmpty(config.EmailContentFileName))
                    {
                        string contentEmail = System.IO.File.ReadAllText($"{pathNotificationConfig}/EmailTemplate/VN/{config.EmailContentFileName}");
                        contentEmail = GenerateText(contentEmail, itemModel.Token);
                        config.EmailTitle = GenerateText(config.EmailTitle, itemModel.Token);
                        AssetCRM.SendMail(receiver.Email, config.EmailTitle, contentEmail);
                    }
                }
            }
            catch
            {
                //gắng thêm writelog
                return;
            }
        }
        /// <summary>
        /// Chỉ dùng cho thông báo tạo trực tiếp từ hệ thống
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public static async Task SendAllMethods(NotificationWithContentRequest itemModel, tbl_UserInformation currentUser)
        {
            try
            {
                using (var dbContext = new lmsDbContext())
                {
                    if (!itemModel.UserIds.Any())
                        return;

                    string domain = configuration.GetSection("MySettings:DomainFE").Value.ToString();
                    foreach (var item in itemModel.UserIds)
                    {
                        var receiver = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item);
                        if (receiver == null)
                            return;

                        dbContext.tbl_Notification.Add(new tbl_Notification
                        {
                            Code = "",
                            AvailableId = 0,
                            Category = 0,
                            Content = itemModel.Content,
                            CreatedBy = currentUser.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true,
                            IsSeen = false,
                            ModifiedBy = currentUser.FullName,
                            ModifiedOn = DateTime.Now,
                            ParamString = null,
                            Title = itemModel.Title,
                            Type = 0,
                            Url = domain,
                            UserId = receiver.UserInformationId
                        });
                        await dbContext.SaveChangesAsync();

                        if (itemModel.PushOneSignal && !string.IsNullOrEmpty(receiver.OneSignal_DeviceId))
                        {
                            AssetCRM.OneSignalPushNotifications(new OneSignalRequest
                            {
                                Headings = itemModel.Title,
                                Content = itemModel.Content,
                                Url = domain
                            });
                        }
                        if (itemModel.SendMail && !string.IsNullOrEmpty(receiver.Email))
                        {
                            AssetCRM.SendMail(receiver.Email, itemModel.Title, itemModel.Content);
                        }
                    }
                }
            }
            catch
            {
                //gắng thêm writelog
                return;
            }
        }
        public static string GenerateText(string content, Hashtable token)
        {
            foreach (DictionaryEntry entry in token)
            {
                content = content.Replace(entry.Key.ToString(),entry.Value.ToString());
            }
            return content;
        }
    }
}