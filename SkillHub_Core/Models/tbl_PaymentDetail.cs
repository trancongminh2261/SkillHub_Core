using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_PaymentDetail:DomainEntity
    {
        public string PaymentType { get; set; }
        public string PartnerCode { get; set; }
        public string BillCode { get; set; }
        /// <summary>
        /// Số tiền
        /// </summary>
        public string Amount { get; set; }
        public int Status { get; set; }
        /// <summary>
        /// Trạng thái thanh toán "chờ thanh toán" hoặc   khởi tạo giao dịch chưa có IPN
        /// </summary>
        public string StatusName {
            get { 
                return Status==0? "Chờ thanh toán":
                        Status==1?"Thành công":
                        Status==2?"Thất bại":"Unknown";
            }
        }
        /// <summary>
        /// Mã lỗi giao dịch
        /// </summary>
        public string PayStatus { get; set; } 
        /// <summary>
        /// thông tin mã lỗi
        /// </summary>
        public string PayInfo { get; set; }
        /// <summary>
        /// Mã website của merchant trên hệ thống của VNPAY. Ví dụ: 2QXUI4J4
        /// </summary>
        public string WebsiteCode { get; set; }
        /// <summary>
        /// Mã giao dịch ghi nhận tại hệ thống VNPAY. Ví dụ: 20170829153052
        /// </summary>
        public string PurcharCode { get; set; }
        /// <summary>
        /// Mã ngân hàng
        /// </summary>
        public string BankCode { get; set; }
        public tbl_PaymentDetail() : base() { }
        public tbl_PaymentDetail(object model) : base(model) { }
    }
}