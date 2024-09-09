using static LMSCore.Models.lmsEnum;

namespace LMSCore.DTO.HomeworkFileInCurriculumDTO
{
    public class GetHomeworkFileInCurriculumDTO
    {
        public int Id { get; set; }
        public string File { get; set; }
        /// <summary>
        /// 1 Giao bài tập
        /// 2 Nộp bài tập
        /// </summary>
        public HomeworkFileType Type { get; set; }
        public string TypeName { get; set; }
        public string FileName { get; set; }
    }
}
