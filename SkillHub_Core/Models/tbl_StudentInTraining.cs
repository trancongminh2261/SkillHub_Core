using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static LMSCore.Models.lmsEnum;

namespace LMSCore.Models
{
    public class tbl_StudentInTraining : DomainEntity
    {
        public int? StudentId { get; set; }
        public int? TrainingRouteId { get; set; }

        [NotMapped]
        public string StudentFullName { get; set; }
        [NotMapped]
        public string TrainingRouteName { get; set; }
        [NotMapped]
        public int CompletedExam { get; set; }
        [NotMapped]
        public int TotalExam { get; set; }
        [NotMapped]
        public double Progress { get; set; }

        public tbl_StudentInTraining() : base() { }
        public tbl_StudentInTraining(object model) : base(model) { }
    }


    public class Get_StudentInTraining : DomainEntity
    {
        public int? StudentId { get; set; }
        public string StudentFullName { get; set; }
        public int? TrainingRouteId { get; set; }
        public string TrainingRouteName { get; set; }
        [DefaultValue(0)]
        public int? CompletedExam { get; set; }
        [DefaultValue(0)]
        public int? TotalExam { get; set; }
        [DefaultValue(0)]
        public int TotalRow { get; set; }
        public double Progress
        {
            get
            {
                if (TotalExam.HasValue && TotalExam.Value > 0 && CompletedExam.HasValue && CompletedExam.Value > 0)
                {
                    return Math.Round((double)CompletedExam.Value / TotalExam.Value * 100, 2);
                }
                else
                {
                    return 0;
                }
            }
        }
    }

    public class Get_TrainingDoingTest 
    {
        public int? StudentId { get; set; }
        public string StudentFullName { get; set; }
        public int TrainingRouteId { get; set; }
        public string TrainingRouteName { get; set; }
        public int TrainingRouteFormId { get; set; }
        public string TrainingRouteFormName { get; set; }
        public string Skill { get; set; }
        /// <summary>
        /// 0 - Basic
        /// 1 - Advance
        /// 2 - Master
        /// </summary>
        public int Level { get; set; }
        public string LevelName => Level == 0 ? "Basic" : Level == 1 ? "Advance" : Level == 2 ? "Master" : "";

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
        /// <summary>
        /// Điểm cao nhất
        /// </summary>
        public double? AverageScore { get; set; }
       
        [DefaultValue(0)]
        [JsonIgnore]
        public int TotalRow { get; set; }
    }
}