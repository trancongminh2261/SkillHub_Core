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
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class TranscriptService
    {
        //cấu hình bảng điểm để sử dụng chung
        //sử dụng lại những phần sau
        //Insert để tạo đợt thi
        //tạo đợt thi xong sử dụng lại GetByClass để lấy danh sách đợt thi của lớp
        public class ColumnValueModel
        {
            public int ColumnTypeId { get; set; }
            public string ColumnTypeName { get; set; }
            public string Value { get; set; }
        }
        public class StudentScoreboardModel
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; }
            public int TransciptId { get; set; }
            public string TransciptName { get; set; }
            public List<ColumnValueModel> Columns { get; set; }     
        }
        public class EditStudentScoreboardModel
        {
            public int TranscriptId { get; set; }
            public List<StudentScoreboardModel> Items { get; set; }
        }


        /*public static async Task<tbl_ScoreColumnTemplate> GetColumn(int ColumnTypeId)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_ColumnType.SingleOrDefaultAsync(x => x.Id == ColumnTypeId);
                if (data == null)
                    return new tbl_ScoreColumnTemplate();
                return data;
            }
        }*/

        /*//lấy bảng điểm của sinh viên
        public static async Task<List<StudentScoreboardModel>> GetStudentScoreboard(int transcriptId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var transcript = await db.tbl_Transcript.SingleOrDefaultAsync(x => x.Id == transcriptId && x.Enable == true);
                if (transcript == null)
                    throw new Exception("Không tìm thấy đợt thi");

                var scoreboards = await db.tbl_Scoreboard.Where(x => x.ClassId == transcript.ClassId && x.Enable == true).ToListAsync();
                var students = await db.tbl_StudentInClass.Where(x => x.ClassId == transcript.ClassId && x.Enable == true && x.StudentId > 0)
                    .Select(x => x.StudentId).Distinct().ToListAsync();

                if (user.RoleId == ((int)RoleEnum.student))
                    students = students.Where(x => x == user.UserInformationId).ToList();

                if (user.RoleId == ((int)RoleEnum.parents))
                {
                    var myChilds = await db.tbl_UserInformation
                        .Where(x => x.ParentId == user.UserInformationId && x.Enable == true)
                        .Select(x => x.UserInformationId).ToListAsync();
                    students = students.Where(x => myChilds.Contains(x ?? 0)).ToList();
                }

                var result = new List<StudentScoreboardModel>();

                foreach (var studentId in students)
                {
                    var studentResult = new StudentScoreboardModel
                    {
                        StudentId = studentId ?? 0,
                        StudentName = Task.Run(() => GetStudent(studentId ?? 0)).Result.FullName,
                        TransciptId = transcriptId,
                        TransciptName = transcript.Name,
                        Columns = new List<ColumnValueModel>()
                    };

                    var studentScoreboard = await db.tbl_StudentScoreboard.Where(x => x.Enable == true && x.StudentId == studentId).ToListAsync();
                    if (studentScoreboard != null)
                    {
                        foreach (var item in studentScoreboard)
                        {
                            var column = new ColumnValueModel
                            {
                                ColumnTypeId = item.ColumnTypeId,
                                ColumnTypeName = Task.Run(() => GetColumn(item.ColumnTypeId)).Result.Name,
                                Value = item.Value ?? ""
                            };
                            studentResult.Columns.Add(column);
                        }
                    }
                    else
                    {
                        foreach (var scoreboard in scoreboards)
                        {
                            var columnValue = new ColumnValueModel
                            {
                                ColumnTypeId = scoreboard.ColumnTypeId,
                                ColumnTypeName = Task.Run(() => GetColumn(scoreboard.ColumnTypeId)).Result.Name,
                                Value = ""
                            };
                        }                      
                    }
                    result.Add(studentResult);
                }
                return result;
            }
        }

        public static async Task UpdateStudentScoreboard(EditStudentScoreboardModel model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        var transcript = await db.tbl_Transcript.SingleOrDefaultAsync(x => x.Id == model.TranscriptId);
                        if (transcript == null)
                            throw new Exception("Không tìm thấy bảng điểm");
                        if (!model.Items.Any())
                            throw new Exception("Không tìm thấy dữ liệu");
                        foreach (var item in model.Items)
                        {
                            var studentScoreboard = await db.tbl_StudentScoreboard
                                .Where(x => x.StudentId == item.StudentId && x.TranscriptId == model.TranscriptId).ToListAsync();
                            //cập nhật giá trị cho từng cột
                            if (studentScoreboard != null)
                            {
                                foreach(var point in studentScoreboard)
                                {
                                    var i = studentScoreboard.FirstOrDefault(x => x.ColumnTypeId == point.Id);
                                    i.Value = point.Value ?? i.Value;
                                }
                            }
                            else
                            {
                                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                                if (student == null)
                                    throw new Exception("Không tìm thấy học viên");
                                foreach( var column in item.Columns)
                                {
                                    db.tbl_StudentScoreboard.Add(
                                    new tbl_Score
                                    {
                                        TranscriptId = model.TranscriptId,
                                        StudentId = item.StudentId,
                                        ColumnTypeId = column.ColumnTypeId,
                                        Value = column.Value,
                                        Enable = true,
                                        CreatedBy = user.FullName,
                                        ModifiedBy = user.FullName,
                                        CreatedOn = DateTime.Now,
                                        ModifiedOn = DateTime.Now
                                    });
                                }                              
                            }
                            await db.SaveChangesAsync();
                        }
                        trans.Commit();
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        throw e;
                    }
                }
            }
        }*/

        //bảng điểm cũ gán cứng điểm nghe nói đọc viết
        public class TranscriptCreate
        {
            [Required]
            public string Name { get; set; }
            [Required]
            public int? ClassId { get; set; }
        }
        public static async Task<tbl_Transcript> Insert(TranscriptCreate model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var _class = await db.tbl_Class.AnyAsync(x => x.Id == model.ClassId);
                    if (!_class)
                        throw new Exception("Không tìm thấy lớp học");
                    var data = new tbl_Transcript
                    {
                        Name = model.Name,
                        ClassId = model.ClassId,
                        Enable = true,
                        CreatedBy = user.FullName,
                        ModifiedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now
                    };
                    db.tbl_Transcript.Add(data);
                    await db.SaveChangesAsync();
                    return data;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await db.tbl_Transcript.SingleOrDefaultAsync(x => x.Id == id);
                    if (data == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    data.Enable = false;
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public class TranscriptModel
        {
            public int Id { get; set; }
            public List<PointModel> Items { get; set; }
        }
        public class PointModel
        {
            public int StudentId { get; set; }
            public int TermId { get; set; }
            public string TermName { get; set; }
            public string Listening { get; set; }
            public string Speaking { get; set; }
            public string Reading { get; set; }
            public string Writing { get; set; }
            public string Grammar { get; set; }
            public string Medium { get; set; }
            public bool? PassOrFail { get; set; }
            public string Note { get; set; }
            public string StudentName { get; set; }
            public string StudentCode { get; set; } 
        }
        
        public static async Task PointEdit(TranscriptModel model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        var transcript = await db.tbl_Transcript.SingleOrDefaultAsync(x => x.Id == model.Id);
                        if (transcript == null)
                            throw new Exception("Không tìm thấy bảng điểm");
                        if (!model.Items.Any())
                            throw new Exception("Không tìm thấy dữ liệu");
                        foreach (var item in model.Items)
                        {
                            var point = await db.tbl_Point
                                .Where(x => x.StudentId == item.StudentId && x.TranscriptId == model.Id).FirstOrDefaultAsync();
                            if (point != null)
                            {
                                point.Listening = item.Listening ?? point.Listening;
                                point.Speaking = item.Speaking ?? point.Speaking;
                                point.Reading = item.Reading ?? point.Reading;
                                point.Writing = item.Writing ?? point.Writing;
                                point.Medium = item.Medium ?? point.Medium;
                                point.Note = item.Note ?? point.Note;
                            }
                            else
                            {
                                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                                if (student == null)
                                    throw new Exception("Không tìm thấy học viên");
                                db.tbl_Point.Add(
                                    new tbl_Point
                                    {
                                        TranscriptId = model.Id,
                                        StudentId = item.StudentId,
                                        Listening = item.Listening,
                                        Speaking = item.Speaking,
                                        Reading = item.Reading,
                                        Writing = item.Writing,
                                        Medium = item.Medium,
                                        Note = item.Note,
                                        Enable = true,
                                        CreatedBy = user.FullName,
                                        ModifiedBy = user.FullName,
                                        CreatedOn = DateTime.Now,
                                        ModifiedOn = DateTime.Now
                                    });
                            }
                            await db.SaveChangesAsync();
                        }
                        trans.Commit();
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        throw e;
                    }
                }
            }
        }
        public static async Task<List<tbl_Transcript>> GetByClass(int classId)
        {
            using (var db = new lmsDbContext())
            {
                return await db.tbl_Transcript.Where(x => x.ClassId == classId && x.Enable == true).OrderByDescending(x => x.Id).ToListAsync();
            }
        }
        public static async Task<List<tbl_Point>> GetByStudentClass(int studentId, int classId)
        {
            using (var db = new lmsDbContext())
            {
                return await (from t in db.tbl_Transcript
                              join p in db.tbl_Point on t.Id equals p.TranscriptId into list
                              from l in list
                              where t.ClassId == classId && l.StudentId == studentId && l.Enable == true && t.Enable == true
                              select l).ToListAsync();
            }
        }
        public static async Task<List<PointModel>> GetPoint(int transcriptId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var transcript = await db.tbl_Transcript.SingleOrDefaultAsync(x => x.Id == transcriptId);
                if (transcript == null)
                    throw new Exception("Không tìm thấy bảng điểm");
                var points = await db.tbl_Point.Where(x => x.TranscriptId == transcript.Id && x.Enable == true).ToListAsync();
                var students = await db.tbl_StudentInClass.Where(x => x.ClassId == transcript.ClassId && x.Enable == true && x.StudentId > 0)
                    .Select(x => x.StudentId).Distinct().ToListAsync();

                if (user.RoleId == ((int)RoleEnum.student))
                    students = students.Where(x => x == user.UserInformationId).ToList();

                if (user.RoleId == ((int)RoleEnum.parents))
                {
                    var myChilds = await db.tbl_UserInformation
                        .Where(x => x.ParentId == user.UserInformationId && x.Enable == true)
                        .Select(x => x.UserInformationId).ToListAsync();
                    students = students.Where(x => myChilds.Contains(x ?? 0)).ToList();
                }

                var result = (from i in students
                              join p in points on i equals p.StudentId into pg
                              from n in pg.DefaultIfEmpty()
                              select new PointModel
                              {
                                  StudentId = i ?? 0,
                                  StudentName = Task.Run(() => GetStudent(i ?? 0)).Result.FullName,
                                  StudentCode = Task.Run(() => GetStudent(i ?? 0)).Result.UserCode,
                                  Listening = n == null ? "" : n.Listening,
                                  Speaking = n == null ? "" : n.Speaking,
                                  Reading = n == null ? "" : n.Reading,
                                  Writing = n == null ? "" : n.Writing,
                                  Grammar = n == null ? "" : n.Grammar,
                                  Medium = n == null ? "" : n.Medium,
                                  PassOrFail = n == null ? null : n.PassOrFail,
                                  Note = n == null ? "" : n.Note,
                                  TermId = transcriptId,
                                  TermName = transcript.Name
                              }).ToList();
                return result;
            }
        }
        public static async Task<tbl_UserInformation> GetStudent(int studentId)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentId);
                if (data == null)
                    return new tbl_UserInformation();
                return data;
            }
        }
        public static async Task<AppDomainResult> GetPointByStudent(PointSearch search)
        {
            using (var db = new lmsDbContext())
            {
                if (search == null)
                {
                    search = new PointSearch();
                }
                string sql = $"Get_Point @PageIndex = {search.PageIndex}," +
                    $"@PageSize = {search.PageSize}," +                   
                    $"@ParentIds = '{search.ParentIds ?? ""}'," +
                    $"@ClassId = '{search.ClassId}'," +
                    $"@StudentIds = '{search.StudentIds ?? ""}'";
                var data = await db.SqlQuery<Get_Point>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                return new AppDomainResult { TotalRow = totalRow, Data = data };
            }
        }
    }
}