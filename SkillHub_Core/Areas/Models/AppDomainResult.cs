using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMSCore.Areas.Models
{
    public class AppDomainResult
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public bool NoContent { get; set; }
        /// <summary>
        /// Sài cho những hàm để check trạng thái và trả ra một data khác
        /// </summary>
        public bool IsActive { get; set; }
        public int TotalRow { get; set; }
        /// <summary>
        /// Dùng chung cho các field show số lượng không phải là totalRow
        /// </summary>
        public int Total { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class AppDomainResult<T>
    {
        public bool Success { get; set; }
        public IList<T> Data { get; set; }
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public int TotalRow { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class AppActionResultDetail<T>
    {
        public string message { get; set; }
        public T data { get; set; }
    }
    public class AppActionResult<T>
    {
        public string message { get; set; }
        public int totalRow { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public int totalPage
        {
            get
            {
                decimal count = this.totalRow;
                if (count > 0)
                    return (int)Math.Ceiling(count / pageSize);
                else return 0;
            }
        }
        public IList<T> data { get; set; }
    }
}