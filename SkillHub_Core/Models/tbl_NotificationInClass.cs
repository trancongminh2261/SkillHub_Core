namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_NotificationInClass : DomainEntity
    {
        public int? ClassId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool? IsSendMail { get; set; }
        public tbl_NotificationInClass() : base() { }
        public tbl_NotificationInClass(object model) : base(model) { }
    }
}