using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static LMSCore.Models.lmsEnum;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Hangfire;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSCore.NotificationConfig;

namespace LMSCore.Services
{
    public class NotificationInClassService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public NotificationInClassService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public static async Task<tbl_NotificationInClass> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_NotificationInClass.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_NotificationInClass> Insert(NotificationInClassCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {

                if (string.IsNullOrEmpty(itemModel.Title)) throw new Exception("Tiêu đề thông báo không được để trống");
                if (string.IsNullOrEmpty(itemModel.Content)) throw new Exception("Nội dung thông báo không được để trống");

                var _class = await db.tbl_Class
                    .FirstOrDefaultAsync(x => x.Id == itemModel.ClassId);
                var model = new tbl_NotificationInClass(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_NotificationInClass.Add(model);
                await db.SaveChangesAsync();

                var studentInClass = await db.tbl_StudentInClass.Where(x => x.ClassId == itemModel.ClassId && x.StudentId.HasValue).ToListAsync();
                if (studentInClass.Any())
                {
                    BackgroundJob.Schedule(() => NotificationService.SendAllMethods(new NotificationWithContentRequest
                    {
                        PushOneSignal = true,
                        SendMail = itemModel.IsSendMail,
                        Content = itemModel.Content,
                        Title = itemModel.Title,
                        UserIds = studentInClass.Select(x=>x.StudentId.Value).ToList()
                    }, user), TimeSpan.FromSeconds(2));
                }

                return model;
            }
        }
        public static async Task<AppDomainResult> GetAll(NotificationInClassSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new NotificationInClassSearch();

                var l = await db.tbl_NotificationInClass.Where(x => x.Enable == true && x.ClassId == baseSearch.ClassId).OrderByDescending(x => x.Id).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };

                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
    }
}