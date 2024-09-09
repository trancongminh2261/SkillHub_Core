using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using Org.BouncyCastle.Math.EC;
using System.Drawing.Printing;

namespace LMSCore.Services
{
    public class SpendingConfigService : DomainService
    {
        public SpendingConfigService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_SpendingConfig> GetById(int id)
        {
            return await dbContext.tbl_SpendingConfig.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task Validate(tbl_SpendingConfig model)
        {
            var isBranch = await dbContext.tbl_Branch.SingleOrDefaultAsync(x => x.Id == model.BranchId && x.Enable == true)
                ?? throw new Exception("Không tìm thấy chi nhánh");
        }
        public async Task<tbl_SpendingConfig> Insert(SpendingConfigCreate itemModel, tbl_UserInformation user)
        {
            var entity = new tbl_SpendingConfig(itemModel);
            await Validate(entity);
            entity.Active = true;
            entity.CreatedBy = entity.ModifiedBy = user.FullName;
            dbContext.tbl_SpendingConfig.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<tbl_SpendingConfig> Update(SpendingConfigUpdate itemModel, tbl_UserInformation user)
        {
            var entity = await dbContext.tbl_SpendingConfig.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
            if (entity == null)
                throw new Exception("Khoản chi không tồn tại");
            entity.Name = itemModel.Name ?? entity.Name;
            entity.Description = itemModel.Description ?? entity.Description;
            entity.Active = itemModel.Active ?? entity.Active;
            entity.Description = itemModel.Description ?? entity.Description;
            entity.ModifiedBy = user.FullName;
            entity.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task Delete(int id)
        {
            var entity = await dbContext.tbl_SpendingConfig.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (entity == null)
                throw new Exception("Khoản chi không tồn tại");
            entity.Enable = false;
            await dbContext.SaveChangesAsync();
        }
        public async Task<AppDomainResult> GetAll(SpendingConfigSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new SpendingConfigSearch();
            var pg = await dbContext.tbl_SpendingConfig.Where(x => x.Enable == true
            && (string.IsNullOrEmpty(baseSearch.Search) || x.Name.Contains(baseSearch.Search))
            && x.BranchId == baseSearch.BranchId
            && (baseSearch.Active == null || x.Active == baseSearch.Active))
                .OrderByDescending(x => x.CreatedOn).Select(x => x.Id).ToListAsync();

            if (!pg.Any())
                return new AppDomainResult() { TotalRow = 0, Data = null };
            int totalRow = pg.Count();
            pg = pg.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
            var data = (from i in pg
                        select Task.Run(() => GetById(i)).Result).ToList();
            return new AppDomainResult() { TotalRow = totalRow, Data = data };
        }
        public async Task<List<tbl_SpendingConfig>> GetDropDown(DropdownSpendingConfig baseSearch)
        {
            return await dbContext.tbl_SpendingConfig.Where(x => x.Enable == true
            && x.BranchId == baseSearch.BranchId
            && x.Active)
                .OrderByDescending(x => x.CreatedOn).ToListAsync();
        }
    }
}