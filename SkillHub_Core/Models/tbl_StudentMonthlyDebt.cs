namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_StudentMonthlyDebt : DomainEntity
    {
        public int? StudentId { get; set; }
        public int? BranchId { get; set; }
        public int? ClassId { get; set; }
        public double? Price { get; set; }
        public DateTime? Month { get; set; }
        public bool? IsPaymentDone { get; set; }
        [NotMapped]
        public string ClassName { get; set; }
        [NotMapped]
        public int? ClassType { get; set; }
        [NotMapped]
        public string ClassTypeName { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public string Avatar { get; set; }
        [NotMapped]
        public string Mobile { get; set; }
        [NotMapped]
        public string Email { get; set; }
        public tbl_StudentMonthlyDebt() : base() { }
        public tbl_StudentMonthlyDebt(object model) : base(model) { }

    }
    public class Get_StudentMonthlyDebt: DomainEntity
    {
        public int? StudentId { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public double? Price { get; set; }
        public DateTime? Month { get; set; }
        public bool? IsPaymentDone { get; set; }
        public int? ClassType { get; set; }
        public string ClassTypeName { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string Avatar { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public int TotalRow{ get; set; }
    }
}