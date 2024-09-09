using System;
using System.Collections.Generic;

namespace LMSCore.DTO.ScheduleDTO
{
    public class TeacherAvailableScheduleDTO
    {
        public class TeacherAvailableScheduleModel
        {
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public List <StudyTimeModel> StudyTimes { get; set; }
        }

        public class StudyTimeModel
        {
            public int StudyTimeId { get; set; }
            public string StudyTimeName { get; set; }
            public List<DayOfWeekModel> DayOfWeek { get; set; }
        }
        public class DayOfWeekModel
        {
            public string DateTime { get; set; }
            public bool SeeMore { get; set; }
            /// <summary>
            /// Còn lại
            /// </summary>
            public int Remains { get; set; }
            public List<TeacherInforModel> Teachers { get; set; }   
        }

        public class TeacherInforModel
        {
            public int TeacherId { get; set; }
            public string TeacherName { get; set; }
            public string TeacheCode { get; set; }
            public string TeacherAvatar { get; set; }
        }
    }
}
