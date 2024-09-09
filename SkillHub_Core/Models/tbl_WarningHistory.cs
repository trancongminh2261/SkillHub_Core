using System.ComponentModel.DataAnnotations.Schema;

namespace LMSCore.Models
{
    public class tbl_WarningHistory : DomainEntity
    {
        public int StudentId { get; set; }
        public int? ClassId { get; set; }
        [NotMapped]
        public string ClassName { get; set; }
        /// <summary>
        /// phân loại này được bổ sung để làm báo cáo thống kê
        /// 1 - bị cảnh cáo
        /// 2 - được gỡ cảnh cáo
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string Content { get; set; }
        public tbl_WarningHistory() : base() { }
        public tbl_WarningHistory(object model) : base(model) { }
    }
    public class Get_WarningHistory : DomainEntity
    {
        public int StudentId { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string Content { get; set; }
        public int TotalRow { get; set; }
    }
}
