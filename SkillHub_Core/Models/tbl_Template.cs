namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_Template : DomainEntity
    {
        /// <summary>
        /// 1 - Hợp đồng
        /// 2 - Điều khoản
        /// 3 - Phiếu thu
        /// 4 - Phiếu chi
        /// 5 - Thư mời phỏng vấn
        /// 6 - Thông báo trúng tuyển
        /// 7 - Thông báo kết quả phỏng vấn (dành cho việc thông báo rớt)
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string Content { get; set; }
        public tbl_Template() : base() { }
        public tbl_Template(object model) : base(model) { }
    }
}