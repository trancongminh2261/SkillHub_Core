using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_FileCurriculumInClass : DomainEntity
    {
        public int? CurriculumDetailId { get; set; }
        public int? FileCurriculumId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileUrlRead { get; set; }
        public int? Index { get; set; }
        public bool? IsComplete { get; set; }
        public bool IsHide { get; set; }
        public int? ClassId { get; set; }
        public tbl_FileCurriculumInClass() : base() { }
        public tbl_FileCurriculumInClass(object model) : base(model) { }

    }

    public class Get_FileCurriculumInClass :DomainEntity
    {
        public int? CurriculumDetailId { get; set; }
        public int? FileCurriculumId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileType
        {
            get
            {
                if (string.IsNullOrEmpty(FileUrl))
                    return "";
                var values = FileUrl.Split('.');
                if (values.Length == 0)
                    return "";
                return values[values.Length - 1].ToString();
            }
        }
        public string FileSize { get; set; }
        public string FileUrlRead { get; set; }
        public int? Index { get; set; }
        public bool? IsComplete { get; set; }
        public bool IsHide { get; set; }
        public Get_FileCurriculumInClass() : base() { }
        public Get_FileCurriculumInClass(object model) : base(model) { }
    }
}