namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;

    public class tbl_CustomerAudio : DomainEntity
    {
        public int? CustomerId { get; set; }
        public string File { get; set; }
        public string Note { get; set; }
        public int? UserCreateId { get; set; }
        [NotMapped]
        public string UserCreateName { get; set; }
        public tbl_CustomerAudio() : base() { }
        public tbl_CustomerAudio(object model) : base(model) { }
    }
}