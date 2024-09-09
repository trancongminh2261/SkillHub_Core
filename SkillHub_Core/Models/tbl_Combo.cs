namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using static LMSCore.Models.lmsEnum;

    public class tbl_Combo : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProgramIds { get; set; }
        public double? Value { get; set; }
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; }
        public string StatusName { get; set; }
        [NotMapped]
        public List<ProgramModel> Programs { get; set; }
        [NotMapped]
        public double PriceProgram { get; set; } = 0;
        [NotMapped]
        public double ReducePrice { get; set; } = 0;
        [NotMapped]
        public double TotalPrice { get; set; } = 0;
        public tbl_Combo() : base() { }
        public tbl_Combo(object model) : base(model) { }
    }
    public class Get_Combo : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProgramIds { get; set; }
        public double? Value { get; set; }
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; }
        public string StatusName { get; set; }
        public List<ProgramModel> Programs { get; set; }
        public double PriceProgram { get; set; } = 0;
        public double ReducePrice { get; set; } = 0;
        public double TotalPrice { get; set; } = 0;

        public int TotalRow { get; set; } = 0;
        public Get_Combo() : base() { }
        public Get_Combo(object model) : base(model) { }
    }
    public class ProgramModel
    {
        public int Id { get; set; }
        public int? GradeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public double? Price { get; set; }
        public string Description { get; set; }
        public int? Index { get; set; }
        public string GradeCode { get; set; }
        public string GradeName { get; set; }
    }
}