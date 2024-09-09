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
using LMSCore.Services.StudentInClass;

namespace LMSCore.Services.Class
{
    public class ClassService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public ClassService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        ///Tải tiết học khi tạo lớp
        public class LessonSearch
        {
            [Required(ErrorMessage = "Vui lòng chọn giáo trình")]
            public int? CurriculumId { get; set; }
            [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
            public DateTime? StartDay { get; set; }
            /// <summary>
            /// Phòng học
            /// </summary>
            public int RoomId { get; set; } = 0;
            public List<TimeModel> TimeModels { get; set; }
            [Required(ErrorMessage = "Vui lòng chọn Trung tâm")]
            public int? BranchId { get; set; }

        }
        public class TimeModel
        {
            /// <summary>
            /// Ngày trong tuần
            /// </summary>
            public int DayOfWeek { get; set; }
            /// <summary>
            /// Ca học
            /// </summary>
            public int StudyTimeId { get; set; }
            [Required(ErrorMessage = "Vui lòng chọn giáo viên")]
            public int? TeacherId { get; set; }
            public List<int> TutorIds { get; set; }
        }
        /// <summary>
        /// Tải lịch học khi tạo lớp
        /// </summary>
        /// <returns></returns>
        //public static async Task<List<ScheduleCreates>> GetLessonWhenCreate(LessonSearch itemModel)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        if (!itemModel.TimeModels.Any())
        //            throw new Exception("Không tìm thấy ngày học");
        //        var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumId);
        //        if (curriculum == null)
        //            throw new Exception("Không tìm thấy giáo trình");
        //        var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
        //        if (branch == null)
        //            throw new Exception("Không tìm thấy trung tâm");
        //        var teacher = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.TeacherId);
        //        if (teacher == null)
        //            throw new Exception("Không tìm thấy giáo viên");
        //        var branchIds = teacher.BranchIds.Split(',');
        //        if (!branchIds.Any(x => x == branch.Id.ToString()))
        //            throw new Exception("Giáo viên không thuộc trung tâm này");
        //        var teacherInProgram = await db.tbl_TeacherInProgram.FirstOrDefaultAsync(x => x.TeacherId == itemModel.TeacherId && x.ProgramId == curriculum.ProgramId && x.Enable == true);
        //        if (teacherInProgram == null)
        //            throw new Exception("Giáo viên không được phép dạy chương trình này");
        //        var room = new tbl_Room { Id = 0, Name = "" };
        //        if (itemModel.RoomId != 0)
        //        {
        //            room = await db.tbl_Room.SingleOrDefaultAsync(x => x.Id == itemModel.RoomId);
        //            if (room == null)
        //                throw new Exception("Không tìm thấy phòng");
        //        }
        //        var result = new List<ScheduleCreates>();
        //        var date = itemModel.StartDay.Value.AddDays(-1).Date;
        //        do
        //        {
        //            date = date.AddDays(1);
        //            var dayOff = await db.tbl_DayOff
        //                .AnyAsync(x => x.Enable == true && x.sDate <= date && x.eDate >= date);
        //            if (dayOff)
        //                continue;
        //            var teacherOff = await db.tbl_TeacherOff
        //                .AnyAsync(x => x.Enable == true && x.StartTime <= date && x.EndTime >= date && x.Status == 2 && x.TeacherId == itemModel.TeacherId);
        //            if (teacherOff)
        //                continue;
        //            foreach (var item in itemModel.TimeModels)
        //            {
        //                if (item.DayOfWeek == ((int)date.Date.DayOfWeek))
        //                {
        //                    var studyTime = await db.tbl_StudyTime.SingleOrDefaultAsync(x => x.Id == item.StudyTimeId);
        //                    if (studyTime == null)
        //                        continue;
        //                    var stimes = studyTime.StartTime.Split(':');
        //                    DateTime st = new DateTime(date.Year, date.Month, date.Day, int.Parse(stimes[0]), int.Parse(stimes[1]), 0);
        //                    var etimes = studyTime.EndTime.Split(':');
        //                    DateTime et = new DateTime(date.Year, date.Month, date.Day, int.Parse(etimes[0]), int.Parse(etimes[1]), 0);

