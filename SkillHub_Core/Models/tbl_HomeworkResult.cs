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
    public class tbl_HomeworkResult : DomainEntity
    {
        /// <summary>
        /// id bài tập
        /// </summary>
        public int HomeworkId { get; set; }
        /// <summary>
        /// id học sinh
        /// </summary>
        public int StudentId { get; set; }
        [NotMapped]
        public string StudentName { get; set; }
        /// <summary>
        /// id lớp học
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// 1 - đã chấm điểm
        /// 2 - chưa chấm điểm
        /// </summary>
        public HomeworkResultType? Type { get; set; }
        public string TypeName { get; set; }
        public string Content { get; set; }
        // Chấm bài
        public int? TeacherId { get; set; }
        [NotMapped]
        public string TeacherName { get; set; }
        public double? Point { get; set; } 
        public string TeacherNote { get; set; }
        /// <summary>
        /// True - Đậu
        /// False - Rớt
        /// </summary>
        public bool? IsPassed { get; set; }
        [NotMapped]
        public List<HomeworkFileDTO> Files { get; set; } = new List<HomeworkFileDTO>();
        public tbl_HomeworkResult() : base() { }
        public tbl_HomeworkResult(object model) : base(model) { }
    }
    public class Get_HomeworkResult
    {
        public int Id { get; set; }
        /// <summary>
        /// id bài tập
        /// </summary>
        public int HomeworkId { get; set; }
        /// <summary>
        /// id học sinh
        /// </summary>
        public int StudentId { get; set; }
        [NotMapped]
        public string StudentName { get; set; }
        [NotMapped]
        public string StudentCode { get; set; }
        /// <summary>
        /// id lớp học
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// 1 - đã chấm điểm
        /// 2 - chưa chấm điểm
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string Content { get; set; }
        // Chấm bài
        public int? TeacherId { get; set; }
        [NotMapped]
        public string TeacherName { get; set; }
        public double? Point { get; set; }
        public string TeacherNote { get; set; }
        public List<HomeworkFileDTO> Files { get; set; } = new List<HomeworkFileDTO>();
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        /// <summary>
        /// True - Đậu
        /// False - Rớt
        /// </summary>
        public bool? IsPassed { get; set; }
        [JsonIgnore]
        public int TotalRow { get; set; }

    }
}