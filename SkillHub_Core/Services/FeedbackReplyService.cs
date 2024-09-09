using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class FeedbackReplyService
    {
        public static async Task<tbl_FeedbackReply> Insert(FeedbackReplyCreate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var model = new tbl_FeedbackReply(itemModel);
                        var feedback = await db.tbl_Feedback.SingleOrDefaultAsync(x => x.Enable == true && x.Id == model.FeedbackId);
                        if (feedback == null)
                            throw new Exception("Phản hồi không tồn tại");
                        //Kiểm tra người reply là ng tạo ra phản hồi có gắn ẩn danh thì ẩn danh luôn reply của người này
                        if (feedback.CreatedIdBy == userLogin.UserInformationId && feedback.IsIncognito == true)
                            model.IsIncognito = true;
                        model.CreatedIdBy = userLogin.UserInformationId;
                        model.CreatedBy = model.ModifiedBy = userLogin.FullName;
                        db.tbl_FeedbackReply.Add(model);
                        await db.SaveChangesAsync();
                        //thông báo rep phản hồi
                        var sendToUser = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == feedback.CreatedIdBy && x.StatusId == (int)AccountStatus.active && x.Enable == true);
                        FeedbackParam  param = new FeedbackParam { FeedbackId = feedback.Id };
                        string paramString = JsonConvert.SerializeObject(param);
                        tbl_Notification notification = new tbl_Notification()
                        {
                            Title = userLogin.FullName+" đã trả lời phản hồi \"" + feedback.Title + "\""+" của bạn",
                            Content = model.Content,
                            UserId = sendToUser.UserInformationId,
                            IsSeen = false,
                            Type = 6,
                            ParamString = paramString,
                            CreatedBy = userLogin.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true
                        };
                        db.tbl_Notification.Add(notification);
                        await db.SaveChangesAsync();
                        //Thread thread = new Thread(() =>
                        //{
                        //    AssetCRM.OneSignalPushNotifications(notification.Title, notification.Content, sendToUser.OneSignal_DeviceId);
                        //    if (sendToUser.IsReceiveMailNotification == true)
                        //        AssetCRM.SendMail(sendToUser.Email, notification.Title, notification.Content);
                        //});
                        //thread.Start();
                        


                        //Thay đổi trạng thái phản hồi khi reply
                        if (feedback.Status == (int)FeedbackStatus.MoiGui)
                        {
                            feedback.Status = (int)FeedbackStatus.DangXuLy;
                            await db.SaveChangesAsync();
                        }
                        tran.Commit();
                        return model;
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
        }

        public static async Task<tbl_FeedbackReply> Update(FeedbackReplyUpdate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_FeedbackReply.SingleOrDefaultAsync(x => x.Enable == true && x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (entity.CreatedIdBy != userLogin.UserInformationId)
                    throw new Exception("Không có quyền thao tác");
                entity.Content = string.IsNullOrEmpty(itemModel.Content) ? entity.Content : itemModel.Content;
                await db.SaveChangesAsync();
                return entity;
            }
        }

        public static async Task Delete(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_FeedbackReply.SingleOrDefaultAsync(x => x.Enable == true && x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (entity.CreatedIdBy != userLogin.UserInformationId && userLogin.RoleId != (int)RoleEnum.admin)
                    throw new Exception("Không có quyền thao tác");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }

        public static async Task<tbl_FeedbackReply> GetById(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_FeedbackReply.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    return null;
                var user = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.CreatedIdBy && x.Enable == true);
                entity.Avatar = user == null ? null : user.Avatar;
                //Kiểm tra cờ ẩn danh nếu có thì ẩn thông tin người tạo reply phản hồi
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

        public static async Task<AppDomainResult> GetAll(FeedbackReplySearch baseSearch, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new FeedbackReplySearch();

                string sql = $"Get_FeedbackReply " +
                    $"@PageIndex = {baseSearch.PageIndex}, " +
                    $"@PageSize = {baseSearch.PageSize}, " +
                    $"@FeedbackId = N'{(baseSearch.FeedbackId == null ? 0 : baseSearch.FeedbackId)}', " +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType == false ? 0 : 1)}'," +
                    $"@AccountLogin = N'{(userLogin.UserInformationId)}'";
                var data = await db.SqlQuery<Get_FeedbackReply>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = data[0].TotalRow ?? 0;
                var result = data.Select(i => new tbl_FeedbackReply(i)).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }

    }
}