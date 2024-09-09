using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_CommissionCampaign : DomainEntity
    {
        /// <summary>
        /// Mã sales
        /// </summary>
        public int SalesId { get; set; }
        // chỉ dùng cho tháng đó thôi
        public int Year { get; set; }
        public int Month { get; set; }
        /// <summary>
        /// Hoa hồng trong tháng
        /// </summary>
        public double Commission { get; set; }
        public double Percent { get; set; }
        public string Descriptions { get; set; }
        /// <summary>
        /// Mốc doanh thu
        /// </summary>
        public double RevenueTargets { get; set; }
        /// <summary>
        /// Trạng thái chốt hoa hồng
        /// 0 - chưa chốt
        /// 1 - đã chốt
        /// </summary>
        public int Status { get; set; }
        public tbl_CommissionCampaign() : base() { }
        public tbl_CommissionCampaign(object model) : base(model) { }
    }
    public class Get_CommissionCampaignSearch : DomainEntity
    {
        public string FullName { get; set; } 
        public string UserCode { get; set; } 
        public int? RoleId { get; set; } 
        public string RoleName { get; set; } 
        public int? Year { get; set; }
        public int? Month { get; set; }
        public double? Commission { get; set; }
        public double? Percent { get; set; }
        public string Descriptions { get; set; }
        public double? RevenueTargets { get; set; }
        public double? TotalRevenue { get; set; }
        public int? Status { get; set; }
        public string StatusName
        {
            get
            {
                return Status == 0 ? "Chưa chốt"
                    : Status == 1 ? "Đã chốt" : "";
            }
        }
        public double? PercentCommission { get; set; }
        public double? ExpectedCommission { get; set; }
        public int TotalRow { get; set; }
    }
}