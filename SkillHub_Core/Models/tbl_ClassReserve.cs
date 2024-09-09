namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_ClassReserve : DomainEntity
    {
        public int? StudentId { get; set; }
        /// <summary>
        /// Lớp cũ
        /// </summary>
        public int? ClassId { get; set; }
        public double? Price { get; set; }
        public int? BranchId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Đang bảo lưu
        /// 2 - Đã học lại
        /// 3 - Đã hoàn tiền
        /// 4 - Hết hạn bảo lưu
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// Số tiền đã dùng
        /// </summary>
        public double MoneyUsed { get; set; }
        /// <summary>
        /// Số tiền còn lại
        /// </summary>
        public double MoneyRemaining { get; set; }
        /// <summary>
        /// Số tiên đã bảo lưu
        /// </summary>
        public double MoneyRefund { get; set; }
        /// <summary>
        /// Số buổi còn lại sau khi bảo lưu
        /// </summary>
        public int? LessonRemaining { get; set; }
        /// <summary>
        /// Ngày hết hạn bảo lưu
        /// </summary>
        public DateTime? Expires { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public string Avatar { get; set; }
        [NotMapped]
        public string AvatarReSize { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        [NotMapped]
        public string ClassName { get; set; }
        public tbl_ClassReserve() : base() { }
        public tbl_ClassReserve(object model) : base(model) { }
    }
    public class Get_ClassReserve : DomainEntity
    {

        public int? StudentId { get; set; }
        /// <summary>
        /// Lớp cũ
        /// </summary>
        public int? ClassId { get; set; }
        public double? Price { get; set; }
        public int? BranchId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Đang bảo lưu
        /// 2 - Đã học lại
        /// 3 - Đã hoàn tiền
        /// 4 - Hết hạn bảo lưu
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// Số tiền đã dùng
        /// </summary>
        public double MoneyUsed { get; set; }
        /// <summary>
        /// Số tiền còn lại
        /// </summary>
        public double MoneyRemaining { get; set; }
        /// <summary>
        /// Số tiên đã bảo lưu
        /// </summary>
        public double MoneyRefund { get; set; }
        public int? LessonRemaining { get; set; }
        /// <summary>
        /// Ngày hết hạn bảo lưu
        /// </summary>
        public DateTime? Expires { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public string BranchName { get; set; }
        public string ClassName { get; set; }
        public int TotalRow { get; set; }
    }

    public class ReserveProvi
    {
        public double? Paid { get; set; }
        public int? CompletedLesson { get; set; }
        public int? TotalLesson { get; set; }
    }

    public class ReviewReserve
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        /// <summary>
        /// Giá của lớp học
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// 1 - Thanh toán một lần 
        /// 2 - Thanh toán hằng tháng
        /// </summary>
        public int PaymentType { get; set; }
        /// <summary>
        /// Tổng tiền bảo lưu dự tính
        /// </summary>
        public double ForecastPrice { get; set; }
        /// <summary>
        /// Chi tiết loại thanh toán một lần
        /// </summary>
        public OnePaymentDetail OnePaymentDetail { get; set; }
        /// <summary>
        /// Chi tiết loại thanh toán hằng tháng
        /// </summary>
        public MonthlyDetail MonthlyDetail { get; set; }
    }
    public class OnePaymentDetail
    {
        /// <summary>
        /// Tổng buổi học
        /// </summary>
        public int TotalLesson { get; set; }
        /// <summary>
        /// Số buổi học còn lại
        /// </summary>
        public int RemainingLesson { get; set; }
    }
    public class MonthlyDetail
    {
        /// <summary>
        /// Số tháng còn lại
        /// </summary>
        public int RemainingMonth { get; set; }
        /// <summary>
        /// Tổng số tháng
        /// </summary>
        public int TotalMonth { get; set; }
    }
}