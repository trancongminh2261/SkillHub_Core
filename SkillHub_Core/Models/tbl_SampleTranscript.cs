namespace LMSCore.Models
{
    /// <summary>
    /// Bảng điểm mẫu
    /// </summary>
    public class tbl_SampleTranscript : DomainEntity
    {
        /// <summary>
        /// Tên bảng điểm mẫu
        /// </summary>
        public string Name { get; set; }
        public tbl_SampleTranscript() : base() { }
        public tbl_SampleTranscript(object model) : base(model) { }
    }
}
