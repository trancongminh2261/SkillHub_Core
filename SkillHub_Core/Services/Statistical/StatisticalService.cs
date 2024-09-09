using LMSCore.Areas.Models;
using LMSCore.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.DashboardService;
using static LMSCore.DTO.StatisticalOfClassDTO;

namespace LMSCore.Services.Statistical
{
    public class StatisticalService : DomainService
    {
        public StatisticalService(lmsDbContext dbContext) : base(dbContext) { }

        #region model + xử lý data
        public class Time
        {
            public int Month { get; set; }
            public int Year { get; set; }
            public int LastMonth { get; set; }
            public int YearOfLastMonth { get; set; }
            public int LastYear { get; set; }
            public int Day { get; set; }
        }

        public static Time GetTimeModel(int? month, int? year)
        {
            DateTime timeNow = DateTime.Now;
            Time time = new Time();
            time.Month = month ?? DateTime.Now.Month;
            time.Year = year ?? DateTime.Now.Year;
            time.LastMonth = time.Month - 1 == 0 ? 12 : time.Month - 1;
            time.YearOfLastMonth = time.LastMonth == 12 ? time.Year - 1 : time.Year;
            time.LastYear = time.Year - 1;
            time.Day = timeNow.Day;
            return time;
        }
        public class StatisticalModel
        {
            public string Type { get; set; }
            public double Value { get; set; } = 0;
        }

        public class StatisticalDescriptionModel : StatisticalModel
        {
            public string Description { get; set; }
        }

        public class StatisticalDetailModel : StatisticalModel
        {
            //ví dụ value là tỉ lệ thì value detail là con số cụ thể
            public double ValueDetail { get; set; } = 0;
        }

        //dùng cho các biểu đồ show dữ liệu 12 tháng trong năm
        public class Statistical12MonthModel
        {
            public string Month { get; set; }
            public string Type { get; set; }
            public double Value { get; set; } = 0;
        }
        //dùng cho các biểu đồ so sánh dữ liệu năm được chọn và năm trước đó
        public class StatisticalCompareModel
        {
            /// <summary>
            /// tiêu đề => trong ant design tiêu đề là type nên đặt vậy luôn FE đỡ rối
            /// </summary>
            public string Type { get; set; }
            /// <summary>
            /// dữ liệu năm được chọn 
            /// </summary>
            public List<StatisticalModel> DataInYear { get; set; }
            /// <summary>
            /// dữ liệu năm được chọn - 1
            /// </summary>
            public List<StatisticalModel> DataPreYear { get; set; }
        }

        // bổ sung thêm nhận xét tăng hay giảm so với năm trước
        public class StatisticalCommentModel : StatisticalModel
        {
            /// <summary>
            /// số lượng chênh lệch
            /// </summary>
            public double? DifferenceQuantity { get; set; }
            /// <summary>
            /// tỷ lệ chênh lệch
            /// </summary>
            public double? DifferenceValue { get; set; }
            /// <summary>
            /// 1 - tăng
            /// 2 - giảm
            /// 3 - không đổi
            /// </summary>
            public int? Status { get; set; }
            [JsonIgnore]
            public string StatusName
            {
                get
                {
                    return Status == 1 ? "tăng"
                        : Status == 2 ? "giảm"
                        : Status == 3 ? "không đổi" : null;
                }
            }
        }
        public class CompareModel
        {
            /// <summary>
            /// số lượng chênh lệch
            /// </summary>
            public double? DifferenceQuantity { get; set; }
            /// <summary>
            /// tỷ lệ chênh lệch
            /// </summary>
            public double? DifferenceValue { get; set; }
            /// <summary>
            /// 1 - tăng
            /// 2 - giảm
            /// 3 - không đổi
            /// </summary>
            public int? Status { get; set; }
        }
        public static CompareModel CompareProgress(double thisMonth, double lastMonth)
        {
            double differenceQuantityValue = 0;
            double differenceRateValue = 0;
            int status = 3;
            //tháng này > 0 && tháng trước = 0 ( tăng 100% )
            if (thisMonth > 0 && lastMonth == 0)
            {
                differenceQuantityValue = Math.Abs(thisMonth);
                differenceRateValue = 100;
                status = 1;
            }
            //tháng này = 0 && tháng trước > 0 ( giảm 100% )
            if (thisMonth == 0 && lastMonth > 0)
            {
                differenceQuantityValue = Math.Abs(lastMonth);
                differenceRateValue = 100;
                status = 2;
            }

            //tháng này > 0 && tháng trước > 0
            if (thisMonth > 0 && lastMonth > 0)
            {
                //chênh lệch
                differenceQuantityValue = Math.Round(Math.Abs(thisMonth - lastMonth), 2);
                differenceRateValue = Math.Round(differenceQuantityValue / lastMonth * 100, 2);
                //tháng này > tháng trước ( tăng percent% )
                if (thisMonth > lastMonth)
                {
                    status = 1;
                }
                //tháng này < tháng trước (giảm percent% )
                if (thisMonth < lastMonth)
                {
                    status = 2;
                }
            }
            return new CompareModel { DifferenceQuantity = differenceQuantityValue, DifferenceValue = differenceRateValue, Status = status };
        }
        #endregion

        #region báo cáo thống kê khách hàng

        #region thống kê số liệu tổng quan
        public async Task<List<StatisticalModel>> CustomerOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var compare = new CompareModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                //số lượng khách hàng
                totalData = await dbContext.tbl_Customer.CountAsync(x => x.Enable == true && x.BranchId != null
                && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data = new StatisticalModel
                {
                    Type = "Tổng khách hàng",
                    Value = totalData
                };
                result.Add(data);
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                //số lượng khách hàng
                totalData = await dbContext.tbl_Customer.CountAsync(x => x.Enable == true && x.SaleId == userLogin.UserInformationId);
                data = new StatisticalModel
                {
                    Type = "Tổng khách hàng",
                    Value = totalData
                };
                result.Add(data);
            }
            #endregion

            return result;
        }

