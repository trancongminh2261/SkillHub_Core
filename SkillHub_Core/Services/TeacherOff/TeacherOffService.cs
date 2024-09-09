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
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Hangfire;

namespace LMSCore.Services.TeacherOff
{
    public class TeacherOffService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public TeacherOffService(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public static async Task<tbl_TeacherOff> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TeacherOff.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_TeacherOff> Insert(TeacherOffCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (!itemModel.StartTime.HasValue || !itemModel.EndTime.HasValue)
                    throw new Exception("Vui lòng chọn thời gian đăng ký nghỉ");
                if (itemModel.EndTime <= itemModel.StartTime)
                    throw new Exception("Thời gian đăng ký không phù hợp");
                var model = new tbl_TeacherOff(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                model.TeacherId = user.UserInformationId;
                db.tbl_TeacherOff.Add(model);
                await db.SaveChangesAsync();

                BackgroundJob.Schedule(() => TeacherOffNotification.NotifyManagerTeacherRequestsTimeOff(new TeacherOffNotificationRequest.NotifyManagerTeacherRequestsTimeOffRequest
                {
                    TeacherOffId = model.Id,
                    CurrentUser = user,
                }), TimeSpan.FromSeconds(2));

                return model;
            }
        }
        public class ArriveModel
        {
            public int UserId { get; set; }
            public string FullName { get; set; }
        }
        public static async Task<tbl_TeacherOff> Update(TeacherOffUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TeacherOff.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");

                entity.Status = itemModel.Status ?? entity.Status;
                entity.StatusName = itemModel.StatusName ?? entity.StatusName;
                entity.Note = itemModel.Note ?? entity.Note;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                if (entity.Status == 2)
                {
                    BackgroundJob.Schedule(() => TeacherOffNotification.NotifyTeacherLeaveRequestHasBeenApproved(new TeacherOffNotificationRequest.NotifyTeacherLeaveRequestHasBeenApprovedRequest
                    {
                        TeacherOffId = entity.Id,
                        CurrentUser = user,
                    }), TimeSpan.FromSeconds(2));
                }
                else if (entity.Status == 3)
                { 
                    BackgroundJob.Schedule(() => TeacherOffNotification.NotifyTeacherLeaveRequestHasBeenCanceled(new TeacherOffNotificationRequest.NotifyTeacherLeaveRequestHasBeenCanceledRequest
                    {
                        TeacherOffId = entity.Id,
                        CurrentUser = user,
                    }), TimeSpan.FromSeconds(2));
                }
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TeacherOff.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(TeacherOffSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TeacherOffSearch();
                int teacherId = 0;
                if (user.RoleId == (int)RoleEnum.teacher)
                    teacherId = user.UserInformationId;

                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;

                string sql = $"Get_TeacherOff @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds}'," +
                    $"@TeacherId = {teacherId}," +
                    $"@Status = N'{baseSearch.Status ?? ""}'";
                var data = await db.SqlQuery<Get_TeacherOff>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_TeacherOff(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        public static async Task<AppDomainResult> GetAllV2(TeacherOffV2Search baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TeacherOffV2Search();
                int teacherId = 0;
                if (user.RoleId == (int)RoleEnum.teacher)
                    teacherId = user.UserInformationId;

                string myBranchIds = baseSearch.BranchIds ?? "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;

                string sql = $"Get_TeacherOff @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds}'," +
                    $"@TeacherId = {teacherId}," +
                    $"@FromDate = '{baseSearch.FromDate}'," +
                    $"@ToDate = '{baseSearch.ToDate}'," +
                    $"@Status = N'{baseSearch.Status ?? ""}'";
                var data = await db.SqlQuery<Get_TeacherOff>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_TeacherOff(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        public static async Task<AppDomainResult> GetScheduleTeacherOff(ScheduleTeacherOffSearch search)
        {
            using (var db = new lmsDbContext())
            {
                var teacherOff = await db.tbl_TeacherOff.SingleOrDefaultAsync(x => x.Id == search.TeacherOffId);
                if (teacherOff == null)
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                string sql = $"Get_Schedule @Search = '', @PageIndex = {search.PageIndex}," +
                    $"@PageSize = {search.PageSize}," +
                    $"@TeacherIds = {teacherOff.TeacherId}," +
                    $"@From = N'{(!teacherOff.StartTime.HasValue ? "" : teacherOff.StartTime.Value.ToString("yyyy-MM-dd HH:mm:ss"))}'," +
                    $"@To = N'{(!teacherOff.EndTime.HasValue ? "" : teacherOff.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss"))}'";
                var data = await db.SqlQuery<Get_Schedule>(sql);

                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Schedule(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}