using System;
using System.Collections;
using System.Collections.Generic;

namespace LMSCore.NotificationConfig
{
    public class NotificationRequest
    {
        public int UserId { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// Id của tính năng đó dành cho việc chuyển trang ở web
        /// Ví dụ: đơn hàng AvailableId = billId
        /// </summary>
        public int? AvailableId { get; set; }
        public Hashtable Token { get; set; }
    }
    public class NotificationWithContentRequest
    {
        public List<int> UserIds { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool PushOneSignal { get; set; }
        public bool SendMail { get; set; }
    }
}
