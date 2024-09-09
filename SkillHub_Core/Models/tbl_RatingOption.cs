using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_RatingOption : DomainEntity
    {
        public string Name { get; set; }
        /// <summary>
        /// 0: False
        /// 1: True
        /// </summary>
        public bool TrueOrFalse { get; set; } = false;
        public int RatingQuestionId { get; set; }
        public string Essay { get; set; }
        public tbl_RatingOption() : base() { }
        public tbl_RatingOption(object model) : base(model) { }
    }
    public class Get_RatingOptionSearch:DomainEntity
    {
        public string Name { get; set; }
        /// <summary>
        /// 0: False
        /// 1: True
        /// </summary>
        public bool TrueOrFalse { get; set; } = false;
        public int RatingQuestionId { get; set; }
        public string RatingQuestionName { get; set; }
        public string Essay { get; set; }
        public int TotalRow { get; set; }
    }
}