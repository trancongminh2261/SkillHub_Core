namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_SalaryConfig : DomainEntity
    {
        public int UserId { get; set; }
        public double Value { get; set; }
        public string Note { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public int RoleId { get; set; }
        [NotMapped]
        public string RoleName { get; set; }
        public tbl_SalaryConfig() : base() { }
        public tbl_SalaryConfig(object model) : base(model) { }
    }
    public class Get_SalaryConfig : DomainEntity
    {
        public int UserId { get; set; }
        public double Value { get; set; }
        public string Note { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int TotalRow { get; set; }
    }
}