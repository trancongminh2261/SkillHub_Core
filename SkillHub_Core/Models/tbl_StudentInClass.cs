namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using static LMSCore.Models.lmsEnum;
    public class tbl_StudentInClass : DomainEntity
    {
        public int? BranchId { get; set; }
        public int? ClassId { get; set; }
        public int? StudentId { get; set; }
        public bool? Warning { get; set; }
        public string WarningContent { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Chính thức
        /// 2 - Học thử
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// Hóa đơn khi đăng ký mua gói
        /// </summary>
        public int? BillDetailId { get; set; }
        /// <summary>
        /// 1 - Thanh toán một lần
        /// 2 - Thanh toán theo tháng
        /// </summary>
        [NotMapped]
        public int PaymentType { get; set; }
        [NotMapped]
        public string PaymentTypeName { get; set; }
        public string TypeName { get; set; }
        [NotMapped]
        public string ClassName { get; set; }
        [NotMapped]
        public int? ClassType { get; set; }
        [NotMapped]
        public string ClassTypeName { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public string Avatar { get; set; }
        [NotMapped]
        public string AvatarReSize { get; set; }
        [NotMapped]
        public string Mobile { get; set; }
        [NotMapped]
        public string Email { get; set; }
        /// <summary>
        /// Tổng buổi học
        /// </summary>
        [NotMapped]
        public int TotalLesson { get; set; }
        /// <summary>
        /// Số buổi học còn lại
        /// </summary>
        [NotMapped]
        public int RemainingLesson { get; set; }
        /// <summary>
        /// Số tháng còn lại
        /// </summary>
        [NotMapped]
        public int RemainingMonth { get; set; }
        /// <summary>
        /// Tổng số tháng
        /// </summary>
        [NotMapped]
        public int TotalMonth { get; set; }
        /// <summary>
        /// true - Đã cấp chứng chỉ
        /// </summary>
        [NotMapped]
        public bool HasCertificate { get; set; }
        /// <summary>
        /// Học viên đã được cập chứng chỉ
        /// true: đã cấp
        /// false: chưa cấp
        /// </summary>
        public tbl_StudentInClass() : base() { }
        public tbl_StudentInClass(object model) : base(model) { }
    }
    public class StudentCertificateModel
    {
        public int? StudentId { get; set; }
        public int? ClassId { get; set; }
        /// <summary>
        /// Tên chứng chỉ
        /// </summary>
        public string CertificateName { get; set; }
        /// <summary>
        /// Tên khóa học
        /// </summary>
        public string CertificateCourse { get; set; }
        /// <summary>
        /// ex: Dear: anh/chị
        /// </summary>
        public string SubTitle { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        public string StudentName { get; set; }
        /// <summary>
        /// Loại chứng chỉ 
        /// </summary>
        public string Type { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
    public class Get_StudentInClass : DomainEntity
    {
        public int? BranchId { get; set; }
        public int? ClassId { get; set; }
        public int? StudentId { get; set; }
        public bool? Warning { get; set; }
        public string WarningContent { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Chính thức
        /// 2 - Học thử
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        /// <summary>
        /// Hóa đơn khi đăng ký mua gói
        /// </summary>
        public int? BillDetailId { get; set; }
        public string ClassName { get; set; }
        public int? ClassType { get; set; }
        public string ClassTypeName { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        /// <summary>
        /// 1 - Thanh toán một lần
        /// 2 - Thanh toán theo tháng
        /// </summary>
        public int PaymentType { get; set; }
        public string PaymentTypeName
        {
            get
            {
                return PaymentType == 1 ? "Thanh toán một lần" : PaymentType == 2 ? "Thanh toán theo tháng" : "";
            }
        }
        /// <summary>
        /// Tổng buổi học
        /// </summary>
        public int TotalLesson { get; set; }
        /// <summary>
        /// Số buổi học còn lại
        /// </summary>
        public int RemainingLesson { get; set; }
        /// <summary>
        /// Số tháng còn lại
        /// </summary>
        public int RemainingMonth { get; set; }
        /// <summary>
        /// Tổng số tháng
        /// </summary>
        public int TotalMonth { get; set; }
        public int TotalRow { get; set; }
    }

    public class Get_StudentInRegis
    {
        public int? StudentId { get; set; }
        public string FullName { get; set; }
        public int? Gender { get; set; }
        public string UserCode { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public string Mobile { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string Email { get; set; }
        public int TotalLesson { get; set; }
        public int TotalRow { get; set; }
    }
    public class Get_UpcomingClass : DomainEntity
    {
        public int? BranchId { get; set; }
        public int? ClassId { get; set; }
        public bool? Warning { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Chính thức
        /// 2 - Học thử
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string ClassName { get; set; }
        public int? ClassType { get; set; }
        public string ClassTypeName { get; set; }
        public DateTime? StartDay { get; set; }
        public DateTime? EndDay { get; set; }
        public double Price { get; set; }
        /// <summary>
        /// Tổng buổi học
        /// </summary>
        public int? TotalLesson { get; set; }
        /// <summary>
        /// Số buổi hoàn thành
        /// </summary>
        public int? LessonCompleted { get; set; }
        public int TotalRow { get; set; }
    }
}