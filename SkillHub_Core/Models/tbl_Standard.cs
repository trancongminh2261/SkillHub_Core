namespace LMS_Project.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using LMSCore.Models;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Tiêu chuẩn đánh giá bài tự luận
    /// </summary>
    public class tbl_Standard : DomainEntity
    {
        /// <summary>
        /// Tiêu chuẩn
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Điểm
        /// </summary>
        public double Point { get; set; }
        /// <summary>
        /// 1 - Dùng chung
        /// 2 - Dùng riêng
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// khóa học
        /// </summary>
        public int VideoCourseId { get; set; }
        [NotMapped]
        public string VideoCourseName { get; set; }
        public tbl_Standard() : base() { }
        public tbl_Standard(object model) : base(model) { }
    }
}