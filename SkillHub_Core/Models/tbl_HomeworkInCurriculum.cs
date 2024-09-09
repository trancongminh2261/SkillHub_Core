using LMSCore.DTO.HomeworkFileInCurriculumDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Models
{
    public class tbl_HomeworkInCurriculum : DomainEntity
    {
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
        /// <summary>
        /// Diễn ra từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - bài tập về nhà
        /// 2 - bài thi
        /// </summary>
        public HomeworkType? Type { get; set; }
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
        [NotMapped]
        public List<GetHomeworkFileInCurriculumDTO> Files { get; set; } = new List<GetHomeworkFileInCurriculumDTO>();
        public tbl_HomeworkInCurriculum() : base() { }
        public tbl_HomeworkInCurriculum(object model) : base(model) { }
    }
}
