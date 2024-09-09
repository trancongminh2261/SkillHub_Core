namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_StudentNote : DomainEntity
    { 
        public int? StudentId { get; set; }
        public string Note { get; set; }
        public tbl_StudentNote() : base() { }
        public tbl_StudentNote(object model) : base(model) { }
    }
}