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
    public class TrainingRouteService
    {
        public static async Task<tbl_TrainingRoute> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TrainingRoute.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_TrainingRoute> Insert(TrainingRouteCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_TrainingRoute(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_TrainingRoute.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_TrainingRoute> Update(TrainingRouteUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TrainingRoute.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.CurrentLevel = itemModel.CurrentLevel ?? entity.CurrentLevel;
                entity.TargetLevel = itemModel.TargetLevel ?? entity.TargetLevel;
                entity.Name = itemModel.Name?? entity.TargetLevel;
                entity.Age = itemModel.Age ?? entity.Age;
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
                var entity = await db.tbl_TrainingRoute.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");

                //Kiểm tra dữ liệu
                var trainingRoute = await db.tbl_StudentInTraining.AnyAsync(x => x.TrainingRouteId == id && x.Enable == true);
                if (trainingRoute)
                    throw new Exception($"Đã có học viên tham gia lộ trình {entity.Name}, không thể xóa");
                
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SearchOptions();

                string sql = $"Get_TrainingRoute " +
                    $"@Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}";

                var data = await db.SqlQuery<Get_TrainingRoute>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_TrainingRoute(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        
        public class TrainingRouteFormParam
        { 
            public int TrainingRouteId { get; set; }
        }
        public static async Task<List<TrainingRouteFormDTO>> GetTrainingRouteForm(TrainingRouteFormParam baseSearch, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_TrainingRoute_Detail " +
                       $"@TrainingRouteId = {baseSearch.TrainingRouteId}";
                var data = await db.SqlQuery<Get_TrainingRoute_Detail>(sql);
                var result = new List<TrainingRouteFormDTO>();
                if (!data.Any())
                    return result;
                result = data.GroupBy(f => new
                {
                    f.Id,
                    f.Name,
                    f.Index
                }).Select(f => new TrainingRouteFormDTO
                {
                    Id = f.Key.Id,
                    Index = f.Key.Index,
                    Name = f.Key.Name,
                    Details = f.Where(d=>d.TrainingRouteDetailId.HasValue && d.TrainingRouteDetailEnable == true).OrderBy(d=>d.TrainingRouteDetailIndex).GroupBy(d => new
                    {
                        d.TrainingRouteDetailId,
                        d.TrainingRouteDetailIndex,
                        d.TrainingRouteDetailLevel,
                        d.TrainingRouteDetailSkill,
                        d.IeltsExamId,
                        d.IeltsExamName
                    }).Select(d => new TrainingRouteDetailDTO
                    {
                        Id = d.Key.TrainingRouteDetailId,
                        Skill = d.Key.TrainingRouteDetailSkill,
                        Index = d.Key.TrainingRouteDetailIndex,
                        Level = d.Key.TrainingRouteDetailLevel,
                        IeltsExamId = d.Key.IeltsExamId,
                        IeltsExamName = d.Key.IeltsExamName,
                        Completed =Task.Run(()=> Completed(db,userLog.UserInformationId,d.Key.TrainingRouteDetailId.Value)).Result
                    }).ToList()
                }).ToList();
                return result;
            }
        }
        public static async Task<bool> Completed(lmsDbContext db, int studentId, int trainingRouteDetailId)
        {
            return await db.tbl_IeltsExamResult
                .AnyAsync(x => x.StudentId == studentId && x.ValueId == trainingRouteDetailId && x.Enable == true);
        }
        public static async Task<double> PercentComplete(int studentId, int trainingRouteId)
        {
            using (var db = new lmsDbContext())
            {
                double result = 0;
                var trainingRouteDetailIds = await db.tbl_TrainingRouteDetail
                    .Where(x => x.Enable == true && x.TrainingRouteId == trainingRouteId)
                    .Select(x => x.Id).ToListAsync();
                if (trainingRouteDetailIds.Any())
                {
                    double total = trainingRouteDetailIds.Count();
                    double completed = 0;
                    foreach (var item in trainingRouteDetailIds)
                    {
                        if (await Completed(db, studentId, item))
                            completed++;
                    }
                    result = Math.Round(((completed * 100) / total),2);
                }
                return result;
            }
        }
    }
}