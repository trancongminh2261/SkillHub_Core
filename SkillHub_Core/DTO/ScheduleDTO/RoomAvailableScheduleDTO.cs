using System.Collections.Generic;

namespace LMSCore.DTO.ScheduleDTO
{
    public class RoomAvailableScheduleDTO
    {
        public class RoomAvailableScheduleModel
        {
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public List<RoomOfStudyTimeModel> StudyTimes { get; set; }
        }

        public class RoomOfStudyTimeModel
        {
            public int StudyTimeId { get; set; }
            public string StudyTimeName { get; set; }
            public List<RoomOfDayOfWeekModel> DayOfWeek { get; set; }
        }
        public class RoomOfDayOfWeekModel
        {
            public string DateTime { get; set; }
            public bool SeeMore { get; set; }
            /// <summary>
            /// Còn lại
            /// </summary>
            public int Remains { get; set; }
            public List<RoomModel> Teachers { get; set; }
        }

        public class RoomModel
        {
            public int RoomId { get; set; }
            public string RoomCode { get; set; }
            public string RoomName { get; set; }
        }
    }
}
