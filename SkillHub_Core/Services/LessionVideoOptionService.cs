using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class LessionVideoOptionService : DomainService
    {
        public LessionVideoOptionService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_LessionVideoOption> GetById(int id)
        {
            return await dbContext.tbl_LessionVideoOption.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<AppDomainResult> GetAll(LessionVideoOptionSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new LessionVideoOptionSearch();
            string sql = $"Get_LessionVideoOption @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@QuestionId = {baseSearch.QuestionId}";
            var data = await dbContext.SqlQuery<Get_LessionVideoOption>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }
        public async Task<tbl_LessionVideoOption> Insert(LessionVideoOptionCreate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var model = new tbl_LessionVideoOption(itemModel);
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                dbContext.tbl_LessionVideoOption.Add(model);
                await dbContext.SaveChangesAsync();
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<tbl_LessionVideoOption> Update(LessionVideoOptionUpdate itemModel, tbl_UserInformation user)
        {
            try
            {
                var entity = await dbContext.tbl_LessionVideoOption.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy lựa chọn");
                entity.Content = itemModel.Content ?? entity.Content;
                entity.TrueFalse = itemModel.TrueFalse ?? entity.TrueFalse;

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
                var entity = await dbContext.tbl_LessionVideoOption.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
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