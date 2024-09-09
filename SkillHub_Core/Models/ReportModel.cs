using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMSCore.Models
{
    public class StudentReportModel
    {
        /// <summary>
        /// Số lượng khách hàng tiềm năng được tạo
        /// </summary>
        public int numberOfLead { get; set; }
        /// <summary>
        /// Tỷ lệ chuyển đổi từ khách hàng tiềm năng thành học viên đăng ký học (pie chart) 
        /// </summary>
        public List<ChartData> conversionRate { get; set; }
        /// <summary>
        /// Xu hướng đi học của học sinh (hàng ngày, hàng tuần, hằng tháng)  (line chart)
        /// </summary>
        public List<ChartData> studyTrend { get; set; }

        public int totalOfFeedback { get; set; }
        public int feedbackReplied { get; set; }
        /// <summary>
        /// Pie chart cho số sao
        /// </summary>
        public List<ChartData> feedbackRate { get; set; }
        /// <summary>
        /// Số học sinh chuyển lớp
        /// </summary>
        public int numberOfMovedStudent { get; set; }
        /// <summary>
        /// Lớp chuyển đi nhiều nhất
        /// </summary>
        public TransferClassModel movedIn { get; set; }
        /// <summary>
        /// Lớp chuyển vào nhiều nhất
        /// </summary>
        public TransferClassModel movedOut { get; set; }
    }

    public class TransferClassModel
    {
        public string className { get; set; }
        public string teacherName { get; set; }
    }
    public class ChartData
    {
        public string label { get; set; }
        public double? value { get; set; }
    }
}