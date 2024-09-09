namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_ExamSectionResult : DomainEntity
    {
        public int? ExamResultId { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Nội dung chú thích đáp án đúng của đoạn văn
        /// </summary>
        public string Explanations { get; set; }
        public int? Index { get; set; }
        public tbl_ExamSectionResult() : base() { }
        public tbl_ExamSectionResult(object model) : base(model) { }
    }
}