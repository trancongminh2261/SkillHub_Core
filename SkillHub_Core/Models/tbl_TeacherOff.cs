namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_TeacherOff : DomainEntity
    {
        public int? TeacherId { get; set; }

        /// <summary>
        /// thời gian bắt đầu
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// thời gian kết thúc
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// lý do nghỉ
        /// </summary>
        [StringLength(1000)]
        public string Reason { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Duyệt
        /// 3 - Không duyệt
        /// </summary>
        public int? Status { get; set; }
        [StringLength(20)]
        public string StatusName { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        public tbl_TeacherOff() : base() { }
        public tbl_TeacherOff(object model) : base(model) { }
    }
    public class Get_TeacherOff : DomainEntity
    {
        public int? TeacherId { get; set; }

        /// <summary>
        /// thời gian bắt đầu
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// thời gian kết thúc
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// lý do nghỉ
        /// </summary>
        [StringLength(1000)]
        public string Reason { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Duyệt
        /// 3 - Không duyệt
        /// </summary>
        public int? Status { get; set; }
        [StringLength(20)]
        public string StatusName { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int TotalRow { get; set; }
    }
}