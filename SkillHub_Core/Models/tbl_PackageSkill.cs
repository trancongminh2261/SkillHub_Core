namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_PackageSkill : DomainEntity
    {
        public int PackageSectionId { get; set; }
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// đề thi
        /// </summary>
        public int IeltsExamId { get; set; }
        public tbl_PackageSkill() : base() { }
        public tbl_PackageSkill(object model) : base(model) { }
    }
}