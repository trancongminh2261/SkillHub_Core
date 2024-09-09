namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_TestAppointment : DomainEntity
    {
        public int? CustomerId { get; set; }
        public int? BranchId { get; set; }
        public int? StudentId { get; set; }
        public DateTime? Time { get; set; }
        public int? TeacherId { get; set; }
        /// <summary>
        /// 1 - Tại trung tâm
        /// 2 - Làm bài trực tuyến
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string ListeningPoint { get; set; }
        public string SpeakingPoint { get; set; }
        public string ReadingPoint { get; set; }
        public string WritingPoint { get; set; }
        public string Vocab { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Học phí tối đa, dùng để tư vấn khóa học
        /// </summary>
        public string Tuitionfee { get; set; }
        /// <summary>
        /// Đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        public string AttachFile { get; set; }
        [NotMapped]
        public string IeltsExamName { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        [NotMapped]
        public string TeacherName { get; set; }
        [NotMapped]
        public int SaleId { get; set; }
        [NotMapped]
        public string SaleName { get; set; }
        /// <summary>
        /// 1 - Chờ kiểm tra
        /// 2 - Đã kiểm tra
        /// 3 - Không học
        /// 4 - Chờ xếp lớp
        /// </summary>
        [NotMapped]
        public int LearningStatus { get; set; }
        [NotMapped]
        public string LearningStatusName { get; set; }
        public tbl_TestAppointment() : base() { }
        public tbl_TestAppointment(object model) : base(model) { }
    }

    public class SimpleTestResult
    {
        public int? StudentId { get; set; }
        public DateTime? Time { get; set; }
        public int? TeacherId { get; set; }
        /// <summary>
        /// 1 - Tại trung tâm
        /// 2 - Làm bài trực tuyến
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string ListeningPoint { get; set; }
        public string SpeakingPoint { get; set; }
        public string ReadingPoint { get; set; }
        public string WritingPoint { get; set; }
        public string Vocab { get; set; }
        public string Note { get; set; }
    }

    public class Get_TestAppointment : DomainEntity
    {
        public int? CustomerId { get; set; }
        public int? BranchId { get; set; }
        public int? StudentId { get; set; }
        public DateTime? Time { get; set; }
        /// <summary>
        /// 1 - Chờ kiểm tra
        /// 2 - Đã kiểm tra
        /// 3 - Không học
        /// 4 - Chờ xếp lớp
        /// </summary>
        public int LearningStatus { get; set; }
        public string LearningStatusName { get; set; }
        public int? TeacherId { get; set; }
        /// <summary>
        /// 1 - Tại trung tâm
        /// 2 - Làm bài trực tuyến
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string ListeningPoint { get; set; }
        public string SpeakingPoint { get; set; }
        public string ReadingPoint { get; set; }
        public string WritingPoint { get; set; }
        public string Vocab { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Học phí tối đa, dùng để tư vấn khóa học
        /// </summary>
        public string Tuitionfee { get; set; }
        /// <summary>
        /// Đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        public string IeltsExamName { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string BranchName { get; set; }
        public string TeacherName { get; set; }
        public int SaleId { get; set; }
        public string SaleName { get; set; }
        public string AttachFile { get; set; }
        public int TotalRow { get; set; }

    }
}