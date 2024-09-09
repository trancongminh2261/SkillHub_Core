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
    public class StudentMonthlyDebtService
    {
        public static async Task<tbl_StudentMonthlyDebt> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_StudentMonthlyDebt.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_StudentMonthlyDebt> Insert(StudentMonthlyDebtCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_StudentMonthlyDebt(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_StudentMonthlyDebt.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_StudentMonthlyDebt> Update(StudentMonthlyDebtUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudentMonthlyDebt.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.IsPaymentDone = itemModel.IsPaymentDone;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudentMonthlyDebt.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(StudentMonthlyDebtSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StudentMonthlyDebtSearch();
                string sql = $"Get_StudentMonthlyDebt " +
                    $"@PageIndex = {baseSearch.PageIndex }," +
                    $"@PageSize = {baseSearch.PageSize }," +
                    $"@ClassId = {baseSearch.ClassId ?? 0}," +
                    $"@Month = {baseSearch.Month ?? 0}," +
                    $"@StudentId = {baseSearch.StudentId ?? 0}";
                var data = await db.SqlQuery<Get_StudentMonthlyDebt>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_StudentMonthlyDebt(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}