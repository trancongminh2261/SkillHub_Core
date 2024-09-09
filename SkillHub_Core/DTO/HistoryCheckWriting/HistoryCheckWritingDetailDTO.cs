using LMSCore.DTO.Domain;
using System.Collections.Generic;

namespace LMSCore.DTO.HistoryCheckWriting
{
    public class HistoryCheckWritingDetailDTO : DomainDTO
    {
        /// <summary>
        /// người gửi request
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// họ và tên
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// mã nhân viên
        /// </summary>
        public string UserCode { get; set; }
        /// <summary>
        /// ảnh đại diện
        /// </summary>
        public string Avatar { get; set; }
        /// <summary>
        /// 1 - IELTS Writing task 1
        /// 2 - IELTS Writing task 2
        /// </summary>
        public int TaskOrder { get; set; }
        /// <summary>
        /// nội dung đề kiểm tra
        /// </summary>
        public string Question { get; set; }
        /// <summary>
        /// nội dung trả lời của học viên
        /// </summary>
        public string Answer { get; set; }
        public List<ResponseDetail> Details { get; set; }
        public HistoryCheckWritingDetailDTO() : base() { }
        public HistoryCheckWritingDetailDTO(object model) : base(model) { }
    }
    public class ResponseDetail
    {
        /// <summary>
        /// id tiêu chí
        /// </summary>
        public int BandDescriptorId { get; set; }
        /// <summary>
        /// tên tiêu chí
        /// </summary>
        public string BandDescriptorName { get; set; }
        /// <summary>
        /// điểm số
        /// </summary>
        public string Score { get; set; }
        /// <summary>
        /// Nội dung mà chatGPT trả ra
        /// </summary>
        public string GPTAnswer { get; set; }
    }
}
