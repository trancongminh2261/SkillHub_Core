using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class StudentRatingFormService : DomainService
    {
        public StudentRatingFormService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_StudentRatingForm> GetById(int id)
        {
            return await dbContext.tbl_StudentRatingForm.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<AppDomainResult> GetAll(StudentRatingFormSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new StudentRatingFormSearch();
            string sql = $"Get_StudentRatingForm @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@StudentId = N'{baseSearch.StudentId}'," +
                $"@RatingSheetId = N'{baseSearch.RatingSheetId}' ";
            var data = await dbContext.SqlQuery<Get_StudentRatingFormSearch>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }
        public async Task<tbl_StudentRatingForm> Insert(StudentRatingFormCreate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId && x.Enable == true);
                if (student == null)
                    throw new Exception("Không tìm thấy sinh viên");
                var ratingSheet = await dbContext.tbl_StudentRatingForm.SingleOrDefaultAsync(x => x.Id == itemModel.RatingSheetId && x.Enable == true);
                if (ratingSheet == null)
                    throw new Exception("Không tìm thấy phiếu khảo sát");
                var model = new tbl_StudentRatingForm(itemModel);
                model.StudentId = itemModel.StudentId;
                model.RatingSheetId = itemModel.RatingSheetId;
                
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                dbContext.tbl_StudentRatingForm.Add(model);
                await dbContext.SaveChangesAsync();
                return model;
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
                var entity = await dbContext.tbl_StudentRatingForm.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy bài đánh giá của sinh viên");
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