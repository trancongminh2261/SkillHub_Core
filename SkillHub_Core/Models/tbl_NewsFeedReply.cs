using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_NewsFeedReply : DomainEntity
    {
        /// <summary>
        /// Id comment
        /// </summary>
        public int? NewsFeedCommentId { get; set; }
        /// <summary>
        /// Nội dung reply
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Id người reply
        /// </summary>
        public int? CreatedIdBy { get; set; }
        /// <summary>
        /// avatar người reply
        /// </summary>
        [NotMapped]
        public string ReplyAvatar { get; set; }
        /// <summary>
        /// Role người cmt
        /// </summary>
        [NotMapped]
        public string RoleName { get; set; }

        /// <summary>
        /// Tên chức vụ trong nhóm
        /// </summary>
        [NotMapped]
        public string TypeNameGroup { get; set; }

        public tbl_NewsFeedReply() : base() { }
        public tbl_NewsFeedReply(object model) : base(model) { }

    }

    public class Get_NewsFeedReply : DomainEntity
    {
        /// <summary>
        /// Id comment
        /// </summary>
        public int? NewsFeedCommentId { get; set; }
        /// <summary>
        /// Nội dung reply
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Id người reply
        /// </summary>
        public int? CreatedIdBy { get; set; }
        /// <summary>
        /// avatar người reply
        /// </summary>
        public string ReplyAvatar { get; set; }
        /// <summary>
        /// Role người cmt
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Tên chức vụ trong nhóm
        /// </summary>
        public string TypeNameGroup { get; set; }

        public int? TotalRow { get; set; }
    }
}