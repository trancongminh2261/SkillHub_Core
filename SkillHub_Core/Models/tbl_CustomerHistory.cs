namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
    public class tbl_CustomerHistory : DomainEntity
    {
        /// <summary>
        /// Id khách hàng
        /// </summary>
        public int? CustomerId { get; set; }
        /// <summary>
        /// Id status cả khách hàng bị chỉnh sửa
        /// </summary>
        public int? CustomerStatusId { get; set; }
        public int? BranchId { get; set; }
        public int? SaleId { get; set; }
        /// <summary>
        /// Ngày lên lịch lại (Khách hàng hẹn lại hôm khác)
        /// </summary>
        public DateTime? RescheduledDate { get; set; }
        [NotMapped]
        public string CustomerName { get; set; }
        [NotMapped]
        public string CustomerStatusName { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        [NotMapped]
        public string SaleName { get; set; }
        public tbl_CustomerHistory() : base() { }
        public tbl_CustomerHistory(object model) : base(model) { }
    }
    public class Get_CustomerHistory : DomainEntity
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int CustomerStatusId { get; set; }
        public string CustomerStatusName { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int? SaleId { get; set; }
        public string SaleName { get; set; }
        /// <summary>
        /// Ngày lên lịch lại (Khách hàng hẹn lại hôm khác)
        /// </summary>
        public DateTime? RescheduledDate { get; set; }
    }
}