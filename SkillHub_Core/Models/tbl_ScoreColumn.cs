using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    /// <summary>
    /// cột điểm trong bảng điểm theo từng lớp
    /// </summary>
    public class tbl_ScoreColumn : DomainEntity
    {
        //lớp
        public int ClassId { get; set; }
        //tên cột
        public string Name { get; set; }
        //hệ số
        public int Factor { get; set; }
        //vị trí
        public int Index { get; set; }
        /// <summary>
        /// loại cột
        /// 1 - Điểm
        /// 2 - Điểm trung bình
        /// 3 - Ghi chú
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public tbl_ScoreColumn() : base() { }
        public tbl_ScoreColumn(object model) : base(model) { }
    }
    public class Get_ScoreColumn : DomainEntity
    {
        public int ClassId { get; set; }
        public string Name { get; set; }
        public int Factor { get; set; }
        public int Index { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public int TotalRow { get; set; }
    }
}