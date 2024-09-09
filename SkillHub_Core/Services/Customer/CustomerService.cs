using Hangfire;
using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.CustomerDTO;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering.Implementations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
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
using static LMSCore.Services.ExcelExportService;

namespace LMSCore.Services.Customer
{
    public class CustomerService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

        public CustomerService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public static async Task<tbl_Customer> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Customer.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                {
                    var customerStatus = await db.tbl_CustomerStatus.SingleOrDefaultAsync(x => x.Id == data.CustomerStatusId);
                    data.CustomerStatusType = customerStatus.Type;
                }
                return data;
            }
        }
        public static async Task Complete(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var status = await db.tbl_CustomerStatus.FirstOrDefaultAsync(x => x.Type == 3);
                        var data = await db.tbl_Customer.SingleOrDefaultAsync(x => x.Id == id);
                        if (data != null && status != null)
                        {
                            data.CustomerStatusId = status.Id;
                            data.ModifiedBy = userLogin.FullName;
                            data.ModifiedOn = DateTime.Now;
                        }
                        await db.SaveChangesAsync();

                        var customerHistory = new tbl_CustomerHistory
                        {
                            CustomerId = data.Id,
                            CustomerStatusId = status.Id,
                            BranchId = data.BranchId,
                            SaleId = data.SaleId,
                            CreatedBy = userLogin.FullName,
                            CreatedOn = DateTime.Now,
                            ModifiedBy = userLogin.FullName,
                            ModifiedOn = DateTime.Now,
                            Enable = true
                        };
                        db.tbl_CustomerHistory.Add(customerHistory);
                        await db.SaveChangesAsync();
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
        /// <summary>
        /// 1 - Tạo khách hàng 
        /// 2 - Tạo học viên
        /// Chia đều tư vấn viên
        /// </summary>
        /// <returns></returns>
        public static async Task<int> GetSaleRadom(string branchId, int type)
        {
            using (var db = new lmsDbContext())
            {
                var sales = await db.tbl_UserInformation
                    .Where(x => x.RoleId == (int)RoleEnum.sale && x.StatusId == (int)AccountStatus.active && x.Enable == true
                    && !string.IsNullOrEmpty(x.BranchIds))
                    .OrderBy(x => x.UserInformationId)
                    .Select(x => new { x.UserInformationId, x.BranchIds })
                    .ToListAsync();
                if (!sales.Any())
                    return 0;
                List<int> removeIds = new List<int>();
                foreach (var sale in sales)
                {
                    var branchIds = sale.BranchIds.Split(',');
                    if (!branchIds.Contains(branchId))
                        removeIds.Add(sale.UserInformationId);
                }
                sales = sales.Where(x => !removeIds.Contains(x.UserInformationId)).ToList();
                if (!sales.Any())
                    return 0;


                int saleId = 0;
                if (type == 1)
                {
                    var customer = await db.tbl_Customer.Where(x => x.Enable == true).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                    if (customer != null)
                        saleId = customer.SaleId ?? 0;
                }
                else if (type == 2)
                {
                    var student = await db.tbl_UserInformation.Where(x => x.Enable == true)
                        .OrderByDescending(x => x.UserInformationId).FirstOrDefaultAsync();
                    if (student != null)
                        saleId = student.SaleId;
                }
                int? newSale = sales.Where(x => x.UserInformationId > saleId).OrderBy(x => x.UserInformationId).Select(x => x.UserInformationId).FirstOrDefault();
                if (newSale == null || newSale == 0)
                    newSale = sales[0].UserInformationId;
                return newSale.Value;
            }
        }
        public static async Task<tbl_Customer> Insert(CustomerCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_Customer(itemModel);
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == model.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                if (itemModel.DesiredProgram.HasValue && itemModel.DesiredProgram != 0)
                {
                    var program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == itemModel.DesiredProgram);
                    if (program == null)
                    {
                        throw new Exception("Không tìm thấy chương trình học");
                    }
                }
                var status = await db.tbl_CustomerStatus.FirstOrDefaultAsync(x => x.Type == 1);
                if (status == null)
                    throw new Exception("Lỗi, vui lòng liên hệ quản trị viên");//Thêm các trạng thái mặc định Migrations/Configuration/Seed
                if (user.RoleId == (int)RoleEnum.sale)
                    model.SaleId = user.UserInformationId;
                if (!string.IsNullOrEmpty(itemModel.ParentEmail))
                {
                    string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                    if (Regex.IsMatch(itemModel.ParentEmail, emailPattern) == false)
                    {
                        throw new Exception("Email phụ huynh có định dạng không hợp lệ");
                    }
                }
                int saleId = await GetSaleRadom(itemModel.BranchId.ToString(), 1);
                model.CustomerStatusId = status.Id;
                model.SaleId = model.SaleId ?? saleId;
                model.CreatedBy = model.ModifiedBy = user.FullName;
                string baseCode = "KH";
                int count = await db.tbl_Customer.CountAsync(x =>
                            x.CreatedOn.Value.Year == model.CreatedOn.Value.Year
                            && x.CreatedOn.Value.Month == model.CreatedOn.Value.Month
                            && x.CreatedOn.Value.Day == model.CreatedOn.Value.Day);
                model.Code = AssetCRM.InitCode(baseCode, DateTime.Now, count + 1);

                db.tbl_Customer.Add(model);
                await db.SaveChangesAsync();

                var customerId = db.tbl_Customer.FirstOrDefault(x => x.Email.Contains(model.Email)).Id;
                if (model.CustomerStatusId != null)
                {
                    var customerHistory = new tbl_CustomerHistory
                    {
                        CustomerId = customerId,
                        CustomerStatusId = model.CustomerStatusId,
                        BranchId = model.BranchId,
                        SaleId = model.SaleId,
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        Enable = true
                    };
                    db.tbl_CustomerHistory.Add(customerHistory);
                    await db.SaveChangesAsync();
                }
                if (itemModel.SaleId.HasValue)
                {
                    BackgroundJob.Schedule(() => CustomerNotification.NotifySaleNewCustomer(new CustomerNotificationRequest.NotifySaleNewCustomerRequest
                    {
                        CustomerId = model.Id,
                        CurrentUser = user,
                    }), TimeSpan.FromSeconds(2));
                }
                return model;
            }
        }
        public class CheckExistModel
        {
            [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
            public string Mobile { get; set; }
            [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
            public string Email { get; set; }
        }
        /// <summary>
        /// Kiểm tra thông tin khách hàng này đã tồn tại hay chưa
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        public static async Task<bool> CheckExist(CheckExistModel itemModel)
        {
            using (var db = new lmsDbContext())
            {
                string email = "";
                string mobile = "";
                if (!string.IsNullOrEmpty(itemModel.Email))
                    email = itemModel.Email;
                if (!string.IsNullOrEmpty(itemModel.Mobile))
                    mobile = itemModel.Mobile;
                var check = await db.tbl_Customer
                    .AnyAsync(x => (x.Email.ToUpper() == itemModel.Email.ToUpper() || x.Mobile.ToUpper() == itemModel.Mobile.ToUpper()) && x.Enable == true);
                if (check)
                    return true;
                return false;
            }
        }
        public static async Task<tbl_Customer> Update(CustomerUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Customer.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (itemModel.BranchId.HasValue)
                {
                    var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                    if (branch == null)
                        throw new Exception("Không tìm thấy trung tâm");
                }
                if (itemModel.DesiredProgram.HasValue && itemModel.DesiredProgram != 0)
                {
                    var program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == itemModel.DesiredProgram);
                    if (program == null)
                    {
                        throw new Exception("Không tìm thấy chương trình học");
                    }
                }
                if (!string.IsNullOrEmpty(itemModel.ParentEmail))
                {
                    string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                    if (Regex.IsMatch(itemModel.ParentEmail, emailPattern) == false)
                    {
                        throw new Exception("Email phụ huynh có định dạng không hợp lệ");
                    }
                }
                if (itemModel.SaleId.HasValue && itemModel.SaleId != entity.SaleId)
                {
                    BackgroundJob.Schedule(() => CustomerNotification.NotifySaleNewCustomer(new CustomerNotificationRequest.NotifySaleNewCustomerRequest
                    {
                        CustomerId = entity.Id,
                        CurrentUser = user,
                    }), TimeSpan.FromSeconds(2));
                }
                entity.LearningNeedId = itemModel.LearningNeedId ?? entity.LearningNeedId;
                entity.CustomerStatusId = itemModel.CustomerStatusId ?? entity.CustomerStatusId;
                entity.FullName = itemModel.FullName ?? entity.FullName;
                entity.Mobile = itemModel.Mobile ?? entity.Mobile;
                entity.Email = itemModel.Email ?? entity.Email;
                entity.SourceId = itemModel.SourceId ?? entity.SourceId;
                entity.SaleId = itemModel.SaleId ?? entity.SaleId;
                entity.PurposeId = itemModel.PurposeId ?? entity.PurposeId;
                entity.AreaId = itemModel.AreaId ?? entity.AreaId;
                entity.DistrictId = itemModel.DistrictId ?? entity.DistrictId;
                entity.WardId = itemModel.WardId ?? entity.WardId;
                entity.Address = itemModel.Address ?? entity.Address;
                entity.BranchId = itemModel.BranchId ?? entity.BranchId;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                entity.JobId = itemModel.JobId ?? entity.JobId;
                entity.ParentName = itemModel.ParentName ?? entity.ParentName;
                entity.ParentEmail = itemModel.ParentEmail ?? entity.ParentEmail;
                entity.ParentMobile = itemModel.ParentMobile ?? entity.ParentMobile;
                entity.EntryPoint = itemModel.EntryPoint ?? entity.EntryPoint;
                entity.DesiredOutputScore = itemModel.DesiredOutputScore ?? entity.DesiredOutputScore;
                entity.DesiredProgram = itemModel.DesiredProgram ?? entity.DesiredProgram;
                await db.SaveChangesAsync();
                if (entity.CustomerStatusId != itemModel.CustomerStatusId && itemModel.CustomerStatusId.HasValue)
                {
                    entity.CustomerStatusId = itemModel.CustomerStatusId;
                    var customerHistory = new tbl_CustomerHistory
                    {
                        CustomerId = entity.Id,
                        CustomerStatusId = entity.CustomerStatusId,
                        BranchId = entity.BranchId,
                        SaleId = entity.SaleId,
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        Enable = true
                    };
                    db.tbl_CustomerHistory.Add(customerHistory);
                    await db.SaveChangesAsync();
                }
                return entity;
            }
        }
        public class SendMailModel
        {
            /// <summary>
            /// mẫu 1,2
            /// </summary>
            public string Ids { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
        }
        public static async Task SendMail(SendMailModel model)
        {
            using (var db = new lmsDbContext())
            {
                var emails = await db.tbl_Customer
                    .Where(x => model.Ids.Contains(x.Id.ToString()) && x.Enable == true)
                    .Select(x => x.Email)
                    .ToListAsync();
                if (emails.Any())
                {
                    Thread send = new Thread(() =>
                    {
                        foreach (var item in emails)
                        {
                            AssetCRM.SendMail(item, model.Title, model.Content);
                        }
                    });
                    send.Start();
                }

            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Customer.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(CustomerSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CustomerSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;
                int mySaleId = 0;
                if (user.RoleId == (int)RoleEnum.sale)
                    mySaleId = user.UserInformationId;
                string sql = $"Get_Customer @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@Code = N'{baseSearch.Code ?? ""}'," +
                    $"@CustomerStatusIds = N'{baseSearch.CustomerStatusIds ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@MySaleId = N'{mySaleId}'," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType ? 1 : 0)}'";
                var data = await db.SqlQuery<Get_Customer>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Customer(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        /// <summary>
        /// Cái này tui test nha ae, chứ k có dùng 
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<AppDomainResult> GetByMinhDaiCa(CustomerSearch baseSearch, tbl_UserInformation currentUser)
        {
            try
            {
                if (baseSearch == null)
                    baseSearch = new CustomerSearch();
                string select = "select c.Id ";
                string selectTotalItem = "select COUNT(c.Id) ";

                StringBuilder body = new StringBuilder();
                body.Append("from tbl_Customer c ");
                body.Append("where c.Enable = 1 ");

                if (!string.IsNullOrEmpty(baseSearch.Search))
                    body.Append($"and (c.FullName like N'%{baseSearch.Search}%' or c.Code like N'%{baseSearch.Search}%') ");
                if (!string.IsNullOrEmpty(baseSearch.FullName))
                    body.Append($"and (c.FullName like N'%{baseSearch.FullName}%') ");
                if (!string.IsNullOrEmpty(baseSearch.Code))
                    body.Append($"and (c.Code like N'%{baseSearch.Code}%') ");
                if (!string.IsNullOrEmpty(baseSearch.CustomerStatusIds))
                    body.Append($"and (c.CustomerStatusId in (select value from string_split('{baseSearch.CustomerStatusIds}',','))) ");
                if (!string.IsNullOrEmpty(baseSearch.BranchIds))
                    body.Append($"and (c.BranchId in (select value from string_split('{baseSearch.BranchIds}',','))) ");
                if (currentUser.RoleId != (int)RoleEnum.admin)
                    body.Append($"and (c.BranchId in (select value from string_split('{currentUser.BranchIds}',','))) ");
                if (currentUser.RoleId == (int)RoleEnum.sale)
                    body.Append($"and c.SaleId = {currentUser.UserInformationId} ");

                string orderBy = $"order by c.Id desc ";
                string pagination = $"offset (({baseSearch.PageIndex} - 1)*{baseSearch.PageSize}) rows fetch next {baseSearch.PageSize} rows only ";
                string sqlQueryTotalRow = selectTotalItem + body.ToString();
                var totalItem = await DapperQuery<int>(sqlQueryTotalRow);
                string sqlQuery = select + body.ToString() + orderBy + pagination;
                var listIds = await DapperQuery<int>(sqlQuery);
                var data = await GetDetails(listIds);
                return new AppDomainResult { TotalRow = totalItem[0], Data = data };
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<List<tbl_Customer>> GetDetails(List<int> listId)
        {
            StringBuilder sqlQuery = new StringBuilder();
            sqlQuery.Append("select c.*, ");
            sqlQuery.Append("b.Name as BranchName, ");
            sqlQuery.Append("cs.Name as CustomerStatusName, ");
            sqlQuery.Append("u.FullName as SaleName, ");
            sqlQuery.Append("u.UserCode as SaleUserCode, ");
            sqlQuery.Append("u.Avatar as SaleAvatar, ");
            sqlQuery.Append("r.Name as ReasonOutName ");
            sqlQuery.Append("from tbl_Customer c ");
            sqlQuery.Append("left join tbl_Branch b on b.Id = c.BranchId ");
            sqlQuery.Append("left join tbl_CustomerStatus cs on cs.Id = c.CustomerStatusId ");
            sqlQuery.Append("left join tbl_UserInformation u on u.UserInformationId = c.SaleId ");
            sqlQuery.Append("left join tbl_ReasonOut r on r.Id = c.ReasonOutId ");
            sqlQuery.Append($"where c.Id in (select value from string_split('{string.Join(",", listId)}',','))");
            var data = await DapperQuery<tbl_Customer>(sqlQuery.ToString());
            return data;
        }

        public static async Task<AppDomainResult> GetAllV2(CustomerV2Search baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CustomerV2Search();
                if (user.RoleId != (int)RoleEnum.admin)
                    if (string.IsNullOrEmpty(baseSearch.BranchIds))
                        baseSearch.BranchIds = user.BranchIds;

                if (user.RoleId == (int)RoleEnum.sale)
                    baseSearch.SaleId = user.UserInformationId;

                string sql = $"Get_CustomerV2 @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@Code = N'{baseSearch.Code ?? ""}'," +
                    $"@CustomerStatusIds = N'{baseSearch.CustomerStatusIds ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@SaleId = N'{baseSearch.SaleId ?? 0}'," +
                    $"@FromDate = '{baseSearch.FromDate?.ToString("yyyy/MM/dd") ?? ""}'," +
                    $"@ToDate = '{baseSearch.ToDate?.ToString("yyyy/MM/dd") ?? ""}'," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType ? 1 : 0)}'";
                var data = await db.SqlQuery<Get_Customer>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Customer(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        public static async Task ImportData(List<CustomerCreate> itemModels, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!itemModels.Any())
                            throw new Exception("Không tìm thấy dữ liệu");
                        foreach (var item in itemModels)
                        {
                            var model = new tbl_Customer(item);
                            var status = await db.tbl_CustomerStatus.FirstOrDefaultAsync(x => x.Type == 1);
                            if (status == null)
                                throw new Exception("Lỗi, vui lòng liên hệ quản trị viên");//Thêm các trạng thái mặc định Migrations/Configuration/Seed

                            model.CustomerStatusId = status.Id;
                            if (!string.IsNullOrEmpty(user.BranchIds))
                            {
                                model.BranchId = int.Parse(user.BranchIds.Substring(0, 1));
                            }

                            model.CreatedBy = model.ModifiedBy = user.FullName;
                            string baseCode = "KH";
                            int count = await db.tbl_Customer.CountAsync(x =>
                                        x.CreatedOn.Value.Year == model.CreatedOn.Value.Year
                                        && x.CreatedOn.Value.Month == model.CreatedOn.Value.Month
                                        && x.CreatedOn.Value.Day == model.CreatedOn.Value.Day);
                            model.Code = AssetCRM.InitCode(baseCode, DateTime.Now, count + 1);
                            db.tbl_Customer.Add(model);
                            await db.SaveChangesAsync();
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


        public static async Task<tbl_Customer> GetCustomerStatusById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Customer.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                {
                    var customerStatus = await db.tbl_CustomerHistory.Where(x => x.CustomerId == data.Id).OrderByDescending(x => x.ModifiedOn).ToListAsync();
                    if (customerStatus.Count == 0)
                    {
                        data.CustomerHistory = null;
                    }
                    else
                    {
                        foreach (var a in customerStatus)
                        {
                            a.CustomerName = db.tbl_Customer.FirstOrDefault(x => x.Id == a.CustomerId)?.FullName;
                            a.CustomerStatusName = db.tbl_CustomerStatus.FirstOrDefault(x => x.Id == a.CustomerStatusId)?.Name;
                            a.SaleName = db.tbl_UserInformation.FirstOrDefault(x => x.UserInformationId == a.SaleId)?.FullName;
                            a.BranchName = db.tbl_Branch.FirstOrDefault(x => x.Id == a.BranchId)?.Name;

                        }
                        data.CustomerHistory = customerStatus;
                    }
                }
                return data;
            }
        }

        public static async Task<tbl_Customer> UpdateV2(CustomerUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Customer.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (itemModel.BranchId.HasValue)
                {
                    var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                    if (branch == null)
                        throw new Exception("Không tìm thấy trung tâm");
                }
                if (itemModel.DesiredProgram.HasValue && itemModel.DesiredProgram != 0)
                {
                    var program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == itemModel.DesiredProgram);
                    if (program == null)
                    {
                        throw new Exception("Không tìm thấy chương trình học");
                    }
                }
                if (!string.IsNullOrEmpty(itemModel.ParentEmail))
                {
                    string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                    if (Regex.IsMatch(itemModel.ParentEmail, emailPattern) == false)
                    {
                        throw new Exception("Email phụ huynh có định dạng không hợp lệ");
                    }
                }
                UserInfoParam param = new UserInfoParam { UserId = entity.Id };
                string paramString = JsonConvert.SerializeObject(param);

                if (itemModel.SaleId.HasValue && itemModel.SaleId != entity.SaleId)
                {
                    BackgroundJob.Schedule(() => CustomerNotification.NotifySaleNewCustomer(new CustomerNotificationRequest.NotifySaleNewCustomerRequest
                    {
                        CustomerId = entity.Id,
                        CurrentUser = user,
                    }), TimeSpan.FromSeconds(2));
                }

                if (itemModel.CustomerStatusId.HasValue && entity.CustomerStatusId != itemModel.CustomerStatusId && user.UserInformationId != entity.SaleId)
                {
                    BackgroundJob.Schedule(() => CustomerNotification.NotifySaleChangeCustomerStatus(new CustomerNotificationRequest.NotifySaleChangeCustomerStatusRequest
                    {
                        CustomerId = entity.Id,
                        CurrentUser = user,
                    }), TimeSpan.FromSeconds(2));
                }
                if (itemModel.CustomerStatusId.HasValue)
                {
                    //Nếu trạng thái khách hàng là Từ chối học => phải truyền lý do từ chối học
                    var customerStatus = await db.tbl_CustomerStatus.SingleOrDefaultAsync(x => x.Id == itemModel.CustomerStatusId && x.Enable == true);
                    if (customerStatus?.Type == 5) // từ chối học
                    {
                        if (!itemModel.ReasonOutId.HasValue)
                            throw new Exception("Vui lòng chọn lý do từ chối học!");
                        var reasonOut = await db.tbl_ReasonOut.SingleOrDefaultAsync(x => x.Id == itemModel.ReasonOutId && x.Enable == true);
                        if (reasonOut == null)
                            throw new Exception("Không tìm thấy nội dung lý do từ chối học!");
                        entity.ReasonOutId = reasonOut.Id;
                        entity.ReasonOutName = reasonOut.Name;
                    }
                    else
                        entity.ReasonOutId = null;
                }
                entity.LearningNeedId = itemModel.LearningNeedId ?? entity.LearningNeedId;
                entity.FullName = itemModel.FullName ?? entity.FullName;
                entity.Mobile = itemModel.Mobile ?? entity.Mobile;
                entity.Email = itemModel.Email ?? entity.Email;
                entity.SourceId = itemModel.SourceId ?? entity.SourceId;
                entity.SaleId = itemModel.SaleId ?? entity.SaleId;
                entity.PurposeId = itemModel.PurposeId ?? entity.PurposeId;
                entity.AreaId = itemModel.AreaId ?? entity.AreaId;
                entity.DistrictId = itemModel.DistrictId ?? entity.DistrictId;
                entity.WardId = itemModel.WardId ?? entity.WardId;
                entity.JobId = itemModel.JobId ?? entity.JobId;
                entity.Address = itemModel.Address ?? entity.Address;
                entity.BranchId = itemModel.BranchId ?? entity.BranchId;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                entity.JobId = itemModel.JobId ?? entity.JobId;
                entity.ParentName = itemModel.ParentName ?? entity.ParentName;
                entity.ParentEmail = itemModel.ParentEmail ?? entity.ParentEmail;
                entity.ParentMobile = itemModel.ParentMobile ?? entity.ParentMobile;
                entity.EntryPoint = itemModel.EntryPoint ?? entity.EntryPoint;
                entity.DesiredOutputScore = itemModel.DesiredOutputScore ?? entity.DesiredOutputScore;
                entity.DesiredProgram = itemModel.DesiredProgram ?? entity.DesiredProgram;
                if ((itemModel.CustomerStatusId.HasValue && entity.CustomerStatusId != itemModel.CustomerStatusId)
                    || (entity.RescheduledDate != itemModel.RescheduledDate && itemModel.RescheduledDate.HasValue))
                {
                    entity.CustomerStatusId = itemModel.CustomerStatusId ?? entity.CustomerStatusId;
                    entity.RescheduledDate = itemModel.RescheduledDate ?? entity.RescheduledDate;
                    var customerHistory = new tbl_CustomerHistory
                    {
                        CustomerId = entity.Id,
                        CustomerStatusId = entity.CustomerStatusId,
                        BranchId = entity.BranchId,
                        SaleId = entity.SaleId,
                        RescheduledDate = entity.RescheduledDate,
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        Enable = true
                    };
                    db.tbl_CustomerHistory.Add(customerHistory);
                }
                await db.SaveChangesAsync();
                return entity;
            }
        }

        public class CustomerImport
        {
            public string BranchIds { get; set; }
            public List<CustomerCreate> DataImports { get; set; }
        }
        public static async Task ImportDataV2(CustomerImport customerImport, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!customerImport.DataImports.Any() || customerImport.DataImports == null)
                            throw new Exception("Không tìm thấy dữ liệu");

                        int branchId = 0;
                        if (!string.IsNullOrEmpty(customerImport.BranchIds))
                        {
                            branchId = int.Parse(customerImport.BranchIds.Substring(0, 1));
                        }
                        if (user.RoleId != (int)RoleEnum.admin)
                        {
                            if (!string.IsNullOrEmpty(user.BranchIds))
                            {
                                branchId = int.Parse(user.BranchIds.Substring(0, 1));
                            }
                        }
                        foreach (var item in customerImport.DataImports)
                        {
                            var model = new tbl_Customer(item);
                            var status = await db.tbl_CustomerStatus.FirstOrDefaultAsync(x => x.Type == 1);
                            if (status == null)
                                throw new Exception("Lỗi, vui lòng liên hệ quản trị viên");//Thêm các trạng thái mặc định Migrations/Configuration/Seed

                            model.CustomerStatusId = status.Id;
                            model.BranchId = branchId;
                            model.CreatedBy = model.ModifiedBy = user.FullName;
                            string baseCode = "KH";
                            int count = await db.tbl_Customer.CountAsync(x =>
                                        x.CreatedOn.Value.Year == model.CreatedOn.Value.Year
                                        && x.CreatedOn.Value.Month == model.CreatedOn.Value.Month
                                        && x.CreatedOn.Value.Day == model.CreatedOn.Value.Day);
                            model.Code = AssetCRM.InitCode(baseCode, DateTime.Now, count + 1);
                            db.tbl_Customer.Add(model);
                            await db.SaveChangesAsync();
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
        public static async Task<string> ExportCustomer(int branchId, string baseUrl, tbl_UserInformation user)
        {
            string url = "";
            string templateName = "Export_SampleCustomer.xlsx";
            string folderToSave = "SampleExcelCustomer";
            string fileNameToSave = $"MẫuThêmKháchHàngTiềmNăng";
            string convertBranchId = branchId.ToString();

            using (var db = new lmsDbContext())
            {
                try
                {
                    List<ListDropDown> dropDowns = new List<ListDropDown>();
                    var branch = db.tbl_Branch.FirstOrDefault(x => x.Id == branchId);
                    if (branch == null)
                        throw new Exception("Không tìm thấy trung tâm");
                    var learningNeeds = new List<tbl_LearningNeed>();
                    var learningPurposes = new List<tbl_Purpose>();
                    var learningSources = new List<tbl_Source>();
                    var programs = new List<tbl_Program>();
                    learningNeeds = db.tbl_LearningNeed.Where(x => x.Enable == true).ToList();
                    learningPurposes = db.tbl_Purpose.Where(x => x.Enable == true).ToList();
                    learningSources = db.tbl_Source.Where(x => x.Enable == true).ToList();
                    programs = db.tbl_Program.Where(x => x.Enable == true).OrderBy(x => x.Name).ToList();
                    var salers = new List<tbl_UserInformation>();

                    if (user.RoleId == (int)RoleEnum.sale)
                    {
                        salers.Add(user);
                    }
                    else
                    {
                        salers = db.tbl_UserInformation.Where(x => x.RoleId == (int)RoleEnum.sale && x.Enable == true && x.BranchIds.Contains(branchId.ToString())).OrderBy(x => x.FullName).ToList();
                        if (salers.Count == 0)
                        {
                            tbl_UserInformation nonUser = new tbl_UserInformation
                            {
                                UserInformationId = 0,
                                FullName = "Trung tâm không có tư vấn viên",
                            };
                            salers.Add(nonUser);
                        }
                    }
                    if (learningNeeds.Count == 0)
                    {
                        tbl_LearningNeed learningNeed = new tbl_LearningNeed
                        {
                            Name = "Hệ thống chưa có dữ liệu về nhu cầu học"
                        };
                        learningNeeds.Add(learningNeed);
                    }
                    if (learningPurposes.Count == 0)
                    {
                        tbl_Purpose purpose = new tbl_Purpose
                        {
                            Name = "Hệ thống chưa có dữ liệu về mục tiêu học"
                        };
                        learningPurposes.Add(purpose);
                    }
                    if (learningPurposes.Count == 0)
                    {
                        tbl_Source source = new tbl_Source
                        {
                            Name = "Hệ thống chưa có dữ liệu về nguồn khách hàng"
                        };
                        learningSources.Add(source);
                    }
                    if (programs.Count == 0)
                    {
                        tbl_Program program = new tbl_Program
                        {
                            Name = "Hệ thống chưa có dữ liệu về chương trình học"
                        };
                        programs.Add(program);
                    }

                    // Gán dữ liệu
                    var dataLearningNeed = learningNeeds.Select(x => new DropDown()
                    {
                        id = x.Id,
                        name = x.Name,
                    }).ToList();
                    var datalearningPurpose = learningPurposes.Select(x => new DropDown()
                    {
                        id = x.Id,
                        name = x.Name,
                    }).ToList();
                    var datalearningSource = learningSources.Select(x => new DropDown()
                    {
                        id = x.Id,
                        name = x.Name,
                    }).ToList();

                    var dataSaler = salers.Select(x => new DropDown()
                    {
                        id = x.UserInformationId,
                        name = string.IsNullOrEmpty(x.UserCode) ? x.FullName
                        : "[" + x.UserCode + "] - [" + x.FullName + "]"
                    }).ToList();

                    var dataProgram = programs.Select(x => new DropDown()
                    {
                        id = x.Id,
                        name = string.IsNullOrEmpty(x.Code) ? x.Name
                        : "[" + x.Code + "] - [" + x.Name + "]"
                    }).ToList();


                    // Gắn vào từng cột
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "D",
                        dataDropDown = dataLearningNeed,
                    });
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "E",
                        dataDropDown = datalearningPurpose,
                    });
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "F",
                        dataDropDown = datalearningSource,
                    });
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "G",
                        dataDropDown = dataSaler,
                    });
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "M",
                        dataDropDown = dataProgram,
                    });

                    url = ExportTemplate(folderToSave, templateName, fileNameToSave, dropDowns, baseUrl);
                }

                catch (Exception e)
                {
                    throw e;
                }
                return url;
            }
        }

        public static async Task<List<tbl_Customer>> InsertAffterImport(List<CustomerAfterImport> itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var customers = new List<tbl_Customer>();
                        var customerMobile = await db.tbl_Customer.Where(x => x.Enable == true && x.Mobile != null).Select(x => new { x.Mobile, x.BranchId }).ToListAsync();
                        var learningNeed = await db.tbl_LearningNeed.Where(x => x.Enable == true).Select(x => new { x.Id, x.Name }).ToListAsync();
                        var purpose = await db.tbl_Purpose.Where(x => x.Enable == true).Select(x => new { x.Id, x.Name }).ToListAsync();
                        var source = await db.tbl_Source.Where(x => x.Enable == true).Select(x => new { x.Id, x.Name }).ToListAsync();
                        var listSaler = db.tbl_UserInformation.Where(x => x.RoleId == (int)RoleEnum.sale && x.Enable == true).ToList();

                        foreach (var c in itemModel)
                        {
                            var customer = new tbl_Customer();
                            customer.FullName = c.FullName;

                            // Kiểm tra thông tin branch
                            var branch = db.tbl_Branch.FirstOrDefault(x => x.Id == c.BranchId);
                            if (branch != null)
                                customer.BranchId = branch.Id;
                            else throw new Exception("Không tìm thấy trung tâm");

                            if (string.IsNullOrEmpty(c.Email))
                                customer.Email = null;
                            else customer.Email = c.Email;

                            if (string.IsNullOrEmpty(c.Mobile))
                                customer.Mobile = null;
                            else customer.Mobile = c.Mobile;

                            // Check nhu cầu học
                            if (!string.IsNullOrEmpty(c.LearningNeedName))
                            {
                                var checkLearningNeed = learningNeed.FirstOrDefault(x => x.Name.Contains(c.LearningNeedName));
                                if (checkLearningNeed != null)
                                    customer.LearningNeedId = checkLearningNeed.Id;
                                else throw new Exception("Nhu cầu học " + "(" + c.LearningNeedName + ")" + " không tồn tại trong hệ thống");
                            }
                            else
                                customer.LearningNeedId = 0;

                            // Check mục đích học
                            if (!string.IsNullOrEmpty(c.PurposeName))
                            {
                                var checkPurpose = purpose.FirstOrDefault(x => x.Name.Contains(c.PurposeName));
                                if (checkPurpose != null)
                                    customer.PurposeId = checkPurpose.Id;
                                else throw new Exception("Mục tiêu học " + "(" + c.PurposeName + ")" + " không tồn tại trong hệ thống");
                            }
                            else customer.PurposeId = 0;

                            // Check nguồn khách hàng
                            if (!string.IsNullOrEmpty(c.SourceName))
                            {
                                var checkSource = source.FirstOrDefault(x => x.Name.Contains(c.SourceName));
                                if (checkSource != null)
                                    customer.SourceId = checkSource.Id;
                                else throw new Exception("Nguồn khách hàng " + "(" + c.SourceName + ")" + " không tồn tại trong hệ thống");
                            }
                            else customer.SourceId = 0;

                            // Nếu trung tâm có tư vấn viên thì bắt buộc phải chọn tư vấn viên và ngược lại
                            string branchId = branch.Id.ToString();

                            var saleInBranch = listSaler.Where(x => x.BranchIds.Contains(branchId)).ToList();
                            if (saleInBranch.Count == 0)
                            {
                                customer.SaleId = 0;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(c.SaleName))
                                {
                                    var userCodeSplit = c.SaleName.Split('-')[0].Trim();
                                    int startIndex = userCodeSplit.IndexOf('[') + 1;
                                    int endIndex = userCodeSplit.IndexOf(']');
                                    string userCode = userCodeSplit.Substring(startIndex, endIndex - startIndex);
                                    var checkSaler = listSaler.FirstOrDefault(x => x.UserCode.ToUpper() == userCode.ToUpper());
                                    if (checkSaler != null)
                                    {
                                        customer.SaleId = checkSaler.UserInformationId;
                                    }
                                    else throw new Exception("Không tìm thấy tư vấn viên có tên " + c.SaleName + " trong trung tâm " + branch.Name);
                                }
                                else throw new Exception("Vui lòng chọn tư vấn viên trong trung tâm " + branch.Name);
                            }
                            customer.ParentName = c.ParentName;
                            customer.ParentEmail = c.ParentEmail;
                            customer.ParentMobile = c.ParentMobile;
                            customer.EntryPoint = c.EntryPoint;
                            customer.DesiredOutputScore = c.DesiredOutputScore;
                            if (!string.IsNullOrEmpty(c.DesiredProgram))
                            {
                                //string programCode = c.DesiredProgram.Split(new string[] { " - " }, StringSplitOptions.None)[0].Trim();
                                string programCodeSplit = c.DesiredProgram.Split('-')[0].Trim();
                                int startIndexProgram = programCodeSplit.IndexOf('[') + 1;
                                int endIndexProgram = programCodeSplit.IndexOf(']');
                                string programCode = programCodeSplit.Substring(startIndexProgram, endIndexProgram - startIndexProgram);
                                var program = db.tbl_Program.FirstOrDefault(x => x.Code.ToUpper() == programCode.ToUpper() && x.Enable == true);
                                if (program != null)
                                {
                                    customer.DesiredProgram = program.Id;
                                }
                                else throw new Exception("Chương trình học " + c.DesiredProgram + " không có trong hệ thống");
                            }
                            else
                            {
                                customer.DesiredProgram = 0;
                            }
                            var customerStatus = await db.tbl_CustomerStatus.Where(x => x.Enable == true && x.Type == 1).Select(x => new {x.Type, x.Id}).FirstOrDefaultAsync();
                            customer.CustomerStatusId = customerStatus.Id;
                            customer.CreatedBy = customer.ModifiedBy = user.FullName;
                            customer.CreatedOn = DateTime.Now;
                            customer.ModifiedOn = DateTime.Now;
                            customer.Enable = true;
                            string baseCode = "KH";
                            int count = await db.tbl_Customer.CountAsync(x =>
                                        x.CreatedOn.Value.Year == customer.CreatedOn.Value.Year
                                        && x.CreatedOn.Value.Month == customer.CreatedOn.Value.Month
                                        && x.CreatedOn.Value.Day == customer.CreatedOn.Value.Day);
                            customer.Code = AssetCRM.InitCode(baseCode, DateTime.Now, count + 1);

                            customers.Add(customer);
                            await db.tbl_Customer.AddAsync(customer);
                            await db.SaveChangesAsync();

                            // Lưu lịch sử trạng thái khách hàng
                            var customerSaved = db.tbl_Customer.FirstOrDefault(x => x.Id == customer.Id && x.Enable == true);
                            if (customerSaved != null)
                            {
                                if (customer.CustomerStatusId != null)
                                {
                                    var customerHistory = new tbl_CustomerHistory
                                    {
                                        CustomerId = customerSaved.Id,
                                        CustomerStatusId = customer.CustomerStatusId,
                                        BranchId = customer.BranchId,
                                        SaleId = customer.SaleId,
                                        CreatedBy = customer.FullName,
                                        CreatedOn = DateTime.Now,
                                        ModifiedBy = user.FullName,
                                        ModifiedOn = DateTime.Now,
                                        Enable = true
                                    };
                                    await db.tbl_CustomerHistory.AddAsync(customerHistory);
                                }
                            }
                        }
                        await db.SaveChangesAsync();
                        ////thông báo cho tư vấn viên
                        //Thread sendNoti = new Thread(async () =>
                        //{
                        //    foreach (var data in customers)
                        //    {
                        //        UserInfoParam param = new UserInfoParam { UserId = data.Id };
                        //        string paramString = JsonConvert.SerializeObject(param);
                        //        await NotificationService.Send(new tbl_Notification
                        //        {
                        //            Title = "Tư vấn cho khách hàng mới.",
                        //            Content = "Bạn được chỉ định tư vấn cho khách hàng " + data.FullName,
                        //            ContentEmail = "Bạn được chỉ định tư vấn cho khách hàng " + data.FullName,
                        //            UserId = data.SaleId,
                        //            Type = 0,
                        //            ParamString = paramString
                        //        }, user);
                        //    }
                        //});
                        //sendNoti.Start();
                        tran.Commit();
                        return customers;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception(ex.Message);
                    }
                }
            }
        }


        public static async Task<List<CustomerExport>> RepairDataCustomerToExport(CustomerExportSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CustomerExportSearch();
                string sql = $"[Get_CustomerExport] " +
                    $"@Search = N'{baseSearch.Search ?? ""}', " +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@Code = N'{baseSearch.Code ?? ""}'," +
                    $"@CustomerStatusIds = N'{baseSearch.CustomerStatusIds ?? ""}'," +
                    $"@BranchIds = N'{(user.RoleId != (int)RoleEnum.admin ? user.BranchIds : baseSearch.BranchIds ?? "")}'," +
                    $"@SaleId = N'{(user.RoleId == (int)RoleEnum.sale ? user.UserInformationId : baseSearch.SaleId ?? 0)}'," +
                    $"@FromDate = '{baseSearch.FromDate?.ToString("yyyy/MM/dd") ?? ""}'," +
                    $"@ToDate = '{baseSearch.ToDate?.ToString("yyyy/MM/dd") ?? ""}'," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType ? 1 : 0)}'";
                var data = await db.SqlQuery<Get_CustomerExport>(sql);
                if (!data.Any())
                    throw new Exception("Không có dữ liệu!");
                var listCustomer = new List<CustomerExport>();
                foreach (var c in data)
                {
                    var customer = new CustomerExport();
                    customer.Code = c.Code;
                    customer.FullName = c.FullName;
                    customer.Email = c.Email;
                    customer.Mobile = c.Mobile;
                    customer.SaleName = c.SaleName;
                    customer.CustomerStatusName = c.CustomerStatusName;
                    customer.BranchName = c.BranchName;
                    customer.CreatedBy = c.CreatedBy;
                    customer.CreatedOn = c.CreatedOn.Value.ToString("dd/MM/yyyy");
                    customer.ModifiedBy = c.ModifiedBy;
                    customer.ModifiedOn = c.ModifiedOn.Value.ToString("dd/MM/yyyy");
                    listCustomer.Add(customer);
                }
                return listCustomer;
            }
        }


        public class CustomerRescheduledDate
        {
            public int SaleId { get; set; }
            public string SaleName { get; set; }
            public string CustomerName { get; set; }
            public DateTime RescheduledDate { get; set; }
            public string Content { get; set; }
        }
        //public static async Task CheckRescheduledDate()
        //{
        //    try
        //    {
        //        using (var db = new lmsDbContext())
        //        {
        //            DateTime today = DateTime.Now;
        //            var listCustomer = new List<CustomerRescheduledDate>();
        //            var customers = await db.tbl_Customer.Where(x => x.Enable == true && x.RescheduledDate != null).ToListAsync();
        //            UrlNotificationModels urlNotification = new UrlNotificationModels();
        //            string url = urlNotification.urlLeads;
        //            string urlEmail = urlNotification.url + url;

        //            string content = "";
        //            string notificationContent = "";
        //            string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
        //            var appRootPath = WebHostEnvironment.Environment.ContentRootPath;
        //            var pathViews = Path.Combine(appRootPath, "Views");
        //            content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Customer/CustomerRescheduledDate.cshtml");

        //            tbl_UserInformation user = new tbl_UserInformation
        //            {
        //                FullName = "Tự động"
        //            };
        //            foreach (var c in customers)
        //            {
        //                var customer = new CustomerRescheduledDate();
        //                var sale = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.Enable == true && x.UserInformationId == c.SaleId);
        //                var customerStatus = await db.tbl_CustomerStatus.FirstOrDefaultAsync(x => x.Enable == true && x.Id == c.CustomerStatusId);
        //                if (c.RescheduledDate.Value.ToString("dd/MM/yyyy") == today.ToString("dd/MM/yyyy"))
        //                {
        //                    content = content.Replace("{SaleName}", sale?.FullName);
        //                    content = content.Replace("{today}", today.ToString("dd/MM/yyyy"));
        //                    content = content.Replace("{CusName}", c.FullName);
        //                    content = content.Replace("{Status}", customerStatus?.Name);
        //                    content = content.Replace("{RescheduledDate}", c.RescheduledDate.Value.ToString("dd/MM/yyyy HH:mm"));
        //                    content = content.Replace("{ProjectName}", projectName);
        //                    content = content.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");
        //                    notificationContent = @"<div>" + content + @"</div>";
        //                    if (sale != null)
        //                    {
        //                        customer.SaleId = sale.UserInformationId;
        //                        customer.SaleName = sale.FullName;
        //                    }
        //                    else
        //                    {
        //                        customer.SaleId = 0;
        //                        customer.SaleName = sale?.FullName;
        //                    }
        //                    customer.CustomerName = c.FullName;
        //                    customer.RescheduledDate = c.RescheduledDate.Value;
        //                    customer.Content = notificationContent;
        //                    listCustomer.Add(customer);
        //                }
        //            }
        //            if (listCustomer.Count != 0)
        //            {
        //                Thread send = new Thread(async () =>
        //                {
        //                    foreach (var cus in listCustomer)
        //                    {
        //                        if (cus.SaleId == 0)
        //                        {
        //                            tbl_Notification notification = new tbl_Notification();

        //                            notification.Title = "Lịch Hẹn Với Khách Hàng";
        //                            notification.ContentEmail = cus.Content;
        //                            notification.Content = "Bạn có lịch hẹn với khách hàng " + cus.CustomerName + " vào hôm nay, ngày " + cus.RescheduledDate.ToString("dd/MM/yyyy HH:mm") + ". Vui lòng kiểm tra!";
        //                            notification.Type = 1;
        //                            notification.Category = 2;
        //                            notification.Url = url;
        //                            notification.UserId = cus.SaleId;
        //                            await NotificationService.Send(notification, user, true);
        //                        }
        //                    }
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        public static async Task<bool> CustomersForSaler(List<SetCustomerForSaler> itemModel, int branchId, int saleId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!itemModel.Any())
                            return true;

                        // Kiểm tra tư vấn viên có thuộc chi nhánh hay không
                        var saler = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == saleId && x.Enable == true);
                        var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == branchId && x.Enable == true);
                        if (saler == null) throw new Exception("Không tìm thấy tư vấn viên");
                        if (branch == null) throw new Exception("Không tìm thấy trung tâm");
                        if (saler.BranchIds.Contains(branch.Id.ToString()) == false) throw new Exception("Tư vấn viên " + saler.FullName + " không có trong trung tâm " + branch.Name);

                        // Lấy list khách hàng ra trước để dùng
                        var customerList = await db.tbl_Customer.Where(x => x.Enable == true).ToListAsync();

                        foreach (var c in itemModel)
                        {
                            // Kiểm tra khách hàng có tồn tại hay không
                            var customer = customerList.FirstOrDefault(x => x.Id == c.CustomerId);
                            if (customer == null) throw new Exception("Khách hàng có ID = " + c.CustomerId + " không tồn tại");

                            // Kiểm tra khách hàng có cùng trung tâm với tư vấn viên hay không
                            if (!customer.BranchId.HasValue)
                                throw new Exception("Khách hàng " + customer.FullName + " chưa có trung tâm. Vui lòng cập nhật trung tâm cho khách hàng trước!");
                            if (saler.BranchIds.Contains(customer.BranchId.ToString()) == false)
                                throw new Exception("Tư vấn viên " + saler.FullName + " và khách hàng " + customer.FullName + " không ở cùng một trung tâm");

                            // Thay đổi tư vấn viên cho khách hàng
                            customer.SaleId = saleId;
                            customer.ModifiedBy = user.FullName;
                            customer.ModifiedOn = DateTime.Now;
                            await db.SaveChangesAsync();
                        }
                        tran.Commit();
                        BackgroundJob.Schedule(() => CustomerNotification.NotifySaleReceivedTheCustomerList(new CustomerNotificationRequest.NotifySaleReceivedTheCustomerListRequest
                        {
                            SaleId = saler.UserInformationId,
                            Amount = itemModel.Count
                        }), TimeSpan.FromSeconds(2));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception(ex.Message);
                        return false;
                    }
                }
            }
        }

        public static async Task<AppDomainResult> GetCustomerForSaler(CustomerForSalerSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CustomerForSalerSearch();
                if (user.RoleId != (int)RoleEnum.admin)
                    baseSearch.BranchIds = user.BranchIds;
                string sql = $"Get_Customer_For_Saler @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@CustomerIds = N'{baseSearch.CustomerIds ?? ""}'," +
                    $"@SaleIds = N'{baseSearch.SaleIds ?? ""}'," +
                    $"@IsAssign = N'{baseSearch.IsAssign ?? false}'," +
                    $"@LearningNeedIds = N'{baseSearch.LearningNeedIds ?? ""}'," +
                    $"@PurposeIds = N'{baseSearch.PurposeIds ?? ""}'," +
                    $"@SourceIds = N'{baseSearch.SourceIds ?? ""}'";
                var data = await db.SqlQuery<CustomerForSalerDTO>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                return new AppDomainResult { TotalRow = totalRow, Data = data };
            }
        }

    }
}