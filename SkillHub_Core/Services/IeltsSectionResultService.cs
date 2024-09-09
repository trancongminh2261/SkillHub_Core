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
    public class IeltsSectionResultService : DomainService
    {
        public IeltsSectionResultService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_IeltsSectionResult> GetById(int id)
        {
            return await dbContext.tbl_IeltsSectionResult.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IeltsResult> Insert(IeltsSectionResultCreate itemModel, tbl_UserInformation userLog)
        {
            var ieltsSection = await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsSectionId);
            if (ieltsSection == null)
                throw new Exception("Không tìm thấy phần");
            var model = new tbl_IeltsSectionResult
            {
                Audio = ieltsSection.Audio,
                CreatedBy = userLog.FullName,
                CreatedOn = DateTime.Now,
                Enable = true,
                Explain = ieltsSection.Explain,
                IeltsExamId = ieltsSection.IeltsExamId,
                IeltsSectionId = ieltsSection.Id,
                IeltsSkillId = ieltsSection.IeltsSkillId,
                IeltsSkillResultId = itemModel.IeltsSkillResultId,
                Index = ieltsSection.Index,
                ModifiedBy = userLog.ModifiedBy,
                ModifiedOn = DateTime.Now,
                Name = ieltsSection.Name,
                ReadingPassage = ieltsSection.ReadingPassage
            };
            dbContext.tbl_IeltsSectionResult.Add(model);
            await dbContext.SaveChangesAsync();
            var ieltsQuestionGroups = await dbContext.tbl_IeltsQuestionGroup
                .Where(x => x.IeltsSectionId == ieltsSection.Id && x.Enable == true).ToListAsync();
            var result = new IeltsResult();
            if (ieltsQuestionGroups.Any())
            {
                var ieltsQuestionGroupResultService = new IeltsQuestionGroupResultService(dbContext);
                foreach (var item in ieltsQuestionGroups)
                {
                    var data = await ieltsQuestionGroupResultService.Insert(new IeltsQuestionGroupResultCreate
                    {
                        DoingTestId = itemModel.DoingTestId,
                        IeltsQuestionGroupId = item.Id,
                        IeltsSectionResultId = model.Id
                    }, userLog);
                    result.MyPoint += data.MyPoint;
                    result.QuestionsMultipleChoiceCorrect += data.QuestionsMultipleChoiceCorrect;
                    result.QuestionsDifficultCorrect += data.QuestionsDifficultCorrect;
                    result.QuestionsEasyCorrect += data.QuestionsEasyCorrect;
                    result.QuestionsNormalCorrect += data.QuestionsNormalCorrect;
                }
            }
            return result;
        }
    }
}