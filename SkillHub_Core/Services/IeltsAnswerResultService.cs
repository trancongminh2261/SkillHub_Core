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
    public class IeltsAnswerResultService : DomainService
    {
        public IeltsAnswerResultService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<List<IeltsAnswerResultModel>> GetIeltsQuestionResult(int ieltsQuestionResultId)
        {
            var data = await dbContext.tbl_IeltsAnswerResult.Where(x => x.IeltsQuestionResultId == ieltsQuestionResultId && x.Enable == true)
                .Select(x => new IeltsAnswerResultModel
                {
                    Id = x.Id,
                    Correct = x.Correct,
                    IeltsAnswerId = x.IeltsAnswerId,
                    IeltsAnswerContent = x.IeltsAnswerContent,
                    Index = x.Index,
                    MyChoice = x.MyChoice,
                    MyIeltsAnswerContent = x.MyIeltsAnswerContent,
                    MyIeltsAnswerId = x.MyIeltsAnswerId,
                    MyIndex = x.MyIndex,
                    Type = x.Type
                }).ToListAsync();
            return data;
        }
        public async Task<tbl_IeltsAnswerResult> Insert(IeltsAnswerResultCreate itemModel, tbl_UserInformation userLog)
        {
            var model = new tbl_IeltsAnswerResult(itemModel);
            model.CreatedBy = model.ModifiedBy = userLog.FullName;
            dbContext.tbl_IeltsAnswerResult.Add(model);
            await dbContext.SaveChangesAsync();
            return model;
        }
    }
}