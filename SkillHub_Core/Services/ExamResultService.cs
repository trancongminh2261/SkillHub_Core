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
    public class ExamResultService
    {
        public class ExamSubmit
        { 
            [Required(ErrorMessage ="Vui lòng nhập đề thi")]
            public int? ExamId { get; set; }
            [Required(ErrorMessage = "Vui lòng chọn bài học")]
            public int? LessonVideoId { get; set; }
            public List<ExerciseSubmit> Items { get; set; }

        }
        public class ExerciseSubmit
        {
            [Required(ErrorMessage = "Vui lòng chọn đầy đủ thông tin")]
            public int? ExerciseId { get; set; }
            /// <summary>
            /// đáp án được chọn
            /// </summary>
            public List<AnswerSubmit> Answers { get; set; }
        }
        public class AnswerSubmit
        { 
            /// <summary>
            /// Đối với dạng 
            /// </summary>
            public int? AnswerId { get; set; }
            public string AnswerContent { get; set; }
            /// <summary>
            /// Áp dụng cho câu hỏi Sắp xếp câu 
            /// </summary>
            public int Index { get; set; }
        }
        public static async Task<tbl_ExamResult> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_ExamResult.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        /// <summary>
        /// Xữ lý nộp bài
        /// </summary>
        /// <param name="model"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task Submit(ExamSubmit model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var exam = await db.tbl_Exam.SingleOrDefaultAsync(x => x.Id == model.ExamId);
                        if (exam == null)
                            throw new Exception("Không tìm thấy đề");
                        var lessonVideo = await db.tbl_LessonVideo.SingleOrDefaultAsync(x => x.Id == model.LessonVideoId);
                        int sectionId = lessonVideo?.SectionId ?? 0;
                        var section = await db.tbl_Section.SingleOrDefaultAsync(x => x.Id == sectionId);
                        int videoCourseId = section?.VideoCourseId ?? 0;

                        var examResult = new tbl_ExamResult
                        {
                            CreatedBy = user.FullName,
                            CreatedOn = DateTime.Now,
                            VideoCourseId = videoCourseId,
                            Enable = true,
                            ExamId = exam.Id,
                            IsPass = false,
                            ModifiedBy = user.FullName,
                            ModifiedOn = DateTime.Now,
                            PassPoint = exam.PassPoint,
                            StudentId = user.UserInformationId,
                            TotalPoint = 0,
                            MyPoint = 0
                        };
                        double totalPoint = 0;
                        double myPoint = 0;
                        db.tbl_ExamResult.Add(examResult);
                        await db.SaveChangesAsync();
                        var examSections = await db.tbl_ExamSection.Where(x => x.ExamId == exam.Id && x.Enable == true).ToListAsync();
                        if (examSections.Any())
                        {
                            foreach (var examSection in examSections)
                            {
                                var examSectionResult = new tbl_ExamSectionResult
                                {
                                    ExamResultId = examResult.Id,
                                    Explanations = examSection.Explanations,
                                    Index = examSection.Index,
                                    Name = examSection.Name,
                                    CreatedBy = user.FullName,
                                    CreatedOn = DateTime.Now,
                                    Enable = true,
                                    ModifiedBy = user.FullName,
                                    ModifiedOn = DateTime.Now,
                                };
                                db.tbl_ExamSectionResult.Add(examSectionResult);
                                await db.SaveChangesAsync();
                                var exerciseGroups = await db.tbl_ExerciseGroup.Where(x => x.ExamSectionId == examSection.Id && x.Enable == true).ToListAsync();
                                if (exerciseGroups.Any())
                                {
                                    foreach (var exerciseGroup in exerciseGroups)
                                    {
                                        var exerciseGroupResult = new tbl_ExerciseGroupResult
                                        {
                                            Content = exerciseGroup.Content,
                                            Name = exerciseGroup.Name,
                                            ExamResultId = examResult.Id,
                                            ExamSectionResultId = examSectionResult.Id,
                                            ExerciseNumber = exerciseGroup.ExerciseNumber,
                                            Index = exerciseGroup.Index,
                                            Level = exerciseGroup.Level,
                                            LevelName = exerciseGroup.LevelName,
                                            Paragraph = exerciseGroup.Paragraph,
                                            Type = exerciseGroup.Type,
                                            TypeName = exerciseGroup.TypeName,
                                            CreatedBy = user.FullName,
                                            Tags = exerciseGroup.Tags,
                                            CreatedOn = DateTime.Now,
                                            Enable = true,
                                            ModifiedBy = user.FullName,
                                            ModifiedOn = DateTime.Now,
                                        };
                                        db.tbl_ExerciseGroupResult.Add(exerciseGroupResult);
                                        await db.SaveChangesAsync();
                                        var exercises = await db.tbl_Exercise.Where(x => x.ExerciseGroupId == exerciseGroup.Id && x.Enable == true).ToListAsync();
                                        if (exercises.Any())
                                        {
                                            foreach (var exercise in exercises)
                                            {
                                                var exerciseResult = new tbl_ExerciseResult
                                                {
                                                    Content = exercise.Content,
                                                    DescribeAnswer = exercise.DescribeAnswer,
                                                    ExamResultId = examResult.Id,
                                                    ExamSectionResultId = examSectionResult.Id,
                                                    ExerciseGroupResultId = exerciseGroupResult.Id,
                                                    ExerciseId = exercise.Id,
                                                    Index = exercise.Index,
                                                    InputId = exercise.InputId,
                                                    IsResult = false,
                                                    Point = 0,
                                                    CreatedBy = user.FullName,
                                                    CreatedOn = DateTime.Now,
                                                    Enable = true,
                                                    ModifiedBy = user.FullName,
                                                    ModifiedOn = DateTime.Now,
                                                };
                                                db.tbl_ExerciseResult.Add(exerciseResult);
                                                await db.SaveChangesAsync();
                                                bool isResult = false;
                                                var exerciseSubmit = model.Items.Where(x => x.ExerciseId == exercise.Id).FirstOrDefault();
                                                var answers = await db.tbl_Answer.Where(x => x.ExerciseId == exercise.Id && x.Enable == true).ToListAsync();
                                                if (answers.Any() || exerciseGroup.Type == ExerciseType.Write || exerciseGroup.Type == ExerciseType.Speak)
                                                {
                                                    switch (exerciseGroup.Type)
                                                    {
                                                        ///Chấm bài trắc nghiệm
                                                        case ExerciseType.MultipleChoice:
                                                            {
                                                                foreach (var answer in answers)
                                                                {
                                                                    if (exerciseSubmit == null)
                                                                    {
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = answer.AnswerContent,
                                                                            AnswerId = answer.Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            MyResult = false,
                                                                            IsTrue = answer.IsTrue,
                                                                            Type = answer.Type,
                                                                            MyAnswerContent = "",
                                                                            MyAnswerId = 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                    }
                                                                    else
                                                                    {
                                                                        var myAnswer = exerciseSubmit.Answers.Where(x => x.AnswerId == answer.Id).FirstOrDefault();
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = answer.AnswerContent,
                                                                            AnswerId = answer.Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            MyResult = myAnswer == null ? false : true,
                                                                            IsTrue = answer.IsTrue,
                                                                            Type = answer.Type,
                                                                            MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                            MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                    }
                                                                    await db.SaveChangesAsync();
                                                                }
                                                                var checkResult = await db.tbl_AnswerResult.AnyAsync(x => x.ExerciseResultId == exerciseResult.Id && x.Enable == true
                                                                && x.IsTrue != x.MyResult);
                                                                if (!checkResult)
                                                                    isResult = true;
                                                            }
                                                            break;
                                                        ///Chấm bài Chọn từ vào ô trống
                                                        case ExerciseType.DragDrop:
                                                            {
                                                                var correctAnswers = answers.Where(x => x.IsTrue == true).ToList();
                                                                if (exerciseSubmit == null)
                                                                {
                                                                    var answerResult = new tbl_AnswerResult
                                                                    {
                                                                        AnswerContent = correctAnswers.FirstOrDefault().AnswerContent,
                                                                        AnswerId = correctAnswers.FirstOrDefault().Id,
                                                                        ExerciseResultId = exerciseResult.Id,
                                                                        MyResult = false,
                                                                        IsTrue = true,
                                                                        Type = correctAnswers.FirstOrDefault().Type,
                                                                        MyAnswerContent = "",
                                                                        MyAnswerId = 0,
                                                                        CreatedBy = user.FullName,
                                                                        CreatedOn = DateTime.Now,
                                                                        Enable = true,
                                                                        ModifiedBy = user.FullName,
                                                                        ModifiedOn = DateTime.Now,
                                                                    };
                                                                    db.tbl_AnswerResult.Add(answerResult);
                                                                }
                                                                else
                                                                {
                                                                    var myAnswer = exerciseSubmit.Answers.FirstOrDefault();
                                                                    var checkResult = correctAnswers.Where(x => x.Id == myAnswer?.AnswerId).FirstOrDefault();
                                                                    if (checkResult != null)
                                                                    {
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = checkResult.AnswerContent,
                                                                            AnswerId = checkResult.Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            MyResult = true,
                                                                            IsTrue = checkResult.IsTrue,
                                                                            Type = checkResult.Type,
                                                                            MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                            MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                        isResult = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = correctAnswers.FirstOrDefault().AnswerContent,
                                                                            AnswerId = correctAnswers.FirstOrDefault().Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            MyResult = true,
                                                                            IsTrue = correctAnswers.FirstOrDefault().IsTrue,
                                                                            Type = correctAnswers.FirstOrDefault().Type,
                                                                            MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                            MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                    }
                                                                }
                                                                await db.SaveChangesAsync();
                                                            }
                                                            break;
                                                        ///Chấm bài điền vào ô trống
                                                        case ExerciseType.FillInTheBlank:
                                                            {
                                                                var correctAnswers = answers.Where(x => x.IsTrue == true).ToList();
                                                                if (exerciseSubmit == null)
                                                                {
                                                                    var answerResult = new tbl_AnswerResult
                                                                    {
                                                                        AnswerContent = correctAnswers.FirstOrDefault().AnswerContent,
                                                                        AnswerId = correctAnswers.FirstOrDefault().Id,
                                                                        ExerciseResultId = exerciseResult.Id,
                                                                        MyResult = false,
                                                                        IsTrue = true,
                                                                        Type = correctAnswers.FirstOrDefault().Type,
                                                                        MyAnswerContent = "",
                                                                        MyAnswerId = 0,
                                                                        CreatedBy = user.FullName,
                                                                        CreatedOn = DateTime.Now,
                                                                        Enable = true,
                                                                        ModifiedBy = user.FullName,
                                                                        ModifiedOn = DateTime.Now,
                                                                    };
                                                                    db.tbl_AnswerResult.Add(answerResult);
                                                                }
                                                                else
                                                                {
                                                                    var myAnswer = exerciseSubmit.Answers.FirstOrDefault();
                                                                    var checkResult = correctAnswers
                                                                        .Where(x => x.AnswerContent.ToUpper() == (myAnswer?.AnswerContent ?? "").ToUpper()).FirstOrDefault();
                                                                    if (checkResult != null)
                                                                    {
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = checkResult.AnswerContent,
                                                                            AnswerId = checkResult.Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            MyResult = true,
                                                                            IsTrue = checkResult.IsTrue,
                                                                            Type = checkResult.Type,
                                                                            MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                            MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                        isResult = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = correctAnswers.FirstOrDefault().AnswerContent,
                                                                            AnswerId = correctAnswers.FirstOrDefault().Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            MyResult = true,
                                                                            IsTrue = correctAnswers.FirstOrDefault().IsTrue,
                                                                            Type = correctAnswers.FirstOrDefault().Type,
                                                                            MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                            MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                    }
                                                                }
                                                                await db.SaveChangesAsync();
                                                            }
                                                            break; ;
                                                        ///Mindmap
                                                        case ExerciseType.Mindmap:
                                                            {

                                                                var correctAnswers = answers.Where(x => x.IsTrue == true).ToList();
                                                                if (exerciseSubmit == null)
                                                                {
                                                                    var answerResult = new tbl_AnswerResult
                                                                    {
                                                                        AnswerContent = correctAnswers.FirstOrDefault().AnswerContent,
                                                                        AnswerId = correctAnswers.FirstOrDefault().Id,
                                                                        ExerciseResultId = exerciseResult.Id,
                                                                        Type = correctAnswers.FirstOrDefault().Type,
                                                                        MyResult = false,
                                                                        IsTrue = true,
                                                                        MyAnswerContent = "",
                                                                        MyAnswerId = 0,
                                                                        CreatedBy = user.FullName,
                                                                        CreatedOn = DateTime.Now,
                                                                        Enable = true,
                                                                        ModifiedBy = user.FullName,
                                                                        ModifiedOn = DateTime.Now,
                                                                    };
                                                                    db.tbl_AnswerResult.Add(answerResult);
                                                                }
                                                                else
                                                                {
                                                                    var myAnswer = exerciseSubmit.Answers.FirstOrDefault();
                                                                    var checkResult = correctAnswers.Where(x => x.Id == myAnswer?.AnswerId).FirstOrDefault();
                                                                    if (checkResult != null)
                                                                    {
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = checkResult.AnswerContent,
                                                                            AnswerId = checkResult.Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            MyResult = true,
                                                                            IsTrue = checkResult.IsTrue,
                                                                            Type = checkResult.Type,
                                                                            MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                            MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                        isResult = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = correctAnswers.FirstOrDefault().AnswerContent,
                                                                            AnswerId = correctAnswers.FirstOrDefault().Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            Type = correctAnswers.FirstOrDefault().Type,
                                                                            MyResult = true,
                                                                            IsTrue = correctAnswers.FirstOrDefault().IsTrue,
                                                                            MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                            MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                    }
                                                                }
                                                                await db.SaveChangesAsync();
                                                            }
                                                            break;
                                                        ///True/False/Not Given
                                                        case ExerciseType.TrueOrFalse:
                                                            {
                                                                var correctAnswers = answers.Where(x => x.IsTrue == true).ToList();
                                                                if (exerciseSubmit == null)
                                                                {
                                                                    var answerResult = new tbl_AnswerResult
                                                                    {
                                                                        AnswerContent = correctAnswers.FirstOrDefault().AnswerContent,
                                                                        AnswerId = correctAnswers.FirstOrDefault().Id,
                                                                        ExerciseResultId = exerciseResult.Id,
                                                                        MyResult = false,
                                                                        IsTrue = true,
                                                                        Type = correctAnswers.FirstOrDefault().Type,
                                                                        MyAnswerContent = "",
                                                                        MyAnswerId = 0,
                                                                        CreatedBy = user.FullName,
                                                                        CreatedOn = DateTime.Now,
                                                                        Enable = true,
                                                                        ModifiedBy = user.FullName,
                                                                        ModifiedOn = DateTime.Now,
                                                                    };
                                                                    db.tbl_AnswerResult.Add(answerResult);
                                                                }
                                                                else
                                                                {
                                                                    var myAnswer = exerciseSubmit.Answers.FirstOrDefault();
                                                                    var checkResult = correctAnswers.Where(x => x.Id == myAnswer?.AnswerId).FirstOrDefault();
                                                                    if (checkResult != null)
                                                                    {
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = checkResult.AnswerContent,
                                                                            AnswerId = checkResult.Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            MyResult = true,
                                                                            IsTrue = checkResult.IsTrue,
                                                                            Type = checkResult.Type,
                                                                            MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                            MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                        isResult = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = correctAnswers.FirstOrDefault().AnswerContent,
                                                                            AnswerId = correctAnswers.FirstOrDefault().Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            MyResult = true,
                                                                            IsTrue = correctAnswers.FirstOrDefault().IsTrue,
                                                                            Type = correctAnswers.FirstOrDefault().Type,
                                                                            MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                            MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                    }
                                                                }
                                                                await db.SaveChangesAsync();
                                                            }
                                                            break;
                                                        ///Sắp xếp câu  vị trí
                                                        case ExerciseType.Sort:
                                                            {
                                                                var correctAnswers = answers.OrderBy(x=>x.Index).ToList();
                                                                isResult = true;
                                                                foreach (var correctAnswer in correctAnswers)
                                                                {
                                                                    if (exerciseSubmit == null)
                                                                    {
                                                                        isResult = false;
                                                                        var answerResult = new tbl_AnswerResult
                                                                        {
                                                                            AnswerContent = correctAnswer.AnswerContent,
                                                                            AnswerId = correctAnswer.Id,
                                                                            ExerciseResultId = exerciseResult.Id,
                                                                            Index = correctAnswer.Index,
                                                                            Type = correctAnswer.Type,
                                                                            MyResult = false,
                                                                            IsTrue = true,
                                                                            MyAnswerContent = "",
                                                                            MyAnswerId = 0,
                                                                            MyIndex = 0,
                                                                            CreatedBy = user.FullName,
                                                                            CreatedOn = DateTime.Now,
                                                                            Enable = true,
                                                                            ModifiedBy = user.FullName,
                                                                            ModifiedOn = DateTime.Now,
                                                                        };
                                                                        db.tbl_AnswerResult.Add(answerResult);
                                                                    }
                                                                    else
                                                                    {
                                                                        var myAnswer = exerciseSubmit.Answers
                                                                            .Where(x=>x.Index == correctAnswer.Index && x.AnswerId == correctAnswer.Id).FirstOrDefault();
                                                                        if (myAnswer != null)
                                                                        {
                                                                            var answerResult = new tbl_AnswerResult
                                                                            {
                                                                                AnswerContent = correctAnswer.AnswerContent,
                                                                                AnswerId = correctAnswer.Id,
                                                                                ExerciseResultId = exerciseResult.Id,
                                                                                MyResult = true,
                                                                                Index = correctAnswer.Index,
                                                                                IsTrue = correctAnswer.IsTrue,
                                                                                Type = correctAnswer.Type,
                                                                                MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                                MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                                CreatedBy = user.FullName,
                                                                                MyIndex = myAnswer.Index,
                                                                                CreatedOn = DateTime.Now,
                                                                                Enable = true,
                                                                                ModifiedBy = user.FullName,
                                                                                ModifiedOn = DateTime.Now,
                                                                            };
                                                                            db.tbl_AnswerResult.Add(answerResult);
                                                                            isResult = true;
                                                                        }
                                                                        else
                                                                        {
                                                                            myAnswer = exerciseSubmit.Answers
                                                                            .Where(x => x.Index == correctAnswer.Index).FirstOrDefault();
                                                                            isResult = false;
                                                                            var answerResult = new tbl_AnswerResult
                                                                            {
                                                                                AnswerContent = correctAnswer.AnswerContent,
                                                                                AnswerId = correctAnswer.Id,
                                                                                ExerciseResultId = exerciseResult.Id,
                                                                                Index = correctAnswer.Index,
                                                                                MyResult = true,
                                                                                IsTrue = correctAnswer.IsTrue,
                                                                                Type = correctAnswer.Type,
                                                                                MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                                MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                                MyIndex = myAnswer.Index,
                                                                                CreatedBy = user.FullName,
                                                                                CreatedOn = DateTime.Now,
                                                                                Enable = true,
                                                                                ModifiedBy = user.FullName,
                                                                                ModifiedOn = DateTime.Now,
                                                                            };
                                                                            db.tbl_AnswerResult.Add(answerResult);
                                                                        }
                                                                    }
                                                                    await db.SaveChangesAsync();
                                                                }
                                                            }
                                                            break;
                                                        ///Viết
                                                        case ExerciseType.Write:
                                                            {
                                                                if (exerciseSubmit == null)
                                                                {
                                                                    var answerResult = new tbl_AnswerResult
                                                                    {
                                                                        AnswerContent = "",
                                                                        AnswerId = 0,
                                                                        ExerciseResultId = exerciseResult.Id,
                                                                        MyResult = false,
                                                                        IsTrue = true,
                                                                        Type = AnswerType.Text,
                                                                        MyAnswerContent = "",
                                                                        MyAnswerId = 0,
                                                                        CreatedBy = user.FullName,
                                                                        CreatedOn = DateTime.Now,
                                                                        Enable = true,
                                                                        ModifiedBy = user.FullName,
                                                                        ModifiedOn = DateTime.Now,
                                                                    };
                                                                    db.tbl_AnswerResult.Add(answerResult);
                                                                }
                                                                else
                                                                {
                                                                    var myAnswer = exerciseSubmit.Answers.FirstOrDefault();
                                                                    var answerResult = new tbl_AnswerResult
                                                                    {
                                                                        AnswerContent = "",
                                                                        AnswerId = 0,
                                                                        ExerciseResultId = exerciseResult.Id,
                                                                        MyResult = true,
                                                                        IsTrue = true,
                                                                        Type = AnswerType.Text,
                                                                        MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                        MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                        CreatedBy = user.FullName,
                                                                        CreatedOn = DateTime.Now,
                                                                        Enable = true,
                                                                        ModifiedBy = user.FullName,
                                                                        ModifiedOn = DateTime.Now,
                                                                    };
                                                                    db.tbl_AnswerResult.Add(answerResult);
                                                                }
                                                                await db.SaveChangesAsync();
                                                            }
                                                            break;
                                                        ///Nói
                                                        case ExerciseType.Speak:
                                                            {
                                                                if (exerciseSubmit == null)
                                                                {
                                                                    var answerResult = new tbl_AnswerResult
                                                                    {
                                                                        AnswerContent = "",
                                                                        AnswerId = 0,
                                                                        ExerciseResultId = exerciseResult.Id,
                                                                        MyResult = false,
                                                                        IsTrue = true,
                                                                        Type = AnswerType.Audio,
                                                                        MyAnswerContent = "",
                                                                        MyAnswerId = 0,
                                                                        CreatedBy = user.FullName,
                                                                        CreatedOn = DateTime.Now,
                                                                        Enable = true,
                                                                        ModifiedBy = user.FullName,
                                                                        ModifiedOn = DateTime.Now,
                                                                    };
                                                                    db.tbl_AnswerResult.Add(answerResult);
                                                                }
                                                                else
                                                                {
                                                                    var myAnswer = exerciseSubmit.Answers.FirstOrDefault();
                                                                    var answerResult = new tbl_AnswerResult
                                                                    {
                                                                        AnswerContent = "",
                                                                        AnswerId = 0,
                                                                        ExerciseResultId = exerciseResult.Id,
                                                                        MyResult = true,
                                                                        IsTrue = true,
                                                                        Type = AnswerType.Audio,
                                                                        MyAnswerContent = myAnswer?.AnswerContent ?? "",
                                                                        MyAnswerId = myAnswer?.AnswerId ?? 0,
                                                                        CreatedBy = user.FullName,
                                                                        CreatedOn = DateTime.Now,
                                                                        Enable = true,
                                                                        ModifiedBy = user.FullName,
                                                                        ModifiedOn = DateTime.Now,
                                                                    };
                                                                    db.tbl_AnswerResult.Add(answerResult);
                                                                }
                                                                await db.SaveChangesAsync();
                                                            }
                                                            break;
                                                    }
                                                }
                                                exerciseResult.IsResult = isResult;
                                                exerciseResult.Point = exercise.Point ?? 0;
                                                totalPoint += exercise.Point ?? 0;
                                                if (isResult)
                                                {
                                                    myPoint += exercise.Point ?? 0;
                                                }
                                                await db.SaveChangesAsync();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        examResult.TotalPoint = totalPoint;
                        examResult.MyPoint = myPoint;
                        if (examResult.MyPoint >= exam.PassPoint)
                        {
                            examResult.IsPass = true;
                        }
                        await db.SaveChangesAsync();
                        tran.Commit();
                        ///Hoàn thành bài học
                        if (examResult.IsPass == true)
                        {
                            try
                            {
                                await LessonVideoService.Completed(model.LessonVideoId ?? 0, user, examResult.Id, examResult.TotalPoint);
                            }
                            catch { }
                        }
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
        }
        public static async Task<AppDomainResult> GetAll(ExamResultSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) return new AppDomainResult { TotalRow = 0, Data = null };
                string sql = $"Get_ExamResult @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ExamId = {baseSearch.ExamId}," +
                    $"@VideoCourseId = {baseSearch.VideoCourseId}," +
                    $"@StudentId ={baseSearch.StudentId}";
                var data = await db.SqlQuery<Get_ExamResult>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_ExamResult(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<AppDomainResult> GetDetail(int examResultId)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_ExamResultDetail @PageIndex = 1," +
                    $"@PageSize = 9999," +
                    $"@ExamResultId = {examResultId}";
                var data = await db.SqlQuery<Get_ExamResultDetail>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                int indexInExam = 0;
                var result = data.GroupBy(es => new { es.ExamSectionResultId, es.ExamSectionName, es.Explanations, es.ExamSectionIndex }).OrderBy(x => x.Key.ExamSectionIndex)
                    .Select(es => new ExamSectionResultModel
                    {
                        Id = es.Key.ExamSectionResultId ?? 0,
                        Name = es.Key.ExamSectionName,
                        Explanations = es.Key.Explanations,
                        Index = es.Key.ExamSectionIndex,
                        ExerciseResultGroups = es.GroupBy(eg => new
                        {
                            eg.Id,
                            eg.Content,
                            eg.Paragraph,
                            eg.ExerciseNumber,
                            eg.Level,
                            eg.LevelName,
                            eg.Type,
                            eg.TypeName,
                            eg.ExerciseGroupIndex,
                            eg.Name,
                        }).OrderBy(x => x.Key.ExerciseGroupIndex).Select(eg => new ExerciseGroupResultModel
                        {
                            Id = eg.Key.Id,
                            Content = eg.Key.Content,
                            Name = eg.Key.Name,
                            Paragraph = eg.Key.Paragraph,
                            ExerciseNumber = eg.Key.ExerciseNumber,
                            Level = eg.Key.Level,
                            LevelName = eg.Key.LevelName,
                            Type = eg.Key.Type,
                            TypeName = eg.Key.TypeName,
                            IsExist = true,
                            Index = eg.Key.ExerciseGroupIndex,
                            ExerciseResults = eg.GroupBy(e => new
                            {
                                e.ExerciseResultId,
                                e.ExerciseContent,
                                e.InputId,
                                e.DescribeAnswer,
                                e.ExerciseIndex,
                                e.Point,
                                e.IsResult,
                            }).OrderBy(x => x.Key.ExerciseIndex).Select(e => new ExerciseResultModel
                            {
                                Id = e.Key.ExerciseResultId ?? 0,
                                Content = e.Key.ExerciseContent,
                                InputId = e.Key.InputId,
                                DescribeAnswer = e.Key.DescribeAnswer,
                                IndexInExam = indexInExam += 1,
                                Index = e.Key.ExerciseIndex,
                                Point = e.Key.Point,
                                IsResult = e.Key.IsResult,
                                Answers = Task.Run(()=> GetAnswers(e.Key.ExerciseResultId ?? 0)).Result,
                                AnswerResults = e.GroupBy(a => new
                                {
                                    a.AnswerResultId,
                                    a.AnswerContent,
                                    a.IsTrue,
                                    a.MyAnswerContent,
                                    a.MyAnswerId,
                                    a.MyResult,
                                    a.AnswerType,
                                    a.AnswerComment,
                                }).Select(a => new AnswerResultModel
                                {
                                    Id = a.Key.AnswerResultId ?? 0,
                                    AnswerContent = a.Key.AnswerContent,
                                    IsTrue = a.Key.IsTrue,
                                    MyAnswerContent = a.Key.MyAnswerContent,
                                    MyAnswerId = a.Key.MyAnswerId,
                                    MyResult = a.Key.MyResult,
                                    Type = a.Key.AnswerType ?? 0,
                                    Comments = JsonConvert.DeserializeObject<List<CommentModel>>(a.Key.AnswerComment ?? ""),
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList();
                return new AppDomainResult { Data = result };
            }
        }
        public static async Task<List<tbl_Answer>> GetAnswers(int exerciseResultId)
        {
            using (var db = new lmsDbContext())
            {
                var exerciseResult = await db.tbl_ExerciseResult.SingleOrDefaultAsync(x => x.Id == exerciseResultId);
                int exerciseId = exerciseResult?.ExerciseId ?? 0;
                var data = await db.tbl_Answer.Where(x => x.ExerciseId == exerciseId && x.Enable == true).ToListAsync();
                return data;
            }
        }
        public class AddTeacherModel
        { 
            public int TeacherId { get; set; }
            public List<int> ExamResultIds { get; set; }
        }
        /// <summary>
        /// Chọn giáo viên chấm bài
        /// </summary>
        /// <returns></returns>
        public static async Task AddTeacher(AddTeacherModel itemModel,tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var teacher = await db.tbl_UserInformation
                                .Where(x => x.UserInformationId == itemModel.TeacherId && x.Enable == true && x.RoleId == ((int)RoleEnum.teacher))
                                .FirstOrDefaultAsync();
                        if (teacher == null)
                            throw new Exception("Không tìm thấy giáo viên");
                        if (!itemModel.ExamResultIds.Any())
                            throw new Exception("Vui lòng chọn kết quả làm bài");
                        foreach (var item in itemModel.ExamResultIds)
                        {
                            var examResult = await db.tbl_ExamResult.SingleOrDefaultAsync(x => x.Id == item);
                            if (examResult == null)
                                throw new Exception("Không tìm thấy kết quả làm bài");
                            examResult.TeacherId = teacher.UserInformationId;
                            if (examResult.Status == 1)
                            {
                                examResult.Status = 2;
                                examResult.StatusName = "Đang chấm bài";
                            }
                            examResult.ModifiedBy = user.FullName;
                            examResult.ModifiedOn = DateTime.Now;
                            await db.SaveChangesAsync();
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
        }
        public class MarkModel
        { 
            public int ExamResultId { get; set; }
            /// <summary>
            /// 1 - Lưu nháp 
            /// 2 - Hoàn thành
            /// </summary>
            public int Type { get; set; }
            public List<ExerciseResultMark> ExerciseResultMarks { get; set; }
        }
        public class ExerciseResultMark
        {
            public int ExerciseResultId { get; set; }
            public double Point { get; set; }
            public int AnswerResultId { get; set; }
            public List<CommentModel> Comments { get; set; }
        }
        public class CommentModel
        {
            public string Note { get; set; }
            public string NoteId { get; set; }
            public string AudioUrl { get; set; }
        }
        public static async Task Mark(MarkModel itemModel,tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var examResult = await db.tbl_ExamResult.SingleOrDefaultAsync(x => x.Id == itemModel.ExamResultId);
                        if (examResult == null)
                            throw new Exception("Không tìm thấy bài làm");
                        if (!itemModel.ExerciseResultMarks.Any())
                            throw new Exception("Không tìm thấy dữ liệu chấm bài");
                        foreach (var item in itemModel.ExerciseResultMarks)
                        {
                            var exerciseResult = await db.tbl_ExerciseResult.SingleOrDefaultAsync(x => x.Id == item.ExerciseResultId);
                            if (exerciseResult == null)
                                throw new Exception("Không tìm thấy câu hỏi");
                            var group = await db.tbl_ExerciseGroupResult.SingleOrDefaultAsync(x => x.Id == exerciseResult.ExerciseGroupResultId);
                            if (group == null)
                                throw new Exception("Không tìm thấy câu hỏi");
                            if (group.Type != ExerciseType.Speak && group.Type != ExerciseType.Write)
                                throw new Exception("Loại câu hỏi không phù hợp");
                            var exercise = await db.tbl_Exercise.SingleOrDefaultAsync(x => x.Id == exerciseResult.ExerciseId);
                            if (exercise != null)
                            {
                                if (item.Point > exercise.Point)
                                    throw new Exception("Điểm không phù hợp");
                            }
                            exerciseResult.Point = item.Point;
                            exerciseResult.ModifiedOn = DateTime.Now;
                            exerciseResult.ModifiedBy = user.FullName;
                            var answerResult = await db.tbl_AnswerResult.SingleOrDefaultAsync(x => x.Id == item.AnswerResultId);
                            if (answerResult == null)
                                throw new Exception("Không tìm thấy đáp án");
                            answerResult.Comment = JsonConvert.SerializeObject(item.Comments);
                            answerResult.ModifiedOn = DateTime.Now;
                            answerResult.ModifiedBy = user.FullName;
                        }
                        await db.SaveChangesAsync();
                        double myPoint = 0;
                        var exerciseResults = await db.tbl_ExerciseResult.Where(x => x.ExamResultId == examResult.Id && x.Enable == true).Select(x => x.Point ?? 0).ToListAsync();
                        if (exerciseResults.Any())
                        {
                            myPoint = exerciseResults.Sum();
                            examResult.MyPoint = myPoint;
                            if (itemModel.Type == 2)
                            {
                                examResult.Status = 3;
                                examResult.StatusName = "Đã chấm xong";
                            }
                        }
                        await db.SaveChangesAsync();
                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
                
            }
        }
    }
}