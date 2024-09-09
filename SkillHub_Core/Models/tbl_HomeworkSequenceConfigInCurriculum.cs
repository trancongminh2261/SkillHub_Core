namespace LMSCore.Models
{
    public class tbl_HomeworkSequenceConfigInCurriculum : DomainEntity
    {
        public int? CurriculumId { get; set; }
        /// <summary>
        /// Cho phép ràng buộc làm bài tập theo thứ tự
        /// </summary>
        public bool? IsAllow { get; set; }
        public tbl_HomeworkSequenceConfigInCurriculum() : base() { }

        public tbl_HomeworkSequenceConfigInCurriculum(object model) : base(model) { }
    }
}
