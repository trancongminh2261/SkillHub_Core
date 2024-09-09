namespace LMSCore.Models
{
    /// <summary>
    /// tiêu chí đánh giá của bài viết writing trong ielts
    /// </summary>
    public class tbl_BandDescriptor : DomainEntity
    {       
        /// <summary>
        /// tên tiêu chí
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// mô tả tiêu chí
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 1 - IELTS Writing task 1
        /// 2 - IELTS Writing task 2
        /// </summary>
        public int TaskOrder { get; set; }
        public tbl_BandDescriptor() : base() { }
        public tbl_BandDescriptor(object model) : base(model) { }
    }
}
