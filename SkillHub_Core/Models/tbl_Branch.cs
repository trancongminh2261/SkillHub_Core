namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Branch : DomainEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? AreaId { get; set; } 
        public int? DistrictId { get; set; }
        public int? WardId { get; set; } 
        public string Address { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Mobile { get; set; }
        public string Email { get; set; }
        public tbl_Branch() : base() { }
        public tbl_Branch(object model) : base(model) { }
    }
}