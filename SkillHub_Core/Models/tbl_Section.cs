namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_Section : DomainEntity
    {
        public int? VideoCourseId { get; set; }
        public string Name { get; set; }
        public int? Index { get; set; }
        public tbl_Section() : base() { }
        public tbl_Section(object model) : base(model) { }
    }
    public class SectionModel
    {
        public int Id { get; set; }
        public int? VideoCourseId { get; set; }
        public string Name { get; set; }
        public int? Index { get; set; }
        public bool isCompleted { get; set; }
    }
}