using System;

namespace LMSCore.Utilities
{
    public class MonaWritelogModel
    {
        public string Domain { get; set; }
        public string Method { get; set; }
        public string EndPoint { get; set; }
        public string Body { get; set; }
        public string Token { get; set; }
        public string Response { get; set; }
        public string HttpCode { get; set; }
        public string CreateOn { get; set; }
        public string UserAgent { get; set; }
    }
}
