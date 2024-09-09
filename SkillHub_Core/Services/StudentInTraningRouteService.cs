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
    public class StudentInTrainingService
    {
        public static async Task<AppDomainResult> GetTrainingDoingTest(TrainingDoingTestSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TrainingDoingTestSearch();

                string sql = $"Get_TrainingRouteDoingTest " +
                    $"@Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@StudentId = {baseSearch.StudentId ?? 0}," +
                    $"@Type = {baseSearch.Type ?? 0}," +
                    $"@Level = {baseSearch.Level ?? 0}," +
                    $"@TrainingRouteId = {baseSearch.TrainingRouteId ?? 0}";

                var data = await db.SqlQuery<Get_TrainingDoingTest>(sql);
                var totalRow = data[0].TotalRow;
                return new AppDomainResult { TotalRow = totalRow, Data = data };
            }
        }
        public static async Task Validate(StudentInTrainingCreate item)
        {
            using (var db = new lmsDbContext())
            {
                var student = await db.tbl_UserInformation.AnyAsync(x => x.Enable == true && x.RoleId == (int)RoleEnum.student && x.UserInformationId == item.StudentId);
                if (!student)
                    throw new Exception("Không tìm thấy thông tin học viên");

                var trainingRoute = await db.tbl_TrainingRoute.AnyAsync(x => x.Enable == true && x.Id == item.TrainingRouteId);
                if (!trainingRoute)
                    throw new Exception("Không tìm thấy thông tin lộ trình");

                var hasTraining = await db.tbl_StudentInTraining.AnyAsync(x => x.Enable == true && x.StudentId== item.StudentId && x.TrainingRouteId == item.TrainingRouteId);
                if (hasTraining)
                    throw new Exception("Học viên đã tham gia lộ trình này");
            }
        }

        public static async Task<tbl_StudentInTraining> Insert(StudentInTrainingCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                await Validate(itemModel);
                var model = new tbl_StudentInTraining(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_StudentInTraining.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudentInTraining.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(StudentInTrainingSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StudentInTrainingSearch();

                string sql = $"Get_StudentInTraining " +
                    $"@Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize},"+
                    $"@StudentId = {baseSearch.StudentId ?? 0},"+
                    $"@TrainingRouteId = {baseSearch.TrainingRouteId ?? 0}";

                var data = await db.SqlQuery<Get_StudentInTraining>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_StudentInTraining(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}