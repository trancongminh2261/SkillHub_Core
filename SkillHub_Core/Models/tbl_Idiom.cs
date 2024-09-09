namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Idiom : DomainEntity
    {
        public string Content { get; set; }
        public tbl_Idiom() : base() { }
        public tbl_Idiom(object model) : base(model) { }
    }
}