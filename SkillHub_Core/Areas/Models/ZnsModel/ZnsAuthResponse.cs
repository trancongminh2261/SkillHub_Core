using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMSCore.Areas.Models.ZnsModel
{
    public class ZnsAuthResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public double expires_in { get; set; }
    }
}