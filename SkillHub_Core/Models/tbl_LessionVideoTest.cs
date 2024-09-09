using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_LessionVideoTest:DomainEntity
    {
        public int SectionId { get; set; }
        public string Name { get; set; }
        public tbl_LessionVideoTest() : base() { }
        public tbl_LessionVideoTest(object model) : base(model) { }
    }
    public class Get_LessionVideoTest: DomainEntity
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public string Name { get; set; }
        public int TotalRow { get; set; }
    }
}