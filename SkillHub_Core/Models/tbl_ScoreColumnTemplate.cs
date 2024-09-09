using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    /// <summary>
    /// loại cột
    /// </summary>
    public class tbl_ScoreColumnTemplate : DomainEntity
    {
        //bảng điểm mẫu
        public int ScoreBoardTemplateId { get; set; }
        //tên cột 
        public string Name { get; set; }
        //hệ số
        public int Factor { get; set; }
        //vị trí
        public int Index { get; set; }
        /// <summary>
        /// loại cột
        /// 1 - cột điểm 
        /// 2 - cột điểm trung bình
        /// 3 - cột ghi chú
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public tbl_ScoreColumnTemplate() : base() { }
        public tbl_ScoreColumnTemplate(object model) : base(model) { }

    }
    public class Get_ScoreColumnTemplate : DomainEntity
    {
        public int ScoreBoardTemplateId { get; set; }
        public string Name { get; set; }
        public int Factor { get; set; }
        public int Index { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public int TotalRow { get; set; }

    }
}