using LMSCore.DTO.Domain;

namespace LMSCore.DTO.ChatGPT.CheckWriting
{
    public class CheckWritingReponseDTO : DomainDTO
    {
        /// <summary>
        /// Id của request
        /// </summary>
        public int HistoryCheckWritingId { get; set; }
        /// <summary>
        /// tiêu chí
        /// </summary>
        public int BandDescriptorId { get; set; }
        /// <summary>
        /// điểm số
        /// </summary>
        public string Score { get; set; }
        /// <summary>
        /// Nội dung mà chatGPT trả ra
        /// </summary>
        public string GPTAnswer { get; set; }
        public CheckWritingReponseDTO() : base() { }
        public CheckWritingReponseDTO(object model) : base(model) { }
    }
}
