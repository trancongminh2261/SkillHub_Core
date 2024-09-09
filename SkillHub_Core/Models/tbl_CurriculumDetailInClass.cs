using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_CurriculumDetailInClass : DomainEntity
    {
        public int? CurriculumIdInClass { get; set; }
        public int? CurriculumDetailId { get; set; }
        public string Name { get; set; }
        public int? Index { get; set; }
        public bool? IsComplete { get; set; }
        public double? CompletePercent { get; set; }
        public bool IsHide { get; set; }
        public tbl_CurriculumDetailInClass() : base() { }
        public tbl_CurriculumDetailInClass(object model) : base(model) { }
    }

    public class Get_CurriculumDetailInClass : DomainEntity
    {
        public int? CurriculumIdInClass { get; set; }
        public int? CurriculumDetailId { get; set; }
        public string Name { get; set; }
        public int? Index { get; set; }
        public bool? IsComplete { get; set; }
        public double? CompletePercent { get; set; }
        public bool IsHide { get; set; }
        public List<Get_FileCurriculumInClass> Files { get; set; }
    }
}