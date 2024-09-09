namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Curriculum : DomainEntity
    {
        public int? ProgramId { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Số buổi
        /// </summary>
        public int? Lesson { get; set; }
        /// <summary>
        /// Thời gian
        /// </summary>
        public int? Time { get; set; }
        public tbl_Curriculum() : base() { }
        public tbl_Curriculum(object model) : base(model) { }
    }
}