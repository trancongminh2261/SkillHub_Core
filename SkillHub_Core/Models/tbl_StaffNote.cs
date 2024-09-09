namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_StaffNote : DomainEntity
    { 
        public int? StaffId { get; set; }
        public string Note { get; set; }
        public tbl_StaffNote() : base() { }
        public tbl_StaffNote(object model) : base(model) { }
    }
}