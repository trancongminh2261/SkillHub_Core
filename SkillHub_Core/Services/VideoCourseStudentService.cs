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
    public class VideoCourseStudentService
    {
        public class ActiveVideoCourseModel
        { 
            public int videoCourseId { get; set; }
            public string ActiveCode { get; set; }
        }
        public static async Task ActiveVideoCourse(ActiveVideoCourseModel itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var videoCourse = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == itemModel.videoCourseId);
                    if (videoCourse == null)
                        throw new Exception("Không tìm thấy khoá học");
                    if (videoCourse.Type != 1)
                        throw new Exception("Không tìm thấy khóa học");
                    if (videoCourse.BeforeCourseId != 0)
                    {
                        var beforeCourseId = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == videoCourse.BeforeCourseId);
                        var validateBefore = await db.tbl_VideoCourseStudent
                            .AnyAsync(x => x.VideoCourseId == itemModel.videoCourseId && x.UserId == user.UserInformationId && x.Enable == true);
                        if (!validateBefore)
                            throw new Exception($"Bạn chưa học khoá {beforeCourseId.Name}");
                    }

                    if (itemModel.ActiveCode != "CodeFree" && videoCourse.Price > 0)
                    {
                        var videoActiveCode = await db.tbl_VideoActiveCode.FirstOrDefaultAsync(x => x.ActiveCode == itemModel.ActiveCode && x.Enable == true);
                        if (videoActiveCode == null)
                            throw new Exception("Không tìm thấy mã kích hoạt");
                        if(videoActiveCode.IsUsed)
                            throw new Exception("Mã kích hoạt đã được sử dụng");
                        if (videoActiveCode.ProductId != videoCourse.Id)
                            throw new Exception("Mã kích hoạt không phù hợp");
                        videoActiveCode.IsUsed = true; //Xác nhận đã dùng
                    }

                    var validate = await db.tbl_VideoCourseStudent
                        .AnyAsync(x => x.VideoCourseId == itemModel.videoCourseId && x.UserId == user.UserInformationId);
                    if (validate)
                        throw new Exception("Bạn đã kích hoạt khoá học này");

                    var model = new tbl_VideoCourseStudent
                    {
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        MyRate = 0,
                        UserId = user.UserInformationId,
                        VideoCourseId = itemModel.videoCourseId
                    };
                    db.tbl_VideoCourseStudent.Add(model);
                    videoCourse.TotalStudent += 1;
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static async Task<tbl_VideoCourseStudent> Insert(VideoCourseStudentCreate itemModel ,tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var videoCourse = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == itemModel.VideoCourseId);
                if (videoCourse == null)
                    throw new Exception("Không tìm thấy khoá học");
                if (videoCourse.Type != 1)
                    throw new Exception("Không tìm thấy khóa học");

                var validate = await db.tbl_VideoCourseStudent
                       .AnyAsync(x => x.VideoCourseId == itemModel.VideoCourseId && x.UserId == itemModel.UserId);
                if (validate)
                    throw new Exception("Đã học khoá này");

                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.UserId);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");

                var model = new tbl_VideoCourseStudent
                {
                    CreatedBy = userLog.FullName,
                    CreatedOn = DateTime.Now,
                    Enable = true,
                    ModifiedBy = userLog.FullName,
                    ModifiedOn = DateTime.Now,
                    MyRate = 0,
                    UserId = itemModel.UserId,
                    VideoCourseId = itemModel.VideoCourseId
                };
                db.tbl_VideoCourseStudent.Add(model);
                videoCourse.TotalStudent += 1;

                db.tbl_HistoryDonate.Add(new tbl_HistoryDonate
                {
                    CreateById = userLog.UserInformationId,
                    CreatedBy = userLog.FullName,
                    CreatedOn = DateTime.Now,
                    Enable = true,
                    ModifiedBy = userLog.FullName,
                    ModifiedOn = DateTime.Now,
                    Type = 1,
                    TypeName = "Tặng khóa video",
                    UserId = itemModel.UserId ?? 0,
                    Note = $"{userLog.FullName} đã tặng khóa {videoCourse.Name} cho học viên {student.FullName}"
                });

                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<AppDomainResult> GetStudentInVideoCourse(StudentInVideoCourseSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) return new AppDomainResult { TotalRow = 0, Data = null };
                DateTime fromDate = DateTime.ParseExact("2000/01/01", "yyyy/MM/dd", null);
                DateTime toDate = DateTime.ParseExact("3000/01/01", "yyyy/MM/dd", null);
                if (baseSearch.FromDate.HasValue)
                {
                    fromDate = baseSearch.FromDate.Value;
                }
                if (baseSearch.ToDate.HasValue)
                {
                    toDate = baseSearch.ToDate.Value.AddDays(1).AddMinutes(-1);
                }
                string sql = $"Get_StudentInVideoCourse @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@VideoCourseId = {baseSearch.VideoCourseId}," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@FromDate = '{fromDate.ToString("MM/dd/yyyy HH:mm:ss")}'," +
                    $"@ToDate = '{toDate.ToString("MM/dd/yyyy HH:mm:ss")}'";
                var data = await db.SqlQuery<Get_StudentInVideoCourse>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new StudentInVideoCourseModel(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<AppDomainResult> GetAll(VideoCourseStudentSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) return new AppDomainResult { TotalRow = 0, Data = null };
                string sql = $"Get_VideoCourseStudent @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Name = N'{baseSearch.Name ?? ""}'," +
                    $"@Stags = N'{baseSearch.Stags ?? ""}'," +
                    $"@UserId = {user.UserInformationId}," +
                    $"@Sort = {baseSearch.Sort}," +
                    $"@SortType = {(baseSearch.SortType == false ? 0 : 1)}";
                var data = await db.SqlQuery<Get_VideoCourseStudent>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_VideoCourseStudent(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public class AddRateModel
        { 
            public int VideoCourseId { get; set; }
            public int MyRate { get; set; }
            public string RateComment { get; set; }
        }
        public static async Task AddRate(AddRateModel model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var videoCourse = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == model.VideoCourseId);
                        var videoCourseStudent = await db.tbl_VideoCourseStudent
                            .Where(x => x.VideoCourseId == model.VideoCourseId && x.UserId == user.UserInformationId).FirstOrDefaultAsync();
                        if (videoCourseStudent == null)
                            throw new Exception("Bạn chưa học khoá này");
                        if (model.MyRate > 5)
                            throw new Exception("Đánh giá không phù hợp");


                        videoCourseStudent.MyRate = model.MyRate;
                        videoCourseStudent.RateComment = model.RateComment ?? "";
                        videoCourseStudent.ModifiedOn = DateTime.Now;
                        videoCourseStudent.ModifiedBy = user.FullName;
                        await db.SaveChangesAsync();
                        var videoCourseStudents = await db.tbl_VideoCourseStudent.Where(x => x.VideoCourseId == model.VideoCourseId && x.MyRate != 0).ToListAsync();
                        if (videoCourseStudents.Any())
                        {
                            videoCourse.TotalRate = (double)(videoCourseStudents.Select(x => (double)x.MyRate.Value).Sum() / videoCourseStudents.Count());
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
        public class RateModel
        {
            public int VideoCourseId { get; set; }
            public int? UserId { get; set; }
            public string Avatar { get; set; }
            public string FullName { get; set; }
            public double MyRate { get; set; }
            public string RateComment { get; set; }
            public DateTime? CreatedOn { get; set; }
        }
        public static async Task<List<RateModel>> GetRate(int videoCourseId)
        {
            using (var db = new lmsDbContext())
            {
                var rate = await db.tbl_VideoCourseStudent.Where(x => x.VideoCourseId == videoCourseId && x.MyRate != 0)
                    .Select(x => new { x.UserId, x.MyRate, x.RateComment, x.ModifiedOn }).ToListAsync();
                var user = await db.tbl_UserInformation.Where(x => x.Enable == true)
                    .Select(x => new { x.UserInformationId, x.FullName, x.Avatar }).ToListAsync();
                var result = (from i in rate
                              join u in user on i.UserId equals u.UserInformationId
                              select new RateModel
                              {
                                  Avatar = u.Avatar,
                                  CreatedOn = i.ModifiedOn,
                                  FullName = u.FullName,
                                  MyRate = i.MyRate ?? 0,
                                  RateComment = i.RateComment,
                                  UserId = i.UserId,
                                  VideoCourseId = videoCourseId,
                              }).ToList();
                return result;
            }
        }
        public class Get_LearningDetail
        {

            public int Id { get; set; }
            public int? UserId { get; set; }
            public string FullName { get; set; }
            public int? SectionId { get; set; }
            public string SectionName { get; set; }
            public int? LessonVideoId { get; set; }
            public string LessonVideoName { get; set; }
            public DateTime? CreatedOn { get; set; }
        }
        public static async Task<object> GetLearningDetail(int videoCourseId,int userId)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_LearningDetail @VideoCourseId = {videoCourseId}," +
                    $"@UserId = {userId}";
                var data = await db.SqlQuery<Get_LearningDetail>(sql);
                if (!data.Any()) return null;
                var result = data.GroupBy(u => new { u.UserId, u.FullName })
                    .Select(us => new
                    {
                        UserId = us.Key.UserId,
                        FullName = us.Key.FullName,
                        Section = us.GroupBy(s => new { s.SectionId, s.SectionName })
                        .Select(ss => new
                        {
                            ss.Key.SectionId,
                            ss.Key.SectionName,
                            LessonVideo = ss.GroupBy(l => new { l.LessonVideoId,l.LessonVideoName,l.CreatedOn,l.Id })
                            .Select(ls => new
                            { 
                                Id = ls.Key.Id,
                                LessonVideoId = ls.Key.LessonVideoId,
                                LessonVideoName = ls.Key.LessonVideoName,
                                CreatedOn = ls.Key.CreatedOn,
                            })
                        }).ToList()
                    }).FirstOrDefault();
                return result;
            }
        }
    }
}