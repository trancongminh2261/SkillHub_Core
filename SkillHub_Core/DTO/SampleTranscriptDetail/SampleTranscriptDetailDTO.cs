using LMSCore.DTO.Domain;
using LMSCore.Enum;

namespace LMSCore.DTO.SampleTranscriptDetail
{
    public class SampleTranscriptDetailDTO : DomainDTO
    {
        /// <summary>
        /// Id bảng điểm
        /// </summary>
        public int SampleTranscriptId { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// SampleTranscriptDetailEnum Type
        /// </summary>
        public string Type { get; set; }
        public int Index { get; set; }
        public SampleTranscriptDetailDTO() : base() { }
        public SampleTranscriptDetailDTO(object model) : base(model) { }
    }
}
