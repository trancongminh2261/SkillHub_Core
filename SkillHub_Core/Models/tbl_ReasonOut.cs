namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using static LMSCore.Models.lmsEnum;

    public class tbl_ReasonOut : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public tbl_ReasonOut() : base() { }
        public tbl_ReasonOut(object model) : base(model) { }
    }
    public class Get_ReasonOut : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int TotalRow { get; set; }
    }
    public class ReasonOutDropDown
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}