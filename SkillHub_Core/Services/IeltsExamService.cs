using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class IeltsExamService : DomainService
    {
        public IeltsExamService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_IeltsExam> GetById(int id)
        {
            var data = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data != null)
            {
                var question = await GetNumberQuestion(data.Id);
                data.TotaFillInWords = question?.TotaFillInWords ?? 0;
                data.TotalArrangement = question?.TotalArrangement ?? 0;
                data.TotalChoices = question?.TotalChoices ?? 0;
                data.TotalChooseWord = question?.TotalChooseWord ?? 0;
                data.TotalMindmap = question?.TotalMindmap ?? 0;
                data.TotalSpeak = question?.TotalSpeak ?? 0;
                data.TotalTF = question?.TotalTF ?? 0;
                data.TotalWrite = question?.TotalWrite ?? 0;
                data.AvatarUserCreate = dbContext.tbl_UserInformation.FirstOrDefault(x => x.UserInformationId == data.UserCreate)?.Avatar;
            }
            return data;
        }
        public async Task<tbl_IeltsExam> Insert(IeltsExamCreate itemModel, tbl_UserInformation userLog)
        {
            var model = new tbl_IeltsExam(itemModel);
            var hasCode = await dbContext.tbl_IeltsExam.AnyAsync(x => x.Code.ToUpper() == itemModel.Code.ToUpper() && x.Enable == true);
            if (hasCode)
                throw new Exception("Đã tồn tại mã đề này");
            model.CreatedBy = model.ModifiedBy = userLog.FullName;
            model.UserCreate = userLog.UserInformationId;
            model.AvatarUserCreate = userLog.Avatar;
            dbContext.tbl_IeltsExam.Add(model);
            await dbContext.SaveChangesAsync();
            return model;
        }
        public async Task<tbl_IeltsExam> Update(IeltsExamUpdate itemModel, tbl_UserInformation userLog)
        {
            var entity = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
            if (entity == null)
                throw new Exception("Không tìm thấy đề");
            if (!string.IsNullOrEmpty(itemModel.Code))
            {
                if (await dbContext.tbl_IeltsExam.AnyAsync(x => x.Code.ToUpper() == itemModel.Code.ToUpper() && x.Id != entity.Id))
                    throw new Exception("Đã tồn tại mã đề này");
            }
            var question = await GetNumberQuestion(entity.Id);
            entity.TotaFillInWords = question?.TotaFillInWords ?? 0;
            entity.TotalArrangement = question?.TotalArrangement ?? 0;
            entity.TotalChoices = question?.TotalChoices ?? 0;
            entity.TotalChooseWord = question?.TotalChooseWord ?? 0;
            entity.TotalMindmap = question?.TotalMindmap ?? 0;
            entity.TotalSpeak = question?.TotalSpeak ?? 0;
            entity.TotalTF = question?.TotalTF ?? 0;
            entity.TotalWrite = question?.TotalWrite ?? 0;

            entity.Name = itemModel.Name ?? entity.Name;
            entity.LevelExam = itemModel.LevelExam ?? entity.LevelExam;
            entity.LevelExamName = entity.LevelExam == 1 ? "Dễ" : entity.LevelExam == 2 ? "Trung Bình" : entity.LevelExam == 3 ? "Khó" : "";
            entity.Code = itemModel.Code ?? entity.Code;
            entity.Thumbnail = itemModel.Thumbnail ?? entity.Thumbnail;
            entity.Time = itemModel.Time ?? entity.Time;
            entity.Description = itemModel.Description ?? entity.Description;
            entity.Active = itemModel.Active ?? entity.Active;
            entity.ModifiedBy = userLog.FullName;
            entity.ModifiedOn = DateTime.Now;
            entity.AvatarUserCreate = dbContext.tbl_UserInformation.FirstOrDefault(x => x.UserInformationId == entity.UserCreate)?.Avatar;
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task Delete(int id, tbl_UserInformation userLog)
        {
            var entity = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new Exception("Không tìm thấy đề");
            entity.Enable = false;
            entity.ModifiedBy = userLog.FullName;
            entity.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
        }
        public async Task ReloadQuestionsAmount(int ieltsExamId)
        {
            var entity = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == ieltsExamId);
            if (entity == null)
                return;
            var skills = await dbContext.tbl_IeltsSkill.Where(x => x.IeltsExamId == entity.Id && x.Enable == true)
                .Select(x => new { x.QuestionsAmount, x.QuestionsDifficult, x.QuestionsNormal, x.QuestionsEasy, x.Point, x.QuestionMultipleChoiceAmount, x.QuestionEssayAmount }).ToListAsync();
            if (skills.Any())
            {
                entity.QuestionsAmount = skills.Sum(x => x.QuestionsAmount);
                entity.QuestionsDifficult = skills.Sum(x => x.QuestionsDifficult);
                entity.QuestionsNormal = skills.Sum(x => x.QuestionsNormal);
                entity.QuestionsEasy = skills.Sum(x => x.QuestionsEasy);
                entity.QuestionMultipleChoiceAmount = skills.Sum(x => x.QuestionMultipleChoiceAmount);
                entity.QuestionEssayAmount = skills.Sum(x => x.QuestionEssayAmount);
                entity.Point = skills.Sum(x => x.Point);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                entity.QuestionsAmount = 0;
                entity.QuestionsDifficult = 0;
                entity.QuestionsNormal = 0;
                entity.QuestionsEasy = 0;
                entity.QuestionMultipleChoiceAmount = 0;
                entity.QuestionEssayAmount = 0;
                entity.Point = 0;
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task SetTime(int ieltsExamId)
        {
            //var entity = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == ieltsExamId);
            //if (entity != null)
            //{
            //    var times = await dbContext.tbl_IeltsSkill.Where(x => x.IeltsExamId == entity.Id && x.Enable == true)
            //            .Select(x => x.Time).ToListAsync();
            //    if (times.Any())
            //    {
            //        entity.Time = times.Sum();
            //        await dbContext.SaveChangesAsync();
            //    }
            //}
        }
        public async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            if (baseSearch == null) baseSearch = new SearchOptions();
            string sql = $"Get_IeltsExam @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'";
            var data = await dbContext.SqlQuery<Get_IeltsExam>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            var result = data.Select(i => new tbl_IeltsExam(i)).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        public class IeltsExamOverviewSearch
        {
            [Required]
            public int IeltsExamId { get; set; }
            public IeltsExamOverviewSearch()
            {
            }
        }
        /// <summary>
        /// Tổng quan đề thi
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public async Task<IeltsExamOverviewModel> GetIeltsExamOverview(IeltsExamOverviewSearch baseSearch)
        {
            var ieltsExam = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == baseSearch.IeltsExamId);
            if (ieltsExam == null)
                return null;
            string sql = $"Get_IeltsExamOverview @IeltsExamId = {baseSearch.IeltsExamId}";
            var data = await dbContext.SqlQuery<Get_IeltsExamOverview>(sql);
            var result = new IeltsExamOverviewModel
            {
                Code = ieltsExam.Code,
                Description = ieltsExam.Description,
                Id = ieltsExam.Id,
                Name = ieltsExam.Name,
                QuestionsAmount = ieltsExam.QuestionsAmount,
                QuestionsDifficult = ieltsExam.QuestionsDifficult,
                QuestionsEasy = ieltsExam.QuestionsEasy,
                QuestionsNormal = ieltsExam.QuestionsNormal,
                Thumbnail = ieltsExam.Thumbnail,
                Time = ieltsExam.Time,
                Point = ieltsExam.Point,
                IeltsSkills = !data.Any() ? null : data.GroupBy(s => new
                {
                    s.Id,
                    s.Audio,
                    s.Index,
                    s.Name,
                    s.QuestionsAmount,
                    s.QuestionsDifficult,
                    s.QuestionsEasy,
                    s.QuestionsNormal,
                    s.Time
                }).Select(s => new IeltsSkillOverviewModel
                {
                    Id = s.Key.Id,
                    Audio = s.Key.Audio,
                    Index = s.Key.Index,
                    Name = s.Key.Name,
                    QuestionsAmount = s.Key.QuestionsAmount,
                    QuestionsDifficult = s.Key.QuestionsDifficult,
                    QuestionsEasy = s.Key.QuestionsEasy,
                    QuestionsNormal = s.Key.QuestionsNormal,
                    Time = s.Key.Time,
                    IeltsSections = s.GroupBy(st => new
                    {
                        st.IeltsSectionId,
                        st.IeltsSectionAudio,
                        st.IeltsSectionExplain,
                        st.IeltsSectionIndex,
                        st.IeltsSectionName,
                        st.IeltsSectionReadingPassage
                    }).Select(st => new IeltsSectionOverviewModel
                    {
                        Id = st.Key.IeltsSectionId,
                        Audio = st.Key.IeltsSectionAudio,
                        Explain = st.Key.IeltsSectionExplain,
                        Index = st.Key.IeltsSectionIndex,
                        Name = st.Key.IeltsSectionName,
                        ReadingPassage = st.Key.IeltsSectionReadingPassage
                    }).ToList()
                }).ToList()
            };
            return result;
        }
        public class IeltsQuestionInSectionSearch
        {
            public int IeltsSectionId { get; set; }
            public int DoingTestId { get; set; }
        }
        /// <summary>
        /// Danh sách câu hỏi trong section
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public async Task<List<IeltsQuestionOverviewModel>> GetIeltsQuestionInSection(IeltsQuestionInSectionSearch baseSearch)
        {
            //Lấy vị trí câu hỏi trong đề
            int startIndex = 0;
            var ieltsSection = await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == baseSearch.IeltsSectionId);
            if (ieltsSection == null)
                return null;
            var ieltsSkill = await dbContext.tbl_IeltsSkill.SingleOrDefaultAsync(x => x.Id == ieltsSection.IeltsSkillId);
            var previousSkills = await dbContext.tbl_IeltsSkill.Where(x => x.IeltsExamId == ieltsSkill.IeltsExamId && x.Index < ieltsSkill.Index && x.Enable == true)
                .Select(x => x.QuestionsAmount).ToListAsync();
            if (previousSkills.Any())
                startIndex += previousSkills.Sum(x => x);
            var previousSectionIds = await dbContext.tbl_IeltsSection.Where(x => x.IeltsSkillId == ieltsSection.IeltsSkillId && x.Index < ieltsSection.Index && x.Enable == true)
                .Select(x => x.Id).ToListAsync();
            if (previousSectionIds.Any())
            {
                foreach (var previousSectionId in previousSectionIds)
                {
                    startIndex += !(await dbContext.tbl_IeltsQuestionGroup.AnyAsync(x => x.IeltsSectionId == previousSectionId && x.Enable == true)) ? 0
                        : await dbContext.tbl_IeltsQuestionGroup.Where(x => x.IeltsSectionId == previousSectionId && x.Enable == true).SumAsync(x => x.QuestionsAmount);
                }
            }

            string sql = $"Get_IeltsQuestionInSection @IeltsSectionId = {baseSearch.IeltsSectionId}, @DoingTestId = {baseSearch.DoingTestId}";
            var data = await dbContext.SqlQuery<Get_IeltsQuestionInSection>(sql);
            var result = (from i in data
                          select new IeltsQuestionOverviewModel
                          {
                              IeltsQuestionGroupId = i.IeltsQuestionGroupId,
                              IeltsQuestionId = i.IeltsQuestionId,
                              IsDone = i.IsDone,
                              InputId = i.InputId,
                              Index = startIndex += 1
                          }).ToList();
            return result;
        }
        public async Task AddQuestionsGroupToExam(AddQuestionsGroupToExam itemModel, tbl_UserInformation userlog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var ieltsSection = await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsSectionId);
                    if (ieltsSection == null)
                        throw new Exception("Không tìm thấy phần này");
                    if (!itemModel.IeltsQuestionGroupIds.Any())
                        throw new Exception("Vui lòng chọn câu hỏi");
                    var ieltsQuestionGroupService = new IeltsQuestionGroupService(dbContext);
                    foreach (var item in itemModel.IeltsQuestionGroupIds)
                    {
                        var ieltsQuestionGroup = await ieltsQuestionGroupService.GetById(item);
                        if (ieltsQuestionGroup == null)
                            continue;

                        var model = new IeltsQuestionGroupCreate
                        {
                            Audio = ieltsQuestionGroup.Audio,
                            Content = ieltsQuestionGroup.Content,
                            IeltsSectionId = ieltsSection.Id,
                            IsHandmade = false, // Tạo Tự động
                            Level = ieltsQuestionGroup.Level,
                            Name = ieltsQuestionGroup.Name,
                            SourceId = ieltsQuestionGroup.Id,
                            TagIds = ieltsQuestionGroup.TagIds,
                            Type = ieltsQuestionGroup.Type,
                            IeltsQuestions = ieltsQuestionGroup.IeltsQuestions.Select(q => new IeltsQuestionInsertOrUpdate
                            {
                                Content = q.Content,
                                SampleAnswer = q.SampleAnswer,
                                Id = 0,
                                Explain = q.Explain,
                                IeltsQuestionGroupId = ieltsQuestionGroup.Id,
                                Index = q.Index,
                                InputId = q.InputId,
                                Point = q.Point,
                                Audio = q.Audio,
                                IeltsAnswers = q.IeltsAnswers.Select(a => new IeltsAnswerInsertOrUpdate
                                {
                                    Content = a.Content,
                                    Id = 0,
                                    IeltsQuestionId = q.Id,
                                    Index = a.Index,
                                    Correct = a.Correct,
                                    Type = a.Type
                                }).ToList()
                            }).ToList()
                        };
                        await ieltsQuestionGroupService.Insert(model, userlog);
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
        public async Task GenerateQuestion(GenerateQuestionCreate itemModel, tbl_UserInformation userlog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var ieltsSection = await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsSectionId);
                    if (ieltsSection == null)
                        throw new Exception("Không tìm thấy phần này");
                    if (itemModel.Questions.Count == 0)
                        throw new Exception("Vui lòng chọn câu hỏi");
                    var ieltsQuestionGroupService = new IeltsQuestionGroupService(dbContext);
                    foreach (var item in itemModel.Questions)
                    {
                        var totalQuestion = await dbContext.tbl_IeltsQuestionGroup.CountAsync(x => x.IeltsExamId == 0
                        && x.IeltsSkillId == 0 && x.IeltsSectionId == 0 && x.Enable == true && x.Type == item.Type && x.Level == item.Level
                        && x.Id == x.SourceId);
                        if (item.Number > totalQuestion)
                            throw new Exception("Số câu hỏi vượt quá câu hỏi sẵn có trong ngân câu hỏi");
                        for (int i = 0; i < item.Number; i++)
                        {
                            var ieltsQuestionGroup = await GetIeltsQuestionGroupRandom(ieltsSection.IeltsExamId, ieltsSection.IeltsSkillId, ieltsSection.Id, item.Type ?? 0, item.Level ?? 0);
                            if (ieltsQuestionGroup == null)
                                continue;

                            var model = new IeltsQuestionGroupCreate
                            {
                                Audio = ieltsQuestionGroup.Audio,
                                Content = ieltsQuestionGroup.Content,
                                IeltsSectionId = ieltsSection.Id,
                                IsHandmade = false, // Tạo Tự động
                                Level = ieltsQuestionGroup.Level,
                                Name = ieltsQuestionGroup.Name,
                                SourceId = ieltsQuestionGroup.Id,
                                TagIds = ieltsQuestionGroup.TagIds,
                                Type = ieltsQuestionGroup.Type,
                                IeltsQuestions = ieltsQuestionGroup.IeltsQuestions.Select(q => new IeltsQuestionInsertOrUpdate
                                {
                                    Content = q.Content,
                                    SampleAnswer = q.SampleAnswer,
                                    Id = 0,
                                    Explain = q.Explain,
                                    IeltsQuestionGroupId = ieltsQuestionGroup.Id,
                                    Index = q.Index,
                                    InputId = q.InputId,
                                    Point = q.Point,
                                    Audio = q.Audio,
                                    IeltsAnswers = q.IeltsAnswers.Select(a => new IeltsAnswerInsertOrUpdate
                                    {
                                        Content = a.Content,
                                        Id = 0,
                                        IeltsQuestionId = q.Id,
                                        Index = a.Index,
                                        Correct = a.Correct,
                                        Type = a.Type
                                    }).ToList()
                                }).ToList()
                            };
                            await ieltsQuestionGroupService.Insert(model, userlog);
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
        public async Task<tbl_IeltsQuestionGroup> GetIeltsQuestionGroupRandom(int ieltsExamId, int ieltsSkillId, int ieltsSectionId, int type, int level)
        {
            var ieltsQuestionGroupService = new IeltsQuestionGroupService(dbContext);
            string sql = $"Get_QuestionRandom @IeltsExamId = {ieltsExamId}, @IeltsSkillId = {ieltsSkillId}, @IeltsSectionId = {ieltsSectionId}, @Type = {type}, @Level = {level}";
            var dataQuery = await dbContext.SqlQuery<tbl_IeltsQuestionGroup>(sql);
            var data = dataQuery.FirstOrDefault();
            data = await ieltsQuestionGroupService.GetById(data.Id);
            return data;
        }
        public class IeltsExamOption
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }
        public async Task<List<IeltsExamOption>> GetIeltsExamOption()
        {
            return await dbContext.tbl_IeltsExam
                .Where(x => x.Enable == true)
                .Select(i => new IeltsExamOption
                {
                    Id = i.Id,
                    Code = i.Code,
                    Name = i.Name
                }).ToListAsync();
        }
        public class QuestionOptionModel
        {
            public int Type { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public string LevelName { get; set; }
            public int TotalQuestion { get; set; }
        }
        public async Task<List<QuestionOptionModel>> QuestionOption()
        {
            List<QuestionOptionModel> rs = new List<QuestionOptionModel>();
            // 8 loại câu hỏi
            for (int i = 1; i <= 8; i++)
            {
                // 3 mức độ khó
                for (int j = 1; j <= 3; j++)
                {
                    rs.Add(new QuestionOptionModel
                    {
                        Type = i,
                        Name = ListTypeQuestionEnum().FirstOrDefault(x => x.Key == i).Value,
                        Level = j,
                        LevelName = ListLevelQuestionEnum().FirstOrDefault(x => x.Key == j).Value,
                        TotalQuestion = Task.Run(() => CountQuestion(i, j)).Result
                    });
                }
            }
            return rs;
        }
        public async Task<int> CountQuestion(int type, int level)
        {
            var count = await dbContext.tbl_IeltsQuestionGroup.CountAsync(x => x.IeltsExamId == 0
                        && x.IeltsSkillId == 0 && x.IeltsSectionId == 0 && x.Enable == true && x.Type == type && x.Level == level
                        && x.Id == x.SourceId);
            return count;
        }
        public async Task GenerateExam(GenerateExamCreate itemModel, tbl_UserInformation userlog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var ieltsSkillService = new IeltsSkillService(dbContext);
                    var ieltsSectionService = new IeltsSectionService(dbContext);
                    if (itemModel.Exam == null)
                        throw new Exception("Không tìm thấy dữ liệu đề thi");
                    //Tạo đề
                    IeltsExamCreate itemExam = new IeltsExamCreate()
                    {
                        Code = itemModel.Exam.Code,
                        Name = itemModel.Exam.Name,
                        Description = itemModel.Exam.Description,
                        Thumbnail = itemModel.Exam.Thumbnail
                    };
                    var ieltsExam = Insert(itemExam, userlog);
                    if (ieltsExam == null)
                        throw new Exception("Tạo đề thất bại");
                    //Tạo kỹ năng
                    if (itemModel.Exam.IeltsSkill != null)
                    {
                        foreach (var skill in itemModel.Exam.IeltsSkill)
                        {
                            IeltsSkillCreate itemSkill = new IeltsSkillCreate()
                            {
                                Name = skill.Name,
                                Time = skill.Time,
                                Audio = skill.Audio,
                                IeltsExamId = ieltsExam.Id
                            };
                            var ieltsSkill = ieltsSkillService.Insert(itemSkill, userlog);
                            if (ieltsSkill == null)
                                continue;
                            //Tạo Section
                            if (skill.IeltsSection == null)
                                continue;
                            foreach (var section in skill.IeltsSection)
                            {
                                IeltsSectionCreate itemsSection = new IeltsSectionCreate()
                                {
                                    Name = section.Name,
                                    Audio = section.Audio,
                                    IeltsSkillId = ieltsSkill.Id,
                                    Explain = section.Explain
                                };
                                var ieltsSection = ieltsSectionService.Insert(itemsSection, userlog);
                                if (ieltsSection == null)
                                    continue;
                                //Tạo câu hỏi tự động
                                GenerateQuestionCreate itemQuestion = new GenerateQuestionCreate()
                                {
                                    IeltsSectionId = ieltsSection.Id,
                                    Questions = new List<GenQuestionModel>()
                                };
                                itemQuestion.Questions.AddRange(section.Questions);
                                await GenerateQuestion(itemQuestion, userlog);
                            }
                        }
                    }
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw new Exception("Tạo đề thất bại");
                }
            }
        }
        public async Task<NumberQuestionModel> GetNumberQuestion(int IeltsExamId)
        {
            string sql = $"Get_GetNumberQuestion @IeltsExamId = {IeltsExamId}";
            var data = await dbContext.SqlQuery<NumberQuestionModel>(sql);
            return data.FirstOrDefault();
        }
    }
}