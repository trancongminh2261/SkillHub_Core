namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Comment : DomainEntity
    {
        public int NewsFeedId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
    }
}