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
    public class IeltsQuestionGroupResultService : DomainService
    {
        public IeltsQuestionGroupResultService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_IeltsQuestionGroupResult> GetById(int id)
        {
            var ieltsQuestionGroupService = new IeltsQuestionGroupService(dbContext);
            var ieltsQuestionResultService = new IeltsQuestionResultService(dbContext);
            var data = await dbContext.tbl_IeltsQuestionGroupResult.SingleOrDefaultAsync(x => x.Id == id);
            data.TagNames = await ieltsQuestionGroupService.GetTagName(data.TagIds);
            data.IeltsQuestionResults = await ieltsQuestionResultService.GetByIeltsQuestionGroupResult(data.Id);
            //var questionResult = await dbContext.tbl_IeltsQuestionResult.SingleOrDefaultAsync(x => x.Enable == true && x.Id == data.IeltsQuestionResults);
            return data;
        }
        public async Task<IeltsResult> Insert(IeltsQuestionGroupResultCreate itemModel, tbl_UserInformation userLog)
        {
            var ieltsQuestionGroup = await dbContext.tbl_IeltsQuestionGroup.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsQuestionGroupId);
            if (ieltsQuestionGroup == null)
                throw new Exception("Không tìm thấy nhóm câu hỏi");
            var model = new tbl_IeltsQuestionGroupResult
            {
                Audio = ieltsQuestionGroup.Audio,
                Content = ieltsQuestionGroup.Content,
                CreatedBy = userLog.FullName,
                CreatedOn = DateTime.Now,
                Enable = true,
                IeltsExamId = ieltsQuestionGroup.IeltsExamId,
                IeltsQuestionGroupId = ieltsQuestionGroup.Id,
                IeltsSectionId = ieltsQuestionGroup.IeltsSectionId,
                IeltsSectionResultId = itemModel.IeltsSectionResultId,
                IeltsSkillId = ieltsQuestionGroup.IeltsSkillId,
                Index = ieltsQuestionGroup.Index,
                Level = ieltsQuestionGroup.Level,
                LevelName = ieltsQuestionGroup.LevelName,
                ModifiedBy = userLog.FullName,
                ModifiedOn = DateTime.Now,
                Name = ieltsQuestionGroup.Name,
                QuestionsAmount = ieltsQuestionGroup.QuestionsAmount,
                SourceId = ieltsQuestionGroup.SourceId,
                TagIds = ieltsQuestionGroup.TagIds,
                Type = ieltsQuestionGroup.Type,
                TypeName = ieltsQuestionGroup.TypeName
            };
            dbContext.tbl_IeltsQuestionGroupResult.Add(model);
            await dbContext.SaveChangesAsync();
            var ieltsQuestions = await dbContext.tbl_IeltsQuestion.Where(x => x.IeltsQuestionGroupId == ieltsQuestionGroup.Id && x.Enable == true).ToListAsync();
            var result = new IeltsResult();
            if (ieltsQuestions.Any())
            {
                var ieltsQuestionResultService = new IeltsQuestionResultService(dbContext);
                foreach (var ieltsQuestion in ieltsQuestions)
                {
                    var data = await ieltsQuestionResultService.Insert(new IeltsQuestionResultCreate
                    { 
                        DoingTestId = itemModel.DoingTestId,
                        IeltsExamId = model.IeltsExamId,
                        IeltsQuestionGroupId = model.IeltsQuestionGroupId,
                        IeltsQuestionGroupResultId = model.Id,
                        IeltsQuestionId = ieltsQuestion.Id,
                        IeltsSectionId = model.IeltsSectionId,
                        IeltsSkillId = model.IeltsSkillId
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
        public class GradingEssayRequest
        {
            public int IeltsQuestionResultId { get; set; }
            public string Note { get; set; }
            public double Point { get; set; }
            public List<GradingEssayItem> Items { get; set; }
        }
        public class GradingEssayItem
        {
            /// <summary>
            /// Thêm mới thì truyền 0
            /// </summary>
            public int Id { get; set; }
            public int IeltsAnswerResultId { get; set; }
            /// <summary>
            /// Nội dung chấm
            /// </summary>
            public string Content { get; set; }
            public string Audio { get; set; }
            public bool? Enable { get; set; }
            public string Note { get; set; }
        }
        /// <summary>
        /// Chấm bài tự luận
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="userLog"></param>
        /// <returns></returns>
        public async Task GradingEssay(GradingEssayRequest itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var ieltsQuestionResult = await dbContext.tbl_IeltsQuestionResult.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsQuestionResultId);
                    if (ieltsQuestionResult == null)
                        throw new Exception("Không tìm thấy câu hỏi");
                    double outPoint = ieltsQuestionResult.Point ?? 0;
                    var ieltsQuestion = await dbContext.tbl_IeltsQuestion.SingleOrDefaultAsync(x => x.Id == ieltsQuestionResult.IeltsQuestionId);
                    if (ieltsQuestion == null)
                        throw new Exception("Không tìm thấy câu hỏi");
                    if (itemModel.Point > ieltsQuestion.Point)
                        throw new Exception($"Điểm tối đa của câu hỏi này là {ieltsQuestion.Point}");
                    ieltsQuestionResult.Note = itemModel.Note;
                    ieltsQuestionResult.Point = itemModel.Point;
                    if (itemModel.Items.Any())
                    {
                        foreach (var item in itemModel.Items)
                        {
                            var ieltsAnswerResult = await dbContext.tbl_IeltsAnswerResult.SingleOrDefaultAsync(x => x.Id == item.IeltsAnswerResultId);
                            if (ieltsAnswerResult == null)
                                throw new Exception("Không tìm thấy câu trả lời");
                            if (item.Id == 0)
                            {
                                var comment = new tbl_IeltsAnswerResultComment
                                {
                                    Audio = item.Audio,
                                    Content = item.Content,
                                    CreatedBy = userLog.FullName,
                                    CreatedOn = DateTime.Now,
                                    Enable = true,
                                    IeltsAnswerResultId = item.IeltsAnswerResultId,
                                    IeltsQuestionResultId = ieltsAnswerResult.IeltsQuestionResultId,
                                    ModifiedBy = userLog.FullName,
                                    ModifiedOn = DateTime.Now,
                                    Note = item.Note
                                };
                                dbContext.tbl_IeltsAnswerResultComment.Add(comment);
                            }
                            else
                            {
                                var comment = await dbContext.tbl_IeltsAnswerResultComment.SingleOrDefaultAsync(x => x.Id == item.Id);
                                if (comment == null)
                                    throw new Exception("Không tìm thấy bình luận");
                                comment.Audio = item.Audio ?? comment.Audio;
                                comment.Content = item.Content ?? comment.Content;
                                comment.Enable = item.Enable ?? comment.Enable;
                            }
                        }
                    }
                    await dbContext.SaveChangesAsync();

                    //Tính lại tổng điểm
                    double differencePoint = itemModel.Point - outPoint;
                    var ieltsQuestionGroupResult = await dbContext.tbl_IeltsQuestionGroupResult.SingleOrDefaultAsync(x => x.Id == ieltsQuestionResult.IeltsQuestionGroupResultId);
                    if (ieltsQuestionGroupResult == null)
                        return;
                    var ieltsSectionResult = await dbContext.tbl_IeltsSectionResult.SingleOrDefaultAsync(x => x.Id == ieltsQuestionGroupResult.IeltsSectionResultId);
                    if (ieltsSectionResult == null)
                        return;
                    var ieltsSkillResult = await dbContext.tbl_IeltsSkillResult.SingleOrDefaultAsync(x => x.Id == ieltsSectionResult.IeltsSkillResultId);
                    if (ieltsSkillResult != null)
                    {
                        ieltsSkillResult.MyPoint += differencePoint;
                        int skillAmount = await dbContext.tbl_IeltsSkillResult.CountAsync(x => x.IeltsExamResultId == ieltsSkillResult.IeltsExamResultId && x.Enable == true);
                        var ieltsExamResult = await dbContext.tbl_IeltsExamResult.SingleOrDefaultAsync(x => x.Id == ieltsSkillResult.IeltsExamResultId);
                        if (ieltsSkillResult != null)
                        {
                            ieltsExamResult.MyPoint += differencePoint;
                            ieltsExamResult.AveragePoint = Math.Round(ieltsExamResult.MyPoint / skillAmount, 2);
                        }
                        await dbContext.SaveChangesAsync();
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
        public class AnswerCommentSearch
        {
            public int IeltsQuestionResultId { get; set; }
            public int IeltsAnswerResultId { get; set; }
        }
        /// <summary>
        /// Lấy thông tin đánh giá câu trả lời
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public async Task<List<Get_IeltsAnswerResultComment>> GetAnswerComment(AnswerCommentSearch baseSearch)
        {
            var data = await dbContext.tbl_IeltsAnswerResultComment
                .Where(x => x.IeltsQuestionResultId == baseSearch.IeltsQuestionResultId && x.IeltsAnswerResultId == baseSearch.IeltsAnswerResultId && x.Enable == true).ToListAsync();
            var result = (from x in data
                          select new Get_IeltsAnswerResultComment
                          {
                              IeltsQuestionResultId = x.IeltsQuestionResultId,
                              IeltsAnswerResultId = x.IeltsAnswerResultId,
                              Content = x.Content,
                              Audio = x.Audio,
                              SampleAnswer = Task.Run(() => GetSampleAnswer(x.IeltsQuestionResultId)).Result,
                              Note = x.Note,
                              Enable = x.Enable,
                              CreatedBy = x.CreatedBy,
                              CreatedOn = x.CreatedOn,
                              ModifiedBy = x.ModifiedBy,
                              ModifiedOn = x.ModifiedOn
                          }).ToList();
            return result;
        }

        public async Task<string> GetSampleAnswer(int IeltsQuestionResultId)
        {
            var result = "";
            var IeltsQuestionResult = await dbContext.tbl_IeltsAnswerResult.SingleOrDefaultAsync(x => x.Id == IeltsQuestionResultId);
            if (IeltsQuestionResult != null)
            {
                var IeltsQuestion = await dbContext.tbl_IeltsQuestion.SingleOrDefaultAsync(x => x.Id == IeltsQuestionResult.IeltsQuestionId);
                if (IeltsQuestion != null)
                {
                    result = IeltsQuestion.SampleAnswer;
                }
            }
            return result;
        }
    }
}