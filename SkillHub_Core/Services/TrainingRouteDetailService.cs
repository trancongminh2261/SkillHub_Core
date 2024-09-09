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
    public class TrainingRouteDetailService
    {
        public static async Task<tbl_TrainingRouteDetail> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TrainingRouteDetail.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_TrainingRouteDetail> Insert(TrainingRouteDetailCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var trainingRouteForm = await db.tbl_TrainingRouteForm.SingleOrDefaultAsync(x => x.Id == itemModel.TrainingRouteFormId);
                if (trainingRouteForm == null)
                    throw new Exception("Không tìm thấy danh mục");
                var model = new tbl_TrainingRouteDetail(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                model.Index = await NewIndex(db, model.TrainingRouteFormId.Value);
                db.tbl_TrainingRouteDetail.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<int> NewIndex(lmsDbContext db, int trainingRouteFormId)
        {
            var lastIndex = await db.tbl_TrainingRouteDetail.Where(x => x.TrainingRouteFormId == trainingRouteFormId && x.Enable == true)
                .OrderByDescending(x => x.Index).FirstOrDefaultAsync();
            if (lastIndex == null)
                return 1;
            return lastIndex.Index + 1;
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
                            var form = await db.tbl_TrainingRouteDetail.SingleOrDefaultAsync(x => x.Id == item.Id);
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
        public static async Task<tbl_TrainingRouteDetail> Update(TrainingRouteDetailUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TrainingRouteDetail.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Skill = itemModel.Skill ?? entity.Skill;
                entity.Level = itemModel.Level ?? entity.Level;
                entity.IeltsExamId = itemModel.IeltsExamId ?? entity.IeltsExamId;
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
                var entity = await db.tbl_TrainingRouteDetail.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(TrainingRouteDetailSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TrainingRouteDetailSearch();

                string sql = $"Get_TrainingRouteDetail " +
                    $"@Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@TrainingRouteId = {baseSearch.TrainingRouteId ?? 0},"+
                    $"@StudentId = {baseSearch.StudentId ?? 0},"+
                    $"@TrainingRouteFormId = {baseSearch.TrainingRouteFormId ?? 0}";

                var data = await db.SqlQuery<Get_TrainingRouteDetail>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_TrainingRouteDetail(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}