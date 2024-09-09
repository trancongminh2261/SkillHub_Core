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
using Microsoft.AspNetCore.Mvc;

namespace LMSCore.Services
{
    public class ReasonOutService
    {
        public static async Task<tbl_ReasonOut> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_ReasonOut.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_ReasonOut> Insert(ReasonOutCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_ReasonOut(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_ReasonOut.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_ReasonOut> Update(ReasonOutUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_ReasonOut.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (data == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                data.Name = itemModel.Name ?? data.Name;
                await db.SaveChangesAsync();
                return data;
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_ReasonOut.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SearchOptions();

                string sql = $"Get_ReasonOut @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Search = N'{baseSearch.Search}'";
                var data = await db.SqlQuery<Get_ReasonOut>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_ReasonOut(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<List<ReasonOutDropDown>> GetDropDown()
        {
            using (var db = new lmsDbContext())
            {
                return await db.tbl_ReasonOut.Where(x => x.Enable == true).Select(x => new ReasonOutDropDown()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToListAsync();
            }
        }
    }
}