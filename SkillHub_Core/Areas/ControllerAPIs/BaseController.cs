using LMSCore.LMS;
using LMSCore.Models;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using LMS_Project.Models;
using LMS_Project.Services;

namespace LMSCore.Areas.ControllerAPIs
{
    [Writelog]
    public class BaseController : ControllerBase
    {      
        [NonAction]
        public string ParseToDate(string value)
        {
            try
            {
                System.DateTime td = System.DateTime.ParseExact(value, "dd/MM/yyyy", null);
                value = td.Date.ToString("yyyy-MM-dd 23:59:59.999");
            }
            catch
            {
                value = null;
            }
            return value;
        }
        [NonAction]
        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
        [NonAction]
        public tbl_UserInformation GetCurrentUser()
        {
            try
            {
                var user = HttpContext.User;
                if(user == null)
                    return null;

                var name = user.Claims.First().Value;
                var username = Encryptor.Decrypt(name);
                var user_information = Task.Run(() => UserInformation.GetById(int.Parse(username))).Result;
                return user_information;
            }
            catch { return null; }

        }
        [NonAction]
        public async Task<tbl_UserInformation> GetCurrentUserAsync()
        {
            try
            {
                var user = HttpContext.User;
                if (user == null)
                    return null;

                var name = user.Claims.First().Value;
                var username = Encryptor.Decrypt(name);
                var user_information = await UserInformation.GetById(int.Parse(username));
                return user_information;
            }
            catch { return null; }

        }
    }
}
