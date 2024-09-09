namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;

    public class tbl_HomeworkFile : DomainEntity
    {
        public int? HomeworkId { get; set; }
        public int? UserId { get; set; }
        public string File { get; set; }
        /// <summary>
        /// 1 Giao bài tập
        /// 2 Nộp bài tập
        /// </summary>
        public HomeworkFileType Type { get; set; }
        public string TypeName { get; set; }
        public tbl_HomeworkFile() : base() { }
        public tbl_HomeworkFile(object model) : base(model) { }
    }
}