namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Refund : DomainEntity
    {
        public int BranchId { get; set; }
        public double Price { get; set; }
        public int? PaymentMethodId { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }
        /// <summary>
        /// 1 - Hoàn tiền thủ công
        /// 2 - Hoàn tiền bảo lưu
        /// 3 - Hoàn tiền chờ xếp lớp
        /// 4 - Hoàn tiền duyệt thanh toán
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Đã duyệt
        /// 3 - Hủy
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        public int? StudentId { get; set; }
        public int? ClassRegistrationId { get; set; }
        public int? ClassReserveId { get; set; }
        public int? BillId { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        public tbl_Refund() : base() { }
        public tbl_Refund(object model) : base(model) { }
    }
    public class Get_Refund : DomainEntity
    {
        public int BranchId { get; set; }
        public double Price { get; set; }
        public int? PaymentMethodId { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }
        /// <summary>
        /// 1 - Hoàn tiền thủ công
        /// 2 - Hoàn tiền bảo lưu
        /// 3 - Hoàn tiền chờ xếp lớp
        /// 4 - Hoàn tiền thanh toán dư
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Đã duyệt
        /// 3 - Hủy
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        public int? StudentId { get; set; }
        public int? ClassRegistrationId { get; set; }
        public int? ClassReserveId { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string BranchName { get; set; }
        public int TotalRow { get; set; }
        public double TotalPrice { get; set; }
        public int AllState { get; set; }
        public int Opened { get; set; }
        public int Approved { get; set; }
        public int Canceled { get; set; }
    }
    public class RefundStatus
    {
        public int AllState { get; set; }
        public int Opened { get; set; }
        public int Approved { get; set; }
        public int Canceled { get; set; }
    }
}