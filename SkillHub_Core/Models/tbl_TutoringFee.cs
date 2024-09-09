namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_TutoringFee : DomainEntity
    {
        public int? TeacherId { get; set; }
        [NotMapped]
        public string TeacherName { get; set; }
        [NotMapped]
        public string TeacherCode { get; set; }
        [NotMapped]
        public string TeacherAvatar { get; set; }
        /// <summary>
        /// phí dạy kèm
        /// </summary>
        public double Fee { get; set; }
        public string Note { get; set; }
        public tbl_TutoringFee() : base() { }
        public tbl_TutoringFee(object model) : base(model) { }
    }
    public class Get_TutoringFee : DomainEntity
    {
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string TeacherCode { get; set; }
        public string TeacherAvatar { get; set; }
        /// <summary>
        /// phí dạy kèm
        /// </summary>
        public double Fee { get; set; }
        public string Note { get; set; }
        public int TotalRow { get; set; }
    }
}