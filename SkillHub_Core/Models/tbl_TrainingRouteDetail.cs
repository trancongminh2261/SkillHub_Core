using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static LMSCore.Models.lmsEnum;

namespace LMSCore.Models
{
    public class tbl_TrainingRouteDetail : DomainEntity
    {
        public string Skill{ get; set; }
        /// <summary>
        /// 0 - Basic
        /// 1 - Advance
        /// 2 - Master
        /// </summary>
        public int Level { get; set; }
        public int? TrainingRouteId { get; set; }
        public int? TrainingRouteFormId{ get; set; }
        public int Index { get; set; }
        public int? IeltsExamId { get; set; }
        [NotMapped]
        public string IeltsExamName { get; set; }

        /// <summary>
        /// Số lần test
        /// </summary>
        [NotMapped]
        public int? DoingTimes { get; set; }
        /// <summary>
        /// Lần test gần nhất
        /// </summary>
        [NotMapped]
        public DateTime? LastestDoing { get; set; }
        /// <summary>
        /// Điểm cao nhất
        /// </summary>
        [NotMapped]
        public double? HighestScore { get; set; }
        public tbl_TrainingRouteDetail() : base() { }
        public tbl_TrainingRouteDetail(object model) : base(model) { }
    }


    public class Get_TrainingRouteDetail : DomainEntity
    {
        public string Skill { get; set; }
        /// <summary>
        /// 0 - Basic
        /// 1 - Advance
        /// 2 - Master
        /// </summary>
        public int Level { get; set; }
        public int TrainingRouteId { get; set; }
        public string TrainingRouteName { get; set; }
        public int TrainingRouteFormId { get; set; }
        public string TrainingRouteFormName { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        /// <summary>
        /// Số lần test
        /// </summary>
        public int? DoingTimes { get; set; }
        /// <summary>
        /// Lần test gần nhất
        /// </summary>
        public DateTime? LastestDoing { get; set; }
        /// <summary>
        /// Điểm cao nhất
        /// </summary>
        public double? HighestScore { get; set; }
        public int TotalRow { get; set; }
    }
}