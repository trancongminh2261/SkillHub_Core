namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_TeacherInProgram : DomainEntity
    {
        public int? TeacherId { get; set; }
        public int? ProgramId { get; set; }
        /// <summary>
        /// Lương trên buổi dạy
        /// </summary>
        public double? TeachingFee { get; set; }
    }
    public class Get_StaffWhenAllowProgram
    {
        public int UserInformationId { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string BranchIds { get; set; }
        public int TotalRow { get; set; }
    }
}