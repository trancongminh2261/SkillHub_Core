using LMSCore.Enum;
using LMSCore.Models;
using System;
using System.Collections.Generic;

namespace LMSCore.DTO
{
    public class StatisticalOfClassDTO
    {
        public class RollUpStatisticalModel
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            public List<RollUpData> RollUpDatas { get; set; } = new List<RollUpData>();
        }
        public class RollUpData
        {
            /// <summary>
            /// ngày bắt đầu
            /// </summary>
            public DateTime? StartTime { get; set; }
            /// <summary>
            /// ngày kết thúc
            /// </summary>
            public DateTime? EndTime { get; set; }

            public List<RollUpDetail> RollUpDetails { get; set; } = new List<RollUpDetail>();
        }
        public class RollUpDetail
        {
            /// <summary>
            /// 1 - có mặt
            /// 2 - vắng có phép
            /// 3 - vắng không phép
            /// 4 - đi muộn
            /// 5 - về sớm
            /// 6 - nghỉ lễ (Hiện tạ chưa có nên sẽ không sử dụng status này)
            /// </summary>
            public int? Status { get; set; }
            public string StatusName { get; set; }
            /// <summary>
            /// tổng số học viên trong lớp
            /// </summary>
            public int TotalRollUp { get; set; } = 0;
            /// <summary>
            /// tổng số điểm danh
            /// </summary>
            public int TotalRollUpByStatus { get; set; } = 0;
        }

        public class HomeworkStatisticalModel
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            public List<HomeworkData> HomeworkDatas { get; set; }
        }
        public class HomeworkData
        {
            /// <summary>
            /// bài tập về nhà
            /// </summary>
            public int HomeworkId { get; set; }
            public string HomeworkName { get; set; }
            /// <summary>
            /// tỷ lệ nội bài
            /// </summary>
            public double HomeworkRate { get; set; } = 0;
            /// <summary>
            /// số bài đã nộp
            /// </summary>
            public int HomeworkSubmit { get; set; } = 0;
            /// <summary>
            /// số bài đã chấm
            /// </summary>
            public int HomeworkGraded { get; set; } = 0;
        }
        public class StudentInClassStatisticalModel
        {
            public List<ScheduleStatisticalModel> ScheduleDatas { get; set; }
            public List<HistoryStatisticalModel> HistoryDatas { get; set; }
            public List<ClassTranscriptModel> ClassTranscriptDatas { get; set; }
        }
        public class ScheduleStatisticalModel
        {
            public DateTime? StudyDate { get; set; }
            public int? Status { get; set; }
            public string StatusName { get; set; }
            public int? LearningStatus { get; set; }
            public string LearningStatusName { get; set; }
            public string Note { get; set; }
        }
        public class HistoryStatisticalModel
        {
            public string HomeworkName { get; set; }
            public DateTime? DoingDate { get; set; }
            public int? TimeSpent { get; set; }
            public double? TotalPoint { get; set; }
            public int? Status { get; set; }
            public string StatusName { get; set; }
        }
        public class ClassTranscriptModel
        {
            public int Id { get; set; }
            /// <summary>
            /// Tên đợt thi
            /// </summary>
            public string Name { get; set; }
            public int? ClassId { get; set; }
            /// <summary>
            /// Đề mẫu
            /// </summary>
            public int? SampleTranscriptId { get; set; }
            /// <summary>
            /// Ngày thi
            /// </summary>
            public DateTime? Date { get; set; }
            /// <summary>
            /// Danh sách điểm của học sinh
            /// </summary>
            public List<ClassTranscriptDetailModel> ClassTranscriptDetailData { get; set; }
        }

        public class ClassTranscriptDetailModel
        {
            public int ClassTranscriptId { get; set; }
            /// <summary>
            /// Tên cột điểm
            /// </summary>
            public string Name { get; set; }
            public string Type { get; set; }
            /// <summary>
            /// Vị trí của cột điểm
            /// </summary>
            public int Index { get; set; }
            public string MaxValue { get; set; }
            public SaveGradesInClassModel SaveGradesInClassData { get; set; }
        }

        public class SaveGradesInClassModel
        {
            public int Id { get; set; }
            public int ClassTranscriptId { get; set; }
            public int ClassTranscriptDetailId { get; set; }
            public int StudentId { get; set; }
            /// <summary>
            /// Điểm của học sinh
            /// </summary>
            public string Value { get; set; }
        }

        public class StatisticalHomeworkKeywordsModel
        {
            public int ClassId { get; set; }
            public int StudentId { get; set; }
            /// <summary>
            /// Danh sách bài tập của học sinh
            /// </summary>
            public List<HomeworkModel> Homeworks { get; set; }
        }

        public class StatisticalTotalHomeworkKeywordsModel
        {
            public int ClassId { get; set; }
            public int StudentId { get; set; }
            /// <summary>
            /// Bảng tổng kết mỗi từ khóa sẽ là bao nhiêu % trong tất cả các bài tập
            /// </summary>
            public TotalDataByTags TotalDataByTags { get; set; }
        }
        public class HomeworkModel
        {
            /// <summary>
            /// bài tập về nhà
            /// </summary>
            public int HomeworkId { get; set; }
            public string HomeworkName { get; set; }
            public double TimeSpent { get; set; }
            public string StartTime { get; set; }
            public List<TagModel> Tags { get; set; }
            public List<TagWrittingAndSpeackingModel> TagWrittingAndSpeacking { get; set; }
        }

        public class TagModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int TotalQuestion { get; set; }
            public int TotalQuestionCorrect { get; set; }
            public int TotalQuestionFail { get; set; }
            public double Percentage { get; set; }
        }

        public class TagWrittingAndSpeackingModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int TotalQuestion { get; set; }
            /// <summary>
            /// Điểm trung bình
            /// </summary>
            public double Average { get; set; }
        }

        public class StatisticalTagModel
        {
            public string Name { get; set; }
            public int TotalQuestion { get; set; }
            public int TotalQuestionCorrect { get; set; }
            public int TotalQuestionFail { get; set; }
            public double Percentage { get; set; }
        }

        public class StatisticalTagWrittingAndSpeackingModel
        {
            public string Name { get; set; }
            public int TotalQuestion { get; set; }
            /// <summary>
            /// Điểm trung bình
            /// </summary>
            public double Average { get; set; }
        }

        public class TotalDataByTags
        {
            public List<StatisticalTagModel> TagModel { get; set; }
            public List<StatisticalTagWrittingAndSpeackingModel> TagWrittingAndSpeacking { get; set; }
        }
    }
}
