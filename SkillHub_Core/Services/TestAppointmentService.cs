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
    public class TestAppointmentService
    {
        public static async Task<tbl_TestAppointment> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TestAppointment.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        //public static async Task<tbl_TestAppointment> Insert(TestAppointmentCreate itemModel, tbl_UserInformation user)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        var model = new tbl_TestAppointment(itemModel);
        //        var checkExist = await db.tbl_TestAppointment.AnyAsync(x => x.StudentId == itemModel.StudentId && x.Enable == false && x.Status == 1);
        //        if (checkExist)
        //            throw new Exception("Học viên có bài kiểm tra chưa làm");
        //        var branch = await db.tbl_Branch.AnyAsync(x => x.Id == itemModel.BranchId && x.Enable == true);
        //        if (!branch)
        //            throw new Exception("Không tìm thấy trung tâm");
        //        var student = await db.tbl_UserInformation.AnyAsync(x => x.UserInformationId == itemModel.StudentId && x.Enable == true);
        //        if (!student)
        //            throw new Exception("Không tìm thấy học viên");
        //        if (itemModel.Type == 2)
        //        {
        //            var ieltsExam = await db.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsExamId);
        //            if (ieltsExam == null)
        //                throw new Exception("Không tìm thấy đề thi");
        //        }
        //        model.CreatedBy = model.ModifiedBy = user.FullName;
        //        model.TeacherId = model.TeacherId ?? 0;
        //        db.tbl_TestAppointment.Add(model);
        //        await db.SaveChangesAsync();
        //        // thêm lịch sử
        //        var learningHistoryService = new LearningHistoryService(db);
        //        var branch2 = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == itemModel.BranchId && x.Enable == true);
        //        var branchName = branch2 == null ? "" : branch2.Name;
        //        await learningHistoryService.Insert(new LearningHistoryCreate
        //        {
        //            StudentId = itemModel.StudentId,
        //            Content = $"Hẹn test tại {branchName} vào lúc {itemModel.Time?.ToString("dd/MM/yyyy HH:mm")}"
        //        }, user);
        //        //
        //        return model;
        //    }
        //}
        public static async Task<tbl_TestAppointment> Update(TestAppointmentUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TestAppointment.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.StudentId);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                if (itemModel.IeltsExamId.HasValue && itemModel.IeltsExamId != 0)
                {
                    var ieltsExam = await db.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsExamId);
                    if (ieltsExam == null)
                        throw new Exception("Không tìm thấy đề thi");
                }
                entity.Time = itemModel.Time ?? entity.Time;
                student.LearningStatus = itemModel.LearningStatus ?? student.LearningStatus;
                student.LearningStatusName = itemModel.LearningStatusName ?? student.LearningStatusName;
                entity.TeacherId = itemModel.TeacherId ?? entity.TeacherId;
                entity.Type = itemModel.Type ?? entity.Type;
                entity.TypeName = itemModel.TypeName ?? entity.TypeName;
                entity.ListeningPoint = itemModel.ListeningPoint ?? entity.ListeningPoint;
                entity.SpeakingPoint = itemModel.SpeakingPoint ?? entity.SpeakingPoint;
                entity.ReadingPoint = itemModel.ReadingPoint ?? entity.ReadingPoint;
                entity.WritingPoint = itemModel.WritingPoint ?? entity.WritingPoint;
                if (!string.IsNullOrEmpty(entity.ListeningPoint) && student.LearningStatus == 1)
                {
                    student.LearningStatus = 2;
                    student.LearningStatusName = tbl_UserInformation.GetLearningStatusName(2);
                }
                entity.Tuitionfee = itemModel.Tuitionfee ?? entity.Tuitionfee;
                entity.Vocab = itemModel.Vocab ?? entity.Vocab;
                entity.Note = itemModel.Note ?? entity.Note;
                entity.IeltsExamId = itemModel.IeltsExamId ?? entity.IeltsExamId;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                entity.AttachFile = itemModel.AttachFile;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TestAppointment.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(TestAppointmentSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TestAppointmentSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                int mySaleId = 0;
                if (user.RoleId == ((int)RoleEnum.sale))
                    mySaleId = user.UserInformationId;
                string sql = $"Get_TestAppointment @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'," +
                    $"@LearningStatus = N'{baseSearch.LearningStatus ?? ""}'," +
                    $"@Type = N'{baseSearch.Type ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@MySaleId = N'{mySaleId}'," +
                    $"@StudentId = {(user.RoleId == ((int)RoleEnum.student) ? user.UserInformationId : baseSearch.StudentId ?? 0)}," +
                    $"@TeacherId = {(user.RoleId == ((int)RoleEnum.teacher) ? user.UserInformationId : baseSearch.TeacherId ?? 0)}," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{baseSearch.SortType}'";
                var data = await db.SqlQuery<Get_TestAppointment>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_TestAppointment(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        //đồng bộ dữ liệu ( bùa chú )
        public static async Task SyncData()
        {
            using (var db = new lmsDbContext())
            {
                var listUser = await db.tbl_UserInformation.Where(x => x.Enable == true).ToListAsync();
                var listTestAppointment = await db.tbl_TestAppointment.Where(x => x.Enable == true).ToListAsync();
                if (listTestAppointment.Count > 0)
                {
                    foreach (var test in listTestAppointment)
                    {
                        var user = listUser.SingleOrDefault(x => x.UserInformationId == test.StudentId);
                        if (user != null)
                        {
                            test.CustomerId = user.CustomerId;
                        }

                    }
                }
                await db.SaveChangesAsync();
            }
        }
    }
}