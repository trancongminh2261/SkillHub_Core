namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    using static LMSCore.Services.ExamResultService;

    public class tbl_ExamResult : DomainEntity
    {
        public int? ExamId { get; set; }
        public int? StudentId { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string Avatar { get; set; }
        /// <summary>
        /// Số điểm đạt
        /// </summary>
        public double PassPoint { get; set; }
        /// <summary>
        /// Đạt
        /// </summary>
        public bool? IsPass { get; set; }
        /// <summary>
        /// Tổng số điểm
        /// </summary>
        public double TotalPoint { get; set; }
        /// <summary>
        /// Số điểm
        /// </summary>
        public double MyPoint { get; set; }
        public int? VideoCourseId { get; set; }
        /// <summary>
        /// 1 - Đã nộp bài
        /// 2 - Đang chấm bài
        /// 3 - Đã chấm xong
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int TeacherId { get; set; }
        public DateTime? StartTime { get; set; }
        public tbl_ExamResult() : base() { }
        public tbl_ExamResult(object model) : base(model) { }
    }
    public class Get_ExamResult : DomainEntity
    {
        public int? ExamId { get; set; }
        public int? StudentId { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        /// <summary>
        /// Số điểm đạt
        /// </summary>
        public double PassPoint { get; set; }
        /// <summary>
        /// Đạt
        /// </summary>
        public bool? IsPass { get; set; }
        /// <summary>
        /// Số điểm
        /// </summary>
        public double TotalPoint { get; set; }
        /// <summary>
        /// Số điểm
        /// </summary>
        public double MyPoint { get; set; }
        public int? VideoCourseId { get; set; }
        /// <summary>
        /// 1 - Đã nộp bài
        /// 2 - Đang chấm bài
        /// 3 - Đã chấm xong
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int TeacherId { get; set; }
        public int TotalRow { get; set; }
    }
    public class Get_ExamResultDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Paragraph { get; set; }
        public int? ExerciseNumber { get; set; }
        public ExerciseLevel? Level { get; set; }
        public string LevelName { get; set; }
        public ExerciseType? Type { get; set; }
        public string TypeName { get; set; }
        public int? ExerciseResultId { get; set; }
        public string ExerciseContent { get; set; }
        public string InputId { get; set; }
        public string DescribeAnswer { get; set; }
        public bool? IsResult { get; set; }
        public int? AnswerResultId { get; set; }
        public string AnswerContent { get; set; }
        public string AnswerComment { get; set; }
        public AnswerType? AnswerType { get; set; }
        public bool? IsTrue { get; set; }
        public int? ExamSectionResultId { get; set; }
        public string ExamSectionName { get; set; }
        public string Explanations { get; set; }
        public int? ExamSectionIndex { get; set; }
        public int? ExerciseGroupIndex { get; set; }
        public double? Point { get; set; }
        public int? ExerciseIndex { get; set; }
        public int? MyAnswerId { get; set; }
        public string MyAnswerContent { get; set; }
        public bool? MyResult { get; set; }
        public int TotalRow { get; set; }
    }
    public class ExamSectionResultModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Nội dung chú thích đáp án đúng của đoạn văn
        /// </summary>
        public string Explanations { get; set; }
        public int? Index { get; set; }
        public List<ExerciseGroupResultModel> ExerciseResultGroups { get; set; }
    }
    public class ExerciseGroupResultModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Paragraph { get; set; }
        public int? ExerciseNumber { get; set; }
        public ExerciseLevel? Level { get; set; }
        public string LevelName { get; set; }
        public ExerciseType? Type { get; set; }
        public string TypeName { get; set; }
        public int? Index { get; set; }
        /// <summary>
        /// Đã tồn tại trong đề
        /// </summary>
        public bool IsExist { get; set; }
        /// <summary>
        /// Vị trí câu bắt đầu trong đề
        /// </summary>
        public int? SIndexInExam
        {
            get
            { return ExerciseResults.Count == 0 ? 0 : ExerciseResults[0].IndexInExam; }
        }
        /// <summary>
        /// Vị trí câu kết thúc trong đề
        /// </summary>
        public int? EIndexInExam
        {
            get
            { return SIndexInExam == 0 ? 0 : SIndexInExam + (ExerciseResults.Count - 1); }
        }
        public List<ExerciseResultModel> ExerciseResults { get; set; }
    }
    public class ExerciseResultModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string InputId { get; set; }
        public string DescribeAnswer { get; set; }
        /// <summary>
        /// Vị trí câu trong đề
        /// </summary>
        public int? IndexInExam { get; set; }
        public int? Index { get; set; }
        public double? Point { get; set; }
        public bool? IsResult { get; set; }
        public List<tbl_Answer> Answers { get; set; }
        public List<AnswerResultModel> AnswerResults { get; set; }
    }
    public class AnswerResultModel
    {
        public int Id { get; set; }
        public string AnswerContent { get; set; }
        public bool? IsTrue { get; set; }
        public List<CommentModel> Comments { get; set; }
        public int? MyAnswerId { get; set; }
        public string MyAnswerContent { get; set; }
        public bool? MyResult { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public AnswerType Type { get; set; }
    }
}