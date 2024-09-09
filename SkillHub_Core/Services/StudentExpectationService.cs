using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;


namespace LMSCore.Services
{
    public class StudentExpectationService
    {
        public static async Task<tbl_StudentExpectation> GetByClassRegistrationId(int classRegistrationId)
        {
            using (var db = new lmsDbContext())
            {
                var student = await db.tbl_ClassRegistration.AnyAsync(x => x.Enable == true && x.Id == classRegistrationId);
                if (!student)
                    throw new Exception("Không tìm thấy thông tin đăng ký học"); 
                var data = await db.tbl_StudentExpectation.SingleOrDefaultAsync(x => x.ClassRegistrationId == classRegistrationId && x.Enable == true);
                return data;
            }
        }
    }
}