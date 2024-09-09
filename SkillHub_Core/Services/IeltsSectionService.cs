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
    public class IeltsSectionService : DomainService
    {
        public IeltsSectionService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_IeltsSection> GetById(int id)
        {
            return await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<int> NewIndex(int ieltsSkillId)
        {
            var lastIndex = await dbContext.tbl_IeltsSection.Where(x => x.IeltsSkillId == ieltsSkillId && x.Enable == true)
                .OrderByDescending(x => x.Index).FirstOrDefaultAsync();
            if (lastIndex == null)
                return 1;
            return lastIndex.Index + 1;
        }
        public async Task<tbl_IeltsSection> Insert(IeltsSectionCreate itemModel, tbl_UserInformation userLog)
        {
            var ieltsSkill = await dbContext.tbl_IeltsSkill.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsSkillId);
            if (ieltsSkill == null)
                throw new Exception("Không tìm thầy kỹ năng");
            var model = new tbl_IeltsSection(itemModel);
            model.Index = await NewIndex(itemModel.IeltsSkillId.Value);
            model.IeltsExamId = ieltsSkill.IeltsExamId;
            model.CreatedBy = model.ModifiedBy = userLog.FullName;
            dbContext.tbl_IeltsSection.Add(model);
            await dbContext.SaveChangesAsync();
            return model;
        }
        public async Task<tbl_IeltsSection> Update(IeltsSectionUpdate itemModel, tbl_UserInformation userLog)
        {
            var entity = await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
            if(entity == null)
                throw new Exception("Không tìm thấy dữ liệu");
            entity.Name = itemModel.Name ?? entity.Name;
            entity.Explain = itemModel.Explain ?? entity.Explain;
            entity.ReadingPassage = itemModel.ReadingPassage ?? entity.ReadingPassage;
            entity.Audio = itemModel.Audio ?? entity.Audio;
            entity.ModifiedBy = userLog.FullName;
            entity.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task Delete(int id, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var entity = await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Enable = false;
                    entity.ModifiedBy = userLog.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await dbContext.SaveChangesAsync();

                    var ieltsSections = await dbContext.tbl_IeltsSection.Where(x => x.IeltsSkillId == entity.IeltsSkillId && x.Enable == true).ToListAsync();
                    if (ieltsSections.Any())
                    {
                        int index = 1;
                        foreach (var ieltsSection in ieltsSections)
                        {
                            ieltsSection.Index = index;
                            await dbContext.SaveChangesAsync();
                            index++;
                        }
                    }

                    IeltsSkillService ieltsSkillService = new IeltsSkillService(dbContext);
                    await ieltsSkillService.ReloadQuestionsAmount(entity.IeltsSkillId);
                    var ieltsQuestionGroups = await dbContext.tbl_IeltsQuestionGroup.Where(x => x.IeltsSectionId == entity.Id && x.Enable == true).ToListAsync();
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
                            var entity = await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == item.Id);
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
        public async Task<AppDomainResult> GetAll(IeltsSectionSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new IeltsSectionSearch();
            var pg = await dbContext.tbl_IeltsSection.Where(x => x.Enable == true
            && (string.IsNullOrEmpty(baseSearch.Search) || x.Name.Contains(baseSearch.Search))
            && x.IeltsSkillId == baseSearch.IeltsSkillId
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