using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_InputTestResult : DomainEntity
    {
        public int InterviewAppointmentId { get; set; }
        //ứng viên
        public int CurriculumVitaeId { get; set; }
        //người tổ chức
        public int OrganizerId { get; set; }
        public double ListeningScore { get; set; }
        public double ReadingScore { get; set; }
        public double WritingScore { get; set; }
        public double SpeakingScore { get; set; }
        //nếu làm kiểm tra online
        /// <summary>
        /// Đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        public string AttachFile { get; set; }
        /// <summary>
        /// 1 - tại trung tâm
        /// 2 - làm online
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public tbl_InputTestResult() : base() { }
        public tbl_InputTestResult(object model) : base(model) { }
    }
    public class Get_InputTestResult : DomainEntity
    {
        public int InterviewAppointmentId { get; set; }
        public int CurriculumVitaeId { get; set; }
        public int OrganizerId { get; set; }
        public double ListeningScore { get; set; }
        public double ReadingScore { get; set; }
        public double WritingScore { get; set; }
        public double SpeakingScore { get; set; }
        public int? IeltsExamId { get; set; }
        public string AttachFile { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public int TotalRow { get; set; }

    }
}