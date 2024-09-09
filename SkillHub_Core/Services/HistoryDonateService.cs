using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class HistoryDonateService
    {
        public static async Task<tbl_HistoryDonate> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_HistoryDonate.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<AppDomainResult> GetAll(HistoryDonateSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new HistoryDonateSearch();
                string sql = $"Get_HistoryDonate @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Type = {baseSearch.Type}";
                var data = await db.SqlQuery<Get_HistoryDonate>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_HistoryDonate(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}