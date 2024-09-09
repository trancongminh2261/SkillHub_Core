namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_PaymentAllow : DomainEntity
    {
        public int? UserId { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        public tbl_PaymentAllow() : base() { }
        public tbl_PaymentAllow(object model) : base(model) { }
    }
}