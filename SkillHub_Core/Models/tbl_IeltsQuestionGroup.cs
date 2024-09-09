using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSCore.Models
{
    public class tbl_IeltsQuestionGroup : DomainEntity
    {
        public int IeltsExamId { get; set; }
        public int IeltsSkillId { get; set; }
        public int IeltsSectionId { get; set; }
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
        /// Nguồn câu hỏi
        /// </summary>
        public int SourceId { get; set; }
        /// <summary>
        /// 1 - Trắc nghiệm
        /// 2 - Chọn từ vào ô trống
        /// 3 - Điền vào ô trống
        /// 4 - Mindmap
        /// 5 - True/False/Not Given
        /// 6 - Sắp xếp câu  câu 
        /// 7 - Viết 
        /// 8 - Nói
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        [NotMapped]
        public List<string> TagNames { get; set; }
        [NotMapped]
        public List<IeltsQuestionModel> IeltsQuestions { get; set; }
        /// <summary>
        /// Kiểm tra có tồn tại trong đề hay không
        /// </summary>
        [NotMapped]
        public bool HasInIeltsExam { get; set; }
        public tbl_IeltsQuestionGroup() : base() { }
        public tbl_IeltsQuestionGroup(object model) : base(model) { }
    }
    public class Get_IeltsQuestionGroup : DomainEntity
    {
        public int IeltsExamId { get; set; }
        public int IeltsSkillId { get; set; }
        public int IeltsSectionId { get; set; }
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
        public List<string> TagNames { get; set; }
        public List<IeltsQuestionModel> IeltsQuestions { get; set; }
        public int TotalRow { get; set; }
    }
}