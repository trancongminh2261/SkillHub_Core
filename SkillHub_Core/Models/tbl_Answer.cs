namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;

    public class tbl_Answer : DomainEntity
    {
        public int? ExerciseId { get; set; }
        public string AnswerContent { get; set; }
        public bool? IsTrue { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public AnswerType Type { get; set; }
        /// <summary>
        /// Áp dụng cho câu sắp xếp
        /// </summary>
        public int Index { get; set; }
        public tbl_Answer() : base() { }
        public tbl_Answer(object model) : base(model) { }
    }
}