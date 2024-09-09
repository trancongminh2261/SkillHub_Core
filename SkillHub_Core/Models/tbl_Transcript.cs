namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Transcript : DomainEntity
    {
        public string Name { get; set; }
        public int? ClassId { get; set; }
    }
}