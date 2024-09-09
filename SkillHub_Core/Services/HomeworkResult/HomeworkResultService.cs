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
using RestSharp.Validation;

namespace LMSCore.Services.HomeworkResult
{
    public class HomeworkResultService : DomainService
    {
        private IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private readonly IWebHostEnvironment _hostingEnvironment;
        public HomeworkResultService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<tbl_HomeworkResult> GetById(int id)
        {
            var data = await dbContext.tbl_HomeworkResult.SingleOrDefaultAsync(x => x.Id == id);
            if (data != null)
            {
                data.Files = await GetFiles(new HomeworkFileSearch()
                {
                    HomeworkId = data.Id,
                    UserId = data.StudentId,
                    Type = HomeworkFileType.SubmitHomework,
                });
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.StudentId && x.Enable == true && x.RoleId == (int)RoleEnum.student);
                data.StudentName = student?.FullName;
                var teacher = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.TeacherId && x.Enable == true && x.RoleId == (int)RoleEnum.teacher);
                data.TeacherName = teacher?.FullName;
            }
            return data;
        }
        public async Task Validate(tbl_HomeworkResult model)
        {
            var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == model.ClassId);
            if (_class == null)
                throw new Exception("Không tìm thấy lớp học");
            var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == model.StudentId && x.Enable == true && x.RoleId == (int)RoleEnum.student);
            if (student == null)
                throw new Exception("Không tìm thấy học viên");
            var homework = await dbContext.tbl_Homework.SingleOrDefaultAsync(x => x.Id == model.HomeworkId && x.Enable == true);
            if (homework == null)
                throw new Exception("Không tìm thấy bài tập");
            var studentInClass = await dbContext.tbl_StudentInClass.AnyAsync(x => x.StudentId == model.StudentId
            && x.Enable == true
            && x.ClassId == model.ClassId);
            if (!studentInClass)
                throw new Exception("Không học viên không thuộc lớp này");

        }
        public async Task<tbl_HomeworkResult> Insert(HomeworkResultCreate request, tbl_UserInformation user)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (user.RoleId != (int)RoleEnum.student)
                        throw new Exception("Đây không phải là tài khoản học viên");
                    //lưu btvn
                    var model = new tbl_HomeworkResult(request);
                    model.StudentId = user.UserInformationId;
                    await Validate(model);
                    model.Type = HomeworkResultType.NoPointYet;
                    model.TypeName = HomeworkResultTypeName(model.Type);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    dbContext.tbl_HomeworkResult.Add(model);
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
                                Type = HomeworkFileType.SubmitHomework,
                                HomeworkId = model.Id,
                            }, user);
                            files.Add(f);
                        }
                    }
                    // Check bài tập tuần tự, theo buổi, theo điểm sản
                    var homework = await dbContext.tbl_Homework.SingleOrDefaultAsync(x => x.Id == model.HomeworkId && x.Enable == true);

                    var homeworkData = await dbContext.tbl_Homework.Where(x => x.Enable == true).ToListAsync();
                    var homeworkInclass = homeworkData
                        .Where(x => x.ClassId == homework.ClassId && x.Enable == true && x.Index < homework.Index && x.Index.HasValue)
                        .OrderBy(x => x.Index)
                        .ToList();
                    var homeworkConfig = await dbContext.tbl_HomeworkSequenceConfigInClass.FirstOrDefaultAsync(x => x.Enable == true && x.ClassId == homework.ClassId);
                    var ieltsExamResultData = await dbContext.tbl_IeltsExamResult.Where(x => x.Enable == true).ToListAsync();
                    var homeworkResultData = await dbContext.tbl_HomeworkResult.Where(x => x.Enable == true).ToListAsync();
                    if (homeworkConfig.IsAllow == true)
                    {
                        foreach (var h in homeworkInclass)
                        {
                            if (h.Type == HomeworkType.Exam)
                            {
                                var ieltsExamResult = ieltsExamResultData.Where(x => x.ValueId == h.Id && x.StudentId == user.UserInformationId).ToList();

                                if (h.CutoffScore.HasValue)
                                {
                                    if (ieltsExamResult.Any(x => x.Status != 1))
                                    {

                                        if (!ieltsExamResult.Any(x => x.IsPassed == true))
                                        {
                                            throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                        }
                                    }
                                    else
                                        throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                }
                                else if (!ieltsExamResult.Any())
                                    throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                            }
                            else if (h.Type == HomeworkType.Homework)
                            {
                                var homeworkResult = homeworkResultData.Where(x => x.HomeworkId == h.Id && x.StudentId == user.UserInformationId).ToList();

                                if (h.CutoffScore.HasValue)
                                {
                                    if (!homeworkResult.Any(x => x.Type == HomeworkResultType.GotPoint))
                                    {

                                        if (!homeworkResult.Any(x => x.IsPassed == true))
                                        {
                                            throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                        }
                                    }
                                    else
                                        throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                                }
                                else if (!homeworkResult.Any())
                                    throw new Exception("Bạn phải vượt qua bài tập " + h.Name + " trước");
                            }
                        }
                    }
                    if (homework.SessionNumber.HasValue && homework.SessionNumber != 0)
                    {
                        var schedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true && x.ClassId == homework.ClassId).OrderBy(x => x.StartTime).ToListAsync();
                        var scheduleElement = schedule.ElementAtOrDefault((int)(homework.SessionNumber - 1));
                        if (scheduleElement != null)
                        {
                            if (scheduleElement.StartTime > DateTime.Now)
                                throw new Exception("Bạn chưa được phép làm bài tập này do chưa đến buổi học ngày " + scheduleElement.StartTime.Value.ToString("dd/MM/yyyy HH:mm"));
                        }
                    }

                    tbl_StudentHomework studentHomework = new tbl_StudentHomework
                    {
                        HomeworkId = homework.Id,
                        ClassId = homework.ClassId,
                        StudentId = user.UserInformationId,
                        Status = (int)StudentHomeworkStatus.DaNop,
                        StatusName = ListStudentHomeworkStatus().SingleOrDefault(x => x.Key == (int)StudentHomeworkStatus.DaNop)?.Value,
                        FromDate = null,
                        ToDate = null,
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true
                    };
                    dbContext.tbl_StudentHomework.Add(studentHomework);
                    await dbContext.SaveChangesAsync();
                    transaction.Commit();
                    model.StudentName = user?.FullName;
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
        public async Task<tbl_HomeworkResult> Update(StudentHomeworkResultUpdate request, tbl_UserInformation user)
        {
            //if (user.RoleId != (int)RoleEnum.student)
            //    throw new Exception("Đây không phải là tài khoản học viên");
            var homework = await dbContext.tbl_HomeworkResult.SingleOrDefaultAsync(x => x.Id == request.Id);
            if (homework == null)
            {
                throw new Exception("BTVN không tồn tại");
            }
            if (homework.Type == HomeworkResultType.GotPoint)
                throw new Exception("BTVN này đã được chấm điểm không thể chỉnh sửa");
            homework.Content = request.Content ?? homework.Content;
            homework.ModifiedBy = user.FullName;
            homework.ModifiedOn = DateTime.Now;

            await dbContext.SaveChangesAsync();

            // Cập nhật file bài tập
            if (request.Files.Any())
            {
                //Xóa file
                var files = await dbContext.tbl_HomeworkFile.Where(x => x.HomeworkId == homework.Id
                && x.UserId == homework.StudentId
                && x.Enable == true).ToListAsync();
                var delFiles = files.Where(x => !request.Files.Where(x => x.Id != 0).Select(f => "," + f.Id + ",").Contains("," + x.Id + ",")).ToList();
                foreach (var delFile in delFiles)
                {
                    await HomeworkFileService.Delete(delFile.Id);
                    files.Remove(delFile);
                }
                //thêm file mới
                var newFiles = request.Files.Where(x => x.Id == 0).ToList();
                foreach (var newFile in newFiles)
                {
                    var f = await HomeworkFileService.Insert(new HomeworkFileCreate()
                    {
                        File = newFile.File,
                        Type = HomeworkFileType.SubmitHomework,
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
                homework.Files = await GetFiles(new HomeworkFileSearch()
                {
                    HomeworkId = homework.Id,
                    UserId = homework.StudentId,
                    Type = HomeworkFileType.SubmitHomework,
                });
            }
            homework.StudentName = user?.FullName;
            return homework;
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
        public async Task<tbl_HomeworkResult> TeacherUpdate(TeacherHomeworkResultUpdate request, tbl_UserInformation user)
        {
            //if (user.RoleId != (int)RoleEnum.teacher)
            //    throw new Exception("Đây không phải là tài khoản giáo viên");
            var homework = await dbContext.tbl_HomeworkResult.SingleOrDefaultAsync(x => x.Id == request.Id);
            var homeworkInClass = await dbContext.tbl_Homework.FirstOrDefaultAsync(x => x.Id == homework.HomeworkId);
            if (homework == null)
            {
                throw new Exception("BTVN không tồn tại");
            }
            if (homeworkInClass == null)
            {
                throw new Exception("BTVN không tồn tại");
            }
            homework.Type = HomeworkResultType.GotPoint;
            homework.TypeName = HomeworkResultTypeName(homework.Type);
            homework.TeacherId = user.UserInformationId;
            homework.TeacherName = user.FullName;
            homework.Point = request.Point ?? homework.Point;
            homework.TeacherNote = request.TeacherNote ?? homework.TeacherNote;
            homework.ModifiedBy = user.FullName;
            homework.ModifiedOn = DateTime.Now;
            if (homeworkInClass.CutoffScore.HasValue)
            {
                if (homework.Point < homeworkInClass.CutoffScore)
                    homework.IsPassed = false;
                else
                    homework.IsPassed = true;
            }

            await dbContext.SaveChangesAsync();

            homework.Files = await GetFiles(new HomeworkFileSearch()
            {
                HomeworkId = homework.Id,
                UserId = homework.StudentId,
                Type = HomeworkFileType.SubmitHomework,
            });
            var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == homework.StudentId && x.Enable == true && x.RoleId == (int)RoleEnum.student);
            homework.StudentName = student?.FullName;

            return homework;
        }
        public async Task<List<Get_HomeworkResult>> GetAll(HomeworkResultSearch baseSearch, tbl_UserInformation userLog)
        {
            if (baseSearch == null) baseSearch = new HomeworkResultSearch();
            if (userLog.RoleId == (int)RoleEnum.student)
            {
                baseSearch.StudentId = userLog.UserInformationId;
            }
            int type = 0;
            if (baseSearch.Type.HasValue)
                type = (int)baseSearch.Type.Value;
            string sql = $"Get_HomeworkResult @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," + $"@PageSize = {baseSearch.PageSize}," +
                $"@HomeworkId = '{baseSearch.HomeworkId}'," +
                $"@StudentId = '{baseSearch.StudentId ?? 0}'," +
                $"@Type = {type}";
            var data = await dbContext.SqlQuery<Get_HomeworkResult>(sql);
            if (data.Any())
                foreach (var d in data)
                {
                    d.Files = await GetFiles(new HomeworkFileSearch()
                    {
                        HomeworkId = d.Id,
                        Type = HomeworkFileType.SubmitHomework,
                    });
                }
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
        public async Task Delete(int id)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var homework = await dbContext.tbl_HomeworkResult.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    if (homework == null)
                    {
                        throw new Exception("Bài tập về nhà không tồn tại");
                    }
                    homework.Enable = false;
                    await dbContext.SaveChangesAsync();
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw new Exception(e.Message);
                }
            }
        }
    }
}