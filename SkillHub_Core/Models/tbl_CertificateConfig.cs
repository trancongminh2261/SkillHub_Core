namespace LMSCore.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    public class tbl_CertificateConfig : DomainEntity
    {
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
        /// <summary>
        /// Loại chứng chỉ 
        /// </summary>
        public string Type { get; set; }
        public tbl_CertificateConfig() : base() { }
        public tbl_CertificateConfig(object model) : base(model) { }
    }
}