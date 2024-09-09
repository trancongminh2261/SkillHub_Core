using Hangfire;
using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using Microsoft.Extensions.Primitives;
using LMSCore.NotificationConfig;
using System.Collections;
namespace LMSCore.Services.Class
{
    public class ClassCronjob
    {
        public static async Task UpdateStatus()
        {
            using (var dbContext = new lmsDbContext())
            {
                var classes = await dbContext.tbl_Class.Where(x => x.Status != 3 && x.Enable == true && x.StartDay.HasValue && x.EndDay.HasValue).ToListAsync();
                if (classes.Any())
                {
                    foreach (var item in classes)
                    {
                        DateTime endDay = item.EndDay.Value.AddDays(1);
                        if (item.Status == 1 && DateTime.Now >= item.StartDay.Value.Date)
                        {
                            item.Status = 2;
                            item.StatusName = "Đang diễn ra";

                        }
                        else if (item.Status == 2 && DateTime.Now > endDay)
                        {
                            item.Status = 3;
                            item.StatusName = "Kết thúc";
                            //cập nhật trạng thái những học viên trong lớp
                            var studentInClasses = await dbContext.tbl_StudentInClass.Where(x => x.ClassId == item.Id && x.Enable == true).Select(x => new { x.StudentId, x.Id}).ToListAsync();
                            if (studentInClasses.Any())
                            {
                                foreach (var studentInClass in studentInClasses)
                                {
                                    var classIds = await dbContext.tbl_StudentInClass.Where(x => x.StudentId == studentInClass.StudentId && x.Enable == true && x.ClassId != item.Id).Select(x => x.ClassId).ToListAsync();
                                    var checkClass = await dbContext.tbl_Class.Where(x => classIds.Contains(x.Id) && x.Status != 3 && x.Enable == true).AnyAsync();
                                    var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentInClass.StudentId);
                                    if (!checkClass)
                                    {
                                        student.LearningStatus = 6;
                                        student.LearningStatusName = "Học xong";
                                        await dbContext.SaveChangesAsync();
                                    }
                                    //cập nhật trạng thái lộ trình học của học viên -> hoàn thành lộ trình có chương trình tương đương với chương trình của lớp học
                                    List<tbl_StudyRoute> studyRoute = await dbContext.tbl_StudyRoute
                                        .Where(x =>
                                            x.StudentId == studentInClass.StudentId
                                            && x.ProgramId == item.ProgramId
                                            && x.Status == (int)StudyRouteStatus.ChuaHoc
                                            && x.Status == (int)StudyRouteStatus.DangHoc
                                            && x.Enable == true).ToListAsync();
                                    if (studyRoute.Any())
                                    {
                                        studyRoute.ForEach(x =>
                                        {
                                            x.Status = (int)StudyRouteStatus.HoanThanh;
                                            x.StatusName = ListStudyRouteStatus().Where(y => y.Key == x.Status).FirstOrDefault().Value;
                                        });
                                        await dbContext.SaveChangesAsync();
                                    }
                                }
                                await ClassNotification.NotifySaleStudentCompletesTheClass(new ClassNotificationRequest.NotifySaleStudentCompletesTheClassRequest
                                {
                                    StudentInClassIds = studentInClasses.Select(x => x.Id).ToList(),
                                    CurrentUser = new tbl_UserInformation
                                    {
                                        FullName = "Tự động"
                                    }
                                });
                            }
                        }
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
