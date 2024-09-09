using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSCore.Models
{
    public partial class tbl_Notification : DomainEntity
    {
        public string Code { get; set; }
        public int? UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Đã xem
        /// </summary>
        public bool? IsSeen { get; set; }
        /// <summary>
        /// 0 - Thông báo hóa đơn (Leads)
        /// 1 - Tài khoản mới
        /// 2 - Lịch dạy
        /// 3 - Duyệt thanh toán
        /// 4 - Thông tin cá nhân
        /// 5 - Lớp học
        /// 6 - Feedback
        /// 7 - Cảnh báo học viên
        /// 8 - Duyệt lịch nghỉ phép
        /// 9 - Đăng ký nghỉ
        /// 10 - Khách hàng
        /// 11 - Thông mua gói combo
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// json object param
        /// </summary>
        public string ParamString { get; set; }
        public string Url { get; set; }
        /// <summary>
        /// 0 - Thông báo lớp học
        /// 1 - Thông báo tài chính
        /// 2 - Thông báo khách hàng
        /// 3 - Thông báo khác
        /// </summary>
        public int? Category { get; set; }
        /// <summary>
        /// Các Id được lưu sẽ tùy theo Category để gửi cho mobile
        /// </summary>
        public int? AvailableId { get; set; }
        public tbl_Notification() : base() { }
        public tbl_Notification(object model) : base(model) { }
    }
    public class Get_Notification : DomainEntity
    {
        public int? UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Đã xem
        /// </summary>
        public bool? IsSeen { get; set; }
        public int? Type { get; set; }
        public string ParamString { get; set; }
        public object Param { get; set; }
        public int TotalRow { get; set; }
        public string Url { get; set; }
        public int? Category { get; set; }
    }

    public class NotificationToParent : DomainEntity
    {
        public string contentEmail { get; set; }
        public string studentName { get; set; }
        public int parentId { get; set; }
    }
}