        //                    var check = await db.tbl_Schedule
        //                        .AnyAsync(x => x.Enable == true && (x.RoomId == itemModel.RoomId || x.TeacherId == itemModel.TeacherId)
        //                            && x.StartTime < et && x.EndTime > st);
        //                    if (check)
        //                        continue;
        //                    result.Add(new ScheduleCreates
        //                    {
        //                        StartTime = st,
        //                        EndTime = et,
        //                        RoomId = room.Id,
        //                        RoomName = room.Name,
        //                        RoomCode = room.Code,
        //                        TeacherId = teacher.UserInformationId,
        //                        TeacherName = teacher.FullName,
        //                        TeacherCode = teacher.UserCode,
        //                        Note = ""
        //                    });
        //                }
        //            }
        //        } while (result.Count() < curriculum.Lesson);
        //        return result;
        //    }
        //}
        public static async Task<List<ScheduleCreates>> GetLessonWhenCreate(LessonSearch itemModel)
        {
            using (var db = new lmsDbContext())
            {
                if (!itemModel.TimeModels.Any())
                    throw new Exception("Không tìm thấy ngày học");
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumId);
                if (curriculum == null)
                    throw new Exception("Không tìm thấy giáo trình");
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                if (itemModel.TimeModels == null || itemModel.TimeModels.Count == 0)
                    throw new Exception("Vui lòng chọn ca học");
                var teacherIds = itemModel.TimeModels.Select(x => x.TeacherId).Distinct().ToList();
                var teachers = await db.tbl_UserInformation.Where(x => x.Enable == true && teacherIds.Contains(x.UserInformationId) && x.RoleId == (int)RoleEnum.teacher).ToListAsync();
                if (teachers == null || teachers.Count != teacherIds.Count())
                    throw new Exception("Giáo viên không tồn tại");

                var allTutorIds = itemModel.TimeModels.Where(x => x.TutorIds != null && x.TutorIds.Count > 0).SelectMany(x => x.TutorIds).Distinct().ToList();
                if (allTutorIds != null && allTutorIds.Count > 0)
                {
                    var tutors = await db.tbl_UserInformation.Where(x => allTutorIds.Contains(x.UserInformationId) && x.RoleId == (int)RoleEnum.tutor).ToListAsync();
                    if (tutors == null || tutors.Count != allTutorIds.Count())
                        throw new Exception("Trợ giảng không tồn tại");
                }

                var teacherInProgram = await db.tbl_TeacherInProgram
                        .Where(x => x.ProgramId == curriculum.ProgramId && x.Enable == true).ToListAsync();

                foreach (var teacher in teachers)
                {
                    var branchIds = teacher.BranchIds.Split(',');
                    if (!branchIds.Any(x => x == branch.Id.ToString()))
                        throw new Exception("Giáo viên không thuộc trung tâm này");
                    var isInProgram = teacherInProgram.Any(x => x.TeacherId == teacher.UserInformationId);
                    if (!isInProgram)
                        throw new Exception($"Giáo viên {teacher.FullName} không được phép dạy chương trình này");
                }

                var room = new tbl_Room { Id = 0, Name = "" };
                if (itemModel.RoomId != 0)
                {
                    room = await db.tbl_Room.SingleOrDefaultAsync(x => x.Id == itemModel.RoomId);
                    if (room == null)
                        throw new Exception("Không tìm thấy phòng");
                }
                var result = new List<ScheduleCreates>();
                var date = itemModel.StartDay.Value.AddDays(-1).Date;
                do
                {
                    date = date.AddDays(1);
                    var dayOff = await db.tbl_DayOff
                        .AnyAsync(x => x.Enable == true && x.sDate <= date && x.eDate >= date);
                    if (dayOff)
                        continue;
                    foreach (var item in itemModel.TimeModels)
                    {
                        if (item.DayOfWeek == (int)date.Date.DayOfWeek)
                        {
                            var studyTime = await db.tbl_StudyTime.SingleOrDefaultAsync(x => x.Id == item.StudyTimeId);
                            if (studyTime == null)
                                continue;

                            var tutorIds = item.TutorIds;
                            if (tutorIds == null) tutorIds = new List<int>();
                            //check xem giáo viên hoặc trợ giảng có xin nghỉ không
                            var teacherOff = await db.tbl_TeacherOff.Where(x => x.Enable == true && x.StartTime <= date && x.EndTime >= date && x.Status == 2 && x.TeacherId.HasValue).ToListAsync();
                            bool isTeacherOff = teacherOff.Any(x => x.TeacherId == item.TeacherId);
                            if (isTeacherOff)
                                continue;
                            bool isTutorOff = teacherOff.Any(x => tutorIds.Contains(x.TeacherId.Value));
                            if (isTutorOff)
                                continue;

                            var stimes = studyTime.StartTime.Split(':');
                            DateTime st = new DateTime(date.Year, date.Month, date.Day, int.Parse(stimes[0]), int.Parse(stimes[1]), 0);
                            var etimes = studyTime.EndTime.Split(':');
                            DateTime et = new DateTime(date.Year, date.Month, date.Day, int.Parse(etimes[0]), int.Parse(etimes[1]), 0);

                            var checkRoom = await db.tbl_Schedule
                                .AnyAsync(x => x.Enable == true && x.RoomId == itemModel.RoomId
                                    && x.StartTime < et && x.EndTime > st);

                            var schedules = await db.tbl_Schedule
                                .Where(x => x.Enable == true && x.StartTime < et && x.EndTime > st).ToListAsync();
                            bool checkTeacher = schedules.Any(x => x.TeacherId == item.TeacherId);
                            foreach (var tutorId in tutorIds.ToList())
                            {
                                if (schedules.Any(x => !string.IsNullOrEmpty(x.TutorIds) && x.TutorIds.Contains(tutorId.ToString())))
                                    tutorIds.Remove(tutorId);
                            }
                            var teacher = teachers.FirstOrDefault(x => x.UserInformationId == item.TeacherId);
                            result.Add(new ScheduleCreates
                            {
                                StartTime = st,
                                EndTime = et,
                                RoomId = checkRoom ? 0 : room.Id,
                                RoomName = checkRoom ? "" : room.Name,
                                RoomCode = checkRoom ? "" : room.Code,
                                TeacherId = checkTeacher ? 0 : teacher.UserInformationId,
                                TeacherName = checkTeacher ? "" : teacher.FullName,
                                TeacherCode = checkTeacher ? "" : teacher.UserCode,
                                TutorIds = string.Join(",", tutorIds),
                                TutorNames = Task.Run(() => UserInformation.GetTutorNames(tutorIds)).Result,
                                Note = ""
                            });

                        }
                    }
                } while (result.Count() < curriculum.Lesson);
                return result;
            }
        }
        public class TeacherModel
        {
            public int TeacherId { get; set; }
            public string TeacherName { get; set; }
            public string TeacherCode { get; set; }
            public string TeacherAvatar { get; set; }
            public string Extension { get; set; }
        }
        public class TeacherSearch
        {
            public int BranchId { get; set; }
            public int ProgramId { get; set; }
        }
        public static async Task<List<TeacherModel>> GetTeacherWhenCreate(TeacherSearch itemModel)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_TeacherWhenCreateClass @BranchId = {itemModel.BranchId}, @ProgramId = {itemModel.ProgramId}";
                var data = await db.SqlQuery<TeacherModel>(sql);
                return data;
            }
        }
        #region tối ưu chọn giáo viên tạo lịch
        //public class TeacherWhenCreateModel : TeacherModel
        //{
        //    /// <summary>
        //    /// true - phù hợp
        //    /// </summary>
        //    public bool Fit { get; set; } = true;
        //    /// <summary>
        //    /// Danh sách lịch bận
        //    /// </summary>
        //    public List<string> Conflicts { get; set; }
        //}
        //public class TeacherWhenCreateSearch
        //{
        //    public int BranchId { get; set; }
        //    public int ProgramId { get; set; }
        //    public int CurriculumId { get; set; }
        //    public DateTime? StartDay { get; set; }
        //    public List<TimeModel> TimeModels { get; set; }

        //}
        //public static async Task<List<TeacherModel>> GetTeacherWhenCreate(TeacherWhenCreateSearch itemModel)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        string sql = $"Get_TeacherWhenCreateClass @BranchId = {itemModel.BranchId}, @ProgramId = {itemModel.ProgramId}";
        //        var data = await db.SqlQuery<TeacherModel>(sql);
        //        var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumId);
        //        if (curriculum == null)
        //            throw new Exception("Không tìm thấy giáo trình");
        //        int totalLesson = curriculum.Lesson ?? 0;
        //        var schedules = new List<ScheduleCreates>();



        //        var result = new List<TeacherWhenCreateModel>();

        //        return data;
        //    }
        //}
        #endregion
        public class CheckModel
        {
            /// <summary>
            /// false - trùng lịch
            /// </summary>
            public bool Value { get; set; }
            public string Note { get; set; }
        }
        /// <summary>
        /// true - trùng
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="roomId"></param>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        public static async Task<CheckModel> CheckRoom(lmsDbContext db, int scheduleId, int roomId, DateTime stime, DateTime etime)
        {
            var schedule = await db.tbl_Schedule
                .FirstOrDefaultAsync(x => x.Enable == true && x.RoomId == roomId
                && x.Id != scheduleId && x.StartTime < etime && x.EndTime > stime
                    && x.StatusTutoring != 2  //Lớp dạy kèm bỏ qua buổi đã hủy
                    && x.StatusTutoring != 4
                    && x.StatusTutoring != 5
                    );
            var room = await db.tbl_Room.SingleOrDefaultAsync(x => x.Id == roomId);
            if (schedule != null)
            {
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                return new CheckModel
                {
                    Value = false,
                    Note = $"Phòng {room?.Name} trùng lịch với lớp {_class?.Name}, từ {schedule.StartTime} đến {schedule.EndTime} "
                };
            }
            return new CheckModel
            {
                Value = true,
                Note = ""
            };
        }
        public static async Task<CheckModel> CheckTeacher(lmsDbContext db, int scheduleId, int teacherId, DateTime stime, DateTime etime)
        {
            var schedule = await db.tbl_Schedule
                .FirstOrDefaultAsync(x => x.Enable == true && x.TeacherId == teacherId
                && x.Id != scheduleId && x.StartTime < etime && x.EndTime > stime
                    && x.StatusTutoring != 2 //Lớp dạy kèm bỏ qua buổi đã hủy
                    && x.StatusTutoring != 4
                    && x.StatusTutoring != 5
                    );
            var teacher = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == teacherId);
            if (schedule != null)
            {
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                if (_class != null && _class.Type == 3)// lớp dạy kèm kiểm tra giáo viên mở lịch
                {
                    var checkScheduleAvailable = await db.tbl_ScheduleAvailable
                        .AnyAsync(x => x.StartTime <= stime && x.EndTime >= etime && x.Enable == true);
                    if (!checkScheduleAvailable)
                        return new CheckModel
                        {
                            Value = false,
                            Note = "Giáo viên không mở lịch thời gian này "
                        };
                }
                return new CheckModel
                {
                    Value = false,
                    Note = $"Giáo viên {teacher?.FullName} trùng lịch với lớp {_class?.Name}, từ {schedule.StartTime} đến {schedule.EndTime} "
                };
            }
            return new CheckModel
            {
                Value = true,
                Note = ""
            };
        }
        public static async Task<CheckModel> CheckTutor(int scheduleId, int tutorId, DateTime stime, DateTime etime)
        {
            using (var db = new lmsDbContext())
            {
                var schedule = await db.tbl_Schedule
                    .FirstOrDefaultAsync(x => x.Enable == true && x.TutorIds.Contains(tutorId.ToString())
                    && x.Id != scheduleId && x.StartTime < etime && x.EndTime > stime
                        && x.StatusTutoring != 2 //Lớp dạy kèm bỏ qua buổi đã hủy
                        && x.StatusTutoring != 4
                        && x.StatusTutoring != 5
                        );
                var teacher = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == tutorId);
                if (schedule != null)
                {
                    var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == schedule.ClassId);
                    if (_class != null)
                        return new CheckModel
                        {
                            Value = false,
                            Note = $"Giáo viên {teacher?.FullName} trùng lịch với lớp {_class?.Name}, từ {schedule.StartTime} đến {schedule.EndTime}"
                        };
                }
                return new CheckModel
                {
                    Value = true,
                    Note = ""
                };
            }
        }
        public class TeacherAvailableModel
        {
            public int TeacherId { get; set; }
            public string TeacherName { get; set; }
            public string TeacherCode { get; set; }
            public bool Fit { get; set; }
            public string Note { get; set; }
        }
        public class TutorAvailableModel
        {
            public int TutorId { get; set; }
            public string TutorName { get; set; }
            public string TutorCode { get; set; }
            public bool Fit { get; set; }
            public string Note { get; set; }
            public double? Salary { get; set; }
        }
        public class TeacherAvailableSearch
        {
            public int ScheduleId { get; set; } = 0;
            [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
            public int? BranchId { get; set; }
            [Required(ErrorMessage = "Vui lòng chọn giáo trình")]
            public int? CurriculumId { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
        public static async Task<List<TeacherAvailableModel>> GetTeacherAvailable(TeacherAvailableSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == baseSearch.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == baseSearch.CurriculumId);
                if (curriculum == null)
                    throw new Exception("Không tìm thấy giáo trình");
                var result = new List<TeacherAvailableModel>();
                var teachers = await db.tbl_TeacherInProgram.Where(x => x.ProgramId == curriculum.ProgramId && x.Enable == true).ToListAsync();
                if (teachers.Any())
                {
                    foreach (var item in teachers)
                    {
                        var teacher = await db.tbl_UserInformation
                            .FirstOrDefaultAsync(x => x.UserInformationId == item.TeacherId && x.Enable == true);
                        if (teacher != null)
                        {
                            var checkBranch = teacher.BranchIds.Split(',').Any(x => x == baseSearch.BranchId.ToString());
                            if (checkBranch)
                            {
                                var check = await CheckTeacher(db, baseSearch.ScheduleId, item.TeacherId.Value, baseSearch.StartTime, baseSearch.EndTime);
                                result.Add(new TeacherAvailableModel
                                {
                                    Fit = check.Value,
                                    Note = check.Note,
                                    TeacherCode = teacher.UserCode,
                                    TeacherId = teacher.UserInformationId,
                                    TeacherName = teacher.FullName
                                });
                            }
                        }
                    }
                }
                return result;
            }
        }

        public static async Task<List<TutorAvailableModel>> GetTutorAvailable(TeacherAvailableSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == baseSearch.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == baseSearch.CurriculumId);
                if (curriculum == null)
                    throw new Exception("Không tìm thấy giáo trình");
                var result = new List<TutorAvailableModel>();
                var tutors = await db.tbl_TutorInProgram.Where(x => x.ProgramId == curriculum.ProgramId && x.Enable == true).ToListAsync();
                if (tutors.Any())
                {
                    foreach (var item in tutors)
                    {
                        var teacher = await db.tbl_UserInformation
                            .FirstOrDefaultAsync(x => x.UserInformationId == item.TutorId && x.Enable == true);
                        if (teacher != null)
                        {
                            var checkBranch = teacher.BranchIds.Split(',').Any(x => x == baseSearch.BranchId.ToString());
                            if (checkBranch)
                            {
                                var check = await CheckTutor(baseSearch.ScheduleId, item.TutorId.Value, baseSearch.StartTime, baseSearch.EndTime);
                                var tmp = new TutorAvailableModel
                                {
                                    Fit = check.Value,
                                    Note = check.Note,
                                    TutorCode = teacher.UserCode,
                                    TutorId = teacher.UserInformationId,
                                    TutorName = teacher.FullName
                                };

                                if (check.Value)
                                {
                                    //lấy lương cấu hình của trợ giảng viên
                                    var salary = await db.tbl_TutorSalary.FirstOrDefaultAsync(x => x.Enable == true && x.TutorId == teacher.UserInformationId);
                                    if (salary != null)
                                    {
                                        var salaryConfig = await db.tbl_TutorSalaryConfig.FirstOrDefaultAsync(x => x.Id == salary.TutorSalaryConfigId);
                                        if (salaryConfig != null)
                                            tmp.Salary = salaryConfig.Salary;
                                    }
                                }
                                result.Add(tmp);
                            }
                        }
                    }
                }
                return result;
            }
        }

        public class TeacherWorkingDay
        {
            public int ClassId;
            public int TeacherId;
            public string TeacherName;
            public int Total;
            public int WorkingDays;
            public int DayOff;
            public string Content;
            public string url;
            public string urlEmail;
        }

        public class RoomAvailableModel
        {
            public int RoomId { get; set; }
            public string RoomName { get; set; }
            public string RoomCode { get; set; }
            public bool Fit { get; set; }
            public string Note { get; set; }
        }
        public class RoomAvailableSearch
        {
            public int ScheduleId { get; set; } = 0;
            [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
            public int? BranchId { get; set; }
            [Required(ErrorMessage = "Vui lòng chọn giáo trình")]
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
        public static async Task<List<RoomAvailableModel>> GetRoomAvailable(RoomAvailableSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == baseSearch.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                var rooms = await db.tbl_Room.Where(x => x.BranchId == baseSearch.BranchId && x.Enable == true).ToListAsync();
                var result = new List<RoomAvailableModel>();
                if (rooms.Any())
                {
                    foreach (var item in rooms)
                    {
                        var check = await CheckRoom(db, baseSearch.ScheduleId, item.Id, baseSearch.StartTime, baseSearch.EndTime);
                        result.Add(new RoomAvailableModel
                        {
                            Fit = check.Value,
                            Note = check.Note,
                            RoomCode = item.Code,
                            RoomId = item.Id,
                            RoomName = item.Name
                        });
                    }
                }
                return result;
            }
        }
        public static async Task<tbl_Class> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == id);
                var curiculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == data.CurriculumId);
                if (data != null)
                {
                    data.Time = curiculum.Time ?? 0;
                    data.Room = await GetRoom(data.Id);
                }
                return data;
            }
        }
        public static async Task<List<GetRoom>> GetRoom(int classId)
        {
            using (var db = new lmsDbContext())
            {
                var listRoom = await db.tbl_Schedule.Where(x => x.ClassId == classId && x.Enable == true).Select(x => "," + x.RoomId + ",").Distinct().ToListAsync();
                var data = new List<GetRoom>();
                if (listRoom.Any())
                    data = await db.tbl_Room.Where(x => listRoom.Contains("," + x.Id + ",")).Select(x => new GetRoom()
                    {
                        RoomId = x.Id,
                        RoomName = x.Name
                    }).ToListAsync();
                return data;
            }
        }
        public static async Task<tbl_Class> Insert(ClassCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var model = new tbl_Class(itemModel);
                        var branch = await BranchService.GetById(model.BranchId.Value);
                        if (branch == null)
                            throw new Exception("Không tìm thấy trung tâm");
                        var program = await ProgramService.GetById(model.ProgramId.Value);
                        if (program == null)
                            throw new Exception("Không tìm thấy chương trình");
                        var curriculum = await CurriculumService.GetById(model.CurriculumId.Value);
                        if (curriculum == null)
                            throw new Exception("Không tìm thấy giáo trình");
                        //var scoreboardTemplate = await db.tbl_ScoreboardTemplate.Where(x => x.Id == itemModel.ScoreboardTemplateId && x.Enable == true).ToListAsync();
                        //if (scoreboardTemplate == null)
                        //    throw new Exception("Không tìm thấy mẫu bảng điểm");
                        if (itemModel.ClassRegistrationIds.Any())
                        {
                            model.GradeId = program.GradeId;
                            if (itemModel.ClassRegistrationIds.Count > itemModel.MaxQuantity)
                                throw new Exception("Số lượng học viên tối đa của lớp phải lớn hơn số học viên được thêm vào lớp");
                        }
                        else if (!itemModel.GradeId.HasValue)
                            throw new Exception("Vui lòng chọn chuyên ngành");
                        var grade = await GradeService.GetById(model.GradeId.Value);
                        if (grade == null)
                            throw new Exception("Không tìm thấy chuyên môn");

                        var academmic = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == itemModel.AcademicId);
                        //if (itemModel.CertificateTemplateId.HasValue)
                        //{
                        //    bool hasCertificateTemplate = await db.tbl_CertificateTemplate.AnyAsync(x => x.Id == itemModel.CertificateTemplateId);
                        //    if (!hasCertificateTemplate)
                        //        throw new Exception("Không tìm thấy mẫu chứng chỉ");
                        //}
                        model.CreatedBy = model.ModifiedBy = user.FullName;
                        db.tbl_Class.Add(model);
                        await db.SaveChangesAsync();

                        // Thêm học viên chờ xếp lớp vào lớp học
                        if (itemModel.ClassRegistrationIds.Any())
                        {
                            var classRegistrationIds = itemModel.ClassRegistrationIds.Select(x => "," + x + ",").ToList();

                            var classRegistration = await db.tbl_ClassRegistration.Where(x => x.Enable == true
                            && classRegistrationIds.Contains("," + x.Id + ",")).ToListAsync();

                            foreach (var item in classRegistration)
                            {
                                if (item.ProgramId != model.ProgramId)
                                    throw new Exception("Học viên không thuộc chương trình học của lớp học này");
                                if (classRegistration.Count(x => x.StudentId == item.StudentId) > 1)
                                {
                                    var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                                    throw new Exception("Bạn đã chọn học viên " + student?.FullName + " nhiều lần. Vui lòng chọn lại");
                                }
                                // cập nhật trạng thái đã xếp lớp
                                item.Status = (int)lmsEnum.ClassRegistrationStatus.Classed;
                                item.StatusName = lmsEnum.ClassRegistrationStatusName(item.Status);
                                // Thêm học viên vào lớp
                                await StudentInClassService.InsertForRegistration(new StudentInClassCreate()
                                {
                                    ClassId = model.Id,
                                    StudentId = item.StudentId,
                                    Type = 1, // Loại học viên chính thức
                                }, user, model);
                            }
                            await db.SaveChangesAsync();
                        }

                        //var scoreColumnTemplate = await db.tbl_ScoreColumnTemplate.Where(x => x.Enable == true && x.ScoreBoardTemplateId == itemModel.ScoreboardTemplateId).ToListAsync();

                        //tạo lớp xong lưu cột điểm mẫu vào cột điểm của lớp
                        //foreach (var item in scoreColumnTemplate)
                        //{
                        //    var scoreColumn = new tbl_ScoreColumn()
                        //    {
                        //        ClassId = model.Id,
                        //        Name = item.Name,
                        //        Factor = item.Factor,
                        //        Index = item.Index,
                        //        Type = item.Type,
                        //        TypeName = item.TypeName,
                        //        CreatedBy = user.FullName,
                        //        CreatedOn = DateTime.Now,
                        //        Enable = true,
                        //        ModifiedBy = user.FullName,
                        //        ModifiedOn = DateTime.Now,
                        //    };
                        //    db.tbl_ScoreColumn.Add(scoreColumn);
                        //}
                        //await db.SaveChangesAsync();
                        //Tạo giáo trình cho lớp học
                        if (itemModel.CurriculumId != null)
                        {
                            var curriculumInClass = new tbl_CurriculumInClass
                            {
                                ClassId = model.Id,
                                CurriculumId = model.CurriculumId,
                                Name = curriculum.Name,
                                IsComplete = false,
                                CompletePercent = 0,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true
                            };
                            db.tbl_CurriculumInClass.Add(curriculumInClass);
                            await db.SaveChangesAsync();
                            // Thêm bài tập của giáo trình vào lớp học
                            var homeworkSequenceConfigInCurriculum = await db.tbl_HomeworkSequenceConfigInCurriculum.FirstOrDefaultAsync(x => x.CurriculumId == itemModel.CurriculumId && x.Enable == true);
                            if (homeworkSequenceConfigInCurriculum != null)
                            {
                                var homeworkSequenceConfigInClass = new tbl_HomeworkSequenceConfigInClass
                                {
                                    ClassId = model.Id,
                                    IsAllow = homeworkSequenceConfigInCurriculum.IsAllow ?? false,
                                    Enable = true,
                                    ModifiedBy = user.FullName,
                                    CreatedBy = user.FullName,
                                    CreatedOn = DateTime.Now,
                                    ModifiedOn = DateTime.Now
                                };
                                await db.tbl_HomeworkSequenceConfigInClass.AddAsync(homeworkSequenceConfigInClass);
                                await db.SaveChangesAsync();
                            }

                            var homeworkInCurriculum = await db.tbl_HomeworkInCurriculum.Where(x => x.Enable == true && x.CurriculumId == itemModel.CurriculumId).ToListAsync();
                            if (homeworkInCurriculum.Count != 0)
                            {
                                foreach (var hc in homeworkInCurriculum)
                                {
                                    var homework = new tbl_Homework
                                    {
                                        ClassId = model.Id,
                                        Name = hc.Name,
                                        IeltsExamId = hc.IeltsExamId,
                                        FromDate = hc.FromDate ?? null,
                                        ToDate = hc.ToDate ?? null,
                                        Note = hc.Note,
                                        TeacherId = null,
                                        Type = hc.Type,
                                        TypeName = hc.TypeName,
                                        HomeworkContent = hc.HomeworkContent,
                                        CutoffScore = hc.CutoffScore,
                                        SessionNumber = hc.SessionNumber,
                                        Index = hc.Index,
                                        Enable = true,
                                        ModifiedBy = user.FullName,
                                        CreatedBy = user.FullName,
                                        CreatedOn = DateTime.Now,
                                        ModifiedOn = DateTime.Now,
                                    };
                                    await db.tbl_Homework.AddAsync(homework);
                                    await db.SaveChangesAsync();

                                    var homeworkFielInCurriculum = await db.tbl_HomeworkFileInCurriculum
                                        .Where(x => x.Enable == true && x.HomeworkInCurriculumId == hc.Id && !string.IsNullOrEmpty(x.File))
                                        .ToListAsync();
                                    if (homeworkFielInCurriculum.Count != 0)
                                    {
                                        foreach (var h in homeworkFielInCurriculum)
                                        {
                                            var file = new tbl_HomeworkFile
                                            {
                                                UserId = user.UserInformationId,
                                                File = h.File,
                                                Type = HomeworkFileType.GiveHomework,
                                                TypeName = HomeworkFileTypeName(HomeworkFileType.GiveHomework),
                                                HomeworkId = homework.Id,
                                                Enable = true,
                                                ModifiedBy = user.FullName,
                                                CreatedBy = user.FullName,
                                                CreatedOn = DateTime.Now,
                                                ModifiedOn = DateTime.Now,
                                            };
                                            await db.tbl_HomeworkFile.AddAsync(file);
                                        }
                                        await db.SaveChangesAsync();
                                    }
                                }
                            }
                            //Thêm các chương của giáo trình
                            var curriculumDetails = await db.tbl_CurriculumDetail.Where(x => x.Enable == true && x.CurriculumId == curriculumInClass.CurriculumId).ToListAsync();
                            if (curriculumDetails.Any())
                            {
                                foreach (var itemCurDetail in curriculumDetails)
                                {
                                    var curDetailInClass = new tbl_CurriculumDetailInClass
                                    {
                                        CurriculumIdInClass = curriculumInClass.Id,
                                        CurriculumDetailId = itemCurDetail.Id,
                                        IsComplete = false,
                                        Name = itemCurDetail.Name,
                                        Index = itemCurDetail.Index,
                                        CompletePercent = 0,
                                        Enable = true,
                                        CreatedBy = user.FullName,
                                        CreatedOn = DateTime.Now,
                                        ModifiedBy = user.FullName,
                                        ModifiedOn = DateTime.Now,
                                    };
                                    db.tbl_CurriculumDetailInClass.Add(curDetailInClass);
                                    await db.SaveChangesAsync();
                                    //Thêm cái file vào chương
                                    var file = await db.tbl_FileInCurriculumDetail.Where(x => x.Enable == true && x.CurriculumDetailId == itemCurDetail.Id).ToListAsync();
                                    if (file.Any())
                                    {
                                        foreach (var itemFile in file)
                                        {
                                            var fileCreate = new tbl_FileCurriculumInClass
                                            {
                                                CurriculumDetailId = curDetailInClass.Id,
                                                FileCurriculumId = itemFile.Id,
                                                IsComplete = false,
                                                IsHide = false,
                                                FileName = itemFile.FileName,
                                                FileUrl = itemFile.FileUrl,
                                                Index = itemFile.Index,
                                                ClassId = model.Id,
                                                Enable = true,
                                                CreatedBy = user.FullName,
                                                CreatedOn = DateTime.Now,
                                                ModifiedBy = user.FullName,
                                                ModifiedOn = DateTime.Now
                                            };
                                            db.tbl_FileCurriculumInClass.Add(fileCreate);
                                            await db.SaveChangesAsync();
                                        }
                                    }
                                }
                            }
                        }

                        if (model.IsMonthly && model.StartDay.HasValue)
                        {
                            //Hẹn giờ tính nợ hằng tháng cho học viên đăng ký lớp này
                            var startDate = model.StartDay.Value;
                            TimeSpan timeSpanDelta = new TimeSpan(startDate.Ticks - DateTime.Now.Ticks);

                            BackgroundJob.Schedule(() => ScheduleMonthlyDebt(model.Id), timeSpanDelta);
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
        }

        public static async Task ScheduleMonthlyDebt(int classId)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    //lấy lớp học
                    var item = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == classId);

                    if (item.Status == 3)
                        return;

                    //Lấy danh sách học sinh lớp học
                    var studentInClass = await db.tbl_StudentInClass.Where(x => x.ClassId == item.Id && x.Enable == true).ToListAsync();
                    var now = DateTime.Now;
                    if (studentInClass != null && studentInClass.Count > 0)
                    {
                        //Lấy danh sách đã mua lớp học
                        var billDetail = await db.tbl_BillDetail.Where(x => x.ClassId == item.Id && x.Enable == true).ToListAsync();
                        //Thêm record đóng tiền cho các học viên
                        foreach (var student in studentInClass)
                        {
                            //kiểm tra xem học viên có trả trước gói này chưa
                            var studentMonthlyDebt = await db.tbl_StudentMonthlyDebt
                                .AnyAsync(x => x.Enable == true && x.ClassId == item.Id && x.StudentId == student.StudentId
                                && x.Month.HasValue && x.Month.Value.Month == DateTime.Now.Month);
                            if (studentMonthlyDebt)
                                continue;
                            var detail = billDetail.FirstOrDefault(x => x.StudentId == student.StudentId && x.MonthAvailable > 0);
                            bool isPayment = detail != null;
                            if (detail != null) detail.MonthAvailable--;
                            tbl_StudentMonthlyDebt smd = new tbl_StudentMonthlyDebt
                            {
                                ClassId = item.Id,
                                StudentId = student.StudentId,
                                Price = item.Price,
                                Month = DateTime.Now,
                                CreatedOn = DateTime.Now,
                                IsPaymentDone = isPayment,
                                Enable = true,
                                BranchId = item.BranchId
                            };
                            db.tbl_StudentMonthlyDebt.Add(smd);
                            await db.SaveChangesAsync();
                        }
                    }

                    //Hẹn giờ 1 tháng sau lại chạy hàm này
                    var startDate = now.AddMonths(1).Date;
                    TimeSpan timeSpanDelta = new TimeSpan(startDate.Ticks - now.Ticks);

                    BackgroundJob.Schedule(() => ScheduleMonthlyDebt(item.Id), timeSpanDelta);
                    tran.Commit();
                }
            }
        }
        public static async Task<tbl_Class> Update(ClassUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy lớp này");
                //cập nhật trạng thái những học viên trong lớp
                if (entity.Status == 3 && itemModel.Status.HasValue && itemModel.Status != 3)
                {
                    var studentIds = await db.tbl_StudentInClass.Where(x => x.ClassId == entity.Id && x.Enable == true).Select(x => x.StudentId).ToListAsync();
                    if (studentIds.Any())
                    {
                        foreach (var s in studentIds)
                        {
                            var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == s);
                            student.LearningStatus = 5;
                            student.LearningStatusName = "Đang học";
                            await db.SaveChangesAsync();
                            //cập nhật trạng thái "Đang học" lộ trình học của học viên -> lộ trình có chương trình tương đương với chương trình của lớp học
                            List<tbl_StudyRoute> studyRoute = await db.tbl_StudyRoute
                                       .Where(x =>
                                           x.StudentId == s
                                           && x.ProgramId == entity.ProgramId
                                           && x.Status == (int)StudyRouteStatus.ChuaHoc
                                           && x.Enable == true).ToListAsync();
                            if (studyRoute.Any())
                            {
                                studyRoute.ForEach(x =>
                                {
                                    x.Status = (int)StudyRouteStatus.DangHoc;
                                    x.StatusName = ListStudyRouteStatus().Where(y => y.Key == x.Status).FirstOrDefault().Value;
                                });
                                await db.SaveChangesAsync();
                            }
                        }
                    }

                    if (entity.IsMonthly && entity.StartDay.HasValue)
                    {
                        //Hẹn giờ tính nợ hằng tháng cho học viên đăng ký lớp này
                        var now = DateTime.Now;
                        var startDate = now.AddMonths(1).AddDays(entity.StartDay.Value.Day); //Ngày thu tiền của lớp trong tháng tới
                        TimeSpan timeSpanDelta = new TimeSpan(startDate.Date.Ticks - now.Ticks);
                        if (now < entity.StartDay.Value)
                            timeSpanDelta = new TimeSpan(entity.StartDay.Value.Ticks - now.Ticks);

                        BackgroundJob.Schedule(() => ScheduleMonthlyDebt(entity.Id), timeSpanDelta);
                    }
                }
                else if (itemModel.Status == 3)
                {
                    var studentInClasses = await db.tbl_StudentInClass.Where(x => x.ClassId == entity.Id && x.Enable == true).Select(x => new { x.StudentId, x.Id }).ToListAsync();
                    if (studentInClasses.Any())
                    {
                        foreach (var studentInClass in studentInClasses)
                        {
                            var classIds = await db.tbl_StudentInClass.Where(x => x.StudentId == studentInClass.StudentId && x.Enable == true && x.ClassId != entity.Id).Select(x => x.ClassId).ToListAsync();
                            var checkClass = await db.tbl_Class.Where(x => classIds.Contains(x.Id) && x.Status != 3 && x.Enable == true).AnyAsync();
                            if (!checkClass)
                            {
                                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentInClass.StudentId);
                                student.LearningStatus = 6;
                                student.LearningStatusName = "Học xong";
                                await db.SaveChangesAsync();
                            }
                            //cập nhật trạng thái "Hoàn thành" lộ trình học của học viên -> lộ trình có chương trình tương đương với chương trình của lớp học
                            List<tbl_StudyRoute> studyRoute = await db.tbl_StudyRoute
                                .Where(x =>
                                    x.StudentId == studentInClass.StudentId
                                    && x.ProgramId == entity.ProgramId
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
                                await db.SaveChangesAsync();
                            }

                        }
                        BackgroundJob.Schedule(() => ClassNotification.NotifySaleStudentCompletesTheClass(new ClassNotificationRequest.NotifySaleStudentCompletesTheClassRequest
                        {
                            StudentInClassIds = studentInClasses.Select(x => x.Id).ToList(),
                            CurrentUser = new tbl_UserInformation
                            {
                                FullName = "Tự động"
                            }
                        }), TimeSpan.FromSeconds(2));
                    }

                }

                entity.Name = itemModel.Name ?? entity.Name;
                entity.Thumbnail = itemModel.Thumbnail ?? entity.Thumbnail;
                entity.Price = itemModel.Price ?? entity.Price;
                entity.Status = itemModel.Status ?? entity.Status;
                entity.StatusName = itemModel.StatusName ?? entity.StatusName;
                entity.AcademicId = itemModel.AcademicId ?? entity.AcademicId;
                entity.TeacherId = itemModel.TeacherId ?? entity.TeacherId;
                entity.MaxQuantity = itemModel.MaxQuantity ?? entity.MaxQuantity;
                entity.CertificateTemplateId = itemModel.CertificateTemplateId ?? entity.CertificateTemplateId;
                entity.EstimatedDay = itemModel.EstimatedDay ?? entity.EstimatedDay;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        /// <summary>
        /// true - lớp đang học (có học viện trong lớp)
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CheckExistStudentInClass(int id)
        {
            using (var db = new lmsDbContext())
            {
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == id);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");
                var studentInClass = await db.tbl_StudentInClass.AnyAsync(x => x.ClassId == id && x.Enable == true);
                return studentInClass;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == id);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");
                _class.Enable = false;
                await db.SaveChangesAsync();
                var schedules = await db.tbl_Schedule.Where(x => x.ClassId == id && x.Enable == true).ToListAsync();
                if (schedules.Any())
                {
                    foreach (var item in schedules)
                    {
                        item.Enable = false;
                    }
                    await db.SaveChangesAsync();
                }
            }
        }
        public class ClassResult : AppDomainResult
        {
            public int Upcoming { get; set; }
            public int Opening { get; set; }
            public int Closing { get; set; }
        }

        public static async Task<int> Upcoming(lmsDbContext db, ClassSearch baseSearch, tbl_UserInformation user)
        {
            var studentInClass = await db.tbl_StudentInClass
                .Where(x => x.StudentId == user.UserInformationId && x.Enable == true)
                .Select(x => x.ClassId).ToListAsync();

            var teacherInClass = await db.tbl_Schedule
               .Where(x => x.TeacherId == user.UserInformationId && x.Enable == true)
               .Select(x => x.ClassId).Distinct().ToListAsync();

            var types = string.IsNullOrEmpty(baseSearch.Types) ? new List<string>() : baseSearch.Types.Split(',').ToList();
            var branchIds = string.IsNullOrEmpty(baseSearch.BranchIds) ? new List<string>() : baseSearch.BranchIds.Split(',').ToList();
            var myBranchIds = string.IsNullOrEmpty(user.BranchIds) ? new List<string>() : user.BranchIds.Split(',').ToList();
            int count = await db.tbl_Class
                    .Where(x => x.Status == 1 && x.Enable == true
                    && (x.Name.Contains(baseSearch.Name) || string.IsNullOrEmpty(baseSearch.Name))
                    && (x.Name.Contains(baseSearch.Search) || string.IsNullOrEmpty(baseSearch.Name))
                    && (user.RoleId != (int)RoleEnum.student || studentInClass.Contains(x.Id))
                    && (user.RoleId != (int)RoleEnum.teacher || teacherInClass.Contains(x.Id))
                    && (string.IsNullOrEmpty(baseSearch.Types) || types.Contains(x.Type.ToString()))
                    && (string.IsNullOrEmpty(baseSearch.BranchIds) || branchIds.Contains(x.BranchId.ToString()))
                    && (user.RoleId == (int)RoleEnum.admin || user.RoleId == (int)RoleEnum.student || user.RoleId == (int)RoleEnum.parents || myBranchIds.Contains(x.BranchId.ToString()))
                    ).CountAsync();
            return count;
        }
        public static async Task<int> Opening(lmsDbContext db, ClassSearch baseSearch, tbl_UserInformation user)
        {
            var studentInClass = await db.tbl_StudentInClass
                .Where(x => x.StudentId == user.UserInformationId && x.Enable == true)
                .Select(x => x.ClassId).ToListAsync();

            var teacherInClass = await db.tbl_Schedule
              .Where(x => x.TeacherId == user.UserInformationId && x.Enable == true)
              .Select(x => x.ClassId).Distinct().ToListAsync();

            var types = string.IsNullOrEmpty(baseSearch.Types) ? new List<string>() : baseSearch.Types.Split(',').ToList();
            var branchIds = string.IsNullOrEmpty(baseSearch.BranchIds) ? new List<string>() : baseSearch.BranchIds.Split(',').ToList();
            var myBranchIds = string.IsNullOrEmpty(user.BranchIds) ? new List<string>() : user.BranchIds.Split(',').ToList();

            int count = await db.tbl_Class
               .Where(x => x.Status == 2 && x.Enable == true
               && (x.Name.Contains(baseSearch.Name) || string.IsNullOrEmpty(baseSearch.Name))
               && (x.Name.Contains(baseSearch.Search) || string.IsNullOrEmpty(baseSearch.Name))
               && (user.RoleId != (int)RoleEnum.student || studentInClass.Contains(x.Id))
               && (user.RoleId != (int)RoleEnum.teacher || teacherInClass.Contains(x.Id))
               && (string.IsNullOrEmpty(baseSearch.Types) || types.Contains(x.Type.ToString()))
               && (string.IsNullOrEmpty(baseSearch.BranchIds) || branchIds.Contains(x.BranchId.ToString()))
               && (user.RoleId == (int)RoleEnum.admin || user.RoleId == (int)RoleEnum.student || user.RoleId == (int)RoleEnum.parents || myBranchIds.Contains(x.BranchId.ToString()))
               ).CountAsync();
            return count;
        }
        public static async Task<int> Closing(lmsDbContext db, ClassSearch baseSearch, tbl_UserInformation user)
        {
            var studentInClass = await db.tbl_StudentInClass
                .Where(x => x.StudentId == user.UserInformationId && x.Enable == true)
                .Select(x => x.ClassId).ToListAsync();

            var teacherInClass = await db.tbl_Schedule
                .Where(x => x.TeacherId == user.UserInformationId && x.Enable == true)
                .Select(x => x.ClassId).Distinct().ToListAsync();

            var types = string.IsNullOrEmpty(baseSearch.Types) ? new List<string>() : baseSearch.Types.Split(',').ToList();
            var branchIds = string.IsNullOrEmpty(baseSearch.BranchIds) ? new List<string>() : baseSearch.BranchIds.Split(',').ToList();
            var myBranchIds = string.IsNullOrEmpty(user.BranchIds) ? new List<string>() : user.BranchIds.Split(',').ToList();

            int count = await db.tbl_Class
                .Where(x => x.Status == 3 && x.Enable == true
                && (x.Name.Contains(baseSearch.Name) || string.IsNullOrEmpty(baseSearch.Name))
                && (x.Name.Contains(baseSearch.Search) || string.IsNullOrEmpty(baseSearch.Name))
                && (user.RoleId != (int)RoleEnum.student || studentInClass.Contains(x.Id))
                && (user.RoleId != (int)RoleEnum.teacher || teacherInClass.Contains(x.Id))
                && (string.IsNullOrEmpty(baseSearch.Types) || types.Contains(x.Type.ToString()))
                && (string.IsNullOrEmpty(baseSearch.BranchIds) || branchIds.Contains(x.BranchId.ToString()))
                && (user.RoleId == (int)RoleEnum.admin || user.RoleId == (int)RoleEnum.student || user.RoleId == (int)RoleEnum.parents || myBranchIds.Contains(x.BranchId.ToString()))
                ).CountAsync();
            return count;
        }
        public static async Task<ClassResult> GetAll(ClassSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ClassSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin && user.RoleId != (int)RoleEnum.student && user.RoleId != (int)RoleEnum.parents)
                    myBranchIds = user.BranchIds;
                int uid = 0;
                int tid = 0;
                if (user.RoleId == (int)RoleEnum.student)
                    uid = user.UserInformationId;
                if (user.RoleId == (int)RoleEnum.parents)
                    uid = baseSearch.StudentId;
                if (user.RoleId == (int)RoleEnum.teacher)
                    tid = user.UserInformationId; ;

                string sql = $"Get_Class @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Name = N'{baseSearch.Name ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@ClassIds = N'{baseSearch.ClassIds ?? ""}'," +
                    $"@Types = N'{baseSearch.Types ?? ""}'," +
                    $"@Status = N'{baseSearch.Status ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@ProgramIds = N'{baseSearch.ProgramIds ?? ""}'," +
                    $"@GradeIds = N'{baseSearch.GradeIds ?? ""}'," +
                    $"@Uid = N'{uid}'," +
                    $"@Tid = N'{tid}'," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType ? 1 : 0)}'";
                var data = await db.SqlQuery<Get_Class>(sql);
                sql = sql.Replace("Get_Class", "Get_CountClassByStatus");
                var countClassByStatus = await db.SqlQuery<CountClassByStatus>(sql);
                //var upcoming = await Upcoming(db, baseSearch, user);
                //var opening = await Opening(db, baseSearch, user);
                //var closing = await Closing(db, baseSearch, user);
                if (!data.Any()) return new ClassResult
                {
                    TotalRow = 0,
                    Data = null,
                    Upcoming = countClassByStatus[0].Upcoming,
                    Opening = countClassByStatus[0].Opening,
                    Closing = countClassByStatus[0].Closing
                };

                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Class
                {
                    Id = i.Id,
                    AcademicId = i.AcademicId,
                    AcademicName = i.AcademicName,
                    BranchId = i.BranchId,
                    BranchName = i.BranchName,
                    CertificateTemplateId = i.CertificateTemplateId,
                    CreatedBy = i.CreatedBy,
                    CreatedOn = i.CreatedOn,
                    CurriculumId = i.CurriculumId,
                    CurriculumName = i.CurriculumName,
                    Enable = i.Enable,
                    EndDay = i.EndDay,
                    GradeId = i.GradeId,
                    GradeName = i.GradeName,
                    IsMonthly = i.IsMonthly,
                    LessonCompleted = i.LessonCompleted,
                    MaxQuantity = i.MaxQuantity,
                    ModifiedBy = i.ModifiedBy,
                    ModifiedOn = i.ModifiedOn,
                    Name = i.Name,
                    PaymentType = i.PaymentType,
                    Price = i.Price,
                    ProgramId = i.ProgramId,
                    ProgramName = i.ProgramName,
                    ScoreboardTemplateId = i.ScoreboardTemplateId,
                    StartDay = i.StartDay,
                    Status = i.Status,
                    StatusName = i.StatusName,
                    TeacherId = i.TeacherId,
                    TeacherName = i.TeacherName,
                    Thumbnail = i.Thumbnail,
                    TotalStudent = i.TotalStudent,
                    TotalLesson = i.TotalLesson,
                    Type = i.Type,
                    TypeName = i.TypeName,
                    EstimatedDay = i.EstimatedDay,
                    Teachers = Task.Run(() => GetTeachers(db, i.Id)).Result
                }).ToList();
                return new ClassResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    Upcoming = countClassByStatus[0].Upcoming,
                    Opening = countClassByStatus[0].Opening,
                    Closing = countClassByStatus[0].Closing
                };
            }
        }
        public static async Task<List<TeacherInClassModel>> GetTeachers(lmsDbContext db, int classId)
        {
            var teacherIds = await db.tbl_Schedule.Where(x => x.ClassId == classId && x.Enable == true)
                .Select(x => x.TeacherId).ToListAsync();
            return await db.tbl_UserInformation.Where(x => x.RoleId == (int)RoleEnum.teacher && x.Enable == true && teacherIds.Contains(x.UserInformationId))
                .Select(x => new TeacherInClassModel
                {
                    TeacherId = x.UserInformationId,
                    TeacherCode = x.UserCode,
                    TeacherName = x.FullName
                }).ToListAsync();
        }
        public static async Task<ClassResult> GetAllGantt(ClassSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ClassSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin && user.RoleId != (int)RoleEnum.student && user.RoleId != (int)RoleEnum.parents)
                    myBranchIds = user.BranchIds;
                int uid = 0;
                if (user.RoleId == (int)RoleEnum.student)
                    uid = user.UserInformationId;
                if (user.RoleId == (int)RoleEnum.parents)
                    uid = baseSearch.StudentId;

                string sql = $"Get_ClassGantt @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Name = N'{baseSearch.Name ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@Types = N'{baseSearch.Types ?? ""}'," +
                    $"@Status = N'{baseSearch.Status ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@Uid = N'{uid}'," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType ? 1 : 0)}'";
                var data = await db.SqlQuery<Get_ClassGantt>(sql);
                if (!data.Any()) return new ClassResult { TotalRow = 0, Data = null };
                var upcoming = data[0].Upcoming;
                var opening = data[0].Opening;
                var closing = data[0].Closing;
                var totalRow = data[0].TotalRow;
                var result = data.Where(x => x.StartDay.HasValue && x.EndDay.HasValue)
                    .Select(x => new { x.Name, x.Thumbnail, Values = new List<DateTime?> { x.StartDay, x.EndDay }, x.Status, x.StatusName }).ToList();
                return new ClassResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    Upcoming = upcoming,
                    Opening = opening,
                    Closing = closing
                };
            }
        }


        public static async Task<ClassResult> GetTotalRow(ClassSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ClassSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin && user.RoleId != (int)RoleEnum.student && user.RoleId != (int)RoleEnum.parents)
                    myBranchIds = user.BranchIds;
                int uid = 0;
                if (user.RoleId == (int)RoleEnum.student)
                    uid = user.UserInformationId;
                if (user.RoleId == (int)RoleEnum.parents)
                    uid = baseSearch.StudentId;

                string sql = $"Get_Class @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Name = N'{baseSearch.Name ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@Types = N'{baseSearch.Types ?? ""}'," +
                    $"@Status = N'{baseSearch.Status ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@Uid = N'{uid}'," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType ? 1 : 0)}'";
                var data = await db.SqlQuery<Get_Class>(sql);
                if (!data.Any()) return new ClassResult { TotalRow = 0, Data = null };
                var upcoming = data[0].Upcoming;
                var opening = data[0].Opening;
                var closing = data[0].Closing;
                var totalRow = data[0].TotalRow;
                return new ClassResult
                {
                    TotalRow = totalRow,
                    Upcoming = upcoming,
                    Opening = opening,
                    Closing = closing
                };
            }
        }
        public static async Task<AppDomainResult> GetRollUpTeacher(RollUpTeacherSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new RollUpTeacherSearch();
                int teacherId = 0;
                if (user.RoleId == (int)RoleEnum.teacher)
                    teacherId = user.UserInformationId;
                string sql = $"Get_RollUpTeacher @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = {baseSearch.ClassId}," +
                    $"@TeacherId = {teacherId}";
                var data = await db.SqlQuery<Get_RollUpTeacher>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new RollUpTeacherModel(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task RollUpTeacher(int scheduleId)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == scheduleId);
                if (entity == null)
                    throw new Exception("Không tìm thấy lịch");
                if (entity.TeacherAttendanceId != 0)
                    entity.TeacherAttendanceId = 0;
                else
                    entity.TeacherAttendanceId = entity.TeacherId;
                await db.SaveChangesAsync();
            }
        }
        public class Get_ScheduleInDateNow
        {
            public string FullName { get; set; }
            public string UserCode { get; set; }
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            public int BranchId { get; set; }
            public string BranchName { get; set; }
            public int RoomId { get; set; }
            public string RoomName { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public int TeacherId { get; set; }
            public string TeacherName { get; set; }
        }
        public static async Task<List<Get_ScheduleInDateNow>> GetScheduleInDateNow(int branchId)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_ScheduleInDateNow @BranchId = {branchId}";
                var data = await db.SqlQuery<Get_ScheduleInDateNow>(sql);
                return data;
            }
        }
        #region Dạy kèm

        public class TeacherTutoringAvailableSearch : SearchOptions
        {
            public int? CurriculumId { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
        public class TeacherTutoringAvailableModel
        {
            public int TeacherId { get; set; }
            public string TeacherName { get; set; }
            public string TeacherCode { get; set; }
            public string Avatar { get; set; }
            public string Extension { get; set; }
            public bool Fit { get; set; }
            public string Note { get; set; }
            public double Rate { get; set; }
        }
        public static async Task<AppDomainResult> GetTeacherTutoringAvailable(TeacherTutoringAvailableSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == baseSearch.CurriculumId);
                if (curriculum == null)
                    throw new Exception("Không tìm thấy giáo trình");
                var result = new List<TeacherTutoringAvailableModel>();
                var teachers = await db.tbl_TeacherInProgram.Where(x => x.ProgramId == curriculum.ProgramId && x.Enable == true).Select(x => new { x.TeacherId }).ToListAsync();
                if (teachers.Any())
                {
                    foreach (var item in teachers)
                    {
                        var teacher = await db.tbl_UserInformation
                            .FirstOrDefaultAsync(x => x.UserInformationId == item.TeacherId && x.Enable == true);
                        if (teacher != null)
                        {
                            var check = await CheckTeacher(db, 0, item.TeacherId.Value, baseSearch.StartTime, baseSearch.EndTime);
                            var checkTimeAvailable = await db.tbl_ScheduleAvailable
                                .AnyAsync(x => x.TeacherId == teacher.UserInformationId
                                && x.Enable == true
                                && x.StartTime <= baseSearch.StartTime
                                && x.EndTime >= baseSearch.EndTime
                                );

                            double rate = 0;
                            var rates = await db.tbl_Schedule
                                .Where(x => x.TeacherAttendanceId == teacher.UserInformationId && x.Enable == true && x.RateTeacher != 0)
                                .Select(x => x.RateTeacher).ToListAsync();
                            if (rates.Any())
                                rate = Math.Round(rates.Average(), 1);

                            result.Add(new TeacherTutoringAvailableModel
                            {
                                Fit = !checkTimeAvailable ? false : check.Value,
                                Note = !checkTimeAvailable ? "Giáo viên không mở lịch trong thời gian này" : check.Note,
                                TeacherCode = teacher.UserCode,
                                TeacherId = teacher.UserInformationId,
                                TeacherName = teacher.FullName,
                                Avatar = teacher.Avatar,
                                Extension = teacher.Extension,
                                Rate = rate
                            });
                        }
                    }
                }
                result = result.OrderByDescending(x => x.Fit).ThenByDescending(x => x.Rate).ToList();
                int totalRow = result.Count();
                return new AppDomainResult
                {
                    Data = result.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList(),
                    TotalRow = totalRow,
                    Success = true
                };
            }
        }
        public class TutoringConfigModel
        {
            /// <summary>
            /// OpenTutoring - Đặt lịch trước bao nhiêu tiếng
            /// CancelTutoring - Hủy lịch trước bao nhiêu tiếng
            /// </summary>
            public string Code { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
        }
        public static async Task<TutoringConfigModel> TutoringConfig(TutoringConfigModel itemModel)
        {
            using (var db = new lmsDbContext())
            {
                if (itemModel.Code != "OpenTutoring" && itemModel.Code != "CancelTutoring")
                    throw new Exception("Mã không phù hợp");
                var data = await db.tbl_Config.FirstOrDefaultAsync(x => x.Code == itemModel.Code);
                if (data == null)
                {
                    data = new tbl_Config
                    {
                        Code = itemModel.Code,
                        Name = itemModel.Code == "OpenTutoring" ? "Đặt lịch trước bao nhiêu tiếng" : "Hủy lịch trước bao nhiêu tiếng",
                        Value = itemModel.Value.ToString(),
                    };
                    db.tbl_Config.Add(data);
                }
                else
                {
                    data.Value = itemModel.Value.ToString();
                }
                await db.SaveChangesAsync();
                return new TutoringConfigModel
                {
                    Code = data.Code,
                    Name = data.Name,
                    Value = int.Parse(data.Value)
                };
            }
        }
        public static async Task<List<TutoringConfigModel>> GetTutoringConfig()
        {
            using (var db = new lmsDbContext())
            {
                var result = new List<TutoringConfigModel>();
                var openTutoring = await db.tbl_Config.FirstOrDefaultAsync(x => x.Code == "OpenTutoring");
                if (openTutoring != null)
                    result.Add(new TutoringConfigModel
                    {
                        Code = openTutoring.Code,
                        Name = openTutoring.Name,
                        Value = int.Parse(openTutoring.Value)
                    });
                else
                    result.Add(new TutoringConfigModel
                    {
                        Code = "OpenTutoring",
                        Name = "Đặt lịch trước bao nhiêu tiếng",
                        Value = 0
                    });

                var cancelTutoring = await db.tbl_Config.FirstOrDefaultAsync(x => x.Code == "CancelTutoring");
                if (cancelTutoring != null)
                    result.Add(new TutoringConfigModel
                    {
                        Code = cancelTutoring.Code,
                        Name = cancelTutoring.Name,
                        Value = int.Parse(cancelTutoring.Value)
                    });
                else
                    result.Add(new TutoringConfigModel
                    {
                        Code = "CancelTutoring",
                        Name = "Hủy lịch trước bao nhiêu tiếng",
                        Value = 0
                    });
                return result;
            }
        }
        public class TutoringCurriculumModel
        {
            /// <summary>
            /// Số buổi đã đặt
            /// </summary>
            public int Booked { get; set; }
            /// <summary>
            /// Số buổi
            /// </summary>
            public int? Lesson { get; set; }
            /// <summary>
            /// Thời gian
            /// </summary>
            public int? Time { get; set; }
        }
        /// <summary>
        /// Lấy chi tiết đã học bao nhiêu buổi và 
        /// </summary>
        /// <returns></returns>
        public static async Task<TutoringCurriculumModel> TutoringCurriculum(int classId)
        {
            using (var db = new lmsDbContext())
            {
                var result = new TutoringCurriculumModel
                {
                    Booked = 0,
                    Lesson = 0,
                    Time = 0,
                };
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == classId);
                if (_class == null)
                    return result;
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == _class.CurriculumId);
                if (curriculum == null)
                    return result;

                result.Lesson = curriculum.Lesson;
                result.Time = curriculum.Time;
                result.Booked = await db.tbl_Schedule.Where(x => x.ClassId == classId
                        && x.Enable == true
                        && x.StatusTutoring != 2
                        && x.StatusTutoring != 4
                        && x.StatusTutoring != 5).CountAsync();
                return result;
            }
        }
        #endregion
        public class AttendanceResult : AppDomainResult
        {
            public List<ScheduleAttendanceDTO> ScheduleAttendances { get; set; }
        }
        public static async Task AllowAttend(tbl_Schedule schedule)
        {
            var now = GetDateTime.Now;
            var config = await AttendaceConfigService.GetAttendaceConfigActive();
            if (config != null)
            {
                if (config.AttendaceTypeModel.Any())
                    foreach (var rule in config.AttendaceTypeModel)
                    {
                        // thời gian điểm danh
                        var time = rule.TimeAttendace.Split(":");
                        int hour = Convert.ToInt32(time[0]);
                        int minute = Convert.ToInt32(time[1]);

                        var dayAttendace = new DateTime(schedule.StartTime.Value.Year, schedule.StartTime.Value.Month, schedule.StartTime.Value.Day).AddHours(hour).AddMinutes(minute);
                        var checkInSchedule = schedule.StartTime <= now && schedule.EndTime >= now;
                        var checkDay = now.Date == dayAttendace.Date;
                        if (rule.TypeId == (int)AttendaceType.DauNgay)
                        {
                            if (!checkDay)
                                if (now < dayAttendace)
                                    throw new Exception("Chưa thể điểm danh");
                                else
                                    throw new Exception("Đã quá hạn điểm danh ");

                            if (now < dayAttendace)
                                throw new Exception("Điểm danh đầu ngày bắt đầu lúc " + rule.TimeAttendace + ".");
                        }
                        else if (rule.TypeId == (int)AttendaceType.DauGioHoc)
                        {
                            if (!checkInSchedule)
                                throw new Exception("Buổi học đã kết thúc");
                            if (now > dayAttendace)
                                throw new Exception("Điểm danh đầu giờ học trước " + minute + " phút.");
                        }
                        else if (rule.TypeId == (int)AttendaceType.CuoiGioHoc)
                        {
                            if (!checkInSchedule)
                                throw new Exception("Buổi học đã kết thúc");
                            if (now < dayAttendace)
                                throw new Exception("Điểm danh cuối giờ học trước " + minute + " phút.");
                        }
                        else if (rule.TypeId == (int)AttendaceType.SauBuoiHoc)
                        {
                            if (checkInSchedule)
                                throw new Exception("Buổi học chưa kết thúc");
                            if (now > dayAttendace)
                                throw new Exception("Điểm danh sau buổi học sau " + minute + " phút.");
                        }
                        else if (rule.TypeId == (int)AttendaceType.CuoiNgay)
                        {
                            if (!checkDay)
                                if (now > dayAttendace)
                                    throw new Exception("Chưa thể điểm danh");
                                else
                                    throw new Exception("Đã quá hạn điểm danh ");
                            if (now > dayAttendace)
                                throw new Exception("Điểm danh cuối ngày kết thúc lúc " + rule.TimeAttendace + ".");
                        }
                    }
            }
        }
        public static async Task<AttendanceResult> GetAttendanceInClass(AttendanceSearch baseSearch, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new AttendanceSearch();
                string studentIds = "";
                if (userLog.RoleId == (int)RoleEnum.student)
                {
                    studentIds = userLog.UserInformationId.ToString();
                }
                else if (userLog.RoleId == (int)RoleEnum.parents)
                {
                    var listStudentId = await db.tbl_UserInformation
                        .Where(x => x.ParentId == userLog.UserInformationId && x.Enable == true)
                        .Select(x => x.UserInformationId).ToListAsync();
                    if (listStudentId.Any())
                    {
                        studentIds = string.Join(",", listStudentId);
                    }
                    else
                    {
                        studentIds = "0";
                    }

                }

                string sql = $"Get_StudentInClassWhenAttendance @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = N'{baseSearch.ClassId ?? 0}'," +
                    $"@StudentIds = N'{studentIds}'";
                var data = await db.SqlQuery<Get_StudentInClassWhenAttendance>(sql);
                var schedules = await db.tbl_Schedule.Where(x => x.ClassId == baseSearch.ClassId.Value && x.Enable == true)
                    .Select(x => new ScheduleAttendanceDTO
                    {
                        Id = x.Id,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime
                    }).OrderBy(x => x.StartTime).ToListAsync();
                if (!data.Any())
                    return new AttendanceResult
                    {
                        TotalRow = 0,
                        Data = new List<StudentInClassWhenAttendanceDTO> { },
                        ScheduleAttendances = schedules
                    };
                var result = (from i in data
                              select new StudentInClassWhenAttendanceDTO
                              {
                                  StudentCode = i.StudentCode,
                                  ClassId = i.ClassId,
                                  ClassName = i.ClassName,
                                  StudentId = i.StudentId,
                                  StudentName = i.StudentName,
                                  Id = i.Id,
                                  Attendances = Task.Run(() => GetAttendance(db, i.ClassId, i.StudentId)).Result
                              }).ToList();
                var totalRow = data[0].TotalRow;
                return new AttendanceResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    ScheduleAttendances = schedules
                };
            }
        }
        public static async Task<List<AttendanceDTO>> GetAttendance(lmsDbContext db, int classId, int studentId)
        {
            return await db.tbl_RollUp.Where(x => x.ClassId == classId && x.StudentId == studentId && x.Enable == true)
                .Select(x => new AttendanceDTO
                {
                    ScheduleId = x.ScheduleId ?? 0,
                    Note = x.Note,
                    Status = x.Status ?? 0,
                    StatusName = x.StatusName
                }).ToListAsync();
        }
        public static async Task UpdateAttendance(AttendanceForm itemModel, tbl_UserInformation userlog)
        {
            using (var db = new lmsDbContext())
            {
                var userInfo = await db.tbl_UserInformation.Where(x => x.Enable == true).ToListAsync();
                var teacher = userInfo.FirstOrDefault(x => x.UserInformationId == userlog.UserInformationId && x.Enable == true);
                var schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == itemModel.ScheduleId);
                await AllowAttend(schedule);
                if (teacher.RoleId == (int)RoleEnum.teacher)
                {
                    if (teacher.UserInformationId != schedule.TeacherId)
                    {
                        throw new Exception("Chỉ giáo viên dạy buổi học này mới được điểm danh");
                    }
                }

                // Tìm thông tin học viên
                var studentInformation = userInfo.FirstOrDefault(x => x.UserInformationId == itemModel.StudentId && x.Enable == true);
                var findClass = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == schedule.ClassId && x.Enable == true);

                if (schedule == null)
                    throw new Exception("Không tìm thấy buổi học");
                var hasStudent = await db.tbl_StudentInClass
                    .AnyAsync(x => x.ClassId == schedule.ClassId && x.StudentId == itemModel.StudentId && x.Enable == true);
                if (!hasStudent)
                    throw new Exception("Không tìm thấy học viên trong lớp");

                //if (studentInformation.ParentId != null || studentInformation.ParentId != 0)
                //{
                //    // Tìm phụ huynh của học viên dựa trên id của học viên
                //    tbl_UserInformation parent = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.Enable == true && x.UserInformationId == studentInformation.ParentId && x.RoleId == (int)RoleEnum.parents);

                //    Thread sendParent = new Thread(async () =>
                //    {
                //        tbl_Notification notification = new tbl_Notification();

                //        notification.Title = "Điểm danh học sinh";
                //        notification.Content = "Học viên " + studentInformation.FullName + " được điểm danh ( " + itemModel.StatusName + " ) tại lớp " + findClass.Name + " vào thời gian " + schedule.StartTime + " đến " + schedule.EndTime;
                //        notification.ContentEmail = "Học viên " + studentInformation.FullName + " được điểm danh ( " + itemModel.StatusName + " ) tại lớp " + findClass.Name + " vào thời gian " + schedule.StartTime + " đến " + schedule.EndTime;
                //        notification.Type = 1;

                //        if (studentInformation.ParentId == parent.UserInformationId)
                //        {
                //            notification.UserId = parent.UserInformationId;
                //            await NotificationService.Send(notification, studentInformation, true);
                //        }
                //    });
                //    sendParent.Start();
                //}

                var rollUp = await db.tbl_RollUp
                                      .FirstOrDefaultAsync(x => x.ScheduleId == schedule.Id && x.StudentId == itemModel.StudentId && x.Enable == true);
                if (rollUp == null)
                {
                    rollUp = new tbl_RollUp
                    {
                        ClassId = schedule.ClassId,
                        CreatedBy = userlog.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        LearningStatus = 1,
                        LearningStatusName = "Giỏi",
                        ModifiedBy = userlog.FullName,
                        ModifiedOn = DateTime.Now,
                        Note = itemModel.Note,
                        ScheduleId = schedule.Id,
                        Status = itemModel.Status,
                        StatusName = itemModel.StatusName,
                        StudentId = itemModel.StudentId
                    };
                    db.tbl_RollUp.Add(rollUp);
                    await db.SaveChangesAsync();
                }
                else
                {
                    rollUp.Status = itemModel.Status ?? rollUp.Status;
                    rollUp.StatusName = itemModel.StatusName ?? rollUp.StatusName;
                    rollUp.Note = itemModel.Note ?? rollUp.Note;
                    rollUp.ModifiedBy = userlog.FullName;
                    rollUp.ModifiedOn = DateTime.Now;
                }
                await db.SaveChangesAsync();
            }
        }
        public static async Task UpdateAttendances(AttendancesForm itemModel, tbl_UserInformation userlog)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (itemModel == null)
                            throw new Exception("Dữ liệu không phù hợp");
                        var schedule = await db.tbl_Schedule.SingleOrDefaultAsync(x => x.Id == itemModel.ScheduleId);
                        if (schedule == null)
                            throw new Exception("Không tìm thấy buổi học");
                        await AllowAttend(schedule);
                        var userInfo = await db.tbl_UserInformation.Where(x => x.Enable == true).ToListAsync();
                        var teacher = userInfo.FirstOrDefault(x => x.UserInformationId == userlog.UserInformationId && x.Enable == true);
                        if (teacher.RoleId == (int)RoleEnum.teacher)
                        {
                            if (teacher.UserInformationId != schedule.TeacherId)
                            {
                                throw new Exception("Chỉ giáo viên dạy buổi học này mới được điểm danh");
                            }
                        }
                        var studentInClasses = await db.tbl_StudentInClass
                            .Where(x => x.ClassId == schedule.ClassId && x.Enable == true)
                            .ToListAsync();
                        var findClass = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == schedule.ClassId && x.Enable == true);
                        List<tbl_UserInformation> students = new List<tbl_UserInformation>();
                        List<tbl_UserInformation> parents = new List<tbl_UserInformation>();
                        // Tìm thông tin của học viên dựa trên id của học viên
                        foreach (var findStudent in studentInClasses)
                        {
                            var student = new tbl_UserInformation();
                            student = userInfo.FirstOrDefault(x => x.Enable == true && x.UserInformationId == findStudent.StudentId && x.ParentId != null);
                            if (student != null)
                            {
                                students.Add(student);
                            }
                        }
                        // Tìm phụ huynh của học viên dựa trên thông của học viên
                        foreach (var findParent in students)
                        {
                            var parent = new tbl_UserInformation();
                            parent = userInfo.FirstOrDefault(x => x.Enable == true && x.UserInformationId == findParent.ParentId && x.RoleId == (int)RoleEnum.parents);
                            if (parent != null)
                            {
                                parents.Add(parent);
                            }
                        }
                        //Thread sendParent = new Thread(async () =>
                        //{
                        //    tbl_Notification notification = new tbl_Notification();

                        //    foreach (var informationOfStudent in students)
                        //    {
                        //        notification.Title = "Điểm danh học sinh";
                        //        notification.Content = "Học viên " + informationOfStudent.FullName + " được điểm danh ( " + itemModel.StatusName + " ) tại lớp " + findClass.Name + " vào thời gian " + schedule.StartTime + " đến " + schedule.EndTime;
                        //        notification.ContentEmail = "Học viên " + informationOfStudent.FullName + " được điểm danh ( " + itemModel.StatusName + " ) tại lớp " + findClass.Name + " vào thời gian " + schedule.StartTime + " đến " + schedule.EndTime;
                        //        notification.Type = 1;

                        //        var parent = parents.FirstOrDefault(p => p.UserInformationId == informationOfStudent.ParentId);
                        //        if (parent != null)
                        //        {
                        //            notification.UserId = parent.UserInformationId;
                        //            await NotificationService.Send(notification, userlog, true);
                        //        }
                        //    }
                        //});
                        //sendParent.Start();


                        if (studentInClasses.Any())
                        {
                            foreach (var item in studentInClasses)
                            {
                                var rollUp = await db.tbl_RollUp
                                    .FirstOrDefaultAsync(x => x.ScheduleId == schedule.Id && x.StudentId == item.StudentId && x.Enable == true);
                                if (rollUp == null)
                                {
                                    rollUp = new tbl_RollUp
                                    {
                                        ClassId = schedule.ClassId,
                                        CreatedBy = userlog.FullName,
                                        CreatedOn = DateTime.Now,
                                        Enable = true,
                                        LearningStatus = 1,
                                        LearningStatusName = "Giỏi",
                                        ModifiedBy = userlog.FullName,
                                        ModifiedOn = DateTime.Now,
                                        Note = "",
                                        ScheduleId = schedule.Id,
                                        Status = itemModel.Status,
                                        StatusName = itemModel.StatusName,
                                        StudentId = item.StudentId
                                    };
                                    db.tbl_RollUp.Add(rollUp);
                                    await db.SaveChangesAsync();
                                }
                                else
                                {
                                    rollUp.Status = itemModel.Status;
                                    rollUp.StatusName = itemModel.StatusName;
                                    rollUp.ModifiedBy = userlog.FullName;
                                    rollUp.ModifiedOn = DateTime.Now;
                                }
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
        public class NotiCheckClass
        {
            public int ClassId;
            public string ClassName = "";
            public DateTime EndDay = DateTime.Now;
            public string TeacherName = "";
            public string Content = "";
            public string Url = "";
            public List<int> IdOfStudent = new List<int>();
            public List<int> IdOfTeacher = new List<int>();
        }
        public static async Task CheckClassExpires()
        {
            string content = "";
            string notificationContent = "";
            string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
            var appRootPath = WebHostEnvironment.Environment.ContentRootPath;
            var pathViews = Path.Combine(appRootPath, "Views");
            content = File.ReadAllText($"{pathViews}/Base/Mail/Class/ClassEnding.cshtml");

            using (var db = new lmsDbContext())
            {
                DateTime today = DateTime.Now;
                // Lấy thông tin lớp học
                var classes = await db.tbl_Class.Where(x => x.Enable == true && x.Status == 2).ToListAsync();
                classes = classes.Where(x => x.EndDay?.Date == today.Date).ToList();
                List<NotiCheckClass> checkClasses = new List<NotiCheckClass>();
                tbl_UserInformation user = new tbl_UserInformation
                {
                    FullName = "Tự động"
                };
                List<int> studentIds = new List<int>();
                string teacherName = "";
                if (classes != null && classes.Count != 0)
                {
                    foreach (var _class in classes)
                    {
                        var notiCheckClass = new NotiCheckClass();
                        UrlNotificationModels urlNotification = new UrlNotificationModels();
                        string url = "class=" + _class.Id + "&curriculum=" + _class.CurriculumId + "&branch=" + _class.BranchId + "&scoreBoardTemplateId=" + _class.ScoreboardTemplateId;
                        string urlEmail = urlNotification.url + urlNotification.urlDetailClass + url;
                        // Lấy thông tin học sinh trong lớp
                        var studentInClass = await db.tbl_StudentInClass.Where(x => x.Enable == true && x.ClassId == _class.Id).ToListAsync();
                        if (studentInClass != null && studentInClass.Count != 0)
                        {
                            foreach (var student in studentInClass)
                            {
                                if (student.StudentId != null)
                                {
                                    int studentId = (int)student.StudentId;
                                    notiCheckClass.IdOfStudent.Add(studentId);
                                }
                            }
                        }
                        // Lấy thông tin giáo viên
                        var teacherInClass = await db.tbl_Schedule.Where(x => x.ClassId == _class.Id && x.Enable == true)
                                        .Select(x => x.TeacherId).Distinct().ToListAsync();
                        var teachers = await db.tbl_UserInformation.Where(x => x.RoleId == 2 && x.Enable == true && teacherInClass.Contains(x.UserInformationId))
                            .Select(x => new TeacherAvailable
                            {
                                Id = x.UserInformationId,
                                TeacherCode = x.UserCode,
                                TeacherName = x.FullName
                            }).ToListAsync();
                        if (teachers != null && teachers.Count != 0)
                        {
                            foreach (var teacher in teachers)
                            {
                                if (teachers.Count > 1)
                                {
                                    notiCheckClass.TeacherName += teacher.TeacherName + ", ";
                                    teacherName = notiCheckClass.TeacherName;
                                }
                                else
                                {
                                    notiCheckClass.TeacherName = teacher.TeacherName;
                                    teacherName = notiCheckClass.TeacherName;
                                }
                                notiCheckClass.IdOfTeacher.Add(teacher.Id);
                            }
                        }

                        content = content.Replace("{ClassName}", _class.Name);
                        content = content.Replace("{TeacherName}", teacherName);
                        content = content.Replace("{TypeName}", _class.TypeName);
                        content = content.Replace("{StartDay}", _class.StartDay.Value.ToString("dd/MM/yyyy"));
                        content = content.Replace("{EndDay}", _class.EndDay.Value.ToString("dd/MM/yyyy"));
                        content = content.Replace("{ProjectName}", projectName);
                        content = content.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");
                        notificationContent = @"<div>" + content + @"</div>";
                        notiCheckClass.Content = notificationContent;
                        notiCheckClass.ClassName = _class.Name;
                        notiCheckClass.Url = url;
                        notiCheckClass.ClassId = _class.Id;
                        checkClasses.Add(notiCheckClass);
                    }
                    //// Gửi học sinh
                    //Thread sendStudent = new Thread(async () =>
                    //{
                    //    foreach (var cst in checkClasses)
                    //    {
                    //        tbl_Notification notification = new tbl_Notification();
                    //        notification.Title = "THÔNG BÁO LỚP HỌC KẾT THÚC";
                    //        notification.Content = "Lớp học " + cst.ClassName + " sẽ hết hạn trong hôm nay ( " + cst.EndDay.ToString("dd/MM/yyyy") + " ). Vui lòng kiểm tra!";
                    //        notification.ContentEmail = notificationContent;
                    //        notification.Type = 2;
                    //        notification.Category = 0;
                    //        notification.Url = cst.Url;
                    //        notification.AvailableId = cst.ClassId;
                    //        foreach (var id in cst.IdOfStudent)
                    //        {
                    //            notification.UserId = id;
                    //            await NotificationService.Send(notification, user, true);
                    //        }
                    //    }
                    //});
                    //sendStudent.Start();

                    //// Gửi cho giáo viên
                    //Thread sendTeacher = new Thread(async () =>
                    //{
                    //    foreach (var ct in checkClasses)
                    //    {
                    //        tbl_Notification notification = new tbl_Notification();
                    //        notification.Title = "THÔNG BÁO LỚP HỌC KẾT THÚC";
                    //        notification.Content = "Lớp học " + ct.ClassName + " sẽ hết hạn trong hôm nay ( " + ct.EndDay.ToString("dd/MM/yyyy") + " ). Vui lòng kiểm tra!";
                    //        notification.ContentEmail = notificationContent;
                    //        notification.Type = 2;
                    //        notification.Category = 0;
                    //        notification.Url = ct.Url;
                    //        notification.AvailableId = ct.ClassId;
                    //        foreach (var id in ct.IdOfTeacher)
                    //        {
                    //            notification.UserId = id;
                    //            await NotificationService.Send(notification, user, true);
                    //        }
                    //    }
                    //});
                    //sendTeacher.Start();
                }
            }
        }


        //public static async Task AutoNotifyTeacherWorkingDays()
        //{
        //    try
        //    {
        //        using (var db = new lmsDbContext())
        //        {
        //            var env = WebHostEnvironment.Environment;
        //            string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
        //            var pathViews = Path.Combine(env.ContentRootPath, "Views");

        //            tbl_UserInformation user = new tbl_UserInformation
        //            {
        //                FullName = "Tự động"
        //            };

        //            List<TeacherWorkingDay> teacherWorkingDays = new List<TeacherWorkingDay>();

        //            var teacherList = await db.tbl_UserInformation
        //                                  .Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.teacher)
        //                                  .ToListAsync();
        //            var currentDate = DateTime.Now;
        //            var scheduleList = await db.tbl_Schedule
        //                                   .Where(x => x.Enable == true &&
        //                                               x.StartTime.Value.Year == currentDate.Year &&
        //                                               x.StartTime.Value.Month == currentDate.Month)
        //                                   .ToListAsync();

        //            foreach (var teacher in teacherList)
        //            {
        //                string content = "";
        //                content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Class/TeacherWorkingDays.cshtml");

        //                var teacherWorkingDay = new TeacherWorkingDay();
        //                var teacherInClass = scheduleList.Where(x => x.TeacherId == teacher.UserInformationId).ToList();
        //                var teacherInClassAttendance = scheduleList.Where(x => x.TeacherAttendanceId == teacher.UserInformationId).ToList();
        //                var teacherOutClass = scheduleList.Where(x => x.TeacherId == teacher.UserInformationId && x.TeacherAttendanceId == 0).ToList();
        //                // Tổng số ngày làm việc
        //                int totalWorkinDays = teacherInClass.Count;
        //                // Số ngày đi làm
        //                int workingDays = teacherInClassAttendance.Count;
        //                // Số ngày nghỉ
        //                int dayOff = teacherOutClass.Count;

        //                content = content.Replace("{Month}", currentDate.Month.ToString());
        //                content = content.Replace("{Year}", currentDate.Year.ToString());
        //                content = content.Replace("{FullName}", teacher.FullName);
        //                content = content.Replace("{TotalDay}", totalWorkinDays.ToString());
        //                content = content.Replace("{DayOn}", workingDays.ToString());
        //                content = content.Replace("{DayOff}", dayOff.ToString());
        //                content = content.Replace("{ProjectName}", projectName);
        //                var notificationContent = @"<div>" + content + @"</div>";

        //                teacherWorkingDay.TeacherId = teacher.UserInformationId;
        //                teacherWorkingDay.TeacherName = teacher.FullName;
        //                teacherWorkingDay.Total = totalWorkinDays;
        //                teacherWorkingDay.WorkingDays = workingDays;
        //                teacherWorkingDay.DayOff = dayOff;
        //                teacherWorkingDay.Content = notificationContent;
        //                teacherWorkingDays.Add(teacherWorkingDay);
        //            }

        //            Thread sendParent = new Thread(async () =>
        //            {
        //                foreach (var t in teacherWorkingDays)
        //                {
        //                    tbl_Notification notification = new tbl_Notification();

        //                    notification.Title = "Thông Báo Số Ngày Làm Việc Của Giáo Viên Trong Tháng " + currentDate.Month + " Năm " + currentDate.Year;
        //                    notification.ContentEmail = t.Content;
        //                    notification.Content = "Tổng số ngày làm việc của giáo viên " + t.TeacherName + " trong tháng " + currentDate.Month + " là " + t.Total + " ngày. Trong đó giáo viên đã làm " + t.WorkingDays + " ngày và đã nghỉ " + t.DayOff + " ngày!";
        //                    notification.Type = 2;
        //                    notification.UserId = t.TeacherId;
        //                    notification.Category = 0;
        //                    await NotificationService.Send(notification, user, true);
        //                }
        //            });
        //            sendParent.Start();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception();
        //    }
        //}


        //public static async Task AutoRemindGradingHomework()
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        var env = WebHostEnvironment.Environment;
        //        string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
        //        var pathViews = Path.Combine(env.ContentRootPath, "Views");
        //        List<TeacherWorkingDay> teacherWorkingDays = new List<TeacherWorkingDay>();
        //        tbl_UserInformation user = new tbl_UserInformation
        //        {
        //            FullName = "Tự động"
        //        };
        //        // Thông tin bài tập chưa chấm
        //        var ieltsExamResult = await db.tbl_IeltsExamResult.Where(x => x.Enable == true && x.Type == 3 && x.Status == 1).ToListAsync();
        //        // Thông tin giáo viên
        //        var teacherList = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.teacher).ToListAsync();
        //        // Thông tin lớp học
        //        var _class = await db.tbl_Class.Where(x => x.Enable == true).ToListAsync();
        //        // Thông tin lịch học
        //        var schedule = await db.tbl_Schedule.Where(x => x.Enable == true).ToListAsync();
        //        // Thông tin bài tập
        //        var homework = await db.tbl_Homework.Where(x => x.Enable == true && x.Type == 1).ToListAsync();
        //        UrlNotificationModels urlNotification = new UrlNotificationModels();
        //        foreach (var itemTeacher in teacherList)
        //        {
        //            string content = "";
        //            content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Class/UngradedAssignments.cshtml");
        //            var a = new StringBuilder();
        //            string ContentHomework = "";
        //            var teacherWorkingDay = new TeacherWorkingDay();

        //            // Tổng số bài tập chưa chấm của giáo viên
        //            var AllIeltsExamResultOfTeacher = ieltsExamResult
        //                .Where(x => x.Enable == true &&
        //                            x.Type == 3 &&
        //                            x.TeacherId == itemTeacher.UserInformationId &&
        //                            x.Status == 1)
        //                .ToList();
        //            // Danh sách lớp học mà giáo viên dạy
        //            var classOfTeacher = schedule.Where(x => x.TeacherId == itemTeacher.UserInformationId)
        //                        .Select(x => x.ClassId).Distinct().ToList();

        //            if (classOfTeacher.Count != 0 && AllIeltsExamResultOfTeacher.Count != 0)
        //            {
        //                // Lọc homeworkId từ danh sách bài tập chưa chấm
        //                var IeltsExamResultOfHomework = ieltsExamResult
        //                    .Where(x => x.Enable == true &&
        //                                x.Type == 3 &&
        //                                x.TeacherId == itemTeacher.UserInformationId &&
        //                                x.Status == 1)
        //                    .Select(x => x.ValueId).Distinct().ToList();
        //                if (IeltsExamResultOfHomework.Count != 0)
        //                {
        //                    int totalExamNotGraded = AllIeltsExamResultOfTeacher.Count;
        //                    bool isFirstIteration = true; // Biến để kiểm tra xem vòng lặp có phải là vòng lặp đầu tiên không
        //                    string url = "";
        //                    string urlEmail = "";
        //                    int classId = 0;
        //                    foreach (var c in classOfTeacher)
        //                    {
        //                        var dataClass = _class.FirstOrDefault(x => x.Id == c);
        //                        // Thông tin trong lớp có bao nhiêu bài tập
        //                        var homeworkInClass = homework.Where(x => x.ClassId == c).ToList();
        //                        if (homeworkInClass.Count != 0)
        //                        {
        //                            foreach (var itemHomework in homeworkInClass)
        //                            {
        //                                // Thông tin số bài làm của học sinh chưa được chấm trong 1 bài tập ở 1 lớp
        //                                var ieltsExamResultInHomework = ieltsExamResult.Where(x => x.ValueId == itemHomework.Id).ToList();
        //                                // Số bài làm học sinh giáo viên chưa chấm
        //                                if (ieltsExamResultInHomework.Count != 0)
        //                                {
        //                                    if (homeworkInClass.Count == 1)
        //                                        ContentHomework = $"<p>{itemHomework.Name} - {ieltsExamResultInHomework.Count} bài</p>";
        //                                    else ContentHomework += $"<p>{itemHomework.Name} - {ieltsExamResultInHomework.Count} bài</p>";
        //                                }
        //                            }
        //                            url = "class=" + dataClass.Id + "&curriculum=" + dataClass.CurriculumId + "&branch=" + dataClass.BranchId + "&scoreBoardTemplateId=" + dataClass.ScoreboardTemplateId;
        //                            urlEmail = urlNotification.url + urlNotification.urlDetailClass + url;
        //                            classId = dataClass.Id;
        //                            string contentUrlEmail = $"<a href=\"{urlEmail}\" target=\"_blank\">";
        //                            content = content.Replace("{ClassName}", dataClass.Name);
        //                            content = content.Replace("{HomeWork}", ContentHomework);
        //                            content = content.Replace("{Url}", contentUrlEmail);
        //                            // Kiểm tra xem vòng lặp có phải là vòng lặp đầu tiên không
        //                            if (!isFirstIteration)
        //                            {
        //                                string row = "<tr>";
        //                                row += $"<td class='ClassTd' >{dataClass.Name}</td>";
        //                                row += $"<td class='ClassTd'> {ContentHomework} </td>";
        //                                row += $"<td class='ClassTd'> {contentUrlEmail} </td>";
        //                                row += "</tr>";
        //                                a.Append(row);
        //                            }
        //                            // Đặt biến isFirstIteration thành false sau vòng lặp đầu tiên
        //                            isFirstIteration = false;
        //                        }
        //                    }
        //                    isFirstIteration = true;
        //                    content = content.Replace("{FullName}", itemTeacher.FullName);
        //                    content = content.Replace("{RL}", a.ToString());
        //                    content = content.Replace("{ProjectName}", projectName);
        //                    var notificationContent = @"<div>" + content + @"</div>";
        //                    teacherWorkingDay.TeacherName = itemTeacher.FullName;
        //                    teacherWorkingDay.TeacherId = itemTeacher.UserInformationId;
        //                    teacherWorkingDay.Total = totalExamNotGraded;
        //                    teacherWorkingDay.Content = notificationContent;
        //                    teacherWorkingDay.url = url;
        //                    teacherWorkingDay.ClassId = classId;
        //                    teacherWorkingDays.Add(teacherWorkingDay);
        //                }
        //            }
        //        }


        //        Thread sendTeacher = new Thread(async () =>
        //        {
        //            foreach (var t in teacherWorkingDays)
        //            {
        //                tbl_Notification notification = new tbl_Notification();
        //                notification.Title = "Thông Báo Bài Tập Chưa Chấm";
        //                notification.ContentEmail = t.Content;
        //                notification.Content = "Tổng số bài tập chưa chấm của bạn trong hôm nay là " + t.Total;
        //                notification.Type = 0;
        //                notification.UserId = t.TeacherId;
        //                notification.Category = 0;
        //                notification.Url = t.url;
        //                notification.AvailableId = t.ClassId;
        //                notification.Id = 0;
        //                await NotificationService.Send(notification, user, true);
        //            }
        //        });
        //        sendTeacher.Start();
        //    }
        //}

        public static async Task<List<AppropriateTeacherModel>> AppropriateTeacher(AppropriateSearch itemModel)
        {
            using (var db = new lmsDbContext())
            {
                if (!itemModel.LessonTime.Any())
                    throw new Exception("Không tìm thấy ngày học");
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumId);
                if (curriculum == null)
                    throw new Exception("Không tìm thấy giáo trình");
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                if (itemModel.LessonTime == null || itemModel.LessonTime.Count == 0)
                    throw new Exception("Vui lòng chọn ca học");
                var program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == itemModel.ProgramId);
                if (program == null)
                    throw new Exception("Không tìm thấy chương trình học");
                string branchId = itemModel.BranchId.ToString();
                // Tìm giáo viên trong chi nhánh
                string sql = $"Get_TeacherWhenCreateClass @BranchId = {itemModel.BranchId}, @ProgramId = {itemModel.ProgramId}";
                var dataTeacher = await db.SqlQuery<TeacherModel>(sql);
                // Lưu tất cả các buổi học tùy theo tổng số buổi của giáo trình
                var countTime = new List<CountTime>();
                var date = itemModel.StartDay.AddDays(-1).Date;
                do
                {
                    date = date.AddDays(1);
                    foreach (var item in itemModel.LessonTime)
                    {
                        if (item.DayOfWeek == (int)date.Date.DayOfWeek)
                        {
                            var studyTime = await db.tbl_StudyTime.SingleOrDefaultAsync(x => x.Id == item.StudyTimeId);
                            if (studyTime == null)
                                continue;
                            var stimes = studyTime.StartTime.Split(':');
                            DateTime st = new DateTime(date.Year, date.Month, date.Day, int.Parse(stimes[0]), int.Parse(stimes[1]), 0);
                            var etimes = studyTime.EndTime.Split(':');
                            DateTime et = new DateTime(date.Year, date.Month, date.Day, int.Parse(etimes[0]), int.Parse(etimes[1]), 0);
                            countTime.Add(new CountTime
                            {
                                StartTime = st,
                                EndTime = et
                            });
                        }
                    }
                } while (countTime.Count() < curriculum.Lesson);

                var teacherResults = new List<AppropriateTeacherModel>();
                if (dataTeacher.Count != 0)
                {
                    foreach (var t in dataTeacher)
                    {
                        var teacherResult = new AppropriateTeacherModel();
                        teacherResult.SameTimes = new List<SameTime>();
                        var sameTime = new SameTime();
                        var sameTimeOff = new SameTime();
                        var teacherInSchedule = await db.tbl_Schedule.Where(x => x.TeacherId == t.TeacherId && x.Enable == true).ToListAsync();
                        var teacherInfo = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == t.TeacherId);
                        var timeOff = await db.tbl_TeacherOff.Where(x => x.Enable == true && x.Status == 2 && x.TeacherId == t.TeacherId).ToListAsync();
                        // Lưu những giáo viên phù hợp và không phù hợp
                        foreach (var ot in teacherInSchedule)
                        {
                            foreach (var nt in countTime)
                            {
                                sameTime = new SameTime();
                                // Kiểm tra xem có bất kỳ thời gian trong teacherInSchedule trùng hoặc nằm trong khoảng thời gian của countTime không
                                if (nt.StartTime >= ot.StartTime && nt.StartTime <= ot.EndTime ||
                                       nt.EndTime >= ot.StartTime && nt.EndTime <= ot.EndTime ||
                                       nt.StartTime <= ot.StartTime && nt.EndTime >= ot.EndTime)
                                {
                                    string dayOfWeekStartTime = ot.StartTime.Value.ToString("dddd", CultureInfo.GetCultureInfo("vi-VN"));
                                    string dayOfWeekEndTime = ot.EndTime.Value.ToString("dddd", CultureInfo.GetCultureInfo("vi-VN"));
                                    sameTime.Type = 1;
                                    sameTime.TypeName = "Trùng ngày dạy";
                                    sameTime.StartTime = dayOfWeekStartTime + ", " + ot.StartTime.Value.ToString("dd/MM/yyyy HH:mm tt");
                                    sameTime.EndTime = dayOfWeekEndTime + ", " + ot.EndTime.Value.ToString("dd/MM/yyyy HH:mm tt");
                                    teacherResult.SameTimes.Add(sameTime);
                                }
                            }
                        }
                        foreach (var to in timeOff)
                        {
                            bool isTimeOffPrinted = false; // Biến này sẽ đánh dấu xem ngày nghỉ đã được lưu hay chưa
                            foreach (var nt in countTime)
                            {
                                if (nt.StartTime >= to.StartTime && nt.StartTime < to.EndTime ||
                                      nt.EndTime > to.StartTime && nt.EndTime <= to.EndTime ||
                                      nt.StartTime <= to.StartTime && nt.EndTime >= to.EndTime)
                                {
                                    if (!isTimeOffPrinted) // Kiểm tra xem ngày nghỉ đã được in ra chưa
                                    {
                                        string dayOfWeekStartTimeOff = to.StartTime.Value.ToString("dddd", CultureInfo.GetCultureInfo("vi-VN"));
                                        string dayOfWeekEndTimeOff = to.StartTime.Value.ToString("dddd", CultureInfo.GetCultureInfo("vi-VN"));
                                        sameTimeOff = new SameTime();
                                        sameTimeOff.Type = 1;
                                        sameTimeOff.TypeName = "Trùng ngày nghỉ";
                                        sameTimeOff.StartTime = dayOfWeekStartTimeOff + ", " + to.StartTime.Value.ToString("dd/MM/yyyy HH:mm tt");
                                        sameTimeOff.EndTime = dayOfWeekEndTimeOff + ", " + to.EndTime.Value.ToString("dd/MM/yyyy HH:mm tt");
                                        teacherResult.SameTimes.Add(sameTimeOff);
                                        isTimeOffPrinted = true; // Đánh dấu là ngày nghỉ đã được lưu
                                    }
                                }
                            }
                        }

                        teacherResult.TeacherId = teacherInfo.UserInformationId;
                        teacherResult.TeacherName = teacherInfo.FullName;
                        if (teacherResult.SameTimes.Count != 0)
                        {
                            teacherResult.AppropriateTeacher = false;
                        }
                        else
                        {
                            teacherResult.AppropriateTeacher = true;
                        }
                        teacherResults.Add(teacherResult);
                    }
                }
                else return teacherResults;
                return teacherResults;
            }
        }

    }
}