using LMSCore.DTO.Domain;

namespace LMSCore.DTO.SaveGradesInClass
{
    public class SaveGradesInClassDTO : DomainDTO
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
        /// Id học viên
        /// </summary>
        public int StudentId { get; set; }
        /// <summary>
        /// Họ tên học viên
        /// </summary>
        public string StudentName { get; set; }
        /// <summary>
        /// Mã học viên
        /// </summary>
        public string StudentCode { get; set; }
        /// <summary>
        /// Giá trị lưu
        /// </summary>
        public string Value { get; set; }
        public SaveGradesInClassDTO() : base() { }
        public SaveGradesInClassDTO(object model) : base(model) { }
    }
}
