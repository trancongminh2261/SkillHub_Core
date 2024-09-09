using LMSCore.Enum;

namespace LMSCore.Models
{
    /// <summary>
    /// Bảng lưu cột điểm của bảng điểm lớp
    /// </summary>
    public class tbl_ClassTranscriptDetail : DomainEntity
    {
        /// <summary>
        /// Id bảng điểm
        /// </summary>
        public int ClassTranscriptId { get; set; }
        /// <summary>
        /// Tên cột điểm
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Loại cột điểm
        /// ClassTranscriptDetailEnum Type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Vị trí cột
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Giá trị tối đa - áp dụng cho cột điểm ( Type = Grades )
        /// </summary>
        public string MaxValue { get; set; }
        public tbl_ClassTranscriptDetail() : base() { }
        public tbl_ClassTranscriptDetail(object model) : base(model) { }
    }
}
