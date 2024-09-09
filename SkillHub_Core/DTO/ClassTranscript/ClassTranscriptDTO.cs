using LMSCore.DTO.Domain;
using System;

namespace LMSCore.DTO.ClassTranscript
{
    public class ClassTranscriptDTO : DomainDTO
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
        public ClassTranscriptDTO() : base() { }
        public ClassTranscriptDTO(object model) : base(model) { }
    }
}
