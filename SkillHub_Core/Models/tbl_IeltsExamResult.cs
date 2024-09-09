using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LMSCore.Models
{
    public class tbl_IeltsExamResult : DomainEntity
    {
        public int StudentId { get; set; }
        public int DoingTestId { get; set; }
        public int IeltsExamId { get; set; }
        public int ValueId { get; set; }
        /// <summary>
        /// 1 - Làm bài thử
        /// 2 - Làm bài hẹn test 
        /// 3 - Bài tập về nhà
        /// 4 - Bộ đề
        /// 5 - Lộ trình luyện tập
        /// 6 - Làm bài thi
        /// </summary>
        public int Type { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Code { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public int QuestionsAmount { get; set; }
        /// <summary>
        /// Số lượng hỏi trắc nghiệm
        /// </summary>
        public int QuestionMultipleChoiceAmount { get; set; }
        /// <summary>
        /// Số lượng câu hỏi tự luận
        /// </summary>
        public int QuestionEssayAmount { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ
        /// </summary>
        public int QuestionsEasy { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình
        /// </summary>
        public int QuestionsNormal { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó
        /// </summary>
        public int QuestionsDifficult { get; set; }
        /// <summary>
        /// Số lượng câu hỏi đúng
        /// </summary>
        public int QuestionsMultipleChoiceCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ đúng
        /// </summary>
        public int QuestionsEasyCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình đúng
        /// </summary>
        public int QuestionsNormalCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó đúng
        /// </summary>
        public int QuestionsDifficultCorrect { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// Tổng thời gian của đề
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        public int TimeSpent { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// true - hiện
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Ghi chú của giáo viên
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Điểm của đề
        /// </summary>
        public double Point { get; set; }
        /// <summary>
        /// Điểm của học viên
        /// </summary>
        public double MyPoint { get; set; }
        /// <summary>
        /// Điểm trung bình của các kỹ năng
        /// </summary>
        public double AveragePoint { get; set; }
        public DateTime StartTime { get; set; }
        public int TeacherId { get; set; }
        /// <summary>
        /// 1 - Đang chấm bài
        /// 2 - Đã chấm xong
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// True - Đậu
        /// False - Rớt
        /// </summary>
        public bool? IsPassed { get; set; }
        [NotMapped]
        public string StudentName { get; set; }
        [NotMapped]
        public string StudentCode { get; set; }
        [NotMapped]
        public string TeacherName { get; set; }
        public tbl_IeltsExamResult() : base() { }
        public tbl_IeltsExamResult(object model) : base(model) { }
    }
    public class Get_IeltsExamResult : DomainEntity
    {
        public int StudentId { get; set; }
        public int DoingTestId { get; set; }
        public int IeltsExamId { get; set; }
        public int ValueId { get; set; }
        /// <summary>
        /// 1 - Làm bài thử
        /// 2 - Làm bài hẹn test 
        /// 3 - Bài tập về nhà
        /// 4 - Bộ đề
        /// </summary>
        public int Type { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Code { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public int QuestionsAmount { get; set; }
        /// <summary>
        /// Số lượng hỏi trắc nghiệm
        /// </summary>
        public int QuestionMultipleChoiceAmount { get; set; }
        /// <summary>
        /// Số lượng câu hỏi tự luận
        /// </summary>
        public int QuestionEssayAmount { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ
        /// </summary>
        public int QuestionsEasy { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình
        /// </summary>
        public int QuestionsNormal { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó
        /// </summary>
        public int QuestionsDifficult { get; set; }
        /// <summary>
        /// Số lượng câu hỏi đúng
        /// </summary>
        public int QuestionsMultipleChoiceCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ đúng
        /// </summary>
        public int QuestionsEasyCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình đúng
        /// </summary>
        public int QuestionsNormalCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó đúng
        /// </summary>
        public int QuestionsDifficultCorrect { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// Tổng thời gian của đề
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        public int TimeSpent { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// true - hiện
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Ghi chú của giáo viên
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Điểm của đề
        /// </summary>
        public double Point { get; set; }
        /// <summary>
        /// Điểm của học viên
        /// </summary>
        public double MyPoint { get; set; }
        /// <summary>
        /// Điểm trung bình của các kỹ năng
        /// </summary>
        public double AveragePoint { get; set; }
        public DateTime StartTime { get; set; }
        public int TeacherId { get; set; }
        /// <summary>
        /// 1 - Đang chấm bài
        /// 2 - Đã chấm xong
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string TeacherName { get; set; }
        /// <summary>
        /// True - Đậu
        /// False - Rớt
        /// </summary>
        public bool? IsPassed { get; set; }
        public int TotalRow { get; set; }
    }
    public class IeltsResult
    {
        /// <summary>
        /// Số lượng câu hỏi trắc nghiệm đúng
        /// </summary>
        public int QuestionsMultipleChoiceCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ đúng
        /// </summary>
        public int QuestionsEasyCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình đúng
        /// </summary>
        public int QuestionsNormalCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó đúng
        /// </summary>
        public int QuestionsDifficultCorrect { get; set; }
        /// <summary>
        /// Điểm của học viên
        /// </summary>
        public double MyPoint { get; set; }
        public IeltsResult()
        {
            QuestionsMultipleChoiceCorrect = 0;
            QuestionsEasyCorrect = 0;
            QuestionsNormalCorrect = 0;
            QuestionsDifficultCorrect = 0;
            MyPoint = 0;
        }
    }
    public class Get_IeltsExamResultOverview
    {
        public int Id { get; set; }
		public string Name { get; set; }
		public int Time { get; set; }
		public string Audio { get; set; }
		public int QuestionsAmount { get; set; }
        /// <summary>
        /// Số lượng hỏi trắc nghiệm
        /// </summary>
        public int QuestionMultipleChoiceAmount { get; set; }
        /// <summary>
        /// Số lượng câu hỏi tự luận
        /// </summary>
        public int QuestionEssayAmount { get; set; }
        public int QuestionsEasy { get; set; }
		public int QuestionsNormal { get; set; }
		public int QuestionsDifficult { get; set; }
		public int QuestionsMultipleChoiceCorrect { get; set; }
		public int QuestionsEasyCorrect { get; set; }
		public int QuestionsNormalCorrect { get; set; }
		public int QuestionsDifficultCorrect { get; set; }
        /// <summary>
        /// Số câu hỏi tự luận đã chấm
        /// </summary>
        public int QuestionEssayGraded { get; set; }
        public int Index { get; set; }
		public double Point { get; set; }
		public double MyPoint { get; set; }
		public string Note { get; set; }
		public int IeltsSectionResultId { get; set; }
		public string IeltsSectionResultName { get; set; }
		public string IeltsSectionResultExplain { get; set; }
		public string IeltsSectionResultReadingPassage { get; set; }
		public string IeltsSectionResultAudio { get; set; }
		public int IeltsSectionResultIndex { get; set; }
    }
    public class IeltsExamResultOverviewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int ValueId { get; set; }
        /// <summary>
        /// 1 - Làm bài thử
        /// 2 - Làm bài hẹn test 
        /// 3 - Bài tập về nhà
        /// 4 - Bộ đề
        /// </summary>
        public int Type { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Code { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public int QuestionsAmount { get; set; }
        /// <summary>
        /// Số lượng hỏi trắc nghiệm
        /// </summary>
        public int QuestionMultipleChoiceAmount { get; set; }
        /// <summary>
        /// Số lượng câu hỏi tự luận
        /// </summary>
        public int QuestionEssayAmount { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ
        /// </summary>
        public int QuestionsEasy { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình
        /// </summary>
        public int QuestionsNormal { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó
        /// </summary>
        public int QuestionsDifficult { get; set; }
        /// <summary>
        /// Số lượng câu hỏi trắc nghiệm đúng
        /// </summary>
        public int QuestionsMultipleChoiceCorrect { get; set; }
        public double MultipleChoiceCorrectPercent { get
            {
                if (QuestionMultipleChoiceAmount == 0)
                    return 0;
                double percent = (((double)QuestionsMultipleChoiceCorrect/(double)QuestionMultipleChoiceAmount) * 100 );
                return Math.Round(percent, 2);
            }
        }
        /// <summary>
        /// Số lương câu hỏi dễ đúng
        /// </summary>
        public int QuestionsEasyCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình đúng
        /// </summary>
        public int QuestionsNormalCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó đúng
        /// </summary>
        public int QuestionsDifficultCorrect { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// Tổng thời gian của đề
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        public int TimeSpent { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Ghi chú của giáo viên
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Điểm của đề
        /// </summary>
        public double Point { get; set; }
        /// <summary>
        /// Điểm của học viên
        /// </summary>
        public double MyPoint { get; set; }
        /// <summary>
        /// Điểm trung bình của các kỹ năng
        /// </summary>
        public double AveragePoint { get; set; }
        public DateTime StartTime { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        /// <summary>
        /// 1 - Đang chấm bài
        /// 2 - Đã chấm xong
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// Số câu hỏi tự luận đã chấm
        /// </summary>
        public int QuestionEssayGraded { get; set; }
        public List<IeltsSkillResultOverviewModel> IeltsSkillResultOverviews { get; set; }
    }
    public class IeltsSkillResultOverviewModel
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        /// <summary>
        /// Tổng thời gian làm của kỹ năng phải bằng thời gian làm đề
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public int QuestionsAmount { get; set; }
        /// <summary>
        /// Số lượng hỏi trắc nghiệm
        /// </summary>
        public int QuestionMultipleChoiceAmount { get; set; }
        /// <summary>
        /// Số lượng câu hỏi tự luận
        /// </summary>
        public int QuestionEssayAmount { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ
        /// </summary>
        public int QuestionsEasy { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình
        /// </summary>
        public int QuestionsNormal { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó
        /// </summary>
        public int QuestionsDifficult { get; set; }
        /// <summary>
        /// Số lượng câu hỏi đúng
        /// </summary>
        public int QuestionsMultipleChoiceCorrect { get; set; }
        public double MultipleChoiceCorrectPercent
        {
            get
            {
                if (QuestionMultipleChoiceAmount == 0)
                    return 0;
                double percent = (((double)QuestionsMultipleChoiceCorrect / (double)QuestionMultipleChoiceAmount) * 100);
                return Math.Round(percent, 2);
            }
        }
        /// <summary>
        /// Số lương câu hỏi dễ đúng
        /// </summary>
        public int QuestionsEasyCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình đúng
        /// </summary>
        public int QuestionsNormalCorrect { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó đúng
        /// </summary>
        public int QuestionsDifficultCorrect { get; set; }
        /// <summary>
        /// Vị trí
        /// </summary>
        public int Index { get; set; }
        public double Point { get; set; }
        /// <summary>
        /// Ghi chú của giáo viên
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Điểm của học viên
        /// </summary>
        public double MyPoint { get; set; }
        /// <summary>
        /// Câu tự luận đã chấm
        /// </summary>
        public int QuestionEssayGraded { get; set; }
        public List<IeltsSectionResultOverviewModel> IeltsSectionResultOverviews { get; set; }
    }
    public class IeltsSectionResultOverviewModel
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        /// <summary>
        /// Giải thích bài làm (hiện khi học viên hoàn thành bài thi)
        /// </summary>
        public string Explain { get; set; }
        /// <summary>
        /// Đoạn văn
        /// </summary>
        public string ReadingPassage { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        public int Index { get; set; }
    }
    public class IeltsSectionResultModel
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        /// <summary>
        /// Giải thích bài làm (hiện khi học viên hoàn thành bài thi)
        /// </summary>
        public string Explain { get; set; }
        /// <summary>
        /// Đoạn văn
        /// </summary>
        public string ReadingPassage { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        public int Index { get; set; }
        public List<IeltsQuestionResultOverviewModel> IeltsQuestionResultOverviews { get; set; }
        /// <summary>
        /// Có tồn tại câu tự luận trong section
        /// </summary>
        public bool HasEssay
        {
            get
            {
                bool result = false;
                if (IeltsQuestionResultOverviews.Count > 0)
                {
                    if (IeltsQuestionResultOverviews.Find(x => x.Type == 7 || x.Type == 8) != null)
                        result = true;
                }
                return result;
            }
        }
        public IeltsSectionResultModel()
        {
            IeltsQuestionResultOverviews = new List<IeltsQuestionResultOverviewModel>();
        }
    }

    public class IeltsQuestionResultOverviewModel
    {
        public int IeltsQuestionGroupResultId { get; set; }
        public int IeltsQuestionResultId { get; set; }
        public double? Point { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public bool Correct { get; set; }
        public int Index { get; set; }
    }
}