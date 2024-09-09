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
    public class TutorSalaryService
    {
        public static async Task<tbl_TutorSalary> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TutorSalary.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task InsertOrUpdate(TutorSalaryCreate itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var tutor = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.TutorId);
                if (tutor == null)
                    throw new Exception("Không tìm thấy trợ giảng");
                if (tutor.RoleId != (int)RoleEnum.tutor)
                    throw new Exception("Vui lòng chọn trợ giảng");
                var entity = await db.tbl_TutorSalary.FirstOrDefaultAsync(x => x.TutorId == tutor.UserInformationId && x.Enable == true);
                if (entity == null)
                {
                    entity = new tbl_TutorSalary
                    {
                        CreatedBy = userLog.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = userLog.FullName,
                        ModifiedOn = DateTime.Now,
                        TutorSalaryConfigId = itemModel.TutorSalaryConfigId,
                        TutorId = itemModel.TutorId,
                        Note = itemModel.Note
                    };
                    db.tbl_TutorSalary.Add(entity);
                }
                else
                {
                    entity.TutorSalaryConfigId = itemModel.TutorSalaryConfigId ?? entity.TutorSalaryConfigId;
                    entity.Note = itemModel.Note ?? entity.Note;
                }
                await db.SaveChangesAsync();
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TutorSalary.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(TutorSalarySearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TutorSalarySearch();
                string sql = $"Get_TutorSalary " +
                    $"@Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@TutorSalaryConfig = {baseSearch.TutorSalaryConfig ?? 0}," +
                    $"@PageSize = {baseSearch.PageSize}";
                var data = await db.SqlQuery<Get_TutorSalary>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_TutorSalary(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}