using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LMSCore.Models
{
    public class tbl_IeltsQuestionResult : DomainEntity
    {
        public int IeltsQuestionGroupResultId { get; set; }
        public int IeltsExamId { get; set; }
        public int IeltsSkillId { get; set; }
        public int IeltsSectionId { get; set; }
        public int IeltsQuestionGroupId { get; set; }
        public int IeltsQuestionId { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// đáp án mẫu
        /// </summary>
        public string SampleAnswer { get; set; }
        public string InputId { get; set; }
        /// <summary>
        /// Giải thích bài làm (hiện khi học viên hoàn thành bài thi)
        /// </summary>
        public string Explain { get; set; }
        /// <summary>
        /// Vị trí của câu trong nhóm
        /// </summary>
        public int? Index { get; set; }
        public double? Point { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        /// <summary>
        /// Đánh giá của giáo viên
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// true - Làm đúng
        /// </summary>
        public bool? Correct { get; set; }
        /// <summary>
        /// Đáp án của đạng ghép đôi
        /// </summary>
        public int AnswerOfMindmap { get; set; }
        public tbl_IeltsQuestionResult() : base() { }
        public tbl_IeltsQuestionResult(object model) : base(model) { }
    }
    public class IeltsQuestionResultModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// đáp án mẫu
        /// </summary>
        public string SampleAnswer { get; set; }
        public string InputId { get; set; }
        /// <summary>
        /// Giải thích bài làm (hiện khi học viên hoàn thành bài thi)
        /// </summary>
        public string Explain { get; set; }
        /// <summary>
        /// Vị trí của câu trong nhóm
        /// </summary>
        public int? Index { get; set; }
        public double? MaxPoint { get; set; }
        public double? Point { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        /// <summary>
        /// true - Làm đúng
        /// </summary>
        public bool? Correct { get; set; }
        public int AnswerOfMindmap { get; set; }
        public string Note { get; set; }
        public List<IeltsAnswerResultModel> IeltsAnswerResults { get; set; }
    }
}