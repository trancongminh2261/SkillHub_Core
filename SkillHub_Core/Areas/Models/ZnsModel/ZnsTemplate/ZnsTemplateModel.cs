using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMSCore.Areas.Models.ZnsModel.ZnsTemplate
{
    public class ZnsTemplateModel
    {
        /// <summary>
        /// 1 - thông báo học phí
        /// 2 - thông báo bài kiểm tra
        /// </summary>
        public int Type { get; set; }
        public object TemplateData { get; set; }
    }
}