namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using System.Reflection;
    using static LMSCore.Models.lmsEnum;
    public class tbl_ScheduleAvailable : DomainEntity
    {
        public int TeacherId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Note { get; set; }
        [NotMapped]
        public string TeacherName { get; set; }
        [NotMapped]
        public string TeacherCode { get; set; }
        public tbl_ScheduleAvailable() : base() { }
        public tbl_ScheduleAvailable(object model) : base(model) { }
    }
    public class Get_ScheduleAvailable : DomainEntity
    {
        public int TeacherId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Note { get; set; }
        public string TeacherName { get; set; }
        public string TeacherCode { get; set; }
        public int TotalRow { get; set; }
    }
}