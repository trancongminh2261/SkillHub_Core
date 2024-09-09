using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    /// <summary>
    /// Mẫu bảng điểm
    /// </summary>
    public class tbl_ScoreBoardTemplate : DomainEntity
    {
        //mã
        public string Code { get; set; }
        //tên mẫu 
        public string Name { get; set; }
        public tbl_ScoreBoardTemplate() : base() { }
        public tbl_ScoreBoardTemplate(object model) : base(model) { }
    }
    public class Get_ScoreBoardTemplate : DomainEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int TotalRow { get; set; }
    }
}