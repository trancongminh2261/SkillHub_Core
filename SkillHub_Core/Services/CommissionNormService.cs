using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class CommissionNormService : DomainService
    {
        public CommissionNormService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_CommissionNorm> GetById(int id)
        {
            var data = await dbContext.tbl_CommissionNorm.SingleOrDefaultAsync(x => x.Id == id);
            return data;
        }
        public async Task<AppDomainResult> GetAll(CommissionNormSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new CommissionNormSearch();
            string sql = $"Get_CommissionNorm @PageIndex = {baseSearch.PageIndex}," +
                        $"@PageSize = {baseSearch.PageSize}," +
                        $"@Search = N'{baseSearch.Search ?? ""}'," +
                        $"@Name = N'{baseSearch.Name ?? ""}' ";
            var data = await dbContext.SqlQuery<Get_CommissionNormSearch>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }

        public async Task<AppDomainResult> GetCommisionConfig(CommissionConfigSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CommissionConfigSearch();
                string sql = $"Get_CommissionConfig @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'";
                var data = await db.SqlQuery<GetAll_CommisionConfig>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                List<Get_CommisionConfig> result = data.GroupBy(x => new { x.Id, x.SalesId, x.Name,x.FullName,x.UserCode,x.RoleId,x.RoleName, x.Descriptions, x.CreatedBy,x.CreatedOn }).Select(i => new Get_CommisionConfig
                {
                    Id = i.Key.Id,
                    SalesId = i.Key.SalesId,
                    Name=i.Key.Name,
                    Descriptions=i.Key.Descriptions,
                    FullName = i.Key.FullName,
                    UserCode = i.Key.UserCode,
                    RoleId = i.Key.RoleId,
                    RoleName = i.Key.RoleName,
                    CreatedOn = i.Key.CreatedOn,
                    CreatedBy=i.Key.CreatedBy,
                    Get_CommissionNorm= i.GroupBy(b=>new
                    {
                        b.CommissionNormId,b.CommissionConfigId,b.MinNorm,b.MaxNorm,b.NormName,b.PercentNew,b.PercentRenewals,
                    }).Select(c =>new Get_CommissionNorm
                    {
                        CommissionNormId = c.Key.CommissionNormId,
                        CommissionConfigId = c.Key.CommissionConfigId,
                        MinNorm=c.Key.MinNorm,
                        MaxNorm=c.Key.MaxNorm,
                        NormName = c.Key.NormName,
                        PercentNew=c.Key.PercentNew,
                        PercentRenewals=c.Key.PercentRenewals
                    }).ToList(),
                }).ToList();
                //var result = data.Select(i => new tbl_SalaryConfig(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public async Task Insert(CommissionConfigCreate CommissionConfig, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var jtem = new tbl_CommissionConfig(CommissionConfig);
                    var sales =await dbContext.tbl_UserInformation.Where(x => x.UserInformationId == jtem.SalesId && x.Enable == true && x.RoleId == (int)RoleEnum.sale).SingleOrDefaultAsync();
                    if(sales==null)
                        throw new Exception("không tìm thấy tư vấn viên!");
                    jtem.CreatedBy = jtem.ModifiedBy = userLog.FullName;
                    var data = dbContext.tbl_CommissionConfig.Add(jtem);
                    await dbContext.SaveChangesAsync();
                    if (jtem.Id != 0)
                    {
                        var item = CommissionConfig.CommissionNormCreate;
                        foreach (var itemModel in item)
                        {
                            if (itemModel.MinNorm >= itemModel.MaxNorm || itemModel.MinNorm < 0)
                                throw new Exception("Định mức không hợp lý");
                            var model = new tbl_CommissionNorm(itemModel);
                            model.CommissionConfigId = jtem.Id;
                            model.CreatedBy = model.ModifiedBy = userLog.FullName;
                            dbContext.tbl_CommissionNorm.Add(model);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    else
                        throw new Exception("Đã có lỗi xảy ra");                                 
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw e;
                }
            }
        }


        public async Task Update(CommissionConfigUpdate CommissionConfig, tbl_UserInformation user)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var jtem = new tbl_CommissionConfig(CommissionConfig);
                    var item = CommissionConfig.CommissionNormUpdate;                    
                    var commission = await dbContext.tbl_CommissionConfig.SingleOrDefaultAsync(x => x.Id == jtem.Id);
                    if(commission == null)
                        throw new Exception("Không tìm thấy cấu hình hoa hồng");
                    commission.Name = jtem.Name;
                    commission.Descriptions = jtem.Descriptions;
                    commission.CreatedBy = commission.ModifiedBy = user.FullName;
                    await dbContext.SaveChangesAsync();
                    foreach (var itemModel in item)
                    {
                        if (itemModel.Id != 0)
                        {
                            var entity = await dbContext.tbl_CommissionNorm.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                            if (entity == null)
                                throw new Exception("Không tìm thấy chuẩn hoa hồng");
                            if (itemModel.MinNorm >= itemModel.MaxNorm || itemModel.MinNorm < 0)
                                throw new Exception("Định mức không hợp lý");
                            var model = new tbl_CommissionNorm(itemModel);
                            entity.Name = itemModel.Name ?? entity.Name;
                            entity.MinNorm = itemModel.MinNorm ?? entity.MinNorm;
                            entity.MaxNorm = itemModel.MaxNorm ?? entity.MaxNorm;
                            entity.PercentNew = itemModel.PercentNew ?? entity.PercentNew;
                            entity.ModifiedBy = user.FullName;
                            entity.ModifiedOn = DateTime.Now;                          
                        }
                        else
                        {
                            if (itemModel.MinNorm >= itemModel.MaxNorm || itemModel.MinNorm < 0)
                                throw new Exception("Định mức không hợp lý");
                            var model = new tbl_CommissionNorm(itemModel);
                            model.CommissionConfigId = commission.Id;
                            model.CreatedBy = model.ModifiedBy = user.FullName;
                            model.Enable = true;
                            model.CreatedOn = DateTime.Now;
                            dbContext.tbl_CommissionNorm.Add(model);
                        }
                        await dbContext.SaveChangesAsync();                     
                    }
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw e;
                }
            }
        }
        public async Task Delete(int id)
        {
            try
            {
                var entity = await dbContext.tbl_CommissionNorm.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy chuẩn hoa hồng");
                entity.Enable = false;
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task DeleteCommissionConfig(int id)
        {
            try
            {
                var entity = await dbContext.tbl_CommissionConfig.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy cấu hình hoa hồng");
                entity.Enable = false;
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}