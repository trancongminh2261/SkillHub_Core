using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Models
{
    public class tbl_Feedback : DomainEntity
    {
        /// <summary>
        /// Tiêu đề phản hồi
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Nội dung phản hồi
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Trạng thái phản hồi
        /// 1 Mới gửi
        /// 2 Đang xử lý
        /// 3 Đã xong
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// Tên trạng thái
        /// </summary>
        [NotMapped]
        public string StatusName
        {
            get
            {
                try
                {
                    return Status == (int)FeedbackStatus.MoiGui ? "Mới gửi" :
                        Status == (int)FeedbackStatus.DangXuLy ? "Đang xử lý" :
                        Status == (int)FeedbackStatus.DaXong ? "Đã xong" : string.Empty;
                }
                catch
                {
                    return null;
                }

            }
        }
        /// <summary>
        /// Cờ ưu tiên
        /// </summary>
        public bool? IsPriority { get; set; }
        /// <summary>
        /// Đánh giá sao
        /// </summary>
        public int? StarRating { get; set; }
        /// <summary>
        /// Cờ ẩn danh
        /// </summary>
        public bool? IsIncognito { get; set; }
        /// <summary>
        /// True: Đã rating, False: Chưa rating
        /// </summary>
        public bool? IsRated { get; set; }
        public int? CreatedIdBy { get; set; }
        [NotMapped]
        public string Avatar { get; set; }
        [NotMapped]
        public string AvatarReSize { get; set; }
        public tbl_Feedback() : base() { }
        public tbl_Feedback(object model) : base(model) { }
    }
    public class Get_Feedback : DomainEntity
    {
        /// <summary>
        /// Tiêu đề phản hồi
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Nội dung phản hồi
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Trạng thái phản hồi
        /// 1 Mới gửi
        /// 2 Đang xử lý
        /// 3 Đã xong
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Cờ ưu tiên
        /// </summary>
        public bool? IsPriority { get; set; }
        /// <summary>
        /// Đánh giá sao
        /// </summary>
        public int? StarRating { get; set; }
        /// <summary>
        /// Cờ ẩn danh
        /// </summary>
        public bool? IsIncognito { get; set; }
        public int? CreatedIdBy { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public int TotalRow { get; set; }
    }
}