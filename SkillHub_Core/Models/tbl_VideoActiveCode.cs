namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_VideoActiveCode : DomainEntity
    {
        public int BillDetailId { get; set; }
        public int ProductId { get; set; }
        public int StudentId { get; set; }
        public string ActiveCode { get; set; }
        /// <summary>
        /// true - đã dùng
        /// </summary>
        public bool IsUsed { get; set; }
        [NotMapped]
        public string ProductName { get; set; }
        [NotMapped]
        public string Thumbnail { get; set; }
        [NotMapped]
        public string StudentName { get; set; }
        [NotMapped]
        public string StudentCode { get; set; }
        public tbl_VideoActiveCode() : base() { }
        public tbl_VideoActiveCode(object model) : base(model) { }
    }
    public class Get_VideoActiveCode : DomainEntity
    {
        public int BillDetailId { get; set; }
        public int ProductId { get; set; }
        public int StudentId { get; set; }
        public string ActiveCode { get; set; }
        /// <summary>
        /// true - đã dùng
        /// </summary>
        public bool IsUsed { get; set; }
        public string ProductName { get; set; }
        public string Thumbnail { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public int TotalRow { get; set; }
    }
}