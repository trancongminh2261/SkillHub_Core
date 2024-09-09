namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_DayOff : DomainEntity
    {
        public string Name { get; set; }
        public DateTime? sDate { get; set; }
        public DateTime? eDate { get; set; }
        public string BranchIds { get; set; }
        public tbl_DayOff() : base() { }
        public tbl_DayOff(object model) : base(model) { }
    }

    public class Get_DayOff : DomainEntity
    {
        public string Name { get; set; }
        public DateTime? sDate { get; set; }
        public DateTime? eDate { get; set; }
        public string BranchIds { get; set; }
        public int TotalRow { get; set; }

    }
}