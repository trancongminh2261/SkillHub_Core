using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class IeltsAnswerService : DomainService
    {
        public IeltsAnswerService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<(List<IeltsAnswerModel>, int)> GetByIeltsQuestion(int ieltsQuestionId, int type, bool IsHideAnswer = false)
        {
            var data = await dbContext.tbl_IeltsAnswer.Where(x => x.IeltsQuestionId == ieltsQuestionId && x.Enable == true)
                .Select(x => new IeltsAnswerModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    Index = IsHideAnswer ? 0 : x.Index,
                    Correct = IsHideAnswer == true ? false : x.Correct,
                    Type = x.Type,
                }).OrderBy(x => x.Index).ThenBy(x => x.Id).ToListAsync();
            if (IsHideAnswer && type == 6)
                data = data.OrderBy(x => Guid.NewGuid()).ToList();
            int correctAmount = await dbContext.tbl_IeltsAnswer
                .CountAsync(x => x.IeltsQuestionId == ieltsQuestionId && x.Enable == true && x.Correct == true);
            return (data, correctAmount);
        }
        public async Task<tbl_IeltsAnswer> InsertOrUpdate(IeltsAnswerInsertOrUpdate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var entity = await dbContext.tbl_IeltsAnswer.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (itemModel.Id == 0)
                {
                    var ieltsQuestion = await dbContext.tbl_IeltsQuestion.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsQuestionId);
                    if (ieltsQuestion == null)
                        throw new Exception("Không tìm thấy câu hỏi");
                    entity = new tbl_IeltsAnswer(itemModel);
                    entity.ModifiedBy = entity.CreatedBy = userLog.FullName;
                    entity.Enable = true;
                    entity.CreatedOn = entity.ModifiedOn = DateTime.Now;
                    dbContext.tbl_IeltsAnswer.Add(entity);
                }
                else
                {
                    if (entity == null)
                        throw new Exception("Không tìm thấy đáp án");
                    entity.Content = itemModel.Content ?? entity.Content;
                    entity.Correct = itemModel.Correct ?? entity.Correct;
                    entity.Type = itemModel.Type ?? entity.Type;
                    entity.Index = itemModel.Index ?? entity.Index;
                    entity.Enable = itemModel.Enable ?? entity.Enable;
                }
                await dbContext.SaveChangesAsync();
                return entity;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}