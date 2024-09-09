using System;

namespace LMSCore
{
    public struct GetDateTime
    {
        public static DateTime Now
        {
            get { return DateTime.UtcNow.AddHours(7); }
        }
    }
}
