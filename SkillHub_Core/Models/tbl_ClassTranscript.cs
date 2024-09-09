using System;

namespace LMSCore.Models
{
    /// <summary>
    /// bảng điểm của lớp
    /// </summary>
    public class tbl_ClassTranscript : DomainEntity
    {
        public int ClassId { get; set; }
        /// <summary>
        /// Dùng bảng điểm mẫu
        /// </summary>
        public int? SampleTranscriptId { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Ngày thi
        /// </summary>
        public DateTime? Date { get; set; }
        public tbl_ClassTranscript() : base() { }
        public tbl_ClassTranscript(object model) : base(model) { }
    }
}
