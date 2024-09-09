using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    /// <summary>
    /// bảng điểm từng học viên
    /// </summary>
    public class tbl_Score : DomainEntity
    {
        
        //học viên
        public int StudentId { get; set; }
        //lớp
        public int ClassId { get; set; }
        //đợt thi
        public int TranscriptId { get; set; }
        //cột
        public int ScoreColumnId { get; set; }
        //giá trị cột ( điểm số hoặc ghi chú )
        public string Value { get; set; }
        public tbl_Score() : base() { }
        public tbl_Score(object model) : base(model) { }
    }
    public class Get_Score : DomainEntity
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public int TranscriptId { get; set; }
        public int ScoreColumnId { get; set; }
        public string Value { get; set; }
        public int TotalRow { get; set; }
    }
}