        public async Task<List<StatisticalCommentModel>> CustomerCompareOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalCommentModel>();
            var data = new StatisticalCommentModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var compare = new CompareModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                //khách hàng mới trong tháng
                totalDataInMonth = await dbContext.tbl_Customer.CountAsync(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString()))
                    && x.CreatedOn.HasValue && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Customer.CountAsync(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString()))
                    && x.CreatedOn.HasValue && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Khách hàng mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số khách hàng cần tư vấn
                totalDataInMonth = await dbContext.tbl_CustomerHistory.Where(x => x.Enable == true &&
                    x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString()))
                    && x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year &&
                    x.CustomerStatusId == 1)
                    .Select(x => x.CustomerId)
                    .Distinct()
                    .CountAsync();
                totalDataPreMonth = await dbContext.tbl_CustomerHistory.Where(x => x.Enable == true &&
                    x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString()))
                    && x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth &&
                    x.CustomerStatusId == 1)
                    .Select(x => x.CustomerId)
                    .Distinct()
                    .CountAsync();
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Khách hàng cần tư vấn",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lịch hẹn test trong tháng
                totalDataInMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString()))
                    && x.CreatedOn.HasValue && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString()))
                    && x.CreatedOn.HasValue && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lịch hẹn test mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lịch hẹn test đã diễn ra trong tháng
                totalDataInMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true &&
                    x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())) &&
                    x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Year == time.Year &&
                    dbContext.tbl_UserInformation.Any(y => y.UserInformationId == x.StudentId && y.LearningStatus == 2) == true);
                totalDataPreMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true &&
                    x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())) &&
                    x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.LastMonth &&
                    x.CreatedOn.Value.Year == time.YearOfLastMonth &&
                    dbContext.tbl_UserInformation.Any(y => y.UserInformationId == x.StudentId && y.LearningStatus == 2) == true);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lịch hẹn test đã diễn ra trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //khách hàng đăng ký học sau test trong tháng
                totalDataInMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true &&
                    x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())) &&
                    x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.Month &&
                    x.CreatedOn.Value.Year == time.Year &&
                    dbContext.tbl_UserInformation.Any(y => y.UserInformationId == x.StudentId && y.LearningStatus == 4) == true);
                totalDataPreMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true &&
                    x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())) &&
                    x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.LastMonth &&
                    x.CreatedOn.Value.Year == time.YearOfLastMonth &&
                    dbContext.tbl_UserInformation.Any(y => y.UserInformationId == x.StudentId && y.LearningStatus == 4) == true);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "khách hàng đăng ký học sau test trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                //khách hàng mới trong tháng
                totalDataInMonth = await dbContext.tbl_Customer.CountAsync(x => x.Enable == true
                    && x.SaleId == userLogin.UserInformationId
                    && x.CreatedOn.HasValue
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Customer.CountAsync(x => x.Enable == true
                    && x.SaleId == userLogin.UserInformationId
                    && x.CreatedOn.HasValue
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Khách hàng mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số khách hàng cần tư vấn
                totalDataInMonth = await dbContext.tbl_CustomerHistory.Where(x => x.Enable == true &&
                    x.SaleId == userLogin.UserInformationId && x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year &&
                    x.CustomerStatusId == 1)
                    .Select(x => x.CustomerId)
                    .Distinct()
                    .CountAsync();
                totalDataPreMonth = await dbContext.tbl_CustomerHistory.Where(x => x.Enable == true &&
                    x.SaleId == userLogin.UserInformationId && x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth &&
                    x.CustomerStatusId == 1)
                    .Select(x => x.CustomerId)
                    .Distinct()
                    .CountAsync();
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Khách hàng cần tư vấn",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lịch hẹn test trong tháng
                var studentIds = await dbContext.tbl_UserInformation
                    .Where(x => x.SaleId == userLogin.UserInformationId && x.Enable == true)
                    .Select(x => x.UserInformationId).ToListAsync();
                totalDataInMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true
                    && x.StudentId.HasValue
                    && studentIds.Any(s => s == x.StudentId.Value)
                    && x.CreatedOn.HasValue
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true
                    && x.StudentId.HasValue
                    && studentIds.Any(s => s == x.StudentId.Value)
                    && x.CreatedOn.HasValue
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lịch hẹn test mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lịch hẹn test đã diễn ra trong tháng
                var studentIdStatus2s = await dbContext.tbl_UserInformation
                    .Where(x => x.SaleId == userLogin.UserInformationId && x.Enable == true && x.LearningStatus == 2)
                    .Select(x => x.UserInformationId).ToListAsync();
                totalDataInMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true
                    && x.StudentId.HasValue
                    && studentIdStatus2s.Any(s => s == x.StudentId.Value)
                    && x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.Month &&
                    x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true
                    && x.StudentId.HasValue
                    && studentIdStatus2s.Any(s => s == x.StudentId.Value)
                    && x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.LastMonth &&
                    x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lịch hẹn test đã diễn ra trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //khách hàng đăng ký học sau test trong tháng
                var studentIdStatus4s = await dbContext.tbl_UserInformation
                    .Where(x => x.SaleId == userLogin.UserInformationId && x.Enable == true && x.LearningStatus == 4)
                    .Select(x => x.UserInformationId).ToListAsync();
                totalDataInMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true
                    && x.StudentId.HasValue
                    && studentIdStatus4s.Any(s => s == x.StudentId.Value)
                    && x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.Month &&
                    x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true
                    && x.StudentId.HasValue
                    && studentIdStatus4s.Any(s => s == x.StudentId.Value)
                    && x.CreatedOn.HasValue &&
                    x.CreatedOn.Value.Month == time.LastMonth &&
                    x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "khách hàng đăng ký học sau test trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion

            return result;
        }
        #endregion

        #region Biểu đồ tỷ lệ từng nguồn khách hàng
        public async Task<List<StatisticalDetailModel>> CustomerBySource(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var result = new List<StatisticalDetailModel>();
            //danh sách nguồn khách hàng
            var listSource = await dbContext.tbl_Source.Where(x => x.Enable == true).ToListAsync();
            if (listSource.Count <= 0) return result;

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                //danh sách khách hàng
                var listCustomer = await dbContext.tbl_Customer.Where(x => x.Enable == true && x.BranchId != null && x.SourceId != null && x.SourceId != 0
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listCustomer.Count <= 0) return result;
                foreach (var item in listSource)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double countData = listCustomer.Count(x => x.SourceId == item.Id);
                    data.Value = 0;
                    if (listCustomer.Count > 0)
                        data.Value = Math.Round(countData / listCustomer.Count * 100, 2);
                    data.ValueDetail = countData;
                    result.Add(data);
                }
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                //danh sách khách hàng
                var listCustomer = await dbContext.tbl_Customer.Where(x => x.Enable == true
                    && x.SourceId != null && x.SourceId != 0
                    && x.SaleId == userLogin.UserInformationId
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listCustomer.Count <= 0) return result;
                foreach (var item in listSource)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double countData = listCustomer.Count(x => x.SourceId == item.Id);
                    data.Value = 0;
                    if (listCustomer.Count > 0)
                        data.Value = Math.Round(countData / listCustomer.Count * 100, 2);
                    data.ValueDetail = countData;
                    result.Add(data);
                }
            }
            #endregion
            return result;
        }
        #endregion

        #region Biểu đồ tỷ lệ nhu cầu học
        public async Task<List<StatisticalDetailModel>> CustomerByLearningNeed(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalDetailModel>();
            var listLearningNeed = await dbContext.tbl_LearningNeed.Where(x => x.Enable == true).ToListAsync();
            if (listLearningNeed.Count <= 0) return result;
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                var listCustomer = await dbContext.tbl_Customer.Where(x => x.Enable == true && x.BranchId != null && x.LearningNeedId != null && x.LearningNeedId != 0
                && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listCustomer.Count <= 0) return result;
                foreach (var item in listLearningNeed)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double customerByLearningNeed = listCustomer.Count(x => x.LearningNeedId == item.Id);
                    data.Value = 0;
                    if (listCustomer.Count > 0)
                        data.Value = Math.Round(customerByLearningNeed / listCustomer.Count * 100, 2);
                    data.ValueDetail = customerByLearningNeed;
                    result.Add(data);
                }
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                var listCustomer = await dbContext.tbl_Customer.Where(x => x.Enable == true
                    && x.LearningNeedId != null && x.LearningNeedId != 0
                    && x.SaleId == userLogin.UserInformationId
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listCustomer.Count <= 0) return result;
                foreach (var item in listLearningNeed)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double customerByLearningNeed = listCustomer.Count(x => x.LearningNeedId == item.Id);
                    data.Value = 0;
                    if (listCustomer.Count > 0)
                        data.Value = Math.Round(customerByLearningNeed / listCustomer.Count * 100, 2);
                    data.ValueDetail = customerByLearningNeed;
                    result.Add(data);
                }
            }
            #endregion
            return result;
        }
        #endregion

        #region Biểu đồ tỷ lệ mục đích học
        public async Task<List<StatisticalDetailModel>> CustomerByLearningPurpose(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalDetailModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var listLearningPurpose = await dbContext.tbl_Purpose.Where(x => x.Enable == true).ToListAsync();
            if (listLearningPurpose.Count <= 0) return result;

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                var listCustomer = await dbContext.tbl_Customer.Where(x => x.Enable == true && x.BranchId != null && x.PurposeId != null && x.PurposeId != 0
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listCustomer.Count <= 0) return result;
                foreach (var item in listLearningPurpose)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double customerByLearningPurpose = listCustomer.Count(x => x.PurposeId == item.Id);
                    data.Value = 0;
                    if (listCustomer.Count > 0)
                        data.Value = Math.Round(customerByLearningPurpose / listCustomer.Count * 100, 2);
                    data.ValueDetail = customerByLearningPurpose;
                    result.Add(data);
                }
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                var listCustomer = await dbContext.tbl_Customer.Where(x => x.Enable == true && x.BranchId != null
                    && x.PurposeId != null && x.PurposeId != 0
                    && x.SaleId == userLogin.UserInformationId
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listCustomer.Count <= 0) return result;
                foreach (var item in listLearningPurpose)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double customerByLearningPurpose = listCustomer.Count(x => x.PurposeId == item.Id);
                    data.Value = 0;
                    if (listCustomer.Count > 0)
                        data.Value = Math.Round(customerByLearningPurpose / listCustomer.Count * 100, 2);
                    data.ValueDetail = customerByLearningPurpose;
                    result.Add(data);
                }
            }
            #endregion

            return result;
        }
        #endregion

        #region thống kê số khách hàng mới trong 12 tháng     
        public async Task<List<Statistical12MonthModel>> NewCustomer12Month(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<Statistical12MonthModel>();
            var data = new Statistical12MonthModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                var listCustomerInYear = await dbContext.tbl_Customer.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                var listCustomerPreYear = await dbContext.tbl_Customer.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Year == time.Year - 1).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new Statistical12MonthModel();
                    data.Month = "Tháng " + i;
                    data.Type = "Khách hàng mới năm nay";
                    if (listCustomerInYear.Count > 0)
                        data.Value = listCustomerInYear.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
                for (int i = 1; i <= 12; i++)
                {
                    data = new Statistical12MonthModel();
                    data.Month = "Tháng " + i;
                    data.Type = "Khách hàng mới năm ngoái";
                    if (listCustomerPreYear.Count > 0)
                        data.Value = listCustomerPreYear.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                var listCustomerInYear = await dbContext.tbl_Customer.Where(x => x.Enable == true
                    && x.SaleId == userLogin.SaleId
                    && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                var listCustomerPreYear = await dbContext.tbl_Customer.Where(x => x.Enable == true
                    && x.SaleId == userLogin.SaleId
                    && x.CreatedOn.Value.Year == time.Year - 1).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new Statistical12MonthModel();
                    data.Month = "Tháng " + i;
                    data.Type = "Khách hàng mới năm nay";
                    if (listCustomerInYear.Count > 0)
                        data.Value = listCustomerInYear.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
                for (int i = 1; i <= 12; i++)
                {
                    data = new Statistical12MonthModel();
                    data.Month = "Tháng " + i;
                    data.Type = "Khách hàng mới năm ngoái";
                    if (listCustomerPreYear.Count > 0)
                        data.Value = listCustomerPreYear.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion
            return result;
        }
        #endregion

        #region thống kê tỷ lệ chuyển đổi khách hàng
        public async Task<List<StatisticalModel>> ConversionRateStatistics(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var listCustomerStatus = await dbContext.tbl_CustomerStatus.Where(x => x.Enable == true).ToListAsync();

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                var listCustomerHistory = await dbContext.tbl_CustomerHistory.Where(x => x.Enable == true && x.BranchId != null
                 && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString()))
                 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year)
                 .ToListAsync();
                if (listCustomerStatus.Count > 0)
                {
                    foreach (var item in listCustomerStatus)
                    {
                        var data = new StatisticalModel();
                        data.Type = item.Name;
                        double countData = listCustomerHistory.Count(x => x.CustomerStatusId == item.Id);
                        data.Value = 0;
                        if (listCustomerHistory.Count > 0)
                        {
                            data.Value = Math.Round(countData / listCustomerHistory.Count * 100, 2);
                        }
                        result.Add(data);
                    }
                }
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                var listCustomerHistory = await dbContext.tbl_CustomerHistory.Where(x => x.Enable == true
                 && x.SaleId == userLogin.UserInformationId
                 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year)
                 .ToListAsync();
                if (listCustomerStatus.Count > 0)
                {
                    foreach (var item in listCustomerStatus)
                    {
                        var data = new StatisticalModel();
                        data.Type = item.Name;
                        double countData = listCustomerHistory.Count(x => x.CustomerStatusId == item.Id);
                        data.Value = 0;
                        if (listCustomerHistory.Count > 0)
                        {
                            data.Value = Math.Round(countData / listCustomerHistory.Count * 100, 2);
                        }
                        result.Add(data);
                    }
                }
            }
            #endregion
            return result;
        }
        #endregion      

        #endregion

        #region báo cáo thống kê học viên

        #region thống kê số liệu tổng quan
        public async Task<List<StatisticalModel>> StudentOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var listSchedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true).ToListAsync();
            var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4).ToListAsync() ?? new List<tbl_UserInformation>();
            var listStudentInClass = await dbContext.tbl_StudentInClass.Where(x => x.Enable == true).ToListAsync();
            var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true).ToListAsync();
            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                //số lượng học viên
                totalData = listStudent.Count(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ","))));
                data = new StatisticalModel
                {
                    Type = "Tổng số học viên",
                    Value = totalData
                };
                result.Add(data);

                //học viên sắp học xong
                totalData = (from sic in listStudentInClass
                             join s in listSchedule on sic.ClassId equals s.ClassId into scheduleGroup
                             from s in scheduleGroup.DefaultIfEmpty()
                             join u in listStudent on sic.StudentId equals u.UserInformationId into studentGroup
                             from u in studentGroup.DefaultIfEmpty()
                             join c in listClass on sic.ClassId equals c.Id into classGroup
                             from c in classGroup.DefaultIfEmpty()
                             where (s == null || s.TeacherAttendanceId == 0)
                               && (c != null && c.Enable == true)
                               && (u != null && (string.IsNullOrEmpty(branchIds)
                                         || branchIds == "0"
                                         || branchIds.Split(',').Intersect(u.BranchIds.Split(',')).Any()))
                             group sic by sic.StudentId into g
                             where g.Count() <= 5
                             select g.Key).Count();
                data = new StatisticalModel
                {
                    Type = "Học viên sắp học xong",
                    Value = totalData
                };
                result.Add(data);
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                //số lượng học viên
                totalData = listStudent.Count(x => x.SaleId == userLogin.UserInformationId);
                data = new StatisticalModel
                {
                    Type = "Tổng số học viên",
                    Value = totalData
                };
                result.Add(data);
                //học viên sắp học xong trong tháng
                totalData = (from sic in listStudentInClass
                             join s in listSchedule on sic.ClassId equals s.ClassId into scheduleGroup
                             from s in scheduleGroup.DefaultIfEmpty()
                             join u in listStudent on sic.StudentId equals u.UserInformationId into studentGroup
                             from u in studentGroup.DefaultIfEmpty()
                             join c in listClass on sic.ClassId equals c.Id into classGroup
                             from c in classGroup.DefaultIfEmpty()
                             where (s == null || s.TeacherAttendanceId == 0)
                              && (c != null && c.Enable == true)
                              && (u != null && u.SaleId == userLogin.UserInformationId)
                              && (u != null && (string.IsNullOrEmpty(branchIds)
                                        || branchIds == "0"
                                        || branchIds.Split(',').Intersect(u.BranchIds.Split(',')).Any()))
                             group sic by sic.StudentId into g
                             where g.Count() <= 5
                             select g.Key).Count();
                data = new StatisticalModel
                {
                    Type = "Học viên sắp học xong",
                    Value = totalData
                };
                result.Add(data);
            }
            #endregion
            return result;
        }

        public async Task<List<StatisticalCommentModel>> StudentCompareOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalCommentModel>();
            var data = new StatisticalCommentModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var compare = new CompareModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4).ToListAsync() ?? new List<tbl_UserInformation>();
            var listStudentInClass = await dbContext.tbl_StudentInClass.Where(x => x.Enable == true).ToListAsync();
            var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true).ToListAsync();
            var listClassReverse = await dbContext.tbl_ClassReserve.Where(x => x.Enable == true).ToListAsync();
            var listClassRegister = await dbContext.tbl_ClassRegistration.Where(x => x.Enable == true).ToListAsync();
            var listRollUp = await dbContext.tbl_RollUp.Where(x => x.Enable == true).ToListAsync();
            var listWarningHistory = await dbContext.tbl_WarningHistory.Where(x => x.Enable == true).ToListAsync();

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                listStudent = listStudent.Where(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true)).ToList();
                //học viên mới trong tháng
                totalDataInMonth = listStudent.Count(x => x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = listStudent.Count(x => x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //học viên đang học trong tháng
                //kiểm tra xem có lớp nào đang diễn ra trong tháng đó không => sau đó lấy ra danh sách nhân viên trong các lớp đó
                //nếu 1 nhân viên tham gia từ 2 lớp trở thì vẫn chỉ tính là 
                totalDataInMonth = listStudentInClass
                .Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && listClass.Any(z => z.Enable == true && z.Id == x.ClassId &&
                        z.StartDay.Value.Month == time.Month && z.StartDay.Value.Year == time.Year ||
                         z.EndDay.Value.Month == time.Month && z.EndDay.Value.Year == time.Year) == true)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                totalDataPreMonth = listStudentInClass
                .Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && listClass.Any(z => z.Enable == true && z.Id == x.ClassId &&
                        z.StartDay.Value.Month == time.LastMonth && z.StartDay.Value.Year == time.YearOfLastMonth ||
                         z.EndDay.Value.Month == time.LastMonth && z.EndDay.Value.Year == time.YearOfLastMonth) == true)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên đang học trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                totalDataInMonth = listStudentInClass
                .Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && listClass.Any(z => z.Enable == true && z.Id == x.ClassId &&
                        z.StartDay.Value.Month == time.Month && z.StartDay.Value.Year == time.Year ||
                         z.EndDay.Value.Month == time.Month && z.EndDay.Value.Year == time.Year) == true)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                totalDataPreMonth = listStudentInClass
                .Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && listClass.Any(z => z.Enable == true && z.Id == x.ClassId &&
                        z.StartDay.Value.Month == time.LastMonth && z.StartDay.Value.Year == time.YearOfLastMonth ||
                         z.EndDay.Value.Month == time.LastMonth && z.EndDay.Value.Year == time.YearOfLastMonth) == true)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên đang học trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //học viên bảo lưu trong tháng => 1 học viên bảo lưu 2 lớp trong tháng thì cũng chỉ tính là 1
                totalDataInMonth = listClassReverse.Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                totalDataPreMonth = listClassReverse.Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên bảo lưu trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //học viên chờ xếp lớp trong tháng => 1 học viên có 2 đơn đang chờ xếp lớp thì cũng chỉ tính là 1 học viên
                totalDataInMonth = listClassRegister.Where(x => x.Enable == true && x.Status == 1
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                totalDataPreMonth = listClassRegister.Where(x => x.Enable == true && x.Status == 1
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên chờ xếp lớp trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lượng học viên sắp học xong

                //số lượng học viên bị cảnh cáo
                totalDataInMonth = listWarningHistory.Where(x => x.Enable == true && x.Type == 1
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year)
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count();
                totalDataPreMonth = listWarningHistory.Where(x => x.Enable == true && x.Type == 1
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth)
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count();
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên bị cảnh báo trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lượng học viên được gỡ cảnh cáo
                totalDataInMonth = listWarningHistory.Where(x => x.Enable == true && x.Type == 2
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year)
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count();
                totalDataPreMonth = listWarningHistory.Where(x => x.Enable == true && x.Type == 2
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth)
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count();
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên được gỡ cảnh báo trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                listStudent = listStudent.Where(x => x.SaleId == userLogin.UserInformationId).ToList();
                //học viên mới trong tháng
                totalDataInMonth = listStudent.Count(x => x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year && x.LearningStatus > 4);
                totalDataPreMonth = listStudent.Count(x => x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth && x.LearningStatus > 4);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //học viên đang học trong tháng
                //kiểm tra xem có lớp nào đang diễn ra trong tháng đó không => sau đó lấy ra danh sách nhân viên trong các lớp đó
                //nếu 1 nhân viên tham gia từ 2 lớp trở thì vẫn chỉ tính là 
                totalDataInMonth = listStudentInClass
                .Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && listClass.Any(z => z.Enable == true && z.Id == x.ClassId &&
                        z.StartDay.Value.Month == time.Month && z.StartDay.Value.Year == time.Year ||
                         z.EndDay.Value.Month == time.Month && z.EndDay.Value.Year == time.Year) == true)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                totalDataPreMonth = listStudentInClass
                .Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && listClass.Any(z => z.Enable == true && z.Id == x.ClassId &&
                        z.StartDay.Value.Month == time.LastMonth && z.StartDay.Value.Year == time.YearOfLastMonth ||
                         z.EndDay.Value.Month == time.LastMonth && z.EndDay.Value.Year == time.YearOfLastMonth) == true)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên đang học trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //học viên bảo lưu trong tháng => 1 học viên bảo lưu 2 lớp trong tháng thì cũng chỉ tính là 1
                totalDataInMonth = listClassReverse.Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                totalDataPreMonth = listClassReverse.Where(x => x.Enable == true
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên bảo lưu trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //học viên chờ xếp lớp trong tháng => 1 học viên có 2 đơn đang chờ xếp lớp thì cũng chỉ tính là 1 học viên
                totalDataInMonth = listClassRegister.Where(x => x.Enable == true && x.Status == 1
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();

                totalDataPreMonth = listClassRegister.Where(x => x.Enable == true && x.Status == 1
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth)
                .GroupBy(x => x.StudentId)
                .Select(group => group.FirstOrDefault())
                .Count();
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên chờ xếp lớp trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lượt vắng của học viên ( chỉ tính vắng không phép và vắng có phép )
                totalDataInMonth = listRollUp.Count(x => x.Enable == true && (x.Status == 2 || x.Status == 3)
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = listRollUp.Count(x => x.Enable == true && (x.Status == 2 || x.Status == 3)
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số lượt vắng của học viên trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lưọng học viên bị cảnh cáo
                totalDataInMonth = listWarningHistory.Count(x => x.Enable == true && x.Type == 1
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = listWarningHistory.Count(x => x.Enable == true && x.Type == 1
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên bị cảnh báo trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lưọng học viên được gỡ cảnh cáo
                totalDataInMonth = listWarningHistory.Count(x => x.Enable == true && x.Type == 2
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = listWarningHistory.Count(x => x.Enable == true && x.Type == 2
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Học viên được gỡ cảnh báo trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion
            return result;
        }
        #endregion

        #region thống kê số lượng học viên theo từng nguồn khách hàng
        public async Task<List<StatisticalDetailModel>> StudentBySource(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalDetailModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            //danh sách nguồn khách hàng
            var listSource = await dbContext.tbl_Source.Where(x => x.Enable == true).ToListAsync();
            if (listSource.Count <= 0) return result;
            //danh sách học viên
            var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0
                && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4
                && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                foreach (var item in listSource)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double countData = listStudent.Count(x => x.BranchIds != null && x.SourceId == item.Id && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true));
                    data.Value = 0;
                    data.ValueDetail = countData;
                    if (listStudent.Count > 0)
                        data.Value = Math.Round(countData / listStudent.Count * 100, 2);
                    result.Add(data);
                }
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                foreach (var item in listSource)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double countData = listStudent.Count(x => x.SaleId == userLogin.UserInformationId && x.SourceId == item.Id);
                    data.Value = 0;
                    data.ValueDetail = countData;
                    if (listStudent.Count > 0)
                        data.Value = Math.Round(countData / listStudent.Count * 100, 2);
                    result.Add(data);
                }
            }
            #endregion

            return result;
        }
        #endregion

        #region Biểu đồ tỷ lệ nhu cầu học
        public async Task<List<StatisticalDetailModel>> StudentByLearningNeed(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var result = new List<StatisticalDetailModel>();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var listLearningNeed = await dbContext.tbl_LearningNeed.Where(x => x.Enable == true).ToListAsync();
            if (listLearningNeed.Count <= 0) return result;
            var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0
                && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4
                && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
            if (listStudent.Count <= 0) return result;

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                foreach (var item in listLearningNeed)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double countData = listStudent.Count(x => x.BranchIds != null && x.LearningNeedId == item.Id && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true));
                    data.Value = 0;
                    data.ValueDetail = countData;
                    if (listStudent.Count > 0)
                        data.Value = Math.Round(countData / listStudent.Count * 100, 2);
                    result.Add(data);
                }
            }
            #endregion

            #region sale 
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                foreach (var item in listLearningNeed)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double countData = listStudent.Count(x => x.SaleId == userLogin.UserInformationId && x.LearningNeedId == item.Id);
                    data.Value = 0;
                    data.ValueDetail = countData;
                    if (listStudent.Count > 0)
                        data.Value = Math.Round(countData / listStudent.Count * 100, 2);
                    result.Add(data);
                }
            }
            #endregion

            return result;
        }
        #endregion

        #region Biểu đồ tỷ lệ mục đích học
        public async Task<List<StatisticalModel>> StudentByLearningPurpose(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var result = new List<StatisticalModel>();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var listLearningPurpose = await dbContext.tbl_Purpose.Where(x => x.Enable == true).ToListAsync();
            if (listLearningPurpose.Count <= 0) return result;
            var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0
                && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4
                && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
            if (listStudent.Count <= 0) return result;

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                foreach (var item in listLearningPurpose)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double countData = listStudent.Count(x => x.BranchIds != null && x.PurposeId == item.Id && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true));
                    data.Value = 0;
                    data.ValueDetail = countData;
                    if (listStudent.Count > 0)
                        data.Value = Math.Round(countData / listStudent.Count * 100, 2);
                    result.Add(data);
                }
            }
            #endregion

            #region sale 
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                foreach (var item in listLearningPurpose)
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    double countData = listStudent.Count(x => x.SaleId == userLogin.UserInformationId && x.PurposeId == item.Id);
                    data.Value = 0;
                    data.ValueDetail = countData;
                    if (listStudent.Count > 0)
                        data.Value = Math.Round(countData / listStudent.Count * 100, 2);
                    result.Add(data);
                }
            }
            #endregion

            return result;
        }
        #endregion

        #region thống kê độ tuổi học viên
        public async Task<List<StatisticalModel>> StudentByAge(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.DOB != null && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4).ToListAsync();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account, sale
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.sale)
            {
                //nhỏ hơn 12 tuổi (trẻ em)
                data = new StatisticalModel();
                data.Type = "< 12";
                if (listStudent.Count > 0)
                {
                    var total = listStudent.Count(x =>
                    (userLogin.RoleId != (int)RoleEnum.sale || x.SaleId == userLogin.UserInformationId)
                    && x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true) && x.DOB.HasValue && DateTime.Now.Year - x.DOB.Value.Year < 12);
                    data.Value = total;
                }
                result.Add(data);
                //12 tuổi - 18 tuổi (thanh thiếu niên)
                data = new StatisticalModel();
                data.Type = "12 - 18";
                if (listStudent.Count > 0)
                {
                    var total = listStudent.Count(x =>
                    (userLogin.RoleId != (int)RoleEnum.sale || x.SaleId == userLogin.UserInformationId)
                    && x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true) && x.DOB.HasValue && DateTime.Now.Year - x.DOB.Value.Year >= 12 && DateTime.Now.Year - x.DOB.Value.Year <= 18);
                    data.Value = total;
                }
                result.Add(data);
                //19 tuổi - 30 tuổi (thanh niên)
                data = new StatisticalModel();
                data.Type = "19 - 30";
                if (listStudent.Count > 0)
                {
                    var total = listStudent.Count(x =>
                    (userLogin.RoleId != (int)RoleEnum.sale || x.SaleId == userLogin.UserInformationId)
                    && x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true) && x.DOB.HasValue && DateTime.Now.Year - x.DOB.Value.Year >= 19 && DateTime.Now.Year - x.DOB.Value.Year <= 30);
                    data.Value = total;
                }
                result.Add(data);
                //31 tuổi - 45 tuổi (người trưởng thành)
                data = new StatisticalModel();
                data.Type = "31 - 45";
                if (listStudent.Count > 0)
                {
                    var total = listStudent.Count(x =>
                    (userLogin.RoleId != (int)RoleEnum.sale || x.SaleId == userLogin.UserInformationId)
                    && x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true) && x.DOB.HasValue && DateTime.Now.Year - x.DOB.Value.Year >= 31 && DateTime.Now.Year - x.DOB.Value.Year <= 45);
                    data.Value = total;
                }
                result.Add(data);
                //lớn hơn 45 tuổi (các đối tượng còn lại) 
                data = new StatisticalModel();
                data.Type = "45+";
                if (listStudent.Count > 0)
                {
                    var total = listStudent.Count(x =>
                    (userLogin.RoleId != (int)RoleEnum.sale || x.SaleId == userLogin.UserInformationId)
                    && x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true) && x.DOB.HasValue && DateTime.Now.Year - x.DOB.Value.Year > 45);
                    data.Value = total;
                }
                result.Add(data);
            }
            #endregion

            //#region sale 
            //if (userLogin.RoleId == (int)RoleEnum.sale)
            //{
            //    //nhỏ hơn 12 tuổi (trẻ em)
            //    data = new StatisticalModel();
            //    data.Type = "< 12";
            //    if (listStudent.Count > 0)
            //        data.Value = listStudent.Count(x => x.SaleId == userLogin.UserInformationId && DateTime.Now.Year - x.DOB.Value.Year < 12);
            //    result.Add(data);
            //    //12 tuổi - 18 tuổi (thanh thiếu niên)
            //    data = new StatisticalModel();
            //    data.Type = "12 - 18";
            //    if (listStudent.Count > 0)
            //        data.Value = listStudent.Count(x => x.SaleId == userLogin.UserInformationId && DateTime.Now.Year - x.DOB.Value.Year >= 12 || DateTime.Now.Year - x.DOB.Value.Year <= 18);
            //    result.Add(data);
            //    //19 tuổi - 30 tuổi (thanh niên)
            //    data = new StatisticalModel();
            //    data.Type = "19 - 30";
            //    if (listStudent.Count > 0)
            //        data.Value = listStudent.Count(x => x.SaleId == userLogin.UserInformationId && DateTime.Now.Year - x.DOB.Value.Year >= 19 || DateTime.Now.Year - x.DOB.Value.Year <= 30);
            //    result.Add(data);
            //    //31 tuổi - 45 tuổi (người trưởng thành)
            //    data = new StatisticalModel();
            //    data.Type = "31 - 45";
            //    if (listStudent.Count > 0)
            //        data.Value = listStudent.Count(x => x.SaleId == userLogin.UserInformationId && DateTime.Now.Year - x.DOB.Value.Year >= 31 || DateTime.Now.Year - x.DOB.Value.Year <= 45);
            //    result.Add(data);
            //    //lớn hơn 45 tuổi (các đối tượng còn lại) 
            //    data = new StatisticalModel();
            //    data.Type = "45+";
            //    if (listStudent.Count > 0)
            //        data.Value = listStudent.Count(x => x.SaleId == userLogin.UserInformationId && DateTime.Now.Year - x.DOB.Value.Year > 45);
            //    result.Add(data);
            //}
            //#endregion

            return result;
        }
        #endregion

        #region thống kê số học viên mới trong 12 tháng
        public async Task<List<Statistical12MonthModel>> NewStudent12Month(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<Statistical12MonthModel>();
            var data = new Statistical12MonthModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var listStudentInYear = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4 && x.CreatedOn.Value.Year == time.Year).ToListAsync();
            var listStudentPreYear = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4 && x.CreatedOn.Value.Year == time.LastYear).ToListAsync();
            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                for (int i = 1; i <= 12; i++)
                {
                    data = new Statistical12MonthModel();
                    data.Month = "Tháng " + i;
                    data.Type = "Học viên mới năm nay";
                    if (listStudentInYear.Count > 0)
                        data.Value = listStudentInYear.Count(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true) && x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
                for (int i = 1; i <= 12; i++)
                {
                    data = new Statistical12MonthModel();
                    data.Month = "Tháng " + i;
                    data.Type = "Học viên mới năm ngoái";
                    if (listStudentPreYear.Count > 0)
                        data.Value = listStudentPreYear.Count(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true) && x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion

            #region sale
            if (userLogin.RoleId == (int)RoleEnum.sale)
            {
                for (int i = 1; i <= 12; i++)
                {
                    data = new Statistical12MonthModel();
                    data.Month = "Tháng " + i;
                    data.Type = "Học viên mới năm nay";
                    if (listStudentInYear.Count > 0)
                        data.Value = listStudentInYear.Count(x => x.SaleId == userLogin.UserInformationId && x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
                for (int i = 1; i <= 12; i++)
                {
                    data = new Statistical12MonthModel();
                    data.Month = "Tháng " + i;
                    data.Type = "Học viên mới năm ngoái";
                    if (listStudentPreYear.Count > 0)
                        data.Value = listStudentPreYear.Count(x => x.SaleId == userLogin.UserInformationId && x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion   
            return result;
        }
        #endregion

        #endregion

        #region báo cáo thống kê ngân hàng đề thi

        #region thống kê số liệu tổng quan
        public async Task<List<StatisticalModel>> ExamSetOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var compare = new CompareModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager)
            {
                //số lượng bộ đề thi
                totalData = await dbContext.tbl_Product.CountAsync(x => x.Enable == true && x.Type == 2);
                data = new StatisticalModel
                {
                    Type = "Tổng số bộ đề",
                    Value = totalData
                };
                result.Add(data);
            }
            #endregion           
            return result;
        }

        public async Task<List<StatisticalCommentModel>> ExamSetCompareOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalCommentModel>();
            var data = new StatisticalCommentModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var compare = new CompareModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager)
            {
                //số kì thi
                totalDataInMonth = await dbContext.tbl_Transcript.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + y.BranchId + ",")))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Transcript.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + y.BranchId + ",")))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số kì thi",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lượng học viên mua bộ đề
                var StudentBuyInMonth = await dbContext.tbl_Product.Where(x => x.Enable == true && x.Type == 2 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (StudentBuyInMonth.Count > 0)
                    totalDataInMonth = StudentBuyInMonth.Sum(x => x.TotalStudent);
                var StudentBuyPreMonth = await dbContext.tbl_Product.Where(x => x.Enable == true && x.Type == 2 && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth).ToListAsync();
                if (StudentBuyPreMonth.Count > 0)
                    totalDataPreMonth = StudentBuyPreMonth.Sum(x => x.TotalStudent);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số học viên mua bộ đề",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số bài tập đã giao
                totalDataInMonth = await dbContext.tbl_Homework.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + y.BranchId + ",")))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Homework.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + y.BranchId + ",")))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số bài tập đã giao",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion           
            return result;
        }
        #endregion

        #region báo cáo học viên mua bộ đề
        public class StudentBuyExamSetModel : StatisticalModel
        {
            public int Id { get; set; }
        }
        /// <summary>
        /// thông tin chung
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public static async Task<AppDomainResult> ReportStudentBuyExamSet(StatisticalSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StatisticalSearch();
                var result = new List<StudentBuyExamSetModel>();
                var listExamSet = await db.tbl_Product.Where(x => x.Enable == true && x.Type == 2).ToListAsync();
                if (listExamSet.Count <= 0) return new AppDomainResult { TotalRow = 0, Data = null };
                double totalStudentBuy = listExamSet.Sum(x => x.TotalStudent);
                foreach (var item in listExamSet)
                {
                    var data = new StudentBuyExamSetModel();
                    data.Id = item.Id;
                    data.Type = item.Name;
                    //số lượng mua bộ đề
                    data.Value = item.TotalStudent;
                    //tỷ lệ mua bộ đề => khi nào cần lấy theo tỷ lệ thì dùng cái này
                    //data.Value = Math.Round(((item.TotalStudent ?? 0) / totalStudentBuy * 100), 2);
                    result.Add(data);
                }
                var totalRow = result.Count;
                // Phân trang
                int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
                result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        /// <summary>
        /// thông tin chi tiết
        /// </summary>
        public class DetailStudentBuyExamSetModel
        {
            /// <summary>
            /// Id
            /// </summary>
            public int UserId { get; set; }
            /// <summary>
            /// họ tên
            /// </summary>
            public string FullName { get; set; }
            /// <summary>
            /// mã nhân viên
            /// </summary>
            public string UserCode { get; set; }
            /// <summary>
            /// ảnh đại diện
            /// </summary>
            public string Avatar { get; set; }
            /// <summary>
            /// ngày mua
            /// </summary>
            public DateTime? PurchaseDate { get; set; }
        }
        public async Task<AppDomainResult> ReportDetailStudentBuyExamSet(ReportDetailSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new ReportDetailSearch();
            var result = new List<DetailStudentBuyExamSetModel>();
            var listBillDetail = await dbContext.tbl_BillDetail.Where(x => x.Enable == true && x.ProductId == baseSearch.Id).ToListAsync();
            if (listBillDetail.Count <= 0) return new AppDomainResult { TotalRow = 0, Data = null };
            foreach (var item in listBillDetail)
            {
                var data = new DetailStudentBuyExamSetModel();
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                if (student == null)
                    continue;
                data.UserId = student.UserInformationId;
                data.UserCode = student.UserCode;
                data.FullName = student.FullName;
                data.Avatar = student.Avatar;
                data.PurchaseDate = item.CreatedOn;
                result.Add(data);
            }
            var totalRow = result.Count;
            // Phân trang
            int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
            result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        #endregion

        #region báo cáo kết quả bài tập về nhà
        public class ReportHomeworkResultModel
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            /// <summary>
            /// trạng thái lớp
            /// 1 - sắp diễn ra
            /// 2 - đang diễn ra
            /// 3 - đã kết thúc
            /// </summary>
            public int? Status { get; set; }
            public string StatusName { get; set; }
            /// <summary>
            /// số bài tập đã giao
            /// </summary>
            public int QuantityDelivered { get; set; } = 0;
            /// <summary>
            /// tỷ lệ hiểu bài
            /// </summary>
            public double ComprehensionRate { get; set; } = 0;
        }

        public async Task<AppDomainResult> ReportHomeworkResult(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var result = new List<ReportHomeworkResultModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true && x.Status != 1 && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))).ToListAsync();
            //type = 1 là bài tập về nhà
            var listHomework = await dbContext.tbl_Homework.Where(x => x.Enable == true && (x.Type == HomeworkType.Homework || x.Type == 0 || x.Type == null)).ToListAsync();
            var listHomeworkResult = await dbContext.tbl_IeltsExamResult.Where(x => x.Enable == true).ToListAsync();
            if (listClass.Count < 0) return new AppDomainResult { TotalRow = 0, Data = null };
            foreach (var item in listClass)
            {
                var data = new ReportHomeworkResultModel();
                data.ClassId = item.Id;
                data.ClassName = item.Name;
                data.Status = item.Status;
                data.StatusName = item.StatusName;
                var listClassHomework = new List<int>();
                if (listHomework.Count > 0)
                    listClassHomework = listHomework.Where(x => x.ClassId == item.Id).Select(x => x.Id).ToList();
                data.QuantityDelivered = listClassHomework.Count();
                double totalPercentage = 0;
                if (listHomeworkResult.Count > 0)
                {
                    var listClassHomeworkResult = listHomeworkResult.Where(x => listClassHomework.Contains(x.ValueId)).ToList();
                    if (listClassHomeworkResult.Count > 0)
                    {
                        // Tính tỷ lệ điểm và tính tổng
                        foreach (var jtem in listClassHomeworkResult)
                        {
                            double percentage = 0;
                            if (jtem.Point != 0)
                            {
                                percentage = Math.Round(jtem.MyPoint / jtem.Point * 100, 2);
                            }
                            totalPercentage += percentage;
                        }

                        // Tính trung bình
                        data.ComprehensionRate = Math.Round(totalPercentage / listClassHomeworkResult.Count, 2);
                    }
                }
                result.Add(data);
            }
            var totalRow = result.Count;
            // Phân trang
            int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
            result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        #endregion

        #region báo cáo kết quả bài thi
        public async Task<AppDomainResult> ReportTestResult(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var result = new List<ReportHomeworkResultModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true && x.Status != 1 && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))).ToListAsync();
            //type = 2 là bài thi
            var listTest = await dbContext.tbl_Homework.Where(x => x.Enable == true && x.Type == HomeworkType.Exam).ToListAsync();
            var listTestResult = await dbContext.tbl_IeltsExamResult.Where(x => x.Enable == true).ToListAsync();
            if (listClass.Count < 0) return new AppDomainResult { TotalRow = 0, Data = null };
            foreach (var item in listClass)
            {
                var data = new ReportHomeworkResultModel();
                data.ClassId = item.Id;
                data.ClassName = item.Name;
                data.Status = item.Status;
                data.StatusName = item.StatusName;
                var listClassTest = new List<int>();
                if (listTest.Count > 0)
                    listClassTest = listTest.Where(x => x.ClassId == item.Id).Select(x => x.Id).ToList();
                data.QuantityDelivered = listClassTest.Count();
                double totalPercentage = 0;
                if (listTestResult.Count > 0)
                {
                    var listClassTestResult = listTestResult.Where(x => listClassTest.Contains(x.ValueId)).ToList();
                    if (listClassTestResult.Count > 0)
                    {
                        // Tính tỷ lệ điểm và tính tổng
                        foreach (var jtem in listClassTestResult)
                        {
                            double percentage = 0;
                            if (jtem.Point != 0)
                            {
                                percentage = Math.Round(jtem.MyPoint / jtem.Point * 100, 2);
                            }
                            totalPercentage += percentage;
                        }

                        // Tính trung bình
                        data.ComprehensionRate = Math.Round(totalPercentage / listClassTestResult.Count, 2);
                    }
                }
                result.Add(data);
            }
            var totalRow = result.Count;
            // Phân trang
            int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
            result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        #endregion

        #endregion

        #region báo cáo thống kê lớp học

        #region thống kê số liệu tổng quan
        public async Task<List<StatisticalDescriptionModel>> ClassOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalDescriptionModel>();
            var data = new StatisticalDescriptionModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            //var branchIdArray = branchIds.Split(',');

            //cái này để tìm các lớp trong khoảng 15 ngày nữa kết thúc
            DateTime currentDate = DateTime.Now;
            DateTime futureDate = currentDate.AddDays(15);

            var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true).ToListAsync();
            var listStudentInClass = await dbContext.tbl_StudentInClass.Where(x => x.Enable == true).ToListAsync();
            var listSchedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true).ToListAsync();
            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                //số lượng lớp
                totalData = listClass.Count(x => x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")));
                data = new StatisticalDescriptionModel
                {
                    Type = "Tổng số lớp học",
                    Value = totalData
                };
                result.Add(data);

                //số lượng lớp sắp đóng
                totalData = listClass.Count(x => x.BranchId != null && x.EndDay >= currentDate && x.EndDay <= futureDate
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")));
                data = new StatisticalDescriptionModel
                {
                    Type = "Lớp học sắp đóng",
                    Value = totalData,
                    Description = "Các lớp sẽ kết thúc trong 15 ngày nữa"
                };
                result.Add(data);

                //số lượng lớp đạt tiêu chuẩn
                totalData = listClass.Count(x => x.BranchId != null && (listStudentInClass.Count(y => y.ClassId == x.Id) >= (x.MaxQuantity * 0.8))
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")));
                data = new StatisticalDescriptionModel
                {
                    Type = "Lớp học đạt tiêu chuẩn",
                    Value = totalData,
                    Description = "Các lớp có số học viên trên 80% số lượng tối đa"
                };
                result.Add(data);
            }
            #endregion

            #region giáo viên - lấy các lớp mà giáo viên có tham gia giảng dạy
            if (userLogin.RoleId == (int)RoleEnum.teacher)
            {
                //số lượng lớp
                totalData = listClass.Count(x => listSchedule.Any(y => y.ClassId == x.Id && y.TeacherId == userLogin.UserInformationId) == true);
                data = new StatisticalDescriptionModel
                {
                    Type = "Tổng số lớp học",
                    Value = totalData
                };
                result.Add(data);

                //số lượng lớp sắp đóng
                totalData = listClass.Count(x => x.EndDay >= currentDate && x.EndDay <= futureDate
                    && listSchedule.Any(y => y.ClassId == x.Id && y.TeacherId == userLogin.UserInformationId) == true);
                data = new StatisticalDescriptionModel
                {
                    Type = "Lớp học sắp đóng",
                    Value = totalData,
                    Description = "Các lớp sẽ kết thúc trong 15 ngày nữa"
                };
                result.Add(data);

                //bài tập về nhà cần chấm
                totalDataInMonth = await dbContext.tbl_IeltsExamResult.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Homework.Any(y => y.Id == x.ValueId && y.TeacherId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_IeltsExamResult.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Homework.Any(y => y.Id == x.ValueId && y.TeacherId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                data = new StatisticalDescriptionModel
                {
                    Type = "Bài tập về nhà cần chấm trong tháng",
                    Value = totalDataInMonth,
                };
                result.Add(data);

                //bài tập về nhà đã chấm
                totalDataInMonth = await dbContext.tbl_IeltsExamResult.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Homework.Any(y => y.Id == x.ValueId && y.TeacherId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year && x.Status == 2);
                totalDataPreMonth = await dbContext.tbl_IeltsExamResult.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Homework.Any(y => y.Id == x.ValueId && y.TeacherId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth && x.Status == 2);
                data = new StatisticalDescriptionModel
                {
                    Type = "Bài tập về nhà đã chấm trong tháng",
                    Value = totalDataInMonth,
                };
                result.Add(data);
            }
            #endregion

            #region học viên - lấy các lớp mà học viên tham gia học
            if (userLogin.RoleId == (int)RoleEnum.student)
            {
                //số lượng lớp của học viên
                totalData = listClass.Count(x => listStudentInClass.Any(y => y.ClassId == x.Id && y.StudentId == userLogin.UserInformationId) == true);
                data = new StatisticalDescriptionModel
                {
                    Type = "Tổng số lớp học",
                    Value = totalData
                };
                result.Add(data);

                //số lượng lớp sắp đóng
                totalData = listClass.Count(x => x.EndDay >= currentDate && x.EndDay <= futureDate
                    && listStudentInClass.Any(y => y.ClassId == x.Id && y.StudentId == userLogin.UserInformationId) == true);
                data = new StatisticalDescriptionModel
                {
                    Type = "Lớp học sắp đóng",
                    Value = totalData,
                    Description = "Các lớp sẽ kết thúc trong 15 ngày nữa"
                };
                result.Add(data);
            }
            #endregion

            #region phụ huynh - lấy các lớp mà con của họ tham gia
            if (userLogin.RoleId == (int)RoleEnum.parents)
            {
                //số lượng lớp của học viên
                totalData = await dbContext.tbl_Class.CountAsync(x => x.Enable == true
                    && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id
                        && dbContext.tbl_UserInformation.Any(z => z.Enable == true && z.UserInformationId == y.StudentId && z.ParentId == userLogin.UserInformationId) == true) == true);
                data = new StatisticalDescriptionModel
                {
                    Type = "Tổng số lớp học",
                    Value = totalData
                };
                result.Add(data);

                //số lượng lớp sắp đóng
                totalData = await dbContext.tbl_Class.CountAsync(x => x.Enable == true && x.EndDay <= futureDate
                    && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id
                        && dbContext.tbl_UserInformation.Any(z => z.Enable == true && z.UserInformationId == y.StudentId && z.ParentId == userLogin.UserInformationId) == true) == true);

                data = new StatisticalDescriptionModel
                {
                    Type = "Lớp học sắp đóng",
                    Value = totalData,
                    Description = "Các lớp sẽ kết thúc trong 15 ngày nữa"
                };
                result.Add(data);
            }
            #endregion
            return result;
        }

        public async Task<List<StatisticalCommentModel>> ClassCompareOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalCommentModel>();
            var data = new StatisticalCommentModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var compare = new CompareModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4).ToListAsync() ?? new List<tbl_UserInformation>();
                listStudent = listStudent.Where(x => (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")))).ToList();
                var listRollUp = await dbContext.tbl_RollUp.Where(x => x.Enable == true).ToListAsync();

                //lớp mới trong tháng
                totalDataInMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lớp mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lớp học kết thúc trong tháng
                totalDataInMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true && x.Status == 3 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true && x.Status == 3 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số lượng lớp học kết thúc",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //tỷ lệ điểm danh trong tháng ( loại trừ status = 6 vì đây là loại nghỉ lễ )
                totalDataInMonth = 0;
                var RollUpInMonth = await dbContext.tbl_RollUp.Where(x => x.Enable == true && x.Status != 6
                    && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + y.BranchId + ",")))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (RollUpInMonth.Count > 0)
                    totalDataInMonth = Math.Round(RollUpInMonth.Count(x => x.Status == 1) / (double)RollUpInMonth.Count * 100, 2);

                totalDataPreMonth = 0;
                var RollUpPreMonth = await dbContext.tbl_RollUp.Where(x => x.Enable == true && x.Status != 6
                    && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + y.BranchId + ",")))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth).ToListAsync();
                if (RollUpPreMonth.Count > 0)
                    totalDataPreMonth = Math.Round(RollUpPreMonth.Count(x => x.Status == 1) / (double)RollUpPreMonth.Count * 100, 2);

                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Tỷ lệ học viên điểm danh trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lượt vắng của học viên ( chỉ tính vắng không phép và vắng có phép )
                totalDataInMonth = listRollUp.Count(x => x.Enable == true && (x.Status == 2 || x.Status == 3)
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = listRollUp.Count(x => x.Enable == true && (x.Status == 2 || x.Status == 3)
                    && listStudent.Any(y => y.UserInformationId == x.StudentId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số lượt vắng của học viên trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion

            #region giáo viên - lấy các lớp mà giáo viên có tham gia giảng dạy
            if (userLogin.RoleId == (int)RoleEnum.teacher)
            {
                //lớp mới trong tháng
                totalDataInMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true
                && dbContext.tbl_Schedule.Any(y => y.ClassId == x.Id && y.TeacherId == userLogin.UserInformationId) == true
                && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true
                && dbContext.tbl_Schedule.Any(y => y.ClassId == x.Id && y.TeacherId == userLogin.UserInformationId) == true
                && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lớp mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lớp học kết thúc trong tháng
                totalDataInMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true && x.Status == 3
                && dbContext.tbl_Schedule.Any(y => y.ClassId == x.Id && y.TeacherId == userLogin.UserInformationId) == true
                && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true && x.Status == 3
                && dbContext.tbl_Schedule.Any(y => y.ClassId == x.Id && y.TeacherId == userLogin.UserInformationId) == true
                && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lớp học kết thúc",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lịch dạy trong tháng
                totalDataInMonth = await dbContext.tbl_Schedule.CountAsync(x => x.Enable == true && x.TeacherId == userLogin.UserInformationId
                && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Schedule.CountAsync(x => x.Enable == true && x.TeacherId == userLogin.UserInformationId
                && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lịch dạy trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lịch đã dạy trong tháng
                totalDataInMonth = await dbContext.tbl_Schedule.CountAsync(x => x.Enable == true && x.TeacherId == userLogin.UserInformationId && x.Status == 2
                && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Schedule.CountAsync(x => x.Enable == true && x.TeacherId == userLogin.UserInformationId && x.Status == 2
                && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lịch đã dạy trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //tỷ lệ điểm danh của giảng viên
                var listScheduleInMonth = await dbContext.tbl_Schedule.Where(x => x.Enable == true && x.TeacherId == userLogin.UserInformationId && x.StartTime.Value.Month == time.Month && x.StartTime.Value.Year == time.Year).ToListAsync();
                totalDataInMonth = 0;
                if (listScheduleInMonth.Count > 0)
                {
                    double rollUpTeacherInMonth = listScheduleInMonth.Count(x => x.TeacherAttendanceId == userLogin.UserInformationId);
                    totalDataInMonth = Math.Round(rollUpTeacherInMonth / listScheduleInMonth.Count * 100, 2);
                }
                var listSchedulePreMonth = await dbContext.tbl_Schedule.Where(x => x.Enable == true && x.TeacherId == userLogin.UserInformationId && x.StartTime.Value.Month == time.LastMonth && x.StartTime.Value.Year == time.YearOfLastMonth).ToListAsync();
                totalDataPreMonth = 0;
                if (listSchedulePreMonth.Count > 0)
                {
                    double rollUpTeacherPreMonth = listSchedulePreMonth.Count(x => x.TeacherAttendanceId == userLogin.UserInformationId);
                    totalDataPreMonth = Math.Round(rollUpTeacherPreMonth / listSchedulePreMonth.Count * 100, 2);
                }
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Tỷ lệ điểm danh giáo viên trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //hẹn test cần chấm
                totalDataInMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true &&
                    x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year &&
                    x.TeacherId == userLogin.UserInformationId);
                totalDataPreMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true &&
                    x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth &&
                    x.TeacherId == userLogin.UserInformationId);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Hẹn test cần chấm trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };

                //có nhập điểm => đã chấm
                totalDataInMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true &&
                    x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year &&
                    x.TeacherId == userLogin.UserInformationId &&
                    (!string.IsNullOrEmpty(x.ListeningPoint) || !string.IsNullOrEmpty(x.ReadingPoint) || !string.IsNullOrEmpty(x.WritingPoint) || !string.IsNullOrEmpty(x.SpeakingPoint)));
                totalDataPreMonth = await dbContext.tbl_TestAppointment.CountAsync(x => x.Enable == true &&
                    x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth &&
                    x.TeacherId == userLogin.UserInformationId &&
                    (!string.IsNullOrEmpty(x.ListeningPoint) || !string.IsNullOrEmpty(x.ReadingPoint) || !string.IsNullOrEmpty(x.WritingPoint) || !string.IsNullOrEmpty(x.SpeakingPoint)));
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Hẹn test đã chấm trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion

            #region học viên - lấy các lớp mà học viên tham gia học
            if (userLogin.RoleId == (int)RoleEnum.student)
            {
                //lớp mới trong tháng
                totalDataInMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id && y.StudentId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id && y.StudentId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lớp mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lớp học kết thúc trong tháng
                totalDataInMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id && y.StudentId == userLogin.UserInformationId) == true
                    && x.Status == 3 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id && y.StudentId == userLogin.UserInformationId) == true
                    && x.Status == 3 && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số lượng lớp học kết thúc",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số buổi học trong tháng
                totalDataInMonth = await dbContext.tbl_Schedule.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.Enable == true
                        && dbContext.tbl_StudentInClass.Any(z => z.Enable == true && z.ClassId == y.Id && z.StudentId == userLogin.UserInformationId) == true) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Schedule.CountAsync(x => x.Enable == true
                   && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.Enable == true
                       && dbContext.tbl_StudentInClass.Any(z => z.Enable == true && z.ClassId == y.Id && z.StudentId == userLogin.UserInformationId) == true) == true
                   && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Buổi học trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lượt điểm danh của học viên ( k tính vắng và nghỉ lễ )
                totalDataInMonth = await dbContext.tbl_RollUp.CountAsync(x => x.Enable == true && x.Status != 2 && x.Status != 3 && x.Status != 6 && x.StudentId == userLogin.UserInformationId && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_RollUp.CountAsync(x => x.Enable == true && x.Status != 2 && x.Status != 3 && x.Status != 6 && x.StudentId == userLogin.UserInformationId && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lượt điểm danh trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lượt vắng của học viên ( tính các buổi vắng có phép và không phép )
                totalDataInMonth = await dbContext.tbl_RollUp.CountAsync(x => x.Enable == true && (x.Status == 2 || x.Status == 3) && x.StudentId == userLogin.UserInformationId && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_RollUp.CountAsync(x => x.Enable == true && (x.Status == 2 || x.Status == 3) && x.StudentId == userLogin.UserInformationId && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lượt vắng trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lần bị cảnh báo
                totalDataInMonth = await dbContext.tbl_WarningHistory.CountAsync(x => x.Enable == true && x.StudentId == userLogin.UserInformationId && x.Type == 1 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_WarningHistory.CountAsync(x => x.Enable == true && x.StudentId == userLogin.UserInformationId && x.Type == 1 && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số lần bị cảnh báo trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lần được gỡ cảnh báo
                totalDataInMonth = await dbContext.tbl_WarningHistory.CountAsync(x => x.Enable == true && x.StudentId == userLogin.UserInformationId && x.Type == 2 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_WarningHistory.CountAsync(x => x.Enable == true && x.StudentId == userLogin.UserInformationId && x.Type == 2 && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số lần được gỡ cảnh báo trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion

            #region phụ huynh - lấy các lớp mà con của họ tham gia
            if (userLogin.RoleId == (int)RoleEnum.parents)
            {
                //lớp mới trong tháng
                totalDataInMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true
                    && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id
                        && dbContext.tbl_UserInformation.Any(z => z.Enable == true && z.UserInformationId == y.StudentId && z.ParentId == userLogin.UserInformationId) == true) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true
                    && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id
                        && dbContext.tbl_UserInformation.Any(z => z.Enable == true && z.UserInformationId == y.StudentId && z.ParentId == userLogin.UserInformationId) == true) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lớp mới trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lớp học kết thúc trong tháng
                totalDataInMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true
                    && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id
                        && dbContext.tbl_UserInformation.Any(z => z.Enable == true && z.UserInformationId == y.StudentId && z.ParentId == userLogin.UserInformationId) == true) == true
                    && x.Status == 3 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Class.CountAsync(x => x.Enable == true
                    && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id
                        && dbContext.tbl_UserInformation.Any(z => z.Enable == true && z.UserInformationId == y.StudentId && z.ParentId == userLogin.UserInformationId) == true) == true
                    && x.Status == 3 && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số lượng lớp học kết thúc",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số buổi học trong tháng
                totalDataInMonth = await dbContext.tbl_Schedule.CountAsync(x => x.Enable == true
                    && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.Enable == true
                        && dbContext.tbl_StudentInClass.Any(z => z.Enable == true && z.ClassId == y.Id
                            && dbContext.tbl_UserInformation.Any(u => u.Enable == true && u.UserInformationId == z.StudentId && u.ParentId == userLogin.UserInformationId) == true) == true) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_Schedule.CountAsync(x => x.Enable == true
                   && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.Enable == true
                        && dbContext.tbl_StudentInClass.Any(z => z.Enable == true && z.ClassId == y.Id
                            && dbContext.tbl_UserInformation.Any(u => u.Enable == true && u.UserInformationId == z.StudentId && u.ParentId == userLogin.UserInformationId) == true) == true) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Buổi học trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lượt điểm danh của học viên ( k tính vắng và nghỉ lễ )
                totalDataInMonth = await dbContext.tbl_RollUp.CountAsync(x => x.Enable == true && x.Status != 2 && x.Status != 3 && x.Status != 6
                    && dbContext.tbl_UserInformation.Any(y => y.Enable == true && y.UserInformationId == x.StudentId && y.ParentId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_RollUp.CountAsync(x => x.Enable == true && x.Status != 2 && x.Status != 3 && x.Status != 6
                    && dbContext.tbl_UserInformation.Any(y => y.Enable == true && y.UserInformationId == x.StudentId && y.ParentId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lượt điểm danh trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lượt vắng của học viên ( tính các buổi vắng có phép và không phép )
                totalDataInMonth = await dbContext.tbl_RollUp.CountAsync(x => x.Enable == true && (x.Status == 2 || x.Status == 3)
                    && dbContext.tbl_UserInformation.Any(y => y.Enable == true && y.UserInformationId == x.StudentId && y.ParentId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_RollUp.CountAsync(x => x.Enable == true && (x.Status == 2 || x.Status == 3)
                    && dbContext.tbl_UserInformation.Any(y => y.Enable == true && y.UserInformationId == x.StudentId && y.ParentId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lượt vắng trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lần bị cảnh báo
                totalDataInMonth = await dbContext.tbl_WarningHistory.CountAsync(x => x.Enable == true
                    && dbContext.tbl_UserInformation.Any(y => y.Enable == true && y.UserInformationId == x.StudentId && y.ParentId == userLogin.UserInformationId) == true
                    && x.Type == 1 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_WarningHistory.CountAsync(x => x.Enable == true
                    && dbContext.tbl_UserInformation.Any(y => y.Enable == true && y.UserInformationId == x.StudentId && y.ParentId == userLogin.UserInformationId) == true
                    && x.Type == 1 && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số lần bị cảnh báo trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //số lần được gỡ cảnh báo
                totalDataInMonth = await dbContext.tbl_WarningHistory.CountAsync(x => x.Enable == true
                    && dbContext.tbl_UserInformation.Any(y => y.Enable == true && y.UserInformationId == x.StudentId && y.ParentId == userLogin.UserInformationId) == true
                    && x.Type == 2 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year);
                totalDataPreMonth = await dbContext.tbl_WarningHistory.CountAsync(x => x.Enable == true
                    && dbContext.tbl_UserInformation.Any(y => y.Enable == true && y.UserInformationId == x.StudentId && y.ParentId == userLogin.UserInformationId) == true
                    && x.Type == 2 && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Số lần được gỡ cảnh báo trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion
            return result;
        }
        #endregion

        #region thống kê tỷ lệ lớp sắp diễn ra, đang diễn ra, và đã kết thúc
        public async Task<List<StatisticalModel>> ClassProgress(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            double countData = 0;
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listClass.Count <= 0) return result;
                //lớp sắp diễn ra
                data = new StatisticalModel();
                data.Type = "Sắp diễn ra";
                countData = listClass.Count(x => x.Status == 1);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
                //lớp đang diễn ra
                data = new StatisticalModel();
                data.Type = "Đang diễn ra";
                countData = listClass.Count(x => x.Status == 2);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
                //lớp đã diễn ra
                data = new StatisticalModel();
                data.Type = "Đã diễn ra";
                countData = listClass.Count(x => x.Status == 3);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
            }
            #endregion

            #region giáo viên - lấy các lớp mà giáo viên có tham gia giảng dạy
            if (userLogin.RoleId == (int)RoleEnum.teacher)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true
                    && dbContext.tbl_Schedule.Any(y => y.ClassId == x.Id && y.TeacherId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listClass.Count <= 0) return result;
                //lớp sắp diễn ra
                data = new StatisticalModel();
                data.Type = "Sắp diễn ra";
                countData = listClass.Count(x => x.Status == 1);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
                //lớp đang diễn ra
                data = new StatisticalModel();
                data.Type = "Đang diễn ra";
                countData = listClass.Count(x => x.Status == 2);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
                //lớp đã diễn ra
                data = new StatisticalModel();
                data.Type = "Đã diễn ra";
                countData = listClass.Count(x => x.Status == 3);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
            }
            #endregion

            #region học viên - lấy các lớp mà học viên tham gia học
            if (userLogin.RoleId == (int)RoleEnum.student)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true
                    && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id && y.StudentId == userLogin.UserInformationId) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listClass.Count <= 0) return result;
                //lớp sắp diễn ra
                data = new StatisticalModel();
                data.Type = "Sắp diễn ra";
                countData = listClass.Count(x => x.Status == 1);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
                //lớp đang diễn ra
                data = new StatisticalModel();
                data.Type = "Đang diễn ra";
                countData = listClass.Count(x => x.Status == 2);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
                //lớp đã diễn ra
                data = new StatisticalModel();
                data.Type = "Đã diễn ra";
                countData = listClass.Count(x => x.Status == 3);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
            }
            #endregion

            #region phụ huynh - lấy các lớp mà con họ tham gia học
            if (userLogin.RoleId == (int)RoleEnum.parents)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true
                    && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id
                        && dbContext.tbl_UserInformation.Any(z => z.Enable == true && z.UserInformationId == y.StudentId && z.ParentId == userLogin.UserInformationId) == true) == true
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listClass.Count <= 0) return result;
                //lớp sắp diễn ra
                data = new StatisticalModel();
                data.Type = "Sắp diễn ra";
                countData = listClass.Count(x => x.Status == 1);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
                //lớp đang diễn ra
                data = new StatisticalModel();
                data.Type = "Đang diễn ra";
                countData = listClass.Count(x => x.Status == 2);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
                //lớp đã diễn ra
                data = new StatisticalModel();
                data.Type = "Đã diễn ra";
                countData = listClass.Count(x => x.Status == 3);
                data.Value = Math.Round(countData / listClass.Count * 100, 2);
                result.Add(data);
            }
            #endregion
            return result;
        }
        #endregion

        #region thống kê số lớp mới trong 12 tháng
        public async Task<List<StatisticalModel>> NewClass12Month(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new StatisticalModel();
                    data.Type = "Tháng " + i;
                    if (listClass.Count > 0)
                        data.Value = listClass.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion

            #region giáo viên - lấy các lớp mà giảng viên có tham gia giảng dạy
            if (userLogin.RoleId == (int)RoleEnum.teacher)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true && x.CreatedOn.Value.Year == time.Year
                && dbContext.tbl_Schedule.Any(y => y.ClassId == x.Id && y.TeacherId == userLogin.UserInformationId) == true).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new StatisticalModel();
                    data.Type = "Tháng " + i;
                    if (listClass.Count > 0)
                        data.Value = listClass.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion

            #region học viên - lấy các lớp mà học viên tham gia học
            if (userLogin.RoleId == (int)RoleEnum.student)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true && x.CreatedOn.Value.Year == time.Year
                && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id && y.StudentId == userLogin.UserInformationId) == true).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new StatisticalModel();
                    data.Type = "Tháng " + i;
                    if (listClass.Count > 0)
                        data.Value = listClass.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion

            #region phụ huynh - lấy các lớp mà con họ tham gia học
            if (userLogin.RoleId == (int)RoleEnum.parents)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true && x.CreatedOn.Value.Year == time.Year
                && dbContext.tbl_StudentInClass.Any(y => y.Enable == true && y.ClassId == x.Id
                        && dbContext.tbl_UserInformation.Any(z => z.Enable == true && z.UserInformationId == y.StudentId && z.ParentId == userLogin.UserInformationId) == true) == true).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new StatisticalModel();
                    data.Type = "Tháng " + i;
                    if (listClass.Count > 0)
                        data.Value = listClass.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion
            return result;
        }
        #endregion

        #region báo cáo dữ liệu chung từng lớp
        public class ReportClassModel
        {
            /// <summary>
            /// Id lớp
            /// </summary>
            public int ClassId { get; set; }
            /// <summary>
            /// tên lớp
            /// </summary>
            public string ClassName { get; set; }
            /// <summary>
            /// số lượng học viên tham gia lớp
            /// </summary>
            public int TotalStudent { get; set; } = 0;
            /// <summary>
            /// số lượng học viên tối đa
            /// </summary>
            public int MaxQuantity { get; set; } = 0;
            /// <summary>
            /// số buổi học đã hoàn thành
            /// </summary>
            public int TotalCompletedSchedule { get; set; } = 0;
            /// <summary>
            /// số buổi học tối đa
            /// </summary>
            public int TotalSchedule { get; set; } = 0;
            /// <summary>
            /// tỷ lệ điểm danh
            /// </summary>
            public double AttendanceRate { get; set; } = 0;
            /// <summary>
            /// số tài liệu đã hoàn thành
            /// </summary>
            public int TotalCompletedDocument { get; set; } = 0;
            /// <summary>
            /// tỷ lệ nộp bài tập về nhà
            /// </summary>
            public double HomeworkRate { get; set; } = 0;
            /// <summary>
            /// tỷ lệ nộp bài trễ
            /// </summary>
            //public double LateSubmissionRate { get; set; } = 0;
        }
        public async Task<AppDomainResult> ReportClass(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<ReportClassModel>();
            var data = new ReportClassModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account, học vụ
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant || userLogin.RoleId == (int)RoleEnum.academic)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))).ToListAsync();
                if (listClass.Count <= 0) return new AppDomainResult { TotalRow = 0, Data = null };
                var listStudentInClass = await dbContext.tbl_StudentInClass.Where(x => x.Enable == true).ToListAsync();
                var listSchedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true).ToListAsync();
                var listRollUp = await dbContext.tbl_RollUp.Where(x => x.Enable == true).ToListAsync();
                var listCurriculum = await dbContext.tbl_CurriculumInClass.Where(x => x.Enable == true).ToListAsync();
                var listCurriculumDetail = await dbContext.tbl_CurriculumDetailInClass.Where(x => x.Enable == true).ToListAsync();
                var listHomework = await dbContext.tbl_Homework.Where(x => x.Enable == true).ToListAsync();
                var listHomeworkResult = await dbContext.tbl_IeltsExamResult.Where(x => x.Enable == true).ToListAsync();
                foreach (var item in listClass)
                {
                    data = new ReportClassModel();
                    data.ClassId = item.Id;
                    data.ClassName = item.Name;
                    //danh sách học viên đã tham gia lớp
                    if (listStudentInClass.Count > 0)
                        data.TotalStudent = listStudentInClass.Count(x => x.ClassId == item.Id);
                    data.MaxQuantity = item.MaxQuantity ?? 0;
                    if (listSchedule.Count > 0)
                    {
                        //số lịch học đã hoàn thành
                        data.TotalCompletedSchedule = listSchedule.Count(x => x.ClassId == item.Id && x.Status == 2);
                        data.TotalSchedule = listSchedule.Count(x => x.ClassId == item.Id);
                    }
                    //ví dụ lớp có 3 buổi học và 10 học viên thì tổng cộng phải có 30 phiếu điểm danh
                    double totalRollUp = data.TotalSchedule * data.TotalStudent;
                    //lấy số phiếu điểm danh với trạng thái là có mặt
                    double totalStudentRollUp = listRollUp.Count(x => x.ClassId == item.Id && x.Status == 1);
                    if (totalRollUp > 0)
                        data.AttendanceRate = Math.Round(totalStudentRollUp / totalRollUp * 100, 2);
                    //lấy số tài liệu đã hoàn thành
                    if (listCurriculumDetail.Count > 0)
                        data.TotalCompletedDocument = listCurriculumDetail.Count(x => x.IsComplete == true && listCurriculum.Any(y => y.ClassId == item.Id && x.CurriculumIdInClass == y.Id) == true);
                    //lấy danh sách bài tập về nhà của lớp đó
                    var listHomeworkOfClass = listHomework.Where(x => x.ClassId == item.Id).Select(x => x.Id).ToList();
                    double totalStudentHomework = 0;
                    //ví dụ lớp có 3 bài tập về nhà và 10 học viên thì tổng cộng phải có 30 bài nộp 
                    if (listHomeworkOfClass.Count > 0)
                        totalStudentHomework = data.TotalStudent * listHomeworkOfClass.Count;
                    //lấy số bài tập học viên đã nộp ( nếu 1 học viên nộp hơn 2 lần thì cũng chỉ tính là 1 )
                    double totalHomeworkResult = listHomeworkResult.Where(x => listHomeworkOfClass.Contains(x.ValueId))
                        .GroupBy(x => x.StudentId)
                        .Select(group => group.First())
                        .Count();
                    if (totalStudentHomework > 0)
                        data.HomeworkRate = Math.Round(totalHomeworkResult / totalStudentHomework * 100, 2);
                    result.Add(data);
                }
            }
            #endregion

            #region giáo viên - lấy các lớp mà giáo viên có tham gia giảng dạy
            if (userLogin.RoleId == (int)RoleEnum.teacher)
            {
                var listClass = await dbContext.tbl_Class.Where(x => x.Enable == true && dbContext.tbl_Schedule.Any(y => y.ClassId == x.Id && y.TeacherId == userLogin.UserInformationId) == true).ToListAsync();
                if (listClass.Count <= 0) return new AppDomainResult { TotalRow = 0, Data = null };
                var listStudentInClass = await dbContext.tbl_StudentInClass.Where(x => x.Enable == true).ToListAsync();
                var listSchedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true).ToListAsync();
                var listRollUp = await dbContext.tbl_RollUp.Where(x => x.Enable == true).ToListAsync();
                var listCurriculum = await dbContext.tbl_CurriculumInClass.Where(x => x.Enable == true).ToListAsync();
                var listCurriculumDetail = await dbContext.tbl_CurriculumDetailInClass.Where(x => x.Enable == true).ToListAsync();
                var listHomework = await dbContext.tbl_Homework.Where(x => x.Enable == true).ToListAsync();
                var listHomeworkResult = await dbContext.tbl_IeltsExamResult.Where(x => x.Enable == true).ToListAsync();
                foreach (var item in listClass)
                {
                    data = new ReportClassModel();
                    data.ClassId = item.Id;
                    data.ClassName = item.Name;
                    //danh sách học viên đã tham gia lớp
                    if (listStudentInClass.Count > 0)
                        data.TotalStudent = listStudentInClass.Count(x => x.ClassId == item.Id);
                    data.MaxQuantity = item.MaxQuantity ?? 0;
                    if (listSchedule.Count > 0)
                    {
                        //số lịch học đã hoàn thành
                        data.TotalCompletedSchedule = listSchedule.Count(x => x.ClassId == item.Id && x.Status == 2);
                        data.TotalSchedule = listSchedule.Count(x => x.ClassId == item.Id);
                    }
                    //ví dụ lớp có 3 buổi học và 10 học viên thì tổng cộng phải có 30 phiếu điểm danh
                    double totalRollUp = data.TotalSchedule * data.TotalStudent;
                    //lấy số phiếu điểm danh với trạng thái là có mặt
                    double totalStudentRollUp = listRollUp.Count(x => x.ClassId == item.Id && x.Status == 1);
                    if (totalRollUp > 0)
                        data.AttendanceRate = Math.Round(totalStudentRollUp / totalRollUp * 100, 2);
                    //lấy số tài liệu đã hoàn thành
                    if (listCurriculumDetail.Count > 0)
                        data.TotalCompletedDocument = listCurriculumDetail.Count(x => x.IsComplete == true && listCurriculum.Any(y => y.ClassId == item.Id && x.CurriculumIdInClass == y.Id) == true);
                    //lấy danh sách bài tập về nhà của lớp đó
                    var listHomeworkOfClass = listHomework.Where(x => x.ClassId == item.Id).Select(x => x.Id).ToList();
                    double totalStudentHomework = 0;
                    //ví dụ lớp có 3 bài tập về nhà và 10 học viên thì tổng cộng phải có 30 bài nộp 
                    if (listHomeworkOfClass.Count > 0)
                        totalStudentHomework = data.TotalStudent * listHomeworkOfClass.Count;
                    //lấy số bài tập học viên đã nộp ( nếu 1 học viên nộp hơn 2 lần thì cũng chỉ tính là 1 )
                    double totalHomeworkResult = listHomeworkResult.Where(x => listHomeworkOfClass.Contains(x.ValueId))
                        .GroupBy(x => x.StudentId)
                        .Select(group => group.First())
                        .Count();
                    if (totalStudentHomework > 0)
                        data.HomeworkRate = Math.Round(totalHomeworkResult / totalStudentHomework * 100, 2);
                    result.Add(data);
                }
            }
            #endregion

            var totalRow = result.Count;
            // Phân trang
            int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
            result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        #endregion

        #region biểu đồ số buổi dạy / học trong 12 tháng - sử dụng cho giáo viên, học viên, phụ huynh
        public async Task<List<StatisticalModel>> Schedule12Month(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            #region giáo viên
            if (userLogin.RoleId == (int)RoleEnum.teacher)
            {
                var listSchedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true && x.TeacherId == userLogin.UserInformationId && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new StatisticalModel();
                    data.Type = "Tháng " + i;
                    if (listSchedule.Count > 0)
                        data.Value = listSchedule.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion

            #region học viên
            if (userLogin.RoleId == (int)RoleEnum.student)
            {
                var listSchedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true
                && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.Enable == true
                        && dbContext.tbl_StudentInClass.Any(z => z.Enable == true && z.ClassId == y.Id && z.StudentId == userLogin.UserInformationId) == true) == true
                && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new StatisticalModel();
                    data.Type = "Tháng " + i;
                    if (listSchedule.Count > 0)
                        data.Value = listSchedule.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion

            #region phụ huynh
            if (userLogin.RoleId == (int)RoleEnum.parents)
            {
                var listSchedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true
                && dbContext.tbl_Class.Any(y => y.Id == x.ClassId && y.Enable == true
                        && dbContext.tbl_StudentInClass.Any(z => z.Enable == true && z.ClassId == y.Id
                            && dbContext.tbl_UserInformation.Any(u => u.Enable == true && u.UserInformationId == z.StudentId && u.ParentId == userLogin.UserInformationId) == true) == true) == true
                && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new StatisticalModel();
                    data.Type = "Tháng " + i;
                    if (listSchedule.Count > 0)
                        data.Value = listSchedule.Count(x => x.CreatedOn.Value.Month == i);
                    result.Add(data);
                }
            }
            #endregion

            return result;
        }
        #endregion

        #endregion

        #region báo cáo thống kê tài chính

        #region thống kê số liệu tổng quan
        public async Task<List<StatisticalCommentModel>> FinanceCompareOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalCommentModel>();
            var data = new StatisticalCommentModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var compare = new CompareModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                // Doanh thu trong tháng
                var RevenueInMonth = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                double totalRevenueInMonth = 0;
                if (RevenueInMonth.Count > 0)
                {
                    totalDataInMonth = RevenueInMonth.Sum(x => x.Value);
                    totalRevenueInMonth = totalDataInMonth;
                }

                var RevenuePreMonth = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth).ToListAsync();
                double totalRevenuePreMonth = 0;
                if (RevenuePreMonth.Count > 0)
                {
                    totalDataPreMonth = RevenuePreMonth.Sum(x => x.Value);
                    totalRevenuePreMonth = totalDataPreMonth;
                }
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Doanh thu trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                // Chi phí trong tháng
                var ExpensesInMonth = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 2 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                double totalExpensesInMonth = 0;
                if (ExpensesInMonth.Count > 0)
                {
                    totalDataInMonth = ExpensesInMonth.Sum(x => x.Value);
                    totalExpensesInMonth = totalDataInMonth;
                }

                var ExpensesPreMonth = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 2 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth).ToListAsync();
                double totalExpensesPreMonth = 0;
                if (ExpensesPreMonth.Count > 0)
                {
                    totalDataPreMonth = ExpensesPreMonth.Sum(x => x.Value);
                    totalExpensesPreMonth = totalDataPreMonth;
                }
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Chi phí trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                /*//lợi nhuận trong tháng = tổng doanh thu - chi phí
                totalDataInMonth = totalRevenueInMonth - totalExpensesInMonth;
                totalDataPreMonth = totalRevenuePreMonth - totalExpensesPreMonth;
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lợi nhuận trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);*/

                //tổng khuyến mãi của học viên trong tháng
                var ReduceInMonth = await dbContext.tbl_Bill.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (ReduceInMonth.Count > 0)
                {
                    totalDataInMonth = ReduceInMonth.Sum(x => x.Reduced);
                }

                var ReducePreMonth = await dbContext.tbl_Bill.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth).ToListAsync();
                if (ReducePreMonth.Count > 0)
                {
                    totalDataPreMonth = ReducePreMonth.Sum(x => x.Reduced);
                }
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Khấu trừ khuyến mãi trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //tổng nợ của học viên trong tháng
                var DeptInMonth = await dbContext.tbl_Bill.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (DeptInMonth.Count > 0)
                {
                    totalDataInMonth = DeptInMonth.Sum(x => x.Debt);
                }

                var DeptPreMonth = await dbContext.tbl_Bill.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth).ToListAsync();
                if (DeptPreMonth.Count > 0)
                {
                    totalDataPreMonth = DeptPreMonth.Sum(x => x.Debt);
                }
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Tổng nợ của học viên trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion

            return result;
        }

        // Create 26/04/24 by Dery: Change CreatedOn -> PaymentDate
        public async Task<List<StatisticalCommentModel>> FinanceCompareOverviewV2(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalCommentModel>();
            var data = new StatisticalCommentModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var compare = new CompareModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                // Doanh thu trong tháng
                var RevenueInMonth = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Month == time.Month && x.PaymentDate.Value.Year == time.Year).ToListAsync();
                double totalRevenueInMonth = 0;
                if (RevenueInMonth.Count > 0)
                {
                    totalDataInMonth = RevenueInMonth.Sum(x => x.Value);
                    totalRevenueInMonth = totalDataInMonth;
                }

                var RevenuePreMonth = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Month == time.LastMonth && x.PaymentDate.Value.Year == time.YearOfLastMonth).ToListAsync();
                double totalRevenuePreMonth = 0;
                if (RevenuePreMonth.Count > 0)
                {
                    totalDataPreMonth = RevenuePreMonth.Sum(x => x.Value);
                    totalRevenuePreMonth = totalDataPreMonth;
                }
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Doanh thu trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                // Chi phí trong tháng
                var ExpensesInMonth = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 2 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Month == time.Month && x.PaymentDate.Value.Year == time.Year).ToListAsync();
                double totalExpensesInMonth = 0;
                if (ExpensesInMonth.Count > 0)
                {
                    totalDataInMonth = ExpensesInMonth.Sum(x => x.Value);
                    totalExpensesInMonth = totalDataInMonth;
                }

                var ExpensesPreMonth = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 2 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Month == time.LastMonth && x.PaymentDate.Value.Year == time.YearOfLastMonth).ToListAsync();
                double totalExpensesPreMonth = 0;
                if (ExpensesPreMonth.Count > 0)
                {
                    totalDataPreMonth = ExpensesPreMonth.Sum(x => x.Value);
                    totalExpensesPreMonth = totalDataPreMonth;
                }
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Chi phí trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //lợi nhuận trong tháng = tổng doanh thu - chi phí
                totalDataInMonth = totalRevenueInMonth - totalExpensesInMonth;
                totalDataPreMonth = totalRevenuePreMonth - totalExpensesPreMonth;
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lợi nhuận trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //tổng nợ của học viên trong tháng
                var DeptInMonth = await dbContext.tbl_Bill.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Month == time.Month && x.PaymentDate.Value.Year == time.Year).ToListAsync();
                if (DeptInMonth.Count > 0)
                {
                    totalDataInMonth = DeptInMonth.Sum(x => x.Debt);
                }

                var DeptPreMonth = await dbContext.tbl_Bill.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Month == time.LastMonth && x.PaymentDate.Value.Year == time.YearOfLastMonth).ToListAsync();
                if (DeptPreMonth.Count > 0)
                {
                    totalDataPreMonth = DeptPreMonth.Sum(x => x.Debt);
                }
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Tổng nợ của học viên trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion

            return result;
        }
        #endregion

        #region báo cáo doanh thu theo từng tháng        
        public async Task<List<Statistical12MonthModel>> Revenue12Month(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<Statistical12MonthModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var filteredList = new List<tbl_PaymentSession>();
            var dataItem = new Statistical12MonthModel();

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                var listPaymentSessionInYear = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                var listPaymentSessionPreYear = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Year == time.LastYear).ToListAsync();
                //type = 1 là phiếu thu
                //type = 2 là phiếu chi
                for (int i = 1; i <= 12; i++)
                {
                    //doanh thu trong năm được chọn tháng thứ i
                    dataItem = new Statistical12MonthModel();
                    dataItem.Month = "Tháng " + i;
                    dataItem.Type = "Doanh thu năm nay";
                    filteredList = listPaymentSessionInYear
                        .Where(x => x.CreatedOn?.Month == i && x.Type == 1).ToList();
                    dataItem.Value = filteredList.Any() ? filteredList.Sum(x => x.Value) : 0;
                    result.Add(dataItem);
                }
                for (int i = 1; i <= 12; i++)
                {
                    //doanh thu trong năm trước tháng thứ i
                    dataItem = new Statistical12MonthModel();
                    dataItem.Month = "Tháng " + i;
                    dataItem.Type = "Doanh thu năm ngoái";
                    filteredList = listPaymentSessionPreYear
                        .Where(x => x.CreatedOn?.Month == i && x.Type == 1).ToList();
                    dataItem.Value = filteredList.Any() ? filteredList.Sum(x => x.Value) : 0;
                    result.Add(dataItem);
                }
            }
            #endregion
            return result;
        }

        // Create 26/04/24 by Dery: Change CreatedOn -> PaymentDate
        public async Task<List<Statistical12MonthModel>> Revenue12MonthV2(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<Statistical12MonthModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var filteredList = new List<tbl_PaymentSession>();
            var dataItem = new Statistical12MonthModel();

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                var listPaymentSessionInYear = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Year == time.Year).ToListAsync();
                var listPaymentSessionPreYear = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Year == time.LastYear).ToListAsync();
                //type = 1 là phiếu thu
                //type = 2 là phiếu chi
                for (int i = 1; i <= 12; i++)
                {
                    //doanh thu trong năm được chọn tháng thứ i
                    dataItem = new Statistical12MonthModel();
                    dataItem.Month = "Tháng " + i;
                    dataItem.Type = "Doanh thu năm nay";
                    filteredList = listPaymentSessionInYear
                        .Where(x => x.PaymentDate?.Month == i && x.Type == 1).ToList();
                    dataItem.Value = filteredList.Any() ? filteredList.Sum(x => x.Value) : 0;
                    result.Add(dataItem);
                }
                for (int i = 1; i <= 12; i++)
                {
                    //doanh thu trong năm trước tháng thứ i
                    dataItem = new Statistical12MonthModel();
                    dataItem.Month = "Tháng " + i;
                    dataItem.Type = "Doanh thu năm ngoái";
                    filteredList = listPaymentSessionPreYear
                        .Where(x => x.PaymentDate?.Month == i && x.Type == 1).ToList();
                    dataItem.Value = filteredList.Any() ? filteredList.Sum(x => x.Value) : 0;
                    result.Add(dataItem);
                }
            }
            #endregion
            return result;
        }
        #endregion

        #region báo cáo chi phí theo từng tháng        
        public async Task<List<Statistical12MonthModel>> Expense12Month(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<Statistical12MonthModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var filteredList = new List<tbl_PaymentSession>();
            var dataItem = new Statistical12MonthModel();

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                var listPaymentSessionInYear = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                var listPaymentSessionPreYear = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Year == time.LastYear).ToListAsync();
                //type = 1 là phiếu thu
                //type = 2 là phiếu chi
                for (int i = 1; i <= 12; i++)
                {
                    //doanh thu trong năm được chọn tháng thứ i
                    dataItem = new Statistical12MonthModel();
                    dataItem.Month = "Tháng " + i;
                    dataItem.Type = "Chi phí năm nay";
                    filteredList = listPaymentSessionInYear
                        .Where(x => x.CreatedOn?.Month == i && x.Type == 2).ToList();
                    dataItem.Value = filteredList.Any() ? filteredList.Sum(x => x.Value) : 0;
                    result.Add(dataItem);
                }
                for (int i = 1; i <= 12; i++)
                {
                    //doanh thu trong năm trước tháng thứ i
                    dataItem = new Statistical12MonthModel();
                    dataItem.Month = "Tháng " + i;
                    dataItem.Type = "Chi phí năm ngoái";
                    filteredList = listPaymentSessionPreYear
                        .Where(x => x.CreatedOn?.Month == i && x.Type == 2).ToList();
                    dataItem.Value = filteredList.Any() ? filteredList.Sum(x => x.Value) : 0;
                    result.Add(dataItem);
                }
            }
            #endregion
            return result;
        }

        // Create 26/04/24 by Dery: Change CreatedOn -> PaymentDate
        public async Task<List<Statistical12MonthModel>> Expense12MonthV2(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<Statistical12MonthModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var filteredList = new List<tbl_PaymentSession>();
            var dataItem = new Statistical12MonthModel();

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                var listPaymentSessionInYear = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Year == time.Year).ToListAsync();
                var listPaymentSessionPreYear = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Year == time.LastYear).ToListAsync();
                //type = 1 là phiếu thu
                //type = 2 là phiếu chi
                for (int i = 1; i <= 12; i++)
                {
                    //doanh thu trong năm được chọn tháng thứ i
                    dataItem = new Statistical12MonthModel();
                    dataItem.Month = "Tháng " + i;
                    dataItem.Type = "Chi phí năm nay";
                    filteredList = listPaymentSessionInYear
                        .Where(x => x.PaymentDate?.Month == i && x.Type == 2).ToList();
                    dataItem.Value = filteredList.Any() ? filteredList.Sum(x => x.Value) : 0;
                    result.Add(dataItem);
                }
                for (int i = 1; i <= 12; i++)
                {
                    //doanh thu trong năm trước tháng thứ i
                    dataItem = new Statistical12MonthModel();
                    dataItem.Month = "Tháng " + i;
                    dataItem.Type = "Chi phí năm ngoái";
                    filteredList = listPaymentSessionPreYear
                        .Where(x => x.PaymentDate?.Month == i && x.Type == 2).ToList();
                    dataItem.Value = filteredList.Any() ? filteredList.Sum(x => x.Value) : 0;
                    result.Add(dataItem);
                }
            }
            #endregion
            return result;
        }
        #endregion

        #region báo cáo doanh thu theo nguồn khách hàng
        /*public async Task<List<StatisticalModel>> RevenueBySource(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var listSource = await dbContext.tbl_Source.Where(x => x.Enable == true).ToListAsync();
            if (listSource.Count <= 0) return result;
            //type = 1 là phiếu thu
            var listPaymentSession = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
            foreach (var item in listSource)
            {
                var data = new StatisticalModel();
                data.Name = item.Name;
                data.QuantityValue = 0;
                var sourcePaymentSession = listPaymentSession.Where(x => dbContext.tbl_UserInformation.Any(y => y.UserInformationId == x.UserId && y.SourceId == item.Id) == true).ToList();
                if (sourcePaymentSession.Count > 0)
                    data.QuantityValue = sourcePaymentSession.Sum(x => x.Value);
                result.Add(data);
            }
            return result;
        }*/

        /*public async Task<List<StatisticalDetailModel>> RevenueBySource(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalDetailModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var listSource = await dbContext.tbl_Source.Where(x => x.Enable == true).ToListAsync();
            if (listSource.Count <= 0) return result;
            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                //type = 1 là phiếu thu
                var concurrentBag = new ConcurrentBag<StatisticalDetailModel>();
                var listPaymentSession = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString()))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                var listUser = await dbContext.tbl_UserInformation.Where(x => x.RoleId == (int)RoleEnum.student).ToListAsync();
                Parallel.ForEach(listSource, (item) =>
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    data.Value = 0;
                    var sourcePaymentSession = listPaymentSession.Where(x => listUser.Any(y => y.UserInformationId == x.UserId && y.SourceId == item.Id) == true).ToList();
                    if (sourcePaymentSession.Count > 0)
                        data.Value = sourcePaymentSession.Sum(x => x.Value);
                    concurrentBag.Add(data);
                });
                result = new List<StatisticalDetailModel>(concurrentBag);
            }
            #endregion
            return result;
        }*/
        // Create 26/04/24 by Dery: Change CreatedOn -> PaymentDate
        public async Task<List<StatisticalDetailModel>> RevenueBySourceV2(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalDetailModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var listSource = await dbContext.tbl_Source.Where(x => x.Enable == true).ToListAsync();
            if (listSource.Count <= 0) return result;
            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                //type = 1 là phiếu thu
                var concurrentBag = new ConcurrentBag<StatisticalDetailModel>();
                var listPaymentSession = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && x.BranchId != null
                    && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.PaymentDate.Value.Month == time.Month && x.PaymentDate.Value.Year == time.Year).ToListAsync();
                double totalRevenue = 0;
                if (listPaymentSession.Count > 0)
                    totalRevenue = listPaymentSession.Sum(x => x.Value);
                var listUser = await dbContext.tbl_UserInformation.Where(x => x.RoleId == (int)RoleEnum.student).ToListAsync();
                Parallel.ForEach(listSource, (item) =>
                {
                    var data = new StatisticalDetailModel();
                    data.Type = item.Name;
                    data.Value = 0;
                    var sourcePaymentSession = listPaymentSession.Where(x => listUser.Any(y => y.UserInformationId == x.UserId && y.SourceId == item.Id) == true).ToList();
                    if (sourcePaymentSession.Any())
                    {
                        data.ValueDetail = sourcePaymentSession.Sum(x => x.Value);
                        if (totalRevenue > 0)
                        {
                            data.Value = Math.Round((data.ValueDetail / totalRevenue * 100), 2);
                        }
                    }

                    concurrentBag.Add(data);
                });
                result = new List<StatisticalDetailModel>(concurrentBag);
            }
            #endregion
            return result;
        }
        #endregion

        #region báo cáo doanh thu theo hoạt động của trung tâm
        /*public async Task<List<StatisticalDetailModel>> RevenueByCenterActivities(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalDetailModel>();
            var data = new StatisticalDetailModel();
            double filterCount = 0;
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                var listBill = await dbContext.tbl_Bill.Where(x => x.Enable == true && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                if (listBill.Count <= 0) return result;
                double totalBill = listBill.Count;

                data = new StatisticalDetailModel();
                data.Type = "Đăng ký học";
                filterCount = listBill.Count(x => x.Type == 1 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));

                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Mua dịch vụ";
                filterCount = listBill.Count(x => x.Type == 2 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Đăng ký lớp dạy kèm";
                filterCount = listBill.Count(x => x.Type == 3 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Tạo thủ công";
                filterCount = listBill.Count(x => x.Type == 4 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Học phí hằng tháng";
                filterCount = listBill.Count(x => x.Type == 5 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Phí chuyển lớp";
                filterCount = listBill.Count(x => x.Type == 6 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);
            }
            #endregion

            return result;
        }*/

        // Create 26/04/24 by Dery: Change CreatedOn -> PaymentDate
        public async Task<List<StatisticalModel>> RevenueByCenterActivitiesV2(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            double filterCount = 0;
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                var listBill = await dbContext.tbl_Bill.Where(x => x.Enable == true && x.PaymentDate.Value.Month == time.Month && x.PaymentDate.Value.Year == time.Year).ToListAsync();
                if (listBill.Count <= 0) return result;
                double totalBill = listBill.Count;

                data = new StatisticalModel();
                data.Type = "Đăng ký học";
                filterCount = listBill.Count(x => x.Type == 1 && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")));

                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalModel();
                data.Type = "Mua dịch vụ";
                filterCount = listBill.Count(x => x.Type == 2 && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalModel();
                data.Type = "Đăng ký lớp dạy kèm";
                filterCount = listBill.Count(x => x.Type == 3 && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalModel();
                data.Type = "Tạo thủ công";
                filterCount = listBill.Count(x => x.Type == 4 && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalModel();
                data.Type = "Học phí hằng tháng";
                filterCount = listBill.Count(x => x.Type == 5 && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalModel();
                data.Type = "Phí chuyển lớp";
                filterCount = listBill.Count(x => x.Type == 6 && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);
            }
            #endregion

            return result;
        }

        /*public async Task<List<StatisticalDetailModel>> RevenueByCenterActivitiesV2(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalDetailModel>();
            var data = new StatisticalDetailModel();
            List<tbl_Bill> filterData = new List<tbl_Bill>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                var listBill = await dbContext.tbl_Bill.Where(x => x.Enable == true).ToListAsync();
                if (listBill.Count <= 0) return result;
                double totalBill = listBill.Count;

                data = new StatisticalDetailModel();
                data.Type = "Đăng ký học";
                filterData = listBill.Where(x => x.Type == 1 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString()))).ToList();
                data.Value = Math.Round(filterData.Count / totalBill * 100, 2);
                data.ValueDetail = filterData.Sum(x => x.);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Mua dịch vụ";
                filterCount = listBill.Count(x => x.Type == 2 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Đăng ký lớp dạy kèm";
                filterCount = listBill.Count(x => x.Type == 3 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Tạo thủ công";
                filterCount = listBill.Count(x => x.Type == 4 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Học phí hằng tháng";
                filterCount = listBill.Count(x => x.Type == 5 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);

                data = new StatisticalDetailModel();
                data.Type = "Phí chuyển lớp";
                filterCount = listBill.Count(x => x.Type == 6 && x.BranchId != null && (branchIds == "" || branchIds == "0" || branchIdArray.Contains(x.BranchId.ToString())));
                data.Value = Math.Round(filterCount / totalBill * 100, 2);
                result.Add(data);
            }
            #endregion

            return result;
        }*/
        #endregion

        #region báo cáo doanh thu theo từng học viên
        public class RevenueByStudentModel
        {
            /// <summary>
            /// thông tin học viên
            /// </summary>
            public string FullName { get; set; }
            public string UserCode { get; set; }
            public string Avatar { get; set; }
            /// <summary>
            /// số tiền thanh toán
            /// </summary>
            public double Value { get; set; }
            /// <summary>
            /// lý do thanh toán
            /// </summary>
            public string Reason { get; set; }
            /// <summary>
            /// phương thức thanh toán
            /// </summary>
            public string PaymentMethodName { get; set; }
            /// <summary>
            /// ngày thanh toán
            /// </summary>
            public DateTime? PaymentDate { get; set; }
            /// <summary>
            /// người duyệt thanh toán
            /// </summary>
            public string PaymentApprover { get; set; }
        }

        /*public async Task<AppDomainResult> RevenueByStudent(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<RevenueByStudentModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4).ToListAsync();
            if (listStudent.Count <= 0) return new AppDomainResult { TotalRow = 0, Data = null };
            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                listStudent = listStudent.Where(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => x.BranchIds.Split(',').Contains(b)))).ToList()
;                //type = 1 là phiếu thu
                var listPaymentSession = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                foreach (var item in listPaymentSession)
                {
                    var data = new RevenueByStudentModel();
                    var user = listStudent.SingleOrDefault(x => x.UserInformationId == item.UserId);
                    if (user == null)
                        continue;
                    data.FullName = user.FullName;
                    data.UserCode = user.UserCode;
                    data.Avatar = user.Avatar;
                    data.Value = item.Value;
                    data.PaymentMethodName = Task.Run(() => GetPaymentMethod(item.PaymentMethodId ?? 0)).Result;
                    data.Reason = item.Reason;
                    data.PaymentDate = item.PaymentDate;
                    data.PaymentApprover = item.ModifiedBy;
                    result.Add(data);
                }
            }
            #endregion
            var totalRow = result.Count;
            // Phân trang
            int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
            result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }*/

        public async Task<string> GetPaymentMethod(int paymentMethodId)
        {
            var result = "";
            var paymentMethod = await dbContext.tbl_PaymentMethod.SingleOrDefaultAsync(x => x.Id == paymentMethodId);
            if (paymentMethod != null)
            {
                result = paymentMethod.Name;
            }
            return result;
        }

        // Create 26/04/24 by Dery: Change CreatedOn -> PaymentDate
        public async Task<AppDomainResult> RevenueByStudentV2(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<RevenueByStudentModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var totalRow = 0;

            var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4).ToListAsync();
            if (listStudent.Count <= 0) return new AppDomainResult { TotalRow = 0, Data = null };
            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                ;                //type = 1 là phiếu thu
                var listPaymentSession = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")) && x.PaymentDate.Value.Month == time.Month && x.PaymentDate.Value.Year == time.Year).ToListAsync();
                totalRow = listPaymentSession.Count;
                // Phân trang
                int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
                listPaymentSession = listPaymentSession.Skip(startIndex).Take(baseSearch.PageSize).ToList();
                foreach (var item in listPaymentSession)
                {
                    var data = new RevenueByStudentModel();
                    var user = listStudent.SingleOrDefault(x => x.UserInformationId == item.UserId);
                    if (user == null)
                    {
                        totalRow--;
                        continue;
                    }
                    data.FullName = user.FullName;
                    data.UserCode = user.UserCode;
                    data.Avatar = user.Avatar;
                    data.Value = item.Value;
                    data.PaymentMethodName = Task.Run(() => GetPaymentMethod(item.PaymentMethodId ?? 0)).Result;
                    data.Reason = item.Reason;
                    data.PaymentDate = item.PaymentDate;
                    data.PaymentApprover = item.ModifiedBy;
                    result.Add(data);
                }
            }
            #endregion

            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        #endregion

        #endregion

        #region báo cáo thống kê nhân viên

        #region thống kê số liệu tổng quan
        public async Task<List<StatisticalModel>> StaffOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var listUser = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId != (int)RoleEnum.admin && x.RoleId != (int)RoleEnum.student && x.RoleId != (int)RoleEnum.parents && x.RoleId != (int)RoleEnum.tutor).ToListAsync();

            #region admin, quản lý
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager)
            {
                //số lượng nhân viên
                totalData = listUser?.Count(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")))) ?? 0;
                data = new StatisticalModel
                {
                    Type = "Tổng số nhân viên",
                    Value = totalData
                };
                result.Add(data);

                //số lượng giáo viên
                totalData = listUser?.Count(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ","))) && x.RoleId == (int)RoleEnum.teacher) ?? 0;
                data = new StatisticalModel
                {
                    Type = "Giáo viên",
                    Value = totalData
                };
                result.Add(data);

                //số lượng quản lý
                totalData = listUser?.Count(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ","))) && x.RoleId == (int)RoleEnum.manager) ?? 0;
                data = new StatisticalModel
                {
                    Type = "Quản lý",
                    Value = totalData
                };
                result.Add(data);

                //số lượng tư vấn viên           
                totalData = listUser?.Count(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ","))) && x.RoleId == (int)RoleEnum.sale) ?? 0;
                data = new StatisticalModel
                {
                    Type = "Tư vấn viên",
                    Value = totalData
                };
                result.Add(data);

                //số lượng kế toán
                totalData = listUser?.Count(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ","))) && x.RoleId == (int)RoleEnum.accountant) ?? 0;
                data = new StatisticalModel
                {
                    Type = "Kế toán",
                    Value = totalData
                };
                result.Add(data);

                //số lượng học vụ
                totalData = listUser?.Count(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ","))) && x.RoleId == (int)RoleEnum.academic) ?? 0;
                data = new StatisticalModel
                {
                    Type = "Học vụ",
                    Value = totalData
                };
                result.Add(data);
            }
            #endregion

            return result;
        }

        public async Task<List<StatisticalCommentModel>> StaffCompareOverview(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<StatisticalCommentModel>();
            var data = new StatisticalCommentModel();
            double totalData = 0;
            double totalDataInMonth = 0;
            double totalDataPreMonth = 0;
            var compare = new CompareModel();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var listUser = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId != (int)RoleEnum.admin && x.RoleId != (int)RoleEnum.student && x.RoleId != (int)RoleEnum.parents).ToListAsync();
            var listCustomerHistory = await dbContext.tbl_CustomerHistory.Where(x => x.Enable == true).ToListAsync();
            var listSchedule = await dbContext.tbl_Schedule.Where(x => x.Enable == true).ToListAsync();
            var listIeltsExamResult = await dbContext.tbl_IeltsExamResult.Where(x => x.Enable == true).ToListAsync();
            var listTestAppointment = await dbContext.tbl_TestAppointment.Where(x => x.Enable == true).ToListAsync();

            #region admin, quản lý
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager)
            {
                //số lượt khách hàng đã tư vấn ( customer status != 1 )
                //=> nếu thống kê số khách hàng tư vấn giả sử trường hợp 1 khách hàng tư vấn nhiều lần thì cũng chỉ tính là 1
                //=> tư vấn viên đó tư vấn nhiều lần mà tính là 1 thì admin nhìn vào sẽ thấy năng suất kém
                totalDataInMonth = listCustomerHistory.Count(x => x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year && x.CustomerStatusId != 1);
                totalDataPreMonth = listCustomerHistory.Count(x => x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth && x.CustomerStatusId != 1);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Lượt tư vấn khách hàng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //tỷ lệ điểm danh của giảng viên
                var listScheduleInMonth = listSchedule.Where(x => x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.StartTime.Value.Month == time.Month && x.StartTime.Value.Year == time.Year).ToList();
                double rollUpTeacherInMonth = listScheduleInMonth?.Count(x => x.TeacherAttendanceId != 0 && x.TeacherAttendanceId != null) ?? 0;
                if (listScheduleInMonth.Any())
                    totalDataInMonth = Math.Round(rollUpTeacherInMonth / listScheduleInMonth.Count * 100, 2);
                else totalDataInMonth = 0;

                var listSchedulePreMonth = listSchedule.Where(x => x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.StartTime.Value.Month == time.LastMonth && x.StartTime.Value.Year == time.YearOfLastMonth).ToList();
                double rollUpTeacherPreMonth = listSchedulePreMonth?.Count(x => x.TeacherAttendanceId != 0 && x.TeacherAttendanceId != null) ?? 0;
                if (listSchedulePreMonth.Any())
                    totalDataPreMonth = Math.Round(rollUpTeacherPreMonth / listSchedulePreMonth.Count * 100, 2);
                else totalDataPreMonth = 0;

                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Tỷ lệ điểm danh giáo viên trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                totalDataInMonth = listIeltsExamResult.Count(x =>
                    listUser.Any(y => y.UserInformationId == x.TeacherId
                        && y.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + y.BranchIds + ",").Contains("," + b + ","))))
                    && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year && x.Status == 2);
                totalDataPreMonth = listIeltsExamResult.Count(x =>
                    listUser.Any(y => y.UserInformationId == x.TeacherId
                        && y.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + y.BranchIds + ",").Contains("," + b + ","))))
                    && x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth && x.Status == 2);
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Bài tập về nhà đã chấm trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);

                //có nhập điểm => đã chấm
                totalDataInMonth = listTestAppointment.Count(x =>
                    x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")) &&
                    x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year &&
                    (!string.IsNullOrEmpty(x.ListeningPoint) || !string.IsNullOrEmpty(x.ReadingPoint) || !string.IsNullOrEmpty(x.WritingPoint) || !string.IsNullOrEmpty(x.SpeakingPoint)));
                totalDataPreMonth = listTestAppointment.Count(x =>
                    x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ",")) &&
                    x.CreatedOn.Value.Month == time.LastMonth && x.CreatedOn.Value.Year == time.YearOfLastMonth &&
                    (!string.IsNullOrEmpty(x.ListeningPoint) || !string.IsNullOrEmpty(x.ReadingPoint) || !string.IsNullOrEmpty(x.WritingPoint) || !string.IsNullOrEmpty(x.SpeakingPoint)));
                compare = CompareProgress(totalDataInMonth, totalDataPreMonth);
                data = new StatisticalCommentModel
                {
                    Type = "Hẹn test đã chấm trong tháng",
                    Value = totalDataInMonth,
                    DifferenceQuantity = compare.DifferenceQuantity,
                    DifferenceValue = compare.DifferenceValue,
                    Status = compare.Status
                };
                result.Add(data);
            }
            #endregion

            return result;
        }
        #endregion       

        #region tỷ lệ chuyển đổi khách 12 tháng 
        public async Task<List<StatisticalModel>> TestAppointment12Month(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var result = new List<StatisticalModel>();
            var data = new StatisticalModel();
            #region admin, quản lý
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager)
            {
                var listCustomer = await dbContext.tbl_Customer.Where(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                var listTestAppointment = await dbContext.tbl_TestAppointment.Where(x => x.Enable == true
                    && x.BranchId != null && (branchIds == "" || branchIds == "0" || ("," + branchIds + ",").Contains("," + x.BranchId + ","))
                    && x.CreatedOn.Value.Year == time.Year).ToListAsync();
                for (int i = 1; i <= 12; i++)
                {
                    data = new StatisticalModel();
                    data.Type = "Tháng " + i;

                    double customerInMonth = 0;
                    if (listCustomer.Count > 0)
                        customerInMonth = listCustomer.Count(x => x.CreatedOn.Value.Month == time.Month);

                    double testAppointmentInMonth = 0;
                    if (listTestAppointment.Count > 0)
                        testAppointmentInMonth = listTestAppointment.Count(x => x.CreatedOn.Value.Month == time.Month);

                    if (testAppointmentInMonth > 0 && customerInMonth > 0)
                        data.Value = Math.Round(testAppointmentInMonth / customerInMonth * 100, 2);

                    result.Add(data);
                }
            }
            #endregion            
            return result;
        }
        #endregion

        #region tỷ lệ khách hàng chuyển qua hẹn test
        public class CustomerConversionModel
        {
            public string FullName { get; set; }
            public string UserCode { get; set; }
            public string Avatar { get; set; }
            /// <summary>
            /// số lượng khách hàng trong tháng
            /// </summary>
            public double CustomerInMonth { get; set; }
            /// <summary>
            /// số buổi hẹn test trong tháng
            /// </summary>
            public double TestInMonth { get; set; }
            /// <summary>
            /// số buổi hẹn test thuộc khách hàng trong tháng
            /// </summary>
            public double TestByCustomerInMonth { get; set; }
            /// <summary>
            /// thời gian chuyển đổi tỷ lệ nhanh nhất
            /// </summary>
            public double? MinTimeConversion { get; set; } = 0;
            /// <summary>
            /// thời gian chuyển đổi tỷ lệ lâu nhất
            /// </summary>
            public double? MaxTimeConversion { get; set; } = 0;
        }
        public async Task<AppDomainResult> CustomerConversion(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var result = new List<CustomerConversionModel>();
            var data = new CustomerConversionModel();

            var listUser = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.LearningStatus > 4).ToListAsync() ?? new List<tbl_UserInformation>();
            //var listSale = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.sale).ToListAsync() ?? new List<tbl_UserInformation>();
            var listCustomer = await dbContext.tbl_Customer.Where(x => x.Enable == true).ToListAsync() ?? new List<tbl_Customer>();
            var listTestAppointment = await dbContext.tbl_TestAppointment.Where(x => x.Enable == true).ToListAsync() ?? new List<tbl_TestAppointment>();
            #region admin, quản lý
            //1 số lượng khách của thằng tư vấn viến trong tháng
            //2 tổng số lượt hẹn test của tư vấn viên đó đã tạo trong tháng => đã tạo trong tháng
            //3 tổng số lượng hẹn test của tư vấn viên đó thuộc khách hàng trong tháng => đã tạo trong tháng với điều kiện khách hàng cũng phải đc tạo trong tháng
            //4 thời gian chuyển đổi khách hàng ngắn nhất
            //5 thời gian chuyển đổi khách hàng lâu nhất
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager)
            {
                var listSale = listUser.Where(x => x.RoleId == (int)RoleEnum.sale && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")))).ToList();

                if (listSale.Count <= 0) return new AppDomainResult { TotalRow = 0, Data = null };
                foreach (var sale in listSale)
                {
                    //var listTotalCustomerOfSale = listCustomer.Where(x => x.SaleId == sale.UserInformationId && x.BranchId != null && (branchIds == "" || branchIds == "0" || (branchIdArray.Contains(x.BranchId.ToString())))).ToList();
                    var listCustomerOfSale = listCustomer.Where(x => x.SaleId == sale.UserInformationId).ToList();
                    data = new CustomerConversionModel();
                    data.FullName = sale.FullName;
                    data.UserCode = sale.UserCode;
                    data.Avatar = sale.Avatar;
                    data.CustomerInMonth = listCustomerOfSale.Count(x => x.CreatedOn.Value.Year == time.Year && x.CreatedOn.Value.Month == time.Month);
                    data.TestInMonth = listTestAppointment.Count(x => x.CreatedOn.Value.Year == time.Year && x.CreatedOn.Value.Month == time.Month && listCustomer.Any(y => y.Id == x.CustomerId && y.SaleId == sale.UserInformationId) == true);
                    data.TestByCustomerInMonth = listTestAppointment.Count(x => x.CreatedOn.Value.Year == time.Year && x.CreatedOn.Value.Month == time.Month && listCustomer.Any(y => y.Id == x.CustomerId && y.SaleId == sale.UserInformationId && y.CreatedOn.Value.Year == time.Year && y.CreatedOn.Value.Month == time.Month) == true);
                    if (listCustomerOfSale.Count > 0)
                    {
                        double? maxTime = 0;
                        double? minTime = 0;
                        foreach (var customer in listCustomerOfSale)
                        {
                            var testAppointmentOfCustomer = listTestAppointment.Where(x => x.CustomerId == customer.Id).ToList();
                            if (testAppointmentOfCustomer.Count > 0)
                            {
                                foreach (var testAppointment in testAppointmentOfCustomer)
                                {
                                    TimeSpan duration = testAppointment.CreatedOn.Value - customer.CreatedOn.Value;
                                    if (minTime == 0 || minTime > duration.TotalHours)
                                        minTime = duration.TotalHours;
                                    if (maxTime < duration.TotalHours)
                                        maxTime = duration.TotalHours;
                                }
                            }
                        }
                        data.MinTimeConversion = Math.Round(Math.Abs(minTime ?? 0), 2);
                        data.MaxTimeConversion = Math.Round(Math.Abs(maxTime ?? 0), 2);
                    }
                    result.Add(data);
                }
            }
            #endregion            
            var totalRow = result.Count;
            // Phân trang
            int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
            result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        #endregion

        #region thống kê doanh thu của tư vấn viên - xếp hạng
        public class ConsultingRevenueModel
        {
            public string UserCode { get; set; }
            public string FullName { get; set; }
            public string Avatar { get; set; }
            public double TotalRevenue { get; set; } = 0;
        }
        public async Task<AppDomainResult> ConsultingRevenue(StatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new StatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<ConsultingRevenueModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');
            var listSale = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.sale).ToListAsync();
            var listConsultingRevenue = await dbContext.tbl_ConsultantRevenue.Where(x => x.Enable == true && x.CreatedOn.Value.Month == time.Month && x.CreatedOn.Value.Year == time.Year).ToListAsync();
            #region admin, quản lý
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager)
            {
                listSale = listSale.Where(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")))).ToList();

                if (listSale.Count <= 0) return new AppDomainResult { TotalRow = 0, Data = null };
                foreach (var item in listSale)
                {
                    var data = new ConsultingRevenueModel();
                    data.UserCode = item.UserCode;
                    data.FullName = item.FullName;
                    data.Avatar = item.Avatar;
                    var saleConsultingRevenue = listConsultingRevenue.Where(x => x.SaleId == item.UserInformationId).ToList();
                    if (saleConsultingRevenue.Count > 0)
                    {
                        data.TotalRevenue = saleConsultingRevenue.Sum(x => x.AmountPaid);
                    }
                    result.Add(data);
                }
            }
            #endregion          
            var totalRow = result.Count;
            result = result.OrderByDescending(x => x.TotalRevenue).ToList();
            // Phân trang
            int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
            result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        #endregion

        #endregion

        #region thống kê và export hoa hồng của tư vấn viên
        public class CommissionOfSaleModel
        {
            /// <summary>
            /// Id tư vấn viên
            /// </summary>
            public int UserId { get; set; }
            /// <summary>
            /// tên tư vấn viên
            /// </summary>
            public string FullName { get; set; }
            /// <summary>
            /// mã tư vấn viên
            /// </summary>
            public string UserCode { get; set; }
            /// <summary>
            /// ảnh đại diện tư vấn viên
            /// </summary>
            public string Avatar { get; set; }
            /// <summary>
            /// ảnh đại diện tư vấn viên (GIẢM DUNG LƯỢNG)
            /// </summary>
            public string AvatarReSize { get; set; }
            /// <summary>
            /// số lượng học viên đóng học phí (đóng bao nhiêu cũng tính)
            /// </summary>
            public int NumberStudentPayingTuition { get; set; }
            /// <summary>
            /// số lượng học viên đóng 100% học phí
            /// </summary>
            public int NumberStudentPayingFullTuition { get; set; }
            /// <summary>
            /// số học viên được mở lớp trong tháng
            /// </summary>
            public int NumberStudentHaveOpenClass { get; set; }
            /// <summary>
            /// % lớp của tư vấn viên mở so với tổng
            /// </summary>
            public double RateStudentHaveClass { get; set; }
        }
        public static async Task<List<CommissionOfSaleModel>> CommissionOfSaleProcess(CommissionOfSaleSearch baseSearch, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CommissionOfSaleSearch();
                var result = new List<CommissionOfSaleModel>();

                string branchIds = baseSearch.BranchIds ?? "";
                if (userLogin.RoleId != (int)RoleEnum.admin)
                    branchIds = userLogin.BranchIds;
                string sql = $"Get_User @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {1}," +
                    $"@PageSize = {999999}," +
                    $"@FullName = N'{""}'," +
                    $"@UserCode = N'{""}'," +
                    $"@RoleIds = N'{(int)RoleEnum.sale}'," +
                    $"@BranchIds = N'{branchIds ?? ""}'," +
                    $"@MyBranchIds= N'{""}'," +
                    $"@SaleId = {0}," +
                    $"@Genders = N'{""}'," +
                    $"@Sort = {0}," +
                    $"@ParentIds = '{""}'," +
                    $"@StudentIds = '{""}'," +
                    $"@SortType = {false}";
                var dataSale = await db.SqlQuery<Get_UserInformation>(sql);
                if (!dataSale.Any()) return result;
                var listSale = dataSale.Select(i => new UserInformationModel(i)).ToList();

                //nếu người xem thông tin là sale thì chỉ lấy ra data của chính người đó
                if (userLogin.RoleId == (int)RoleEnum.sale)
                    listSale = listSale.Where(x => x.UserInformationId == userLogin.UserInformationId).ToList();

                var listConsultantRevenue = await db.tbl_ConsultantRevenue.Where(x => x.Enable == true
                    && (baseSearch.Month == 0 || baseSearch.Month != 0 && x.CreatedOn.Value.Month == baseSearch.Month)
                    && (baseSearch.Year == 0 || baseSearch.Year != 0 && x.CreatedOn.Value.Year == baseSearch.Year))
                    .ToListAsync();

                var listStudentInClass = await db.tbl_StudentInClass.Where(x => x.Enable == true
                    && (baseSearch.Month == 0 || baseSearch.Month != 0 && x.CreatedOn.Value.Month == baseSearch.Month)
                    && (baseSearch.Year == 0 || baseSearch.Year != 0 && x.CreatedOn.Value.Year == baseSearch.Year))
                    .ToListAsync();

                var listClass = await db.tbl_Class.Where(x => x.Enable == true
                    && (baseSearch.Month == 0 || baseSearch.Month != 0 && x.StartDay.Value.Month == baseSearch.Month)
                    && (baseSearch.Year == 0 || baseSearch.Year != 0 && x.StartDay.Value.Year == baseSearch.Year))
                    .ToListAsync();

                foreach (var item in listSale)
                {
                    var data = new CommissionOfSaleModel();
                    data.UserId = item.UserInformationId;
                    data.UserCode = item.UserCode;
                    data.FullName = item.FullName;
                    data.Avatar = item.Avatar;
                    data.AvatarReSize = item.AvatarReSize;
                    data.NumberStudentPayingTuition = listConsultantRevenue.Count(x => x.SaleId == item.UserInformationId);
                    data.NumberStudentPayingFullTuition = listConsultantRevenue.Count(x => x.SaleId == item.UserInformationId && x.TotalPrice <= x.AmountPaid);
                    //với nếu nhiều học viên học chung 1 lớp thì cũng chỉ tính là 1
                    listStudentInClass = listStudentInClass.Where(x => db.tbl_UserInformation.Any(y => y.UserInformationId == x.StudentId && y.SaleId == item.UserInformationId) == true)
                                                            .GroupBy(x => x.ClassId)
                                                            .Select(group => group.FirstOrDefault()).ToList();
                    data.NumberStudentHaveOpenClass = listStudentInClass.Count(x => db.tbl_UserInformation.Any(y => y.UserInformationId == x.StudentId && y.SaleId == item.UserInformationId) == true);
                    data.RateStudentHaveClass = 0;
                    if (listStudentInClass.Count > 0)
                    {
                        data.RateStudentHaveClass = Math.Round(data.NumberStudentHaveOpenClass / (double)listClass.Count * 100, 2);
                    }
                    result.Add(data);
                }
                return result;
            }
        }

        #region thống kê hoa hồng của tư vấn viên      
        public static async Task<AppDomainResult> StatisticalCommissionOfSale(CommissionOfSaleSearch baseSearch, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CommissionOfSaleSearch();
                var result = await CommissionOfSaleProcess(baseSearch, userLogin);
                if (!result.Any() || result == null) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = result.Count;
                // Phân trang
                int startIndex = (baseSearch.PageIndex - 1) * baseSearch.PageSize;
                result = result.Skip(startIndex).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        #endregion

        #region export hoa hồng của tư vấn viên
        public class ExportCommissionOfSaleModel
        {
            /// <summary>
            /// mã tư vấn viên
            /// </summary>
            public string UserCode { get; set; }
            /// <summary>
            /// tên tư vấn viên
            /// </summary>
            public string FullName { get; set; }
            /// <summary>
            /// số lượng học viên đóng học phí (đóng bao nhiêu cũng tính)
            /// </summary>
            public int NumberStudentPayingTuition { get; set; }
            /// <summary>
            /// số lượng học viên đóng 100% học phí
            /// </summary>
            public int NumberStudentPayingFullTuition { get; set; }
            /// <summary>
            /// số học viên được mở lớp trong tháng
            /// </summary>
            public int NumberStudentHaveOpenClass { get; set; }
            /// <summary>
            /// % lớp của tư vấn viên mở so với tổng
            /// </summary>
            public double RateStudentHaveClass { get; set; }
            public ExportCommissionOfSaleModel() { }
            public ExportCommissionOfSaleModel(object model)
            {
                foreach (PropertyInfo me in GetType().GetProperties())
                {
                    foreach (PropertyInfo item in model.GetType().GetProperties())
                    {
                        if (me.Name == item.Name)
                        {
                            me.SetValue(this, item.GetValue(model));
                        }
                    }
                }
            }
        }
        public static async Task<List<ExportCommissionOfSaleModel>> ExportCommissionOfSale(CommissionOfSaleSearch baseSearch, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CommissionOfSaleSearch();
                var result = new List<ExportCommissionOfSaleModel>();
                var data = await CommissionOfSaleProcess(baseSearch, userLogin);
                if (data != null && data.Any())
                    result = data.Select(i => new ExportCommissionOfSaleModel(i)).ToList();
                return result;
            }
        }
        #endregion

        #endregion

        #region thống kê trong lớp học
        public static async Task<RollUpStatisticalModel> StatisticalRollUp(int classId)
        {
            using (var db = new lmsDbContext())
            {
                var result = new RollUpStatisticalModel();
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Enable == true && x.Id == classId);
                if (_class == null)
                    throw new Exception("Không tìm thấy thông tin lớp");
                result.ClassId = _class.Id;
                result.ClassName = _class.Name;
                var schedules = await db.tbl_Schedule.Where(x => x.Enable == true && x.ClassId == classId && x.StartTime.Value.Date <= DateTime.Now.Date).OrderByDescending(x => x.StartTime).ToListAsync();
                if (schedules.Count <= 0) return result;
                var rollUps = await db.tbl_RollUp.Where(x => x.Enable == true && x.ClassId == classId).ToListAsync();
                var datas = new List<RollUpData>();

                foreach (var item in schedules)
                {
                    var details = new List<RollUpDetail>();
                    var data = new RollUpData();
                    data.StartTime = item.StartTime;
                    data.EndTime = item.EndTime;
                    #region Có mặt
                    var detail = new RollUpDetail();
                    detail.Status = 1;
                    detail.StatusName = "Có mặt";
                    if (rollUps.Any() && rollUps != null)
                    {
                        detail.TotalRollUp = rollUps.Count(x => x.ScheduleId == item.Id);
                        detail.TotalRollUpByStatus = rollUps.Count(x => x.ScheduleId == item.Id && x.Status == detail.Status);
                    }
                    details.Add(detail);
                    #endregion

                    #region Vắng có phép
                    detail = new RollUpDetail();
                    detail.Status = 2;
                    detail.StatusName = "Vắng có phép";
                    if (rollUps.Any() && rollUps != null)
                    {
                        detail.TotalRollUp = rollUps.Count(x => x.ScheduleId == item.Id);
                        detail.TotalRollUpByStatus = rollUps.Count(x => x.ScheduleId == item.Id && x.Status == detail.Status);
                    }
                    details.Add(detail);
                    #endregion

                    #region Vắng không phép
                    detail = new RollUpDetail();
                    detail.Status = 3;
                    detail.StatusName = "Vắng không phép";
                    if (rollUps.Any() && rollUps != null)
                    {
                        detail.TotalRollUp = rollUps.Count(x => x.ScheduleId == item.Id);
                        detail.TotalRollUpByStatus = rollUps.Count(x => x.ScheduleId == item.Id && x.Status == detail.Status);
                    }
                    details.Add(detail);
                    #endregion

                    #region Đi muộn
                    detail = new RollUpDetail();
                    detail.Status = 4;
                    detail.StatusName = "Đi muộn";
                    if (rollUps.Any() && rollUps != null)
                    {
                        detail.TotalRollUp = rollUps.Count(x => x.ScheduleId == item.Id);
                        detail.TotalRollUpByStatus = rollUps.Count(x => x.ScheduleId == item.Id && x.Status == detail.Status);
                    }
                    details.Add(detail);
                    #endregion

                    #region Về sớm
                    detail = new RollUpDetail();
                    detail.Status = 5;
                    detail.StatusName = "Về sớm";
                    if (rollUps.Any() && rollUps != null)
                    {
                        detail.TotalRollUp = rollUps.Count(x => x.ScheduleId == item.Id);
                        detail.TotalRollUpByStatus = rollUps.Count(x => x.ScheduleId == item.Id && x.Status == detail.Status);
                    }
                    details.Add(detail);
                    #endregion

                    #region Nghỉ lễ
                    //detail = new RollUpDetail();
                    //detail.Status = 6;
                    //detail.StatusName = "Nghỉ lễ";
                    //if (rollUps.Any() && rollUps != null)
                    //{
                    //    detail.TotalRollUp = rollUps.Count(x => x.ScheduleId == item.Id);
                    //    detail.TotalRollUpByStatus = rollUps.Count(x => x.ScheduleId == item.Id && x.Status == detail.Status);
                    //}
                    //details.Add(detail);
                    #endregion
                    data.RollUpDetails = details;
                    datas.Add(data);
                }
                result.RollUpDatas = datas;
                return result;
            }
        }
        public static async Task<HomeworkStatisticalModel> StatisticalHomework(int classId)
        {
            using (var db = new lmsDbContext())
            {
                var result = new HomeworkStatisticalModel();
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Enable == true && x.Id == classId);
                if (_class == null)
                    throw new Exception("Không tìm thấy thông tin lớp");
                result.ClassId = _class.Id;
                result.ClassName = _class.Name;
                var homeworks = await db.tbl_Homework.Where(x => x.Enable == true && x.ClassId == classId).OrderByDescending(x => x.CreatedOn).ToListAsync();
                var ieltsExamResult = await db.tbl_IeltsExamResult.Where(x => x.Enable == true && x.Type == 3).ToListAsync();
                if (homeworks.Count <= 0) return result;
                var TotalStudent = await db.tbl_StudentInClass.CountAsync(x => x.Enable == true && x.ClassId == classId);
                var studentHomeworks = await db.tbl_StudentHomework.Where(x => x.Enable == true && x.ClassId == classId).ToListAsync();
                var datas = new List<HomeworkData>();

                foreach (var item in homeworks)
                {
                    var data = new HomeworkData();
                    data.HomeworkId = item.Id;
                    data.HomeworkName = item.Name;
                    data.HomeworkSubmit = studentHomeworks.Where(x => x.Status == (int)StudentHomeworkStatus.DaNop && x.HomeworkId == item.Id).Count();
                    data.HomeworkGraded = ieltsExamResult.Count(x => x.Status == 2 && x.ValueId == item.Id);
                    if (TotalStudent > 0)
                        data.HomeworkRate = Math.Round(((double)data.HomeworkSubmit / (double)TotalStudent * 100), 2);
                    datas.Add(data);
                }
                result.HomeworkDatas = datas;
                return result;
            }
        }
        public static async Task<StudentInClassStatisticalModel> StatisticalStudentInClass(StatisticalStudentInClassSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var result = new StudentInClassStatisticalModel();
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Enable == true && x.Id == baseSearch.ClassId);
                if (_class == null)
                    throw new Exception("Không tìm thấy thông tin lớp");
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.Enable == true && x.UserInformationId == baseSearch.StudentId);
                if (student == null)
                    throw new Exception("Không tìm thấy thông tin học viên");

                var ieltsExamResults = await db.tbl_IeltsExamResult.Where(x => x.Enable == true && x.Type == 3 && x.StudentId == baseSearch.StudentId).ToListAsync();
                var rollUps = await db.tbl_RollUp.Where(x => x.Enable == true && x.ClassId == baseSearch.ClassId && x.StudentId == baseSearch.StudentId).ToListAsync();
                var scoreColumns = await db.tbl_ScoreColumn.Where(x => x.Enable == true && x.ClassId == baseSearch.ClassId).OrderBy(x => x.Index).ToListAsync();
                var scores = await db.tbl_Score.Where(x => x.Enable == true && x.ClassId == baseSearch.ClassId && x.StudentId == baseSearch.StudentId).ToListAsync();
                var scheduleDatas = new List<ScheduleStatisticalModel>();
                var schedules = await db.tbl_Schedule.Where(x => x.Enable == true && x.ClassId == baseSearch.ClassId).OrderByDescending(x => x.StartTime).ToListAsync();
                if (schedules.Any() && scheduleDatas != null)
                {
                    foreach (var item in schedules)
                    {
                        var scheduleData = new ScheduleStatisticalModel();
                        var rollUp = rollUps.SingleOrDefault(x => x.ScheduleId == item.Id);
                        if (rollUp == null) rollUp = new tbl_RollUp();
                        scheduleData.StudyDate = item.StartTime;
                        scheduleData.Status = rollUp.Status;
                        scheduleData.StatusName = rollUp.StatusName;
                        scheduleData.LearningStatus = rollUp.LearningStatus;
                        scheduleData.LearningStatusName = rollUp.LearningStatusName;
                        scheduleData.Note = rollUp.Note;
                        scheduleDatas.Add(scheduleData);
                    }
                }
                result.ScheduleDatas = scheduleDatas;

                var historyDatas = new List<HistoryStatisticalModel>();
                var homeworks = await db.tbl_Homework.Where(x => x.Enable == true && x.ClassId == baseSearch.ClassId).OrderByDescending(x => x.CreatedOn).ToListAsync();
                if (homeworks.Any() && homeworks != null)
                {
                    foreach (var item in homeworks)
                    {
                        var historyData = new HistoryStatisticalModel();
                        var ieltsExamResult = ieltsExamResults.FirstOrDefault(x => x.ValueId == item.Id);
                        if (ieltsExamResult == null) ieltsExamResult = new tbl_IeltsExamResult();
                        historyData.HomeworkName = item.Name;
                        historyData.DoingDate = ieltsExamResult.CreatedOn;
                        historyData.TimeSpent = ieltsExamResult.TimeSpent;
                        historyData.TotalPoint = ieltsExamResult.MyPoint;
                        historyData.Status = ieltsExamResult.Status;
                        historyData.StatusName = ieltsExamResult.StatusName;
                        historyDatas.Add(historyData);
                    }
                }
                result.HistoryDatas = historyDatas;

                var classTranscriptDatas = new List<ClassTranscriptModel>();
                var classTranscripts = await db.tbl_ClassTranscript.Where(x => x.Enable == true && x.ClassId == baseSearch.ClassId).OrderByDescending(x => x.CreatedOn).ToListAsync();
                if (classTranscripts.Count != 0)
                {
                    foreach (var item in classTranscripts)
                    {
                        var classTranscriptData = new ClassTranscriptModel();
                        var classTranscriptDetailDatas = new List<ClassTranscriptDetailModel>();
                        classTranscriptData.Id = item.Id;
                        classTranscriptData.Name = item.Name;
                        classTranscriptData.ClassId = item.ClassId;
                        classTranscriptData.SampleTranscriptId = item.SampleTranscriptId;
                        classTranscriptData.Date = item.Date;

                        var classTranscriptDetail = await db.tbl_ClassTranscriptDetail.Where(x => x.ClassTranscriptId == item.Id).ToListAsync();
                        if (classTranscriptDetail.Count != 0)
                        {
                            foreach (var ctd in classTranscriptDetail)
                            {
                                var classGrades = await db.tbl_SaveGradesInClass.FirstOrDefaultAsync(x => x.Enable == true && x.ClassTranscriptId == item.Id && x.ClassTranscriptDetailId == ctd.Id);
                                if (classGrades != null)
                                {
                                    var saveGradesInClassData = new SaveGradesInClassModel
                                    {
                                        Id = classGrades.Id,
                                        ClassTranscriptId = classGrades.ClassTranscriptId,
                                        ClassTranscriptDetailId = classGrades.ClassTranscriptDetailId,
                                        StudentId = classGrades.StudentId,
                                        Value = classGrades.Value
                                    };

                                    var classTranscriptDetailData = new ClassTranscriptDetailModel
                                    {
                                        ClassTranscriptId = ctd.ClassTranscriptId,
                                        Name = ctd.Name,
                                        Type = ctd.Type,
                                        MaxValue = ctd.MaxValue,
                                        Index = ctd.Index,
                                        SaveGradesInClassData = saveGradesInClassData
                                    };
                                    classTranscriptDetailDatas.Add(classTranscriptDetailData);
                                }
                            }
                        }
                        classTranscriptData.ClassTranscriptDetailData = classTranscriptDetailDatas;
                        classTranscriptDatas.Add(classTranscriptData);
                    }
                }
                result.ClassTranscriptDatas = classTranscriptDatas;
                return result;
            }
        }

        public static async Task<StatisticalHomeworkKeywordsModel> StatisticalHomeworkKeywords(StatisticalHomeworkKeywordsSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var result = new StatisticalHomeworkKeywordsModel();
                var _class = await db.tbl_Class.Where(x => x.Enable == true && x.Id == baseSearch.ClassId).Select(x => new { x.Id }).SingleOrDefaultAsync();
                if (_class == null)
                    throw new Exception("Không tìm thấy thông tin lớp");
                var student = await db.tbl_UserInformation.Where(x => x.Enable == true && x.UserInformationId == baseSearch.StudentId).Select(x => new { x.UserInformationId }).SingleOrDefaultAsync();
                if (student == null)
                    throw new Exception("Không tìm thấy thông tin học viên");
                var homeworkDatas = new List<HomeworkModel>();

                // Lấy data ra trước để sử dung sau
                var homeworks = await db.tbl_Homework.Where(x => x.Enable == true && x.Type == HomeworkType.Exam && x.ClassId == baseSearch.ClassId).ToListAsync();
                var tags = await (from t in db.tbl_Tag
                                  join tg in db.tbl_TagCategory on t.TagCategoryId equals tg.Id
                                  where t.Enable == true && tg.Enable == true && tg.Type == 2
                                  select new
                                  {
                                      t.Id,
                                      t.Name
                                  }).OrderBy(x => x.Name).ToListAsync();
                var ieltsSkillDatas = await db.tbl_IeltsSkillResult.Where(x => x.Enable == true).Select(x => new { x.Id, x.IeltsExamResultId, x.IeltsExamId }).ToListAsync();
                var ieltsQuestionGroupDatas = await db.tbl_IeltsQuestionGroupResult.Where(x => x.Enable == true).Select(x => new { x.Id, x.IeltsSkillId, x.Type, x.TagIds, x.IeltsSectionResultId }).ToListAsync();
                var ieltsQuestionResultDatas = await db.tbl_IeltsQuestionResult.Where(x => x.Enable == true).Select(x => new { x.Id, x.IeltsQuestionGroupResultId, x.Correct, x.Point }).ToListAsync();
                var ieltsExamResultDatas = await db.tbl_IeltsExamResult.Where(x => x.Enable == true && x.Status != 1).OrderByDescending(x => x.StartTime.Date).ThenByDescending(x => x.StartTime.TimeOfDay).Select(x => new { x.Id, x.TimeSpent, x.ValueId, x.StudentId, x.IeltsExamId, x.StartTime }).ToListAsync();
                var ieltsSectionDatas = await db.tbl_IeltsSectionResult.Where(x => x.Enable == true).Select(x => new { x.Id, x.IeltsSkillResultId }).ToListAsync();

                var statisticalTagDatas = new List<StatisticalTagModel>();
                var statisticalTagWrittingAndSpeackingDatas = new List<StatisticalTagWrittingAndSpeackingModel>();

                if (homeworks.Count != 0 && tags.Count != 0)
                {
                    foreach (var h in homeworks)
                    {
                        var ieltsExamResults = ieltsExamResultDatas.Where(x => x.IeltsExamId == h.IeltsExamId && x.ValueId == h.Id && x.StudentId == baseSearch.StudentId).ToList();
                        if (ieltsExamResults.Count != 0)
                        {
                            foreach (var i in ieltsExamResults)
                            {
                                var tagDatas = new List<TagModel>();
                                var tagWrittingAndSpeackingDatas = new List<TagWrittingAndSpeackingModel>();

                                int timeSpent = i.TimeSpent;
                                string startTime = i.StartTime.ToString("HH:mm dd/MM/yyyy");
                                foreach (var t in tags)
                                {
                                    // Số liệu cho thống kê theo từng bài tập
                                    int totalQuestion = 0;
                                    int totalQuestionCorrect = 0;
                                    int totalQuestionFail = 0;
                                    double percentage = 0;

                                    int totalQuestionWrittingAndSpeaking = 0;
                                    double totalPointWrittingAndSpeaking = 0;
                                    double average = 0;
                                    var ieltsSkills = ieltsSkillDatas.Where(x => x.IeltsExamResultId == i.Id && x.IeltsExamId == h.IeltsExamId).ToList();
                                    if (ieltsSkills.Count != 0)
                                    {
                                        foreach (var ik in ieltsSkills)
                                        {
                                            var ieltsSections = ieltsSectionDatas.Where(x => x.IeltsSkillResultId == ik.Id).ToList();
                                            foreach (var ils in ieltsSections)
                                            {
                                                if (ieltsQuestionGroupDatas.Count != 0)
                                                {
                                                    // Lấy danh sách các câu hỏi có từ khóa không phải là nói và viết
                                                    var ieltsQuestionGroups = ieltsQuestionGroupDatas.Where(x => x.IeltsSectionResultId == ils.Id && !string.IsNullOrEmpty(x.TagIds) && x.TagIds.Contains(t.Id.ToString()) && x.Type != 7 && x.Type != 8).ToList();
                                                    if (ieltsQuestionGroups.Count != 0)
                                                    {
                                                        foreach (var iq in ieltsQuestionGroups)
                                                        {
                                                            var ieltsQuestionResults = ieltsQuestionResultDatas.Where(x => x.IeltsQuestionGroupResultId == iq.Id).ToList();

                                                            // Tính số liệu thống kê theo từng bài tập
                                                            totalQuestion += ieltsQuestionResults.Count();
                                                            totalQuestionCorrect += ieltsQuestionResults.Count(x => x.Correct == true);
                                                            totalQuestionFail += ieltsQuestionResults.Count(x => x.Correct == false);
                                                        }
                                                    }

                                                    var ieltsQuestionGroupWrittingAndSpeackings = ieltsQuestionGroupDatas.Where(x => x.IeltsSectionResultId == ils.Id && !string.IsNullOrEmpty(x.TagIds) && x.TagIds.Contains(t.Id.ToString()) && (x.Type == 7 || x.Type == 8)).ToList();
                                                    if (ieltsQuestionGroupWrittingAndSpeackings.Count != 0)
                                                    {
                                                        foreach (var iqw in ieltsQuestionGroupWrittingAndSpeackings)
                                                        {
                                                            // Lấy danh sách các câu hỏi có từ khóa không là nói và viết
                                                            var ieltsQuestionResultWrittingAndSpeackings = ieltsQuestionResultDatas.Where(x => x.IeltsQuestionGroupResultId == iqw.Id && x.Point != null).ToList();
                                                            if (ieltsQuestionResultWrittingAndSpeackings.Count != 0)
                                                            {
                                                                // Tính số liệu thống kê theo từng bài tập
                                                                totalQuestionWrittingAndSpeaking += ieltsQuestionResultWrittingAndSpeackings.Count();
                                                                totalPointWrittingAndSpeaking += ieltsQuestionResultWrittingAndSpeackings.Sum(x => x.Point ?? 0);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (totalQuestion != 0)
                                    {
                                        if (totalQuestion != 0 && totalQuestionCorrect != 0)
                                        {
                                            // Tính số liệu thống kê theo từng bài tập
                                            percentage = Math.Round((((double)totalQuestionCorrect / totalQuestion) * 100), 2);
                                        }
                                        var tag = new TagModel
                                        {
                                            Id = t.Id,
                                            Name = t.Name,
                                            TotalQuestion = totalQuestion,
                                            TotalQuestionCorrect = totalQuestionCorrect,
                                            TotalQuestionFail = totalQuestionFail,
                                            Percentage = percentage
                                        };
                                        tagDatas.Add(tag);
                                    }
                                    if (totalQuestionWrittingAndSpeaking != 0)
                                    {
                                        // Tính số liệu thống kê theo từng bài tập
                                        average = Math.Round(((double)totalPointWrittingAndSpeaking / totalQuestionWrittingAndSpeaking), 2);


                                        var tagWrittingAndSpeacking = new TagWrittingAndSpeackingModel
                                        {
                                            Id = t.Id,
                                            Name = t.Name,
                                            TotalQuestion = totalQuestionWrittingAndSpeaking,
                                            Average = average
                                        };
                                        tagWrittingAndSpeackingDatas.Add(tagWrittingAndSpeacking);
                                    }
                                }
                                var homework = new HomeworkModel
                                {
                                    HomeworkId = h.Id,
                                    HomeworkName = h.Name,
                                    TimeSpent = timeSpent,
                                    StartTime = startTime,
                                    Tags = tagDatas,
                                    TagWrittingAndSpeacking = tagWrittingAndSpeackingDatas
                                };
                                homeworkDatas.Add(homework);
                            }
                        }
                    }
                    result.ClassId = baseSearch.ClassId;
                    result.StudentId = baseSearch.StudentId;
                    result.Homeworks = homeworkDatas;
                }
                return result;
            }
        }

        public static async Task<StatisticalTotalHomeworkKeywordsModel> StatisticalTotalHomeworkKeywords(StatisticalHomeworkKeywordsSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var result = new StatisticalTotalHomeworkKeywordsModel();
                var _class = await db.tbl_Class.Where(x => x.Enable == true && x.Id == baseSearch.ClassId).Select(x => new { x.Id }).SingleOrDefaultAsync();
                if (_class == null)
                    throw new Exception("Không tìm thấy thông tin lớp");
                var student = await db.tbl_UserInformation.Where(x => x.Enable == true && x.UserInformationId == baseSearch.StudentId).Select(x => new { x.UserInformationId }).SingleOrDefaultAsync();
                if (student == null)
                    throw new Exception("Không tìm thấy thông tin học viên");

                // Lấy data ra trước để sử dung sau
                var homeworks = await db.tbl_Homework.Where(x => x.Enable == true && x.Type == HomeworkType.Exam && x.ClassId == baseSearch.ClassId).ToListAsync();
                var tags = await (from t in db.tbl_Tag
                                  join tg in db.tbl_TagCategory on t.TagCategoryId equals tg.Id
                                  where t.Enable == true && tg.Enable == true && tg.Type == 2
                                  select new
                                  {
                                      t.Id,
                                      t.Name
                                  }).OrderBy(x => x.Name).ToListAsync();
                var ieltsSkillDatas = await db.tbl_IeltsSkillResult.Where(x => x.Enable == true).Select(x => new { x.Id, x.IeltsExamResultId, x.IeltsExamId }).ToListAsync();
                var ieltsQuestionGroupDatas = await db.tbl_IeltsQuestionGroupResult.Where(x => x.Enable == true).Select(x => new { x.Id, x.IeltsSkillId, x.Type, x.TagIds, x.IeltsSectionResultId }).ToListAsync();
                var ieltsQuestionResultDatas = await db.tbl_IeltsQuestionResult.Where(x => x.Enable == true).Select(x => new { x.Id, x.IeltsQuestionGroupResultId, x.Correct, x.Point }).ToListAsync();
                var ieltsExamResultDatas = await db.tbl_IeltsExamResult.Where(x => x.Enable == true).Select(x => new { x.Id, x.TimeSpent, x.ValueId, x.StudentId, x.IeltsExamId }).ToListAsync();
                var ieltsSectionDatas = await db.tbl_IeltsSectionResult.Where(x => x.Enable == true).Select(x => new { x.Id, x.IeltsSkillResultId }).ToListAsync();

                var statisticalTagDatas = new List<StatisticalTagModel>();
                var statisticalTagWrittingAndSpeackingDatas = new List<StatisticalTagWrittingAndSpeackingModel>();

                if (homeworks.Count != 0 && tags.Count != 0)
                {
                    foreach (var t in tags)
                    {
                        // Số liệu cho thống kê theo từng bài tập
                        int totalQuestion = 0;
                        int totalQuestionCorrect = 0;
                        int totalQuestionFail = 0;
                        double percentage = 0;

                        int totalQuestionWrittingAndSpeaking = 0;
                        double totalPointWrittingAndSpeaking = 0;
                        double average = 0;
                        foreach (var h in homeworks)
                        {
                            var ieltsExamResults = ieltsExamResultDatas.Where(x => x.IeltsExamId == h.IeltsExamId && x.ValueId == h.Id && x.StudentId == baseSearch.StudentId).ToList();
                            if (ieltsExamResults.Count != 0)
                            {
                                foreach (var i in ieltsExamResults)
                                {
                                    var ieltsSkills = ieltsSkillDatas.Where(x => x.IeltsExamResultId == i.Id && x.IeltsExamId == h.IeltsExamId).ToList();
                                    if (ieltsSkills.Count != 0)
                                    {
                                        foreach (var ik in ieltsSkills)
                                        {
                                            var ieltsSections = ieltsSectionDatas.Where(x => x.IeltsSkillResultId == ik.Id).ToList();
                                            foreach (var ils in ieltsSections)
                                            {
                                                if (ieltsQuestionGroupDatas.Count != 0)
                                                {
                                                    // Lấy danh sách các câu hỏi có từ khóa không phải là nói và viết
                                                    var ieltsQuestionGroups = ieltsQuestionGroupDatas.Where(x => x.IeltsSectionResultId == ils.Id && !string.IsNullOrEmpty(x.TagIds) && x.TagIds.Contains(t.Id.ToString()) && x.Type != 7 && x.Type != 8).ToList();
                                                    if (ieltsQuestionGroups.Count != 0)
                                                    {
                                                        foreach (var iq in ieltsQuestionGroups)
                                                        {
                                                            var ieltsQuestionResults = ieltsQuestionResultDatas.Where(x => x.IeltsQuestionGroupResultId == iq.Id).ToList();

                                                            // Tính số liệu thống kê tất cả bài tập
                                                            totalQuestion += ieltsQuestionResults.Count();
                                                            totalQuestionCorrect += ieltsQuestionResults.Count(x => x.Correct == true);
                                                            totalQuestionFail += ieltsQuestionResults.Count(x => x.Correct == false);
                                                        }
                                                    }

                                                    var ieltsQuestionGroupWrittingAndSpeackings = ieltsQuestionGroupDatas.Where(x => x.IeltsSectionResultId == ils.Id && !string.IsNullOrEmpty(x.TagIds) && x.TagIds.Contains(t.Id.ToString()) && (x.Type == 7 || x.Type == 8)).ToList();
                                                    if (ieltsQuestionGroupWrittingAndSpeackings.Count != 0)
                                                    {
                                                        foreach (var iqw in ieltsQuestionGroupWrittingAndSpeackings)
                                                        {
                                                            // Tính số liệu thống kê tất cả bài tập
                                                            var ieltsQuestionResultWrittingAndSpeackings = ieltsQuestionResultDatas.Where(x => x.IeltsQuestionGroupResultId == iqw.Id && x.Point != null).ToList();
                                                            if (ieltsQuestionResultWrittingAndSpeackings.Count != 0)
                                                            {
                                                                // Tính số liệu thống kê tất cả bài tập
                                                                totalQuestionWrittingAndSpeaking += ieltsQuestionResultWrittingAndSpeackings.Count();
                                                                totalPointWrittingAndSpeaking += ieltsQuestionResultWrittingAndSpeackings.Sum(x => x.Point ?? 0);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (totalQuestion != 0 && totalQuestionCorrect != 0)
                        {
                            // Tính số liệu thống kê tất cả bài tập
                            percentage = Math.Round((((double)totalQuestionCorrect / totalQuestion) * 100), 2);
                        }
                        if (totalQuestionWrittingAndSpeaking != 0)
                        {
                            // Tính số liệu thống kê tất cả bài tập
                            average = Math.Round(((double)totalPointWrittingAndSpeaking / totalQuestionWrittingAndSpeaking), 2);
                        }
                        var statisticalTag = new StatisticalTagModel
                        {
                            Name = t.Name,
                            TotalQuestion = totalQuestion,
                            TotalQuestionCorrect = totalQuestionCorrect,
                            TotalQuestionFail = totalQuestionFail,
                            Percentage = percentage
                        };
                        statisticalTagDatas.Add(statisticalTag);
                        var statisticalTagWrittingAndSpeacking = new StatisticalTagWrittingAndSpeackingModel
                        {
                            Name = t.Name,
                            TotalQuestion = totalQuestionWrittingAndSpeaking,
                            Average = average
                        };
                        statisticalTagWrittingAndSpeackingDatas.Add(statisticalTagWrittingAndSpeacking);
                    }
                    var totalDataByTags = new TotalDataByTags();
                    totalDataByTags.TagModel = statisticalTagDatas;
                    totalDataByTags.TagWrittingAndSpeacking = statisticalTagWrittingAndSpeackingDatas;
                    result.ClassId = baseSearch.ClassId;
                    result.StudentId = baseSearch.StudentId;
                    result.TotalDataByTags = totalDataByTags;
                }
                return result;
            }
        }
        #endregion

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------      
        #region thống kê cũ
        /*public static Time GetTime()
        {
            DateTime timeNow = DateTime.Now;
            Time time = new Time();
            time.Month = timeNow.Month;
            time.Year = timeNow.Year;
            time.LastMonth = time.Month - 1 == 0 ? 12 : time.Month - 1;
            time.YearOfLastMonth = time.LastMonth == 12 ? time.Year - 1 : time.Year;
            time.LastYear = timeNow.Year - 1;
            time.Day = timeNow.Day;
            return time;
        }
        public static async Task<List<ListOverviewModel>> GetStatisticalOverview(OverviewFilter search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (search == null)
                {
                    search = new OverviewFilter();
                }
                int? Role = 0;
                if (user.RoleId == (int)RoleEnum.parents)
                {
                    search.UserId = search.UserId;
                    Role = (int)RoleEnum.student;
                }
                else
                {
                    search.UserId = user.UserInformationId;
                    Role = user.RoleId;
                }
                //Time time = GetTime();
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                string sql = $"GetGetStatistical_Overview " +
                        //$"@Month = N'{time.Month}'," +
                        //$"@Year = N'{search.Year}'," +
                        //$"@LastMonth = N'{time.LastMonth}'," +
                        //$"@LastYear = N'{time.LastYear}'," +
                        //$"@Day = N'{time.Day}'," +
                        $"@RoleId = N'{Role}'," +
                        $"@UserId = N'{search.UserId}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                var result = await db.SqlQuery<OverviewModel>(sql);

                string increase = "Tăng ";
                string decrease = "Giảm ";
                string restString = " so với tháng trước";
                double value = 0;
                int tang = 1;
                int giam = 2;
                foreach (var item in result)
                {
                    if (item.PreValue.HasValue)
                    {
                        if (item.Value - item.PreValue == 0)
                        {
                            item.SubValue = null;
                            item.Type = 0;
                        }
                        //nếu tháng này = 0 và tháng trước < 0 thì tháng này tăng 100
                        else if (item.Value == 0 && item.PreValue.Value < 0)
                        {
                            item.SubValue = increase + "100%" + restString;
                            item.Type = tang;
                        }
                        //nếu tháng này = 0 và tháng trước > 0 thì tháng này giảm 100
                        else if (item.Value == 0 && item.PreValue.Value > 0)
                        {
                            item.SubValue = decrease + "100%" + restString;
                            item.Type = giam;
                        }
                        //nếu tháng này > 0 và tháng trước = 0 thì tháng này tăng 100
                        else if (item.Value > 0 && item.PreValue.Value == 0)
                        {
                            item.SubValue = increase + "100%" + restString;
                            item.Type = tang;
                        }
                        //nếu tháng này < 0 và tháng trước = 0 thì tháng này giảm 100
                        else if (item.Value < 0 && item.PreValue.Value == 0)
                        {
                            item.SubValue = decrease + "100%" + restString;
                            item.Type = giam;
                        }
                        else
                        {
                            value = Math.Round(Math.Abs((item.Value / item.PreValue.Value * 100) - 100), 2);
                            if (value > 0)
                            {
                                item.SubValue = increase + value + "%" + restString;
                                item.Type = tang;
                            }
                            else
                            {
                                item.SubValue = decrease + value + "%" + restString;
                                item.Type = giam;
                            }
                        }
                    }

                }

                var temp = result.GroupBy(x => x.Groups).Select(x => new ListOverviewModel
                {
                    Id = x.First().Groups,
                    Title = ListStatisticalOverviewGroups().SingleOrDefault(y => y.Key == x.First().Groups).Value,
                    OverviewModel = x.ToList()
                }).ToList();
                return temp.Where(x => x.OverviewModel.Any()).ToList();
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalAge(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                string sql = $"GetStatistical_Chart_Age " +
                        $"@Year = N'{search.Year}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalFeedbackRating(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                string sql = $"GetStatistical_Chart_FeedbackRating " +
                        $"@Year = N'{search.Year}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalNewClass(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                //string sql = $"GetStatistical_Chart_NewClass " +
                //        $"@Year = N'{search.Year}'," +
                //        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                //return await db.SqlQuery<StatisticalYear>(sql);
                return new List<StatisticalYear>
                {
                    new StatisticalYear { Name = "1", Value = 39, PreValue = 0 },
                    new StatisticalYear { Name = "2", Value = 76, PreValue = 0 },
                    new StatisticalYear { Name = "3", Value = 34, PreValue = 0 },
                    new StatisticalYear { Name = "4", Value = 45, PreValue = 0 },
                    new StatisticalYear { Name = "5", Value = 42, PreValue = 0 },
                    new StatisticalYear { Name = "6", Value = 67, PreValue = 0 },
                    new StatisticalYear { Name = "7", Value = 77, PreValue = 0 },
                    new StatisticalYear { Name = "8", Value = 34, PreValue = 0 },
                    new StatisticalYear { Name = "9", Value = 13, PreValue = 0 },
                    new StatisticalYear { Name = "10", Value = 54, PreValue = 0 },
                    new StatisticalYear { Name = "11", Value = 50, PreValue = 0 },
                    new StatisticalYear { Name = "12", Value = 67, PreValue = 0 },
                };
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalNewCustomer(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                //string sql = $"GetStatistical_Chart_NewCustomer " +
                //        $"@Year = N'{search.Year}'," +
                //        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                //return await db.SqlQuery<StatisticalYear>(sql);
                return new List<StatisticalYear>
                {
                    new StatisticalYear { Name = "1", Value = 139, PreValue = 0 },
                    new StatisticalYear { Name = "2", Value = 76, PreValue = 0 },
                    new StatisticalYear { Name = "3", Value = 134, PreValue = 0 },
                    new StatisticalYear { Name = "4", Value = 15, PreValue = 0 },
                    new StatisticalYear { Name = "5", Value = 342, PreValue = 0 },
                    new StatisticalYear { Name = "6", Value = 467, PreValue = 0 },
                    new StatisticalYear { Name = "7", Value = 177, PreValue = 0 },
                    new StatisticalYear { Name = "8", Value = 234, PreValue = 0 },
                    new StatisticalYear { Name = "9", Value = 313, PreValue = 0 },
                    new StatisticalYear { Name = "10", Value = 154, PreValue = 0 },
                    new StatisticalYear { Name = "11", Value = 150, PreValue = 0 },
                    new StatisticalYear { Name = "12", Value = 267, PreValue = 0 },
                };
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalPayment(ChartSearch search)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                //string sql = $"GetStatistical_Chart_Payment " +
                //        $"@Year = N'{search.Year}'," +
                //        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                //return await db.SqlQuery<StatisticalYear>(sql);
                return new List<StatisticalYear>
                {
                    new StatisticalYear { Name = "1", Value = 450000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "2", Value = 850000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "3", Value = 150000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "4", Value = 340000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "5", Value = 670000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "6", Value = 250000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "7", Value = 1350000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "8", Value = 550000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "9", Value = 750000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "10", Value = 490000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "11", Value = 670000000, PreValue = 645000000 },
                    new StatisticalYear { Name = "12", Value = 890000000, PreValue = 645000000 },
                };
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalTopLearningNeed(ChartSearch search)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                string sql = $"GetStatistical_Chart_TopLearningNeed " +
                        $"@Year = N'{search.Year}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalTopPurpose(ChartSearch search)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                string sql = $"GetStatistical_Chart_TopPurpose " +
                        $"@Year = N'{search.Year}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalTopSource(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                string sql = $"GetStatistical_Chart_TopSource " +
                        $"@Year = N'{search.Year}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }
        }
        public static async Task<List<StatisticalYear>> GetStatisticalTopJob(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                string sql = $"GetStatistical_Chart_TopJob " +
                        $"@Year = N'{search.Year}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalTotalScheduleTeacher(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                string sql = $"GetStatistical_Chart_TotalScheduleTeacher " +
                        $"@Year = N'{search.Year}'," +
                        $"@UserId = N'{user.UserInformationId}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalTotalScheduleStudent(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                string sql = $"GetStatistical_Chart_TotalScheduleStudent " +
                        $"@Year = N'{search.Year}'," +
                        $"@UserId = N'{user.UserInformationId}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalRateTeacher(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                string sql = $"GetStatistical_Chart_RateTeacher " +
                        $"@Year = N'{search.Year}'," +
                        $"@UserId = N'{user.UserInformationId}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }

        }
        public static async Task<List<StatisticalYear>> GetStatisticalNewCustomerOfSalesId(ChartSearch search, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                int? userId = null;
                if (user.RoleId == (int)RoleEnum.sale)
                {
                    userId = user.UserInformationId;
                }
                Time time = GetTime();
                if (search == null)
                {
                    search = new ChartSearch
                    {
                        Year = time.Year,
                    };
                }
                else
                {
                    search.Year = search.Year ?? time.Year;
                }
                if (search.BranchIds != null)
                    search.BranchIds = checkBranchAccess(user, search.BranchIds);
                else
                    search.BranchIds = user.BranchIds;
                string sql = $"GetStatistical_Chart_NewCustomerOfSales " +
                        $"@Year = N'{search.Year}'," +
                        $"@UserId = N'{user.UserInformationId}'," +
                        $"@BranchIds = N'{(search.BranchIds) ?? ""}'";
                return await db.SqlQuery<StatisticalYear>(sql);
            }

        }
        /// <summary>
        /// Thống kê khách hàng hẹn test theo năm theo sales
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static async Task<List<StatisticialTestAppointmentModel>> StatisticialTestAppointment(StatisticialCustomerInYearSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                int? userId = null;
                if (user.RoleId == (int)RoleEnum.sale)
                    userId = user.UserInformationId;
                if (baseSearch == null) baseSearch = new StatisticialCustomerInYearSearch();
                if (baseSearch.BranchIds != null)
                    baseSearch.BranchIds = checkBranchAccess(user, baseSearch.BranchIds);
                else
                    baseSearch.BranchIds = user.BranchIds;
                string sql = $"GetStatistical_Chart_TestAppointment @BranchIds = '{baseSearch.BranchIds}',@Year='{baseSearch.Year}',@UserId='{userId}'";
                var data = await db.SqlQuery<StatisticialTestAppointmentModel>(sql);
                return data;
            }

        }
        public static string checkBranchAccess(tbl_UserInformation user, string searchBranch)
        {
            // Kiểm tra role của user
            // Phân lại chi nhánh đúng với chi nhánh của user
            string result = string.Empty;
            if (user.RoleId != 1)
            {
                string branchAccess = string.Empty;
                string userBranchIDs = "," + user.BranchIds + ",";
                var searchBranchIDs = searchBranch.Split(',');
                foreach (var branchID in searchBranchIDs)
                {
                    if (userBranchIDs.IndexOf("," + branchID + ",") != -1)
                    {
                        branchAccess += branchID + ",";
                    }
                }
                if (branchAccess.Length > 0)
                    result = branchAccess.Remove(branchAccess.Length - 1);
                else
                    result = null;
            }
            else
                result = searchBranch;
            return result;
        }*/
        #endregion
    }
}