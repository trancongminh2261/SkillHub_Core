using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_StudentRollUpQrCode : DomainEntity
    {
        public int StudentId { get; set; }
        public int ScheduleId { get; set; }
        public int ClassId { get; set; }
        public string QrCode { get; set; }
        public string Note { get; set; }
        public tbl_StudentRollUpQrCode() : base() { }
        public tbl_StudentRollUpQrCode(object model) : base(model) { }
    }
}