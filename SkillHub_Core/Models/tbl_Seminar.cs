namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_Seminar : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? VideoCourseId { get; set; }
        public int LeaderId { get; set; }
        /// <summary>
        /// 1 - Chưa diễn ra
        /// 2 - Đang diễn ra
        /// 3 - Kết thúc
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// Số lượng người tham gia
        /// </summary>
        public int? Member { get; set; }
        public string Thumbnail { get; set; }
        public tbl_Seminar() : base() { }
        public tbl_Seminar(object model) : base(model) { }
    }
    public class Get_Seminar : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? VideoCourseId { get; set; }
        public string VideoCourseName { get; set; }
        public int LeaderId { get; set; }
        public string LeaderName { get; set; }
        /// <summary>
        /// 1 - Chưa diễn ra
        /// 2 - Đang diễn ra
        /// 3 - Kết thúc
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// Số lượng người tham gia
        /// </summary>
        public int? Member { get; set; }
        public string Thumbnail { get; set; }
        public string RoomId { get; set; }
        public string RoomPass { get; set; }
        public string SignatureTeacher { get; set; }
        public string SignatureStudent { get; set; }
        public string APIKey { get; set; }
        public int TotalRow { get; set; }
    }
    public class SeminarModel : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? VideoCourseId { get; set; }
        public string VideoCourseName { get; set; }
        public int LeaderId { get; set; }
        public string LeaderName { get; set; }
        public int? Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// Số lượng người tham gia
        /// </summary>
        public int? Member { get; set; }
        public string Thumbnail { get; set; }
        public string RoomId { get; set; }
        public string RoomPass { get; set; }
        public string SignatureTeacher { get; set; }
        public string SignatureStudent { get; set; }
        public string APIKey { get; set; }
        public int Button
        {
            get
            {
                int result = 0;
                if (DateTime.Now.AddMinutes(15) > StartTime)//Cho tạo phòng trước 15p
                {
                    result = Status.Value;
                    if (DateTime.Now > EndTime.Value.AddMinutes(15))
                        result = 3;
                }
                if (Status.Value == 2) return 2;
                return result;
            }
        }
        public string ButtonName { get {
                return Button == 1 ? "Bắt đầu" : Button == 2 ? "Vào phòng" : Button == 3 ? "Kết thúc" : "";
            } 
        }
        public SeminarModel() : base() { }
        public SeminarModel(object model) : base(model) { }
    }
}