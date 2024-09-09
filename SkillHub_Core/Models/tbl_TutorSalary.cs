namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_TutorSalary : DomainEntity
    {
        public int? TutorId { get; set; }
        [NotMapped]
        public string TutorName { get; set; }
        [NotMapped]
        public string TutorCode { get; set; }
        [NotMapped]
        public string TutorAvatar { get; set; }
        public int? TutorSalaryConfigId{ get; set; }
        [NotMapped]
        public double? Salary{ get; set; }
        public string Note { get; set; }
        public tbl_TutorSalary() : base() { }
        public tbl_TutorSalary(object model) : base(model) { }
    }
    public class Get_TutorSalary : DomainEntity
    {
        public int? TutorId { get; set; }
        public string TutorName { get; set; }
        public string TutorCode { get; set; }
        public string TutorAvatar { get; set; }
        public int? TutorSalaryConfigId { get; set; }
        public string Code { get; set; }
        public double? Salary { get; set; }
        public string Note { get; set; }
        public int TotalRow { get; set; }
    }
}