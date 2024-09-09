using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_NewsFeedComment : DomainEntity
    {
        /// <summary>
        /// id Bản tin
        /// </summary>    
        public int? NewsFeedId { get; set; }
        /// <summary>
        /// Nội dung bình luận
        /// </summary>     
        public string Content { get; set; }       
        /// <summary>
        /// Số lượng reply
        /// </summary>
        public int? TotalReply { get; set; }
        /// <summary>
        /// Id người bình luận
        /// </summary>
        public int? CreatedIdBy { get; set; }
        /// <summary>
        /// Hình người bình luận
        /// </summary>
        [NotMapped]
        public string CommentedAvatar { get; set; }
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

        public tbl_NewsFeedComment() : base() { }
        public tbl_NewsFeedComment(object model) : base(model) { }
    }
    public class Get_NewsFeedComment : DomainEntity
    {
        /// <summary>
        /// id Bản tin
        /// </summary>    
        public int? NewsFeedId { get; set; }
        /// <summary>
        /// Nội dung bình luận
        /// </summary>     
        public string Content { get; set; }
        /// <summary>
        /// Số lượng reply
        /// </summary>
        public int? TotalReply { get; set; }
        /// <summary>
        /// Id người bình luận
        /// </summary>
        public int? CreatedIdBy { get; set; }
        /// <summary>
        /// Hình người bình luận
        /// </summary>
        public string CommentedAvatar { get; set; }
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