namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Point : DomainEntity
    {
        public int? TranscriptId { get; set; }
        public int? StudentId { get; set; }
        public string Listening { get; set; }
        public string Speaking { get; set; }
        public string Reading { get; set; }
        public string Writing { get; set; }
        public string Grammar { get; set; }
        public string Medium { get; set; }
        public bool? PassOrFail { get; set; }
        public string Note { get; set; }
        [NotMapped]
        public string TranscriptModel { get; set; }
        [NotMapped]
        public string StudentModel { get; set; }
        public tbl_Point() : base() { }
        public tbl_Point(object model) : base(model) { }
    }
    public class Get_Point : DomainEntity
    {
        public int? TranscriptId { get; set; }
        public int? StudentId { get; set; }
        public string Listening { get; set; }
        public string Speaking { get; set; }
        public string Reading { get; set; }
        public string Writing { get; set; }
        public string Grammar { get; set; }
        public string Medium { get; set; }
        public bool? PassOrFail { get; set; }
        public string Note { get; set; }
        public string TranscriptModel { get; set; }
        public string StudentModel { get; set; }
        public string ClassModel { get; set; }
        public int TotalRow { get; set; }
    }
}