namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_StudentExpectation : DomainEntity
    {
        public int? StudentId { get; set; }
        public int? ClassRegistrationId { get; set; }
        public int? ExectedDay { get; set; } = 1;
        /// <summary>
        /// Ca học
        /// </summary>
        public int? StudyTimeId { get; set; }
        public string Note { get; set; }
        public tbl_StudentExpectation() : base() { }
        public tbl_StudentExpectation(object model) : base(model) { }
    }
}