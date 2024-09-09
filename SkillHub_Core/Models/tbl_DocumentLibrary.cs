using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_DocumentLibrary : DomainEntity
    {
        /// <summary>
        /// Id chủ đề
        /// </summary>
        public int? DirectoryId { get; set; }
        public string Background { get; set; }
        /// <summary>
        /// Link file
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// Link đọc file dưới dạng html5
        /// </summary>
        public string FileUrlRead { get; set; }
        /// <summary>
        /// Tên file
        /// </summary>
        public string FileName{ get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Kiểu file
        /// </summary>
        [NotMapped]
        public string FileType
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(FileUrl))
                        return "";
                    var values = FileUrl.Split('.');
                    if (values.Length == 0)
                        return "";
                    return values[values.Length - 1].ToString();
                }
                catch { return null; }

            }
        }

        public tbl_DocumentLibrary() : base() { }
        public tbl_DocumentLibrary(object model) : base(model) { }
    }


    public class Get_DocumentLibrary : DomainEntity
    {
        public string Background { get; set; }
        /// <summary>
        /// Id chủ đề
        /// </summary>
        public int? DirectoryId { get; set; }
        /// <summary>
        /// Link file
        /// </summary>
        public string FileUrl { get; set; }
        public string FileUrlRead { get; set; }
        public string FileName{ get; set; }
        public string Description { get; set; }
        public int TotalRow { get; set; }
    }
}