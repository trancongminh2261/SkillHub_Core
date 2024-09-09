using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_CurriculumInClass : DomainEntity
    {
        public int? CurriculumId { get; set; }
        public int? ClassId { get; set; }
        public string Name { get; set; }
        public bool? IsComplete { get; set; }
        public double? CompletePercent { get; set; }
        public tbl_CurriculumInClass() : base() { }
        public tbl_CurriculumInClass(object model) : base(model) { }
    }

    public class Get_CurriculumInClass : DomainEntity
    {
        public int? CurriculumId { get; set; }
        public string Name { get; set; }
        public int? Lesson { get; set; }
        public int? Time { get; set; }
        public int? ClassId { get; set; }
        public bool? IsComplete { get; set; }
        public double? CompletePercent { get; set; }
    }
}