namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    using static LMSCore.Services.ExamResultService;
    public class tbl_PackageStudent : DomainEntity
    {
        public int PackageId { get; set; }
        public int StudentId { get; set; }
        [NotMapped]
        public double Price { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string Tags { get; set; }
        [NotMapped]
        public string Thumbnail { get; set; }
        [NotMapped]
        public double TotalRate { get; set; }
        [NotMapped]
        public double TotalStudent { get; set; }
        public tbl_PackageStudent() : base() { }
        public tbl_PackageStudent(object model) : base(model) { }
    }
    public class Get_PackageStudent : DomainEntity
    {
        public int PackageId { get; set; }
        public int StudentId { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public string Thumbnail { get; set; }
        public double TotalRate { get; set; }
        public double TotalStudent { get; set; }
        public int TotalRow { get; set; }
    }
}