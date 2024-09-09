using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_ZnsConfig : DomainEntity
    {
        /// <summary>
        /// id ứng dụng
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// khóa bí mật của ứng dụng
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// RefreshToken
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// AccessToken
        /// </summary>
        public string AccessToken { get; set; }
        public double ExpiresIn { get; set; }
        public DateTime TokenModifiedOn { get; set; }
        
    }
    public class Get_ZnsConfig : DomainEntity
    {
        /// <summary>
        /// id ứng dụng
        /// </summary>
        public string AppId { get; set; }
        //ngày cập nhật token
        public DateTime TokenModifiedOn { get; set; }
        public Get_ZnsConfig() : base() { }
        public Get_ZnsConfig(object model) : base(model) { }
    }
}