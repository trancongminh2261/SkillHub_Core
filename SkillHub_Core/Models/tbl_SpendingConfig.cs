namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;

    public class tbl_SpendingConfig : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public int? BranchId { get; set; }
        public tbl_SpendingConfig() : base() { }
        public tbl_SpendingConfig(object model) : base(model) { }
    }
}