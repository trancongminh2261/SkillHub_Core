﻿//using LMSCore.Areas.Models;
//using LMSCore.Areas.Request;
//using LMSCore.LMS;
//using LMSCore.Models;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using Microsoft.EntityFrameworkCore;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;
//using static LMSCore.Models.lmsEnum;

//namespace LMSCore.Services
//{
//    public class SeminarService
//    {
//        public static async Task<tbl_Seminar> GetById(int id)
//        {
//            using (var db = new lmsDbContext())
//            {
//                var data = await db.tbl_Seminar.SingleOrDefaultAsync(x => x.Id == id);
//                return data;
//            }
//        }
//        public static async Task<tbl_Seminar> Insert(SeminarCreate seminarCreate, tbl_UserInformation user)
//        {
//            using (var db = new lmsDbContext())
//            {
//                var model = new tbl_Seminar(seminarCreate);
//                model.CreatedBy = model.ModifiedBy = user.FullName;
//                db.tbl_Seminar.Add(model);
//                await db.SaveChangesAsync();

//                ///Gửi thông báo
//                Thread send = new Thread(async () =>
//                {
//                    string sTime = seminarCreate.StartTime.Value.ToString("HH:MM dd/MM/yyyy");
//                    string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
//                    string title = "Workshop";
//                    string content = $"Bạn có buổi workshop mới vào lúc {sTime}, thông báo từ {projectName}";
//                    await NotificationService.Send(
//                                        new tbl_Notification
//                                        {
//                                            UserId = seminarCreate.LeaderId,
//                                            Title = title,
//                                            Content = content
//                                        }, new tbl_UserInformation { FullName = "Tự động" });
//                    var students = await db.tbl_VideoCourseStudent.Where(x => x.VideoCourseId == seminarCreate.VideoCourseId)
//                        .Select(x => x.UserId).ToListAsync();
//                    if (students.Any())
//                    {
//                        foreach (var item in students)
//                        {
//                            await NotificationService.Send(
//                                          new tbl_Notification
//                                          {
//                                              UserId = item,
//                                              Title = title,
//                                              Content = content
//                                          }, new tbl_UserInformation { FullName = "Tự động" });
//                        }
//                    }
//                });
//                send.Start();

//                return model;
//            }
//        }
//        public static async Task Delete(int id)
//        {
//            using (var db = new lmsDbContext())
//            {
//                try
//                {
//                    var entity = await db.tbl_Seminar.SingleOrDefaultAsync(x => x.Id == id);
//                    if (entity == null)
//                        throw new Exception("Không tìm thấy dữ liệu");
//                    entity.Enable = false;
//                    await db.SaveChangesAsync();
//                }
//                catch (Exception e)
//                {
//                    throw e;
//                }
//            }
//        }
//        public static async Task<tbl_Seminar> Update(SeminarUpdate model, tbl_UserInformation user)
//        {
//            using (var db = new lmsDbContext())
//            {
//                try
//                {
//                    var entity = await db.tbl_Seminar.SingleOrDefaultAsync(x => x.Id == model.Id);
//                    entity.Name = model.Name ?? entity.Name;
//                    entity.Description = model.Description ?? entity.Description;
//                    entity.StartTime = model.StartTime ?? entity.StartTime;
//                    entity.EndTime = model.EndTime ?? entity.EndTime;
//                    entity.VideoCourseId = model.VideoCourseId ?? entity.VideoCourseId;
//                    entity.LeaderId = model.LeaderId ?? entity.LeaderId;
//                    entity.Member = model.Member ?? entity.Member;
//                    entity.Thumbnail = model.Thumbnail ?? entity.Thumbnail;
//                    entity.ModifiedOn = model.ModifiedOn;
//                    entity.ModifiedBy = user.FullName;
//                    await db.SaveChangesAsync();
//                    return entity;
//                }
//                catch (Exception e)
//                {
//                    throw e;
//                }
//            }
//        }
//        public static async Task<AppDomainResult> GetAll(SeminarSearch baseSearch, tbl_UserInformation user)
//        {
//            using (var db = new lmsDbContext())
//            {
//                if (baseSearch == null) return new AppDomainResult { TotalRow = 0, Data = null };
//                string sql = $"Get_Seminar @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
//                    $"@PageSize = {baseSearch.PageSize}," +
//                    $"@Name = N'{baseSearch.Name ?? ""}'," +
//                    $"@RoleId = {user.RoleId ?? 0}," +
//                    $"@MyUserId = {user.UserInformationId}," +
//                    $"@Status = {(baseSearch.Status == null ? 0 : ((int?)baseSearch.Status))}";
//                var data = await db.SqlQuery<Get_Seminar>(sql);
//                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
//                var totalRow = data[0].TotalRow;
//                var result = data.Select(i => new SeminarModel(i)).ToList();
//                return new AppDomainResult { TotalRow = totalRow, Data = result };
//            }
//        }
//    }
//}