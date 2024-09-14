using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMSCore.Areas.Models.ZnsModel
{
    public class ZnsRequest
    {
        public string mode { get; set; }
        public string phone { get; set; }
        public string template_id { get; set; }
        public object template_data { get; set; }
    }
}