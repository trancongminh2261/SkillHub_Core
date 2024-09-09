namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Discount : DomainEntity
    {
        public string Code { get; set; }
        /// <summary>
        /// 1 - Giảm tiền 
        /// 2 - Giảm phần trăm
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        /// <summary>
        /// 1 - Gói lẻ
        /// 2 - Gói combo
        /// </summary>
        public int? PackageType { get; set; }
        public string PackageTypeName { get; set; }
        public double Value { get; set; }
        /// <summary>
        /// 1 - Đang diễn ra
        /// 2 - Đã kết thúc
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        public string Note { get; set; }
        public DateTime? Expired { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int? Quantity { get; set; }
        /// <summary>
        /// Số lượng đã dùng
        /// </summary>
        public int? UsedQuantity { get; set; }
        /// <summary>
        /// Khuyến mãi tối đa
        /// </summary>
        public double? MaxDiscount { get; set; }
        /// <summary>
        /// trung tâm
        /// </summary>
        public string BranchIds { get; set; }
        public tbl_Discount() : base() { }
        public tbl_Discount(object model) : base(model) { }
    }
    public class Get_Discount : DomainEntity
    {
        public string Code { get; set; }
        /// <summary>
        /// 1 - Giảm tiền 
        /// 2 - Giảm phần trăm
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        /// <summary>
        /// 1 - Gói lẻ
        /// 2 - Gói combo
        /// </summary>
        public int? PackageType { get; set; }
        public string PackageTypeName { get; set; }
        public double Value { get; set; }
        /// <summary>
        /// 1 - Đang khả dung
        /// 2 - Đã kết thúc
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        public string Note { get; set; }
        public DateTime? Expired { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int? Quantity { get; set; }
        /// <summary>
        /// Số lượng đã dùng
        /// </summary>
        public int? UsedQuantity { get; set; }
        /// <summary>
        /// Khuyến mãi tối đa
        /// </summary>
        public double? MaxDiscount { get; set; }
        public int TotalRow { get; set; }
    }
}