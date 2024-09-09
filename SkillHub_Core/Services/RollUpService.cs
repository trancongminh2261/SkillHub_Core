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
    public class RollUpService
    {
        public static async Task<tbl_RollUp> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_RollUp.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_RollUp> InsertOrUpdate(RollUpCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == itemModel.ScheduleId);
                if (schedule == null)
                    throw new Exception("Không tìm thấy buổi học");
                var entity = await db.tbl_RollUp.FirstOrDefaultAsync(x => x.ScheduleId == itemModel.ScheduleId && x.StudentId == itemModel.StudentId && x.Enable == true);
                if (entity == null)
                {
                    entity = new tbl_RollUp(itemModel);
                    entity.ClassId = schedule.ClassId;
                    entity.CreatedBy = entity.ModifiedBy = user.FullName;
                    db.tbl_RollUp.Add(entity);
                }
                else
                {
                    entity.Status = itemModel.Status ?? entity.Status;
                    entity.StatusName = itemModel.StatusName ?? entity.StatusName;
                    entity.LearningStatus = itemModel.LearningStatus ?? entity.LearningStatus;
                    entity.LearningStatusName = itemModel.LearningStatusName ?? entity.LearningStatusName;
                    entity.Note = itemModel.Note ?? entity.Note;
                }
                schedule.TeacherAttendanceId = schedule.TeacherId;//Điểm danh giáo viên
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task<AppDomainResult> GetAll(RollUpSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new RollUpSearch();
                string sql = $"Get_RollUp @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = N'{baseSearch.ClassId}'," +
                    $"@StudentIds = N'{baseSearch.StudentIds ?? ""}'," +
                    $"@ParentIds = N'{baseSearch.ParentIds ?? ""}'," +
                    $"@ScheduleId = N'{baseSearch.ScheduleId}'";
                var data = await db.SqlQuery<Get_RollUp>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new RollUpModel(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<AppDomainResult> GetRollUpStudent(RollUpSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new RollUpSearch();
                string sql = $"Get_RollUpOfStudent @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = N'{baseSearch.ClassId}'," +
                    $"@StudentIds = N'{baseSearch.StudentIds ?? ""}'," +
                    $"@ParentIds = N'{baseSearch.ParentIds ?? ""}'," +
                    $"@ScheduleId = N'{baseSearch.ScheduleId}'";
                var data = await db.SqlQuery<Get_RollUp>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new RollUpModel(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}