using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LMSCore.Models
{
    public class tbl_IeltsSectionResult : DomainEntity
    {
        public int IeltsSkillResultId { get; set; }
        public int IeltsExamId { get; set; }
        public int IeltsSkillId { get; set; }
        public int IeltsSectionId { get; set; }
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
        public tbl_IeltsSectionResult() : base() { }
        public tbl_IeltsSectionResult(object model) : base(model) { }
    }
}