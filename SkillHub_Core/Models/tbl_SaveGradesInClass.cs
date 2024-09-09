namespace LMSCore.Models
{
    /// <summary>
    /// Bảng lưu điểm theo cột điểm trong bảng điểm của lớp
    /// </summary>
    public class tbl_SaveGradesInClass : DomainEntity
    {
        /// <summary>
        /// Id bảng điểm của lớp
        /// </summary>
        public int ClassTranscriptId { get; set; }
        /// <summary>
        /// Id cột điểm trong bảng điểm lớp
        /// </summary>
        public int ClassTranscriptDetailId { get; set; }
        /// <summary>
        /// Học viên
        /// </summary>
        public int StudentId { get; set; }
        /// <summary>
        /// Giá trị lưu
        /// </summary>
        public string Value { get; set; }
        public tbl_SaveGradesInClass() : base() { }
        public tbl_SaveGradesInClass(object model) : base(model) { }
    }
}
