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
    public class DashboardService
    {
        public class CountModel
        {
            public string Type { get; set; }
            public int Count { get; set; }
        }
        #region Thống kê học viên
        public static async Task<List<CountModel>> OverviewModel()
        {
            using (var db = new lmsDbContext())
            {
                var data = new List<CountModel>();
                ///
                var CountStudent = await db.tbl_UserInformation.CountAsync(x=>x.RoleId == ((int)RoleEnum.student) && x.Enable == true);
                var CountCourse = await db.tbl_Product.CountAsync(x => x.Enable == true);
                var CountCertificate = await db.tbl_Certificate.CountAsync(x => x.Enable == true);
                var HoiThaoChuaDienRa = await db.tbl_Seminar.CountAsync(x => x.Status == (int)SeminarStatus.ChuaDienRa && x.Enable == true);
                var HoiThaoDangDienRa = await db.tbl_Seminar.CountAsync(x => x.Status == (int)SeminarStatus.DangDienRa && x.Enable == true);
                var HoiThaoDaKetThuc = await db.tbl_Seminar.CountAsync(x => x.Status == (int)SeminarStatus.KetThuc && x.Enable == true);

                data.Add(new CountModel { Type = "Học viên", Count = CountStudent });
                data.Add(new CountModel { Type = "Khóa học",Count=CountCourse });
                data.Add(new CountModel { Type = "Chứng chỉ", Count = CountCertificate });
                data.Add(new CountModel { Type = "Hội thảo chưa diễn ra", Count = HoiThaoChuaDienRa });
                data.Add(new CountModel { Type = "Hội thảo đang diễn ra", Count = HoiThaoDangDienRa });
                data.Add(new CountModel { Type = "Hội thảo đã kết thúc", Count = HoiThaoDaKetThuc });

                ///
                return data;

            }
        }

        //Danh sách thống kê mà giáo viên có thể xem
        public static async Task<List<CountModel>> OverviewModelForTeacher(tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var data = new List<CountModel>();
                ///
                var countStudent = await db.tbl_UserInformation.CountAsync(x => x.RoleId == ((int)RoleEnum.student) && x.Enable == true);
                var hoiThaoChuaDienRa = await db.tbl_Seminar.CountAsync(x => x.Status == (int)SeminarStatus.ChuaDienRa && x.Enable == true && x.LeaderId == user.UserInformationId);
                var hoiThaoDangDienRa = await db.tbl_Seminar.CountAsync(x => x.Status == (int)SeminarStatus.DangDienRa && x.Enable == true && x.LeaderId == user.UserInformationId);
                var hoiThaoDaKetThuc = await db.tbl_Seminar.CountAsync(x => x.Status == (int)SeminarStatus.KetThuc && x.Enable == true && x.LeaderId == user.UserInformationId);
                var hoiThaoCuaGV = await db.tbl_Seminar.CountAsync(x => x.LeaderId == user.UserInformationId&&x.Enable== true && x.LeaderId == user.UserInformationId);

                data.Add(new CountModel { Type = "Học viên", Count = countStudent });
                data.Add(new CountModel { Type = "Hội thảo chưa diễn ra", Count = hoiThaoChuaDienRa });
                data.Add(new CountModel { Type = "Hội thảo đang diễn ra", Count = hoiThaoDangDienRa });
                data.Add(new CountModel { Type = "Hội thảo đã kết thúc", Count = hoiThaoDaKetThuc });
                data.Add(new CountModel { Type = "Tổng hội thảo của tôi", Count = hoiThaoCuaGV });

                ///
                return data;

            }
        }

        public class CountModelInMonth
        {
            public string Type { get; set; }
            public int Count { get; set; } 
            public string Note { get; set; }
        }

        //Thống kê theo tháng
        public static async Task<List<CountModelInMonth>> OverviewModelInMonth()
        {
            var data = new List<CountModelInMonth>();
            using (var db = new lmsDbContext())
            {
                var CountStudentInMonth = await db.tbl_UserInformation.CountAsync(x => x.RoleId == ((int)RoleEnum.student) && x.Enable == true && x.CreatedOn.Value.Month==DateTime.Now.Month);
                var CountStudentPreMonth = await db.tbl_UserInformation.CountAsync(x => x.RoleId == ((int)RoleEnum.student) && x.Enable == true && x.CreatedOn.Value.Month == DateTime.Now.Month - 1);
                var CountCertificateInMonth = await db.tbl_Certificate.CountAsync(x => x.Enable == true && x.CreatedOn.Value.Month == DateTime.Now.Month);
                var CountCertificatePreMonth = await db.tbl_Certificate.CountAsync(x => x.Enable == true && x.CreatedOn.Value.Month == DateTime.Now.Month-1);
                //var CountCourse = await db.tbl_VideoCourse.CountAsync(x => x.Enable == true && x.CreatedOn.Value.Month == DateTime.Now.Month);
                var HoiThaoChuaDienRa = await db.tbl_Seminar.CountAsync(x => x.Status == (int)SeminarStatus.ChuaDienRa && x.Enable == true && x.CreatedOn.Value.Month == DateTime.Now.Month);
                var HoiThaoDangDienRa = await db.tbl_Seminar.CountAsync(x => x.Status == (int)SeminarStatus.DangDienRa && x.Enable == true && x.CreatedOn.Value.Month == DateTime.Now.Month);
                var HoiThaoDaKetThuc = await db.tbl_Seminar.CountAsync(x => x.Status == (int)SeminarStatus.KetThuc && x.Enable == true && x.CreatedOn.Value.Month == DateTime.Now.Month);
                double StatisticStudent = Math.Abs(CountStudentInMonth - CountStudentPreMonth);
                double StatisticCertificate = Math.Abs(CountCertificateInMonth - CountCertificatePreMonth);
                if (CountStudentInMonth < CountStudentPreMonth || CountCertificateInMonth<CountCertificatePreMonth)
                {
                    data.Add(new CountModelInMonth {Type= "Học viên mới trong tháng", Count = CountStudentInMonth, Note =$"Giảm {StatisticStudent} học viên so với tháng trước" });
                    data.Add(new CountModelInMonth {Type= "Chứng chỉ mới trong tháng", Count = CountCertificateInMonth, Note = $"Giảm {StatisticCertificate} chứng chỉ so với tháng trước" });   
                }
                else
                {
                    data.Add(new CountModelInMonth {Type="Học viên mới trong tháng", Count = CountStudentInMonth, Note = $"Tăng {StatisticStudent}học viên so với tháng trước"});
                    data.Add(new CountModelInMonth {Type="Học viên mới trong tháng", Count = CountStudentInMonth, Note = $"Tăng {StatisticCertificate}chứng chỉ so với tháng trước" });
                }
                return data;
            }
            
        }
        public class CountAgeStudent
        {
            /// <summary>
            /// 2 - tên loại thống kê
            /// 3 - số lượng trên hệ thống
            /// </summary>
            public int Type { get; set; }
            public string Note { get; set; }
            public int Count { get; set; }
        }

        //Thống kê học viên theo độ tuổi
        public static async Task<List<CountAgeStudent>> StatisticForAge()
        {
            var data = new List<CountAgeStudent>();
            using (var db = new lmsDbContext())
            {
                var model = new tbl_UserInformation();
                var Under18 = await (from age in db.tbl_UserInformation where (DateTime.Now.Year - age.DOB.Value.Year < 18 && age.Enable == true && age.RoleId==(int)RoleEnum.student) select age.DOB).CountAsync();
                var Over18 = await (from age in db.tbl_UserInformation where ((DateTime.Now.Year - age.DOB.Value.Year) >=18 && (DateTime.Now.Year - age.DOB.Value.Year) <25 && age.Enable == true && age.RoleId == (int)RoleEnum.student) select age.DOB).CountAsync();
                var Over25 = await (from age in db.tbl_UserInformation where ((DateTime.Now.Year - age.DOB.Value.Year) >= 25 && (DateTime.Now.Year - age.DOB.Value.Year) < 35 && age.Enable == true && age.RoleId == (int)RoleEnum.student) select age.DOB).CountAsync();
                var Over35 = await (from age in db.tbl_UserInformation where ((DateTime.Now.Year - age.DOB.Value.Year) >= 35 && (DateTime.Now.Year - age.DOB.Value.Year) < 45 && age.Enable == true && age.RoleId == (int)RoleEnum.student) select age.DOB).CountAsync();
                var Over45 = await (from age in db.tbl_UserInformation where ((DateTime.Now.Year - age.DOB.Value.Year) >= 45 && age.Enable == true && age.RoleId == (int)RoleEnum.student) select age.DOB).CountAsync();
                data.Add(new CountAgeStudent { Type = 1, Note = "Học viên dưới 18", Count = Under18 });
                data.Add(new CountAgeStudent { Type = 2, Note = "Học viên từ 18 đến 25", Count = Over18 });
                data.Add(new CountAgeStudent { Type = 3, Note = "Học viên từ 25 đến 35", Count = Over25 });
                data.Add(new CountAgeStudent { Type = 4, Note = "Học viên từ 35 đến 45", Count = Over35 });
                data.Add(new CountAgeStudent { Type = 5, Note = "Học viên trên 45", Count = Over45 });
                return data;
            }
        }
        public class Get_CountTopStudentInCourse
        {
            public int Id { get; set; }
            public string Name { get;set; }
            public int Total { get; set; }
        }

        //Thống kê top 5 khóa học có nhiều học viên nhất
        public static async Task<List<Get_CountTopStudentInCourse>> StatisticTopCourse()
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_CountTopCouseStudent";
                var data = await db.SqlQuery<Get_CountTopStudentInCourse>(sql);              
                return data;
            }
        }

        public class Get_CompleteCousre
        {
            public int Id { get; set; }
        }
        #endregion
        //Thống kê khóa học của học viên
        public static async Task<List<CountModel>> StatisticCourseStudent(tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var data = new List<CountModel>();
                //var hoiThaoDangDienRa = await (from ht in db.tbl_Seminar where(ht.Status == (int)SeminarStatus.DangDienRa && ht.Enable == true) select ht.Id).CountAsync();
                //var tongKhoaDangHoc = await (from kch in db.tbl_VideoCourseStudent
                //                                   join c in db.tbl_VideoCourse on kch.VideoCourseId equals c.Id
                //                                   where (kch.Enable == true && c.Id == kch.VideoCourseId && c.Enable == true && kch.UserId == user.UserInformationId)
                //                                   select c.Id).CountAsync();
                //var tongKhoaHoc = await (from kh in db.tbl_VideoCourse where (kh.Enable == true) select kh.Id).CountAsync();
                //var tongCauHoi = await (from ch in db.tbl_QuestionInVideo where (ch.Enable == true && ch.UserId == user.UserInformationId) select ch.Id).CountAsync();
                //int tongKhoaChuaHoc = tongKhoaHoc - tongKhoaDangHoc;
                //data.Add(new CountModel { Type = "Hội thảo đang diễn ra", Count = hoiThaoDangDienRa });
                //data.Add(new CountModel { Type = "Tổng khóa đang học", Count = tongKhoaDangHoc });
                //data.Add(new CountModel { Type = "Tổng khóa chưa học", Count = tongKhoaChuaHoc });
                //string sql = $"Get_CompleteCousre @UserId = {user.UserInformationId}";
                //var data2 = await db.SqlQuery<Get_CompleteCousre>(sql).CountAsync();
                //data.Add(new CountModel { Type = "Tổng khóa đã hoàn thành", Count = data2 });
                //data.Add(new CountModel { Type = "Tổng câu hỏi đã đặt", Count = tongCauHoi });
                return data;

            }
        }
        public class Get_Dashboard_LearningDetails
        { 
            public int Id { get; set; }
            public int? UserId { get; set; }
            public string Name { get; set; }
            public double Completed { get; set; }
            public double Lesson { get; set; }
            private double SetPercent { get; set; }
            public double Percent {
                get { return Math.Round(SetPercent, 2); }
                set { SetPercent = value; } }
        }
        public static async Task<List<Get_Dashboard_LearningDetails>> LearningDetails(tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_Dashboard_LearningDetails @UserId = {user.UserInformationId}";
                var data = await db.SqlQuery<Get_Dashboard_LearningDetails>(sql);
                return data;
            }
        }
        public class Get_Dashboard_OverviewLearning
        {
            public int UserInformationId { get;set;}
            public string FullName { get; set; }
            public string Email { get; set; }
            public string RoleName { get; set; }
            public int Course { get; set; }
            public int Completed { get; set; }
            public double Point { get; set; }
            public DateTime? CreatedOn { get; set; }
            public int TotalExam { get; set; }
            public int TotalCourse { get; set; }
            public int TotalCourseCompleted { get; set; }
            public int TotalRow { get; set; }
        }
        public class Dashboard_OverviewLearningModel
        {
            public int UserInformationId { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string RoleName { get; set; }
            public int Course { get; set; }
            public int Completed { get; set; }
            public double Point { get; set; }
            public DateTime? CreatedOn { get; set; }
        }
        public class OverviewLearningModel
        {
            public int TotalStudent { get; set; } = 0;
            public int TotalExam { get; set; } = 0;
            public int TotalCourseCompleted { get; set; } = 0;
            public int TotalCourse { get; set; } = 0;
            public int TotalRow { get; set; } = 0;
            public List<Dashboard_OverviewLearningModel> Data { get; set; }
        }
        public static async Task<OverviewLearningModel> OverviewLearning(OverviewSearch search)
        {
            using (var db = new lmsDbContext())
            {
                if (search == null) search = new OverviewSearch();
                string sql = $"Get_Dashboard_OverviewLearning @PageIndex = {search.PageIndex}, @PageSize = {search.PageSize}, @Search = {search.Search ?? "''"}";
                var data = await db.SqlQuery<Get_Dashboard_OverviewLearning>(sql);
                if (!data.Any()) return new OverviewLearningModel();
                var result = new OverviewLearningModel
                {
                    TotalStudent = data[0].TotalRow,
                    TotalCourse = data[0].TotalCourse,
                    TotalCourseCompleted = data[0].TotalCourseCompleted,
                    TotalRow = data[0].TotalRow,
                    TotalExam = data[0].TotalExam,
                    Data = data.Select(x => new Dashboard_OverviewLearningModel
                    {
                        Completed = x.Completed,
                        Course = x.Course,
                        Email = x.Email,
                        FullName = x.FullName,
                        Point = x.Point,
                        RoleName = x.RoleName,
                        CreatedOn = x.CreatedOn,
                        UserInformationId = x.UserInformationId,
                    }).ToList()
                };
                return result;
            }
        }
        public class Get_Dashboard_OverviewVideoCourse
        { 
            public int Id { get; set; }
            public string Name { get; set; }
            public int VideoCourse { get; set; }
            public int Completed { get; set; }
            public int TotalStudent { get; set; }
            public int TotalCompleted { get; set; }
            public int TotalRow { get; set; }
        }
        public class Dashboard_OverviewVideoCourseModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int VideoCourse { get; set; }
            public int Completed { get; set; }
        }
        public class OverviewVideoCourseModel
        {
            public int TotalVideoCourse { get; set; }
            public int TotalStudent { get; set; }
            public int TotalCompleted { get; set; }
            public int TotalRow { get; set; }
            public List<Dashboard_OverviewVideoCourseModel> Data { get; set; }
        }
        public static async Task<OverviewVideoCourseModel> OverviewVideoCourse(OverviewSearch search)
        {
            using (var db = new lmsDbContext())
            {
                if (search == null) search = new OverviewSearch();
                string sql = $"Get_Dashboard_OverviewVideoCourse @PageIndex = {search.PageIndex}, @PageSize = {search.PageSize}, @Search = {search.Search ?? "''"}";
                var data = await db.SqlQuery<Get_Dashboard_OverviewVideoCourse>(sql);
                if (!data.Any()) return new OverviewVideoCourseModel();
                var result = new OverviewVideoCourseModel
                {
                    TotalVideoCourse = data[0].TotalRow,
                    TotalStudent = data[0].TotalStudent,
                    TotalCompleted = data[0].TotalCompleted,
                    TotalRow = data[0].TotalRow,
                    Data = data.Select(x => new Dashboard_OverviewVideoCourseModel
                    {
                        Completed = x.Completed,
                        VideoCourse = x.VideoCourse,
                        Id = x.Id,
                        Name = x.Name
                    }).ToList()
                };
                return result;
            }
        }
        public class Get_Dashboard_OverviewExam
        { 
            public int Id { get; set; }
            public string Name { get; set; }
            public string VideoCourseName { get; set; }
            public int Completed { get; set; }
            public int Pass { get; set; }
            public double Medium { get; set; }
            public int TotalExam { get; set; }
            public int TotalCompleted { get; set; }
            public int TotalPass { get; set; }
            public double TotalMedium { get; set; }
            public int TotalRow { get; set; }
        }
        public class Dashboard_OverviewExam
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string VideoCourseName { get; set; }
            public int Completed { get; set; }
            public int Pass { get; set; }
            public double Medium { get; set; }
        }
        public class OverviewExamModel
        {
            public int TotalExam { get; set; }
            public int TotalCompleted { get; set; }
            public int TotalPass { get; set; }
            public double TotalMedium { get; set; }
            public int TotalRow { get; set; }
            public List<Dashboard_OverviewExam> Data { get; set; }
        }
        public static async Task<OverviewExamModel> OverviewExam(OverviewSearch search)
        {
            using (var db = new lmsDbContext())
            {
                if (search == null) search = new OverviewSearch();
                string sql = $"Get_Dashboard_OverviewExam @PageIndex = {search.PageIndex}, @PageSize = {search.PageSize}, @Search = {search.Search ?? "''"}";
                var data = await db.SqlQuery<Get_Dashboard_OverviewExam>(sql);
                if (!data.Any()) return new OverviewExamModel();
                var result = new OverviewExamModel
                {
                    TotalExam = data[0].TotalRow,
                    TotalMedium = Math.Round(data[0].TotalMedium,2),
                    TotalPass = data[0].TotalPass,
                    TotalCompleted = data[0].TotalCompleted,
                    TotalRow = data[0].TotalRow,
                    Data = data.Select(x => new Dashboard_OverviewExam
                    {
                        Completed = x.Completed,
                        Medium = Math.Round(x.Medium,2),
                        Pass = x.Pass,
                        VideoCourseName = x.VideoCourseName,
                        Id = x.Id,
                        Name = x.Name
                    }).ToList()
                };
                return result;
            }
        }
        public class StatisticialCustomerSearch
        {
            public string BranchIds { get; set; } = null;
        }
        public class StatisticialCustomerInYearSearch
        {
            public string BranchIds { get; set; } = null;
            public int? Year { get; set; }
        }
        /// <summary>
        /// Thống kê khách hàng và học theo tháng dành cho tư vấn viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static async Task<List<StatisticialCustomerInMonth>> StatisticialCustomerInMonth(StatisticialCustomerSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StatisticialCustomerSearch();
                string sql = $"GetStatistical_Chart_CustomerInMonth @BranchIds = {baseSearch.BranchIds}";
                var data = await db.SqlQuery<StatisticialCustomerInMonth>(sql);
                var result = new List<StatisticialCustomerInMonth>();
                if (data.Any())
                {
                    result = data.Select(x => new StatisticialCustomerInMonth
                    {
                        Name = x.Name,
                        ValueInMonth = x.ValueInMonth,
                        ValuePreInMonth = x.ValuePreInMonth,
                        Note = (x.ValueInMonth - x.ValuePreInMonth) >= 0 ? $"Tăng { Math.Abs(x.ValueInMonth - x.ValuePreInMonth)}" : $"Giảm { Math.Abs(x.ValueInMonth - x.ValuePreInMonth)}"
                    }).ToList();
                }
                return result;
            }
        
        }
        /// <summary>
        /// Thống kê khách hàng và học viên dành cho tư vấn viên
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static async Task<List<StatisticialAllCustomer>> StatisticialAllCustomer(StatisticialCustomerSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StatisticialCustomerSearch();
                string sql = $"GetStatistical_Chart_AllCustomer @BranchIds = '{baseSearch.BranchIds}'";
                var data = await db.SqlQuery<StatisticialAllCustomer>(sql);
                return data;
            }

        }

        
    }
}
