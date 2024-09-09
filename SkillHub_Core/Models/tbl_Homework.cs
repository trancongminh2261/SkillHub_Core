using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using LMSCore.DTO.HomeworkFileDTO;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Models
{
    public class tbl_Homework : DomainEntity
    {
        /// <summary>
        /// id lớp học
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// tên
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// id bộ đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        /// <summary>
        /// thời gian bắt đầu làm bài
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// thời gian kết thúc làm bài
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Giáo viên chấm bài
        /// </summary>
        public int? TeacherId { get; set; }
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
        public List<HomeworkFileDTO> Files { get; set; } = new List<HomeworkFileDTO>();
        public tbl_Homework() : base() { }
        public tbl_Homework(object model) : base(model) { }
    }
    public class Get_Homework : DomainEntity
    {
        /// <summary>
        /// id lớp học
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// tên btvn
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// id bộ đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        /// <summary>
        /// thời gian bắt đầu làm bài
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// thời gian kết thúc làm bài
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Giáo viên chấm bài
        /// </summary>
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; }
        /// <summary>
        /// tên lớp học
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// tên bộ đề
        /// </summary>
        public string IeltsExamName { get; set; }
        /// <summary>
        /// mã bộ đề
        /// </summary>
        public string IeltsExamCode { get; set; }
        [JsonIgnore]
        public int TotalRow { get; set; }
        /// <summary>
        /// điểm của đề
        /// </summary>
        public double? IeltsExamPoint { get; set; }
        /// <summary>
        /// thời gian làm bài của đề
        /// </summary>
        public int? IeltsExamTime { get; set; }
        public DateTime? MyFromDate { get; set; }
        public DateTime? MyToDate { get; set; }
        public int? MyTimeSpent { get; set; }
        public int? MyStatus { get; set; }
        public string MyStatusName { get; set; }
        public double? MyPoint { get; set; }
        /// <summary>
        /// 1 - bài tập về nhà
        /// 2 - bài thi
        /// </summary>
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
        public List<HomeworkFileDTO> Files { get; set; } = new List<HomeworkFileDTO>();

    }
}