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
using System.Threading;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class StudentAssessmentService
    {
        public static async Task<tbl_StudentAssessment> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_StudentAssessment.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        /// <summary>
        /// Giáo viên đánh giá học viên trong lớp học 1 với 1
        /// Tự động cập nhật trạng thái lớp học và điểm danh giáo viên
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userLog"></param>
        /// <returns></returns>
        public static async Task InsertOrUpdate(StudentAssessmentCreate model, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == model.ScheduleId);
                    if (schedule == null)
                        throw new Exception("Không tìm thấy buổi học");
                    if (schedule.StartTime > DateTime.Now)
                        throw new Exception("Không thể tạo đánh giá, vui lòng đợi đến giờ");

                    var studentInClass = await db.tbl_StudentInClass.FirstOrDefaultAsync(x => x.ClassId == schedule.ClassId && x.Enable == true);
                    if (studentInClass == null)
                        throw new Exception("Không tìm thấy học viên");

                    var data = await db.tbl_StudentAssessment.FirstOrDefaultAsync(x => x.ScheduleId == schedule.Id && x.Enable == true);
                    if (data == null)
                    {
                        data = new tbl_StudentAssessment
                        {
                            ClassId = schedule.ClassId.Value,
                            CreatedBy = userLog.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true,
                            Listening = model.Listening,
                            ModifiedBy = userLog.FullName,
                            ModifiedOn = DateTime.Now,
                            Note = model.Note,
                            Reading = model.Reading,
                            ScheduleId = schedule.Id,
                            Speaking = model.Speaking,
                            StudentId = studentInClass.StudentId.Value,
                            Writing = model.Writing
                        };
                        db.tbl_StudentAssessment.Add(data);

                        schedule.TeacherAttendanceId = schedule.TeacherId;
                        schedule.StatusTutoring = 3;
                        schedule.StatusTutoringName = "Đã học";
                    }
                    else
                    {
                        data.Listening = model.Listening ?? data.Listening;
                        data.Note = model.Note ?? data.Note;
                        data.Reading = model.Reading ?? data.Reading;
                        data.Speaking = model.Speaking ?? data.Speaking;
                        data.Writing = model.Writing ?? data.Writing;
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static async Task<AppDomainResult> GetAll(StudentAssessmentSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) return new AppDomainResult { TotalRow = 0, Data = null };
                string sql = $"Get_StudentAssessment @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = N'{baseSearch.ClassId ?? 0}'";
                var data = await db.SqlQuery<Get_StudentAssessment>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_StudentAssessment(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}