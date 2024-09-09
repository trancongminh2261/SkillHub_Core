using System;
using System.Collections.Generic;

namespace LMSCore.DTO.ClassDTO
{
    public class StudentLearningOutcomesDTO
    {
        public string ParentName { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string Attendance { get; set; }
        public List<HomeworkInClassModel> HomeworkInClassModel { get; set; }
        public List<ScoreInClassModel> ScoreInClassModel { get; set; }
    }

    public class HomeworkInClassModel
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string HomeworkContent { get; set; }
        public string TeacherName { get; set; }
    }

    public class ScoreInClassModel
    {
        /// <summary>
        /// Tên đợt thi
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Danh sách điểm của học sinh
        /// </summary>
        public List<ClassTranscriptDetailModel> ClassTranscriptDetailModel { get; set; }
    }

    public class ClassTranscriptDetailModel
    {
        /// <summary>
        /// Tên cột điểm
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Điểm
        /// </summary>
        public string Value { get; set; }

    }
}
