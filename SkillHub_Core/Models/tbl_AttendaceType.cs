namespace LMSCore.Models
{
    using System;
    public partial class tbl_AttendaceType : DomainEntity
    {
        /// <summary>
        /// type = 1: điểm danh đầu ngày
        /// type = 2 : điểm danh đầu học giờ
        /// type = 3 : điểm danh cuối học giờ
        /// type = 4 : điểm danh sau giờ
        /// type = 5 : điểm danh cuối ngày
        /// </summary>
        public int? TypeId { get; set; }
        public string TypeName { get; set; }
        public string TimeAttendace { get; set; } // HH:mm
        public tbl_AttendaceType() : base() { }
        public tbl_AttendaceType(object model) : base(model) { }
    }
}
