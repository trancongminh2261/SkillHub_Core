using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_LessionVideoOption:DomainEntity
    {
        public int QuestionId { get; set; } 
        public string Content { get; set; }
        public bool TrueFalse { get; set; }
        public tbl_LessionVideoOption() : base() { }
        public tbl_LessionVideoOption(object model) : base(model) { }
    }
    public class Get_LessionVideoOption : DomainEntity
    {
        public int QuestionId { get; set; }
        public string QuestionName { get; set; }
        public string Content { get; set; }
        public bool TrueFalse { get; set; }
        public int TotalRow { get; set; }
    }
}