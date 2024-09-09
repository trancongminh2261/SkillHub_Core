using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_CommissionConfig : DomainEntity
    {
        public int SalesId { get; set; }
        /// <summary>
        /// tên cấu hình hoa hồng
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Descriptions { get; set; }
        public tbl_CommissionConfig() : base() { }
        public tbl_CommissionConfig(object model) : base(model) { }
    }
}