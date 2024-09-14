using System;
using System.Collections.Generic;
using System.Linq;
using LMSCore.Users;

namespace LMSCore.Areas.Models
{
    public class UserOption
    {
        public int UserInformationId { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
    }
}