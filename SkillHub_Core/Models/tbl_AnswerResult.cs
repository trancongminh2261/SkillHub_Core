namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_AnswerResult : DomainEntity
    {
        public int? ExerciseResultId { get; set; }
        public int? AnswerId { get; set; }
        public string AnswerContent { get; set; }
        public int Index { get; set; }
        public bool? IsTrue { get; set; }
        public int? MyAnswerId { get; set; }
        public string MyAnswerContent { get; set; }
        public bool? MyResult { get; set; }
        public int MyIndex { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public AnswerType Type { get; set; }
        public string Comment { get; set; }
        public string FileAudio { get; set; }
        public tbl_AnswerResult() : base() { }
        public tbl_AnswerResult(object model) : base(model) { }
    }
}