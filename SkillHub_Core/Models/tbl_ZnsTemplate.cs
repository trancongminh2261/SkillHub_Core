using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_ZnsTemplate : DomainEntity
    {
        /// <summary>
        /// id template
        /// </summary>
        public string TemplateId { get; set; }
        /// <summary>
        /// tên template
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// loại template
        /// 1 - template thông báo học phí
        /// 2 - template thông kết quả bài thi
        /// </summary>
        public int Type { get; set; }
    }
}