namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_IeltsAnswerResultComment : DomainEntity
    {
        public int IeltsQuestionResultId { get; set; }
        public int IeltsAnswerResultId { get; set; }
        /// <summary>
        /// Nội dung chấm
        /// </summary>
        public string Content { get; set; }
        public string Audio { get; set; }
        public tbl_IeltsAnswerResultComment() : base() { }
        public tbl_IeltsAnswerResultComment(object model) : base(model) { }
        public string Note { get; set; }
    }

    public class Get_IeltsAnswerResultComment : DomainEntity
    {
        public int IeltsQuestionResultId { get; set; }
        public int IeltsAnswerResultId { get; set; }
        /// <summary>
        /// Nội dung chấm
        /// </summary>
        public string Content { get; set; }
        public string Audio { get; set; }
        public string Note { get; set; }
        public string SampleAnswer { get; set; }
    }
}