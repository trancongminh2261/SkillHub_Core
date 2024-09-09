using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class TuitionPackageService : DomainService
    {
        public TuitionPackageService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_TuitionPackage> GetById(int id)
        {
            return await dbContext.tbl_TuitionPackage.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<tbl_TuitionPackage> Insert(TuitionPackageCreate itemModel, tbl_UserInformation userLog)
        {
            if (itemModel.Months < 1)
                throw new Exception("Vui lòng nhập thời gian dùng gói từ một tháng trở lên");
            if (AssetCRM.CheckUnicode(itemModel.Code))
                throw new Exception("Mã gói không được phép gõ dấu");
            var hasCode = await dbContext.tbl_TuitionPackage.AnyAsync(x => x.Code.ToUpper() == itemModel.Code.ToUpper() && x.Enable == true);
            if (hasCode)
                throw new Exception("Mã gói này đã tồn tại");
            if (itemModel.DiscountType == 2 && itemModel.Discount > 100)
                throw new Exception("Không được phép giảm trên 100% học phí");
            var model = new tbl_TuitionPackage(itemModel);
            model.CreatedBy = model.ModifiedBy = userLog.FullName;
            dbContext.tbl_TuitionPackage.Add(model);
            await dbContext.SaveChangesAsync();
            return model;
        }
        public async Task<tbl_TuitionPackage> Update(TuitionPackageUpdate itemModel, tbl_UserInformation userLog)
        {
            var entity = await dbContext.tbl_TuitionPackage.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
            if (entity == null)
                throw new Exception("Không tìm thấy dữ liệu");
            if (!string.IsNullOrEmpty(itemModel.Code))
            {
                var hasCode = await dbContext.tbl_TuitionPackage.AnyAsync(x => x.Code.ToUpper() == itemModel.Code.ToUpper() && x.Id != entity.Id && x.Enable == true);
                if (hasCode)
                    throw new Exception("Mã gói này đã tồn tại");
            }
            entity.Code = itemModel.Code ?? entity.Code;
            entity.MaxDiscount = itemModel.MaxDiscount ?? entity.MaxDiscount;
            entity.Months = itemModel.Months ?? entity.Months;
            if (entity.Months < 1)
                throw new Exception("Vui lòng nhập thời gian dùng gói từ một tháng trở lên");
            entity.DiscountType = itemModel.DiscountType ?? entity.DiscountType;
            entity.DiscountTypeName = itemModel.DiscountTypeName ?? entity.DiscountTypeName;
            entity.Discount = itemModel.Discount ?? entity.Discount;
            entity.Description = itemModel.Description ?? entity.Description;
            if (entity.DiscountType == 2 && entity.Discount > 100)
                throw new Exception("Không được phép giảm trên 100% học phí");
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task Delete(int id)
        {
            var entity = await dbContext.tbl_TuitionPackage.SingleOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new Exception("Không tìm thấy dữ liệu");
            entity.Enable = false;
            await dbContext.SaveChangesAsync();
        }
        public async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            if (baseSearch == null) baseSearch = new SearchOptions();
            var pg = await dbContext.tbl_TuitionPackage.Where(x => x.Enable == true
            && (string.IsNullOrEmpty(baseSearch.Search) || x.Code.Contains(baseSearch.Search)))
                .OrderByDescending(x => x.CreatedOn).Select(x => x.Id).ToListAsync();

            if (!pg.Any())
                return new AppDomainResult() { TotalRow = 0, Data = null };
            int totalRow = pg.Count();
            pg = pg.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
            var data = (from i in pg
                        select Task.Run(() => GetById(i)).Result).ToList();
            return new AppDomainResult() { TotalRow = totalRow, Data = data };
        }
    }
}