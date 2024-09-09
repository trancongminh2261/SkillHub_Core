using LMSCore.DTO.HomeworkFileInCurriculumDTO;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.DTO.HomeworkInCurriculumDTO
{
    public class GetHomeworkInCurriculumDTO
    {
        public int Id { get; set; }
        /// <summary>
        /// Id giáo trình
        /// </summary>
        public int? CurriculumId { get; set; }
        /// <summary>
        /// Tên bài tập
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Đề được lấy trong ngân hàng đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        public string IeltsExamName { get; set; }
        /// <summary>
        /// Diễn ra từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Note { get; set; }
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string HomeworkContent { get; set; }
        /// <summary>
        /// Điểm sàn
        /// </summary>
        public double? CutoffScore { get; set; }
        /// <summary>
        /// Số buổi
        /// </summary>
        public int? SessionNumber { get; set; }
        /// <summary>
        /// Vị trí của bài tập
        /// </summary>
        public int? Index { get; set; }
        public int TotalRow { get; set; }
        public List<GetHomeworkFileInCurriculumDTO> Files { get; set; } = new List<GetHomeworkFileInCurriculumDTO>();
    }
}