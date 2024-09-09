namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_StudyTime : DomainEntity
    {
        public string Name { get; set; }
        /// <summary>
        /// định dạng HH:mm
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// định dạng HH:mm
        /// </summary>
        public string EndTime { get; set; }
        public double Time { get; set; }
        public tbl_StudyTime() : base() { }
        public tbl_StudyTime(object model) : base(model) { }
    }
}