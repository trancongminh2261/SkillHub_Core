using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class TimeReport
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int LastMonth { get; set; }
        public int YearOfLastMonth { get; set; }
        public int LastYear { get; set; }
        public int Day { get; set; }
    }
    public class ReportService
    {
        public static TimeReport GetTime()
        {
            DateTime timeNow = DateTime.Now;
            TimeReport time = new TimeReport();
            time.Month = timeNow.Month;
            time.Year = timeNow.Year;
            time.LastMonth = time.Month - 1 == 0 ? 12 : time.Month - 1;
            time.YearOfLastMonth = time.LastMonth == 12 ? time.Year - 1 : time.Year;
            time.LastYear = timeNow.Year - 1;
            time.Day = timeNow.Day;
            return time;
        }

        #region Thống kê học viên - Student report
        /*public async Task<int> StudentReport(DateTime? fromDt, DateTime? toDt)
        {
            DateTime now = DateTime.Now;
            if (fromDt == null)
                fromDt = new DateTime(now.Year, now.Month, 1);
            if (toDt == null)
                toDt = now.AddDays(1).AddSeconds(-1);
            int result = 0;
            
            //học viên, leads được tạo trong tháng
            var students = await _db.tbl_UserInformation.Where(x => fromDt <= x.CreatedOn && x.CreatedOn <= toDt && x.Enable == true).ToListAsync();

            //Số lượng khách hàng tiềm năng được tạo(label)
            int numberOfLead = students.Count(x => x.LearningStatus == 1);

            //Tỷ lệ chuyển đổi từ khách hàng tiềm năng thành học viên đăng ký học(pie chart)
            int total = students.Count();
            //Xu hướng đi học của học sinh(hàng ngày, hàng tuần, hằng tháng)(line chart)
            //Phản hồi của sinh viên và chỉ số hài lòng – tổng số yêu cầu, bao nhiêu yêu cầu đượcphản hồi, piechart cho số sao
            //Tỷ lệ chuyển sinh viên: số học viên chuyển lớp, lớp được chuyển đi, chuyển vào nhiều nhất

            return result;
        }*/
        #endregion

        #region Thống kê lớp
        //thống kê số lớp đã tạo so với tháng trước
        public static async Task<ReportModel> GetStatisticalClassCreatedCompareLastMonth()
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    TimeReport time = GetTime();
                    var ListClassCreatedThisMonth = await _db.tbl_Class.Where(x => x.Enable == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                    var ListClassCreatedLastMonth = await _db.tbl_Class.Where(x => x.Enable == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth).ToListAsync();
                    string message = "";
                    double countThisMonth = ListClassCreatedThisMonth.Count();
                    double countLastMonth = ListClassCreatedLastMonth.Count();
                    double count = 0;
                    double percentage = 0;
                    //test lấy thử thông tin trong hàm get time
                    //result = "năm: " + time.Year + " tháng: " + time.Month + " năm trước: " + time.LastYear + " tháng trước: " + time.LastYear + " năm của tháng trước: " + time.YearOfLastMonth + " ngày: " + time.Day;
                    if (countLastMonth == 0 && countThisMonth == 0)
                    {
                        message = "Không có lớp được tạo trong 2 tháng qua";
                    }
                    if (countLastMonth == 0 && countThisMonth != 0)
                    {
                        message = "Tăng 100% so với tháng trước";
                    }
                    if (countLastMonth != 0 && countThisMonth != 0 && countThisMonth > countLastMonth)
                    {
                        count = countThisMonth - countLastMonth;
                        percentage = Math.Round(Math.Abs(count / countLastMonth * 100), 2);
                        if (percentage > 100)
                            percentage = 100;
                        message = "Tăng " + percentage + "% so với tháng trước";
                    }
                    if (countLastMonth != 0 && countThisMonth != 0 && countThisMonth < countLastMonth)
                    {
                        count = countLastMonth - countThisMonth;
                        percentage = Math.Round(Math.Abs(count / countLastMonth * 100), 2);
                        if (percentage > 100)
                            percentage = 100;
                        message = "Giảm " + percentage + "% ) so với tháng trước";
                    }
                    if (countLastMonth != 0 && countThisMonth != 0 && countThisMonth == countLastMonth)
                    {
                        message = "Không đổi so với tháng trước";
                    }
                    ReportModel reportModel = new ReportModel
                    {
                        Message = message,
                        Value = countThisMonth
                    };
                    return reportModel;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        #endregion

        #region Thống kê khách hàng
        //Thống kê số lượng khách hàng tiềm năng được tạo và so sánh với tháng trước
        #endregion

        #region Thống kê học viên
        //Thống kê tỉ lệ chuyển đổi từ khách hàng tiềm năng sang học viên hằng tháng
        public static async Task<List<LineChartDataPercent>> GetStatisticsConversionRate(int Year)
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    List<LineChartDataPercent> ListLineChartDataPercent = new List<LineChartDataPercent>();
                    for (int i = 1; i <= 12; i++)
                    {

                        double CustomerInMonth = await _db.tbl_Customer.Where(x => x.CreatedOn.Value.Year == Year && x.CreatedOn.Value.Month == i && x.Enable == true).CountAsync();
                        double StudentInMonth = await _db.tbl_UserInformation.Where(x => x.RoleId == 3 && x.CreatedOn.Value.Year == Year && x.CreatedOn.Value.Month == i && x.Enable == true).CountAsync();
                        double result = 0;
                        if (CustomerInMonth == 0)
                        {
                            result = 0;
                        }
                        if (CustomerInMonth > 0)
                        {
                            result = Math.Round(Math.Abs((StudentInMonth / CustomerInMonth * 100) - 100), 2);
                            if (result > 0)
                            {
                                result = 100;
                            }
                        }
                        LineChartDataPercent lineChartDataPercent = new LineChartDataPercent
                        {
                            Month = i,
                            Value = result
                        };
                        ListLineChartDataPercent.Add(lineChartDataPercent);
                    }
                    return ListLineChartDataPercent;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        //Thống kê số lượng học viên chuyển lớp hằng tháng
        public static async Task<List<LineChartData>> GetStatisticsStudentChangeClass(int Year)
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    List<LineChartData> ListLineChartData = new List<LineChartData>();
                    for (int i = 1; i <= 12; i++)
                    {

                        var result = await _db.tbl_ClassChange.Where(x => x.CreatedOn.Value.Year == Year && x.CreatedOn.Value.Month == i && x.Enable == true).CountAsync();
                        LineChartData lineChartData = new LineChartData
                        {
                            Month = i,
                            Value = result
                        };
                        ListLineChartData.Add(lineChartData);
                    }
                    return ListLineChartData;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        //Thống kê số lượng học viên đăng ký lớp hằng tháng
        public static async Task<List<LineChartData>> GetStatisticsStudentRegistrationClass(int Year)
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    List<LineChartData> ListLineChartData = new List<LineChartData>();
                    for (int i = 1; i <= 12; i++)
                    {

                        var result = await _db.tbl_ClassRegistration.Where(x => x.CreatedOn.Value.Year == Year && x.CreatedOn.Value.Month == i && x.Enable == true && x.Status != 3).CountAsync();
                        LineChartData lineChartData = new LineChartData
                        {
                            Month = i,
                            Value = result
                        };
                        ListLineChartData.Add(lineChartData);
                    }
                    return ListLineChartData;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        //Thống kê kết quả học tập của học viên
        public static async Task<List<PieChartData>> GetStatisticsStudentMediumScore(int Year)
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    List<PieChartData> ListPieChartData = new List<PieChartData>();
                    var TotalScoresGreater = new int[9];

                    var ListStudentAssessment = await _db.tbl_StudentAssessment
                        .Where(x => x.Enable == true && x.CreatedOn.Value.Year == Year)
                        .ToListAsync();

                    foreach (var item in ListStudentAssessment)
                    {
                        double ScoreListening = int.Parse(item.Listening);
                        double ScoreSpeaking = int.Parse(item.Speaking);
                        double ScoreReading = int.Parse(item.Reading);
                        double ScoreWriting = int.Parse(item.Writing);
                        double MediumScore = Math.Round(((ScoreListening + ScoreSpeaking + ScoreReading + ScoreWriting) / 4), 1);

                        for (int i = 0; i < TotalScoresGreater.Length; i++)
                        {
                            if (MediumScore > i)
                            {
                                TotalScoresGreater[i] += 1;
                            }
                        }
                    }

                    for (int i = 0; i < TotalScoresGreater.Length; i++)
                    {
                        int j = i + 1;
                        PieChartData pieChartData = new PieChartData
                        {
                            Category = i + " - " + j,
                            Value = TotalScoresGreater[i],
                        };
                        ListPieChartData.Add(pieChartData);
                    }
                    return ListPieChartData;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion

        #region Thống kê nhân viên
        //Thống kê vai trò của nhân viên trong hệ thống 
        public static async Task<List<PieChartData>> GetStatisticsStaffRole()
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    List<PieChartData> ListPieChartData = new List<PieChartData>();
                    var RollIds = new List<int?> { 2, 4, 5, 6, 7 };
                    double TotalStaff = await _db.tbl_UserInformation
                        .Where(x => x.Enable == true && RollIds.Contains(x.RoleId))
                        .CountAsync();

                    //double TotalStaff = await _db.tbl_UserInformation.Where(x => x.Enable == true && (x.RoleId == 2 || x.RoleId == 4 || x.RoleId == 5 || x.RoleId == 6 || x.RoleId == 7)).CountAsync();

                    foreach (var roleId in RollIds)
                    {
                        double TotalStaffByRoll = 0;
                        TotalStaffByRoll = await _db.tbl_UserInformation
                                .Where(x => x.Enable == true && x.RoleId == roleId)
                                .CountAsync();

                        var Percentage = Math.Round(Math.Abs((TotalStaffByRoll / TotalStaff * 100) - 100), 2);

                        var staff = await _db.tbl_UserInformation
                            .Where(x => x.Enable == true && x.RoleId == roleId)
                            .FirstOrDefaultAsync();

                        var RollName = staff.RoleName;

                        PieChartData pieChartData = new PieChartData
                        {
                            Category = RollName,
                            Value = Percentage,
                        };

                        ListPieChartData.Add(pieChartData);
                    }

                    return ListPieChartData;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion

        #region Thống kê tài chính
        //Thống kê số lượng thanh toán chờ duyệt, đã duyệt, và không duyệt mỗi tháng
        public static async Task<List<MultiLineChartData>> GetStatisticsManagePayment(int Year)
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    List<MultiLineChartData> ListMultiLineChartData = new List<MultiLineChartData>();
                    for (int i = 1; i <= 12; i++)
                    {
                        //số liệu danh sách thanh toán chờ duyệt
                        var AwaitPayment = await _db.tbl_PaymentApprove.Where(x => x.CreatedOn.Value.Year == Year && x.CreatedOn.Value.Month == i && x.Enable == true && x.Status == 1).CountAsync();
                        MultiLineChartData AwaitPaymentData = new MultiLineChartData();
                        AwaitPaymentData.Month = i;
                        AwaitPaymentData.Category = "Chờ duyệt";
                        AwaitPaymentData.Value = AwaitPayment;
                        ListMultiLineChartData.Add(AwaitPaymentData);
                        //số liệu danh sách thanh toán đã duyệt
                        var ApprovePayment = await _db.tbl_PaymentApprove.Where(x => x.CreatedOn.Value.Year == Year && x.CreatedOn.Value.Month == i && x.Enable == true && x.Status == 2).CountAsync();
                        MultiLineChartData ApprovePaymentData = new MultiLineChartData();
                        ApprovePaymentData.Month = i;
                        ApprovePaymentData.Category = "Đã duyệt";
                        ApprovePaymentData.Value = ApprovePayment;
                        ListMultiLineChartData.Add(ApprovePaymentData);
                        //số liệu danh sách thanh toán không duyệt
                        var CancelPayment = await _db.tbl_PaymentApprove.Where(x => x.CreatedOn.Value.Year == Year && x.CreatedOn.Value.Month == i && x.Enable == true && x.Status == 3).CountAsync();
                        MultiLineChartData CancelPaymentData = new MultiLineChartData();
                        CancelPaymentData.Month = i;
                        CancelPaymentData.Category = "Không duyệt";
                        CancelPaymentData.Value = CancelPayment;
                        ListMultiLineChartData.Add(CancelPaymentData);
                    }
                    return ListMultiLineChartData;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion

        #region Thống kê khóa học video
        //thống kê số lượng khóa học video
        public static async Task<int> GetStatisticsVideoCoursesAvailable()
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    var result = await _db.tbl_VideoCourseStudent.Where(x => x.Enable == true).CountAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion

        #region Báo cáo tài chính
        //Thống kê doanh thu theo từng loại
        public static async Task<List<ReportPaymentModel>> GetStatisticsPaymentByCategory()
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    TimeReport time = GetTime();
                    List<ReportPaymentModel> ListReportPaymentModel = new List<ReportPaymentModel>();
                    for (int i = 1; i <= 5; i++)
                    {
                        double Percentage = 0;
                        string Message = "";
                        string Increase = "Tăng ";
                        string Decrease = "Giảm ";
                        string RestString = " so với tháng trước";
                        //lấy loại thanh toán
                        var Category = await _db.tbl_Bill.Where(x => x.Type == i).FirstOrDefaultAsync();
                        //var CategoryName = "Doanh thu từ " + Category.TypeName;
                        //lấy danh sách bill tháng này
                        var ListBillThisMonth = await _db.tbl_Bill.Where(x => x.Enable == true && x.Type == i && x.CreatedOn.Value.Month == time.Month).ToListAsync();
                        //lấy danh sách bill tháng trước
                        var ListBillLastMonth = await _db.tbl_Bill.Where(x => x.Enable == true && x.Type == i && x.CreatedOn.Value.Month == time.LastMonth).ToListAsync();

                        //Tính doanh thu tháng này
                        double TotalRevenueThisMonth = 0;
                        foreach (var item in ListBillThisMonth)
                        {
                            TotalRevenueThisMonth += item.Paid;
                        }
                        //Tính doanh thu tháng trước
                        double TotalRevenueLastMonth = 0;
                        foreach (var item in ListBillLastMonth)
                        {
                            TotalRevenueLastMonth += item.Paid;
                        }

                        //Nhận xét
                        //Nếu tháng trước = 0 && tháng này == 0
                        if (TotalRevenueLastMonth == 0 && TotalRevenueThisMonth == 0)
                        {
                            Message = "Không có doanh thu trong 2 tháng gần đây";
                        }
                        //Nếu tháng trước = 0 && tháng này > 0 => tăng 100%
                        if (TotalRevenueLastMonth == 0 && TotalRevenueThisMonth > 0)
                        {
                            Message = Increase + "100%" + RestString;
                        }
                        //Nếu tháng trước > 0 && tháng này = 0 => giảm 100%
                        if (TotalRevenueLastMonth > 0 && TotalRevenueThisMonth == 0)
                        {
                            Message = Decrease + "100%" + RestString;
                        }
                        if (TotalRevenueLastMonth > 0 && TotalRevenueLastMonth > 0)
                        {
                            //tính giá trị % tăng giảm
                            Percentage = Math.Round(Math.Abs((TotalRevenueThisMonth / TotalRevenueLastMonth * 100) - 100), 2);
                            //Nếu tháng trước < tháng này => tăng %
                            if (TotalRevenueLastMonth < TotalRevenueThisMonth)
                            {
                                Message = Increase + Percentage + "%" + RestString;
                            }
                            //Nếu tháng trước > tháng này => giảm %
                            if (TotalRevenueLastMonth > TotalRevenueThisMonth)
                            {
                                Message = Decrease + Percentage + "%" + RestString;
                            }
                        }
                        ReportPaymentModel reportRevenue = new ReportPaymentModel
                        {
                            Category = "Doanh thu từ " + Category.TypeName,
                            Message = Message,
                            Value = TotalRevenueThisMonth
                        };
                        ListReportPaymentModel.Add(reportRevenue);

                        //Tính tiền nợ tháng này
                        double TotalDebtThisMonth = 0;
                        foreach (var item in ListBillThisMonth)
                        {
                            TotalDebtThisMonth += item.Debt;
                        }
                        //Tính tiền nợ tháng trước
                        double TotalDebtLastMonth = 0;
                        foreach (var item in ListBillLastMonth)
                        {
                            TotalDebtLastMonth += item.Debt;
                        }
                        //Nhận xét
                        //Nếu tháng trước = 0 && tháng này == 0
                        if (TotalDebtLastMonth == 0 && TotalDebtThisMonth == 0)
                        {
                            Message = "Không có tiền nợ trong 2 tháng gần đây";
                        }
                        //Nếu tháng trước = 0 && tháng này > 0 => tăng 100%
                        if (TotalDebtLastMonth == 0 && TotalDebtThisMonth > 0)
                        {
                            Message = Increase + "100%" + RestString;
                        }
                        //Nếu tháng trước > 0 && tháng này = 0 => giảm 100%
                        if (TotalDebtLastMonth > 0 && TotalDebtThisMonth == 0)
                        {
                            Message = Decrease + "100%" + RestString;
                        }

                        if (TotalDebtLastMonth > 0 && TotalDebtThisMonth > 0)
                        {
                            //tính giá trị % tăng giảm
                            Percentage = Math.Round(Math.Abs((TotalDebtThisMonth / TotalDebtLastMonth * 100) - 100), 2);
                            //Nếu tháng trước < tháng này => tăng %
                            if (TotalDebtLastMonth < TotalDebtThisMonth)
                            {
                                Message = Increase + Percentage + "%" + RestString;
                            }
                            //Nếu tháng trước > tháng này => giảm %
                            if (TotalDebtLastMonth > TotalDebtThisMonth)
                            {
                                Message = Decrease + Percentage + "%" + RestString;
                            }
                        }

                        ReportPaymentModel reportDebt = new ReportPaymentModel
                        {
                            Category = "Tiền nợ từ " + Category.TypeName,
                            Message = Message,
                            Value = TotalDebtThisMonth
                        };

                        ListReportPaymentModel.Add(reportDebt);
                    }
                    return ListReportPaymentModel;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        #endregion

        #region Thống kê lịch test
        //Thống kê số buổi test được tạo hằng tháng
        public static async Task<List<LineChartData>> GetStatisticsTestAppointment(int Year)
        {
            using (lmsDbContext _db = new lmsDbContext())
            {
                try
                {
                    List<LineChartData> ListLineChartData = new List<LineChartData>();
                    for (int i = 1; i <= 12; i++)
                    {

                        var result = await _db.tbl_TestAppointment.Where(x => x.CreatedOn.Value.Year == Year && x.CreatedOn.Value.Month == i && x.Enable == true).CountAsync();
                        LineChartData lineChartData = new LineChartData
                        {
                            Month = i,
                            Value = result
                        };
                        ListLineChartData.Add(lineChartData);
                    }
                    return ListLineChartData;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion
    }
}