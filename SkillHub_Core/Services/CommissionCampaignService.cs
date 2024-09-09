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
    public class CommissionCampaignService : DomainService
    {
        public CommissionCampaignService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_CommissionCampaign> GetById(int id)
        {
            return await dbContext.tbl_CommissionCampaign.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<AppDomainResult> GetAll(CommissionCampaignSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new CommissionCampaignSearch();
            string sql = $"Get_RevenueSales @PageIndex = {baseSearch.PageIndex}," +
                        $"@PageSize = {baseSearch.PageSize}," +                      
                        $"@Year = {baseSearch.Year}," +
                        $"@Month = {baseSearch.Month},"+
                         $"@Search = N'{baseSearch.Search ?? ""}'," +
                        $"@FullName = N'{baseSearch.FullName ??""}'," +
                        $"@UserCode = N'{baseSearch.UserCode ??""}'";
            var data = await dbContext.SqlQuery<Get_CommissionCampaignSearch>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            //var result = data.Select(i => new tbl_CommissionCampaign(i)).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }

        // tạo Tự động
        public static async Task InsertAuto()
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        DateTime time = DateTime.Now;
                        int year = time.Year;
                        int month = time.Month;
                        //var check = await db.tbl_Salary.AnyAsync(x => x.Year == year && x.Month == month && x.Enable == true);
                        //if (check)
                        //    throw new Exception($"Đã tính lương tháng {month} năm {year}");

                        var staffs = await db.tbl_UserInformation
                            .Where(x => x.Enable == true && x.StatusId == ((int)AccountStatus.active) && x.RoleId == (int)RoleEnum.sale)
                            .Select(x => x.UserInformationId)
                            .ToListAsync();
                        if (staffs.Any())
                        {
                            foreach (var item in staffs)
                            {
                                var check = await db.tbl_CommissionCampaign.AnyAsync(x => x.Year == year && x.Month == month && x.Enable == true && x.SalesId == item);
                                if (check)
                                    continue; // Nhân viên này đã có trong chiến dịch
                                var commissionCampaign = new tbl_CommissionCampaign
                                {
                                    SalesId = item,
                                    CreatedBy = "Tự động",
                                    CreatedOn = DateTime.Now,
                                    Enable = true,
                                    ModifiedBy =  "Tự động",
                                    ModifiedOn = DateTime.Now,
                                    Month = month,
                                    Year = year,
                                };
                                db.tbl_CommissionCampaign.Add(commissionCampaign);
                                await db.SaveChangesAsync();
                            }
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
        }
        public async Task<tbl_CommissionCampaign> Insert(CommissionCampaignCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var staffs = await db.tbl_UserInformation
                        .Where(x => x.Enable == true && x.StatusId == ((int)AccountStatus.active) && x.RoleId == (int)RoleEnum.sale && x.UserInformationId == itemModel.SalesId)
                        .FirstOrDefaultAsync();
                    if(staffs ==null)
                        throw new Exception("không tìm thấy tư vấn viên!");
                    else 
                    {
                        var check = await db.tbl_CommissionCampaign.AnyAsync(x => x.Year == itemModel.Year && x.Month == itemModel.Month && x.Enable == true && x.SalesId == itemModel.SalesId);
                        if (check)
                            throw new Exception("Tư vấn viên này đã có trong chiến dịch!");
                        var commissionCampaign = new tbl_CommissionCampaign
                        {
                            SalesId = itemModel.SalesId,
                            RevenueTargets = itemModel.RevenueTargets,
                            CreatedBy = user.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true,
                            ModifiedBy = user.FullName,
                            ModifiedOn = DateTime.Now,
                            Month = itemModel.Month,
                            Year = itemModel.Year,
                        };
                        db.tbl_CommissionCampaign.Add(commissionCampaign);
                        await db.SaveChangesAsync();
                        return commissionCampaign;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public async Task<tbl_CommissionCampaign> Update(CommissionCampaignUpdate itemModel, tbl_UserInformation user)
        {
            try
            {
                var entity = await dbContext.tbl_CommissionCampaign.SingleOrDefaultAsync
                         (x => x.Id == itemModel.Id
                         && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy chiến dịch hoa hồng");
                //---------------------------------------------------------
                //entity.Norm = itemModel.Norm ?? entity.Norm;
                entity.RevenueTargets = itemModel.RevenueTargets;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await dbContext.SaveChangesAsync();
                return entity;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task ClosingCommission(List<ClosingCommission> itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in itemModel)
                        {
                            var CommissionConfig = await db.tbl_CommissionCampaign.SingleOrDefaultAsync(x => x.Id == item.Id && x.Enable ==true && x.Status == (int)CommissionConfigStatus.ChuaChot);
                            if (CommissionConfig == null)
                                throw new Exception("Chốt hoa hồng thất bại, vui lòng kiểm tra lại chiến dịch!");
                            CommissionConfig.Commission = item.Commission;
                            CommissionConfig.Status = (int)CommissionConfigStatus.DaChot;
                            CommissionConfig.Percent = item.Percent;
                            CommissionConfig.Descriptions = item.Description;
                            await db.SaveChangesAsync();
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
        }
        public async Task Delete(int id)
        {
            try
            {
                var entity = await dbContext.tbl_CommissionCampaign.SingleOrDefaultAsync(x => x.Id == id);
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
    }
}