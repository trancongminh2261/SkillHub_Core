namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_SectionCompleted : DomainEntity
    {
        public int? UserId { get; set; }
        public int? VideoCourseId { get; set; }
        public int? SectionId { get; set; }
        public tbl_SectionCompleted() : base() { }
        public tbl_SectionCompleted(object model) : base(model) { }
    }
}