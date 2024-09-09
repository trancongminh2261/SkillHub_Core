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
    public class SalaryConfigService
    {
        public static async Task<tbl_SalaryConfig> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_SalaryConfig.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_SalaryConfig> InsertOrUpdate(SalaryConfigCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var userInformation = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.UserId);
                if (userInformation == null)
                    throw new Exception("Không tìm thấy nhân viên");
                var entity = await db.tbl_SalaryConfig.FirstOrDefaultAsync(x => x.UserId == itemModel.UserId && x.Enable == true);
                if (entity == null)
                {
                    entity = new tbl_SalaryConfig(itemModel);
                    entity.CreatedBy = entity.ModifiedBy = user.FullName;
                    db.tbl_SalaryConfig.Add(entity);
                }
                else
                {
                    entity.Value = itemModel.Value ?? entity.Value;
                    entity.Note = itemModel.Note ?? entity.Note;
                }
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_SalaryConfig.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(SalaryConfigSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SalaryConfigSearch();
                string sql = $"Get_SalaryConfig @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'";
                var data = await db.SqlQuery<Get_SalaryConfig>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_SalaryConfig(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<AppDomainResult> GetAllV2(SalaryConfigV2Search baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SalaryConfigV2Search();
                string sql = $"Get_SalaryConfigV2 @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranchIds = '{baseSearch.BranchIds ?? ""}'," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'";
                var data = await db.SqlQuery<Get_SalaryConfig>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_SalaryConfig(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public class Get_UserAvailable_SalaryConfig
        {
            public int UserInformationId { get; set; }
            public string FullName { get; set; }
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public string UserCode { get; set; }
        }
        public static async Task<List<Get_UserAvailable_SalaryConfig>> GetUserAvailable()
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_UserAvailable_SalaryConfig";
                var data = await db.SqlQuery<Get_UserAvailable_SalaryConfig>(sql);
                return data;
            }
        }
        public static async Task<List<Get_UserAvailable_SalaryConfig>> GetUserAvailableV2(string BranchIds)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_UserAvailable_SalaryConfigV2 " +
                    $"@BranchIds = '{BranchIds ?? ""}'";
                var data = await db.SqlQuery<Get_UserAvailable_SalaryConfig>(sql);
                return data;
            }
        }
    }
}