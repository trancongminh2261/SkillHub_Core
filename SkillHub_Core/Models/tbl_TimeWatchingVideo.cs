namespace LMS_Project.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using LMSCore.Models;
    
    public class tbl_TimeWatchingVideo : DomainEntity
    {
        public int LessonVideoId { get; set; }
        /// <summary>
        /// Tổng thời gian đã xem
        /// </summary>
        public double TotalSecond { get; set; }
        /// <summary>
        /// Người xem video
        /// </summary>
        public int UserId { get;set; }
        public tbl_TimeWatchingVideo() : base() { }
        public tbl_TimeWatchingVideo(object model) : base(model) { }
    }
}