namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_MarkQuantity : DomainEntity
    {
        public int StudentId { get; set; }
        /// <summary>
        /// Tổng số lượng
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Đã dùng
        /// </summary>
        public int UsedQuantity { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        public tbl_MarkQuantity() : base() { }
        public tbl_MarkQuantity(object model) : base(model) { }
    }
    public class Get_MarkQuantity : DomainEntity
    {
        public int StudentId { get; set; }
        /// <summary>
        /// Tổng số lượng
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Đã dùng
        /// </summary>
        public int UsedQuantity { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int TotalRow { get; set; }
    }
}