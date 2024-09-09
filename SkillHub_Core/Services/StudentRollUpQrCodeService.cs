using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Services.Class.ClassService;

namespace LMSCore.Services
{
    public class StudentRollUpQrCodeService
    {
        public static async Task<tbl_StudentRollUpQrCode> Insert(int studentId, int scheduleId)
        {
            using (var db = new lmsDbContext())
            {
                tbl_UserInformation student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentId && x.Enable == true);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                tbl_Schedule schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == scheduleId);
                if (schedule == null)
                    throw new Exception("Không tìm thấy buổi học");
                tbl_StudentInClass studentInClass = await db.tbl_StudentInClass.SingleOrDefaultAsync(x => x.StudentId == student.UserInformationId && x.ClassId == schedule.ClassId && x.Enable == true);
                if (studentInClass == null)
                {
                    throw new Exception("Học viên không thuộc lớp này");
                }
                //DateTime timeNow = DateTime.Now;
                //if ((timeNow - schedule.EndTime.Value).Ticks < 0)
                //{
                //    throw new Exception("Buổi học đã kết thúc");
                //}
                tbl_StudentRollUpQrCode checkExists = await db.tbl_StudentRollUpQrCode.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ScheduleId == scheduleId && x.Enable == true);
                if (checkExists != null)
                    throw new Exception("Học viên đã tạo mã Qr cho buổi học này");
                string tempCode = student.UserInformationId.ToString() + "_" + schedule.Id.ToString();
                tbl_StudentRollUpQrCode entity = new tbl_StudentRollUpQrCode();
                entity.StudentId = studentId;
                entity.ScheduleId = scheduleId;
                entity.ClassId = schedule.ClassId.Value;
                entity.QrCode = AssetCRM.CreateQRCode(tempCode, AssetCRM.RemoveUnicode(tempCode + "_" + student.FullName));
                entity.CreatedBy = student.FullName;
                entity.Enable = true;
                entity.CreatedOn = DateTime.Now;
                db.tbl_StudentRollUpQrCode.Add(entity);
                await db.SaveChangesAsync();
                return entity;
            }
        }
        //public static async Task<tbl_StudentRollUpQrCode> GetQrCodeByStudent(int studentId, int scheduleId)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        tbl_StudentRollUpQrCode qrCode = await db.tbl_StudentRollUpQrCode.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ScheduleId == scheduleId && x.Enable == true);
        //        if (qrCode == null)
        //        {
        //            qrCode = await Insert(studentId, scheduleId);
        //        }
        //        qrCode.QrCode = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + qrCode.QrCode;
        //        return qrCode;
        //    }
        //}
        public static async Task<object> AttendanceByQrCode(StudentRollUpQrCodeCreate request)
        {
            using (var db = new lmsDbContext())
            {
                tbl_StudentRollUpQrCode qrCode = await db.tbl_StudentRollUpQrCode.SingleOrDefaultAsync(x => x.StudentId == request.StudentId && x.ScheduleId == request.ScheduleId && x.Enable == true);
                if (qrCode == null)
                {
                    throw new Exception("Mã Qr không hợp lệ");
                }
                tbl_RollUp checkRollUp = await db.tbl_RollUp.SingleOrDefaultAsync(x => x.ScheduleId == qrCode.ScheduleId && x.StudentId == qrCode.StudentId && x.Enable == true);
                if (checkRollUp != null)
                {
                    throw new Exception("Học viên đã điểm danh buổi học này");
                }

                RollUpCreate rollUpCreate = new RollUpCreate
                {
                    ScheduleId = qrCode.ScheduleId,
                    StudentId = qrCode.StudentId,
                    Status = 1,
                    LearningStatus = 1,
                };
                tbl_RollUp rollUp = new tbl_RollUp(rollUpCreate);
                var schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == rollUp.ScheduleId);
                rollUp.ClassId = schedule.ClassId;
                db.tbl_RollUp.Add(rollUp);
                await db.SaveChangesAsync();

                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == rollUp.StudentId);
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == rollUp.ClassId);
                var teacher = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == schedule.TeacherId);
                var result = new 
                {
                    ClassId = rollUp.ClassId,
                    FullName = student?.FullName,
                    UserCode = student?.UserCode,
                    LearningStatus = rollUp.LearningStatus,
                    LearningStatusName = rollUp.LearningStatusName,
                    Note = rollUp.Note,
                    ScheduleId = rollUp.ScheduleId,
                    Status = rollUp.Status,
                    StatusName = rollUp.StatusName,
                    StudentId = rollUp.StudentId,
                    TeacherId = teacher?.UserInformationId,
                    TeacherName = teacher?.FullName,
                    StartTime = schedule?.StartTime,
                    EndTime = schedule?.EndTime,
                    ClassName = _class?.Name,
                };
                
                return result;
            }
        }
    }
}