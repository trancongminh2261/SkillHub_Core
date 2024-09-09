using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Hangfire;
using LMSCore.DTO.HomeworkFileDTO;
using static System.Net.WebRequestMethods;

namespace LMSCore.Services.Homework
{
    public class HomeworkService : DomainService
    {
        private IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private readonly IWebHostEnvironment _hostingEnvironment;
        public HomeworkService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<tbl_Homework> GetById(int id)
        {
            var data = await dbContext.tbl_Homework.SingleOrDefaultAsync(x => x.Id == id);
            if (data != null)
                data.Files = await GetFiles(new HomeworkFileSearch()
                {
                    HomeworkId = data.Id,
                    Type = HomeworkFileType.GiveHomework,
                });
            return data;
        }
        public async Task<tbl_Homework> Insert(HomeworkCreate request, tbl_UserInformation user)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (request.Type == HomeworkType.Exam && request.IeltsExamId == null)
                        throw new Exception("Vui lòng chọn đề");
                    if (request.Type == HomeworkType.Homework && string.IsNullOrEmpty(request.HomeworkContent) && !request.AddFiles.Any())
                        throw new Exception("Vui lòng điền nội dung bài tập hoặc thêm file bài tập");
                    if (request.IeltsExamId.HasValue && request.Type == HomeworkType.Exam)
                    {
                        tbl_IeltsExam exam = await dbContext.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == request.IeltsExamId);
                        if (exam == null)
                            throw new Exception("Không tìm thấy để thi");
                    }
                    tbl_Class _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == request.ClassId);
                    if (_class == null) { throw new Exception("Không tìm thấy lớp học"); }
                    var teacher = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == request.TeacherId && x.Enable == true && x.RoleId == 2);
                    if (teacher == null)
                        throw new Exception("Không tìm thấy giáo viên");

                    //lưu btvn
                    var model = new tbl_Homework(request);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    dbContext.tbl_Homework.Add(model);
                    await dbContext.SaveChangesAsync();
                    // Lưu file
                    var files = new List<tbl_HomeworkFile>();
                    if (request.AddFiles.Any())
                    {
                        foreach (var file in request.AddFiles)
                        {
                            var f = await HomeworkFileService.Insert(new HomeworkFileCreate()
                            {
                                File = file.File,
                                Type = HomeworkFileType.GiveHomework,
                                HomeworkId = model.Id,
                            }, user);
                            files.Add(f);
                        }
                    }
                    transaction.Commit();

                    BackgroundJob.Schedule(() => HomeworkNotification.NotifyStudentHaveHomework(new HomeworkNotificationRequest.NotifyStudentHaveHomeworkRequest
                    {
                        HomeworkId = model.Id,
                        CurrentUser = user,
                    }), TimeSpan.FromSeconds(2));
                    model.Files = files.Select(x => new HomeworkFileDTO()
                    {
                        File = x.File,
                        Id = x.Id,
                        Type = x.Type,
                        TypeName = x.TypeName,
                        FileName = GetFileName(x.File)
                    }).ToList();
                    return model;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }
        public async Task<tbl_Homework> Update(HomeworkUpdate request, tbl_UserInformation user)
        {
            tbl_Homework homework = await dbContext.tbl_Homework.SingleOrDefaultAsync(x => x.Id == request.Id);
            if (homework == null)
            {
                throw new Exception("BTVN không tồn tại");
            }
            if (request.IeltsExamId.HasValue && homework.Type == HomeworkType.Exam)
            {
                var hasIeltsExam = await dbContext.tbl_IeltsExam
                    .AnyAsync(x => x.Id == request.IeltsExamId && x.Enable == true);
                if (!hasIeltsExam)
                    throw new Exception("Không tìm thấy đề");
            }
            var hasResult = await dbContext.tbl_IeltsExamResult.AnyAsync(x => x.ValueId == request.Id && x.Enable == true && x.Type == 3);
            if (request.IeltsExamId.HasValue && hasResult && homework.IeltsExamId != request.IeltsExamId)
                throw new Exception("Học viên đã làm bài, không thể sửa đề");
            if (request.TeacherId.HasValue)
            {
                var teacher = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == request.TeacherId && x.Enable == true && x.RoleId == 2);
                if (teacher == null)
                    throw new Exception("Không tìm thấy giáo viên");
            }
            homework.HomeworkContent = request.HomeworkContent ?? homework.HomeworkContent;
            homework.TeacherId = request.TeacherId ?? homework.TeacherId;
            homework.IeltsExamId = request.IeltsExamId ?? homework.IeltsExamId;
            homework.Name = request.Name ?? homework.Name;
            homework.FromDate = request.FromDate ?? homework.FromDate;
            homework.ToDate = request.ToDate ?? homework.ToDate;
            homework.Note = request.Note ?? homework.Note;
            homework.ModifiedBy = user.FullName;
            homework.ModifiedOn = DateTime.Now;
            homework.CutoffScore = request.CutoffScore;
            homework.Index = request.Index;
            homework.SessionNumber = request.SessionNumber;

            await dbContext.SaveChangesAsync();

            // Cập nhật file bài tập
            if (request.Files.Any())
            {
                //Xóa file
                var files = await dbContext.tbl_HomeworkFile.Where(x => x.HomeworkId == homework.Id && x.Enable == true).ToListAsync();
                var delFiles = files.Where(x => !request.Files.Where(x => x.Id != 0).Select(f => "," + f.Id + ",").Contains("," + x.Id + ",")).ToList();
                foreach (var delFile in delFiles)
                {
                    await HomeworkFileService.Delete(delFile.Id);
                    files.Remove(delFile);
                }
                ////update file
                //var upFiles = request.Files.Where(x => x.Id != 0).ToList(); 
                //foreach (var upFile in upFiles)
                //{
                //    var f = await HomeworkFileService.Update(new HomeworkFileUpdate()
                //    {
                //        File = upFile.File,
                //        Id = upFile.Id,
                //    }, user);
                //    files.Add(f);
                //}
                //thêm file mới
                var newFiles = request.Files.Where(x => x.Id == 0).ToList();
                foreach (var newFile in newFiles)
                {
                    var f = await HomeworkFileService.Insert(new HomeworkFileCreate()
                    {
                        File = newFile.File,
                        Type = HomeworkFileType.GiveHomework,
                        HomeworkId = homework.Id,
                    }, user);
                    files.Add(f);
                }
                homework.Files = files.Select(x => new HomeworkFileDTO()
                {
                    File = x.File,
                    Id = x.Id,
                    Type = x.Type,
                    TypeName = x.TypeName,
                    FileName = GetFileName(x.File)
                }).ToList();
            }
            else
            {
                var files = await dbContext.tbl_HomeworkFile.Where(x => x.HomeworkId == homework.Id && x.Enable == true).ToListAsync();
                foreach (var delFile in files)
                {
                    await HomeworkFileService.Delete(delFile.Id);
                }
            }
            return homework;
        }
        public async Task<List<Get_Homework>> GetAll(HomeworkSearch baseSearch, tbl_UserInformation userLog)
        {
            if (baseSearch == null) baseSearch = new HomeworkSearch();
            if (userLog.RoleId == (int)RoleEnum.student)
            {
                baseSearch.StudentId = userLog.UserInformationId;
            }
            string sql = $"Get_Homework @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," + $"@PageSize = {baseSearch.PageSize}," +
                $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                $"@ToDate = N'{baseSearch.ToDate ?? ""}'," +
                $"@StudentId = '{baseSearch.StudentId ?? 0}'," +
                $"@ClassId = {baseSearch.ClassId ?? 0}";
            var data = await dbContext.SqlQuery<Get_Homework>(sql);
            if (data.Any())
                data.ForEach(x =>
                {
                    x.Files = Task.Run(() => GetFiles(new HomeworkFileSearch()
                    {
                        HomeworkId = x.Id,
                        Type = HomeworkFileType.GiveHomework,
                    })).Result;
                });
            return data;
        }
        public async Task<List<HomeworkFileDTO>> GetFiles(HomeworkFileSearch fileSearch)
        {
            var result = new List<HomeworkFileDTO>();
            var data = await HomeworkFileService.GetAll(fileSearch);
            if (data.Any())
                result = data.Select(x => new HomeworkFileDTO()
                {
                    File = x.File,
                    Id = x.Id,
                    Type = x.Type,
                    TypeName = x.TypeName,
                    FileName = GetFileName(x.File)
                }).ToList();
            return result;
        }
        public string GetFileName(string linkFile)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(linkFile))
                return result;
            var arr = linkFile.Split("/").ToArray();
            var name = arr[arr.Length - 1].Split("-")[0];
            var ext = arr[arr.Length - 1].Split("-")[1].Split(".")[1];
            result = name + "." + ext;
            return result;
        }
        public async Task Delete(int id)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var homeworkInClassData = await dbContext.tbl_Homework.Where(x => x.Enable == true).ToListAsync();

                    tbl_Homework homework = homeworkInClassData.SingleOrDefault(x => x.Id == id);
                    if (homework == null)
                    {
                        throw new Exception("Bài tập về nhà không tồn tại");
                    }
                    homework.Enable = false;
                    var homeworkInClass = homeworkInClassData.Where(x => x.Enable == true && x.ClassId == homework.ClassId && x.Index.HasValue).OrderBy(x => x.Index).ToList();
                    if (homeworkInClass.Any())
                    {
                        homeworkInClass.ForEach(h => h.Index = homeworkInClass.IndexOf(h) + 1);
                        await dbContext.SaveChangesAsync();
                    }
                    await dbContext.SaveChangesAsync();
                    List<tbl_StudentHomework> listStudentHomework = await dbContext.tbl_StudentHomework.Where(x => x.HomeworkId == id && x.Enable == true).ToListAsync();
                    if (listStudentHomework.Any())
                    {
                        foreach (var sh in listStudentHomework)
                        {
                            sh.Enable = false;
                        }
                        await dbContext.SaveChangesAsync();
                    }
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw new Exception(e.Message);
                }
            }
        }

        public async Task<List<TeacherAvailable>> GetTeacherAvailable()
        {
            using (var db = new lmsDbContext())
            {
                var teachers = await db.tbl_UserInformation.Where(x => x.RoleId == 2 && x.Enable == true)
                    .Select(x => new TeacherAvailable
                    {
                        Id = x.UserInformationId,
                        TeacherCode = x.UserCode,
                        TeacherName = x.FullName
                    }).ToListAsync();

                return teachers;
            }
        }
        public async Task<List<TeacherAvailable>> GetTeacherAvailable(int classId)
        {
            using (var db = new lmsDbContext())
            {

                var teacherInClass = await db.tbl_Schedule.Where(x => x.ClassId == classId && x.Enable == true)
                    .Select(x => x.TeacherId).Distinct().ToListAsync();
                var teachers = await db.tbl_UserInformation.Where(x => x.RoleId == 2 && x.Enable == true && teacherInClass.Contains(x.UserInformationId))
                    .Select(x => new TeacherAvailable
                    {
                        Id = x.UserInformationId,
                        TeacherCode = x.UserCode,
                        TeacherName = x.FullName
                    }).ToListAsync();
                return teachers;
            }
        }

        public async Task<List<tbl_Homework>> UpdateIndexHomework(List<IndexHomeworkUpdate> request, tbl_UserInformation user)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var homeworkData = new List<tbl_Homework>();
                    var homeworkModel = await dbContext.tbl_Homework
                        .Where(x => x.Enable == true)
                        .ToListAsync();
                    var entity = request;
                    foreach (var item in request)
                    {
                        var homeworkInCurriculum = homeworkModel.SingleOrDefault(x => x.Id == item.Id);
                        if (homeworkInCurriculum == null)
                            throw new Exception("Không tìm thấy thông tin bài tập có Id " + item.Id);
                        homeworkInCurriculum.Index = item.Index;
                        homeworkData.Add(homeworkInCurriculum);
                    }
                    await dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return homeworkData;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        public async Task UpdateTeacher(HomeworkForTeacherUpdate itemModel)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var teacher = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == itemModel.TeacherId);
                    if (teacher == null)
                        throw new Exception("Không tìm thấy thông tin giáo viên ");
                    var homeworkData = await db.tbl_Homework.Where(x => x.Enable == true).ToListAsync();
                    var schedule = await db.tbl_Schedule.Where(x => x.Enable == true && x.TeacherId == itemModel.TeacherId)
                        .Select(x => new { x.Id, x.ClassId, x.TeacherId })
                        .ToListAsync();
                    int classId = 0;
                    int checkHomeworkId = 0;
                    var index = 0;
                    foreach (var homeworkId in itemModel.HomeworkIds)
                    {
                        var homework = homeworkData.FirstOrDefault(x => x.Id == homeworkId);
                        if (homework == null)
                            throw new Exception("Không tìm thấy thông tin bài tập có id là " + homeworkId);
                        if (!schedule.Any(x => x.ClassId == homework.ClassId))
                        {
                            throw new Exception("Không thể thêm bài tập cho giáo viên " + teacher.FullName + " không thuộc lớp chứa các bài tập này");
                        }
                        if (index == 0)
                        {
                            classId = homework.ClassId;
                            checkHomeworkId = homework.Id;
                            index++;
                        }
                        if(classId != homework.ClassId)
                            throw new Exception("Bài tập có Id = " + checkHomeworkId + " không cùng cùng một lớp với bài tập có Id " + homework.Id);
                        homework.TeacherId = itemModel.TeacherId;
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}