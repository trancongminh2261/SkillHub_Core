namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_ZoomConfig : DomainEntity
    {
        /// <summary>
        /// Tên
        /// </summary>
        public string Name { get; set; }
        public string AccountId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool? Active { get; set; }
        public tbl_ZoomConfig() : base() { }
        public tbl_ZoomConfig(object model) : base(model) { }
    }
}