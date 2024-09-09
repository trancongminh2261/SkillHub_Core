using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class RatingQuestionService : DomainService
    {
        public RatingQuestionService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_RatingQuestion> GetById(int id)
        {
            return await dbContext.tbl_RatingQuestion.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<AppDomainResult> GetAll(RatingQuestionSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new RatingQuestionSearch();
            string sql = $"Get_RatingQuestion @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@RatingSheetId = N'{baseSearch.RatingSheetId}' ";
            var data = await dbContext.SqlQuery<Get_RatingQuestionSearch>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }
        public async Task<tbl_RatingQuestion> Insert(RatingQuestionCreate itemModel, tbl_UserInformation userLog)
        {
            try 
            {
                var RatingSheet= await dbContext.tbl_RatingSheet.SingleOrDefaultAsync(x => x.Id == itemModel.RatingSheetId && x.Enable == true);
                if (RatingSheet == null)
                    throw new Exception("Không tìm thấy bảng khảo sát");
                var model = new tbl_RatingQuestion(itemModel);
                model.Name = itemModel.Name;
                model.Type = itemModel.Type;
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                dbContext.tbl_RatingQuestion.Add(model);
                await dbContext.SaveChangesAsync();
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<tbl_RatingQuestion> Update(RatingQuestionUpdate itemModel, tbl_UserInformation user)
        {
            try
            { 
                var entity = await dbContext.tbl_RatingQuestion.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable==true);
                if (entity == null)
                    throw new Exception("Không tìm thấy câu hỏi khảo sát");
                entity.Name = itemModel.Name ?? entity.Name;
                entity.Type = itemModel.Type; 
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
                var entity = await dbContext.tbl_RatingQuestion.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy câu hỏi khảo sát");
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