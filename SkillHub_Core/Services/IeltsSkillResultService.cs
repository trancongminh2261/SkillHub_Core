using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class IeltsSkillResultService : DomainService
    {
        public IeltsSkillResultService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_IeltsSkillResult> GetById(int id)
        {
            return await dbContext.tbl_IeltsSkillResult.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<tbl_IeltsSkillResult> Insert(IeltsSkillResultCreate itemModel, tbl_UserInformation userLog)
        {
            var ieltsSkill = await dbContext.tbl_IeltsSkill.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsSkillId);
            if (ieltsSkill == null)
                throw new Exception("Không tìm thấy kỹ năng");
            //var timeSpentOfSkill = JsonConvert.DeserializeObject<List<DoingTestService.SaveTimeItem>>(itemModel.TimeSpentOfSkill)
            //    .Where(x=>x.IeltsSkillId == ieltsSkill.Id).FirstOrDefault();
            var model = new tbl_IeltsSkillResult
            {
                Audio = ieltsSkill.Audio,
                CreatedBy = userLog.FullName,
                CreatedOn = DateTime.Now,
                Enable = true,
                IeltsExamId = ieltsSkill.IeltsExamId,
                IeltsExamResultId = itemModel.IeltsExamResultId,
                IeltsSkillId = ieltsSkill.Id,
                Index = ieltsSkill.Index,
                ModifiedBy = userLog.FullName,
                ModifiedOn = DateTime.Now,
                Name = ieltsSkill.Name,
                Note = "",
                Point = ieltsSkill.Point,
                QuestionsAmount = ieltsSkill.QuestionsAmount,
                QuestionsDifficult = ieltsSkill.QuestionsDifficult,
                QuestionsEasy = ieltsSkill.QuestionsEasy,
                QuestionsNormal = ieltsSkill.QuestionsNormal,
                MyPoint = 0,
                QuestionsMultipleChoiceCorrect = 0,
                QuestionEssayAmount = ieltsSkill.QuestionEssayAmount,
                QuestionMultipleChoiceAmount = ieltsSkill.QuestionMultipleChoiceAmount,
                QuestionsDifficultCorrect = 0,
                QuestionsEasyCorrect = 0,
                QuestionsNormalCorrect = 0,
                //TimeSpent = timeSpentOfSkill?.TimeSpent ?? 0,
                Time = ieltsSkill.Time
            };
            dbContext.tbl_IeltsSkillResult.Add(model);
            await dbContext.SaveChangesAsync();
            var ieltsSections = await dbContext.tbl_IeltsSection.Where(x => x.IeltsSkillId == ieltsSkill.Id && x.Enable == true).ToListAsync();
            if (ieltsSections.Any())
            {
                var ieltsSectionResultService = new IeltsSectionResultService(dbContext);
                foreach (var item in ieltsSections)
                {
                    var data = await ieltsSectionResultService.Insert(new IeltsSectionResultCreate
                    {
                        DoingTestId = itemModel.DoingTestId,
                        IeltsSectionId = item.Id,
                        IeltsSkillResultId = model.Id
                    }, userLog);
                    model.MyPoint += data.MyPoint;
                    model.QuestionsMultipleChoiceCorrect += data.QuestionsMultipleChoiceCorrect;
                    model.QuestionsDifficultCorrect += data.QuestionsDifficultCorrect;
                    model.QuestionsEasyCorrect += data.QuestionsEasyCorrect;
                    model.QuestionsNormalCorrect += data.QuestionsNormalCorrect;
                }
                await dbContext.SaveChangesAsync();
            }
            return model;
        }
    }
}