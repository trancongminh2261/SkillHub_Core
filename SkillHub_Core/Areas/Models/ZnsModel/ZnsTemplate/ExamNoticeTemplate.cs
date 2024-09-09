using System.ComponentModel;

namespace LMSCore.Areas.Models.ZnsModel.ZnsTemplate
{
    public class ExamNoticeTemplate
    {
        /// <summary>
        /// tên học viên
        /// </summary>
        [Description("Tên học viên")]
        public string student_name { get; set; }
        /// <summary>
        /// mã học viên
        /// </summary>
        [Description("Mã học viên")]
        public string student_code { get; set; }
        /// <summary>
        /// tên bài kiểm tra
        /// </summary>
        [Description("Tên bài thi")]
        public string exam_name { get; set; }

        /// <summary>
        /// điểm nghe
        /// </summary>
        [Description("Điểm nghe")]
        public double listening { get; set; }
        /// <summary>
        /// điểm nói
        /// </summary>
        [Description("Điểm nói")]
        public double speaking { get; set; }
        /// <summary>
        /// điểm đọc
        /// </summary>
        [Description("Điểm đọc")]
        public double reading { get; set; }
        /// <summary>
        /// điểm viết
        /// </summary>
        [Description("Điểm viết")]
        public double writing { get; set; }
        [Description("Tổng điểm")]
        public double total_score { get; set; }

        public ExamNoticeTemplate(string student_name, string student_code, string exam_name, double speaking, double listening, double reading, double writing, double total_score)
        {
            this.student_name = student_name;
            this.student_code = student_code;
            this.exam_name = exam_name;
            this.listening = listening;
            this.speaking = speaking;
            this.reading = reading;
            this.writing = writing;
            this.total_score = total_score;
        }
    }
}