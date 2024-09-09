namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_FrequentlyQuestion : DomainEntity
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string RoleIds { get; set; }
        [NotMapped]
        public string RoleNames { get; set; }
        public tbl_FrequentlyQuestion() : base() { }
        public tbl_FrequentlyQuestion(object model) : base(model) { }
    }
}