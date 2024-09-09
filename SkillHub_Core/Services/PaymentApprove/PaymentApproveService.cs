using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.Class.ClassService;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using LMSCore.Services.Bill;
using Hangfire;

namespace LMSCore.Services.PaymentApprove
{
    public class PaymentApproveService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public PaymentApproveService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public static async Task<tbl_PaymentApprove> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_PaymentApprove.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        /// <summary>
        /// Tránh trường hợp học viên thanh toán quá số tiền nên sẽ có tính năng lưu lại số tiền duyệt và hoàn tiền
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task Approve(int id, int status,string baseUrl, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (user.RoleId != (int)RoleEnum.admin && user.RoleId != (int)RoleEnum.accountant && user.RoleId != (int)RoleEnum.manager)
                    throw new Exception("Không được phép duyệt");
                var entity = await db.tbl_PaymentApprove.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Status = status;
                entity.StatusName = status == 1 ? "Chờ duyệt"
                                    : status == 2 ? "Đã duyệt"
                                    : status == 3 ? "Không duyệt" : "";

                var bill = await db.tbl_Bill.SingleOrDefaultAsync(x => x.Id == entity.BillId);
                if (bill == null)
                    throw new Exception("Không tìm thấy thông tin thanh toán");
                var discount = db.tbl_Discount.FirstOrDefault(x => x.Id == bill.DiscountId);
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == bill.BranchId);
                ParamOnList param = new ParamOnList { Search = entity.BillCode };
                string paramString = JsonConvert.SerializeObject(param);
                if (entity.Status == 2)
                {
                    await BillService.Payment(new BillService.PaymentCreate
                    {
                        Id = entity.BillId ?? 0,
                        Note = entity.Note,
                        Paid = entity.Money,
                        PaymentMethodId = entity.PaymentMethodId,
                        PaymentDate = entity.PaymentDate
                    }, baseUrl, user);

                    BackgroundJob.Schedule(() => PaymentApproveNotification.NotifyPaymentRequestHasBeenApproved(new PaymentApproveNotificationRequest.NotifyPaymentRequestHasBeenApprovedRequest
                    {
                        PaymentApproveId = entity.Id,
                        CurrentUser = user
                    }), TimeSpan.FromSeconds(2));
                }
                else if (entity.Status == 3)
                {
                    BackgroundJob.Schedule(() => PaymentApproveNotification.NotifyPaymentRequestHasBeenCanceled(new PaymentApproveNotificationRequest.NotifyPaymentRequestHasBeenCanceledRequest
                    {
                        PaymentApproveId = entity.Id,
                        CurrentUser = user
                    }), TimeSpan.FromSeconds(2));
                }
                await db.SaveChangesAsync();
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_PaymentApprove.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }

        public static async Task<PaymentApproveResult> GetAllV2(PaymentApproveV2Search baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new PaymentApproveV2Search();
                string myBranchIds = baseSearch.BranchIds ?? "";

                //nếu không phải admin thì chỉ lấy theo branch của người đó
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;

                if (user.RoleId != (int)RoleEnum.admin && user.RoleId != (int)RoleEnum.accountant && user.RoleId != (int)RoleEnum.manager && user.RoleId != (int)RoleEnum.academic)
                    baseSearch.UserId = user.UserInformationId;

                string sql = $"Get_PaymentApprove @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Status = '{baseSearch.Status}'," +
                    $"@UserId = '{baseSearch.UserId}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'";


                var data = await db.SqlQuery<Get_PaymentApprove>(sql);
                if (!data.Any()) return new PaymentApproveResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_PaymentApprove(i)).ToList();
                var totalMoney = data[0].TotalMoney;
                var approved = data[0].Approved;
                var allState = data[0].AllState;
                var opened = data[0].Opened;
                var canceled = data[0].Canceled;

                return new PaymentApproveResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    TotalMoney = totalMoney,
                    AllState = allState,
                    Opened = opened,
                    Approved = approved,
                    Canceled = canceled
                };
            }
        }

        public static async Task<PaymentApproveResult> GetAll(PaymentApproveSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new PaymentApproveSearch();
                string myBranchIds = "";

                //nếu không phải admin thì chỉ lấy theo branch của người đó
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;

                if (user.RoleId != (int)RoleEnum.admin && user.RoleId != (int)RoleEnum.accountant && user.RoleId != (int)RoleEnum.manager)
                    baseSearch.UserId = user.UserInformationId;

                string sql = $"Get_PaymentApprove @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Status = '{baseSearch.Status}'," +
                    $"@UserId = '{baseSearch.UserId}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'";


                var data = await db.SqlQuery<Get_PaymentApprove>(sql);
                if (!data.Any()) return new PaymentApproveResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_PaymentApprove(i)).ToList();
                var totalMoney = data[0].TotalMoney;
                var approved = data[0].Approved;
                var allState = data[0].AllState;
                var opened = data[0].Opened;
                var canceled = data[0].Canceled;

                return new PaymentApproveResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    TotalMoney = totalMoney,
                    AllState = allState,
                    Opened = opened,
                    Approved = approved,
                    Canceled = canceled
                };
            }
        }

        public class PaymentApproveResult : AppDomainResult
        {
            public double TotalMoney { get; set; }
            public int AllState { get; set; }
            public int Opened { get; set; }
            public int Approved { get; set; }
            public int Canceled { get; set; }
        }
    }
}