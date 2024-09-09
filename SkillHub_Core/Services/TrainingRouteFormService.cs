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
    public class TrainingRouteFormService
    {
        public static async Task<tbl_TrainingRouteForm> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TrainingRouteForm.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_TrainingRouteForm> Insert(TrainingRouteFormCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var trainingRoute = await db.tbl_TrainingRoute.SingleOrDefaultAsync(x => x.Id == itemModel.TrainingRouteId);
                if (trainingRoute == null)
                    throw new Exception("Không tìm thấy bài luyện tập"); 
                var model = new tbl_TrainingRouteForm(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                model.Index = await NewIndex(db, model.TrainingRouteId.Value);
                db.tbl_TrainingRouteForm.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<int> NewIndex(lmsDbContext db, int trainingRouteId)
        {
            var lastIndex = await db.tbl_TrainingRouteForm.Where(x => x.TrainingRouteId == trainingRouteId && x.Enable == true)
                .OrderByDescending(x => x.Index).FirstOrDefaultAsync();
            if (lastIndex == null)
                return 1;
            return lastIndex.Index + 1;
        }
        public static async Task<tbl_TrainingRouteForm> Update(TrainingRouteFormUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TrainingRouteForm.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Name = itemModel.Name ?? entity.Name;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task ChangeIndex(ChangeIndexModel itemModel)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!itemModel.Items.Any())
                            throw new Exception("Không tìm thấy dữ liệu");
                        foreach (var item in itemModel.Items)
                        {
                            var form = await db.tbl_TrainingRouteForm.SingleOrDefaultAsync(x => x.Id == item.Id);
                            if (form == null)
                                throw new Exception("Không tìm thấy dữ liệu");
                            form.Index = item.Index;
                            await db.SaveChangesAsync();
                        }
                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TrainingRouteForm.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");

                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(TrainingRouteFormSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TrainingRouteFormSearch();

                string sql = $"Get_TrainingRouteForm " +
                    $"@Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@TrainingRouteId = {baseSearch.TrainingRouteId ?? 0}";

                var data = await db.SqlQuery<Get_TrainingRouteForm>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_TrainingRouteForm(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}