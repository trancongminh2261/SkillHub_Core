namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static LMSCore.Models.lmsEnum;
    public class tbl_Customer : DomainEntity
    {
        public string Code { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        public int? CustomerStatusId { get; set; }
        public int? ReasonOutId { get; set; }
        [NotMapped]
        public string ReasonOutName { get; set; }
        public string FullName { get; set; }
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int? SaleId { get; set; }
        /// <summary>
        /// Tên phụ huynh
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// Email phụ huynh
        /// </summary>
        public string ParentEmail { get; set; }
        /// <summary>
        /// Số điện thoại phụ huynh
        /// </summary>
        public string ParentMobile { get; set; }
        /// <summary>
        /// Điểm đầu vào
        /// </summary>
        public double? EntryPoint { get; set; }
        /// <summary>
        /// Điểm đầu ra mong muốn
        /// </summary>
        public double? DesiredOutputScore { get; set; }
        /// <summary>
        /// Chương trình muốn học (Nó chính là ProgramId)
        /// </summary>
        public int? DesiredProgram { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        public int? JobId { get; set; }
        /// <summary>
        /// Trung tâm
        /// </summary>
        public int? BranchId { get; set; }
        /// <summary>
        /// Ngày lên lịch lại (Khách hàng hẹn lại hôm khác)
        /// </summary>
        public DateTime? RescheduledDate { get; set; }
        [NotMapped]
        public int? CustomerStatusType { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        [NotMapped]
        public string CustomerStatusName { get; set; }
        [NotMapped]
        public string LearningNeedName { get; set; }
        [NotMapped]
        public string PurposeName { get; set; }
        [NotMapped]
        public string SourceName { get; set; }
        [NotMapped]
        public string SaleName { get; set; }
        [NotMapped]
        public string SaleUserCode { get; set; }
        [NotMapped]
        public string SaleAvatar { get; set; }
        [NotMapped]
        public List<tbl_CustomerHistory> CustomerHistory { get; set; }
        public tbl_Customer() : base() { }
        public tbl_Customer(object model) : base(model) { }
    }
    public class Get_Customer : DomainEntity
    {
        public string Code { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        public int? CustomerStatusId { get; set; }
        public int? CustomerStatusType { get; set; }
        public int? ReasonOutId { get; set; }
        public string ReasonOutName { get; set; }
        public string FullName { get; set; }
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int? SaleId { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        public int? JobId { get; set; }
        /// <summary>
        /// Trung tâm
        /// </summary>
        public int? BranchId { get; set; }
        public string BranchName { get; set; }
        public string CustomerStatusName { get; set; }
        public string SaleName { get; set; }
        public string SaleUserCode { get; set; }
        public string SaleAvatar { get; set; }
        /// <summary>
        /// Ngày lên lịch lại (Khách hàng hẹn lại hôm khác)
        /// </summary>
        public DateTime? RescheduledDate { get; set; }
        /// <summary>
        /// Tên phụ huynh
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// Email phụ huynh
        /// </summary>
        public string ParentEmail { get; set; }
        /// <summary>
        /// Số điện thoại phụ huynh
        /// </summary>
        public string ParentMobile { get; set; }
        /// <summary>
        /// Điểm đầu vào
        /// </summary>
        public double? EntryPoint { get; set; }
        /// <summary>
        /// Điểm đầu ra mong muốn
        /// </summary>
        public double? DesiredOutputScore { get; set; }
        /// <summary>
        /// Chương trình muốn học (Nó chính là ProgramId)
        /// </summary>
        public int? DesiredProgram { get; set; }
        public int TotalRow { get; set; }
    }

    public class CustomerModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public string LearningNeedName { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public string PurposeName { get; set; }
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>
        public string SourceName { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public string SaleName { get; set; }
        /// <summary>
        /// Tên phụ huynh
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// Email phụ huynh
        /// </summary>
        public string ParentEmail { get; set; }
        /// <summary>
        /// Số điện thoại phụ huynh
        /// </summary>
        public string ParentMobile { get; set; }
        /// <summary>
        /// Điểm đầu vào
        /// </summary>
        public string EntryPoint { get; set; }
        /// <summary>
        /// Điểm đầu ra mong muốn
        /// </summary>
        public string DesiredOutputScore { get; set; }
        /// <summary>
        /// Chương trình muốn học
        /// </summary>
        public string DesiredProgram { get; set; }
    }

    public class CustomerVistor : DomainEntity
    {
        public string FullName { get; set; }
        public string Code { get; set; }
        public string BranchName { get; set; }
        public string TypeName { get; set; }
        public DateTime Time { get; set; }
        public string LearningStatusName { get; set; }
        public string TeacherName { get; set; }
        public DateTime CreateOn { get; set; }
        public string CreateBy { get; set; }
    }

    // Dùng để hứng dữ liệu từ DB
    public class Get_CustomerExport
    {
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string SaleName { get; set; }
        public string CustomerStatusName { get; set; }
        public string BranchName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    // Xử lý ngày tháng rồi đưa vào Fetch
    public class CustomerExport
    {
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string SaleName { get; set; }
        public string CustomerStatusName { get; set; }
        public string BranchName { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedOn { get; set; }
    }

}