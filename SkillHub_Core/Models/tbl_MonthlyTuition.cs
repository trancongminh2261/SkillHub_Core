using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    /// <summary>
    /// Thông tin đóng trước của học viên
    /// </summary>
    public class tbl_MonthlyTuition : DomainEntity
    {
        public int StudentId { get; set; }
        /// <summary>
        /// Lớp học
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// Tháng
        /// </summary>
        public int Month { get; set; }
        /// <summary>
        /// Năm
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// Đơn hàng phát sinh
        /// </summary>
        public int BillId { get; set; }
        /// <summary>
        /// 1 - Chưa thanh toán
        /// 2 - Đã thanh toán
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        [NotMapped]
        public string StudentName { get; set; }
        [NotMapped]
        public string StudentCode { get; set; }
        [NotMapped]
        public string ClassName { get; set; }
        [NotMapped]
        public double Price { get; set; }
        public tbl_MonthlyTuition() : base() { }
        public tbl_MonthlyTuition(object model) : base(model) { }
    }
    public class Get_MonthlyTuition : DomainEntity
    {
        public int StudentId { get; set; }
        /// <summary>
        /// Lớp học
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// Tháng
        /// </summary>
        public int Month { get; set; }
        /// <summary>
        /// Năm
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// Đơn hàng phát sinh
        /// </summary>
        public int BillId { get; set; }
        /// <summary>
        /// 1 - Chưa thanh toán
        /// 2 - Đã thanh toán
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string ClassName { get; set; }
        public int TotalRow { get; set; }
    }
}