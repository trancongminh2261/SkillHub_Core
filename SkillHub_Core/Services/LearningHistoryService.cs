using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class LearningHistoryService : DomainService
    {
        public LearningHistoryService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_LearningHistory> GetById(int id)
        {
            return await dbContext.tbl_LearningHistory.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<AppDomainResult> GetAll(LearningHistorySearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new LearningHistorySearch();
            string sql = $"Get_LearningHistory @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}', " +
                $"@StudentId = {baseSearch.StudentId}";
            var data = await dbContext.SqlQuery<Get_LearningHistory>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            var result = data.Select(i => new tbl_LearningHistory(i)).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }

        public async Task<tbl_LearningHistory> Insert(LearningHistoryCreate itemModel, tbl_UserInformation userLog = null)
        {
            var model = new tbl_LearningHistory(itemModel);
            model.CreatedBy = model.ModifiedBy = userLog == null ? "Tự động" : userLog.FullName;
            dbContext.tbl_LearningHistory.Add(model);
            await dbContext.SaveChangesAsync();
            return model;
        }
    }
}