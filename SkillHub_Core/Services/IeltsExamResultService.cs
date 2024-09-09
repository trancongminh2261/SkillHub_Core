using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class IeltsExamResultService : DomainService
    {
        public IeltsExamResultService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_IeltsExamResult> GetById(int id)
        {
            var data = await dbContext.tbl_IeltsExamResult.SingleOrDefaultAsync(x => x.Id == id);
            data.StudentName = (await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.StudentId))?.FullName;
            data.TeacherName = (await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.TeacherId))?.FullName;
            return await dbContext.tbl_IeltsExamResult.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<tbl_IeltsExamResult> Insert(IeltsExamResultCreate itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    //var doingTest = await dbContext.tbl_DoingTest.SingleOrDefaultAsync(x => x.Id == itemModel.DoingTestId && x.Status == 1);
                    var doingTest = await dbContext.tbl_DoingTest.SingleOrDefaultAsync(x => x.Id == itemModel.DoingTestId);
                    tbl_Homework homework = await dbContext.tbl_Homework.SingleOrDefaultAsync(x => x.Id == doingTest.ValueId && doingTest.Type == 3);
                    int classId = homework?.ClassId ?? 0;
                    tbl_Class _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == classId);
                    if (doingTest == null)
                        throw new Exception("Không tìm thấy bài làm");
                    if (userLog.UserInformationId != doingTest.StudentId)
                        throw new Exception("Bạn không thể nộp bài của học viên khác");
                    var student = await dbContext.tbl_UserInformation
                        .SingleOrDefaultAsync(x => x.UserInformationId == doingTest.StudentId);
                    var ieltsExam = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == doingTest.IeltsExamId);
                    int teacherId = 0;
                    if (doingTest.Type != 1)
                    {
                        //Kiểm tra theo từng loại
                        if (doingTest.Type == 3 || doingTest.Type == 6)
                        {
                            if (homework != null)
                            {
                                tbl_StudentHomework studentHomework = await dbContext.tbl_StudentHomework
                                    .Where(x => x.Enable == true && x.StudentId == userLog.UserInformationId && x.HomeworkId == doingTest.ValueId).OrderByDescending(x => x.CreatedOn).FirstOrDefaultAsync();
                                if (studentHomework != null)
                                {
                                    studentHomework.TimeSpent = doingTest.TimeSpent > ieltsExam.Time ? ieltsExam.Time : doingTest.TimeSpent;
                                    studentHomework.ToDate = DateTime.Now;
                                    studentHomework.Status = (int)StudentHomeworkStatus.DaNop;
                                    studentHomework.StatusName = ListStudentHomeworkStatus().SingleOrDefault(x => x.Key == studentHomework.Status)?.Value;
                                    await dbContext.SaveChangesAsync();
                                }
                                teacherId = homework.TeacherId ?? 0;
                            }
                        }
                        else if (doingTest.Type == 2)
                        {
                            var testAppointment = await dbContext.tbl_TestAppointment.SingleOrDefaultAsync(x => x.Id == doingTest.ValueId);
                            if (testAppointment == null)
                                throw new Exception("Không tìm thấy bài hẹn test");
                            student.LearningStatus = 2;
                            student.LearningStatusName = tbl_UserInformation.GetLearningStatusName(2);
                            await dbContext.SaveChangesAsync();
                            teacherId = testAppointment.TeacherId ?? 0;
                        }
                    }
                    int timeSpent = (int)DateTime.Now.Subtract(doingTest.StartTime).TotalMinutes;
                    var model = new tbl_IeltsExamResult
                    {
                        Active = ieltsExam.Active,
                        Code = ieltsExam.Code,
                        CreatedBy = userLog.FullName,
                        CreatedOn = DateTime.Now,
                        Description = ieltsExam.Description,
                        Enable = true,
                        IeltsExamId = ieltsExam.Id,
                        ModifiedBy = userLog.FullName,
                        ModifiedOn = DateTime.Now,
                        Name = ieltsExam.Name,
                        DoingTestId = itemModel.DoingTestId,
                        Point = ieltsExam.Point,
                        QuestionsAmount = ieltsExam.QuestionsAmount,
                        QuestionsDifficult = ieltsExam.QuestionsDifficult,
                        QuestionsEasy = ieltsExam.QuestionsEasy,
                        QuestionsNormal = ieltsExam.QuestionsNormal,
                        StudentId = doingTest.StudentId,
                        Thumbnail = ieltsExam.Thumbnail,
                        Time = ieltsExam.Time,
                        StartTime = doingTest.StartTime,
                        TimeSpent = timeSpent > ieltsExam.Time ? ieltsExam.Time : timeSpent,
                        Type = doingTest.Type,
                        ValueId = doingTest.ValueId,
                        TeacherId = teacherId,
                        Status = 1,
                        StatusName = "Đang chấm bài",
                        QuestionMultipleChoiceAmount = ieltsExam.QuestionMultipleChoiceAmount,
                        QuestionEssayAmount = ieltsExam.QuestionEssayAmount,
                        QuestionsMultipleChoiceCorrect = 0,
                        QuestionsDifficultCorrect = 0,
                        QuestionsEasyCorrect = 0,
                        QuestionsNormalCorrect = 0,
                        Note = "",
                        MyPoint = 0,
                        AveragePoint = 0,
                    };
                    dbContext.tbl_IeltsExamResult.Add(model);
                    doingTest.Status = 2;
                    doingTest.StatusName = "Đã nộp";
                    await dbContext.SaveChangesAsync();
                    int QuestionsMultipleChoiceCorrect = 0;
                    int questionsDifficultCorrect = 0;
                    int questionsEasyCorrect = 0;
                    int questionsNormalCorrect = 0;
                    double myPoint = 0;
                    double averagePoint = 0;
                    var ieltsSkills = await dbContext.tbl_IeltsSkill.Where(x => x.IeltsExamId == model.IeltsExamId && x.Enable == true).ToListAsync();
                    if (ieltsSkills.Any())
                    {
                        var ieltsSkillResult = new IeltsSkillResultService(dbContext);
                        foreach (var item in ieltsSkills)
                        {
                            var data = await ieltsSkillResult.Insert(new IeltsSkillResultCreate
                            {
                                DoingTestId = itemModel.DoingTestId,
                                IeltsExamResultId = model.Id,
                                TimeSpentOfSkill = doingTest.TimeSpentOfSkill,
                                IeltsSkillId = item.Id
                            }, userLog);
                            QuestionsMultipleChoiceCorrect += data.QuestionsMultipleChoiceCorrect;
                            questionsDifficultCorrect += data.QuestionsDifficultCorrect;
                            questionsEasyCorrect += data.QuestionsEasyCorrect;
                            questionsNormalCorrect += data.QuestionsNormalCorrect;
                            myPoint += data.MyPoint;
                        }
                        averagePoint = Math.Round((myPoint / ieltsSkills.Count()), 2);
                    }
                    model.QuestionsMultipleChoiceCorrect = QuestionsMultipleChoiceCorrect;
                    model.QuestionsDifficultCorrect = questionsDifficultCorrect;
                    model.QuestionsEasyCorrect = questionsEasyCorrect;
                    model.QuestionsNormalCorrect = questionsNormalCorrect;
                    model.MyPoint = myPoint;
                    model.AveragePoint = averagePoint;
                    if (!await dbContext.tbl_IeltsQuestionGroup
                        .AnyAsync(x => x.IeltsExamId == doingTest.IeltsExamId && x.Enable == true && (x.Type == 7 || x.Type == 8) && x.QuestionsAmount > 0))
                    {
                        model.Status = 3;
                        model.StatusName = "Đã chấm xong";
                        if (homework.CutoffScore.HasValue)
                        {
                            if (model.MyPoint < homework.CutoffScore)
                            {
                                model.IsPassed = false;
                            }
                            else
                                model.IsPassed = true;
                        }

                    }
                    await dbContext.SaveChangesAsync();

                    //UrlNotificationModels urlNotification = new UrlNotificationModels();
                    //string url = urlNotification.urlDetailIeltsExamResult + model.Id;
                    ////string urlEmail = urlNotification.url + url;
                    //if (_class != null)
                    //{
                    //    // Gửi cho học sinh
                    //    Thread sendStudent = new Thread(async () =>
                    //    {
                    //        tbl_Notification notification = new tbl_Notification();

                    //        notification.Title = "THÔNG BÁO BÀI TẬP ĐÃ ĐƯỢC CHẤM";
                    //        notification.Content = "Bài tập (<b>" + homework.Name + "</b>) với bộ đề (<b>" + ieltsExam.Name + "</b>) của bạn ở lớp " + _class.Name + " đã được giáo viên chấm. Vui lòng kiểm tra!";
                    //        notification.Type = 5;
                    //        notification.UserId = student.UserInformationId;
                    //        notification.Category = 0;
                    //        notification.Url = url;
                    //        notification.AvailableId = model.Id;
                    //        await NotificationService.Send(notification, userLog, false);
                    //    });
                    //    sendStudent.Start();

                    //    // Gửi cho phụ huynh
                    //    Thread sendParent = new Thread(async () =>
                    //    {
                    //        tbl_Notification notification = new tbl_Notification();

                    //        notification.Title = "THÔNG BÁO BÀI TẬP ĐÃ ĐƯỢC CHẤM";
                    //        notification.Content = "Bài tập (<b>" + homework.Name + "</b>) với bộ đề (<b>" + ieltsExam.Name + "</b>) của học sinh " + student.FullName + " ở lớp " + _class.Name + " đã được giáo viên chấm. Vui lòng kiểm tra!";
                    //        notification.Type = 5;
                    //        notification.UserId = student.ParentId;
                    //        notification.Category = 0;
                    //        notification.Url = url;
                    //        notification.AvailableId = model.Id;
                    //        await NotificationService.Send(notification, userLog, false);
                    //    });
                    //    sendParent.Start();
                    //}

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
        public class IeltsExamResultReview
        {
            public int Id { get; set; }
            public string Note { get; set; }
            public List<IeltsSkillResultReview> Items { get; set; }
        }
        public class IeltsSkillResultReview
        {
            public int IeltsSkillResultId { get; set; }
            public string Note { get; set; }
        }
        /// <summary>
        /// Giáo viên đánh giá bài làm của học viên
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="userLog"></param>
        /// <returns></returns>
        public async Task Review(IeltsExamResultReview itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var ieltsExamResult = await dbContext.tbl_IeltsExamResult.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                    if (ieltsExamResult == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    var homework = await dbContext.tbl_Homework.FirstOrDefaultAsync(x => x.Id == ieltsExamResult.ValueId);
                    if (homework != null)
                    {
                        if (homework.CutoffScore.HasValue)
                        {
                            if (ieltsExamResult.MyPoint < homework.CutoffScore)
                            {
                                ieltsExamResult.IsPassed = false;
                            }
                            else
                                ieltsExamResult.IsPassed = true;
                        }
                    }
                    ieltsExamResult.Note = itemModel.Note ?? ieltsExamResult.Note;
                    ieltsExamResult.Status = 2;
                    ieltsExamResult.StatusName = "Đã chấm xong";
                    ieltsExamResult.ModifiedBy = userLog.FullName;
                    ieltsExamResult.ModifiedOn = DateTime.Now;
                    await dbContext.SaveChangesAsync();
                    if (itemModel.Items.Any())
                    {
                        foreach (var item in itemModel.Items)
                        {
                            var ieltsSkillResult = await dbContext.tbl_IeltsSkillResult.SingleOrDefaultAsync(x => x.Id == item.IeltsSkillResultId);
                            if (ieltsSkillResult == null)
                                throw new Exception("Không tìm thấy kỹ năng");
                            ieltsSkillResult.Note = item.Note ?? ieltsExamResult.Note;
                            await dbContext.SaveChangesAsync();
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
        public class ChooseTeacherRequest
        {
            public int TeacherId { get; set; }
            public List<int> IeltsExamResultIds { get; set; }
        }
        /// <summary>
        /// Chọn giáo viên chấm bài
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        public async Task ChooseTeacher(ChooseTeacherRequest itemModel)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var teacher = await dbContext.tbl_UserInformation
                        .SingleOrDefaultAsync(x => x.UserInformationId == itemModel.TeacherId && x.Enable == true && x.RoleId == ((int)RoleEnum.teacher));
                    if (teacher == null)
                        throw new Exception("Không tìm thấy giáo viên");
                    if (itemModel.IeltsExamResultIds.Any())
                    {
                        foreach (var item in itemModel.IeltsExamResultIds)
                        {
                            var ieltsExamResult = await dbContext.tbl_IeltsExamResult.SingleOrDefaultAsync(x => x.Id == item);
                            if (ieltsExamResult == null)
                                throw new Exception("Không tìm thấy bài làm");
                            ieltsExamResult.TeacherId = teacher.UserInformationId;
                            await dbContext.SaveChangesAsync();
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
        public async Task<AppDomainResult> GetAll(IeltsExamResultSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new IeltsExamResultSearch();
            string sql = $"Get_IeltsExamResult @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@IeltsExamId = {baseSearch.IeltsExamId}," +
                $"@StudentId = {baseSearch.StudentId}," +
                $"@ValueId = {baseSearch.ValueId}," +
                $"@Type = {baseSearch.Type}," +
                $"@TeacherId = {baseSearch.TeacherId}," +
                $"@Status = {baseSearch.Status}," +
                $"@Search = N'{baseSearch.Search ?? ""}'";
            var data = await dbContext.SqlQuery<Get_IeltsExamResult>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            var result = data.Select(i => new tbl_IeltsExamResult(i)).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        public class IeltsExamResultOverviewSearch
        {
            public int IeltsExamResultId { get; set; }
            public IeltsExamResultOverviewSearch()
            {
                IeltsExamResultId = 0;
            }
        }
        public async Task<IeltsExamResultOverviewModel> GetIeltsExamResultOverview(IeltsExamResultOverviewSearch baseSearch)
        {
            var ieltsExamResult = await GetById(baseSearch.IeltsExamResultId);
            if (ieltsExamResult == null)
                return null;
            string sql = $"Get_IeltsExamResultOverview @IeltsExamResultId = {baseSearch.IeltsExamResultId}";
            var data = await dbContext.SqlQuery<Get_IeltsExamResultOverview>(sql);
            var result = new IeltsExamResultOverviewModel
            {
                Id = ieltsExamResult.Id,
                AveragePoint = ieltsExamResult.AveragePoint,
                Code = ieltsExamResult.Code,
                Description = ieltsExamResult.Description,
                MyPoint = ieltsExamResult.MyPoint,
                Name = ieltsExamResult.Name,
                Note = ieltsExamResult.Note,
                Point = ieltsExamResult.Point,
                QuestionsAmount = ieltsExamResult.QuestionsAmount,
                QuestionEssayAmount = ieltsExamResult.QuestionEssayAmount,
                QuestionMultipleChoiceAmount = ieltsExamResult.QuestionMultipleChoiceAmount,
                QuestionsMultipleChoiceCorrect = ieltsExamResult.QuestionsMultipleChoiceCorrect,
                QuestionsDifficult = ieltsExamResult.QuestionsDifficult,
                QuestionsDifficultCorrect = ieltsExamResult.QuestionsDifficultCorrect,
                QuestionsEasy = ieltsExamResult.QuestionsEasy,
                QuestionsEasyCorrect = ieltsExamResult.QuestionsEasyCorrect,
                QuestionsNormal = ieltsExamResult.QuestionsNormal,
                QuestionsNormalCorrect = ieltsExamResult.QuestionsNormalCorrect,
                StartTime = ieltsExamResult.StartTime,
                Status = ieltsExamResult.Status,
                StatusName = ieltsExamResult.StatusName,
                StudentId = ieltsExamResult.StudentId,
                StudentName = ieltsExamResult.StudentName,
                TeacherId = ieltsExamResult.TeacherId,
                TeacherName = ieltsExamResult.TeacherName,
                Thumbnail = ieltsExamResult.Thumbnail,
                Time = ieltsExamResult.Time,
                TimeSpent = ieltsExamResult.TimeSpent,
                Type = ieltsExamResult.Type,
                ValueId = ieltsExamResult.ValueId,
                IeltsSkillResultOverviews = data.GroupBy(sk => new
                {
                    sk.Id,
                    sk.Audio,
                    sk.Index,
                    sk.MyPoint,
                    sk.Name,
                    sk.Note,
                    sk.Point,
                    sk.QuestionsAmount,
                    sk.QuestionEssayAmount,
                    sk.QuestionMultipleChoiceAmount,
                    sk.QuestionsMultipleChoiceCorrect,
                    sk.QuestionsDifficult,
                    sk.QuestionsDifficultCorrect,
                    sk.QuestionsEasy,
                    sk.QuestionsEasyCorrect,
                    sk.QuestionsNormal,
                    sk.QuestionsNormalCorrect,
                    sk.Time,
                    sk.QuestionEssayGraded
                }).Select(sk => new IeltsSkillResultOverviewModel
                {
                    Id = sk.Key.Id,
                    Audio = sk.Key.Audio,
                    Index = sk.Key.Index,
                    MyPoint = sk.Key.MyPoint,
                    Name = sk.Key.Name,
                    Note = sk.Key.Note,
                    Point = sk.Key.Point,
                    QuestionsAmount = sk.Key.QuestionsAmount,
                    QuestionEssayAmount = sk.Key.QuestionEssayAmount,
                    QuestionMultipleChoiceAmount = sk.Key.QuestionMultipleChoiceAmount,
                    QuestionsMultipleChoiceCorrect = sk.Key.QuestionsMultipleChoiceCorrect,
                    QuestionsDifficult = sk.Key.QuestionsDifficult,
                    QuestionsDifficultCorrect = sk.Key.QuestionsDifficultCorrect,
                    QuestionsEasy = sk.Key.QuestionsEasy,
                    QuestionsEasyCorrect = sk.Key.QuestionsEasyCorrect,
                    QuestionsNormal = sk.Key.QuestionsNormal,
                    QuestionsNormalCorrect = sk.Key.QuestionsNormalCorrect,
                    Time = sk.Key.Time,
                    QuestionEssayGraded = sk.Key.QuestionEssayGraded,
                    IeltsSectionResultOverviews = sk.GroupBy(s => new
                    {
                        s.IeltsSectionResultId,
                        s.IeltsSectionResultAudio,
                        s.IeltsSectionResultExplain,
                        s.IeltsSectionResultIndex,
                        s.IeltsSectionResultName,
                        s.IeltsSectionResultReadingPassage
                    }).Select(s => new IeltsSectionResultOverviewModel
                    {
                        Id = s.Key.IeltsSectionResultId,
                        Audio = s.Key.IeltsSectionResultAudio,
                        Explain = s.Key.IeltsSectionResultExplain,
                        Index = s.Key.IeltsSectionResultIndex,
                        Name = s.Key.IeltsSectionResultName,
                        ReadingPassage = s.Key.IeltsSectionResultReadingPassage
                    }).ToList()
                }).ToList()
            };
            return result;
        }
        public class Get_IeltsQuestionInSectionResult
        {
            public int IeltsQuestionResultId { get; set; }
            public int IeltsQuestionGroupResultId { get; set; }
            public double? Point { get; set; }
            public int Type { get; set; }
            public string TypeName { get; set; }
            public bool Correct { get; set; }
        }
        public class IeltsQuestionInSectionResultSearch
        {
            public int IeltsSectionResultId { get; set; }
        }
        public async Task<List<IeltsQuestionResultOverviewModel>> GetIeltsQuestionInSectionResult(IeltsQuestionInSectionResultSearch baseSearch)
        {
            //Lấy vị trí câu hỏi trong đề
            int startIndex = 0;
            var ieltsSectionResult = await dbContext.tbl_IeltsSectionResult.SingleOrDefaultAsync(x => x.Id == baseSearch.IeltsSectionResultId);
            if (ieltsSectionResult == null)
                return null;
            var ieltsSkillResult = await dbContext.tbl_IeltsSkillResult.SingleOrDefaultAsync(x => x.Id == ieltsSectionResult.IeltsSkillResultId);
            var previousSkills = await dbContext.tbl_IeltsSkillResult.Where(x => x.IeltsExamResultId == ieltsSkillResult.IeltsExamResultId && x.Index < ieltsSkillResult.Index && x.Enable == true)
                .Select(x => x.QuestionsAmount).ToListAsync();
            if (previousSkills.Any())
                startIndex += previousSkills.Sum(x => x);
            var previousSectionIds = await dbContext.tbl_IeltsSectionResult.Where(x => x.IeltsSkillResultId == ieltsSectionResult.IeltsSkillResultId && x.Index < ieltsSectionResult.Index && x.Enable == true)
                .Select(x => x.Id).ToListAsync();
            if (previousSectionIds.Any())
            {
                foreach (var previousSectionId in previousSectionIds)
                {
                    var questionsAmounts = await dbContext.tbl_IeltsQuestionGroupResult.Where(x => x.IeltsSectionResultId == previousSectionId && x.Enable == true)
                        .Select(x => x.QuestionsAmount).ToListAsync();
                    if (questionsAmounts.Any())
                        startIndex += questionsAmounts.Sum();
                }
            }

            string sql = $"Get_IeltsQuestionInSectionResult @IeltsSectionResultId = {baseSearch.IeltsSectionResultId}";
            var data = await dbContext.SqlQuery<Get_IeltsQuestionInSectionResult>(sql);
            var result = (from i in data
                          select new IeltsQuestionResultOverviewModel
                          {
                              IeltsQuestionGroupResultId = i.IeltsQuestionGroupResultId,
                              IeltsQuestionResultId = i.IeltsQuestionResultId,
                              Index = startIndex += 1,
                              Correct = i.Correct,
                              Point = i.Point,
                              Type = i.Type,
                              TypeName = i.TypeName
                          }).ToList();
            return result;
        }
        public class IeltsSkillResultSearch
        {
            public int IeltsSkillResultId { get; set; }
        }
        public async Task<List<IeltsSectionResultModel>> GetIeltsSkillResultDetail(IeltsSkillResultSearch baseSearch)
        {
            var ieltsSectionResult = await dbContext.tbl_IeltsSectionResult
                .Where(x => x.IeltsSkillResultId == baseSearch.IeltsSkillResultId && x.Enable == true)
                .Select(x => new
                {
                    Id = x.Id,
                    Audio = x.Audio,
                    Explain = x.Explain,
                    Index = x.Index,
                    Name = x.Name,
                    ReadingPassage = x.ReadingPassage
                }).ToListAsync();
            var result = (from i in ieltsSectionResult
                          select new IeltsSectionResultModel
                          {
                              Id = i.Id,
                              Audio = i.Audio,
                              Explain = i.Explain,
                              Index = i.Index,
                              Name = i.Name,
                              ReadingPassage = i.ReadingPassage,
                              IeltsQuestionResultOverviews = Task.Run(() => GetIeltsQuestionInSectionResult(new IeltsQuestionInSectionResultSearch { IeltsSectionResultId = i.Id })).Result
                          }).ToList();
            return result;
        }
    }
}