namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_CurriculumDetail : DomainEntity
    {
        public int? CurriculumId { get; set; }
        public int? Index { get; set; }
        public string Name { get; set; }                                                                                                                                                                                               
        public tbl_CurriculumDetail() : base() { }
        public tbl_CurriculumDetail(object model) : base(model) { }
    }

    public class Get_InCurriculumDetail : DomainEntity
    {
        public int? CurriculumId { get; set; }
        public int? Index { get; set; }
        public string Name { get; set; }
        public List<FileInCurriculumDetailModel> Files { get; set; }
        public Get_InCurriculumDetail() : base() { }
        public Get_InCurriculumDetail(object model) : base(model) { }
    }
}