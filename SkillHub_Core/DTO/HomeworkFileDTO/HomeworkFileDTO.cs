using static LMSCore.Models.lmsEnum;

namespace LMSCore.DTO.HomeworkFileDTO
{
    // Dùng cho getAll Saler bao gồm saler đã bị khóa và bị xóa
    public class HomeworkFileDTO
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
