namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_TutorInProgram : DomainEntity
    {
        public int? TutorId { get; set; }
        public int? ProgramId { get; set; }
    }
}