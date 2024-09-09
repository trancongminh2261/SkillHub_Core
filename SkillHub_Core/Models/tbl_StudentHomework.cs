using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSCore.Models
{
    public class tbl_StudentHomework : DomainEntity
    {
        /// <summary>
        /// id btvn
        /// </summary>
        public int HomeworkId { get; set; }
        /// <summary>
        /// id lớp học
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// id học viên
        /// </summary>
        public int StudentId { get; set; }
        /// <summary>
        /// id bộ đề
        /// </summary>
        public int IeltsExamId { get; set; }
        /// <summary>
        /// trạng thái
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// thời gian học viên bắt đầu làm bài
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// thời gian học viên nộp bài
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Điểm của đề
        /// </summary>
        public double Point { get; set; }
        ///// <summary>
        ///// Điểm của học viên
        ///// </summary>
        //public double MyPoint { get; set; }
        /// <summary>
        /// Tổng thời gian của đề
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        public int TimeSpent { get; set; }
        public tbl_StudentHomework() : base() { }
        public tbl_StudentHomework(object model) : base(model) { }

       
    }
   public class Get_StudentHomework
    {
        /// <summary>
        /// id btvn
        /// </summary>
        public int? HomeworkId { get; set; }
        /// <summary>
        /// id lớp học
        /// </summary>
        public int? ClassId { get; set; }
        /// <summary>
        /// id học viên
        /// </summary>
        public int? StudentId { get; set; }
        /// <summary>
        /// id bộ đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        /// <summary>
        /// trạng thái

        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// thời gian học viên bắt đầu làm bài
        /// </summary>
        public DateTime? MyFromDate { get; set; }
        /// <summary>
        /// thời gian học viên nộp bài
        /// </summary>
        public DateTime? MyToDate { get; set; }

        /// <summary>
        /// Điểm của đề
        /// </summary>
        public double? Point { get; set; }
        /// <summary>
        /// Điểm của học viên
        /// </summary>
        public double? MyPoint { get; set; }
        /// <summary>
        /// Tổng thời gian của đề
        /// </summary>
        public int? Time { get; set; }
        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        public int? MyTimeSpent { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string StudentEmail { get; set; }
        public string StudentMobile { get; set; }
        public int TotalRow { get; set; }
    }
}