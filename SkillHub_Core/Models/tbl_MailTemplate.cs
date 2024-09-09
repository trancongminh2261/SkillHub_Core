using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_MailTemplate : DomainEntity
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        /// <summary>
        /// 1 - Thư mời tham dự phỏng vấn
        /// 2 - Thư thông báo ứng viên được nhận
        /// 3 - Thư thông báo ứng viên bị loại
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public tbl_MailTemplate() : base() { }
        public tbl_MailTemplate(object model) : base(model) { }
    }
}