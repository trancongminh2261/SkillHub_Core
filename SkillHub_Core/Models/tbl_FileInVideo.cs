﻿namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.LMS.AssetCRM;

    public class tbl_FileInVideo : DomainEntity
    {
        public int? LessonVideoId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public tbl_FileInVideo() : base() { }
        public tbl_FileInVideo(object model) : base(model) { }
    }
}