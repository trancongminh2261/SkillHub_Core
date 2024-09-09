namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Contract : DomainEntity
    {
        public string Name { get; set; }
        public int StudentId { get; set; }
        public string Content { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        public tbl_Contract() : base() { }
        public tbl_Contract(object model) : base(model) { }
    }
    public class Get_Contract : DomainEntity
    {
        public string Name { get; set; }
        public int StudentId { get; set; }
        public string Content { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int TotalRow { get; set; }
    }
}