using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_NewsFeedLike : DomainEntity
    {
        /// <summary>
        /// id Bản tin
        /// </summary>
        public int? NewsFeedId { get; set; }

        /// <summary>
        /// Cờ like
        /// </summary>
        public bool? IsLike { get; set; }


        /// <summary>
        /// Id người like
        /// </summary>
        public int? CreatedIdBy { get; set; }

        /// <summary>
        /// Hình người like
        /// </summary>
        [NotMapped]
        public string LikedAvatar { get; set; }

        public tbl_NewsFeedLike() : base() { }
        public tbl_NewsFeedLike(object model) : base(model) { }
    }

    public class Get_NewsFeedLike : DomainEntity
    {
        /// <summary>
        /// id Bản tin
        /// </summary>
        public int? NewsFeedId { get; set; }

        /// <summary>
        /// Cờ like
        /// </summary>
        public bool? IsLike { get; set; }


        /// <summary>
        /// Id người like
        /// </summary>
        public int? CreatedIdBy { get; set; }

        /// <summary>
        /// Hình người like
        /// </summary>
        public string LikedAvatar { get; set; }
        public int? TotalRow { get; set; }
    }
}