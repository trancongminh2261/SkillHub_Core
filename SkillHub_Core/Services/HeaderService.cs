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
using LMSCore.DTO.Header;


namespace LMSCore.Services
{
    public class HeaderService
    {
        public static async Task<AppDomainResult> GetFullData(HeaderOptions baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) return new AppDomainResult { TotalRow = 0, Data = null };

                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                if (user.RoleId == (int)RoleEnum.parents)
                    myBranchIds = "";
                string sql = $"Get_Header @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@MyBranchIds= N'{myBranchIds}'," +
                    $"@UserId = {user.UserInformationId}," +
                    $"@UserRoleId = {user.RoleId ?? 0}," +
                    $"@Search = N'{baseSearch.Search ?? ""}'";
                var data = await db.SqlQuery<Get_HeaderDTO>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = data[0].TotalRow;
                var result = data.Select(i => new HeaderDTO(i)).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
        public static async Task<MenuNumberDTO> GetMenuNumber(MenuNumberOptions baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var result = new MenuNumberDTO();
                // Số lượng phản hồi
                result.Feedback = await FeedbackService.GetFeedbackInProcess(baseSearch.BranchIds, user);
                // Số lượng học viên lập kết hoạch mở lớp
                result.ProgramRegistration = await ClassRegistrationService.GetClassRegistrationNumber(baseSearch.BranchIds, user);
                return result;
            }
        }
    }
}