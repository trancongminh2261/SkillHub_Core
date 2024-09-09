namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Job : DomainEntity
    {
        public string Name { get; set; }
        public tbl_Job() : base() { }
        public tbl_Job(object model) : base(model) { }
    }
}