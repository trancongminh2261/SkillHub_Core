using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LMSCore.Models
{
    public class tbl_CheckWritingResponse : DomainEntity
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
        public tbl_CheckWritingResponse() : base() { }
        public tbl_CheckWritingResponse(object model) : base(model) { }
    }
}
