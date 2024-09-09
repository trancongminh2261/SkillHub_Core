using System;

namespace LMSCore.DTO.ScheduleDTO
{
    public class ScheduleExpectedDTO
    {
        public class Get_Schedule_Expected
        {
            public int? Id { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string ClassName { get; set; }
            public string RoomName { get; set; }
            public double? TeachingFee { get; set; }
            public int TotalRow { get; set; }
        }
    }
}
