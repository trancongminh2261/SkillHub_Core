using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSCore.Models
{
    public class tbl_IeltsQuestionGroupResult : DomainEntity
    {
        public int IeltsSectionResultId { get; set; }
        public int IeltsExamId { get; set; }
        public int IeltsSkillId { get; set; }
        public int IeltsSectionId { get; set; }
        public int IeltsQuestionGroupId { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public int QuestionsAmount { get; set; }
        /// <summary>
        /// Nội dung câu
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        /// <summary>
        /// Vị trí
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 1 - Dễ 
        /// 2 - Trung bình
        /// 3 - Khó
        /// </summary>
        public int Level { get; set; }
        public string LevelName { get; set; }
        /// <summary>
        /// Từ khóa
        /// </summary>
        public string TagIds { get; set; }
        /// <summary>
        /// Từ khóa
        /// </summary>
        [NotMapped]
        public List<string> TagNames { get; set; }
        /// <summary>
        /// Nguồn câu hỏi
        /// </summary>
        public int SourceId { get; set; }
        /// <summary>
        /// 1 - Trắc nghiệm
        /// 2 - Chọn từ vào ô trống
        /// 3 - Điền vào ô trống
        /// 4 - Mindmap
        /// 5 - True/False/Not Given
        /// 6 - Sắp xếp câu  
        /// 7 - Viết 
        /// 8 - Nói
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        [NotMapped]
        public List<IeltsQuestionResultModel> IeltsQuestionResults { get; set; }
        public tbl_IeltsQuestionGroupResult() : base() { }
        public tbl_IeltsQuestionGroupResult(object model) : base(model) { }
    }
}