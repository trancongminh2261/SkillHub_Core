using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Services.TranscriptService;

namespace LMSCore.Models
{
    public class tbl_StudentPointRecord : DomainEntity
    {
        public int? ClassId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }

        public int? StudentId { get; set; }
        public int Attend { get; set; }
        public int TotalLessons { get; set; }
        public int Unexcused { get; set; }
        public string Transcript { get; set; }// {StudentId: 197, Reading: '', Listening: '2', Writing: '', Grammar: null, …} theo transcriptId
        ///<summary>
        ///Hạnh kiểm			
        /// </summary>
        public string Behaviour { get; set; } = null;
        ///<summary>
        ///Học lực		
        /// </summary>
        public string AcademicPerformance { get; set; } = null;
        ///<summary>
        ///Ghi chú và đề xuất của giáo viên (nếu có)	
        /// </summary>
        public string Note { get; set; } = null;
        public string PDFUrl { get; set; } = null;
        public tbl_StudentPointRecord() : base() { }
        public tbl_StudentPointRecord(object model) : base(model) { }
    }
    public class Attendance
    {
        public int Attend { get; set; }
        public int TotalLessons { get; set; }
        public int Unexcused { get; set; }
    }
    public class Get_StudentPointRecord : DomainEntity
    {
        public int? ClassId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }

        public int? StudentId { get; set; }
        public string UserCode { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; } 
        public int Attend { get; set; }
        public int TotalLessons { get; set; }
        public int Unexcused { get; set; }
        public string Transcript { get; set; }// {StudentId: 197, Reading: '', Listening: '2', Writing: '', Grammar: null, …} theo transcriptId
        ///<summary>
        ///Hạnh kiểm			
        /// </summary>
        public string Behaviour { get; set; } = null;
        ///<summary>
        ///Học lực		
        /// </summary>
        public string AcademicPerformance { get; set; } = null;
        ///<summary>
        ///Ghi chú và đề xuất của giáo viên (nếu có)	
        /// </summary>
        public string Note { get; set; } = null;
        public string PDFUrl { get; set; } = null;
        public int TotalRow { get; set; }
    }
}