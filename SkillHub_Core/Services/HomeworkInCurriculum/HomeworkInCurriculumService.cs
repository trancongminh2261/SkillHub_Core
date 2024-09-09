using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.HomeworkFileDTO;
using LMSCore.DTO.HomeworkFileInCurriculumDTO;
using LMSCore.DTO.HomeworkInCurriculumDTO;
using LMSCore.Models;
using LMSCore.Services.HomeworkFileInCurriculum;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services.HomeworkInCurriculum
{
    public class HomeworkInCurriculumService : DomainService
    {
        private IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private readonly IWebHostEnvironment _hostingEnvironment;
        public HomeworkInCurriculumService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<List<GetHomeworkInCurriculumDTO>> GetAll(HomeworkSearchInCurriculum baseSearch, tbl_UserInformation userLog)
        {
            if (baseSearch == null) baseSearch = new HomeworkSearchInCurriculum();
            string sql = $"Get_HomeworkInCurriculum @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," + $"@PageSize = {baseSearch.PageSize}," +
                $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                $"@ToDate = N'{baseSearch.ToDate ?? ""}'," +
                $"@CurriculumId = {baseSearch.CurriculumId ?? 0}";
            var data = await dbContext.SqlQuery<GetHomeworkInCurriculumDTO>(sql);
            if (data.Any())
            {
                data.ForEach(x =>
                {
                    x.Files = Task.Run(() => GetFiles(new HomeworkFileInCurriculumSearch()
                    {
                        HomeworkInCurriculumId = x.Id,
                        Type = HomeworkFileType.GiveHomework,
                    })).Result;
                });
            }
            return data;
        }
        public async Task<List<GetHomeworkFileInCurriculumDTO>> GetFiles(HomeworkFileInCurriculumSearch fileSearch)
        {
            var result = new List<GetHomeworkFileInCurriculumDTO>();
            var data = await HomeworkFileInCurriculumService.GetAll(fileSearch);
            if (data.Any())
                result = data.Select(x => new GetHomeworkFileInCurriculumDTO()
                {
                    File = x.File,
                    Id = x.Id,
                    Type = x.Type,
                    TypeName = x.TypeName,
                    FileName = GetDataConfig.GetFileName(x.File)
                }).ToList();
            return result;
        }

        public static async Task<List<int>> GetSessionNumberCurriculum(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Enable == true && x.Id == id);
                    if (curriculum == null)
                        throw new Exception("Không tìm thấy chương trình");
                    if (curriculum.Lesson.HasValue && curriculum.Lesson != 0)
                    {
                        var lessonNumbers = Enumerable.Range(1, (int)curriculum.Lesson).ToList();
                        return lessonNumbers;
                    }
                    else return null;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public async Task<tbl_HomeworkInCurriculum> Insert(HomeworkInCurriculumCreate request, tbl_UserInformation user)
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
                    //lưu btvn
                    var model = new tbl_HomeworkInCurriculum(request);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    dbContext.tbl_HomeworkInCurriculum.Add(model);
                    await dbContext.SaveChangesAsync();
                    // Lưu file
                    var files = new List<tbl_HomeworkFileInCurriculum>();
                    if (request.AddFiles.Any())
                    {
                        foreach (var file in request.AddFiles)
                        {
                            var f = await HomeworkFileInCurriculumService.Insert(new HomeworkFileInCurriculumCreate()
                            {
                                File = file.File,
                                Type = HomeworkFileType.GiveHomework,
                                HomeworkInCurriculumId = model.Id,
                            }, user);
                            files.Add(f);
                        }
                    }
                    model.Files = files.Select(x => new GetHomeworkFileInCurriculumDTO()
                    {
                        File = x.File,
                        Id = x.Id,
                        Type = x.Type,
                        TypeName = x.TypeName,
                        FileName = GetDataConfig.GetFileName(x.File)
                    }).ToList();
                    transaction.Commit();
                    return model;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        public async Task<tbl_HomeworkInCurriculum> Update(HomeworkInCurriculumUpdate request, tbl_UserInformation user)
        {
            tbl_HomeworkInCurriculum homework = await dbContext.tbl_HomeworkInCurriculum.SingleOrDefaultAsync(x => x.Id == request.Id);
            if (homework == null)
            {
                throw new Exception("Bài tập không tồn tại");
            }
            if (request.IeltsExamId.HasValue && homework.Type == HomeworkType.Exam)
            {
                var hasIeltsExam = await dbContext.tbl_IeltsExam
                    .AnyAsync(x => x.Id == request.IeltsExamId && x.Enable == true);
                if (!hasIeltsExam)
                    throw new Exception("Không tìm thấy đề");
            }
            homework.HomeworkContent = request.HomeworkContent ?? homework.HomeworkContent;
            homework.IeltsExamId = request.IeltsExamId ?? homework.IeltsExamId;
            homework.Name = request.Name ?? homework.Name;
            homework.FromDate = request.FromDate ?? homework.FromDate;
            homework.ToDate = request.ToDate ?? homework.ToDate;
            homework.Note = request.Note ?? homework.Note;
            homework.CutoffScore = request.CutoffScore;
            homework.Index = request.Index;
            homework.SessionNumber = request.SessionNumber;
            homework.ModifiedBy = user.FullName;
            homework.ModifiedOn = DateTime.Now;

            await dbContext.SaveChangesAsync();

            // Cập nhật file bài tập
            if (request.Files.Any())
            {
                //Xóa file
                var files = await dbContext.tbl_HomeworkFileInCurriculum.Where(x => x.HomeworkInCurriculumId == homework.Id && x.Enable == true).ToListAsync();
                var delFiles = files.Where(x => !request.Files.Where(x => x.Id.HasValue && x.Id != 0).Select(f => "," + f.Id + ",").Contains("," + x.Id + ",")).ToList();
                foreach (var delFile in delFiles)
                {
                    await HomeworkFileInCurriculumService.Delete(delFile.Id);
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
                //var newFiles = request.Files.Where(x => x.Id == 0).ToList();
                foreach (var newFile in request.Files)
                {
                    if (!newFile.Id.HasValue)
                    {
                        var f = await HomeworkFileInCurriculumService.Insert(new HomeworkFileInCurriculumCreate()
                        {
                            File = newFile.File,
                            Type = HomeworkFileType.GiveHomework,
                            HomeworkInCurriculumId = homework.Id,
                        }, user);
                        files.Add(f);
                    }
                }
                homework.Files = files.Select(x => new GetHomeworkFileInCurriculumDTO()
                {
                    File = x.File,
                    Id = x.Id,
                    Type = x.Type,
                    TypeName = x.TypeName,
                    FileName = GetDataConfig.GetFileName(x.File)
                }).ToList();
            }
            else
            {
                var files = await dbContext.tbl_HomeworkFileInCurriculum.Where(x => x.HomeworkInCurriculumId == homework.Id && x.Enable == true).ToListAsync();
                foreach (var delFile in files)
                {
                    await HomeworkFileInCurriculumService.Delete(delFile.Id);
                }
            }
            return homework;
        }

        public async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var homeworkInCurriculumData = await dbContext.tbl_HomeworkInCurriculum.Where(x => x.Enable == true).ToListAsync();
                        var entity = homeworkInCurriculumData.SingleOrDefault(x => x.Id == id && x.Enable == true);
                        if (entity == null)
                            throw new Exception("Không tìm thấy dữ liệu bài tập");
                        entity.Enable = false;
                        var homeworkInCurriculum = homeworkInCurriculumData.Where(x => x.Enable == true && x.CurriculumId == entity.CurriculumId && x.Index.HasValue).OrderBy(x => x.Index).ToList();
                        if (homeworkInCurriculum.Any())
                        {
                            homeworkInCurriculum.ForEach(h => h.Index = homeworkInCurriculum.IndexOf(h) + 1);
                            await dbContext.SaveChangesAsync();
                        }
                        await db.SaveChangesAsync();
                        await HomeworkFileInCurriculumService.DeleteByHomeworkInCurriculumId(entity.Id);
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw e;
                    }
                }
            }
        }

        public async Task<List<tbl_HomeworkInCurriculum>> UpdateIndexHomeworkInCurriculum(List<IndexHomeworkInCurriculum> request, tbl_UserInformation user)
        {
            try
            {
                var homeworkInCurriculumData = new List<tbl_HomeworkInCurriculum>();
                var homeworkInCurriculumModel = await dbContext.tbl_HomeworkInCurriculum
                    .Where(x => x.Enable == true)
                    .ToListAsync();
                var entity = request;
                foreach (var item in request)
                {
                    var homeworkInCurriculum = homeworkInCurriculumModel.SingleOrDefault(x => x.Id == item.Id);
                    if (homeworkInCurriculum == null)
                        throw new Exception("Không tìm thấy thông tin giáo trình có Id " + item.Id);
                    homeworkInCurriculum.Index = item.Index;
                    homeworkInCurriculumData.Add(homeworkInCurriculum);
                }
                await dbContext.SaveChangesAsync();
                return homeworkInCurriculumData;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
