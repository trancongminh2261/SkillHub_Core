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
    public class VideoActiveCodeService
    {
        public static async Task<tbl_VideoActiveCode> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_VideoActiveCode.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<AppDomainResult> GetAll(VideoActiveCodeSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new VideoActiveCodeSearch();
                if (user.RoleId == ((int)lmsEnum.RoleEnum.student))
                    baseSearch.StudentId = user.UserInformationId;

                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;

                string sql = $"Get_VideoActiveCode @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BillDetailId = N'{baseSearch.BillDetailId}'," +
                    $"@MyBranchIds = N'{myBranchIds}'," +
                    $"@StudentId = N'{baseSearch.StudentId}'";
                var data = await db.SqlQuery<Get_VideoActiveCode>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_VideoActiveCode(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

    }
}