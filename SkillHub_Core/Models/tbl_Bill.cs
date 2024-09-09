namespace LMSCore.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_Bill : DomainEntity
    {
        public string Code { get; set; }
        public int StudentId { get; set; }
        public double TotalPrice { get; set; }
        public int? DiscountId { get; set; }
        //public int? LessonDiscountId { get; set; }
        [NotMapped]
        public int? LessonDiscount { get; set; }
        [NotMapped]
        public string LessonDiscountCode { get; set; }
        [NotMapped]
        public string DiscountCode { get; set; }
        /// <summary>
        /// Giảm
        /// </summary>
        public double Reduced { get; set; }
        /// <summary>
        /// Đã trả
        /// </summary>
        public double Paid { get; set; }
        /// <summary>
        /// Nợ
        /// </summary>
        public double Debt { get; set; }
        /// <summary>
        /// Thanh toán bằng tiền bảo lưu
        /// </summary>
        public double UsedMoneyReserve { get; set; }
        /// <summary>
        /// Sử dụng bảo lưu để thanh toán
        /// </summary>
        public int ClassReserveId { get; set; }
        /// <summary>
        /// Ngày hẹn thanh toán
        /// </summary>
        public DateTime? PaymentAppointmentDate { get; set; }
        /// <summary>
        /// Ngày hoàn thành
        /// </summary>
        public DateTime? CompleteDate { get; set; }
        public int BranchId { get; set; }
        /// <summary>
        /// Đăng ký gói học phí
        /// </summary>
        public int TuitionPackageId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Đăng ký học 
        /// 2 - Mua dịch vụ
        /// 3 - Đăng ký lớp dạy kèm
        /// 4 - Tạo thủ công
        /// 5 - Học phí hằng tháng
        /// 6 - Phí chuyển lớp
        /// 7 - Mua gói combo
        /// </summary>
        public int Type { get; set; }
        public static string GetTypeName(int Type)
        {
            return Type == 1 ? "Đăng ký học"
            : Type == 2 ? "Mua dịch vụ"
            : Type == 3 ? "Đăng ký lớp dạy kèm"
            : Type == 4 ? "Tạo thủ công"
            : Type == 5 ? "Học phí hằng tháng"
            : Type == 6 ? "Phí chuyển lớp" : "";
        }
        public string TypeName { get; set; }
        public int PaymentMethodId { get; set; }
        /// <summary>
        /// Ngày hóa đơn được thanh toán
        /// </summary>
        public DateTime? PaymentDate { get; set; }
        /// <summary>
        /// Bill đã được duyệt chưa
        /// </summary>
        public bool? IsApproved { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserEmail { get; set; }
        [NotMapped]
        public string UserPhone { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        [NotMapped]
        public List<ProductModel> Products { get; set; } = new List<ProductModel>();
        [NotMapped]
        public double? DiscountPrice { get; set; }
        [NotMapped]
        public string ComboName { get; set; }
        [NotMapped]
        public DateTime? SDateCombo { get; set; }
        [NotMapped]
        public DateTime? EDateCombo { get; set; }

        /// <summary>
        /// Tổng giá trị combo
        /// </summary>
        public double? TotalPriceCombo { get; set; }

        /// <summary>
        /// Giảm combo
        /// </summary>
        public double? ComboReduced { get; set; }

        public tbl_Bill() : base() { }
        public tbl_Bill(object model) : base(model) { }
    }
    public class ProductModel {
        public string Code { get; set; }
        public string ClassName { get; set; }
        public string GradeName { get; set; }
        public string ProgramName { get; set; }
        public double Price { get; set; }
        public double? PriceMonth { get; set; }
        public int? TotalMonth { get; set; }
    }
    public class Get_Bill : DomainEntity
    {
        public string Code { get; set; }
        public int StudentId { get; set; }
        public double TotalPrice { get; set; }
        public int? DiscountId { get; set; }
        public string DiscountCode { get; set; }
        /// <summary>
        /// Giảm
        /// </summary>
        public double Reduced { get; set; }
        /// <summary>
        /// Đã trả
        /// </summary>
        public double Paid { get; set; }
        /// <summary>
        /// Nợ
        /// </summary>
        public double Debt { get; set; }
        /// <summary>
        /// Thanh toán bằng tiền bảo lưu
        /// </summary>
        public double UsedMoneyReserve { get; set; }
        /// <summary>
        /// Sử dụng bảo lưu để thanh toán
        /// </summary>
        public int ClassReserveId { get; set; }
        /// <summary>
        /// Ngày hẹn thanh toán
        /// </summary>
        public DateTime? PaymentAppointmentDate { get; set; }
        /// <summary>
        /// Ngày hoàn thành
        /// </summary>
        public DateTime? CompleteDate { get; set; }
        public int BranchId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Đăng ký học 
        /// 2 - Mua dịch vụ
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string BranchName { get; set; }
        public double SumTotalPrice { get; set; }
        public double SumPaid { get; set; }
        public double SumDebt { get; set; }
        public double SumReduced { get; set; }
        public int TotalRow { get; set; }
        public int Type_All { get; set; }
        public int Type_Regis { get; set; }
        public int Type_Service { get; set; }
        public int Type_Tutorial { get; set; }
        public int Type_Manual { get; set; }
        public int Type_Monthly { get; set; }
        public int Type_ClassChange { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}