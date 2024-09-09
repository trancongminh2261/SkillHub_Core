using LMSCore.Enum;

namespace LMSCore.Models
{
    /// <summary>
    /// Bảng cột điểm của bảng điểm mẫu
    /// </summary>
    public class tbl_SampleTranscriptDetail : DomainEntity
    {
        /// <summary>
        /// Id bảng điểm
        /// </summary>
        public int SampleTranscriptId { get; set; }
        /// <summary>
        /// Tên cột điểm
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Loại cột điểm
        /// SampleTranscriptDetailEnum Type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Vị trí cột
        /// </summary>
        public int Index { get; set; }
        public tbl_SampleTranscriptDetail() : base() { }
        public tbl_SampleTranscriptDetail(object model) : base(model) { }
    }
}
