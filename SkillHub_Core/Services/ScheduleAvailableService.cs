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
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class ScheduleAvailableService
    {
        public static async Task<tbl_ScheduleAvailable> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_ScheduleAvailable.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_ScheduleAvailable> Insert(ScheduleAvailableCreate itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                if (userLog.RoleId == ((int)RoleEnum.teacher))
                    itemModel.TeacherId = userLog.UserInformationId;
                else
                {
                    var checkTeacher = await db.tbl_UserInformation
                        .AnyAsync(x => x.UserInformationId == itemModel.TeacherId && x.RoleId == ((int)RoleEnum.teacher));
                    if (!checkTeacher)
                        throw new Exception("Không tìm thấy giáo viên");
                }
                if (itemModel.EndTime <= itemModel.StartTime)
                    throw new Exception("Thời gian không phù hợp");

                var model = new tbl_ScheduleAvailable(itemModel);
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                db.tbl_ScheduleAvailable.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_ScheduleAvailable> Update(ScheduleAvailableUpdate itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_ScheduleAvailable.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (userLog.RoleId == ((int)RoleEnum.teacher) && userLog.UserInformationId != entity.TeacherId)
                    throw new Exception("Bạn không được phép chỉnh sửa thông tin này");
                entity.StartTime = itemModel.StartTime ?? entity.StartTime;
                entity.EndTime = itemModel.EndTime ?? entity.EndTime;
                entity.Note = itemModel.Note ?? entity.Note;
                entity.ModifiedBy = userLog.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_ScheduleAvailable.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(ScheduleAvailableSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ScheduleAvailableSearch();
                int studentId = 0;
                if (user.RoleId == ((int)RoleEnum.teacher))
                    baseSearch.TeacherId = user.UserInformationId;
                string sql = $"Get_ScheduleAvailable @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@From = N'{(baseSearch.From.Value.ToString("yyyy-MM-dd HH:mm:ss"))}'," +
                    $"@To = N'{(baseSearch.To.Value.ToString("yyyy-MM-dd HH:mm:ss"))}'," +
                    $"@TeacherId = {baseSearch.TeacherId}";
                var data = await db.SqlQuery<Get_ScheduleAvailable>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_ScheduleAvailable(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}