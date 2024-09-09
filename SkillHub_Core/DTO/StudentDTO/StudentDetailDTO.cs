using System.ComponentModel.DataAnnotations;
using System;
using LMSCore.Models;
using OfficeOpenXml;

namespace LMSCore.DTO.StudentDTO
{
    public class StudentDetailDTO
    {
        public int Id { get; set; }
        public StudentInformation Information { get; set; }
        public StudentParents Parents { get; set; }
        public StudentAddress Address { get; set; }
        public StudentAcount Acount { get; set; }
        public StudentDetailDTO()
        {
            Information = new StudentInformation();
            Parents = new StudentParents();
            Address = new StudentAddress();
            Acount = new StudentAcount();
        }
    }
    public class StudentInformation
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? DOB { get; set; }
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        /// <summary>
        /// 1 - Khóa 
        /// 2 - Hoạt động
        /// </summary>
        public int? StatusId { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        /// <summary>
        /// Trung tâm - Học viên chỉ thuộc 1 chi nhánh duy nhất
        /// </summary>
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        /// <summary>
        /// Ngày hoạt động
        /// </summary>
        public DateTime ActiveDate { get; set; }
        /// <summary>
        /// 1 - Chờ kiểm tra
        /// 2 - Đã kiểm tra
        /// 3 - Không học
        /// 4 - Chờ xếp lớp
        /// 5 - Đang học
        /// 6 - Học xong
        /// </summary>
        public int LearningStatus { get; set; }
        public string LearningStatusName { get { return tbl_UserInformation.GetLearningStatusName(LearningStatus); } }
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>
        public int? SourceId { get; set; }
        public string SourceName { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        public string LearningNeedName { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int SaleId { get; set; }
        public string SaleName { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public string PurposeName { get; set; }
        public string Extension { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }
        /// <summary>
        /// nghề nghiệp
        /// </summary>
        public int? JobId { get; set; }
        public string JobName { get; set; }
        /// <summary>
        /// Thông tin từ khách hàng nào
        /// </summary>
        public int CustomerId { get; set; }
    }
    public class StudentParents
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// 1 - Khóa 
        /// 2 - Hoạt động
        /// </summary>
        public int? StatusId { get; set; }
        /// <summary>
        /// Giới thiệu thêm
        /// </summary>
        public string Extension { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
    }
    public class StudentAddress
    {
        public string Address { get; set; }
        public int? AreaId { get; set; }
        public string AreaName { get; set; }
        public int? DistrictId { get; set; }
        public string DistrictName { get; set; }
        public int? WardId { get; set; }
        public string WardName { get; set; }
    }
    public class StudentAcount
    {
        public string UserName { get; set; }
    }
}
