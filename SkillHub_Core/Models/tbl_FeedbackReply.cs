using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_FeedbackReply : DomainEntity
    {
        public int? FeedbackId { get; set; }
        /// <summary>
        /// Nội dung
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Cờ ẩn danh
        /// </summary>
        public bool? IsIncognito { get; set; }
        public int? CreatedIdBy { get; set; }
        [NotMapped]
        public string Avatar { get; set; }
        public tbl_FeedbackReply() : base() { }
        public tbl_FeedbackReply(object model) : base(model) { }
    }
    public class Get_FeedbackReply : DomainEntity
    {
        public int? FeedbackId { get; set; }
        /// <summary>
        /// Nội dung
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Cờ ẩn danh
        /// </summary>
        public bool? IsIncognito { get; set; }
        public int? CreatedIdBy { get; set; }
        public string Avatar { get; set; }
        public int? TotalRow { get; set; }
    }
}