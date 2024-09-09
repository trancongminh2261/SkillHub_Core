using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    /// <summary>
    /// gói học phí
    /// </summary>
    public class tbl_TuitionPackage : DomainEntity
    { 
        public string Code { get; set; }
        /// <summary>
        /// Số tháng
        /// </summary>
        public int Months { get; set; }
        /// <summary>
        /// 1 - Giảm theo số tiền
        /// 2 - Giảm theo phần trăm 
        /// </summary>
        public int DiscountType { get; set; }
        public string DiscountTypeName { get; set; }
        public double Discount { get; set; }
        /// <summary>
        /// Giảm tối đa
        /// </summary>
        public double? MaxDiscount { get; set; }
        public string Description { get; set; }
        public tbl_TuitionPackage() : base() { }
        public tbl_TuitionPackage(object model) : base(model) { }
    }
}