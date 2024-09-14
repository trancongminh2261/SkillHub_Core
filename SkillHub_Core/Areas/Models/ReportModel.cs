using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;
using System.Windows.Markup;

namespace LMSCore.Areas.Models
{
    public class ReportModel
    {
        //nhận xét sự tăng giảm
        public string Message { get; set; }
        public double Value { get; set; }
    }
    public class PieChartData
    {
        public string Category { get; set; }
        public double Value { get; set; }
    }
    public class MultiLineChartData
    {
        public int Month { get; set; }
        public string Category { get; set; }
        public int Value { get; set; }
    }
    public class LineChartData
    {
        public int Month { get; set; }
        public int Value { get; set; }
    }
    public class LineChartDataPercent
    {
        public int Month { get; set; }
        public double Value { get; set; }
    }
    public class ReportPaymentModel
    {
        public string Category { get; set; }
        //nhận xét sự tăng giảm
        public string Message { get; set; }
        public double Value { get; set; }
    }
    /*public class ReportScoreModel
    {
        public string Category { get; set; }
        //nhận xét sự tăng giảm
        public string ScoreLevel { get; set; }
        public double Value { get; set; }
    }*/

}