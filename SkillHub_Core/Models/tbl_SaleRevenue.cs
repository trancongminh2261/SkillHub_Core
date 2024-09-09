using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    // thanh toán cho bill nào, với số tiền bao nhiêu
    public class tbl_SaleRevenue : DomainEntity
    {
        public int SaleId { get; set; } 
        public int BillId { get; set; }
        // đợt đó thanh toán bao nhiêu
        public double Value { get; set; }
        public int? Status { get; set; }
        public tbl_SaleRevenue() : base() { }
        public tbl_SaleRevenue(object model) : base(model) { }
    }
    public class Get_SaleRevenue : DomainEntity
    {
        public int SaleId { get; set; }
        public string SaleName { get; set; }
        public int BillId { get; set; }
        // đợt đó thanh toán bao nhiêu
        public double Value { get; set; }
        public double ToTalPrice { get; set; }
        public double Reduced { get; set; }
        public double Debt { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; } 
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public int TotalRow { get; set; } 
        public Get_SaleRevenue(object model) : base(model) { }
    }
}