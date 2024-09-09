using LMSCore.DTO.Domain;
using LMSCore.Enum;

namespace LMSCore.DTO.ClassTranscriptDetail
{
    public class ClassTranscriptDetailDTO : DomainDTO
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
        public ClassTranscriptDetailDTO() : base() { }
        public ClassTranscriptDetailDTO(object model) : base(model) { }
    }
}
