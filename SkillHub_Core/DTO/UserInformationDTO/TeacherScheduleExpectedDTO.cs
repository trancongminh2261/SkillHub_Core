using System;

namespace LMSCore.DTO.UserInformationDTO
{
    public class TeacherScheduleExpectedDTO
    {
        /// <summary>
        /// Model lấy danh sách giáo viên với thông tin số buổi đi dạy dự kiến
        /// </summary>
        public class TeacherScheduleExpected
        {
            public string BranchIds { get; set; }
            /// <summary>
            /// Id của giáo viên
            /// </summary>
            public int? TeacherId { get; set; }
            /// <summary>
            /// Tên
            /// </summary>
            public string TeacherName { get; set; }
            /// <summary>
            /// Mã giáo viên
            /// </summary>
            public string TeacherCode { get; set; }
            /// <summary>
            /// Role giáo viên
            /// </summary>
            public int? RoleId { get; set; }
            /// <summary>
            /// Role giáo viên
            /// </summary>
            public string RoleName { get; set; }
            /// <summary>
            /// Ngày thử việc
            /// </summary>
            //public DateTime? StartWorkingDay { get; set; }
            /// <summary>
            /// Ngày làm việc chính thức
            /// </summary>
            //public DateTime? OfficialWorkingDay { get; set; }
            /// <summary>
            /// Tổng số buổi đã dạy của giáo viên
            /// </summary>
            public int? TotalScheduleAttendance { get; set; }
            /// <summary>
            /// Tổng số buổi dạy của giáo viên
            /// </summary>
            public int? TotalScheduleExpected { get; set; }
            /// <summary>
            /// Lương cứng của giáo viên
            /// </summary>
            public double? SalaryConfig { get; set; }
            /// <summary>
            /// Tổng lương giáo viên nhận chỉ gồm các buổi đi dạy
            /// </summary>
            public double? TotalSalaryAttendance { get; set; }
            /// <summary>
            /// Tổng lương dự kiến của giáo viên bao gồm các buổi không đi dạy
            /// </summary>
            public double? TotalSalaryExpected { get; set; }
            /// <summary>
            /// Lương giáo viên nhận chỉ gồm các buổi đi dạy
            /// </summary>
            public double? SalaryAttendance { get; set; }
            /// <summary>
            /// Lương dự kiến của giáo viên bao gồm các buổi không đi dạy
            /// </summary>
            public double? SalaryExpected { get; set; }
            public int TotalRow { get; set; }
        }
    }
}
