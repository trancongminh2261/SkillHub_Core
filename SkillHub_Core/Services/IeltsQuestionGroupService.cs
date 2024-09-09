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
    public class IeltsQuestionGroupService : DomainService
    {
        public IeltsQuestionGroupService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_IeltsQuestionGroup> GetById(int id)
        {
            var ieltsQuestionService = new IeltsQuestionService(dbContext);
            var data = await dbContext.tbl_IeltsQuestionGroup.SingleOrDefaultAsync(x => x.Id == id);
            data.TagNames = await this.GetTagName(data.TagIds);
            data.IeltsQuestions = await ieltsQuestionService.GetByIeltsQuestionGroup(data.Id, data.Type, 0);
            return data;
        }
        public async Task<List<string>> GetTagName(string tags)
        {
            if (tags == null)
                return new List<string>();
            using (var db = new lmsDbContext())
            {
                var tagList = tags.Split(',').ToList();
                var tagObjects = await db.tbl_Tag.Where(x => tagList.Contains(x.Id.ToString())).ToListAsync();
                if (tagObjects.Any())
                    return tagObjects.Select(x => x.Name).ToList();
                return new List<string>();
            }
        }
        public async Task<int> NewIndex(int ieltsSectionId)
        {
            var lastIndex = await dbContext.tbl_IeltsQuestionGroup
                .Where(x => x.IeltsSectionId == ieltsSectionId && x.Enable == true)
                .OrderByDescending(x => x.Index).FirstOrDefaultAsync();
            if (lastIndex == null)
                return 1;
            return lastIndex.Index + 1;
        }
        public async Task<tbl_IeltsQuestionGroup> Insert(IeltsQuestionGroupCreate itemModel, tbl_UserInformation userLog)
        {
            if (itemModel.Level < 1 && itemModel.Level > 3)
                throw new Exception("Cấp độ không phù hợp");
            if (itemModel.Type < 1 && itemModel.Type > 8)
                throw new Exception("Loại đề không phù hợp");

            var model = new tbl_IeltsQuestionGroup
            {
                Audio = itemModel.Audio,
                Content = itemModel.Content,
                IeltsSectionId = itemModel.IeltsSectionId,
                Level = itemModel.Level,
                LevelName = itemModel.LevelName,
                Name = itemModel.Name,
                TagIds = itemModel.TagIds,
                Type = itemModel.Type,
                TypeName = itemModel.TypeName,
                Enable = true,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
            };
            var ieltsSection = new tbl_IeltsSection();
            if (itemModel.IeltsSectionId != 0)
            {
                ieltsSection = await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsSectionId);
                if (ieltsSection == null)
                    throw new Exception("Không tìm thấy phần này");
            }
            if (itemModel.IeltsSectionId != 0)
                model.Index = await NewIndex(itemModel.IeltsSectionId);
            model.IeltsExamId = ieltsSection.IeltsExamId;
            model.IeltsSkillId = ieltsSection.IeltsSkillId;
            model.CreatedBy = model.ModifiedBy = userLog.FullName;
            dbContext.tbl_IeltsQuestionGroup.Add(model);
            await dbContext.SaveChangesAsync();

            //Copy thì lưu lại Id câu hỏi ở đây, nếu tạo thủ công id câu gốc là chính nó
            if (itemModel.SourceId == 0)
                model.SourceId = model.Id;
            else model.SourceId = itemModel.SourceId;

            if (itemModel.IeltsSectionId != 0 && itemModel.IsHandmade)
                model.SourceId = 0;

            await dbContext.SaveChangesAsync();

            if (itemModel.IeltsQuestions.Any())
            {
                var ieltsQuestionService = new IeltsQuestionService(dbContext);
                foreach (var item in itemModel.IeltsQuestions)
                {
                    item.IeltsQuestionGroupId = model.Id;
                    await ieltsQuestionService.InsertOrUpdate(item, userLog);
                }
            }
            await ReloadQuestionsAmount(model.Id);
            ////Tạm ẩn chức năng clone câu hỏi
            //if (model.IeltsSectionId != 0 && itemModel.IsHandmade)
            //{
            //    itemModel.IeltsSectionId = 0;
            //    var source = await this.Insert(itemModel, userLog);
            //    model.SourceId = source.Id;
            //    await dbContext.SaveChangesAsync();
            //}
            return model;
        }
        public async Task ReloadQuestionsAmount(int ieltsQuestionGroupId)
        {
            var ieltsQuestionGroup = await dbContext.tbl_IeltsQuestionGroup.SingleOrDefaultAsync(x => x.Id == ieltsQuestionGroupId);
            if (ieltsQuestionGroup != null)
            {
                ieltsQuestionGroup.QuestionsAmount = await dbContext.tbl_IeltsQuestion.CountAsync(x => x.IeltsQuestionGroupId == ieltsQuestionGroup.Id && x.Enable == true);
                await dbContext.SaveChangesAsync();
                if (ieltsQuestionGroup.IeltsSectionId != 0)
                {
                    var ieltsSkillService = new IeltsSkillService(dbContext);
                    await ieltsSkillService.ReloadQuestionsAmount(ieltsQuestionGroup.IeltsSkillId);
                }
            }
        }
        public async Task<tbl_IeltsQuestionGroup> Update(IeltsQuestionGroupUpdate itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var entity = await dbContext.tbl_IeltsQuestionGroup.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Name = itemModel.Name ?? entity.Name;
                    entity.Content = itemModel.Content ?? entity.Content;
                    entity.Audio = itemModel.Audio ?? entity.Audio;
                    entity.TagIds = itemModel.TagIds ?? entity.TagIds;
                    entity.Level = itemModel.Level ?? entity.Level;
                    entity.LevelName = itemModel.LevelName ?? entity.LevelName;
                    if (itemModel.IeltsQuestions.Any())
                    {
                        var ieltsQuestionService = new IeltsQuestionService(dbContext);
                        foreach (var item in itemModel.IeltsQuestions)
                        {
                            item.IeltsQuestionGroupId = entity.Id;
                            await ieltsQuestionService.InsertOrUpdate(item, userLog);
                        }
                    }
                    await ReloadQuestionsAmount(entity.Id);
                    if (entity.IeltsSectionId != 0)
                    {

                    }
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
        public async Task Delete(int id)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var entity = await dbContext.tbl_IeltsQuestionGroup.SingleOrDefaultAsync(x => x.Id == id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Enable = false;
                    await dbContext.SaveChangesAsync();
                    var ieltsQuestions = await dbContext.tbl_IeltsQuestion.Where(x => x.IeltsQuestionGroupId == id && x.Enable == true).ToListAsync();
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
                        await dbContext.SaveChangesAsync();
                    }
                    var ieltsSkillService = new IeltsSkillService(dbContext);
                    await ieltsSkillService.ReloadQuestionsAmount(entity.IeltsSkillId);
                    tran.Commit();
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
            if (itemModel.Items.Any())
            {
                foreach (var item in itemModel.Items)
                {
                    var entity = await dbContext.tbl_IeltsQuestionGroup.SingleOrDefaultAsync(x => x.Id == item.Id);
                    entity.Index = item.Index;
                    entity.ModifiedBy = userLog.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await dbContext.SaveChangesAsync();
                }
            }
        }
        public async Task<AppDomainResult> GetAll(IeltsQuestionGroupSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new IeltsQuestionGroupSearch();
            string sql = $"Get_IeltsQuestionGroup @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Levels = N'{baseSearch.Levels}'," +
                $"@Types = N'{baseSearch.Types}'," +
                $"@TagIds = N'{baseSearch.TagIds}'," +
                $"@IsSource = N'{(!baseSearch.IsSource.HasValue ? 0 : baseSearch.IsSource.Value ? 1 : 2)}'," +
                $"@Search = N'{baseSearch.Search ?? ""}'";
            var data = await dbContext.SqlQuery<Get_IeltsQuestionGroup>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            var ieltsQuestionService = new IeltsQuestionService(dbContext);
            var result = (from i in data
                          select new tbl_IeltsQuestionGroup
                          {
                              Audio = i.Audio,
                              Content = i.Content,
                              CreatedBy = i.CreatedBy,
                              CreatedOn = i.CreatedOn,
                              Enable = i.Enable,
                              Id = i.Id,
                              IeltsExamId = i.IeltsExamId,
                              IeltsSectionId = i.IeltsSectionId,
                              IeltsSkillId = i.IeltsSkillId,
                              Index = i.Index,
                              Level = i.Level,
                              LevelName = i.LevelName,
                              ModifiedBy = i.ModifiedBy,
                              ModifiedOn = i.ModifiedOn,
                              Name = i.Name,
                              QuestionsAmount = i.QuestionsAmount,
                              SourceId = i.SourceId,
                              Type = i.Type,
                              TypeName = i.TypeName,
                              TagIds = i.TagIds,
                              TagNames = Task.Run(() => this.GetTagName(i.TagIds)).Result,
                              IeltsQuestions = Task.Run(() => ieltsQuestionService.GetByIeltsQuestionGroup(i.Id, i.Type, 0)).Result,
                              HasInIeltsExam = Task.Run(() => this.HasInIeltsExam(i.Id, baseSearch.HasInIeltsExamId)).Result,
                          }).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        public async Task<bool> HasInIeltsExam(int sourceId, int ieltsExamId)
        {
            if (ieltsExamId == 0)
                return false;
            var has = await dbContext.tbl_IeltsQuestionGroup.AnyAsync(x => x.IeltsExamId == ieltsExamId && x.SourceId == sourceId && x.Enable == true);
            return has;
        }
    }
}