using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMSCore.Areas.Models
{
    public class ThumbnailModel
    {
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string FileUrlRead { get; set; }

    }
}