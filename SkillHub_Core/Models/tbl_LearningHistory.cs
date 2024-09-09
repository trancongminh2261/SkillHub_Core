using LMSCore.Areas.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_LearningHistory : DomainEntity
    { 
        public int StudentId { get; set; }
        public int? ClassId { get; set; }
        public string Content { get; set; }
        public tbl_LearningHistory() : base() { }
        public tbl_LearningHistory(object model) : base(model) { }
    }
    public class Get_LearningHistory : DomainEntity
    {
        public int StudentId { get; set; }
        public int? ClassId { get; set; }
        public string Content { get; set; }
        public int TotalRow { get; set; }
    }
}