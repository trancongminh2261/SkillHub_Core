using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_Commission : DomainEntity
    {
        //public int StudentId { get; set; }
        public int SaleId { get; set; } 
        // Tổng giá trị nv đó mang về trong tháng đó
        public double TotalTuitionFee { get; set; }
        // mã chiến dịch, không có thì khỏi tính hoa hồng
        public int CommissionCampaignId { get; set; }
        // % hoa hồng
        public double Percent { get; set; }
        // tổng hoa hồng
        public double Commission { get; set; }
        //public string listCommision { get; set; }
        public tbl_Commission() : base() { }
        public tbl_Commission(object model) : base(model) { }

    }
    // ds chi tiết hoa hồng
    public class Get_CommissionSearch : DomainEntity
    {
        public int SaleId { get; set; }
        public string SaleName { get; set; } 
        // Tổng giá trị nv đó mang về trong tháng đó
        public double TotalTuitionFee { get; set; }
        // mã chiến dịch, không có thì khỏi tính hoa hồng
        public int CommissionCampaignId { get; set; }
        public int CommissionCampaignName { get; set; }
        // % hoa hồng
        public int Percent { get; set; } 
        // tổng hoa hồng
        public double Commission { get; set; } 
        public int TotalRow { get; set; }
    }
    public class Get_OnePreViewCommission
    {
        public int SaleId { get; set; }
        public string SaleName { get; set; }
        // Tổng giá trị nv đó mang về trong tháng đó
        public double TotalTuitionFee { get; set; }
        public int Percent { get; set; } 
        public double Commission { get; set; } 
        public int TotalRow { get; set; }
    }
}