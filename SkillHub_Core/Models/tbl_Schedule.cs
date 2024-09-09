namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using System.Reflection;
    using static LMSCore.Models.lmsEnum;
    public class tbl_Schedule : DomainEntity
    {
        public int? ClassId { get; set; }
        [NotMapped]
        public string ClassName { get; set; }
        public int BranchId { get; set; }
        public int RoomId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TeacherId { get; set; }
        public int TeacherAttendanceId { get; set; }
        [NotMapped]
        public string RoomName { get; set; }
        [NotMapped]
        public string RoomCode { get; set; }
        [NotMapped]
        public string TeacherName { get; set; }
        [NotMapped]
        public string TeacherAvatar{ get; set; }
        [NotMapped]
        public string TeacherCode { get; set; }
        [NotMapped]
        public int TotalRow { get; set; }
        /// <summary>
        /// 1 - Chưa học
        /// 2 - Đã học
        /// </summary>
        [NotMapped]
        public int Status
        {
            get
            {
                int result = 1;
                if (TeacherAttendanceId != 0)
                    result = 2;
                return result;
            }
        }
        [NotMapped]
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Chưa học"
                        : Status == 2 ? "Đã học" : "";
            }
        }
        public bool IsOpenZoom { get; set; }
        public int HostId { get; set; }
        public string ZoomId { get; set; }
        public string ZoomPass { get; set; }
        public string StartUrl { get; set; }
        public string JoinUrl { get; set; }
        public int? ZoomConfigId { get; set; }
        public int? SalaryId { get; set; }
        public double? TeachingFee { get; set; }
        public double? TutorFee { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Mới đặt 
        /// 2 - Hủy
        /// 3 - Đã học 
        /// 4 - Giáo viên vắng mặt
        /// 5 - Sự cố kỹ thuật
        /// 6 - Giáo viên vào trễ
        /// 7 - Học viên vắng mặt
        /// </summary>
        public int StatusTutoring { get; set; }
        public string StatusTutoringName { get; set; }
        /// <summary>
        /// 1 - 5 sao
        /// </summary>
        public int RateTeacher { get; set; }
        public string RateTeacherComment { get; set; }
        /// <summary>
        /// cờ thông báo
        /// </summary>
        [DefaultValue(false)]
        public bool Announced { get; set; }
        /// <summary>
        /// Danh sách trợ giảng
        /// </summary>
        public string TutorIds { get; set; }
        public bool SentNotificationTeacher { get; set; }
        public bool SentNotificationStudent { get; set; }
        public tbl_Schedule() : base() { }
        public tbl_Schedule(object model) : base(model) { }
    }
    public class Get_Schedule : DomainEntity
    {
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public int BranchId { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomCode { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string TeacherAvatar { get; set; }
        public string TeacherCode { get; set; }
        public int TeacherAttendanceId { get; set; }
        public bool IsOpenZoom { get; set; }
        public string ZoomId { get; set; }
        public string ZoomPass { get; set; }
        public string StartUrl { get; set; }
        public string JoinUrl { get; set; }
        public int? ZoomConfigId { get; set; }
        public string Note { get; set; }

        public int? SalaryId { get; set; }
        public double? TeachingFee { get; set; }
        /// <summary>
        /// 1 - Mới đặt 
        /// 2 - Hủy
        /// 3 - Đã học 
        /// 4 - Giáo viên vắng mặt
        /// 5 - Sự cố kỹ thuật
        /// 6 - Giáo viên vào trễ
        /// 7 - Học viên vắng mặt
        /// </summary>
        public int StatusTutoring { get; set; }
        public string StatusTutoringName { get; set; }
        /// <summary>
        /// 1 - 5 sao
        /// </summary>
        public int RateTeacher { get; set; }
        public string RateTeacherComment { get; set; }
        /// <summary>
        /// Danh sách trợ giảng
        /// </summary>
        public string TutorIds { get; set; }

        public int TotalRow { get; set; }
    }
    public class RollUpTeacherModel
    {
        public int ScheduleId { get; set; }
        public int ClassId { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string TeacherCode { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TeacherAttendanceId { get; set; }
        public RollUpTeacherModel() { }
        public RollUpTeacherModel(object model)
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
    public class Get_RollUpTeacher
    {
        public int ScheduleId { get; set; }
        public int ClassId { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string TeacherCode { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TeacherAttendanceId { get; set; }
        public int TotalRow { get; set; }
    }
}