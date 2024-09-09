using LMSCore.DTO.Domain;

namespace LMSCore.DTO.HistoryCheckWriting
{
    public class HistoryCheckWritingDTO : DomainDTO
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
        public HistoryCheckWritingDTO() : base() { }
        public HistoryCheckWritingDTO(object model) : base(model) { }
    }
}
