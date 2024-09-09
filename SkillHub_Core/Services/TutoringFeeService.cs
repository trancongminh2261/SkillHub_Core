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
    public class TutoringFeeService
    {
        public static async Task<tbl_TutoringFee> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TutoringFee.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task InsertOrUpdate(TutoringFeeCreate itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var teacher = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.TeacherId);
                if (teacher == null)
                    throw new Exception("Không tìm thấy giáo viên");
                var entity = await db.tbl_TutoringFee.FirstOrDefaultAsync(x => x.TeacherId == teacher.UserInformationId && x.Enable == true);
                if (entity == null)
                {
                    entity = new tbl_TutoringFee
                    {
                        CreatedBy = userLog.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        Fee = itemModel.Fee ?? 0,
                        ModifiedBy = userLog.FullName,
                        ModifiedOn = DateTime.Now,
                        Note = itemModel.Note,
                        TeacherId = itemModel.TeacherId
                    };
                    db.tbl_TutoringFee.Add(entity);
                }
                else
                {
                    entity.Fee = itemModel.Fee ?? entity.Fee;
                    entity.Note = itemModel.Note ?? entity.Note;
                }
                await db.SaveChangesAsync();
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TutoringFee.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SearchOptions();
                string sql = $"Get_TutoringFee @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}";
                var data = await db.SqlQuery<Get_TutoringFee>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_TutoringFee(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public class Get_TeacherAvailable_TutoringFee
        {
            public int TeacherId { get; set; }
            public string TeacherName { get; set; }
            public string TeacherCode { get; set; }
            public string TeacherAvatar { get; set; }
        }
        public static async Task<List<Get_TeacherAvailable_TutoringFee>> GetTeacherAvailable()
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_TeacherAvailable_TutoringFee";
                var data = await db.SqlQuery<Get_TeacherAvailable_TutoringFee>(sql);
                return data;
            }
        }
    }
}