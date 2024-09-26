using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.Models;
using LMSCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LMS_Project.Services
{
    public class FileInVideoService : DomainService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public string domain { get; private set; }
        public string projectName { get; private set; }

        public FileInVideoService(lmsDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(context)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            // Gán giá trị cho các biến từ configuration trong constructor
            domain = _configuration["MySettings:DomainFE"];
            projectName = _configuration["MySettings:ProjectName"];
        }
        public void PushNotiNewFile(int lessonId, tbl_UserInformation userLog)
        {
                try
                {
                    var lesson = dbContext.tbl_LessonVideo.SingleOrDefault(x => x.Id == lessonId);
                    if (lesson == null)
                        return;

                    var section = dbContext.tbl_Section.SingleOrDefault(x => x.Id == lesson.SectionId);
                    if (section == null)
                        return;

                    var videoCourse = dbContext.tbl_VideoCourse.SingleOrDefault(x => x.Id == section.VideoCourseId);
                    if (videoCourse == null)
                        return;

                    var studentIds = dbContext.tbl_VideoCourseStudent.Where(x => x.VideoCourseId == videoCourse.Id && x.Enable == true)
                        .Select(x => x.UserId).Distinct().ToList();
                    if (studentIds.Any())
                    {
                        //https://skillhub.mona.software/learning/?course=84&sectionIds=57&currentLessonId=1170
                        //string href = $"<a href=\"{domain}/course/video-course/detail/?slug={videoCourse.Id}\"><b style=\"color: blue;\">Tại đây</b></a>";
                        string href = $"<a href=\"{domain}/learning/?course={videoCourse.Id}&sectionIds={section.Id}&currentLessonId={lesson.Id}\"><b style=\"color: blue;\">Tại đây</b></a>";
                        string title = "Thông báo tài liệu mới";
                        string content = $"Bài học {lesson.Name} trong khóa {videoCourse.Name} đã được thêm tài liệu mới, vui lòng truy cập {href} để học";
                        string onesignalContent = $"Bài học {lesson.Name} trong khóa {videoCourse.Name} đã được thêm tài liệu mới, vui lòng kiểm tra lại thông tin";

                        foreach (var studentId in studentIds)
                        {
                            var student = dbContext.tbl_UserInformation.SingleOrDefault(x => x.UserInformationId == studentId);
                            NotificationService.SendNotThread(dbContext,
                                new NotificationService.SendNotThreadModel
                                {
                                    Content = content,
                                    Email = "",
                                    EmailContent = "",
                                    OnesignalId = student.OneSignal_DeviceId,
                                    Title = title,
                                    UserId = student.UserInformationId,
                                    OnesignalContent = onesignalContent,
                                    OnesignalUrl = $"{domain}/learning/?course={videoCourse.Id}&sectionIds={section.Id}&currentLessonId={lesson.Id}"
                                }
                                , userLog, false);
                        }
                    }
                }
                catch
                {
                    return;
                }

        }
        public async Task<tbl_FileInVideo> Insert(FileInVideoCreate fileInVideoCreate, tbl_UserInformation user)
        {
                try
                {
                    var lessonVideo = await dbContext.tbl_LessonVideo.SingleOrDefaultAsync(x => x.Id == fileInVideoCreate.LessonVideoId);
                    if (lessonVideo == null)
                        throw new Exception("Không tìm thấy bài học");
                    var model = new tbl_FileInVideo(fileInVideoCreate);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    dbContext.tbl_FileInVideo.Add(model);
                    await dbContext.SaveChangesAsync();
                    Thread pushnoti = new Thread(() =>
                        PushNotiNewFile(lessonVideo.Id, user));
                    pushnoti.Start();
                    return model;
                }
                catch (Exception e)
                {
                    throw e;
                }
        }
        public async Task Delete(int id)
        {
                try
                {
                    var fileInVideo = await dbContext.tbl_FileInVideo.SingleOrDefaultAsync(x => x.Id == id);
                    if (fileInVideo == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    fileInVideo.Enable = false;
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }
        }
        public async Task<List<tbl_FileInVideo>> GetByLesson(int lessonVideoId)
        {
                var data = await dbContext.tbl_FileInVideo.Where(x => x.LessonVideoId == lessonVideoId && x.Enable == true).OrderBy(x => x.Id).ToListAsync();
                return data;
        }
    }
}