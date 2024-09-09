using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class LessonVideoService
    {
        public static async Task<tbl_LessonVideo> Insert(LessonVideoCreate lessonVideoCreate,tbl_UserInformation user,string mapPath)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var section = await db.tbl_Section.SingleOrDefaultAsync(x => x.Id == lessonVideoCreate.SectionId);
                    if (section == null)
                        throw new Exception("Không tìm thấy phần này");
                    var model = new tbl_LessonVideo(lessonVideoCreate);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    var prevIndex = await db.tbl_LessonVideo
                        .Where(x => x.SectionId == model.SectionId)
                        .OrderByDescending(x => x.Index).FirstOrDefaultAsync();
                    model.Index = prevIndex == null ? 1 : (prevIndex.Index + 1);
                    if (model.FileType == LessonFileType.FileUpload)
                    {
                        /////đọc thời gian video tại đây
                        //string[] videoUrls = model.VideoUrl.Split('/');
                        //if (videoUrls.Length > 0)
                        //{
                        //    try
                        //    {
                        //        string mapUrl = $"{mapPath}/{videoUrls[videoUrls.Length - 1]}";
                        //        var player = new WindowsMediaPlayer();
                        //        var clip = player.newMedia(mapUrl);
                        //        double time = clip.duration;
                        //        model.Minute = (int)(time / 60);
                        //    }
                        //    catch { }
                        //}
                    }
                    db.tbl_LessonVideo.Add(model);
                    await db.SaveChangesAsync();
                    return model;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static async Task<tbl_LessonVideo> Update(LessonVideoUpdate model, tbl_UserInformation user, string mapPath)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_LessonVideo.SingleOrDefaultAsync(x => x.Id == model.Id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Name = model.Name ?? entity.Name;
                    entity.ExamId = model.ExamId ?? entity.ExamId;
                    entity.VideoUrl = model.VideoUrl ?? entity.VideoUrl;
                    if (entity.FileType == LessonFileType.FileUpload)
                    {
                        /////đọc thời gian video tại đây
                        //string[] videoUrls = model.VideoUrl.Split('/');
                        //if (videoUrls.Length > 0)
                        //{
                        //    try
                        //    {
                        //        string mapUrl = $"{mapPath}/{videoUrls[videoUrls.Length - 1]}";
                        //        var player = new WindowsMediaPlayer();
                        //        var clip = player.newMedia(mapUrl);
                        //        double time = clip.duration;
                        //        entity.Minute = (int)(time / 60);
                        //    }
                        //    catch { }
                        //}
                    }
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = model.ModifiedOn;
                    await db.SaveChangesAsync();
                    return entity;
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
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entity = await db.tbl_LessonVideo.SingleOrDefaultAsync(x => x.Id == id);
                        if (entity == null)
                            throw new Exception("Không tìm thấy dữ liệu");
                        entity.Enable = false;
                        await db.SaveChangesAsync();
                        //Cập nhật lại vi trí
                        var lessonVideos = await db.tbl_LessonVideo
                            .Where(x => x.SectionId == entity.SectionId && x.Enable == true && x.Index > entity.Index)
                            .ToListAsync();
                        if (lessonVideos.Any())
                        {
                            foreach (var item in lessonVideos)
                            {
                                var lessonVideo = await db.tbl_LessonVideo.SingleOrDefaultAsync(x => x.Id == item.Id);
                                lessonVideo.Index -= 1;
                                await db.SaveChangesAsync();
                            }
                        }
                        //Xóa hoàn thành
                        var lessonCompleteds = await db.tbl_LessonCompleted.Where(x => x.LessonVideoId == id)
                            .Select(x => x.Id).ToListAsync();
                        if (lessonCompleteds.Any())
                        {
                            foreach (var item in lessonCompleteds)
                            {
                                var lessonCompleted = await db.tbl_LessonCompleted.SingleOrDefaultAsync(x => x.Id == item);
                                lessonCompleted.Enable = false;
                                await db.SaveChangesAsync();
                            }
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
        public class ChangeLessonIndexModel
        {
            public List<ChangeLessonIndexItem> Items { get; set; }
        }
        public class ChangeLessonIndexItem
        {
            public int Id { get; set; }
            public int Index { get; set; }
        }
        public static async Task ChangeIndex(ChangeLessonIndexModel model)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!model.Items.Any())
                            throw new Exception("Không tìm thấy dữ liệu");
                        foreach (var item in model.Items)
                        {
                            var lessonVideo = await db.tbl_LessonVideo.SingleOrDefaultAsync(x => x.Id == item.Id);
                            if (lessonVideo == null)
                                throw new Exception("Không tìm thấy dữ liệu");
                            lessonVideo.Index = item.Index;
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
        public static async Task<List<LessonVideoModel>> GetBySection(int sectionId,tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var completeds = await db.tbl_LessonCompleted
                    .Where(x => x.SectionId == sectionId && x.UserId == user.UserInformationId)
                    .Select(x=>x.LessonVideoId).ToListAsync();
                var data = await db.tbl_LessonVideo.Where(x => x.SectionId == sectionId && x.Enable == true).OrderBy(x => x.Index).ToListAsync();
                var result = (from i in data
                             join completed in completeds on i.Id equals completed into pg
                             from completed in pg.DefaultIfEmpty()
                             select new LessonVideoModel
                             {
                                 ExamId = i.ExamId,
                                 Id = i.Id,
                                 Index = i.Index,
                                 Name = i.Name,
                                 SectionId = i.SectionId,
                                 Type = i.Type,
                                 TypeName = i.TypeName,
                                 VideoUrl = i.VideoUrl,
                                 isCompleted = completed.HasValue ? true : false,
                                 FileType = i.FileType,
                                 Minute = i.Minute
                             }).ToList();
                return result;
            }
        }
        /// <summary>
        /// Đối với bài kiểm tra sẽ lưu lại điểm
        /// </summary>
        /// <param name="lessonVideoId"></param>
        /// <param name="user"></param>
        /// <param name="examResultId"></param>
        /// <param name="totalPoint"></param>
        /// <returns></returns>
        public static async Task Completed(int lessonVideoId, tbl_UserInformation user, int examResultId = 0, double totalPoint = 0)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var lessonVideo = await db.tbl_LessonVideo.SingleOrDefaultAsync(x => x.Id == lessonVideoId);
                        if (lessonVideo == null)
                            throw new Exception("Không tìm thấy nội dung bài học");
                        ///Phải hoàn thành bài trước đó
                        if (lessonVideo.Index != 1)
                        {
                            var obligatoryCompleted = await db.tbl_LessonVideo
                                .Where(x => x.SectionId == lessonVideo.SectionId && x.Index == (lessonVideo.Index - 1) && x.Enable == true)
                                .FirstOrDefaultAsync();
                            if (obligatoryCompleted != null)
                            {
                                var validateIndex = await db.tbl_LessonCompleted.AnyAsync(x => x.LessonVideoId == obligatoryCompleted.Id && x.UserId == user.UserInformationId);
                                if (!validateIndex)
                                    throw new Exception("Chưa học đến bài này");
                            }
                        }
                        var validate = await db.tbl_LessonCompleted.AnyAsync(x => x.LessonVideoId == lessonVideoId && x.UserId == user.UserInformationId);
                        if (validate)
                            throw new Exception("Đã hoàn thành");
                        var model = new tbl_LessonCompleted
                        {
                            CreatedBy = user.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true,
                            LessonVideoId = lessonVideoId,
                            ExamResultId = examResultId,
                            TotalPoint = totalPoint,
                            ModifiedBy = user.FullName,
                            ModifiedOn = DateTime.Now,
                            SectionId = lessonVideo.SectionId,
                            UserId = user.UserInformationId
                        };
                        db.tbl_LessonCompleted.Add(model);
                        //await CompletedSection(model.SectionId.Value, user);
                        await db.SaveChangesAsync();
                        var section = await db.tbl_Section.SingleOrDefaultAsync(x => x.Id == model.SectionId.Value);
                        var sectionCompled = await db.tbl_SectionCompleted.AnyAsync(x => x.SectionId == section.Id && x.UserId == user.UserInformationId);
                        if (section != null && !sectionCompled)
                        {
                            bool completed = true;
                            var lessonVideos = await db.tbl_LessonVideo.Where(x => x.SectionId == section.Id && x.Enable == true).ToListAsync();
                            if (lessonVideos.Any())
                            {
                                foreach (var item in lessonVideos)
                                {
                                    var lessonCompleted = await db.tbl_LessonCompleted.AnyAsync(x => x.LessonVideoId == item.Id && x.UserId == user.UserInformationId && x.Enable == true);
                                    if (!lessonCompleted)
                                        completed = false;
                                }
                            }
                            if (completed)
                            {
                                db.tbl_SectionCompleted.Add(new tbl_SectionCompleted
                                {
                                    CreatedBy = user.FullName,
                                    CreatedOn = DateTime.Now,
                                    Enable = true,
                                    ModifiedBy = user.FullName,
                                    ModifiedOn = DateTime.Now,
                                    SectionId = model.SectionId.Value,
                                    VideoCourseId = section.VideoCourseId,
                                    UserId = user.UserInformationId
                                });
                                await db.SaveChangesAsync();
                            }
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
        //public static async Task CompletedSection(int sectionId,tbl_UserInformation user)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        var section = await db.tbl_Section.SingleOrDefaultAsync(x => x.Id == sectionId);
        //        if (section != null)
        //        {
        //            bool completed = true;
        //            var lessonVideos = await db.tbl_LessonVideo.Where(x => x.SectionId == section.Id).ToListAsync();
        //            if (lessonVideos.Any())
        //            {
        //                foreach (var item in lessonVideos)
        //                {
        //                    var lessonCompleted = await db.tbl_LessonCompleted.AnyAsync(x => x.LessonVideoId == item.Id && x.UserId == user.UserInformationId);
        //                    if (!lessonCompleted)
        //                        completed = false;
        //                }
        //            }
        //            if(completed)
        //            {
        //                db.tbl_SectionCompleted.Add(new tbl_SectionCompleted
        //                {
        //                    CreatedBy = user.FullName,
        //                    CreatedOn = DateTime.Now,
        //                    Enable = true,
        //                    ModifiedBy = user.FullName,
        //                    ModifiedOn = DateTime.Now,
        //                    SectionId = sectionId,
        //                    VideoCourseId = section.VideoCourseId,
        //                    UserId = user.UserInformationId
        //                });
        //                await db.SaveChangesAsync();
        //            }
        //        }
        //        ///Cấp chứng chỉnh khi hoàn thành khoá học cuối cùng
        //        var lastSection = await db.tbl_Section
        //            .Where(x => x.VideoCourseId == section.VideoCourseId).OrderByDescending(x => x.Index).FirstOrDefaultAsync();
        //        if (lastSection.Id == section.Id)
        //        {
                    
        //        }
        //    }
        //}
    }
}