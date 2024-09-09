using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class RatingOptionService : DomainService
    {
        public RatingOptionService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_RatingOption> GetById(int id)
        {
            return await dbContext.tbl_RatingOption.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<AppDomainResult> GetAll(RatingOptionSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new RatingOptionSearch();
            string sql = $"Get_RatingOption @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@Name = N'{baseSearch.Name ?? ""}'," +
                $"@RatingQuestionId = N'{baseSearch.RatingQuestionId}' ";
            var data = await dbContext.SqlQuery<Get_RatingOptionSearch>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }
        public async Task<tbl_RatingOption> Insert(RatingOptionCreate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var model = new tbl_RatingOption(itemModel);
                model.Name = itemModel.Name;
                model.TrueOrFalse = itemModel.TrueOrFalse;
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                dbContext.tbl_RatingOption.Add(model);
                await dbContext.SaveChangesAsync();
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<tbl_RatingOption> Update(RatingOptionUpdate itemModel, tbl_UserInformation user)
        {
            try
            { 
                var entity = await dbContext.tbl_RatingOption.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy lựa chọn");
                entity.Name = itemModel.Name ?? entity.Name;
                entity.TrueOrFalse = itemModel.TrueOrFalse; 
                entity.Essay = itemModel.Essay ?? entity.Essay;

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
        public async Task Delete(int id)
        {
            try
            {
                var entity = await dbContext.tbl_RatingOption.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy lựa chọn");
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