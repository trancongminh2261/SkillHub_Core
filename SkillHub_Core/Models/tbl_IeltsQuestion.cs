using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LMSCore.Models
{
    public class tbl_IeltsQuestion : DomainEntity
    {
        public int IeltsExamId { get; set; }
        public int IeltsSkillId { get; set; }
        public int IeltsSectionId { get; set; }
        public int IeltsQuestionGroupId { get; set; }
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
        public tbl_IeltsQuestion() : base() { }
        public tbl_IeltsQuestion(object model) : base(model) { }
    }
    public class IeltsQuestionModel
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
        public double? Point { get; set; }
        /// <summary>
        /// Số đáp án đúng
        /// </summary>
        public int CorrectAmount { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        public List<IeltsAnswerModel> IeltsAnswers { get; set; }
        public List<DoingTestDetailModel> DoingTestDetails { get; set; }
    }
}