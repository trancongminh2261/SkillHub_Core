using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_DocumentLibraryDirectory : DomainEntity
    {
        /// <summary>
        /// Tên chủ đề
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Tổng số lượng tài liệu
        /// </summary>
        public int TotalDocument { get; set; }
        public tbl_DocumentLibraryDirectory() : base() { }
        public tbl_DocumentLibraryDirectory(object model) : base(model) { }
    }
    public class Get_DocumentLibraryDirectory : DomainEntity
    {
        /// <summary>
        /// Tên chủ đề
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Tổng số lượng tài liệu
        /// </summary>
        public int TotalDocument { get; set; }
    }
}