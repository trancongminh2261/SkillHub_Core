using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_CommissionNorm : DomainEntity
    {
        public int? CommissionConfigId { get; set; }
        public string Name { get; set; }
        public double MinNorm { get; set; }
        public double MaxNorm { get; set; }
        /// <summary>
        /// Phần trăm dành cho HV mới
        /// </summary>
        public double PercentNew { get; set; }
        /// <summary>
        /// Phần trăm dành cho HV đã đăng kí 2 khóa học trở lên
        /// </summary>
        public double PercentRenewals { get; set; }
        public tbl_CommissionNorm() : base() { }
        public tbl_CommissionNorm(object model) : base(model) { }
    }
    public class Get_CommissionNormSearch : DomainEntity
    {
        public string Name { get; set; }
        public double MinNorm { get; set; }
        public double MaxNorm { get; set; }
        public int Percent { get; set; }
        public int TotalRow { get; set; }
    }
    public class GetAll_CommisionConfig : DomainEntity
    {
        public int SalesId { get; set; }
        public string Name { get; set; }
        public string Descriptions { get; set; }
        public int CommissionNormId { get; set; }
        public int CommissionConfigId { get; set; }
        public double? MinNorm { get; set; }
        public double? MaxNorm { get; set; }
        public string NormName { get; set; }
        public double? PercentNew { get; set; }
        public double? PercentRenewals { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int TotalRow { get; set; }
    }

    public class Get_CommisionConfig : DomainEntity
    {
        public int SalesId { get; set; }
        public string Name { get; set; }
        public string Descriptions { get; set; }      
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int TotalRow { get; set; }
        public List<Get_CommissionNorm> Get_CommissionNorm { get; set; }
    }
    public class Get_CommissionNorm 
    {
        public int CommissionNormId { get; set; }
        public int CommissionConfigId { get; set; }
        public double? MinNorm { get; set; }
        public double? MaxNorm { get; set; }
        public string NormName { get; set; }
        public double? PercentNew { get; set; }
        public double? PercentRenewals { get; set; }
    }
}