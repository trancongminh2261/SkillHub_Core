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
    public class IeltsSkillService : DomainService
    {
        public IeltsSkillService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_IeltsSkill> GetById(int id)
        {
            return await dbContext.tbl_IeltsSkill.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<int> NewIndex(int ieltsExamId)
        {
            var lastIndex = await dbContext.tbl_IeltsSkill.Where(x => x.IeltsExamId == ieltsExamId && x.Enable == true)
                .OrderByDescending(x => x.Index).FirstOrDefaultAsync();
            if (lastIndex == null)
                return 1;
            return lastIndex.Index + 1;
        }
        public async Task<tbl_IeltsSkill> Insert(IeltsSkillCreate itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var hasIeltsExam = await dbContext.tbl_IeltsExam.AnyAsync(x => x.Id == itemModel.IeltsExamId);
                    if (!hasIeltsExam)
                        throw new Exception("Không tìm thấy đề");
                    var model = new tbl_IeltsSkill(itemModel);
                    model.Index = await NewIndex(itemModel.IeltsExamId.Value);
                    model.CreatedBy = model.ModifiedBy = userLog.FullName;
                    dbContext.tbl_IeltsSkill.Add(model);
                    await dbContext.SaveChangesAsync();
                    IeltsExamService ieltsExamService = new IeltsExamService(dbContext);
                    await ieltsExamService.SetTime(model.IeltsExamId);
                    tran.Commit();
                    return model;
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw e;
                }
            }
        }
        public async Task<tbl_IeltsSkill> Update(IeltsSkillUpdate itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var entity = await dbContext.tbl_IeltsSkill.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");

                    entity.Name = itemModel.Name ?? entity.Name;
                    entity.Time = itemModel.Time ?? entity.Time;
                    entity.Audio = itemModel.Audio ?? entity.Audio;
                    entity.ModifiedBy = userLog.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await dbContext.SaveChangesAsync();
                    IeltsExamService ieltsExamService = new IeltsExamService(dbContext);
                    await ieltsExamService.SetTime(entity.IeltsExamId);
                    tran.Commit();
                    return entity;
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw e;
                }
            }
        }
        public async Task ChangeIndex(ChangeIndexModel itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (itemModel.Items.Any())
                    {
                        foreach (var item in itemModel.Items)
                        {
                            var entity = await dbContext.tbl_IeltsSkill.SingleOrDefaultAsync(x => x.Id == item.Id);
                            if (entity != null)
                            {
                                entity.Index = item.Index;
                                entity.ModifiedBy = userLog.FullName;
                                entity.ModifiedOn = DateTime.Now;
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw e;
                }
            }
        }
        public async Task Delete(int id, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var entity = await dbContext.tbl_IeltsSkill.SingleOrDefaultAsync(x => x.Id == id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Enable = false;
                    entity.ModifiedBy = userLog.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await dbContext.SaveChangesAsync();

                    var ieltsSkills = await dbContext.tbl_IeltsSkill.Where(x => x.Enable == true && x.IeltsExamId == entity.IeltsExamId).ToListAsync();
                    if (ieltsSkills.Any())
                    {
                        int index = 1;
                        foreach (var item in ieltsSkills)
                        {
                            item.Index = index;
                            index++;
                        }
                    }

                    //cập nhật lại số lượng câu hỏi của đề
                    IeltsExamService ieltsExamService = new IeltsExamService(dbContext);
                    await ieltsExamService.ReloadQuestionsAmount(entity.IeltsExamId);
                    await ieltsExamService.SetTime(entity.IeltsExamId);

                    //Xóa nội dung
                    var ieltsSections = await dbContext.tbl_IeltsSection.Where(x => x.IeltsSkillId == entity.Id && x.Enable == true).ToListAsync();
                    if (ieltsSections.Any())
                    {
                        foreach (var ieltsSection in ieltsSections)
                        {
                            ieltsSection.Enable = false;
                            var ieltsQuestionGroups = await dbContext.tbl_IeltsQuestionGroup.Where(x => x.IeltsSectionId == ieltsSection.Id && x.Enable == true).ToListAsync();
                            if (ieltsQuestionGroups.Any())
                            {
                                foreach (var ieltsQuestionGroup in ieltsQuestionGroups)
                                {
                                    ieltsQuestionGroup.Enable = false;
                                    var ieltsQuestions = await dbContext.tbl_IeltsQuestion.Where(x => x.IeltsQuestionGroupId == ieltsQuestionGroup.Id && x.Enable == true).ToListAsync();
                                    if (ieltsQuestions.Any())
                                    {
                                        foreach (var ieltsQuestion in ieltsQuestions)
                                        {
                                            ieltsQuestion.Enable = false;
                                            var ieltsAnswers = await dbContext.tbl_IeltsAnswer.Where(x => x.IeltsQuestionId == ieltsQuestion.Id && x.Enable == true).ToListAsync();
                                            if (ieltsAnswers.Any())
                                            {
                                                foreach (var ieltsAnswer in ieltsAnswers)
                                                {
                                                    ieltsAnswer.Enable = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    await dbContext.SaveChangesAsync();
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw e;
                }
            }
        }
        public async Task ReloadQuestionsAmount(int ieltsSkillId)
        {
            var ieltsSkill = await dbContext.tbl_IeltsSkill.SingleOrDefaultAsync(x => x.Id == ieltsSkillId);
            if (ieltsSkill != null)
            {
                int questionsAmount = 0;
                int questionsDifficult = 0;
                int questionsNormal = 0;
                int questionsEasy = 0;
                int questionMultipleChoiceAmount = 0;
                int questionEssayAmount = 0;
                var ieltsSectionIds = await dbContext.tbl_IeltsSection.Where(x => x.IeltsSkillId == ieltsSkillId && x.Enable == true)
                    .Select(x => x.Id).ToListAsync();
                if (ieltsSectionIds.Any())
                {
                    foreach (var ieltsSectionId in ieltsSectionIds)
                    {
                        var questionGroups = await dbContext.tbl_IeltsQuestionGroup.Where(x => x.IeltsSectionId == ieltsSectionId && x.Enable == true)
                            .Select(x => new { x.QuestionsAmount, x.Level,x.Type }).ToListAsync();
                        questionsAmount += questionGroups.Sum(x => x.QuestionsAmount);
                        questionsDifficult += questionGroups.Where(x=>x.Level == 3).Sum(x => x.QuestionsAmount);
                        questionsNormal += questionGroups.Where(x => x.Level == 2).Sum(x => x.QuestionsAmount);
                        questionsEasy += questionGroups.Where(x => x.Level == 1).Sum(x => x.QuestionsAmount);
                        questionMultipleChoiceAmount += questionGroups.Where(x => x.Type != 7 && x.Type != 8).Sum(x => x.QuestionsAmount);
                        questionEssayAmount += questionGroups.Where(x => x.Type == 7 || x.Type == 8).Sum(x => x.QuestionsAmount);
                    }
                }
                double point = 0;
                var points = await dbContext.tbl_IeltsQuestion.Where(x => x.IeltsSkillId == ieltsSkill.Id && x.Enable == true).Select(x => x.Point).ToListAsync();
                if (points.Any())
                    point = points.Sum(x=>x ?? 0);
                //ieltsSkill.Point = await dbContext.tbl_IeltsQuestion.Where(x => x.IeltsSkillId == ieltsSkill.Id && x.Enable == true).SumAsync(x => x.Point ?? 0);
                ieltsSkill.Point = point;
                ieltsSkill.QuestionsAmount = questionsAmount;
                ieltsSkill.QuestionsDifficult = questionsDifficult;
                ieltsSkill.QuestionsNormal = questionsNormal;
                ieltsSkill.QuestionsEasy = questionsEasy;
                ieltsSkill.QuestionMultipleChoiceAmount = questionMultipleChoiceAmount;
                ieltsSkill.QuestionEssayAmount = questionEssayAmount;
                await dbContext.SaveChangesAsync();
                IeltsExamService ieltsExamService = new IeltsExamService(dbContext);
                await ieltsExamService.ReloadQuestionsAmount(ieltsSkill.IeltsExamId);
            }
        }
        public async Task<AppDomainResult> GetAll(IeltsSkillSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new IeltsSkillSearch();
            var pg = await dbContext.tbl_IeltsSkill.Where(x => x.Enable == true
            && (string.IsNullOrEmpty(baseSearch.Search) || x.Name.Contains(baseSearch.Search))
            && x.IeltsExamId == baseSearch.IeltsExamId
            ).OrderBy(x => x.Index).Select(x => x.Id).ToListAsync();

            if (!pg.Any())
                return new AppDomainResult() { TotalRow = 0, Data = null };
            int totalRow = pg.Count();
            pg = pg.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
            var data = (from i in pg
                        select Task.Run(() => GetById(i)).Result).ToList();
            return new AppDomainResult() { TotalRow = totalRow, Data = data };
        }
    }
}