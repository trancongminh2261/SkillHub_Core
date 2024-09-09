using System.ComponentModel.DataAnnotations;
using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Hangfire.Annotations;
using Org.BouncyCastle.Pkcs;

namespace LMSCore.DTO.StaffDTO
{
    public class StaffDetailDTO
    {
        public int Id { get; set; }
        public StaffInformation Information { get; set; }
        public StaffAddress Address { get; set; }
        public StaffBank Bank { get; set; }
        public StaffAccount Account { get; set; }
    }
    public class StaffInformation
    {
        public string FullName { get; set; }
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? DOB { get; set; }
        /// <summary>
        /// Giới tính
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        /// <summary>
        /// Mã nhân viên
        /// </summary>
        public string UserCode { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? StatusId { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        /// <summary>
        /// Giới thiệu thêm
        /// </summary>
        public string Extension { get; set; }
        public string BranchIds { get; set; }
        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }
    }
    public class StaffAddress
    {
        public string Address { get; set; }
        public int? AreaId { get; set; }
        public string AreaName { get; set; }
        public int? DistrictId { get; set; }
        public string DistrictName { get; set; }
        public int? WardId { get; set; }
        public string WardName { get; set; }
    }
    public class StaffBank
    {
        /// <summary>
        /// Số tài khoản ngân hàng
        /// </summary>
        public string BankAccountNumber { get; set; }
        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        public string BankAccountName { get; set; }
        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// Tên chi nhánh
        /// </summary>
        public string BankBranch { get; set; }
    }
    public class StaffAccount
    { 
        public string UserName { get; set; }
    }
}
