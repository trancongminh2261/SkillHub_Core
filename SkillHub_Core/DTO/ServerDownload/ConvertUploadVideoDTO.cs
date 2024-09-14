using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMS_Project.DTO.ServerDownload
{
    public class ConvertUploadVideoDTO
    {
        /// <summary>
        /// id sau khi upload video
        /// </summary>
        public string VideoUploadId { get; set; }
        /// <summary>
        /// thời lượng của video ( tính bằng giây )
        /// </summary>
        public int Minute { get; set; }
        /// <summary>
        /// ảnh bìa
        /// </summary>
        public string Thumbnail { get; set; }
        public string VideoUrl { get; set; }
    }
}