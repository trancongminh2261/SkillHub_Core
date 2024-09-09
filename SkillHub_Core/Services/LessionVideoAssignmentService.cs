using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class LessionVideoAssignmentService : DomainService
    {
        public LessionVideoAssignmentService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_LessionVideoAssignment> GetById(int id)
        {
            return await dbContext.tbl_LessionVideoAssignment.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        //public async Task<AppDomainResult> GetAll(LessionVideoAssignmentSearch baseSearch)
        //{
        //    if (baseSearch == null) baseSearch = new LessionVideoAssignmentSearch();
        //    string sql = $"Get_LessionVideoAssignment @PageIndex = {baseSearch.PageIndex}," +
        //        $"@PageSize = {baseSearch.PageSize}," +
        //        $"@Search = N'{baseSearch.Search ?? ""}' ";
        //    var data = await dbContext.SqlQuery<Get_LessionVideoAssignment>(sql);
        //    if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
        //    var totalRow = data[0].TotalRow;
        //    return new AppDomainResult { TotalRow = totalRow, Data = data };
        //}
        public async Task<tbl_LessionVideoAssignment> Insert(LessionVideoAssignmentCreate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var model = new tbl_LessionVideoAssignment(itemModel);
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                dbContext.tbl_LessionVideoAssignment.Add(model);
                await dbContext.SaveChangesAsync();
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //public async Task Delete(int id)
        //{
        //    try
        //    {
        //        var entity = await dbContext.tbl_LessionVideoAssignment.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        //        if (entity == null)
        //            throw new Exception("Không tìm thấy bảng lựa chọn của học viên");
        //        entity.Enable = false;
        //        await dbContext.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}
    }
}