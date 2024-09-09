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
    public class SectionService
    {
        public static async Task<tbl_Section> Insert(SectionCreate sectionCreate, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var videoCourse = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == sectionCreate.VideoCourseId);
                    if (videoCourse == null)
                        throw new Exception("Không tìm thấy khoá học");
                    var model = new tbl_Section(sectionCreate);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    var prevIndex = await db.tbl_Section
                        .Where(x => x.VideoCourseId == model.VideoCourseId)
                        .OrderByDescending(x => x.Index).FirstOrDefaultAsync();
                    model.Index = prevIndex == null ? 1 : (prevIndex.Index + 1);
                    db.tbl_Section.Add(model);
                    await db.SaveChangesAsync();
                    return model;
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
        }
        public static async Task<tbl_Section> Update(SectionUpdate model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_Section.SingleOrDefaultAsync(x => x.Id == model.Id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Name = model.Name ?? entity.Name;
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
                        var entity = await db.tbl_Section.SingleOrDefaultAsync(x => x.Id == id);
                        if (entity == null)
                            throw new Exception("Không tìm thấy dữ liệu");
                        entity.Enable = false;
                        await db.SaveChangesAsync();
                        //Cập nhật lại vi trí
                        var sections = await db.tbl_Section
                            .Where(x => x.VideoCourseId == entity.VideoCourseId && x.Enable == true && x.Index > entity.Index)
                            .ToListAsync();
                        if (sections.Any())
                        {
                            foreach (var item in sections)
                            {
                                var section = await db.tbl_Section.SingleOrDefaultAsync(x => x.Id == item.Id);
                                section.Index -= 1;
                                await db.SaveChangesAsync();
                            }
                        }
                        //Xóa hoàn thành
                        var completeds = await db.tbl_SectionCompleted.Where(x => x.SectionId == id)
                            .Select(x => x.Id).ToListAsync();
                        if (completeds.Any())
                        {
                            foreach (var item in completeds)
                            {
                                var sectionCompleted = await db.tbl_SectionCompleted.SingleOrDefaultAsync(x => x.Id == item);
                                sectionCompleted.Enable = false;
                                await db.SaveChangesAsync();
                            }
                        }
                        //Xóa lesson liên quan
                        var lessons = await db.tbl_LessonVideo.Where(x => x.SectionId == id)
                            .Select(x => x.Id).ToListAsync();
                        if (lessons.Any())
                        {
                            foreach (var item in lessons)
                            {
                                var lesson = await db.tbl_LessonVideo.SingleOrDefaultAsync(x => x.Id == item);
                                lesson.Enable = false;
                                await db.SaveChangesAsync();
                            }
                        }
                        var lessonCompleteds = await db.tbl_LessonCompleted.Where(x => x.SectionId == id)
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
        public static async Task ChangeIndex(ChangeIndexModel model)
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
                            var section = await db.tbl_Section.SingleOrDefaultAsync(x => x.Id == item.Id);
                            if (section == null)
                                throw new Exception("Không tìm thấy dữ liệu");
                            section.Index = item.Index;
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
        /// <summary>
        /// Phần trăm hoàn thành của khoá học
        /// </summary>
        /// <param name="videoCourseId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<double> GetComplete(int videoCourseId,tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                double result = 0;
                double completed = 0;
                double total = 0;
                var section = await db.tbl_Section.Where(x => x.VideoCourseId == videoCourseId && x.Enable == true)
                    .Select(x=>x.Id).ToListAsync();
                if (section.Any())
                {
                    foreach (var item in section)
                    {
                        total += await db.tbl_LessonVideo.CountAsync(x => x.SectionId == item && x.Enable == true);
                        completed += await db.tbl_LessonCompleted.CountAsync(x => x.SectionId == item && x.UserId == user.UserInformationId && x.Enable == true);
                    }
                    if (completed == 0)
                        return 0;
                    result = (completed * 100) / total;
                }
                return result;
            }
        }
        public static async Task<List<SectionModel>> GetByVideoCourse(int videoCourseId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    if (user.RoleId == ((int)RoleEnum.student))
                    {
                        var validate = await db.tbl_VideoCourseStudent.AnyAsync(x => x.VideoCourseId == videoCourseId && x.UserId == user.UserInformationId);
                        if (!validate)
                            throw new Exception("Bạn chưa đăng ký học");
                    }
                    var completeds = await db.tbl_SectionCompleted
                    .Where(x => x.VideoCourseId == videoCourseId && x.UserId == user.UserInformationId)
                    .Select(x => x.SectionId).ToListAsync();
                    var data = await db.tbl_Section.Where(x => x.VideoCourseId == videoCourseId && x.Enable == true).OrderBy(x => x.Index).ToListAsync();
                    var result = (from i in data
                                  join completed in completeds on i.Id equals completed into pg
                                  from completed in pg.DefaultIfEmpty()
                                  select new SectionModel
                                  {
                                      Id = i.Id,
                                      Index = i.Index,
                                      Name = i.Name,
                                      VideoCourseId = i.VideoCourseId,
                                      isCompleted = completed.HasValue ? true : false
                                  }).ToList();
                    return result;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}