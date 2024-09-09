using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;


namespace LMSCore.Services
{
    public class DoingTestService : DomainService
    {
        public DoingTestService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_DoingTest> GetById(int id)
        {
            var data = await dbContext.tbl_DoingTest.SingleOrDefaultAsync(x => x.Id == id);
            if (data != null)
            {
                if (!string.IsNullOrEmpty(data.TimeSpentOfSkill))
                    data.TimeSpentOfSkillDTO = JsonConvert.DeserializeObject<TimeSpentOfSkillDTO>(data.TimeSpentOfSkill);
            }
            return data;
        }
        public async Task<tbl_DoingTest> Insert(DoingTestCreate itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var ieltsExam = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsExamId);
                    if (ieltsExam == null)
                        throw new Exception("Không tìm thấy để thi");
                    var firstSkill = await dbContext.tbl_IeltsSkill.Where(x => x.IeltsExamId == ieltsExam.Id && x.Enable == true)
                        .OrderBy(x => x.Index).FirstOrDefaultAsync();
                    if (firstSkill == null)
                        throw new Exception("Đề chưa được cấu hình kỹ năng, vui lòng liên hệ quản trị viên để được hỗ trợ!");
                    var firstSection = await dbContext.tbl_IeltsSection.Where(x => x.IeltsSkillId == firstSkill.Id && x.Enable == true)
                        .OrderBy(x => x.Index).FirstOrDefaultAsync();
                    if (firstSection == null)
                        throw new Exception($"Kỹ năng {firstSkill.Name} chưa được cấu hình nội dung, vui lòng liên hệ quản trị viên để được hỗ trợ!");
                    if (itemModel.Type != 1)
                    {
                        var draft = await dbContext.tbl_DoingTest
                            .AnyAsync(x => x.Type == itemModel.Type && x.StudentId == userLog.UserInformationId && x.ValueId == itemModel.ValueId && x.Enable == true && x.Status == 1);
                        if (draft)
                        {
                            throw new Exception("Hãy làm bài làm trước đó trước khi bạn làm bài mới");
                        }
                    }

