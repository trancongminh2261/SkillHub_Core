namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_Config 
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Register - Đăng ký tài khoản
        /// OpenTutoring - Đặt lịch trước bao nhiêu tiếng
        /// CancelTutoring - Hủy lịch trước bao nhiêu tiếng
        /// AllowDeviceLimit - Giới hạn thiết bị
        /// </summary>
        public string Code { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int? Quantity { get; set; }
    }
}