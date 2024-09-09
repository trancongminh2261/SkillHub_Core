namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    public class tbl_DoingTest : DomainEntity
    {
        public int IeltsExamId { get; set; }
        public int StudentId { get; set; }
        /// <summary>
        /// HomewworkId
        /// </summary>
        public int ValueId { get; set; }
        /// <summary>
        /// 1 - Làm bài thử
        /// 2 - Làm bài hẹn test 
        /// 3 - Bài tập về nhà
        /// 4 - Bộ đề
        /// 5 - Lộ trình luyên tập
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// Với bài test của lộ trình
        /// </summary>
        public int? TrainingRouteDetailId { get; set; }
        public string TypeName { get; set; }
        /// <summary>
        /// 1 - Đang làm 
        /// 2 - Đã nộp
        /// 3 - Đã hủy
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        public int TimeSpent { get; set; }
        /// <summary>
        /// Thời gian làm bài của từng kỹ năng - lưu json
        /// </summary>
        [JsonIgnore]
        public string TimeSpentOfSkill { get; set; }
        [NotMapped]
        public TimeSpentOfSkillDTO TimeSpentOfSkillDTO { get; set; }
        public tbl_DoingTest() : base() { }
        public tbl_DoingTest(object model) : base(model) { }
    }
    public class TimeSpentOfSkillDTO
    {
        public DateTime Now { get { return DateTime.Now; } }
        public int IeltsSkillId { get; set; }
        public int IeltsSectionId { get; set; }
        public DateTime StartTime { get; set; }
        public int Time { get; set; }
        public double TotalSecondsRemaining
        {
            get
            {
                double result = Math.Round((Time * 60) - DateTime.Now.Subtract(StartTime).TotalSeconds, 0);
                result = result < 0 ? 0 : result;
                return result;
            }
        }
    }
    public class Get_DoingTestOverview
    {
        public int ExamSectionId { get; set; }
        public string ExamSectionName { get; set; }
        public string Audio { get; set; }
        /// <summary>
        /// Nhóm câu
        /// </summary>
        public int ExerciseGroupId { get; set; }
        /// <summary>
        /// vị trí câu hỏi
        /// </summary>
        public int? ExerciseId { get; set; }
        public int Status { get; set; }
    }
    public class DoingTestOverviewModel
    {
        public string ExamName { get; set; }
        public string ExamCode { get; set; }
        public int Time { get; set; }
        public string Audio { get; set; }
        /// <summary>
        /// Sớ lượng câu hỏi
        /// </summary>
        public int NumberExercise { get; set; }
        public DateTime? StartTime { get; set; }
        public List<DoingTestSectionModel> Sections { get; set; }
    }
    public class DoingTestSectionModel
    {
        public int ExamSectionId { get; set; }
        public string ExamSectionName { get; set; }
        public string Audio { get; set; }
        public List<DoingTestSectionItem> Items { get; set; }
    }
    public class DoingTestSectionItem
    {
        /// <summary>
        /// Nhóm câu
        /// </summary>
        public int ExerciseGroupId { get; set; }
        /// <summary>
        /// vị trí câu hỏi
        /// </summary>
        public int? ExerciseId { get; set; }
        /// <summary>
        /// Số thứ tự câu hỏi
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 1 - Chưa làm 
        /// 2 - Hoàn thành
        /// </summary>
        public int Status { get; set; }
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Chưa làm" : Status == 2 ? "Hoàn thành" : "";
            }
        }
    }
}