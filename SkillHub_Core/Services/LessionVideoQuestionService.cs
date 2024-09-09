using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class LessionVideoQuestionService : DomainService
    {
        public LessionVideoQuestionService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_LessionVideoQuestion> GetById(int id)
        {
            return await dbContext.tbl_LessionVideoQuestion.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<AppDomainResult> GetAll(LessionVideoQuestionSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new LessionVideoQuestionSearch();
            string sql = $"Get_LessionVideoQuestion @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@TestId = {baseSearch.TestId}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@Level = N'{baseSearch.Level ?? ""}' ";
            var data = await dbContext.SqlQuery<Get_LessionVideoQuestion>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        } 
        public async Task<tbl_LessionVideoQuestion> Insert(LessionVideoQuestionCreate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var model = new tbl_LessionVideoQuestion(itemModel);
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                dbContext.tbl_LessionVideoQuestion.Add(model);
                await dbContext.SaveChangesAsync();
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<tbl_LessionVideoQuestion> Update(LessionVideoQuestionUpdate itemModel, tbl_UserInformation user)
        {
            try
            {
                var entity = await dbContext.tbl_LessionVideoQuestion.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy câu hỏi");
                entity.Name = itemModel.Name ?? entity.Name;
                entity.Level = itemModel.Level ?? entity.Level;
                entity.LevelName = itemModel.LevelName;

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
                var entity = await dbContext.tbl_LessionVideoQuestion.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy câu hỏi");
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