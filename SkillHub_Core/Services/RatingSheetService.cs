using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class RatingSheetService : DomainService
    {
        public RatingSheetService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_RatingSheet> GetById(int id)
        {
            return await dbContext.tbl_RatingSheet.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<AppDomainResult> GetAll(RatingSheetSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new RatingSheetSearch();
            string sql = $"Get_RatingSheet @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@ClassId = N'{baseSearch.ClassId}' ";
            var data = await dbContext.SqlQuery<Get_RatingSheetSearch>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }
        public async Task<tbl_RatingSheet> Insert(RatingSheetCreate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var model = new tbl_RatingSheet(itemModel);
                model.Name = itemModel.Name;
                model.ClassId = itemModel.ClassId;
                model.Note = itemModel.Note;
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                dbContext.tbl_RatingSheet.Add(model);
                await dbContext.SaveChangesAsync();
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<tbl_RatingSheet> Update(RatingSheetUpdate itemModel, tbl_UserInformation user)
        {
            try
            {
                var entity = await dbContext.tbl_RatingSheet.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy phiếu khảo sát");
                entity.Name = itemModel.Name ?? entity.Name;
                entity.Note = itemModel.Note ?? entity.Note;
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
                var entity = await dbContext.tbl_RatingSheet.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy  phiếu khảo sát");
                entity.Enable = false;
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<AppDomainResult> GetDetail(int id)
        {
            var data = await dbContext.tbl_RatingSheet.SingleOrDefaultAsync(x => x.Id == id);
            if (data == null)
                throw new Exception("Không tìm thấy bảng khảo sát");
            // lấy bảng khảo sát
            GetRatingSheetContent res = new GetRatingSheetContent(data); 
            var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == data.ClassId);
            res.ClassId = _class?.Id;
            res.ClassName = _class == null ? null : _class.Name;

            //khởi tạo nội dung: detail
            List<QuestionOption> RatingSheetContent = new List<QuestionOption>();
            // ds câu hỏi
            var ques = await dbContext.tbl_RatingQuestion.Where(x => x.RatingSheetId == data.Id).ToListAsync();
            if (ques.Count!=0)
            {
                QuestionOption questionOption = new QuestionOption();
                foreach (var q in ques)
                {
                    questionOption.QuestionId = q.Id;
                    questionOption.QuestionName = q.Name;
                    questionOption.Type = q.Type;
                    questionOption.TypeName = q.TypeName;

                    //lấy ds option
                    List<Option> listOption = new List<Option>();
                    var option = await dbContext.tbl_RatingOption.Where(x => x.RatingQuestionId == q.Id).ToListAsync();
                    if (option.Count!=0)
                    {
                        foreach(var opt in option)
                        {
                            Option x = new Option();
                            x.OptionId = opt.Id;
                            x.OptionContent = opt.Name;
                            x.TrueOrFalse = opt.TrueOrFalse;
                            x.Essay = opt.Essay;
                            listOption.Add(x);
                        } 
                    }
                    questionOption.listOption = listOption;
                }
                RatingSheetContent.Add(questionOption);
            }
            res.RatingSheetContent = RatingSheetContent;

            return new AppDomainResult { Data = res };
        }
    }
}