                    if (itemModel.Type != 1)
                    {
                        //Kiểm tra lại từng loại
                        if (itemModel.Type == 3)// bài tập về nhìn
                        {
                            var homeworkData = await dbContext.tbl_Homework.Where(x => x.Enable == true).ToListAsync();
                            tbl_Homework homework = homeworkData.SingleOrDefault(x => x.Id == itemModel.ValueId && x.Enable == true);
                            var homeworkInclass = homeworkData
                                .Where(x => x.ClassId == homework.ClassId && x.Enable == true && x.Index < homework.Index && x.Index.HasValue)
                                .OrderBy(x => x.Index)
                                .ToList();
                            var homeworkConfig = await dbContext.tbl_HomeworkSequenceConfigInClass.FirstOrDefaultAsync(x => x.Enable == true && x.ClassId == homework.ClassId);
                            var ieltsExamResultData = await dbContext.tbl_IeltsExamResult.Where(x => x.Enable == true).ToListAsync();
                            var homeworkResultData = await dbContext.tbl_HomeworkResult.Where(x => x.Enable == true).ToListAsync();

                            // Check bài tập tuần tự, theo buổi, theo điểm sản
                            if (homeworkConfig.IsAllow == true)
                            {
                                foreach (var h in homeworkInclass)
                                {
                                    if (h.Type == HomeworkType.Exam)
                                    {
                                        var ieltsExamResult = ieltsExamResultData.Where(x => x.ValueId == h.Id && x.StudentId == userLog.UserInformationId).ToList();

                                        if (h.CutoffScore.HasValue)
                                        {
                                            if (ieltsExamResult.Any(x => x.Status != 1))
                                            {

                                                if (!ieltsExamResult.Any(x => x.IsPassed == true))
                                                {
                                                    throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                                }
                                            }
                                            else
                                                throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                        }
                                        else if (!ieltsExamResult.Any())
                                            throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                    }
                                    else if (h.Type == HomeworkType.Homework)
                                    {
                                        var homeworkResult = homeworkResultData.Where(x => x.HomeworkId == h.Id && x.StudentId == userLog.UserInformationId).ToList();

                                        if (h.CutoffScore.HasValue)
                                        {
                                            if (!homeworkResult.Any(x => x.Type == HomeworkResultType.GotPoint))
                                            {

                                                if (!homeworkResult.Any(x => x.IsPassed == true))
                                                {
                                                    throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                                }
                                            }
                                            else
                                                throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                        }
                                        else if (!homeworkResult.Any())
                                            throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                    }
                                }
                            }
                            if (homework.SessionNumber.HasValue && homework.SessionNumber != 0)
                            {
                                var schedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true && x.ClassId == homework.ClassId).OrderBy(x => x.StartTime).ToListAsync();
                                var scheduleElement = schedule.ElementAtOrDefault((int)(homework.SessionNumber - 1));
                                if (scheduleElement != null)
                                {
                                    if (scheduleElement.StartTime > DateTime.Now)
                                        throw new Exception("Bạn chưa được phép làm bài tập này do chưa đến buổi học ngày " + scheduleElement.StartTime.Value.ToString("dd/MM/yyyy HH:mm"));
                                }
                            }
                            if (homework == null)
                            {
                                throw new Exception("Không tìm thấy bài tập");
                            }
                            if (homework.ToDate.HasValue)
                                if (DateTime.Now.Date > homework.ToDate.Value.Date)
                                    throw new Exception("Bài tập đã hết hạn không thể làm");
                            //tbl_StudentHomework studentHomework = await dbContext.tbl_StudentHomework.FirstOrDefaultAsync(x => x.Enable == true && x.StudentId == userLog.UserInformationId && x.HomeworkId == homework.Id);
                            tbl_StudentHomework studentHomework = new tbl_StudentHomework
                            {
                                HomeworkId = homework.Id,
                                ClassId = homework.ClassId,
                                StudentId = userLog.UserInformationId,
                                IeltsExamId = ieltsExam.Id,
                                Status = (int)StudentHomeworkStatus.DangLam,
                                StatusName = ListStudentHomeworkStatus().SingleOrDefault(x => x.Key == (int)StudentHomeworkStatus.DangLam)?.Value,
                                FromDate = null,
                                ToDate = null,
                                Time = ieltsExam.Time,
                                Point = ieltsExam.Point,
                                CreatedBy = userLog.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true
                            };
                            dbContext.tbl_StudentHomework.Add(studentHomework);
                            await dbContext.SaveChangesAsync();

                        }
                    }
                    var timeSpentOfSkill = new TimeSpentOfSkillDTO
                    {
                        IeltsSectionId = firstSection.Id,
                        IeltsSkillId = firstSkill.Id,
                        StartTime = DateTime.Now,
                        Time = ieltsExam.Time
                    };
                    var model = new tbl_DoingTest(itemModel);
                    model.TimeSpentOfSkill = JsonConvert.SerializeObject(timeSpentOfSkill);
                    model.TimeSpentOfSkillDTO = timeSpentOfSkill;
                    model.StartTime = DateTime.Now;
                    model.StudentId = userLog.UserInformationId;
                    model.CreatedBy = model.ModifiedBy = userLog.FullName;
                    dbContext.tbl_DoingTest.Add(model);
                    await dbContext.SaveChangesAsync();
                    var doingTests = await dbContext.tbl_DoingTest
                        .Where(x => x.Type == itemModel.Type
                        && x.StudentId == userLog.UserInformationId
                        && x.ValueId == itemModel.ValueId
                        && x.Enable == true
                        && x.Status == 1
                        && x.Id != model.Id)
                        .ToListAsync();
                    if (doingTests.Any())
                    {
                        foreach (var item in doingTests)
                        {
                            item.Status = 3;
                            item.StatusName = "Đã hủy";
                        }
                        await dbContext.SaveChangesAsync();
                    }
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
        public async Task InsertCurrentSection(DoingTestCurrentSection itemModel)  
        {
            var doingTest = await dbContext.tbl_DoingTest.SingleOrDefaultAsync(x => x.Id == itemModel.DoingTestId);
            if (doingTest == null)
                throw new Exception("Không tìm thấy bài làm");
            if (string.IsNullOrEmpty(doingTest.TimeSpentOfSkill))
                throw new Exception("Có lỗi xảy ra, vui lòng liên hệ quản trị viên để được hỗ trợ!");
            var ieltsSection = await dbContext.tbl_IeltsSection.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsSectionId && x.Enable == true);
            if (ieltsSection == null)
                throw new Exception("Đề chưa được cấu hình đầy đủ nội dung, vui lòng liên hệ quản trị viên để được hỗ trợ!");
            var timeSpentOfSkillDTO = JsonConvert.DeserializeObject<TimeSpentOfSkillDTO>(doingTest.TimeSpentOfSkill);
            timeSpentOfSkillDTO.IeltsSkillId = ieltsSection.IeltsSkillId;
            timeSpentOfSkillDTO.IeltsSectionId = ieltsSection.Id;
            doingTest.TimeSpentOfSkill = JsonConvert.SerializeObject(timeSpentOfSkillDTO);
            await dbContext.SaveChangesAsync();
        }
        public async Task<int> GetTotalQuestionUncompleted(int doingTestId)
        {
            var doingTest = await dbContext.tbl_DoingTest.SingleOrDefaultAsync(x => x.Id == doingTestId);
            if (doingTest == null)
                return 0;
            var ieltsExam = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == doingTest.IeltsExamId);
            if (ieltsExam == null)
                return 0;

            var doingTestDetails = await dbContext.tbl_DoingTestDetail
                .Where(x => x.DoingTestId == doingTestId && x.Enable == true).ToListAsync();
            if (!doingTestDetails.Any())
                return ieltsExam.QuestionsAmount;
            return ieltsExam.QuestionsAmount - doingTestDetails.Select(x => x.IeltsQuestionId).Distinct().Count();
        }
        public async Task<int> GetTotalQuestionUncompletedSkill(TotalQuestionUncompletedSkillSearch baseSearch)
        {
            var doingTest = await dbContext.tbl_DoingTest.SingleOrDefaultAsync(x => x.Id == baseSearch.DoingTestId);
            if (doingTest == null)
                return 0;
            var questionInSkills = await dbContext.tbl_IeltsQuestion.Where(x => x.IeltsSkillId == baseSearch.IeltsSkillId && x.Enable == true)
                .Select(x => x.Id).ToListAsync();
            if (!questionInSkills.Any())
                return 0;
            int countCompleted = await dbContext.tbl_DoingTestDetail.Where(x => x.DoingTestId == baseSearch.DoingTestId && questionInSkills.Contains(x.IeltsQuestionId) && x.Enable == true)
                .Select(x => x.IeltsQuestionId).Distinct().CountAsync();
            return questionInSkills.Count() - countCompleted;

        }
        public class SaveTimeModel
        {
            public int DoingTestId { get; set; }
            /// <summary>
            /// Thời gian làm bài
            /// </summary>
            public int? TimeSpent { get; set; }
            public List<SaveTimeItem> Items { get; set; }
        }
        public class SaveTimeItem
        {
            public int IeltsSkillId { get; set; }
            /// <summary>
            /// Thời gian làm bài
            /// </summary>
            public int TimeSpent { get; set; }
        }
        public async Task SaveTime(SaveTimeModel itemModel)
        {
            var doingTest = await dbContext.tbl_DoingTest.SingleOrDefaultAsync(x => x.Id == itemModel.DoingTestId);
            if (doingTest == null)
                throw new Exception("Không tìm thấy dữ liệu");
            doingTest.TimeSpent = itemModel.TimeSpent ?? doingTest.TimeSpent;
            doingTest.TimeSpentOfSkill = JsonConvert.SerializeObject(itemModel.Items);
            await dbContext.SaveChangesAsync();
        }
        public class DoingTestDraftSearch
        {
            public int ValueId { get; set; }
            /// <summary>
            /// 1 - Làm bài thử
            /// 2 - Làm bài hẹn test 
            /// 3 - Bài tập về nhà
            /// 4 - Bộ đề
            /// </summary>
            [Required(ErrorMessage = "Vui lòng chọn loại")]
            public int? Type { get; set; }
        }
        public async Task<tbl_DoingTest> GetDraft(DoingTestDraftSearch baseSearch, tbl_UserInformation userLog)
        {
            var draft = await dbContext.tbl_DoingTest
                .FirstOrDefaultAsync(x => x.Type == baseSearch.Type && x.StudentId == userLog.UserInformationId && x.ValueId == baseSearch.ValueId && x.Enable == true && x.Status == 1);
            return draft;
        }
        public class DoingTestDetailInsertOrUpdates
        {
            [Required(ErrorMessage = "Vui lòng chọn bản nháp")]
            public int? DoingTestId { get; set; }
            /// <summary>
            /// Câu hỏi
            /// </summary>
            public int IeltsQuestionId { get; set; }
            public List<DoingTestDetailInsertOrUpdate> Items { get; set; }
        }
        public class DoingTestDetailInsertOrUpdate
        {
            /// <summary>
            /// Khi tạo mới không cần truyền Id
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// Đáp án chọn
            /// </summary>
            public int? IeltsAnswerId { get; set; }
            /// <summary>
            /// Nội dung đáp án
            /// </summary>
            public string IeltsAnswerContent { get; set; }
            public int? Type { get; set; }
            public int? Index { get; set; }
            public bool? Enable { get; set; }
        }
        public async Task<tbl_IeltsQuestionGroup> InsertOrUpdateDetails(DoingTestDetailInsertOrUpdates itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var doingTest = await dbContext.tbl_DoingTest.SingleOrDefaultAsync(x => x.Id == itemModel.DoingTestId);
                    if (doingTest == null)
                        throw new Exception("Không tìm thấy bản nháp");
                    if (doingTest.Status != 1)
                        throw new Exception($"Bản nháp {doingTest.StatusName}");
                    var ieltsQuestion = await dbContext.tbl_IeltsQuestion.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsQuestionId);
                    if (ieltsQuestion == null)
                        throw new Exception("Không tìm thấy câu hỏi");
                    if (itemModel.Items.Any())
                    {
                        foreach (var item in itemModel.Items)
                        {
                            if (item.Id == 0)
                            {
                                var IeltsAnswer = await dbContext.tbl_IeltsAnswer.SingleOrDefaultAsync(x => x.Id == item.IeltsAnswerId);
                                var detail = new tbl_DoingTestDetail
                                {
                                    CreatedBy = userLog.CreatedBy,
                                    CreatedOn = DateTime.Now,
                                    DoingTestId = doingTest.Id,
                                    Enable = true,
                                    IeltsAnswerContent = IeltsAnswer != null ? IeltsAnswer.Content : item.IeltsAnswerContent,
                                    IeltsAnswerId = item.IeltsAnswerId ?? 0,
                                    IeltsQuestionGroupId = ieltsQuestion.IeltsQuestionGroupId,
                                    IeltsQuestionId = ieltsQuestion.Id,
                                    Index = item.Index ?? 0,
                                    ModifiedBy = userLog.FullName,
                                    ModifiedOn = DateTime.Now,
                                    Type = item.Type ?? 0
                                };
                                dbContext.tbl_DoingTestDetail.Add(detail);
                            }
                            else
                            {
                                var detail = await dbContext.tbl_DoingTestDetail.SingleOrDefaultAsync(x => x.Id == item.Id);
                                if (detail == null)
                                    throw new Exception("Không tìm thấy dữ liệu");
                                var IeltsAnswer = await dbContext.tbl_IeltsAnswer.SingleOrDefaultAsync(x => x.Id == detail.IeltsAnswerId);
                                detail.IeltsAnswerContent = string.IsNullOrEmpty(item.IeltsAnswerContent) ? IeltsAnswer?.Content : item.IeltsAnswerContent;
                                detail.IeltsAnswerId = item.IeltsAnswerId ?? detail.IeltsAnswerId;
                                detail.Index = item.Index ?? detail.Index;
                                detail.Type = item.Type ?? detail.Type;
                                detail.Enable = item.Enable ?? detail.Enable;
                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    tran.Commit();
                    return await GetIeltsQuestionGroup(new DoingTestIeltsQuestionGroupSearch
                    {
                        IeltsQuestionGroupId = ieltsQuestion.IeltsQuestionGroupId,
                        DoingTestId = itemModel.DoingTestId.Value
                    });
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw e;
                }
            }
        }
        public async Task<List<DoingTestDetailModel>> GetDetail(int ieltsQuestionId, int doingTestId)
        {
            var data = await dbContext.tbl_DoingTestDetail.Where(x => x.IeltsQuestionId == ieltsQuestionId && x.DoingTestId == doingTestId && x.Enable == true).ToListAsync();
            if (!data.Any())
                return null;
            return data.Select(x => new DoingTestDetailModel
            {
                Id = x.Id,
                IeltsAnswerContent = x.IeltsAnswerContent,
                IeltsAnswerId = x.IeltsAnswerId,
                Index = x.Index,
                Type = x.Type
            }).OrderBy(x => x.Index).ToList();
        }
        public class DoingTestIeltsQuestionGroupSearch
        {
            public int IeltsQuestionGroupId { get; set; }
            public int DoingTestId { get; set; }
        }
        public async Task<tbl_IeltsQuestionGroup> GetIeltsQuestionGroup(DoingTestIeltsQuestionGroupSearch baseSearch)
        {
            var ieltsQuestionService = new IeltsQuestionService(dbContext);
            var ieltsQuestionGroupService = new IeltsQuestionGroupService(dbContext);
            var data = await dbContext.tbl_IeltsQuestionGroup.SingleOrDefaultAsync(x => x.Id == baseSearch.IeltsQuestionGroupId);
            data.TagNames = await ieltsQuestionGroupService.GetTagName(data.TagIds);
            data.IeltsQuestions = await ieltsQuestionService.GetByIeltsQuestionGroup(data.Id, data.Type, baseSearch.DoingTestId);
            return data;
        }
    }
}