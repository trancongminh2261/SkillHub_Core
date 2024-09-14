using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LMSCore.Users;

namespace LMSCore.Areas.Models.ZnsModel.ZnsTemplate
{
    public class TuitionNoticeTemplate
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
        /// tên lớp học
        /// </summary>
        [Description("Tên khóa học")]
        public string class_name { get; set; }
      
        /// <summary>
        /// chi phí
        /// </summary>
        [Description("Số tiền chuyển khoản")]
        public double cost { get; set; }
        /// <summary>
        /// hạn chót
        /// </summary>
        [Description("Hạn chót")]
        public string end_date { get; set; }

        public TuitionNoticeTemplate(string student_name, string student_code, string class_name, double cost, string end_date)
        {
            this.student_name = student_name;
            this.student_code = student_code;
            this.class_name = class_name;           
            this.cost = cost;
            this.end_date = end_date;
        }
    }
}