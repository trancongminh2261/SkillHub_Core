using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_JobPosition : DomainEntity
    {
        public string Name { get; set; }
        public tbl_JobPosition() : base() { }
        public tbl_JobPosition(object model) : base(model) { }
    }
    public class Get_JobPosition : DomainEntity
    {
        public string Name { get; set; }
        public int TotalRow { get; set; }
    }
}