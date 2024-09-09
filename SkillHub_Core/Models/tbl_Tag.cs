namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Tag : DomainEntity
    {
        public string Name { get; set; }
        public int TagCategoryId { get; set; }
        [NotMapped]
        public string TagCategoryName { get; set; }
        public tbl_Tag() : base() { }
        public tbl_Tag(object model) : base(model) { }
    }
    public class Get_Tag : DomainEntity
    {
        public string Name { get; set; }
        public int TagCategoryId { get; set; }
        public string TagCategoryName { get; set; }
        public int TotalRow { get; set; }
    }
}