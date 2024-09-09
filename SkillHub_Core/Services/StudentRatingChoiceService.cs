using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class StudentRatingChoiceService : DomainService
    {
        public StudentRatingChoiceService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_StudentRatingChoice> GetByStudentRatingFormId(int StudentRatingFormId)
        {
            return await dbContext.tbl_StudentRatingChoice.SingleOrDefaultAsync(x => x.StudentRatingFormId == StudentRatingFormId
                                                                             && x.Enable == true);
        }

        public async Task<tbl_StudentRatingChoice> Insert(StudentRatingChoiceCreate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var ratingForm= dbContext.tbl_StudentRatingForm.SingleOrDefaultAsync(x => x.Id == itemModel.StudentRatingFormId
                                                                              && x.Enable == true);
                if (ratingForm == null)
                    throw new Exception("Not found!"); 

                var model = new tbl_StudentRatingChoice(itemModel);
                model.ListRatingAnswer= JsonConvert.SerializeObject(itemModel.ListRatingChoice);
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                dbContext.tbl_StudentRatingChoice.Add(model);
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
        //        var entity = await dbContext.tbl_StudentRatingChoice.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
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
        //public async Task<AppDomainResult> GetAll(StudentRatingChoiceSearch baseSearch)
        //{
        //    if (baseSearch == null) baseSearch = new StudentRatingChoiceSearch();
        //    string sql = $"Get_tbl_StudentRatingChoice @PageIndex = {baseSearch.PageIndex}," +
        //        $"@PageSize = {baseSearch.PageSize}," +
        //        $"@Search = N'{baseSearch.Search ?? ""}'," +
        //        $"@StudentRatingFormId = N'{baseSearch.StudentRatingFormId}'";
        //    var data = await dbContext.SqlQuery<Get_StudentRatingChoiceSearch>(sql);
        //    if (data==null) return new AppDomainResult { TotalRow = 0, Data = null };
        //    return new AppDomainResult { Data = data };
        //}
    }
}