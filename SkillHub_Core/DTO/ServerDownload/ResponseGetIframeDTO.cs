using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMS_Project.DTO.ServerDownload
{
    public class ResponseGetIframeDTO
    {
        public string link { get; set; }
        public string iframe { get; set; }
    }
}