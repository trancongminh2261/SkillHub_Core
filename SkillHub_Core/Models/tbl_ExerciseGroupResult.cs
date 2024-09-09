namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_ExerciseGroupResult : DomainEntity
    {
        public string Name { get; set; }
        public int? ExamResultId { get; set; }
        public int? ExamSectionResultId { get; set; }
        public string Content { get; set; }
        public string Paragraph { get; set; }
        public int? ExerciseNumber { get; set; }
        public ExerciseLevel? Level { get; set; }
        public string LevelName { get; set; }
        public ExerciseType? Type { get; set; }
        public string TypeName { get; set; }
        /// <summary>
        /// Vị trí của nhóm trong đề
        /// </summary>
        public int? Index { get; set; }
        public string Tags { get; set; }
        public tbl_ExerciseGroupResult() : base() { }
        public tbl_ExerciseGroupResult(object model) : base(model) { }
    }
}