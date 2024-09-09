namespace LMSCore.Models
{
    public class tbl_HistoryCheckWriting : DomainEntity
    {
        /// <summary>
        /// người gửi request
        /// </summary>
        public int UserId { get; set; }
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
        public tbl_HistoryCheckWriting() : base() { }
        public tbl_HistoryCheckWriting(object model) : base(model) { }
    }
}
