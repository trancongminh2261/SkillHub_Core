namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using System.Reflection;
    using static LMSCore.Models.lmsEnum;
    public class tbl_StudentAssessment : DomainEntity
    {
        public int ClassId { get; set; }
        public int ScheduleId { get; set; }
        [NotMapped]
        public DateTime? StartTime { get; set; }
        [NotMapped]
        public DateTime? EndTime { get; set; }
        public int StudentId { get; set; }
        public string Listening { get; set; }
        public string Speaking { get; set; }
        public string Reading { get; set; }
        public string Writing { get; set; }
        public string Note { get; set; }
        public tbl_StudentAssessment() : base() { }
        public tbl_StudentAssessment(object model) : base(model) { }

    }
    public class Get_StudentAssessment : DomainEntity
    {
        public int ClassId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ScheduleId { get; set; }
        public int StudentId { get; set; }
        public string Listening { get; set; }
        public string Speaking { get; set; }
        public string Reading { get; set; }
        public string Writing { get; set; }
        public string Note { get; set; }
        public int TotalRow { get; set; }
    }
}