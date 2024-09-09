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
    public class IeltsQuestionService : DomainService
    {
        public IeltsQuestionService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<List<IeltsQuestionModel>> GetByIeltsQuestionGroup(int ieltsQuestionGroupId, int type, int doingTestId)
        {
            var doingTestService = new DoingTestService(dbContext);
            var ieltsAnswerService = new IeltsAnswerService(dbContext);
            var data = await dbContext.tbl_IeltsQuestion.Where(x => x.IeltsQuestionGroupId == ieltsQuestionGroupId && x.Enable == true)
                .Select(x => new IeltsQuestionModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    SampleAnswer = (doingTestId == 0) ? x.SampleAnswer : "",
                    Explain = x.Explain,
                    Index = x.Index,
                    InputId = x.InputId,
                    Point = x.Point,
                    Audio = x.Audio
                }).OrderBy(x => x.Index).ToListAsync();
            //Đảo vị trí câu hỏi Chọn từ vào ô trống
            var ieltsQuestionIds = data.Select(x => x.Id).OrderBy(x => Guid.NewGuid()).ToList();
            data = (from i in data
                    select new IeltsQuestionModel
                    {
                        Id = i.Id,
                        Content = i.Content,
                        SampleAnswer = i.SampleAnswer,
                        Explain = i.Explain,
                        Index = i.Index,
                        InputId = i.InputId,
                        Point = i.Point,
                        IeltsAnswers = Task.Run(() => ieltsAnswerService.GetByIeltsQuestion((doingTestId != 0 && type == 2) ? ieltsQuestionIds[data.FindIndex(x => x.Id == i.Id)] : i.Id, type, doingTestId != 0 ? true : false)).Result.Item1,
                        CorrectAmount = Task.Run(() => ieltsAnswerService.GetByIeltsQuestion(i.Id, type, doingTestId != 0 ? true : false)).Result.Item2,
                        DoingTestDetails = Task.Run(() => doingTestService.GetDetail(i.Id, doingTestId)).Result,
                        Audio = i.Audio
                    }).ToList();
            return data;
        }

        public async Task<tbl_IeltsQuestion> InsertOrUpdate(IeltsQuestionInsertOrUpdate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var ieltsAnswerService = new IeltsAnswerService(dbContext);
                var entity = await dbContext.tbl_IeltsQuestion.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (itemModel.Id == 0)
                {
                    var ieltsQuestionGroup = await dbContext.tbl_IeltsQuestionGroup.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsQuestionGroupId);
                    if (ieltsQuestionGroup == null)
                        throw new Exception("Không tìm thấy nhóm câu hỏi");
                    entity = new tbl_IeltsQuestion(itemModel);
                    entity.IeltsExamId = ieltsQuestionGroup.IeltsExamId;
                    entity.IeltsQuestionGroupId = ieltsQuestionGroup.Id;
                    entity.IeltsSectionId = ieltsQuestionGroup.IeltsSectionId;
                    entity.IeltsSkillId = ieltsQuestionGroup.IeltsSkillId;
                    entity.ModifiedBy = entity.CreatedBy = userLog.FullName;
                    entity.Enable = true;
                    entity.CreatedOn = entity.ModifiedOn = DateTime.Now;
                    dbContext.tbl_IeltsQuestion.Add(entity);
                }
                else
                {
                    if (entity == null)
                        throw new Exception("Không tìm thấy câu hỏi");
                    entity.Content = itemModel.Content ?? entity.Content;
                    entity.SampleAnswer = itemModel.SampleAnswer ?? entity.SampleAnswer;
                    entity.InputId = itemModel.InputId ?? entity.InputId;
                    entity.Explain = itemModel.Explain ?? entity.Explain;
                    entity.Index = itemModel.Index ?? entity.Index;
                    entity.Point = itemModel.Point ?? entity.Point;
                    entity.Audio = itemModel.Audio ?? entity.Audio;
                    entity.Enable = itemModel.Enable ?? entity.Enable;
                }
                await dbContext.SaveChangesAsync();
                if (itemModel.IeltsAnswers.Any())
                {
                    foreach (var item in itemModel.IeltsAnswers)
                    {
                        item.IeltsQuestionId = entity.Id;
                        await ieltsAnswerService.InsertOrUpdate(item, userLog);
                    }
                }
                return entity;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}