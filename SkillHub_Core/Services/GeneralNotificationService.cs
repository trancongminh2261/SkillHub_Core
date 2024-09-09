using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;
using Hangfire;
using LMSCore.NotificationConfig;

namespace LMSCore.Services
{
    public class GeneralNotificationService
    {
        public static async Task<tbl_GeneralNotification> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_GeneralNotification.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_GeneralNotification> Insert(GeneralNotificationCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_GeneralNotification(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_GeneralNotification.Add(model);
                await db.SaveChangesAsync();
                var userIds = model.UserIds.Split(',').ToList();
                var listUserId = new List<int>();
                userIds.ForEach(x =>
                {
                    int userId = 0;
                    int.TryParse(x, out userId);
                    if (userId != 0)
                        listUserId.Add(userId);
                });

                BackgroundJob.Schedule(() => NotificationService.SendAllMethods(new NotificationWithContentRequest
                {
                    PushOneSignal = true,
                    SendMail = itemModel.IsSendMail,
                    Content = itemModel.Content,
                    Title = itemModel.Title,
                    UserIds = listUserId
                },user), TimeSpan.FromSeconds(2));

                return model;
            }
        }

        public static async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SearchOptions();

                var l = await db.tbl_GeneralNotification.Where(x => x.Enable == true).OrderByDescending(x => x.Id).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };

                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
        public class ReceiverModel
        {
            public int UserInformationId { get; set; }
            public string FullName { get; set; }
            public string UserCode { get; set; }
            public string Avatar { get; set; }
        }
        public static async Task<List<ReceiverModel>> GetReceiver(int id)
        {
            using (var db = new lmsDbContext())
            {
                var generalNotification = await db.tbl_GeneralNotification.SingleOrDefaultAsync(x => x.Id == id);
                if (generalNotification == null) return new List<ReceiverModel>(); 
                string sql = $"Get_Receiver " +
                     $"@UserIds = N'{generalNotification.UserIds ?? ""}'";
                var result = await db.SqlQuery<ReceiverModel>(sql);
                return result;
            }
        }
    }
}