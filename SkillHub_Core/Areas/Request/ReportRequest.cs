using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMSCore.Areas.Request
{
    public class ReportRequest
    {
        public DateTime? fromDt { get; set; }
        public DateTime? toDt { get; set; }
    }
}