using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace LMSCore.Services
{
    public class FeedbackService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public FeedbackService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public static async Task<tbl_Feedback> Insert(FeedbackCreate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_Feedback(itemModel);
                model.Status = (int)FeedbackStatus.MoiGui;
                model.CreatedIdBy = userLogin.UserInformationId;
                model.CreatedBy = model.ModifiedBy = userLogin.FullName;
                model.StarRating = 0;
                db.tbl_Feedback.Add(model);
                await db.SaveChangesAsync();
                UrlNotificationModels urlNotificationModels = new UrlNotificationModels();
                var url = urlNotificationModels.urlFeedback;
                //thÔng báo cho học vụ có feedback mới
                List<tbl_UserInformation> userInfor = await db.tbl_UserInformation.Where(x => x.Enable == true && x.BranchIds.Contains(userLogin.BranchIds)).ToListAsync();
                var academics = userInfor.Where(x => x.RoleId == (int)RoleEnum.academic).ToList();
                var managers = userInfor.Where(x => x.RoleId == (int)RoleEnum.manager).ToList();
                //// Gửi thông báo cho học vụ
                //if (academics.Any())
                //{
                //    Thread sendNoti = new Thread(async () =>
                //    {
                //        foreach (var a in academics)
                //        {
                //            FeedbackParam param = new FeedbackParam { FeedbackId = model.Id };
                //            string paramString = JsonConvert.SerializeObject(param);
                //            await NotificationService.Send(new tbl_Notification
                //            {
                //                Title = "Bạn nhận được phản hồi mới",
                //                Content = model.Content,
                //                ContentEmail = model.Content,
                //                UserId = a.UserInformationId,
                //                Type = 6,
                //                ParamString = paramString,
                //                Category = 0,
                //                Url = url
                //            }, userLogin);
                //        }
                //    });
                //    sendNoti.Start();
                //}
                //// Gửi thông báo cho quản lý
                //if (managers.Any())
                //{
                //    Thread sendNoti = new Thread(async () =>
                //    {
                //        foreach (var m in academics)
                //        {
                //            FeedbackParam param = new FeedbackParam { FeedbackId = model.Id };
                //            string paramString = JsonConvert.SerializeObject(param);
                //            await NotificationService.Send(new tbl_Notification
                //            {
                //                Title = "Bạn nhận được phản hồi mới",
                //                Content = model.Content,
                //                ContentEmail = model.Content,
                //                UserId = m.UserInformationId,
                //                Type = 6,
                //                ParamString = paramString,
                //                Category = 0,
                //                Url = url
                //            }, userLogin);
                //        }
                //    });
                //    sendNoti.Start();
                //}
                //// Gửi thông báo cho tư vấn viên của học sinh
                //if (userLogin.SaleId != null && userLogin.SaleId != 0)
                //{
                //    Thread sendNoti = new Thread(async () =>
                //    {
                //        FeedbackParam param = new FeedbackParam { FeedbackId = model.Id };
                //        string paramString = JsonConvert.SerializeObject(param);
                //        await NotificationService.Send(new tbl_Notification
                //        {
                //            Title = "Bạn nhận được phản hồi mới",
                //            Content = model.Content,
                //            ContentEmail = model.Content,
                //            UserId = userLogin.SaleId,
                //            Type = 6,
                //            ParamString = paramString,
                //            Category = 0,
                //            Url = url
                //        }, userLogin);
                //    });
                //    sendNoti.Start();
                //}
                return model;
            }
        }
        public static async Task<tbl_Feedback> Update(FeedbackUpdate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Feedback.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                //Kiểm tra nếu user là người gửi phản hồi thì trạng thái phản hồi phải là mới gửi thì cho phép sửa thông tin
                if (entity.CreatedIdBy == userLogin.UserInformationId && entity.Status != (int)FeedbackStatus.MoiGui)
                    throw new Exception("Phản hồi đang được xử lý, thao tác thất bại");
                entity.Title = string.IsNullOrEmpty(itemModel.Title) ? entity.Title : itemModel.Title == null ? entity.Title : itemModel.Title;
                entity.Content = string.IsNullOrEmpty(itemModel.Content) ? entity.Content : itemModel.Content == null ? entity.Content : itemModel.Content;
                entity.IsIncognito = itemModel.IsIncognito == null ? entity.IsIncognito : itemModel.IsIncognito;
                entity.IsPriority = itemModel.IsPriority == null ? entity.IsPriority : itemModel.IsPriority;
                //Trường hợp đổi trạngg thái
                if (itemModel.Status != null)
                {
                    entity.Status = itemModel.Status;
                }
                await db.SaveChangesAsync();
                return entity;
            }
        }

        public static async Task RatingFeedBack(int id, int startRating, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                string content = "";
                string notificationContent = "";
                string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
                var pathViews = Path.Combine(_hostingEnvironment.ContentRootPath, "Views");
                content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Class/Feedback.cshtml");
                // Thông tin FeedBack
                var entity = await db.tbl_Feedback.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                // Thông tin học viên FeedBack
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.CreatedIdBy);
                // Thông tin chi nhánh
                int branchId = int.Parse(student.BranchIds);
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == branchId);
                if (entity == null)
                    throw new Exception("Không tìm thấy thông tin phản hồi");
                if (branch == null)
                    throw new Exception("Không tìm thấy thông tin chi nhánh");
                if (student == null)
                    throw new Exception("Không tìm thấy thông tin người đánh giá");
                if (entity.IsRated == true)
                {
                    throw new Exception("Phản hồi này đã được đánh giá rồi");
                }
                if (startRating > 0)
                {
                    entity.IsRated = true;
                }
                if (startRating <= 2)
                {
                    //UrlNotificationModels urlNotification = new UrlNotificationModels();
                    //string url = urlNotification.urlDetailFeedBack + id;
                    //string urlEmail = urlNotification.url + url;

                    //content = content.Replace("{today}", DateTime.Now.ToString("dd/MM/yyyy"));
                    //content = content.Replace("{BranchName}", branch.Name);
                    //if (entity.IsIncognito == true)
                    //    content = content.Replace("{FullName}", "Ẩn danh");
                    //else content = content.Replace("{FullName}", entity.CreatedBy);
                    //content = content.Replace("{StartRating}", startRating.ToString());
                    //content = content.Replace("{Title}", entity.Title);
                    //content = content.Replace("{Content}", entity.Content);
                    //content = content.Replace("{ProjectName}", projectName);
                    //content = content.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");
                    //notificationContent = @"<div>" + content + @"</div>";

                    //// Thông báo cho admin
                    //List<tbl_UserInformation> admins = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.admin).ToListAsync();
                    //Thread sendAdmin = new Thread(async () =>
                    //{

                    //    tbl_Notification notification = new tbl_Notification();

                    //    notification.Title = "ĐÁNH GIÁ PHẢN HỒI";
                    //    notification.Content = "Hệ thống nhận được đánh giá " + startRating + " sao với nội dung: " + entity.Content + ". Vui lòng kiểm tra";
                    //    notification.ContentEmail = notificationContent;
                    //    notification.Type = 6;
                    //    notification.Category = 0;
                    //    notification.Url = url;
                    //    notification.AvailableId = id;
                    //    foreach (var ad in admins)
                    //    {
                    //        notification.UserId = ad.UserInformationId;
                    //        await NotificationService.Send(notification, userLogin, true);
                    //    }
                    //});
                    //sendAdmin.Start();

                    //// Thông báo cho quản lý
                    //List<tbl_UserInformation> managers = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.manager && x.BranchIds.Contains(branchId.ToString())).ToListAsync();
                    //Thread sendManager = new Thread(async () =>
                    //{

                    //    tbl_Notification notification = new tbl_Notification();

                    //    notification.Title = "ĐÁNH GIÁ PHẢN HỒI";
                    //    notification.Content = "Hệ thống nhận được đánh giá " + startRating + " sao với nội dung: " + entity.Content + " .Vui lòng kiểm tra";
                    //    notification.ContentEmail = notificationContent;
                    //    notification.Type = 6;
                    //    notification.Category = 0;
                    //    notification.Url = url;
                    //    notification.AvailableId = id;
                    //    foreach (var manager in managers)
                    //    {
                    //        notification.UserId = manager.UserInformationId;
                    //        await NotificationService.Send(notification, userLogin, true);
                    //    }
                    //});
                    //sendManager.Start();
                }
                ////Phản hồi phải ở trạng thái xử lý đã xong mới cho học viên đánh giá
                //if (entity.CreatedIdBy == userLogin.UserInformationId && entity.Status != (int)FeedbackStatus.DaXong)
                //    throw new Exception("Đánh giá thất bại");
                entity.StarRating = startRating;
                await db.SaveChangesAsync();
            }
        }

        public static async Task<tbl_Feedback> GetById(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Feedback.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    return null;
                var user = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.CreatedIdBy && x.Enable == true);
                entity.Avatar = user == null ? null : user.Avatar;
                //Kiểm tra cờ ẩn danh nếu có thì ẩn thông tin người tạo phản hồi
                if (entity.IsIncognito == true && entity.CreatedIdBy != userLogin.UserInformationId && userLogin.RoleId != (int)RoleEnum.admin)
                {
                    entity.Avatar = null;
                    entity.CreatedIdBy = null;
                    entity.CreatedBy = null;
                    entity.ModifiedBy = null;
                }
                return entity;
            }
        }

        public static async Task<AppDomainResult> GetAll(FeedbackSearch baseSearch, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new FeedbackSearch();

                string userIds = "";
                if (userLogin.RoleId == ((int)RoleEnum.student))
                    userIds = userLogin.UserInformationId.ToString();
                else
                    userIds = baseSearch.UserIds ?? "";

                string branchIds = (
                            userLogin.RoleId == ((int)RoleEnum.admin)
                            || userLogin.RoleId == ((int)RoleEnum.student)
                            || userLogin.RoleId == ((int)RoleEnum.parents)
                            || userLogin.RoleId == ((int)RoleEnum.sale)
                        ) ? "" : userLogin.BranchIds;
                string sql = $"Get_Feedback @Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}, " +
                    $"@PageSize = {baseSearch.PageSize}, " +
                    $"@Status = N'{(baseSearch.Status == null ? 0 : baseSearch.Status)}', " +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType == false ? 0 : 1)}'," +
                    $"@SaleId = N'{(userLogin.RoleId == ((int)RoleEnum.sale) ? userLogin.UserInformationId : 0)}'," +
                    $"@UserIds = N'{userIds}', " +
                    $"@BranchIds = N'{branchIds}'";
                var data = await db.SqlQuery<Get_Feedback>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Feedback(i)).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }

        }
        public static async Task<AppDomainResult> GetAllV2(FeedbackV2Search baseSearch, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new FeedbackV2Search();
                //var BranchIds = baseSearch.BranchIds ?? "";
                //if (userLogin.RoleId != ((int)RoleEnum.admin))
                //    BranchIds = userLogin.BranchIds;

                string userIds = "";
                if (userLogin.RoleId == ((int)RoleEnum.student))
                    userIds = userLogin.UserInformationId.ToString();
                else
                    userIds = baseSearch.UserIds ?? "";

                var SaleId = userLogin.RoleId == ((int)RoleEnum.sale) ? userLogin.UserInformationId : 0;
                string sql = $"Get_Feedback @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Search = N'{baseSearch.Search ?? ""}'," +
                    $"@Status = {baseSearch.Status ?? 0}," +
                    $"@Sort = {baseSearch.Sort}," +
                    $"@SortType = {(baseSearch.SortType == false ? 0 : 1)}," +
                    $"@SaleId = {SaleId}," +
                    $"@UserIds = N'{baseSearch.UserIds ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'";
                var data = await db.SqlQuery<Get_Feedback>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Feedback(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        public static async Task<int> GetFeedbackInProcess(string branchIds, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    int userId = 0; 
                    if (userLogin.RoleId == (int)RoleEnum.student)
                        userId = userLogin.UserInformationId;
                    var SaleId = userLogin.RoleId == ((int)RoleEnum.sale) ? userLogin.UserInformationId : 0;
                    string sql = $"Get_FeedbackInProcess @BranchIds = N'{branchIds ?? ""}'," +
                        $"@SaleId = {SaleId}," +
                        $"@UserId = {userId}";
                    var data = await db.SqlQuery<Get_Feedback>(sql);
                    var count = data.Count();
                    return count;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

            }
        }
    }
}