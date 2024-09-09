using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Web;
using LMSCore.Areas.Models;
using System.Globalization;

namespace LMSCore.Services
{
    public class ScheduleEmptyService : DomainService
    {
        public ScheduleEmptyService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<TeacherScheduleEmptyResult> GetTeacherScheduleEmpty(TeacherScheduleEmptySearch search)
        {
            if (search.DateStart > search.DateEnd || search.DateStart < DateTime.Now.Date)
                throw new Exception("Chọn ngày chưa phù hợp");
            //lấy chi nhánh
            var branch = await dbContext.tbl_Branch.FirstOrDefaultAsync(x => x.Id == search.BranchId && x.Enable == true);
            if (branch == null)
                throw new Exception("Chi nhánh không tồn tại");
            //lấy chương trình học
            var program = await dbContext.tbl_Program.FirstOrDefaultAsync(x => x.Id == search.ProgramId && x.Enable == true);
            if (program == null)
                throw new Exception("Chương trình học không tồn tại");
            //lấy ds lớp theo ct học
            var listClass = await dbContext.tbl_Class.Where(x => x.ProgramId == search.ProgramId
                                                            && x.Enable == true
                                                            && x.BranchId == search.BranchId).ToListAsync();
            if (listClass.Count == 0)
                throw new Exception("Chi nhánh chưa có lớp nào dạy chương trình này"); 
            // giáo viên theo Id gv nếu có
            var listTeacher = new List<tbl_UserInformation>();
            if (search.TeacherId != null)
            {
                var splitTeacherId = search.TeacherId.Split(',');
                foreach (var item in splitTeacherId)
                {
                    var tc = await dbContext.tbl_UserInformation.FirstOrDefaultAsync(x =>x.UserInformationId.ToString()==item 
                                                                        && x.RoleId == 2 
                                                                        && x.Enable == true);
                    if ( tc != null)
                        listTeacher.Add(tc);
                }
            }
            else
                listTeacher = await dbContext.tbl_UserInformation.Where(x => x.RoleId == 2 && x.Enable == true).ToListAsync();
            // lọc gv theo trung tâm
            for (int x = listTeacher.Count - 1; x > -1; x--)
            {
                if (listTeacher[x].BranchIds.Split(',').Contains(search.BranchId.ToString()) != true)
                    listTeacher.RemoveAt(x);
            }
            if (listTeacher.Count == 0)
                throw new Exception("Chi nhánh chưa có giáo viên nào");
            //----------------------------------------------------------------// 
            var dateStart = search.DateStart.Date;
            var dateEnd = search.DateEnd.Date;
            // ds ca  
            var listStudyTime = new List<tbl_StudyTime>();
            if (search.StudyTime != null)
            {
                var splitStudyTime = search.StudyTime.Split(',');
                foreach (var item in splitStudyTime)
                {
                    int idValue = int.Parse(item);
                    var studyTime = await dbContext.tbl_StudyTime.FirstOrDefaultAsync(x => x.Id  == idValue && x.Enable == true);
                    if (studyTime != null)
                        listStudyTime.Add(studyTime);
                }
            }
            else
                listStudyTime = await dbContext.tbl_StudyTime.Where(x => x.Enable == true).ToListAsync();  
            // lưu kết quả trả về
            TeacherScheduleEmptyResult result = new TeacherScheduleEmptyResult();
            result.ProgramId = program.Id;
            result.ProgramName = program.Name;
            result.BranchId = branch.Id;
            result.BranchName = branch.Name;
            result.DateStart = search.DateStart;
            result.DateEnd = search.DateEnd;
            List<TeacherScheduleEmptyStudyTime> listTeacherScheduleEmptyStudyTime = new List<TeacherScheduleEmptyStudyTime>();
            // ngày-> shift-> program-> class-> schedule
            do
            {
                foreach (var s in listStudyTime)
                {
                    TeacherScheduleEmptyStudyTime teacherScheduleEmptyStudyTime = new TeacherScheduleEmptyStudyTime();
                    teacherScheduleEmptyStudyTime.StudyTimeId = s.Id;
                    teacherScheduleEmptyStudyTime.StudyTimeName = s.Name;
                    teacherScheduleEmptyStudyTime.StartTime = s.StartTime;
                    teacherScheduleEmptyStudyTime.EndTime = s.EndTime;
                    teacherScheduleEmptyStudyTime.Date = dateStart.ToString("dd-MM-yyyy");
                    List<TeacherScheduleEmpty> listTeacherScheduleEmpty = new List<TeacherScheduleEmpty>();
                    foreach (var _class in listClass)
                    {
                        // lấy ds lịch học theo lớp,ca 
                        TimeSpan time1 = TimeSpan.Parse(s.StartTime);
                        var gioBdCaHoc = dateStart.Add(time1).ToString("yyyy-MM-dd HH:mm:ss");
                        TimeSpan time2 = TimeSpan.Parse(s.EndTime);
                        var gioKtCaHoc = dateStart.Add(time2).ToString("yyyy-MM-dd HH:mm:ss");
                        //var listScheduleByDay = await dbContext.tbl_Schedule.Where(x => x.ClassId == _class.Id
                        //                                                && x.Enable == true
                        //                                                && x.TeacherId == (search.TeacherId == 0 ? x.TeacherId : search.TeacherId)
                        //                                                && x.StartTime.Value.Year == dateStart.Year
                        //                                                && x.StartTime.Value.Month == dateStart.Month
                        //                                                && x.StartTime.Value.Day == dateStart.Day
                        //                                                && x.StartTime.Value<gioKtCaHoc
                        //                                                && x.EndTime.Value>gioBdCaHoc).OrderBy(x => x.StartTime).ToListAsync();
                        var ds = dateStart.ToString("yyyy-MM-dd");
                        var de = dateEnd.ToString("yyyy-MM-dd");
                        // cái này đã bao gồm giáo viên nghỉ phép
                        string sql = $"Get_ScheduleEmptyTeacher @ClassId ={_class.Id}," +
                                     $" @TeacherIds =  N'{search.TeacherId??""}'," +
                                     $"@DateStart =  N'{ds}'," +
                                     $"@DateEnd =  N'{de}'," +
                                     $"@GioBdCaHoc =  N'{gioBdCaHoc}'," +
                                     $"@GioKtCaHoc = N'{gioKtCaHoc}'";
                        var listScheduleByDay = await dbContext.SqlQuery<tbl_Schedule>(sql);
                        foreach (var teacher in listTeacher)
                        { 
                            if ( listScheduleByDay.Any(x => x.TeacherId != teacher.UserInformationId) || listScheduleByDay.Count == 0 )
                            {
                                TeacherScheduleEmpty teacherScheduleEmpty = new TeacherScheduleEmpty();
                                teacherScheduleEmpty.TeacherId = teacher.UserInformationId;
                                teacherScheduleEmpty.TeacherCode = teacher.UserCode;
                                teacherScheduleEmpty.TeacherName = teacher.FullName;
                                teacherScheduleEmpty.TeacherAvt = teacher.Avatar;
                                listTeacherScheduleEmpty.Add(teacherScheduleEmpty);
                            }
                        }
                    }
                    teacherScheduleEmptyStudyTime.TeacherScheduleEmpty = listTeacherScheduleEmpty;
                    teacherScheduleEmptyStudyTime.TotalRow = listTeacherScheduleEmpty.Count();
                    listTeacherScheduleEmptyStudyTime.Add(teacherScheduleEmptyStudyTime);
                }
                dateStart = dateStart.AddDays(1);
            } while (dateStart <= dateEnd);
            result.ListTeacherScheduleEmptyStudyTime = listTeacherScheduleEmptyStudyTime.OrderBy(x => x.StudyTimeId).ToList();
            result.TotalRow = listTeacherScheduleEmptyStudyTime.Count;
            return result;
        }
        public class TeacherScheduleEmptyResult
        {
            public int ProgramId { get; set; }
            public string ProgramName { get; set; }
            public int BranchId { get; set; }
            public string BranchName { get; set; }
            public DateTime DateStart { get; set; }
            public DateTime DateEnd { get; set; }
            public int TotalRow { get; set; }
            public List<TeacherScheduleEmptyStudyTime> ListTeacherScheduleEmptyStudyTime { get; set; }

        }
        public class TeacherScheduleEmptyStudyTime
        {
            public int StudyTimeId { get; set; }
            public string StudyTimeName { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string Date { get; set; }
            public int TotalRow { get; set; }
            public List<TeacherScheduleEmpty> TeacherScheduleEmpty { get; set; }
        }
        public class TeacherScheduleEmpty
        {
            public int TeacherId { get; set; }
            public string TeacherCode { get; set; }
            public string TeacherName { get; set; }
            public string TeacherAvt { get; set; }
        }




