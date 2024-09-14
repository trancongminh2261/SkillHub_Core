using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMSCore.Areas.Models.ZnsModel
{
    public class ZnsAccessTokenResponse
    {
        public ZnsAuthResponse Data { get; set; }
        public bool Success { get; set; }
    }
}