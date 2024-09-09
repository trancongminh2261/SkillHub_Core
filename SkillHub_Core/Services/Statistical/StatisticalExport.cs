using LMSCore.Areas.Models;
using LMSCore.Models;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services.Statistical
{
    public class StatisticalExport : DomainService
    {
        public StatisticalExport(lmsDbContext dbContext) : base(dbContext) { }

        #region model
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

        #region chuẩn bị data

        #region báo cáo thống kê tài chính

        // - - - báo cáo doanh thu theo từng học viên - - -
        public class RevenueByStudentModel
        {
            /// <summary>
            /// mã nhân viên
            /// </summary>
            public string UserCode { get; set; }
            /// <summary>
            /// họ tên học viên
            /// </summary>
            public string FullName { get; set; }
            /// <summary>
            /// số tiền thanh toán
            /// </summary>
            public string Value { get; set; }
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
            public string PaymentDate { get; set; }
            /// <summary>
            /// người duyệt thanh toán
            /// </summary>
            public string PaymentApprover { get; set; }
        }

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

        public async Task<List<RevenueByStudentModel>> RevenueByStudent(ExportStatisticalSearch baseSearch, tbl_UserInformation userLogin)
        {
            if (baseSearch == null) baseSearch = new ExportStatisticalSearch();
            var time = GetTimeModel(baseSearch.Month, baseSearch.Year);
            var result = new List<RevenueByStudentModel>();
            var branchIds = baseSearch.BranchIds ?? "";
            if (userLogin.RoleId != (int)RoleEnum.admin)
                branchIds = userLogin.BranchIds;
            var branchIdArray = branchIds.Split(',');

            var listStudent = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student && x.LearningStatus > 4).ToListAsync();
            if (listStudent.Count <= 0) return result;
            #region admin, quản lý, account
            if (userLogin.RoleId == (int)RoleEnum.admin || userLogin.RoleId == (int)RoleEnum.manager || userLogin.RoleId == (int)RoleEnum.accountant)
            {
                //listStudent = listStudent.Where(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => x.BranchIds.Split(',').Contains("," + b + ",")))).ToList()
                listStudent = listStudent.Where(x => x.BranchIds != null && (branchIds == "" || branchIds == "0" || branchIdArray.Any(b => ("," + x.BranchIds + ",").Contains("," + b + ",")) == true)).ToList()
;                //type = 1 là phiếu thu
                var listPaymentSession = await dbContext.tbl_PaymentSession.Where(x => x.Enable == true && x.Type == 1 && x.PaymentDate.Value.Month == time.Month && x.PaymentDate.Value.Year == time.Year).ToListAsync();

                foreach (var item in listPaymentSession)
                {
                    var data = new RevenueByStudentModel();
                    var user = listStudent.SingleOrDefault(x => x.UserInformationId == item.UserId);
                    if (user == null)
                        continue;
                    data.FullName = user.FullName;
                    data.UserCode = user.UserCode;
                    data.Value = String.Format("{0:0,0}", item.Value);
                    data.PaymentMethodName = Task.Run(() => GetPaymentMethod(item.PaymentMethodId ?? 0)).Result;
                    data.Reason = item.Reason;
                    data.PaymentDate = item.PaymentDate?.ToString("dd/MM/yyyy");
                    data.PaymentApprover = item.ModifiedBy;
                    result.Add(data);
                }
            }
            #endregion

            return result;
        }

        #endregion

        #endregion       
    }
}
