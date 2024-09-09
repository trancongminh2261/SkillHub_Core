using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services.WarningHistory
{
    public class WarningHistoryService : DomainService
    {
        public WarningHistoryService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_WarningHistory> GetById(int id)
        {
            return await dbContext.tbl_WarningHistory.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<AppDomainResult> GetAll(WarningHistorySearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new WarningHistorySearch();
            string sql = $"Get_WarningHistory @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@StudentId = {baseSearch.StudentId}," +
                $"@ClassId = {baseSearch.ClassId ?? 0}";
            var data = await dbContext.SqlQuery<Get_WarningHistory>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            var result = data.Select(i => new tbl_WarningHistory(i)).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }

        public async Task<tbl_WarningHistory> Insert(WarningHistoryCreate itemModel, tbl_UserInformation userLog = null)
        {
            var model = new tbl_WarningHistory(itemModel);
            model.CreatedBy = model.ModifiedBy = userLog == null ? "Tự động" : userLog.FullName;
            dbContext.tbl_WarningHistory.Add((tbl_WarningHistory)model);
            await dbContext.SaveChangesAsync();
            return model;
        }
    }
}
