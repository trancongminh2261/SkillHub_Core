using System.Reflection.Emit;

namespace LMSCore.NotificationConfig
{
    public class NotificationConfigModel
    {
        public string Code { get; set; }
        public int Type { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string OnesignalTitle { get; set; }
        public string OnesignalContent { get; set; }
        public string EmailTitle { get; set; }
        public string EmailContentFileName { get; set; }
        public string Url { get; set; }
        public int Category { get; set; }
    }
}
