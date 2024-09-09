namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class tbl_PaymentApprove : DomainEntity
    {
        /// <summary>
        /// Yêu cầu từ ai
        /// </summary>
        public int? UserId { get; set; }
        public int? BillId { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Đã duyệt 
        /// 3 - Không duyệt
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// Số tiền
        /// </summary>
        public double Money { get; set; }
        public string Note { get; set; }
        public int? PaymentMethodId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int CreateById { get; set; }
        [NotMapped]
        public string BillCode { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        public tbl_PaymentApprove() : base() { }
        public tbl_PaymentApprove(object model) : base(model) { }
    }
    public class Get_PaymentApprove : DomainEntity
    {
        /// <summary>
        /// Yêu cầu từ ai
        /// </summary>
        public int? UserId { get; set; }
        public int? BillId { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Đã duyệt 
        /// 3 - Không duyệt
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// Số tiền
        /// </summary>
        public double Money { get; set; }
        /// <summary>
        /// Số tiền duyệt
        /// </summary>
        public double MoneyApprove { get; set; }
        /// <summary>
        /// Hoàn trả
        /// </summary>
        public double Refunded { get; set; }
        public string Note { get; set; }
        public string BillCode { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int TotalRow { get; set; }
        public double TotalMoney { get; set; }
        public int AllState { get; set; }
        public int Opened { get; set; }
        public int Approved { get; set; }
        public int Canceled { get; set; }
    }
}