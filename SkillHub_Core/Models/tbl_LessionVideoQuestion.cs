using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_LessionVideoQuestion : DomainEntity
    {
        public int TestId { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 1:Dễ
        /// 2:Tb
        /// 3:Khó
        /// </summary>
        public int Level { get; set; }
        public string LevelName { get; set; }
        public tbl_LessionVideoQuestion() : base() { }
        public tbl_LessionVideoQuestion(object model) : base(model) { }
    }
    public class Get_LessionVideoQuestion : DomainEntity
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string LevelName { get; set; }
        public int TotalRow { get; set; }
    }
    //public class Get_LessionVideoSetQuestion
    //{
    //    public int TestId { get; set; }
    //    public int TestName { get; set; }
    //    public int QuestionId { get; set; }
    //    public string QuestionName { get; set; }
    //    public int Level { get; set; }
    //    public string LevelName { get; set; }
    //    public int TotalRow { get; set; }
    //    public string ListOption { get; set; } 
    //}
}