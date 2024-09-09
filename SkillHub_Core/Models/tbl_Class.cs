namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using static LMSCore.Models.lmsEnum;
    public class tbl_Class : DomainEntity
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public int? BranchId { get; set; }
        public int? GradeId { get; set; }
        public int? ProgramId { get; set; }
        public int? CurriculumId { get; set; }
        /// <summary>
        /// Thời gian trên mỗi buổi học
        /// </summary>
        [NotMapped]
        public int Time { get; set; }
        public DateTime? StartDay { get; set; }
        public DateTime? EndDay { get; set; }
        public double Price { get; set; }
        public bool IsMonthly { get; set; }
        /// <summary>
        /// 1 - Sắp diễn ra
        /// 2 - Đang diễn ra
        /// 3 - Kết thúc
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        public int? AcademicId { get; set; }
        public int? TeacherId { get; set; }
        /// <summary>
        /// 1 - Offline
        /// 2 - Online
        /// 3 - Dạy kèm
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        /// <summary>
        /// Số lượng tối đa
        /// </summary>
        public int? MaxQuantity { get; set; }
        /// <summary>
        /// Mẫu chứng chỉ
        /// </summary>
        public int? CertificateTemplateId { get; set; }
        /// <summary>
        /// Bảng điểm mẫu
        /// </summary>
        public int ScoreboardTemplateId { get; set; }
        /// <summary>
        /// 1 - Thanh toán một lần
        /// 2 - Thanh toán theo tháng
        /// </summary>
        public int PaymentType { get; set; }
        /// <summary>
        /// Ngày dự kiến mở lớp
        /// </summary>
        public DateTime? EstimatedDay { get; set; }
        [NotMapped]
        public string PaymentTypeName
        {
            get
            {
                return PaymentType == 1 ? "Thanh toán một lần" : PaymentType == 2 ? "Thanh toán theo tháng" : "";
            }
        }
        /// <summary>
        /// Số lượng học viên
        /// </summary>
        [NotMapped]
        public int? TotalStudent { get; set; }
        /// <summary>
        /// Tổng buổi học
        /// </summary>
        [NotMapped]
        public int? TotalLesson { get; set; }
        /// <summary>
        /// Số buổi hoàn thành
        /// </summary>
        [NotMapped]
        public int? LessonCompleted { get; set; }
        [NotMapped]
        public string ProgramName { get; set; }
        [NotMapped]
        public string GradeName { get; set; }
        [NotMapped]
        public string CurriculumName { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        [NotMapped]
        public string TeacherName { get; set; }
        [NotMapped]
        public string AcademicName { get; set; }
        [NotMapped]
        public List<GetRoom> Room { get; set; }
        [NotMapped]
        public List<TeacherInClassModel> Teachers { get; set; }
        public tbl_Class() : base() { }
        public tbl_Class(object model) : base(model) { }
    }
    public class GetRoom
    {
        public int RoomId { get; set; }

        public string RoomName { get; set; }
    }
    public class TeacherInClassModel
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string TeacherCode { get; set; }
    }
    public class CountClassByStatus
    {
        public int Upcoming { get; set; } = 0;
        public int Opening { get; set; } = 0;
        public int Closing { get; set; } = 0;
    }
    public class Get_Class : DomainEntity
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public int? BranchId { get; set; }
        public int? GradeId { get; set; }
        public int? ProgramId { get; set; }
        public int? CurriculumId { get; set; }
        public DateTime? StartDay { get; set; }
        public DateTime? EndDay { get; set; }
        public DateTime? EstimatedDay { get; set; }
        public double Price { get; set; }
        public bool IsMonthly { get; set; }
        /// <summary>
        /// 1 - Sắp diễn ra
        /// 2 - Đang diễn ra
        /// 3 - Kết thúc
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        public int? AcademicId { get; set; }
        public int? TeacherId { get; set; }
        /// <summary>
        /// 1 - Offline
        /// 2 - Online
        /// 3 - Dạy kèm
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        /// <summary>
        /// Số lượng tối đa
        /// </summary>
        public int? MaxQuantity { get; set; }
        /// <summary>
        /// Mẫu chứng chỉ
        /// </summary>
        public int? CertificateTemplateId { get; set; }
        /// <summary>
        /// Bảng điểm mẫu
        /// </summary>
        public int ScoreboardTemplateId { get; set; }
        /// <summary>
        /// 1 - Thanh toán một lần
        /// 2 - Thanh toán theo tháng
        /// </summary>
        public int PaymentType { get; set; }
        /// <summary>
        /// Số lượng học viên
        /// </summary>
        [NotMapped]
        public int? TotalStudent { get; set; }
        /// <summary>
        /// Tổng buổi học
        /// </summary>
        [NotMapped]
        public int? TotalLesson { get; set; }
        /// <summary>
        /// Số buổi hoàn thành
        /// </summary>
        [NotMapped]
        public int? LessonCompleted { get; set; }
        public string ProgramName { get; set; }
        public string GradeName { get; set; }
        public string CurriculumName { get; set; }
        public string BranchName { get; set; }
        public string TeacherName { get; set; }
        public string AcademicName { get; set; }
        public int Upcoming { get; set; }
        public int Opening { get; set; }
        public int Closing { get; set; }
        public int TotalRow { get; set; }
    }

    public class Get_ClassGantt : DomainEntity
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public DateTime? StartDay { get; set; }
        public DateTime? EndDay { get; set; }
        public int? Status { get; set; }
        public string StatusName { get; set; }
        public int Upcoming { get; set; }
        public int Opening { get; set; }
        public int Closing { get; set; }
        public int TotalRow { get; set; }
    }

    public class TeacherAvailable : DomainEntity
    {
        public int Id { get; set; }
        public string TeacherName { get; set; }
        public string TeacherCode { get; set; }
    }
}