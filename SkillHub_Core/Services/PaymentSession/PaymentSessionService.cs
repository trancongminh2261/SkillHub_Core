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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using System.IO;
using ICSharpCode.SharpZipLib.Core;

namespace LMSCore.Services.PaymentSession
{
    public class PaymentSessionService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public PaymentSessionService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public static async Task<tbl_PaymentSession> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_PaymentSession.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                {
                    var user = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.UserId);
                    data.FullName = user?.FullName;
                    data.UserCode = user?.UserCode;
                    data.UserEmail = user?.Email;
                    data.UserPhone = user?.Mobile;
                }
                return data;
            }
        }
        public static async Task<tbl_PaymentSession> Insert(PaymentSessionCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                var model = new tbl_PaymentSession(itemModel);
                if (model.Type == 1 && (model.UserId == null || model.UserId == 0))
                    throw new Exception("Vui lòng chọn học viên cho phiếu thu!");
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_PaymentSession.Add(model);
                model.PrintContent = Task.Run(() => GetPrintContent(
                    model.Type ?? 0,
                    model.UserId ?? 0,
                    model.Reason,
                    model.Value,
                    user.FullName
                    )).Result;
                await db.SaveChangesAsync();

                //var entity = await db.tbl_PaymentSession.SingleOrDefaultAsync(x => x.Id == model.Id);
                //if (entity == null)
                //    throw new Exception("Không tìm thấy dữ liệu để câp nhật QRCoder");

                //var URLQRCoder = myuri + "/?paymentID=" + model.Id + "&Name=" + model.TypeName;
                //entity.QRCoder = AssetCRM.CreateQRCode(URLQRCoder, AssetCRM.RemoveUnicode(model.Id + "_" + model.TypeName));
                ////db.tbl_PaymentSession.Add(model);

                //await db.SaveChangesAsync();
                return model;
            }
        }

        public static async Task<tbl_PaymentSession> InsertV2(PaymentSessionCreateV2 itemModel, tbl_UserInformation user, string baseUrl)
        {
            using (var db = new lmsDbContext())
            {
                // Thông tin URL gắn vào QRCode
                UrlNotificationModels urlNotification = new UrlNotificationModels();

                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                var model = new tbl_PaymentSession(itemModel);
                if (model.Type == 1 && (model.UserId == null || model.UserId == 0))
                    throw new Exception("Vui lòng chọn học viên cho phiếu thu!");
                if (model.Type == 2 && model.SpendingConfigId == null)
                    throw new Exception("Vui lòng chọn loại phiếu chi!");
                if (model.SpendingConfigId != null)
                {
                    var spendingConfig = await db.tbl_SpendingConfig.SingleOrDefaultAsync(x => x.Id == itemModel.SpendingConfigId)
                      ?? throw new Exception("Loại phiếu chi không tồn tại!");
                    if (!spendingConfig.Active)
                        throw new Exception("Loại phiếu chi chưa được kích hoạt!");
                }
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_PaymentSession.Add(model);
                model.PrintContent = Task.Run(() => GetPrintContent(
                    model.Type ?? 0,
                    model.UserId ?? 0,
                    model.Reason,
                    model.Value,
                    user.FullName
                    )).Result;
                await db.SaveChangesAsync();

                // Chuẩn bị thông tin và tạo thời gian gắn vào tên QRCode để không bị trùng tên file
                var userInfor = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == model.UserId);
                if (userInfor != null)
                {
                    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string nameFile = "ThuChi_" + userInfor.FullName.Replace(" ", "");
                    string folderToSave = "QRCodePaymentSession";
                    string name = $"{nameFile}_{timestamp}";
                    string host = urlNotification.url;
                    string data = host + urlNotification.urlPayMentSessionDetail + model.Id;
                    // Mặc định lưu ảnh với định dạng jpg
                    var QRCode = AssetCRM.CreateQRCodeV2(data, name, folderToSave, baseUrl);

                    // Lưu QRCode vào PaymentSession vừa tạo ở trên
                    var paymentSessionEntity = await db.tbl_PaymentSession.FirstOrDefaultAsync(x => x.Id == model.Id);
                    paymentSessionEntity.QRCode = QRCode;
                }
                await db.SaveChangesAsync();

                //var entity = await db.tbl_PaymentSession.SingleOrDefaultAsync(x => x.Id == model.Id);
                //if (entity == null)
                //    throw new Exception("Không tìm thấy dữ liệu để câp nhật QRCoder");

                //var URLQRCoder = myuri + "/?paymentID=" + model.Id + "&Name=" + model.TypeName;
                //entity.QRCoder = AssetCRM.CreateQRCode(URLQRCoder, AssetCRM.RemoveUnicode(model.Id + "_" + model.TypeName));
                ////db.tbl_PaymentSession.Add(model);

                //await db.SaveChangesAsync();
                return model;
            }
        }

        public static async Task<tbl_PaymentSession> Update(PaymentSessionUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_PaymentSession.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.PrintContent = itemModel.PrintContent ?? entity.PrintContent;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task<tbl_PaymentSession> UpdateV2(PaymentSessionUpdateV2 itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_PaymentSession.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.PrintContent = itemModel.PrintContent ?? entity.PrintContent;
                entity.PaymentDate = itemModel.PaymentDate ?? entity.PaymentDate;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task<string> GetPrintContent(int type, int studentId, string reason, double value, string createBy, string fullName = "", string userCode = "")
        {
            using (var db = new lmsDbContext())
            {
                string result = "";
                int typeTemplate = type + 2;
                var template = await db.tbl_Template.FirstOrDefaultAsync(x => x.Type == typeTemplate && x.Enable == true);
                if (template != null)
                    result = template.Content;

                if (fullName == "" && userCode == "")
                {
                    var student = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == studentId);
                    if (student != null)
                    {
                        fullName = student.FullName;
                        userCode = student.UserCode;
                    }
                }
                result = result.Replace("{HoVaTen}", fullName);
                result = result.Replace("{MaHocVien}", userCode);
                result = result.Replace("{Ngay}", DateTime.Now.Day.ToString());
                result = result.Replace("{Thang}", DateTime.Now.Month.ToString());
                result = result.Replace("{Nam}", DateTime.Now.Year.ToString());
                result = result.Replace("{LyDo}", reason);
                result = result.Replace("{SoTienThu}", string.Format("{0:0,0}", value));
                result = result.Replace("{SoTienChi}", string.Format("{0:0,0}", value));
                result = result.Replace("{NguoiThu}", createBy);
                result = result.Replace("{NguoiChi}", createBy);
                return result;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_PaymentSession.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public class PaymentSessionResult : AppDomainResult
        {
            public double TotalRevenue { get; set; }
            public double TotalIncome { get; set; }
            public double TotalExpense { get; set; }
        }
        public static async Task<PaymentSessionResult> GetAll(PaymentSessionSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new PaymentSessionSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;
                string sql = $"Get_PaymentSession @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Type = {baseSearch.Type ?? 0}," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@BillId = {baseSearch.BillId ?? 0}," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'";

                var data = await db.SqlQuery<Get_PaymentSession>(sql);
                if (!data.Any()) return new PaymentSessionResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var totalRevenue = data[0].TotalRevenue;
                var totalIncome = data[0].TotalIncome;
                var totalExpense = data[0].TotalExpense;
                var totalValue = data[0].TotalValue;
                var result = data.Select(i => new tbl_PaymentSession(i)).ToList();
                return new PaymentSessionResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    TotalRevenue = totalRevenue,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                };
            }
        }

        public static async Task<PaymentSessionResult> GetAllV2(PaymentSessionSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new PaymentSessionSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;
                string sql = $"Get_PaymentSession @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Type = {baseSearch.Type}," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'";

                var data = await db.SqlQuery<Get_PaymentSession>(sql);
                if (!data.Any()) return new PaymentSessionResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var totalRevenue = data[0].TotalRevenue;
                var totalIncome = data[0].TotalIncome;
                var totalExpense = data[0].TotalExpense;
                var totalValue = data[0].TotalValue;
                var result = data.Select(i => new tbl_PaymentSession(i)).ToList();
                return new PaymentSessionResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    TotalRevenue = totalRevenue,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                };
            }
        }

        public static async Task<PaymentSessionResult> GetPaymentSessionByBillId(PaymentSessionOfStudentSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                //string myBranchIds = "";
                //if (user != null)
                //    if (user.RoleId != (int)RoleEnum.admin)
                //        myBranchIds = user.BranchIds;

                var branch = await db.tbl_Branch.AnyAsync(x => x.Id == baseSearch.BranchId);
                if (!branch) throw new Exception("Không tìm thấy trung tâm");
                var bill = await db.tbl_Bill.AnyAsync(x => x.Id == baseSearch.BillId);
                if (!bill) throw new Exception("Không tìm thấy hóa đơn");

                string sql = $"Get_PaymentSession_ByBillId " +
                    $"@BranchId = {baseSearch.BranchId}," +
                    //$"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@BillId = N'{baseSearch.BillId}'";

                var data = await db.SqlQuery<Get_PaymentSession>(sql);
                if (!data.Any()) return new PaymentSessionResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var totalRevenue = data[0].TotalRevenue;
                var totalIncome = data[0].TotalIncome;
                var totalExpense = data[0].TotalExpense;
                var totalValue = data[0].TotalValue;
                var result = data.Select(i => new tbl_PaymentSession(i)).ToList();
                return new PaymentSessionResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    TotalRevenue = totalRevenue,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                };
            }
        }
        //public static async Task<string> ExportPdf(string content, string path, string pathViews, string domain)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        try
        //        {
        //            var link = string.Empty;
        //            if (!string.IsNullOrEmpty(content))
        //            {
        //                var nameFile = Guid.NewGuid() + ".pdf";
        //                string savePath = $"{path}/PaymentSessionPdf/{nameFile}";
        //                link = $"{domain}/Upload/PaymentSessionPdf/{nameFile}";
        //                using (MemoryStream stream = new MemoryStream())
        //                {
        //                    var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
        //                    {
        //                        Path = path
        //                    });
        //                    await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
        //                    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        //                    {
        //                        Headless = true,
        //                        ExecutablePath = browserFetcher.RevisionInfo(BrowserFetcher.DefaultRevision).ExecutablePath
        //                    });
        //                    var page = await browser.NewPageAsync();
        //                    await page.EmulateMediaTypeAsync(MediaType.Screen);
        //                    await page.SetContentAsync(content);
        //                    var pdfContent = await page.PdfStreamAsync(new PdfOptions
        //                    {
        //                        Format = PaperFormat.A4,
        //                        PrintBackground = true
        //                    });
        //                    await pdfContent.CopyToAsync(stream);

        //                    byte[] bytes = stream.ToArray();
        //                    stream.Close();
        //                    await browser.CloseAsync();

        //                    var newStream = new MemoryStream(bytes);

        //                    using (var fileStream = File.Create(savePath))
        //                    {
        //                        newStream.WriteTo(fileStream);
        //                    }
        //                    await db.SaveChangesAsync();
        //                }
        //            }
        //            else
        //            {
        //                throw new Exception("Không tìm thấy hóa đơn");
        //            }
        //            return link;
        //        }
        //        catch (Exception e)
        //        {
        //            throw e;
        //        }

        //    }
        //}

    }
}