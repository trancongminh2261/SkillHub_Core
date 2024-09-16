﻿using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.Models;
using LMSCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMS_Project.Services
{
    public class SectionService : DomainService
    {
        public SectionService(lmsDbContext dbContext) : base(dbContext) { }

        public async Task<tbl_Section> Insert(SectionCreate sectionCreate, tbl_UserInformation user)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    IHttpContextAccessor _httpContextAccessor;
                    var videoCourse = await dbContext.tbl_VideoCourse.SingleOrDefaultAsync(x => x.Id == sectionCreate.VideoCourseId);
                    if (videoCourse == null)
                        throw new Exception("Không tìm thấy khoá học");
                    var model = new tbl_Section(sectionCreate);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    var prevIndex = await dbContext.tbl_Section
                        .Where(x => x.VideoCourseId == model.VideoCourseId)
                        .OrderByDescending(x => x.Index).FirstOrDefaultAsync();
                    model.Index = prevIndex == null ? 1 : (prevIndex.Index + 1);
                    dbContext.tbl_Section.Add(model);
                    await dbContext.SaveChangesAsync();
                    var userCompleteds = await dbContext.tbl_VideoCourseStudent.Where(x => x.VideoCourseId == videoCourse.Id && x.Status == 3 && x.Enable == true)
                        .Select(x => x.UserId).ToListAsync();
                    if (userCompleteds.Any())
                    {
                        foreach (var item in userCompleteds)
                        {
                            var userCompleted = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item);
                            await LessonVideoService.CompletedVideoCourse(dbContext, videoCourse.Id, userCompleted);
                        }
                    }
                    tran.Commit();
                    return model;
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw e;
                }
            }
        }
        public async Task<tbl_Section> Update(SectionUpdate model, tbl_UserInformation user)
        {

                try
                {
                    var entity = await dbContext.tbl_Section.SingleOrDefaultAsync(x => x.Id == model.Id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Name = model.Name ?? entity.Name;
                    await dbContext.SaveChangesAsync();
                    return entity;
                }
                catch (Exception e)
                {
                    throw e;
                }
        }
        public async Task Delete(int id)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var entity = await dbContext.tbl_Section.SingleOrDefaultAsync(x => x.Id == id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Enable = false;
                    await dbContext.SaveChangesAsync();
                    //Cập nhật lại vi trí
                    var sections = await dbContext.tbl_Section
                        .Where(x => x.VideoCourseId == entity.VideoCourseId && x.Enable == true && x.Index > entity.Index)
                        .ToListAsync();
                    if (sections.Any())
                    {
                        foreach (var item in sections)
                        {
                            var section = await dbContext.tbl_Section.SingleOrDefaultAsync(x => x.Id == item.Id);
                            section.Index -= 1;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    //Xóa lesson liên quan
                    var lessons = await dbContext.tbl_LessonVideo.Where(x => x.SectionId == id)
                        .Select(x => x.Id).ToListAsync();
                    if (lessons.Any())
                    {
                        foreach (var item in lessons)
                        {
                            var lesson = await dbContext.tbl_LessonVideo.SingleOrDefaultAsync(x => x.Id == item);
                            lesson.Enable = false;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    var lessonCompleteds = await dbContext.tbl_LessonCompleted.Where(x => x.SectionId == id)
                        .Select(x => x.Id).ToListAsync();
                    if (lessonCompleteds.Any())
                    {
                        foreach (var item in lessonCompleteds)
                        {
                            var lessonCompleted = await dbContext.tbl_LessonCompleted.SingleOrDefaultAsync(x => x.Id == item);
                            lessonCompleted.Enable = false;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    var completeds = await dbContext.tbl_SectionCompleted.Where(x => x.SectionId == id)
                           .Select(x => x.Id).ToListAsync();
                    if (completeds.Any())
                    {
                        foreach (var item in completeds)
                        {
                            var sectionCompleted = await dbContext.tbl_SectionCompleted.SingleOrDefaultAsync(x => x.Id == item);
                            sectionCompleted.Enable = false;
                            await dbContext.SaveChangesAsync();

                            var userCompleted = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == sectionCompleted.UserId);
                            await LessonVideoService.CompletedVideoCourse(dbContext, sectionCompleted.VideoCourseId.Value, userCompleted);
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
        public class ChangeIndexModel
        {
            public List<ChangeIndexItem> Items { get; set; }
        }
        public class ChangeIndexItem
        {
            public int Id { get; set; }
            public int Index { get; set; }
        }
        public async Task ChangeIndex(ChangeIndexModel model)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!model.Items.Any())
                        throw new Exception("Không tìm thấy dữ liệu");
                    foreach (var item in model.Items)
                    {
                        var section = await dbContext.tbl_Section.SingleOrDefaultAsync(x => x.Id == item.Id);
                        if (section == null)
                            throw new Exception("Không tìm thấy dữ liệu");
                        section.Index = item.Index;
                        await dbContext.SaveChangesAsync();
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
        /// <summary>
        /// Phần trăm hoàn thành của khoá học
        /// </summary>
        /// <param name="videoCourseId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<double> GetComplete(int videoCourseId, tbl_UserInformation user)
        {
            double result = 0;
            double completed = 0;
            double total = 0;
            var section = await dbContext.tbl_Section.Where(x => x.VideoCourseId == videoCourseId && x.Enable == true)
                .Select(x => x.Id).Distinct().ToListAsync();
            if (section.Any())
            {
                foreach (var item in section)
                {
                    var lesson = await dbContext.tbl_LessonVideo.Where(x => x.SectionId == item && x.Enable == true)
                        .Select(x => x.Id).Distinct().ToListAsync();
                    total += lesson.Count();
                    completed += await dbContext.tbl_LessonCompleted
                        .Where(x => x.SectionId == item && x.UserId == user.UserInformationId && x.Enable == true && lesson.Contains(x.LessonVideoId ?? 0))
                        .Select(x => x.LessonVideoId).Distinct().CountAsync();
                }
                if (completed == 0)
                    return 0;
                result = (completed * 100) / total;
            }
            var videoCourseStudent = await dbContext.tbl_VideoCourseStudent
                .FirstOrDefaultAsync(x => x.VideoCourseId == videoCourseId && x.UserId == user.UserInformationId && x.Enable == true);
            if (videoCourseStudent != null)
                videoCourseStudent.CompletedPercent = Math.Round(result, 0);
            await dbContext.SaveChangesAsync();
            return Math.Round(result, 0);
        }
        public async Task<List<SectionModel>> GetByVideoCourse(int videoCourseId, tbl_UserInformation user)
        {
            try
            {
                if (user.RoleId == ((int)RoleEnum.student))
                {
                    var validate = await dbContext.tbl_VideoCourseStudent.AnyAsync(x => x.VideoCourseId == videoCourseId && x.UserId == user.UserInformationId);
                    if (!validate)
                        throw new Exception("Bạn chưa đăng ký học");
                }
                //var completeds = await dbContext.tbl_SectionCompleted
                //    .Where(x => x.VideoCourseId == videoCourseId && x.UserId == user.UserInformationId && x.Enable == true)
                //    .Select(x => x.SectionId).Distinct().ToListAsync();

                var data = await dbContext.tbl_Section.Where(x => x.VideoCourseId == videoCourseId && x.Enable == true).OrderBy(x => x.Index).ToListAsync();
                var result = (from i in data
                                  //join completed in completeds on i.Id equals completed into pg
                                  //from completed in pg.DefaultIfEmpty()
                              select new SectionModel
                              {
                                  Id = i.Id,
                                  Index = i.Index,
                                  Name = i.Name,
                                  Minute = Task.Run(() => GetMinute(i.Id)).Result,
                                  VideoCourseId = i.VideoCourseId,
                                  CompletedPercent = Task.Run(() => GetCompletedPercent(i.Id, user.UserInformationId)).Result,
                                  //isCompleted = completed.HasValue ? true : false
                                  isCompleted = Task.Run(() => GetCompleted(i.VideoCourseId.Value, i.Id, user.UserInformationId)).Result
                              }).ToList();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public async Task<double> GetCompletedPercent(int sectionId, int userId)
        {
            var lessons = await dbContext.tbl_LessonVideo.Where(x => x.SectionId == sectionId && x.Enable == true)
                   .Select(x => x.Id).ToListAsync();
            double total = lessons.Count();
            if (total == 0)
                return 0;
            double completed = await dbContext.tbl_LessonCompleted
                .CountAsync(x => x.SectionId == sectionId && x.Enable == true && x.UserId == userId && lessons.Contains(x.LessonVideoId ?? 0));
            return Math.Round(((completed / total) * 100), 0);

        }
        /// <summary>
        /// Hoàn thành video đối với vài trường hợp đặc biệt
        /// </summary>
        /// <param name="videoCourseId"></param>
        /// <param name="sectionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> GetCompleted(int videoCourseId, int sectionId, int userId)
        {
            var completed = await dbContext.tbl_SectionCompleted
                          .AnyAsync(x => x.SectionId == sectionId && x.UserId == userId && x.Enable == true);
            if (completed)
                return completed;

            var lessonVideos = await dbContext.tbl_LessonVideo.Where(x => x.SectionId == sectionId && x.Enable == true).Select(x => x.Id).ToListAsync();
            if (lessonVideos.Any())
            {
                foreach (var item in lessonVideos)
                {
                    var lessonCompleted = await dbContext.tbl_LessonCompleted.AnyAsync(x => x.LessonVideoId == item && x.UserId == userId && x.Enable == true);
                    if (!lessonCompleted)
                        return false;
                }
                dbContext.tbl_SectionCompleted.Add(new tbl_SectionCompleted
                {
                    CreatedBy = "Tự động",
                    CreatedOn = DateTime.Now,
                    Enable = true,
                    ModifiedBy = "Tự động",
                    ModifiedOn = DateTime.Now,
                    UserId = userId,
                    SectionId = sectionId,
                    VideoCourseId = videoCourseId
                });
                await dbContext.SaveChangesAsync();
                return true;
            }
            return false;


        }
        public async Task<int> GetMinute(int sectionId)
        {
            int result = 0;
            var lessons = await dbContext.tbl_LessonVideo.Where(x => x.SectionId == sectionId && x.Enable == true).Select(x => x.Minute).ToListAsync();
            if (lessons.Any())
            {
                result = lessons.Sum();
            }
            return result;

        }
    }
}