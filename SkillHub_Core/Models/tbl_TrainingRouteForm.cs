using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static LMSCore.Models.lmsEnum;

namespace LMSCore.Models
{
    public class tbl_TrainingRouteForm : DomainEntity
    {
        public int? TrainingRouteId { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        [NotMapped]
        public int? TotalExam { get; set; }
        [NotMapped]
        public int? CompletedExam { get; set; }
        public tbl_TrainingRouteForm() : base() { }
        public tbl_TrainingRouteForm(object model) : base(model) { }
    }


    public class Get_TrainingRouteForm : DomainEntity
    {
        public int? TrainingRouteId { get; set; }
        public string TrainingRouteName { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public int? TotalExam { get; set; }
        public int? CompletedExam { get; set; }
        public int TotalRow { get; set; }
    }
}