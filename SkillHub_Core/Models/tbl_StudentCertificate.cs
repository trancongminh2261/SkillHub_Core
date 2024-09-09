namespace LMSCore.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    public class tbl_StudentCertificate : DomainEntity
    {
        public int? StudentId { get; set; }
        public int? ClassId { get; set; }
        /// <summary>
        /// Tên chứng chỉ
        /// </summary>
        public string CertificateName { get; set; }
        /// <summary>
        /// Tên khóa học
        /// </summary>
        public string CertificateCourse { get; set; }
        /// <summary>
        /// ex: Dear: anh/chị
        /// </summary>
        public string SubTitle { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        [NotMapped]
        public string StudentName { get; set; }
        /// <summary>
        /// Loại chứng chỉ 
        /// </summary>
        public string Type { get; set; }
        public tbl_StudentCertificate() : base() { }
        public tbl_StudentCertificate(object model) : base(model) { }
    }
}