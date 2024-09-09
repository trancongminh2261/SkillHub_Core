using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMSCore.Areas.Models
{
    public class ObjectResult
    {
        public object obj { get; set;}
        public int TotalRow { get; set;}
        public object IsDone { get; set; }
    }
}