        public async Task<RoomScheduleEmptyResult> GetRoomScheduleEmpty(RoomScheduleEmptySearch search)
        {
            if (search.DateStart > search.DateEnd || search.DateStart < DateTime.Now.Date)
                throw new Exception("Chọn ngày chưa phù hợp");
            //lấy chi nhánh
            var branch = await dbContext.tbl_Branch.FirstOrDefaultAsync(x => x.Id == search.BranchId && x.Enable == true);
            if (branch == null)
                throw new Exception("Chi nhánh không tồn tại");
            //lấy ds phòng học
            var listRoom = new List<tbl_Room>();
            if (search.RooomId != null)
            {
                var splitRoom = search.RooomId.Split(',');
                foreach (var item in splitRoom)
                {
                    var room = await dbContext.tbl_Room.FirstOrDefaultAsync(x => x.Id.ToString() == item
                                                                    && x.Enable == true
                                                                     && x.BranchId == search.BranchId);
                    if (room != null)
                        listRoom.Add(room);
                }
            }
            else
                listRoom = await dbContext.tbl_Room.Where(x => x.Enable == true && x.BranchId == search.BranchId).ToListAsync();
            if (listRoom.Count == 0)
                throw new Exception("Không có phòng học nào tồn tại");
            // ds ca  
            var listStudyTime = new List<tbl_StudyTime>();
            if (search.StudyTime != null)
            {
                var sliptStudyTime = search.StudyTime.Split(',');
                foreach (var item in sliptStudyTime)
                {
                    var studyTime = await dbContext.tbl_StudyTime.FirstOrDefaultAsync(x => x.Id.ToString() == item && x.Enable == true);
                    if (studyTime != null)
                        listStudyTime.Add(studyTime);
                }
            }
            else
                listStudyTime = await dbContext.tbl_StudyTime.Where(x => x.Enable == true).ToListAsync();
            if (listStudyTime.Count == 0)
                throw new Exception("Không có ca học nào tồn tại");
            //----------------------------------------------------------------// 
            var dateStart = search.DateStart.Date;
            var dateEnd = search.DateEnd.Date;
            // lưu kết quả trả về
            RoomScheduleEmptyResult result = new RoomScheduleEmptyResult();
            result.BranchId = branch.Id;
            result.BranchName = branch.Name;
            result.DateStart = search.DateStart;
            result.DateEnd = search.DateEnd;
            List<RoomScheduleEmptyStudyTime> listRoomScheduleEmptyStudyTime = new List<RoomScheduleEmptyStudyTime>();
            do
            {
                foreach (var s in listStudyTime)
                {
                    RoomScheduleEmptyStudyTime emptyStudyTime = new RoomScheduleEmptyStudyTime();
                    emptyStudyTime.StudyTimeId = s.Id;
                    emptyStudyTime.StudyTimeName = s.Name;
                    emptyStudyTime.StartTime = s.StartTime;
                    emptyStudyTime.EndTime = s.EndTime;
                    emptyStudyTime.Date = dateStart.ToString("dd-MM-yyyy");
                    List<RoomScheduleEmpty> listRoomScheduleEmpty = new List<RoomScheduleEmpty>();
                    // lấy ds lịch học theo lớp,ca 
                    TimeSpan time1 = TimeSpan.Parse(s.StartTime);
                    var gioBdCaHoc = dateStart.Add(time1).ToString("yyyy-MM-dd HH:mm:ss"); ;
                    TimeSpan time2 = TimeSpan.Parse(s.EndTime);
                    var gioKtCaHoc = dateStart.Add(time2).ToString("yyyy-MM-dd HH:mm:ss"); ;
                    //var listScheduleByDay = await dbContext.tbl_Schedule.Where(x => x.RoomId == (search.RooomId == 0 ? x.RoomId : search.RooomId)
                    //                                                && x.Enable == true
                    //                                                && x.StartTime.Value.Date == dateStart
                    //                                                && x.StartTime.Value < gioKtCaHoc
                    //                                                && x.EndTime.Value > gioBdCaHoc).OrderBy(x => x.StartTime).ToListAsync();
                    var ds = dateStart.ToString("yyyy-MM-dd");
                    var de = dateEnd.ToString("yyyy-MM-dd");
                    string sql = $"Get_ScheduleEmptyRoom @RooomId = N'{search.RooomId??""}'," +
                                 $" @BranchId =  {search.BranchId}," +
                                 $"@DateStart =  N'{ds}'," + 
                                 $"@GioBdCaHoc =  N'{gioBdCaHoc}'," +
                                 $"@GioKtCaHoc = N'{gioKtCaHoc}'";
                    var listScheduleByDay = await dbContext.SqlQuery<tbl_Schedule>(sql);
                    foreach (var room in listRoom)
                    {
                        if (listScheduleByDay.Any(x => x.RoomId != room.Id) || listScheduleByDay.Count == 0)
                        {
                            RoomScheduleEmpty roomScheduleEmpty = new RoomScheduleEmpty();
                            roomScheduleEmpty.RoomId = room.Id;
                            roomScheduleEmpty.RoomName = room.Name;
                            listRoomScheduleEmpty.Add(roomScheduleEmpty);
                        }
                    }
                    emptyStudyTime.ListRoomScheduleEmpty = listRoomScheduleEmpty;
                    emptyStudyTime.TotalRow = listRoomScheduleEmpty.Count();
                    listRoomScheduleEmptyStudyTime.Add(emptyStudyTime);
                }
                dateStart = dateStart.AddDays(1);
            } while (dateStart <= dateEnd);

            result.ListEmptyStudyTime = listRoomScheduleEmptyStudyTime.OrderBy(x=>x.StudyTimeId).ToList();
            result.TotalRow = listRoomScheduleEmptyStudyTime.Count;
            return result;
        }
        public class RoomScheduleEmptyResult
        {
            public int BranchId { get; set; }
            public string BranchName { get; set; }
            public DateTime DateStart { get; set; }
            public DateTime DateEnd { get; set; }
            public int TotalRow { get; set; }
            public List<RoomScheduleEmptyStudyTime> ListEmptyStudyTime { get; set; }

        }
        public class RoomScheduleEmptyStudyTime
        {
            public int StudyTimeId { get; set; }
            public string StudyTimeName { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string Date { get; set; }
            public int TotalRow { get; set; }
            public List<RoomScheduleEmpty> ListRoomScheduleEmpty { get; set; }
        }
        public class RoomScheduleEmpty
        {
            public int RoomId { get; set; }
            public string RoomName { get; set; }
        }
       
        
        
        
        public async Task<AppDomainResult> GetStatisticTeacherSchedule(TeacherScheduleStatistic search)
        {
            // giáo viên tại trung tâm
            using (var db = new lmsDbContext())
            {
                var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == search.BranchId);
                if (branch == null)
                    throw new Exception("Chi nhánh không tồn tại");

                string sql = $"GetStatistic_TeacherSchedule @PageIndex = {search.PageIndex}," +
                        $"@branchId = {search.BranchId}," +
                        $"@month = {search.Month}," +
                        $"@year = {search.Year}," +
                        $"@PageSize = {search.PageSize}," +
                        $"@Search = N'{search.Search ?? ""}'";
                var data = await db.SqlQuery<Get_StatisticTeacherSchedule>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                return new AppDomainResult { TotalRow = data[0].TotalSchedule, Data = data };
            }
        }
        public class Get_StatisticTeacherSchedule
        {
            public int UserInformationId { get; set; }
            public string UserCode { get; set; }
            public string FullName { get; set; }
            public int Value { get; set; } 
            public int TotalSchedule { get; set; }
        }
    }
}