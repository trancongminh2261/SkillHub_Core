using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.BranchDTO;
using LMSCore.DTO.PopupConfigDTO;
using LMSCore.Models;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services.PopupConfig
{
    public class PopupConfigService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public PopupConfigService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Hàm check
        private static async Task<tbl_PopupConfig> CheckTime(DateTime sDate, DateTime eDate, int id, string branchId, double durating)
        {
            using (var db = new lmsDbContext())
            {
                var successMDate = await db.tbl_PopupConfig.FirstOrDefaultAsync(x =>
                        ((
                        x.STime <= sDate
                        && x.ETime >= sDate
                        ) || (
                        x.STime <= eDate
                        && x.ETime >= eDate
                        ))
                        && x.Id != id
                        && ("," + x.BranchIds + ",").Contains("," + branchId + ",")
                        && x.Durating == durating
                        && x.Enable == true);
                if (successMDate != null)
                    return successMDate;
                var successEDate = await db.tbl_PopupConfig.FirstOrDefaultAsync(x =>
                        ((
                        sDate <= x.STime
                        && eDate >= x.STime
                        ) || (
                        sDate <= x.ETime
                        && eDate >= x.ETime
                        ))
                        && x.Id != id
                        && ("," + x.BranchIds + ",").Contains("," + branchId + ",")
                        && x.Durating == durating
                        && x.Enable == true);
                if (successEDate != null)
                    return successEDate;
                return null;
            }
        }
        #endregion

        public static async Task<GetPopupConfigDTO> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_PopupConfig.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                {
                    var result = new GetPopupConfigDTO(data);
                    result.Branches = await GetDataConfig.GetBranches(data.BranchIds);
                    return result;
                }
                else
                    return null;
            }
        }

        public static async Task<AppDomainResult> GetAll(PopupConfigSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new PopupConfigSearch();
                string sql = $"Get_PopupConfig " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranchId = '{baseSearch.BranchId}'," +
                    $"@Search = N'{baseSearch.Search}'";
                var data = await db.SqlQuery<GetPopupConfigDTO>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                foreach (var d in data)
                    d.Branches = await GetDataConfig.GetBranches(d.BranchIds);
                return new AppDomainResult { TotalRow = totalRow, Data = data };
            }
        }

        public static async Task<AppDomainResult> GetPopupCurrent(tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (user.RoleId != (int)lmsEnum.RoleEnum.student)
                    return new AppDomainResult { TotalRow = 0, Data = null };
                var branchId = string.Empty;
                if (!string.IsNullOrEmpty(user.BranchIds))
                    branchId = user.BranchIds.Split(',')[0];
                var now = GetDateTime.Now;
                string sql = $"Get_PopupConfigCurrent " +
                    $"@BranchId = '{branchId}'," +
                    $"@Now = '{now}'";
                var data = await db.SqlQuery<GetPopupConfigDTO>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                foreach (var d in data)
                    d.Branches = await GetDataConfig.GetBranches(d.BranchIds);
                return new AppDomainResult { TotalRow = totalRow, Data = data };
            }
        }

        public static async Task<GetPopupConfigDTO> Insert(PopupConfigCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (!(itemModel.STime <= itemModel.ETime))
                    throw new Exception("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc");
                var model = new tbl_PopupConfig(itemModel);
                var branches = new List<GetBranchesDTO>();
                if (!String.IsNullOrEmpty(itemModel.BranchIds))
                {
                    var branchIds = itemModel.BranchIds.Split(',');
                    foreach (var branchId in branchIds)
                    {
                        var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id.ToString() == branchId && x.Enable == true);
                        if (branch == null)
                            throw new Exception("Chi nhánh không tồn tại");
                        branches.Add(new GetBranchesDTO() { Id = branch.Id, Name = branch.Name });
                    }
                }
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_PopupConfig.Add(model);
                await db.SaveChangesAsync();
                var popup = new GetPopupConfigDTO(model);
                popup.Branches = branches;
                return popup;
            }
        }

        public static async Task<GetPopupConfigDTO> Update(PopupConfigUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                
                var entity = await db.tbl_PopupConfig.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.BranchIds = itemModel.BranchIds ?? entity.BranchIds;
                var branches = new List<GetBranchesDTO>();
                if (!String.IsNullOrEmpty(entity.BranchIds))
                {
                    var branchIds = entity.BranchIds.Split(',');
                    foreach (var branchId in branchIds)
                    {
                        var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id.ToString() == branchId && x.Enable == true);
                        if (branch == null)
                            throw new Exception("Chi nhánh không tồn tại");
                        branches.Add(new GetBranchesDTO() { Id = branch.Id, Name = branch.Name });
                    }
                }
                entity.Name = itemModel.Name ?? entity.Name;
                entity.Content = itemModel.Content ?? entity.Content;
                entity.STime = itemModel.STime ?? entity.STime;
                entity.ETime = itemModel.ETime ?? entity.ETime;
                entity.Durating = itemModel.Durating ?? entity.Durating;
                entity.Url = itemModel.Url ?? entity.Url;
                entity.IsShow = itemModel.IsShow ?? entity.IsShow;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                var popup = new GetPopupConfigDTO(entity);
                popup.Branches = branches;
                return popup;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_PopupConfig.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
    }
}
