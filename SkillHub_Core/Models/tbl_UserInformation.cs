using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace LMSCore.Models
{
    public partial class tbl_UserInformation
    {
        [Key]
        public int UserInformationId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
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
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// Trạng thái 
        /// 1 - Khóa 
        /// 2 - Hoạt động
        /// </summary>
        public int? StatusId { get; set; }
        /// <summary>
        /// RoleEnum
        /// </summary>
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        /// <summary>
        /// Tỉnh/ TP
        /// </summary>
        public int? AreaId { get; set; }
        /// <summary>
        /// Quận/ Huyện
        /// </summary>
        public int? DistrictId { get; set; }
        /// <summary>
        /// Phường/ Xã
        /// </summary>
        public int? WardId { get; set; }
        public string OneSignal_DeviceId { get; set; }
        public string Password { get; set; }

        public string KeyForgotPassword { get; set; }

        public DateTime? CreatedDateKeyForgot { get; set; }
        /// <summary>
        /// trung tâm
        /// </summary>
        public string BranchIds { get; set; }
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
        public string LearningStatusName { get; set; }
        public static string GetLearningStatusName(int learningStatus) =>
            learningStatus == 1 ? "Chờ kiểm tra"
            : learningStatus == 2 ? "Đã kiểm tra"
            : learningStatus == 3 ? "Không học"
            : learningStatus == 4 ? "Chờ xếp lớp"
            : learningStatus == 5 ? "Đang học"
            : learningStatus == 6 ? "Học xong" : null;
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int SaleId { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public string Extension { get; set; }
        public bool? Enable { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }
        /// <summary>
        /// id cha
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// nghề nghiệp
        /// </summary>
        public int? JobId { get; set; }
        public string RefreshToken { get; set; }
        /// <summary>
        /// Hạn sử dụng refresh token
        /// </summary>
        public DateTime? RefreshTokenExpires { get; set; }
        public bool IsReceiveMailNotification { get; set; } = true;
        public int? CommissionConfigId { get; set; }
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
        /// <summary>
        /// Thông tin từ khách hàng nào
        /// </summary>
        public int CustomerId { get; set; }
        public tbl_UserInformation() { }
        public tbl_UserInformation(object model)
        {
            foreach (PropertyInfo me in this.GetType().GetProperties())
            {
                foreach (PropertyInfo item in model.GetType().GetProperties())
                {
                    if (me.Name == item.Name)
                    {
                        me.SetValue(this, item.GetValue(model));
                    }
                }
            }
        }
    }
    public class Get_UserInformation
    {
        [Key]
        public int UserInformationId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public DateTime? DOB { get; set; } = DateTime.Now; // ngày sinh
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        public string Mobile { get; set; } // số điện thoại
        public string Email { get; set; }
        public string Address { get; set; }
        public int? StatusId { get; set; } // 1 khóa 0 hoạt động
        public int? RoleId { get; set; } //1 admin 2 giáo viên 3 học viên
        public string RoleName { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public int? AreaId { get; set; } // tình/tp
        public int? DistrictId { get; set; }//quận/huyện
        public int? WardId { get; set; } // phường/xã 
        public string OneSignal_DeviceId { get; set; }
        public string Password { get; set; }

        public string KeyForgotPassword { get; set; }

        public DateTime? CreatedDateKeyForgot { get; set; }
        /// <summary>
        /// trung tâm
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// 1 - Chờ xếp lớp
        /// 2 - Đang học
        /// 3 - Học xong
        /// </summary>
        public int LearningStatus { get; set; }
        public string LearningStatusName { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int SaleId { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public string Extension { get; set; }
        /// <summary>
        /// nghề nghiệp
        /// </summary>
        public int? JobId { get; set; }
        public string JobName { get; set; }
        /// <summary>
        /// id cha
        /// </summary>
        public int? ParentId { get; set; }
        public bool? Enable { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }
        public string SourceName { get; set; }
        public string LearningNeedName { get; set; }
        public string PurposeName { get; set; }
        public string SaleName { get; set; }
        public string AreaName { get; set; }
        public string WardName { get; set; }
        public string DistrictName { get; set; }
        public int TotalRow { get; set; }
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

    public class UserInformationModel
    {
        public int UserInformationId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public DateTime? DOB { get; set; } // ngày sinh
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        public string Mobile { get; set; } // số điện thoại
        public string Email { get; set; }
        public string Address { get; set; }
        public int? StatusId { get; set; } // 1 khóa 0 hoạt động
        public int? RoleId { get; set; } //1 admin 2 giáo viên 3 học viên
        public string RoleName { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public int? AreaId { get; set; } // tình/tp
        public int? DistrictId { get; set; }//quận/huyện
        public int? WardId { get; set; } // phường/xã 
        /// <summary>
        /// trung tâm
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// 1 - Chờ xếp lớp
        /// 2 - Đang học
        /// 3 - Học xong
        /// </summary>
        public int LearningStatus { get; set; }
        public string LearningStatusName { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int SaleId { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public string Extension { get; set; }
        /// <summary>
        /// nghề nghiệp
        /// </summary>
        public int? JobId { get; set; }
        /// <summary>
        /// id cha
        /// </summary>
        public int? ParentId { get; set; }
        public bool? Enable { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }
        public string SourceName { get; set; }
        public string LearningNeedName { get; set; }
        public string PurposeName { get; set; }
        public string SaleName { get; set; }
        public string AreaName { get; set; }
        public string WardName { get; set; }
        public string DistrictName { get; set; }
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
        public UserInformationModel() { }
        public UserInformationModel(object model)
        {
            foreach (PropertyInfo me in this.GetType().GetProperties())
            {
                foreach (PropertyInfo item in model.GetType().GetProperties())
                {
                    if (me.Name == item.Name)
                    {
                        me.SetValue(this, item.GetValue(model));
                    }
                }
            }
        }
    }
    //class nhận data trả ra từ store
    public class UserExport
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }      
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public string LearningStatusName { get; set; }
        public string SaleName { get; set; }
        public string JobName { get; set; }
        public UserExport() { }
        public UserExport(object model)
        {
            foreach (PropertyInfo me in this.GetType().GetProperties())
            {
                foreach (PropertyInfo item in model.GetType().GetProperties())
                {
                    if (me.Name == item.Name)
                    {
                        me.SetValue(this, item.GetValue(model));
                    }
                }
            }
        }
    }
    //export thông tin học viên
    public partial class StudentExport
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }     
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string LearningStatusName { get; set; }
        public string SaleName { get; set; }
        public string JobName { get; set; }
        public StudentExport() { }
        public StudentExport(object model)
        {
            foreach (PropertyInfo me in this.GetType().GetProperties())
            {
                foreach (PropertyInfo item in model.GetType().GetProperties())
                {
                    if (me.Name == item.Name)
                    {
                        me.SetValue(this, item.GetValue(model));
                    }
                }
            }
        }
    }
    //export thông tin nhân viên
    public partial class StaffExport
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }      
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public StaffExport() { }
        public StaffExport(object model)
        {
            foreach (PropertyInfo me in this.GetType().GetProperties())
            {
                foreach (PropertyInfo item in model.GetType().GetProperties())
                {
                    if (me.Name == item.Name)
                    {
                        me.SetValue(this, item.GetValue(model));
                    }
                }
            }
        }
    }

    // Hứng dữ liệu từ export file mẫu học sinh
    public class StudentModelExampleExport
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string DOB { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
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
    }

    // Hứng dữ liệu từ export file mẫu nhân viên
    public class EmployeeModelExampleExport
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string DOB { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }
}
