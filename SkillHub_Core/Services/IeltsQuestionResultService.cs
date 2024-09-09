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
    public class IeltsQuestionResultService : DomainService
    {
        public IeltsQuestionResultService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<List<IeltsQuestionResultModel>> GetByIeltsQuestionGroupResult(int ieltsQuestionGroupResultId)
        {
            var ieltsAnswerResultService = new IeltsAnswerResultService(dbContext);
            var data = (from i in await dbContext.tbl_IeltsQuestionResult.Where(x => x.IeltsQuestionGroupResultId == ieltsQuestionGroupResultId && x.Enable == true)
                       .ToListAsync()
                        select new IeltsQuestionResultModel
                        {
                            Id = i.Id,
                            Content = i.Content,
                            SampleAnswer = i.SampleAnswer,
                            Correct = i.Correct,
                            Explain = i.Explain,
                            Index = i.Index,
                            InputId = i.InputId,
                            MaxPoint = Task.Run(()=> GetMaxPoint(i.IeltsQuestionId)).Result,
                            Point = i.Point,
                            AnswerOfMindmap = i.AnswerOfMindmap,
                            Note = i.Note,
                            Audio = i.Audio,
                            IeltsAnswerResults = Task.Run(() => ieltsAnswerResultService.GetIeltsQuestionResult(i.Id)).Result
                        }).ToList();
            return data;
        }
        public async Task<double?> GetMaxPoint(int ieltsQuestionId)
        {
            return (await dbContext.tbl_IeltsQuestion.SingleOrDefaultAsync(x => x.Id == ieltsQuestionId))
                ?.Point;
        }
        public async Task<IeltsResult> Insert(IeltsQuestionResultCreate itemModel, tbl_UserInformation userLog)
        {
            var ieltsQuestion = await dbContext.tbl_IeltsQuestion.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsQuestionId);
            if (ieltsQuestion == null)
                throw new Exception("Không tìm thấy câu hỏi");
            var ieltsQuestionGroup = await dbContext.tbl_IeltsQuestionGroup.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsQuestionGroupId);
            if (ieltsQuestionGroup == null)
                throw new Exception("Không tìm thấy nhóm câu");
            var model = new tbl_IeltsQuestionResult(itemModel);
            model.Content = ieltsQuestion.Content;
            model.SampleAnswer = ieltsQuestion.SampleAnswer;
            model.Explain = ieltsQuestion.Explain;
            model.Index = ieltsQuestion.Index;
            model.InputId = ieltsQuestion.InputId;
            model.Point = ieltsQuestion.Point;
            model.Audio = ieltsQuestion.Audio;
            model.Correct = false;
            model.CreatedBy = model.ModifiedBy = userLog.FullName;
            dbContext.tbl_IeltsQuestionResult.Add(model);
            await dbContext.SaveChangesAsync();

            var ieltsAnswerResultService = new IeltsAnswerResultService(dbContext);
            var doingTestDetails = await dbContext.tbl_DoingTestDetail.Where(x => x.DoingTestId == itemModel.DoingTestId
            && x.IeltsQuestionId == model.IeltsQuestionId
            && x.Enable == true).ToListAsync();

            if (ieltsQuestionGroup.Type == 7 || ieltsQuestionGroup.Type == 8)// Bài tự luận
            {
                model.Point = null;
                await dbContext.SaveChangesAsync();
                var myAnswer = doingTestDetails.FirstOrDefault();
                await ieltsAnswerResultService.Insert(new IeltsAnswerResultCreate
                {
                    Correct = false,
                    IeltsAnswerContent = "",
                    IeltsAnswerId = 0,
                    IeltsQuestionId = model.IeltsQuestionId,
                    IeltsQuestionResultId = model.Id,
                    Index = 0,
                    MyChoice = myAnswer != null ? true : false,
                    MyIeltsAnswerContent = myAnswer?.IeltsAnswerContent ?? "",
                    MyIeltsAnswerId = myAnswer?.IeltsAnswerId ?? 0,
                    MyIndex = myAnswer?.Index ?? 0,
                    Type = 1
                }, userLog);
            }
            else
            {
                var ieltsAnswers = await dbContext.tbl_IeltsAnswer.Where(x => x.IeltsQuestionId == model.IeltsQuestionId && x.Enable == true).ToListAsync();
                if (ieltsAnswers.Any())
                {
                    foreach (var item in ieltsAnswers)
                    {
                        var myAnswer = doingTestDetails.FirstOrDefault(x => x.IeltsAnswerId == item.Id);
                        if (ieltsQuestionGroup.Type == 3)
                            myAnswer = doingTestDetails.OrderByDescending(x => x.Id).FirstOrDefault();
                        if (ieltsQuestionGroup.Type == 2)
                        {
                            myAnswer = doingTestDetails.OrderByDescending(x => x.Id).FirstOrDefault();
                            if (!item.Correct)
                                continue;
                        }

                        bool myChoice = myAnswer != null ? true : false;

                        await ieltsAnswerResultService.Insert(new IeltsAnswerResultCreate
                        {
                            Correct = item.Correct,
                            IeltsAnswerContent = item.Content ?? "",
                            IeltsAnswerId = item.Id,
                            IeltsQuestionId = model.IeltsQuestionId,
                            IeltsQuestionResultId = model.Id,
                            Index = item.Index,
                            MyChoice = myChoice,
                            MyIeltsAnswerContent = myAnswer?.IeltsAnswerContent ?? "",
                            MyIeltsAnswerId = myAnswer?.IeltsAnswerId ?? 0,
                            MyIndex = myAnswer?.Index ?? 0,
                            Type = item.Type
                        }, userLog);
                    }
                }
            }
            if (ieltsQuestionGroup.Type == 4)
            {
                model.AnswerOfMindmap = doingTestDetails.FirstOrDefault()?.IeltsAnswerId ?? 0;
            }
            //Chấm bài
            if (ieltsQuestionGroup.Type != 7 && ieltsQuestionGroup.Type != 8)
            {
                model.Correct = await this.Grading(model, ieltsQuestionGroup.Type);
                await dbContext.SaveChangesAsync();
            }
            return new IeltsResult
            {
                MyPoint = model.Correct.Value ? (model.Point ?? 0) : 0,
                QuestionsMultipleChoiceCorrect = model.Correct.Value ? 1 : 0,
                QuestionsDifficultCorrect = (model.Correct.Value && ieltsQuestionGroup.Level == 3) ? 1 : 0,
                QuestionsNormalCorrect = (model.Correct.Value && ieltsQuestionGroup.Level == 2) ? 1 : 0,
                QuestionsEasyCorrect = (model.Correct.Value && ieltsQuestionGroup.Level == 1) ? 1 : 0,
            };
        }
        /// <summary>
        /// Chấm bài
        /// </summary>
        /// <param name="ieltsQuestionResult"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<bool> Grading(tbl_IeltsQuestionResult ieltsQuestionResult, int type)
        {
            bool result = true;
            var ieltsAnswerResults = await dbContext.tbl_IeltsAnswerResult.Where(x => x.IeltsQuestionResultId == ieltsQuestionResult.Id && x.Enable == true).ToListAsync();
            switch (type)
            {
                case 1://Trắc nghiệm
                    {
                        if (ieltsAnswerResults.Any(x => x.Correct != x.MyChoice))
                            result = false;
                        break;
                    }
                case 2://Chọn từ vào ô trống
                    {
                        result = false;
                        if (!ieltsAnswerResults.Any())
                            result = false;
                        var myAnswerResult = ieltsAnswerResults.FirstOrDefault(x => x.Correct && x.IeltsAnswerId == x.MyIeltsAnswerId);
                        if (myAnswerResult != null)
                            result = true;
                        else
                            result = false;
                        break;
                    }
                case 3://Điền vào ô trống
                    {
                        if (!ieltsAnswerResults.Any())
                            result = false;
                        bool correct = (from i in ieltsAnswerResults
                                        where RevokeSpecialCharacters(i.IeltsAnswerContent.ToLower()).Trim() == RevokeSpecialCharacters(i.MyIeltsAnswerContent.ToLower()).Trim()
                                        select i).Any();
                        //if (!ieltsAnswerResults.Any(x => x.IeltsAnswerContent.Trim().ToUpper() == x.MyIeltsAnswerContent.Trim().ToUpper()))
                        if (!correct)
                            result = false;
                        break;
                    }
                case 4://Mindmap
                    {
                        if (!ieltsAnswerResults.Any(x => x.MyIeltsAnswerId != 0))
                            result = false;
                        if (ieltsAnswerResults.Any(x => x.MyChoice == x.Correct && x.MyIeltsAnswerId != 0))
                            result = true;
                        else result = false;
                        break;
                    }
                case 5://True/False/Not Given
                    {
                        if (ieltsAnswerResults.Any(x => x.Correct != x.MyChoice))
                            result = false;
                        break;
                    }

                case 6://Sắp xếp câu 
                    {
                        if (!ieltsAnswerResults.Any())
                            result = false;
                        if (ieltsAnswerResults.Any(x => x.Index != x.MyIndex))
                            result = false;
                        break;
                    }
            }
            return result;
        }

        public static string RevokeSpecialCharacters(string text)
        {
            string result = "";
            string[] arr = new string[] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "a", "s", "d", "f", "g", "h", "j",
            "1","2","3","4","5","6","7","8","9","0",
            "k","l","z","x","c","v","b","n","m"};

            if (text.Length > 0)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (arr.Contains(text[i].ToString()))
                        result += text[i].ToString();
                }
            }
            return result;
        }
        public class GradingEssayRequest
        {
            public int Id { get; set; }
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
                    var ieltsQuestionResult = await dbContext.tbl_IeltsQuestionResult.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                    if (ieltsQuestionResult == null)
                        throw new Exception("Không tìm thấy câu hỏi");
                    double outPoint = ieltsQuestionResult.Point ?? 0;
                    var ieltsQuestion = await dbContext.tbl_IeltsQuestion.SingleOrDefaultAsync(x => x.IeltsQuestionGroupId == ieltsQuestionResult.IeltsQuestionId);
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
                                    ModifiedOn = DateTime.Now
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
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    //Tính lại tổng điểm
                    double differencePoint = itemModel.Point - outPoint;
                    var ieltsQuestionGroupResult = await dbContext.tbl_IeltsQuestionGroupResult.SingleOrDefaultAsync(x => x.Id == ieltsQuestionResult.IeltsQuestionGroupResultId);
                    if (ieltsQuestionGroupResult == null)
                        return;
                    var ieltsSectionResult = await dbContext.tbl_IeltsSectionResult.SingleOrDefaultAsync(x => x.Id == ieltsQuestionGroupResult.IeltsSectionResultId);
                    if (ieltsSectionResult == null)
                        return;
                    var ieltsSkillResult = await dbContext.tbl_IeltsSkillResult.SingleOrDefaultAsync(x => x.Id == ieltsSectionResult.IeltsSkillId);
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
        public async Task<List<tbl_IeltsAnswerResultComment>> GetAnswerComment(AnswerCommentSearch baseSearch)
        {
            var data = await dbContext.tbl_IeltsAnswerResultComment
                .Where(x => x.IeltsQuestionResultId == baseSearch.IeltsQuestionResultId && x.IeltsAnswerResultId == baseSearch.IeltsAnswerResultId && x.Enable == true)
                .ToListAsync();
            return data;
        }
    }
}