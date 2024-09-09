using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_ConsultantRevenue : DomainEntity
    {
        /// <summary>
        /// tư vấn viên
        /// </summary>
        public int SaleId { get; set; }
        /// <summary>
        /// học viên
        /// </summary>
        public int StudentId { get; set; }
        /// <summary>
        /// hóa đơn
        /// </summary>
        public int BillId { get; set; }
        /// <summary>
        /// thanh toán
        /// </summary>
        public int? PaymentSessionId { get; set; }
        /// <summary>
        /// tổng số tiền cần đóng
        /// </summary>
        public double TotalPrice { get; set; }
        /// <summary>
        /// số tiền thu được
        /// </summary>
        public double AmountPaid { get; set; }

        [NotMapped]
        public string SaleName { get; set; }
        [NotMapped]
        public string SaleCode { get; set; }
        [NotMapped]
        public string SaleAvatar { get; set; }
        [NotMapped]
        public string StudentName { get; set; }
        [NotMapped]
        public string StudentCode { get; set; }
        [NotMapped]
        public string StudentAvatar { get; set; }
        /// <summary>
        /// 1 - Đăng ký học 
        /// 2 - Mua dịch vụ
        /// 3 - Đăng ký lớp dạy kèm
        /// 4 - Tạo thủ công
        /// 5 - Học phí hằng tháng
        /// 6 - Phí chuyển lớp
        /// </summary>
        [NotMapped]
        public int BillType { get; set; }
        [NotMapped]
        public string BillTypeName { get; set; }
        
        public tbl_ConsultantRevenue() : base() { }
        public tbl_ConsultantRevenue(object model) : base(model) { }
    }
    public class Get_ConsultantRevenue : DomainEntity
    {
        public int SaleId { get; set; }
        public string SaleName { get; set; }
        public string SaleCode { get; set; }
        public string SaleAvatar { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string StudentAvatar { get; set; }
        public int BillId { get; set; }
        public int? PaymentSessionId { get; set; }
        public double TotalPrice { get; set; }
        public double AmountPaid { get; set; }
        public int BillType { get; set; }
        public string BillTypeName { get; set; }       
        public int TotalRow { get; set; }
    }
}