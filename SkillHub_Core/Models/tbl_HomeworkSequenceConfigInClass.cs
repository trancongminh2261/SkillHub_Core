namespace LMSCore.Models
{
    public class tbl_HomeworkSequenceConfigInClass : DomainEntity
    {
        public int? ClassId { get; set; }
        /// <summary>
        /// Cho phép ràng buộc làm bài tập theo thứ tự
        /// </summary>
        public bool? IsAllow { get; set; }
        public tbl_HomeworkSequenceConfigInClass() : base() { }

        public tbl_HomeworkSequenceConfigInClass(object model) : base(model) { }
    }
}
