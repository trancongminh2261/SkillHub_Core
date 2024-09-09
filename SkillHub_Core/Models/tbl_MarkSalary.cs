namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_MarkSalary : DomainEntity
    {
        public int TeacherId { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        /// <summary>
        /// Lương chấm bài
        /// </summary>
        public double Salary { get; set; }
        public tbl_MarkSalary() : base() { }
        public tbl_MarkSalary(object model) : base(model) { }
    }
    public class Get_MarkSalary : DomainEntity
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        /// <summary>
        /// Lương chấm bài
        /// </summary>
        public double Salary { get; set; }
        public int TotalRow { get; set; }
    }
}