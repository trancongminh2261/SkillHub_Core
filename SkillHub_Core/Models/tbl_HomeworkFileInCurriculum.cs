using static LMSCore.Models.lmsEnum;

namespace LMSCore.Models
{
    public class tbl_HomeworkFileInCurriculum : DomainEntity
    {
        public int? HomeworkInCurriculumId { get; set; }
        public string File { get; set; }
        /// <summary>
        /// 1 Giao bài tập
        /// 2 Nộp bài tập
        /// </summary>
        public HomeworkFileType Type { get; set; }
        public string TypeName { get; set; }
        public tbl_HomeworkFileInCurriculum() : base() { }
        public tbl_HomeworkFileInCurriculum(object model) : base(model) { }
    }
}
