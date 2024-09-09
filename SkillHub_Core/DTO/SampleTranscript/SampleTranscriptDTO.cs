using LMSCore.DTO.Domain;

namespace LMSCore.DTO.SampleTranscript
{
    public class SampleTranscriptDTO : DomainDTO
    {
        public string Name { get; set; }
        public SampleTranscriptDTO() : base() { }
        public SampleTranscriptDTO(object model) : base(model) { }
    }
}
