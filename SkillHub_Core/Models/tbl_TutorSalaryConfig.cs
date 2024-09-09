namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_TutorSalaryConfig: DomainEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public double? Salary{ get; set; }
        public tbl_TutorSalaryConfig() : base() { }
        public tbl_TutorSalaryConfig(object model) : base(model) { }
    }
    public class Get_TutorSalaryConfig : DomainEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public double? Salary { get; set; }
    }
}