/*using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Services
{
    public class StudentPointService
    {
        public class StudentPointModel
        {
            public int TransciptId { get; set; }
            public List<PointModel> Items { get; set; }
        }
        public class PointModel
        {
            public int StudentId { get; set; }
            public int ColumnTypeId { get; set; }
            public string Value { get; set; }
        }

        public static async Task PointEdit(StudentPointModel model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        var transcript = await db.tbl_Transcript.SingleOrDefaultAsync(x => x.Id == model.TransciptId);
                        if (transcript == null)
                            throw new Exception("Không tìm thấy bảng điểm");
                        if (!model.Items.Any())
                            throw new Exception("Không tìm thấy dữ liệu");
                        foreach (var item in model.Items)
                        {
                            var point = await db.tbl_StudentScoreboard
                                .Where(x => x.StudentId == item.StudentId && x.TransciptId == model.TransciptId).ToListAsync();
                            if (point != null)
                            {
                                *//*point.Listening = item.Listening ?? point.Listening;
                                point.Speaking = item.Speaking ?? point.Speaking;
                                point.Reading = item.Reading ?? point.Reading;
                                point.Writing = item.Writing ?? point.Writing;
                                point.Medium = item.Medium ?? point.Medium;
                                point.Note = item.Note ?? point.Note;*//*
                            }
                            else
                            {
                                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                                if (student == null)
                                    throw new Exception("Không tìm thấy học viên");
                                *//*db.tbl_Point.Add(
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
                                    });*//*
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
    }
}*/