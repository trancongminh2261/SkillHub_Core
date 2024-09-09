using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSCore.Models
{
    public class tbl_IeltsSkillResult : DomainEntity
    {
        public int IeltsExamResultId { get; set; }
        public int IeltsSkillId { get; set; }
        public int IeltsExamId { get; set; }
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
        /// Thời gian làm bài
        /// </summary>
        public int TimeSpent { get; set; }
        public tbl_IeltsSkillResult() : base() { }
        public tbl_IeltsSkillResult(object model) : base(model) { }
    }
}