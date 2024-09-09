using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using Microsoft.AspNetCore;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using LMSCore.DTO.UserInformationDTO;
using LMSCore.Services.Customer;
using LMSCore.DTO.HomeworkFileDTO;

namespace LMSCore.Services
{
    public class UserInformation : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public UserInformation(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task ValidUserName(string userName, int id)
        {
            if (string.IsNullOrEmpty(userName))
                throw new Exception("Vui lòng không bỏ trống tài khoản");
            if (!CoreContants.UserNameFormat(userName))
                throw new Exception("Tài khoản không hợp lệ, vui lòng nhập viết liền không dấu");
            var hasUserName = await dbContext.tbl_UserInformation
                .AnyAsync(x => x.UserInformationId != id && !string.IsNullOrEmpty(x.UserName) && userName.ToUpper() == x.UserName.ToUpper() && x.Enable == true);
            if (hasUserName)
                throw new Exception("Tên đăng nhập này đã được sử dụng");
        }
        public static string ConvertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public static async Task<tbl_UserInformation> GetById(int userinfomationId)
        {
            tbl_UserInformation account = new tbl_UserInformation();
            using (lmsDbContext _db = new lmsDbContext())
            {
                account = await _db.tbl_UserInformation.Where(c => c.UserInformationId == userinfomationId).FirstOrDefaultAsync();
                if (account != null)
                    if (!account.ParentId.HasValue)
                        account.ParentId = 0;
            }
            return account;
        }
        public static tbl_UserInformation GetByUserName(string username)
        {
            tbl_UserInformation account = new tbl_UserInformation();
            using (lmsDbContext _db = new lmsDbContext())
            {
                account = _db.tbl_UserInformation.Where(c => c.UserName.ToUpper() == username.ToUpper()).FirstOrDefault();
            }
            return account;
        }
        public static async Task<tbl_UserInformation> Insert(tbl_UserInformation user, tbl_UserInformation userLogin, bool register, string programIds = "", int ParentOff = 0)
        {
            using (var db = new lmsDbContext())
            {
                DateTime today = DateTime.Now;
                string content = "";
                string notificationContent = "";
                string branchNamesContendEmail = "";
                string branchNamesContend = "";
                //var httpContext = HttpContext.Current;
                //var pathViews = Path.Combine(httpContext.Server.MapPath("~/Views"));
                var pathViews = Path.Combine(Directory.GetCurrentDirectory(), "Views");
                content = System.IO.File.ReadAllText(Path.Combine(pathViews, "Base", "Mail", "UserInformation", "NewUser.cshtml"));
                content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/UserInformation/NewUser.cshtml");
                string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();

                UrlNotificationModels urlNotification = new UrlNotificationModels();
                string url = "";
                if (user.RoleId == (int)RoleEnum.student)
                {
                    url = urlNotification.urlStudent;
                }
                else if (user.RoleId == (int)RoleEnum.parents)
                {
                    url = urlNotification.urlParent;
                }
                else
                {
                    url = urlNotification.urlEmployee;
                }
                string urlEmail = urlNotification.url + url;

                List<int> branchIds = user.BranchIds.Split(',').Select(int.Parse).ToList();
                foreach (var id in branchIds)
                {
                    var data = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    branchNamesContendEmail += $"<li>{data.Name}</li>";
                    if (branchIds.Count > 1) branchNamesContend += data.Name + ", ";
                    else branchNamesContend = data.Name;
                }

                content = content.Replace("{Today}", today.ToString("dd/MM/yyyy"));
                content = content.Replace("{FullName}", user.FullName);
                content = content.Replace("{RoleName}", user.RoleName);
                content = content.Replace("{Email}", user.Email);
                content = content.Replace("{UserName}", user.UserName);
                content = content.Replace("{Password}", user.Password);
                content = content.Replace("{BranchName}", branchNamesContendEmail);
                content = content.Replace("{ProjectName}", projectName);
                content = content.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

                notificationContent = @"<div>" + content + @"</div>";

                ////nếu không phải học viên hoặc phụ huynh thì thông báo cho admin
                //if (register == false && (user.RoleId != (int)RoleEnum.student || user.RoleId != (int)RoleEnum.parents))
                //{
                //    List<tbl_UserInformation> admins = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.admin).ToListAsync();
                //    Thread send = new Thread(async () =>
                //    {
                //        foreach (var ad in admins)
                //        {
                //            tbl_Notification notification = new tbl_Notification();

                //            notification.Title = "Tài Khoản Mới Được Tạo";
                //            notification.Content = userLogin.FullName + " đã tạo tài khoản mới: " + user.FullName + " chức vụ " + user.RoleName + " ở chi nhánh " + branchNamesContend + ". Vui lòng kiểm tra";
                //            notification.ContentEmail = notificationContent;
                //            notification.Type = 1;
                //            notification.Category = 3;
                //            notification.Url = url;
                //            notification.UserId = ad.UserInformationId;
                //            await NotificationService.Send(notification, user, true);
                //        }
                //    });
                //    send.Start();
                //}
                await ValidateUser(user.UserName, user.Email);

                if (userLogin.RoleId == ((int)RoleEnum.sale))
                    user.SaleId = userLogin.UserInformationId;
                else if (user.SaleId == 0)
                    user.SaleId = await CustomerService.GetSaleRadom(user.BranchIds, 2);
                if (register == true)
                    user.Enable = false;
                user.CreatedBy = user.ModifiedBy = userLogin.FullName;
                user.Password = Encryptor.Encrypt(user.Password);
                user.ActiveDate = DateTime.Now;

                string baseCode = user.RoleId == ((int)RoleEnum.admin) ? "QTV"
                    : user.RoleId == ((int)RoleEnum.student) ? "HV"
                    : user.RoleId == ((int)RoleEnum.parents) ? "PH" : "NV";
                int count = await db.tbl_UserInformation.CountAsync(x => ((x.RoleId == user.RoleId && baseCode != "NV")
                            || (x.RoleId != ((int)RoleEnum.admin) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && baseCode == "NV"))
                            && x.CreatedOn.Value.Year == user.CreatedOn.Value.Year
                            && x.CreatedOn.Value.Month == user.CreatedOn.Value.Month
                            && x.CreatedOn.Value.Day == user.CreatedOn.Value.Day);
                user.UserCode = AssetCRM.InitCode(baseCode, user.CreatedOn.Value, count + 1);

                db.tbl_UserInformation.Add(user);
                await db.SaveChangesAsync();
                if (ParentOff != 0)
                {
                    var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == ParentOff);
                    if (student == null)
                        throw new Exception("Không tìm thấy học viên");
                    student.ParentId = user.UserInformationId;
                    await db.SaveChangesAsync();
                }
                if (register == true)
                {
                    await GenerateOTPAndSendMail(user, "Mã Xác Nhận", "");
                }
                //Thêm giáo viên vào chương trình khi tạo
                if ((user.RoleId == (int)RoleEnum.teacher) && !string.IsNullOrEmpty(programIds))
                {
                    var listProgram = programIds.Split(',').ToList();
                    foreach (var item in listProgram)
                    {
                        int teacherId = int.Parse(item);
                        db.tbl_TeacherInProgram.Add(new tbl_TeacherInProgram
                        {
                            CreatedBy = userLogin.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true,
                            ModifiedBy = userLogin.FullName,
                            ModifiedOn = DateTime.Now,
                            ProgramId = teacherId,
                            TeacherId = user.UserInformationId
                        });
                        await db.SaveChangesAsync();
                    }
                }
                if ((user.RoleId == (int)RoleEnum.tutor) && !string.IsNullOrEmpty(programIds))
                {
                    var listProgram = programIds.Split(',').ToList();
                    foreach (var item in listProgram)
                    {
                        int teacherId = int.Parse(item);
                        db.tbl_TutorInProgram.Add(new tbl_TutorInProgram
                        {
                            CreatedBy = userLogin.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true,
                            ModifiedBy = userLogin.FullName,
                            ModifiedOn = DateTime.Now,
                            ProgramId = teacherId,
                            TutorId = user.UserInformationId
                        });
                        await db.SaveChangesAsync();
                    }
                }
                return user;
            }
        }
        public static async Task<bool> GenerateOTPAndSendMail(tbl_UserInformation user, string title, string content)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var OTP = new tbl_OTPHistory();
                    // tạo mã OTP sau 10 phút hủy mã OTP
                    var otps = await db.tbl_OTPHistory.Where(d => d.UserId == user.UserInformationId && d.Enable == true && d.Status == (int)OTPStatus.ChuaXacNhan).ToListAsync();
                    if (otps.Count() >= 1)
                    {
                        foreach (var item in otps)
                        {
                            item.Enable = false;
                        }
                    }

                    OTP.UserId = user.UserInformationId;
                    OTP.OtpValue = AssetCRM.RandomString(6);
                    OTP.Email = user.Email;
                    OTP.IsEmail = true;
                    OTP.ExpiredDate = DateTime.Now.AddMinutes(10);
                    OTP.CreatedOn = DateTime.Now;
                    OTP.Enable = true;
                    OTP.CreatedBy = "Đăng ký";
                    OTP.ModifiedOn = DateTime.Now;
                    var res = db.tbl_OTPHistory.Add(OTP);
                    await db.SaveChangesAsync();
                    // gửi mã OTP qua mail
                    if (String.Empty.Contains(user.Email)) throw new Exception("Can't send mail");
                    // tạo luồng gửi mail
                    //Thread SendMail = new Thread(() => { ThreadSendMail(Tos, CCs, BCCs, oTP); });
                    AssetCRM.SendMail(user.Email, title, $"Mã xác nhận của bạn là : {OTP.OtpValue}");
                    //SendMail.Start();
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Can't send mail");
                }
            }
        }
        public static async Task ValidateUser(string userName, string email)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var checkUser = await db.tbl_UserInformation
                        .Where(x => x.UserName.ToUpper() == userName.ToUpper() && x.Enable == true).AnyAsync();
                    if (checkUser)
                        throw new Exception($"Tên đăng nhập {userName} đã tồn tại");

                    if (!UserNameFormat(userName))
                        throw new Exception($"Tài khoản đăng nhập {userName} không hợp lệ");
                    var checkEmail = await db.tbl_UserInformation
                        .Where(x => x.Email.ToUpper() == email.ToUpper() && x.Enable == true).AnyAsync();
                    if (checkEmail)
                        throw new Exception($"Email {email} đã được đăng kí");
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static bool UserNameFormat(string value)
        {
            string[] arr = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
            "í","ì","ỉ","ĩ","ị",
            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
            "ý","ỳ","ỷ","ỹ","ỵ"," ",};
            foreach (var item in arr)
            {
                if (value.Contains(item))
                    return false;
            }
            return true;
        }
        public static async Task<tbl_UserInformation> Update(tbl_UserInformation user, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                try
                {

                    if (userLogin.RoleId != ((int)RoleEnum.admin) & (user.RoleId == ((int)RoleEnum.admin) || user.RoleId == ((int)RoleEnum.manager)))
                        throw new Exception("Bạn không thể cập nhật thông tin Quản trị viên và Quản lý");
                    var entity = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == user.UserInformationId);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    if (!string.IsNullOrEmpty(user.UserName) && user.UserName.ToUpper() != entity.UserName.ToUpper())
                    {
                        var validate = await db.tbl_UserInformation.Where(x => x.UserName.ToUpper() == user.UserName.ToUpper() && x.Enable == true).AnyAsync();
                        if (validate)
                            throw new Exception("Tên đăng nhập đã tồn tại");
                    }
                    entity.FullName = user.FullName ?? entity.FullName;
                    entity.UserName = user.UserName ?? entity.UserName;
                    entity.Email = user.Email ?? entity.Email;
                    entity.DOB = user.DOB ?? entity.DOB;
                    entity.Gender = user.Gender ?? entity.Gender;
                    entity.BranchIds = user.BranchIds ?? entity.BranchIds;
                    entity.Mobile = user.Mobile ?? entity.Mobile;
                    entity.Address = user.Address ?? entity.Address;
                    if (entity.StatusId == ((int)AccountStatus.inActive) && user.StatusId == ((int)AccountStatus.active))
                        entity.ActiveDate = DateTime.Now;
                    entity.StatusId = user.StatusId ?? entity.StatusId;
                    entity.Avatar = user.Avatar ?? entity.Avatar;
                    entity.AvatarReSize = user.AvatarReSize ?? entity.AvatarReSize;
                    entity.AreaId = user.AreaId ?? entity.AreaId;
                    entity.DistrictId = user.DistrictId ?? entity.DistrictId;
                    entity.WardId = user.WardId ?? entity.WardId;
                    entity.Password = user.Password == null ? entity.Password : Encryptor.Encrypt(user.Password);
                    entity.SourceId = user.SourceId ?? entity.SourceId;
                    entity.LearningNeedId = user.LearningNeedId ?? entity.LearningNeedId;
                    entity.SaleId = user.SaleId == 0 ? entity.SaleId : user.SaleId;
                    entity.PurposeId = user.PurposeId ?? entity.PurposeId;
                    entity.Extension = user.Extension ?? entity.Extension;
                    entity.ParentId = user.ParentId ?? entity.ParentId;
                    entity.ModifiedOn = user.ModifiedOn;
                    entity.ModifiedBy = userLogin.FullName;
                    entity.JobId = user.JobId ?? entity.JobId;
                    entity.IsReceiveMailNotification = user.IsReceiveMailNotification;
                    entity.BankAccountName = user.BankAccountName;
                    entity.BankAccountNumber = user.BankAccountNumber;
                    entity.BankBranch = user.BankBranch;
                    entity.BankName = user.BankName;
                    await db.SaveChangesAsync();
                    return entity;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static async Task Delete(int userInformationId)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var user = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == userInformationId);
                    if (user == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    user.Enable = false;
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public class RoleModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public static async Task<AppDomainResult> GetAll(UserSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) return new AppDomainResult { TotalRow = 0, Data = null };

                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                int saleId = 0;
                if (user.RoleId == ((int)RoleEnum.sale))
                    saleId = user.UserInformationId;
                if (user.RoleId == (int)RoleEnum.parents)
                    myBranchIds = "";

                string sql = $"Get_User @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@UserInformationIds = N'{baseSearch.UserInformationIds ?? ""}'," +
                    $"@FullName = N'{baseSearch.FullName ?? ""}'," +
                    $"@UserCode = N'{baseSearch.UserCode ?? ""}'," +
                    $"@RoleIds = N'{baseSearch.RoleIds ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@MyBranchIds= N'{myBranchIds}'," +
                    $"@SaleId = {saleId}," +
                    $"@Genders = N'{baseSearch.Genders ?? ""}'," +
                    $"@Sort = {baseSearch.Sort}," +
                    $"@ParentIds = '{baseSearch.ParentIds ?? ""}'," +
                    $"@StudentIds = '{baseSearch.StudentIds ?? ""}'," +
                    $"@TeacherIds = '{baseSearch.TeacherIds ?? ""}'," +
                    $"@MyRole = {user.RoleId ?? 0}," +
                    $"@MyId = {user.UserInformationId}," +
                    $"@SortType = {(baseSearch.SortType == false ? 0 : 1)}";
                var data = await db.SqlQuery<Get_UserInformation>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new UserInformationModel(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task ImportData(List<RegisterModel> model, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!model.Any())
                            throw new Exception("Không tìm thấy dữ liệu");
                        foreach (var item in model)
                        {
                            var checkUser = await db.tbl_UserInformation
                                .Where(x => x.UserName.ToUpper() == item.UserName.ToUpper() && x.Enable == true).AnyAsync();
                            if (checkUser)
                                throw new Exception($"Tên đăng nhập {item.UserName} đã tồn tại");

                            if (!UserNameFormat(item.UserName))
                                throw new Exception($"Tài khoản đăng nhập {item.UserName} không hợp lệ");
                            var newUser = new tbl_UserInformation(item);
                            newUser.CreatedBy = newUser.ModifiedBy = user.FullName;

                            if (user.RoleId == ((int)RoleEnum.sale))
                                newUser.SaleId = user.UserInformationId;

                            if (!string.IsNullOrEmpty(user.BranchIds))
                            {
                                newUser.BranchIds = user.BranchIds.Substring(0, 1);
                            }

                            newUser.ActiveDate = DateTime.Now;

                            string baseCode = user.RoleId == ((int)RoleEnum.admin) ? "QTV"
                                : user.RoleId == ((int)RoleEnum.student) ? "HV"
                                : user.RoleId == ((int)RoleEnum.parents) ? "PH" : "NV";
                            int count = await db.tbl_UserInformation.CountAsync(x => ((x.RoleId == user.RoleId && baseCode != "NV")
                                        || (x.RoleId != ((int)RoleEnum.admin) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && baseCode == "NV"))
                                        && x.CreatedOn.Value.Year == user.CreatedOn.Value.Year
                                        && x.CreatedOn.Value.Month == user.CreatedOn.Value.Month
                                        && x.CreatedOn.Value.Day == user.CreatedOn.Value.Day);
                            newUser.UserCode = AssetCRM.InitCode(baseCode, newUser.CreatedOn.Value, count + 1);
                            newUser.SaleId = await CustomerService.GetSaleRadom(newUser.BranchIds.ToString(), 2);
                            db.tbl_UserInformation.Add(newUser);
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
        public class UserImport
        {
            public string BranchIds { get; set; }
            public List<RegisterModel> DataImports { get; set; }
        }
        public static async Task ImportDataV2(UserImport userImport, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!userImport.DataImports.Any() || userImport.DataImports == null)
                            throw new Exception("Không tìm thấy dữ liệu");

                        string branchId = "";
                        if (!string.IsNullOrEmpty(user.BranchIds))
                        {
                            branchId = userImport.BranchIds.Substring(0, 1);
                        }
                        if (user.RoleId != (int)RoleEnum.admin)
                        {
                            if (!string.IsNullOrEmpty(user.BranchIds))
                            {
                                branchId = user.BranchIds.Substring(0, 1);
                            }
                        }
                        foreach (var item in userImport.DataImports)
                        {
                            var checkUser = await db.tbl_UserInformation
                                .Where(x => x.UserName.ToUpper() == item.UserName.ToUpper() && x.Enable == true).AnyAsync();
                            if (checkUser)
                                throw new Exception($"Tên đăng nhập {item.UserName} đã tồn tại");

                            if (!UserNameFormat(item.UserName))
                                throw new Exception($"Tài khoản đăng nhập {item.UserName} không hợp lệ");
                            var newUser = new tbl_UserInformation(item);
                            newUser.CreatedBy = newUser.ModifiedBy = user.FullName;

                            if (user.RoleId == ((int)RoleEnum.sale))
                                newUser.SaleId = user.UserInformationId;

                            newUser.BranchIds = branchId;

                            newUser.ActiveDate = DateTime.Now;

                            string baseCode = user.RoleId == ((int)RoleEnum.admin) ? "QTV"
                                : user.RoleId == ((int)RoleEnum.student) ? "HV"
                                : user.RoleId == ((int)RoleEnum.parents) ? "PH" : "NV";
                            int count = await db.tbl_UserInformation.CountAsync(x => ((x.RoleId == user.RoleId && baseCode != "NV")
                                        || (x.RoleId != ((int)RoleEnum.admin) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && baseCode == "NV"))
                                        && x.CreatedOn.Value.Year == user.CreatedOn.Value.Year
                                        && x.CreatedOn.Value.Month == user.CreatedOn.Value.Month
                                        && x.CreatedOn.Value.Day == user.CreatedOn.Value.Day);
                            newUser.UserCode = AssetCRM.InitCode(baseCode, newUser.CreatedOn.Value, count + 1);
                            newUser.SaleId = await CustomerService.GetSaleRadom(newUser.BranchIds.ToString(), 2);
                            db.tbl_UserInformation.Add(newUser);
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
        public static async Task<bool> Update_OneSignal_DeviceId(string oneSignal_deviceId, tbl_UserInformation userInformation)
        {
            using (var db = new lmsDbContext())
            {
                if (!string.IsNullOrEmpty(oneSignal_deviceId))
                {
                    var user = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == userInformation.UserInformationId);
                    user.OneSignal_DeviceId = oneSignal_deviceId;
                    await db.SaveChangesAsync();
                }
                return true;
            }
        }
        public static async Task AutoInActive()
        {
            using (var db = new lmsDbContext())
            {
                var time = DateTime.Now.AddMonths(-3);
                var users = await db.tbl_UserInformation
                    .Where(x => x.StatusId == ((int)AccountStatus.active) && x.Enable == true && x.ActiveDate < time && x.RoleId == ((int)RoleEnum.student))
                    .Select(x => x.UserInformationId).ToListAsync();
                if (users.Any())
                {
                    foreach (var item in users)
                    {
                        var user = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item);
                        user.StatusId = ((int)AccountStatus.inActive);
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
        public class Get_UserAvailable
        {
            public int UserInformationId { get; set; }
            public string FullName { get; set; }
            public string UserCode { get; set; }
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }
            public DateTime? DOB { get; set; }
        }
        public class UserInformationAvailableSearch
        {
            public int RoleId { get; set; }
            public int? BranchId { get; set; }
        }
        public static async Task<List<Get_UserAvailable>> GetUserAvailable(UserInformationAvailableSearch baseSearch, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new UserInformationAvailableSearch();
                int saleId = 0;
                if (userLog.RoleId == ((int)RoleEnum.sale))
                    saleId = userLog.UserInformationId;
                int parentId = 0;
                if (userLog.RoleId == ((int)RoleEnum.parents))
                    parentId = userLog.UserInformationId;
                var data = new List<Get_UserAvailable>();
                if (userLog.RoleId == ((int)RoleEnum.student) && baseSearch.RoleId == ((int)RoleEnum.student))
                {
                    data.Add(new Get_UserAvailable
                    {
                        RoleId = userLog.RoleId.Value,
                        FullName = userLog.FullName,
                        RoleName = userLog.RoleName,
                        UserCode = userLog.UserCode,
                        UserInformationId = userLog.UserInformationId,
                        Mobile = userLog.Mobile,
                        Email = userLog.Email,
                        DOB = userLog.DOB
                    });
                }
                else
                {
                    string sql = $"Get_UserAvailable @RoleId = {baseSearch.RoleId}, @SaleId = {saleId}, @BranchId = '{baseSearch.BranchId.ToString() ?? ""}', @ParentId = {parentId}";
                    data = await db.SqlQuery<Get_UserAvailable>(sql);
                }
                return data;
            }
        }
        // cập nhập bật/tắt thông báo 
        //public static async Task<tbl_UserInformation> UpdateReceiveMailNotification(int userInfomationId, bool onOrOff)
        //{ 
        //    using (lmsDbContext _db = new lmsDbContext())
        //    {
        //        var account = await _db.tbl_UserInformation.FirstOrDefaultAsync(c => c.UserInformationId == userInfomationId && c.Enable==true);
        //        if (account == null)
        //            throw new Exception("Không tìm thấy thông tin người dùng");
        //        account.isReceiveMailNotification = onOrOff;
        //        account.ModifiedBy = account.FullName;
        //        await _db.SaveChangesAsync();
        //        return account;
        //    }
        //}

        /*chuyển khách hàng sang học viên và tạo lịch test*/
        public static async Task<(tbl_UserInformation, tbl_TestAppointment)> InsertUserAndTestAppointment(
            NewUserCreate userCreate,
            tbl_UserInformation userLogin,
            NewTestAppointmentCreate testAppointmentCreate)
        {
            using (var db = new lmsDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        #region Xử lý chuyển khách hàng sang học viên
                        var user = new tbl_UserInformation(userCreate);
                        if (userCreate.DesiredProgram.HasValue && userCreate.DesiredProgram != 0)
                        {
                            // Kiểm tra chương trình nếu user chọn chương trình muốn học
                            var checkProgram = await db.tbl_Program.AnyAsync(x => x.Id == userCreate.DesiredProgram);
                            if (!checkProgram) throw new Exception("Không tìm chương trình có Id = " + userCreate.IeltsExamId);
                        }
                        await ValidateInputUserInformation(db, userCreate, testAppointmentCreate);
                        var userInfo = await db.tbl_UserInformation.ToListAsync();
                        if (userCreate.SaleId.HasValue && userCreate.SaleId != 0)
                            user.SaleId = userCreate.SaleId.Value;
                        else if (userLogin.RoleId == ((int)RoleEnum.sale))
                            user.SaleId = userLogin.UserInformationId;
                        else if (user.SaleId == 0)
                            user.SaleId = await CustomerService.GetSaleRadom(userCreate.BranchId.ToString(), 2);
                        user.BranchIds = userCreate.BranchId.ToString();
                        user.CreatedBy = user.ModifiedBy = userLogin.FullName;
                        user.Password = Encryptor.Encrypt(user.Password);
                        user.ActiveDate = DateTime.Now;

                        string baseCode = user.RoleId == ((int)RoleEnum.admin) ? "QTV"
                            : user.RoleId == ((int)RoleEnum.student) ? "HV"
                            : user.RoleId == ((int)RoleEnum.parents) ? "PH" : "NV";
                        int count = userInfo.Count(x => ((x.RoleId == user.RoleId && baseCode != "NV")
                                    || (x.RoleId != ((int)RoleEnum.admin) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && baseCode == "NV"))
                                    && x.CreatedOn.Value.Year == user.CreatedOn.Value.Year
                                    && x.CreatedOn.Value.Month == user.CreatedOn.Value.Month
                                    && x.CreatedOn.Value.Day == user.CreatedOn.Value.Day);
                        user.UserCode = AssetCRM.InitCode(baseCode, user.CreatedOn.Value, count + 1);

                        db.tbl_UserInformation.Add(user);
                        await db.SaveChangesAsync();
                        #endregion

                        // Nếu khách hàng có đầy đủ thông tin của phụ huynh (Tên phụ huynh, SĐT, Email)
                        #region Thêm mới phụ huynh của khách hàng
                        if (!string.IsNullOrEmpty(userCreate.ParentName) && !string.IsNullOrEmpty(userCreate.ParentMobile) && !string.IsNullOrEmpty(userCreate.ParentEmail))
                        {
                            var userParent = new tbl_UserInformation();
                            var checkParentEmail = userInfo.FirstOrDefault(x => x.Email != null && x.Email.ToUpper() == userCreate.ParentEmail.ToUpper() && x.Enable == true);
                            if (checkParentEmail != null)
                            {
                                throw new Exception("Email " + userCreate.ParentEmail + " của phụ huynh đã tồn tại");
                            }
                            var checkParentPhone = userInfo.FirstOrDefault(x => x.Mobile != null && x.Mobile.ToUpper() == userCreate.ParentMobile.ToUpper() && x.Enable == true);
                            if (checkParentPhone != null)
                            {
                                throw new Exception("Số điện thoại " + userCreate.ParentMobile + " của phụ huynh đã tồn tại");
                            }
                            userParent.Enable = true;
                            userParent.FullName = userCreate.ParentName;
                            userParent.Mobile = userCreate.ParentMobile;
                            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                            if (Regex.IsMatch(userCreate.ParentEmail, emailPattern) == false)
                            {
                                throw new Exception("Email phụ huynh có định dạng không hợp lệ");
                            }
                            userParent.Email = userCreate.ParentEmail;
                            userParent.RoleId = (int)RoleEnum.parents;
                            userParent.SaleId = user.SaleId;
                            userParent.Gender = 0;
                            userParent.ActiveDate = DateTime.Now;
                            userParent.StatusId = (int)AccountStatus.active;
                            userParent.BranchIds = userCreate.BranchId.ToString();
                            userParent.CreatedOn = userParent.ModifiedOn = DateTime.Now;
                            userParent.CreatedBy = userParent.ModifiedBy = userLogin.FullName;
                            string baseCodeParent = userParent.RoleId == ((int)RoleEnum.admin) ? "QTV"
                            : userParent.RoleId == ((int)RoleEnum.student) ? "HV"
                            : userParent.RoleId == ((int)RoleEnum.parents) ? "PH" : "NV";
                            int countParent = userInfo.Count(x => ((x.RoleId == userParent.RoleId && baseCode != "NV")
                                        || (x.RoleId != ((int)RoleEnum.admin) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && baseCode == "NV"))
                                        && x.CreatedOn.Value.Year == userParent.CreatedOn.Value.Year
                                        && x.CreatedOn.Value.Month == userParent.CreatedOn.Value.Month
                                        && x.CreatedOn.Value.Day == userParent.CreatedOn.Value.Day);
                            userParent.UserCode = AssetCRM.InitCode(baseCodeParent, userParent.CreatedOn.Value, countParent + 1);
                            userParent.UserName = userParent.UserCode;
                            userParent.Password = Encryptor.Encrypt(userParent.UserCode);
                            db.tbl_UserInformation.Add(userParent);
                            await db.SaveChangesAsync();
                        }
                        #endregion

                        #region Xử lý thêm lịch test
                        if (userCreate.IeltsExamId.HasValue)
                        {
                            // Kiểm tra đề thi trước khi lưu vào TestAppointment
                            var checkIeltsExam = await db.tbl_IeltsExam.AnyAsync(x => x.Id == userCreate.IeltsExamId);
                            if (!checkIeltsExam) throw new Exception("Không tìm thấy đề thi có Id = " + userCreate.IeltsExamId);
                        }
                        var testAppointment = new tbl_TestAppointment(testAppointmentCreate);
                        int studentId = user.UserInformationId;
                        testAppointment.CustomerId = userCreate.CustomerId;
                        testAppointment.BranchId = userCreate.BranchId;
                        testAppointment.StudentId = studentId;
                        testAppointment.CreatedBy = testAppointment.ModifiedBy = userLogin.FullName;
                        testAppointment.TeacherId = testAppointment.TeacherId ?? 0;
                        testAppointment.IeltsExamId = userCreate.IeltsExamId ?? 0;
                        db.tbl_TestAppointment.Add(testAppointment);
                        await db.SaveChangesAsync();
                        // thêm lịch sử
                        var learningHistoryService = new LearningHistoryService(db);
                        var branch2 = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == userCreate.BranchId && x.Enable == true);
                        var branchName = branch2 == null ? "" : branch2.Name;
                        await learningHistoryService.Insert(new LearningHistoryCreate
                        {
                            StudentId = studentId,
                            Content = $"Hẹn test tại {branchName} vào lúc {testAppointmentCreate.Time?.ToString("dd/MM/yyyy HH:mm")}"
                        }, user);
                        #endregion

                        #region Lưu thông tin vào báo cáo CSKH cho tư vấn viên
                        if (user.SaleId != null && user.SaleId != 0)
                        {
                            tbl_LeadSwitchTest leadSwitchTest = new tbl_LeadSwitchTest();
                            leadSwitchTest.SaleId = user.SaleId;
                            leadSwitchTest.StudentId = user.UserInformationId;
                            leadSwitchTest.TestAppointmentId = testAppointment.Id;
                            leadSwitchTest.CreatedOn = DateTime.Now;
                            leadSwitchTest.ModifiedOn = DateTime.Now;
                            leadSwitchTest.Enable = true;
                            leadSwitchTest.CreatedBy = leadSwitchTest.ModifiedBy = user.FullName;
                            db.tbl_LeadSwitchTest.Add(leadSwitchTest);
                            await db.SaveChangesAsync();
                        }
                        #endregion
                        // Hoàn thành transaction
                        transaction.Commit();
                        return (user, testAppointment);
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi xảy ra, hủy bỏ transaction
                        transaction.Rollback();

                        // Xử lý lỗi hoặc ném lại lỗi để bên ngoài xử lý
                        throw ex;
                    }
                }
            }
        }
        //validate input trước khi tạo lịch test
        public static async Task ValidateInputUserInformation(lmsDbContext db, NewUserCreate userCreate, NewTestAppointmentCreate testAppointmentCreate)
        {
            try
            {
                //Kiểm tra tên đăng nhập
                var checkUser = await db.tbl_UserInformation
                    .Where(x => x.UserName.ToUpper() == userCreate.UserName.ToUpper() && x.Enable == true).AnyAsync();
                if (checkUser)
                    throw new Exception($"Tên đăng nhập {userCreate.UserName} đã tồn tại");
                if (!UserNameFormat(userCreate.UserName))
                    throw new Exception($"Tài khoản đăng nhập {userCreate.UserName} không hợp lệ");

                //Kiểm tra mail
                var checkEmail = await db.tbl_UserInformation
                    .Where(x => x.Email.ToUpper() == userCreate.Email.ToUpper() && x.Enable == true).AnyAsync();
                if (checkEmail)
                    throw new Exception($"Email {userCreate.Email} đã được đăng kí");

                //Kiểm tra trung tâm
                var branch = await db.tbl_Branch.AnyAsync(x => x.Id == userCreate.BranchId && x.Enable == true);
                if (!branch)
                    throw new Exception("Không tìm thấy trung tâm");

                //Kiểm tra tỉnh thành phố
                if (userCreate.AreaId.HasValue)
                {
                    var area = await db.tbl_Area.AnyAsync(x => x.Id == userCreate.AreaId && x.Enable == true);
                    if (!area)
                        throw new Exception("Không tìm thấy tỉnh / thành phố");
                }

                //Kiểm tra quận huyện
                if (userCreate.DistrictId.HasValue)
                {
                    var district = await db.tbl_District.AnyAsync(x => x.Id == userCreate.DistrictId && x.Enable == true && x.AreaId == userCreate.AreaId);
                    if (!district)
                        throw new Exception("Quận / huyện này không thuộc tỉnh / thành phố bạn chọn");
                }

                //Kiểm tra phường xã
                if (userCreate.WardId.HasValue)
                {
                    var ward = await db.tbl_Ward.AnyAsync(x => x.Id == userCreate.WardId && x.Enable == true && x.DistrictId == userCreate.DistrictId);
                    if (!ward)
                        throw new Exception("Phường / xã này không thuộc quận / huyện mà bạn chọn");
                }

                //Kiểm tra nguồn học
                if (userCreate.SourceId.HasValue)
                {
                    var source = await db.tbl_Source.AnyAsync(x => x.Id == userCreate.SourceId && x.Enable == true);
                    if (!source)
                        throw new Exception("Không tìm thấy nguồn");
                }

                //Kiểm tra nhu cầu học
                if (userCreate.LearningNeedId.HasValue)
                {
                    var learningneed = await db.tbl_LearningNeed.AnyAsync(x => x.Id == userCreate.LearningNeedId && x.Enable == true);
                    if (!learningneed)
                        throw new Exception("Không tìm thấy nhu cầu học");
                }

                //Kiểm tra tư vấn viên
                if (userCreate.SaleId.HasValue)
                {
                    var sale = await db.tbl_UserInformation.AnyAsync(x => x.UserInformationId == userCreate.SaleId && x.Enable == true);
                    if (!sale)
                        throw new Exception("Không tìm thấy tư vấn viên");
                }

                //Kiểm tra mục đích học
                if (userCreate.PurposeId.HasValue)
                {
                    var purpose = await db.tbl_Purpose.AnyAsync(x => x.Id == userCreate.PurposeId && x.Enable == true);
                    if (!purpose)
                        throw new Exception("Không tìm thấy mục đích học");
                }

                //Kiểm tra khách hàng
                if (userCreate.CustomerId.HasValue && userCreate.CustomerId != 0)
                {
                    var customer = await db.tbl_Customer.AnyAsync(x => x.Id == userCreate.CustomerId && x.Enable == true);
                    if (!customer)
                        throw new Exception("Không tìm thấy thông tin khách hàng");
                }

                //Kiểm tra giáo viên
                if (testAppointmentCreate.TeacherId.HasValue && testAppointmentCreate.TeacherId != 0)
                {
                    var teacher = await db.tbl_UserInformation.AnyAsync(x => x.UserInformationId == testAppointmentCreate.TeacherId && x.Enable == true && x.BranchIds.Contains(userCreate.BranchId.ToString()));
                    if (!teacher)
                        throw new Exception("Không tìm thấy giảng viên");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static async Task<List<string>> GetTutorNames(List<int> ids)
        {
            using (var db = new lmsDbContext())
            {
                return await db.tbl_UserInformation.Where(x => x.Enable == true && ids.Contains(x.UserInformationId)).Select(x => x.FullName).ToListAsync();
            }
        }

        public static async Task<tbl_UserInformation> InsertParent(tbl_UserInformation user, tbl_UserInformation userLogin, bool register, string programIds = "", int userInformationId = 0)
        {
            using (var db = new lmsDbContext())
            {
                ////nếu không phải học viên hoặc phụ huynh thì thông báo cho admin
                //if (register == false && (user.RoleId != (int)RoleEnum.student || user.RoleId != (int)RoleEnum.parents))
                //{
                //    Thread sendTeacher = new Thread(async () =>
                //    {
                //        List<tbl_UserInformation> admins = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.admin).ToListAsync();

                //        tbl_Notification notification = new tbl_Notification();

                //        notification.Title = "Tài khoản mới được tạo";
                //        notification.Content = userLogin.FullName + " đã tạo tài khoản mới. Vui lòng kiểm tra";
                //        notification.Type = 1;
                //        foreach (var ad in admins)
                //        {
                //            notification.UserId = ad.UserInformationId;
                //            await NotificationService.Send(notification, user, false);
                //        }
                //    });
                //    sendTeacher.Start();
                //}
                await ValidateUser(user.UserName, user.Email);
                var checkStudent = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == userInformationId);
                if (checkStudent == null) throw new Exception("Không tìm thấy thông tin của của học sinh");
                if (userLogin.RoleId == ((int)RoleEnum.sale))
                    user.SaleId = userLogin.UserInformationId;
                else if (user.SaleId == 0)
                    user.SaleId = await CustomerService.GetSaleRadom(user.BranchIds, 2);
                if (register == true)
                    user.Enable = false;
                user.CreatedBy = user.ModifiedBy = userLogin.FullName;
                user.Password = Encryptor.Encrypt(user.Password);
                user.ActiveDate = DateTime.Now;

                string baseCode = user.RoleId == ((int)RoleEnum.admin) ? "QTV"
                    : user.RoleId == ((int)RoleEnum.student) ? "HV"
                    : user.RoleId == ((int)RoleEnum.parents) ? "PH" : "NV";
                int count = await db.tbl_UserInformation.CountAsync(x => ((x.RoleId == user.RoleId && baseCode != "NV")
                            || (x.RoleId != ((int)RoleEnum.admin) && x.RoleId != ((int)RoleEnum.student) && x.RoleId != ((int)RoleEnum.parents) && baseCode == "NV"))
                            && x.CreatedOn.Value.Year == user.CreatedOn.Value.Year
                            && x.CreatedOn.Value.Month == user.CreatedOn.Value.Month
                            && x.CreatedOn.Value.Day == user.CreatedOn.Value.Day);
                user.UserCode = AssetCRM.InitCode(baseCode, user.CreatedOn.Value, count + 1);
                user.BranchIds = checkStudent.BranchIds;

                db.tbl_UserInformation.Add(user);
                await db.SaveChangesAsync();

                using (lmsDbContext _db = new lmsDbContext())
                {
                    var student = await _db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == userInformationId);
                    if (student == null) throw new Exception("Không tìm thấy thông tin của của học sinh");
                    if (student != null)
                    {
                        if (student.ParentId == null || student.ParentId == 0)
                        {
                            student.ParentId = user.UserInformationId;
                            await _db.SaveChangesAsync();
                        }
                        else if (student.ParentId != user.UserInformationId)
                        {
                            student.ParentId = user.UserInformationId;
                            await _db.SaveChangesAsync();
                        }
                    }
                }
                return user;
            }
        }


        public static async Task<Get_UserInformation> GetParentById(int userinfomationId)
        {
            tbl_UserInformation account = new tbl_UserInformation();
            using (lmsDbContext _db = new lmsDbContext())
            {
                account = await _db.tbl_UserInformation.Where(c => c.UserInformationId == userinfomationId).FirstOrDefaultAsync();
                if (account != null)
                {
                    if (!account.ParentId.HasValue)
                    {
                        throw new Exception("Không tìm thấy thông tin phụ huynh của học sinh");
                    }
                }
                else
                {
                    throw new Exception("Không tìm thấy thông tin của của học sinh");
                }

                // Tìm thông tin của phụ huynh
                string sql = $"Get_ParentByStudentId " +
                    $"@ParentIds = '{account.ParentId}'";
                var data = await _db.SqlQuery<Get_UserInformation>(sql);
                return data[0];
            }
        }

        public class NotificationToUser
        {
            public int Id;
            public string Name;
            public string ContenEmail = "";
            public string sDate;
            public string eDate;
        }

        //Tự động chúc mừng sinh nhật
        //public static async Task AutomatedBirthdayWishes()
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        DateTime birthday = DateTime.Now;
        //        List<NotificationToUser> notificationStudentBirthdays = new List<NotificationToUser>();
        //        tbl_UserInformation user = new tbl_UserInformation
        //        {
        //            FullName = "Tự động"
        //        };
        //        string content = "";
        //        bool success = false;
        //        string notificationContentToStudent = "";
        //        string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
        //        var appRootPath = WebHostEnvironment.Environment.ContentRootPath;
        //        var pathViews = Path.Combine(appRootPath, "Views");
        //        content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/UserInformation/HappyBirthday.cshtml");

        //        var studentInformation = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.student && x.DOB != null).ToListAsync();

        //        if (studentInformation != null && studentInformation.Count != 0)
        //        {
        //            foreach (var st in studentInformation)
        //            {
        //                if (st.DOB.Value.ToString("dd/MM") == birthday.ToString("dd/MM"))
        //                {
        //                    var notificationBirthday = new NotificationToUser();
        //                    content = content.Replace("{item1}", st.FullName);
        //                    content = content.Replace("{item2}", projectName);
        //                    notificationContentToStudent = @"<div>" + content + @"</div>";
        //                    notificationBirthday.Id = st.UserInformationId;
        //                    notificationBirthday.Name = st.FullName;
        //                    notificationBirthday.ContenEmail = notificationContentToStudent;
        //                    notificationStudentBirthdays.Add(notificationBirthday);
        //                    success = true;
        //                }
        //            }
        //            //Gửi cho học sinh
        //            if (success == true)
        //            {
        //                Thread sendStudent = new Thread(async () =>
        //                {
        //                    foreach (var notiStudent in notificationStudentBirthdays)
        //                    {
        //                        tbl_Notification notification = new tbl_Notification();

        //                        notification.Title = "CHÚC MỪNG SINH NHẬT";
        //                        notification.ContentEmail = notiStudent.ContenEmail;
        //                        notification.Content = "Chúc mừng sinh nhật " + notiStudent.Name + ". Chúc bạn có một ngày sinh nhật vui vẻ và hạnh phúc!";
        //                        notification.Type = 1;
        //                        notification.Category = 0;
        //                        notification.UserId = notiStudent.Id;
        //                        await NotificationService.Send(notification, user, true);
        //                    }
        //                });
        //                sendStudent.Start();
        //            }
        //        }
        //    }
        //}
        ////Thông báo ngày nghỉ
        //public static async Task AutoNotifyHoliday()
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        DateTime today = DateTime.Now;
        //        List<NotificationToUser> notificationToUsers = new List<NotificationToUser>();
        //        tbl_UserInformation user = new tbl_UserInformation
        //        {
        //            FullName = "Tự động"
        //        };
        //        string content = "";
        //        string notificationContent = "";
        //        bool success = false;
        //        string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
        //        var appRootPath = WebHostEnvironment.Environment.ContentRootPath;
        //        var pathViews = Path.Combine(appRootPath, "Views");
        //        content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/UserInformation/DayOff.cshtml");

        //        var studentInformation = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.student).ToListAsync();
        //        var parentInformation = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.parents).ToListAsync();

        //        var dayOffs = await db.tbl_DayOff.Where(x => x.Enable == true).ToListAsync();
        //        foreach (var dayOff in dayOffs)
        //        {
        //            TimeSpan countDownConvert = (TimeSpan)(dayOff.sDate.Value.Date - today.Date);
        //            int countDownDayOff = countDownConvert.Days;
        //            if (countDownDayOff == 7)
        //            {
        //                var notification = new NotificationToUser();
        //                TimeSpan dayOffConvert = (TimeSpan)(dayOff.eDate.Value.Date - dayOff.sDate.Value.Date);
        //                int countDayOff = dayOffConvert.Days;
        //                content = content.Replace("{Name}", dayOff.Name);
        //                content = content.Replace("{Count}", countDayOff.ToString() + " ngày");
        //                content = content.Replace("{sDate}", dayOff.sDate.Value.ToString("dd/MM/yyyy"));
        //                content = content.Replace("{eDate}", dayOff.eDate.Value.ToString("dd/MM/yyyy"));
        //                content = content.Replace("{ProjectName}", projectName);
        //                notificationContent = @"<div>" + content + @"</div>";
        //                notification.Name = dayOff.Name;
        //                notification.sDate = dayOff.sDate.Value.ToString("dd/MM/yyyy");
        //                notification.eDate = dayOff.eDate.Value.ToString("dd/MM/yyyy");
        //                notification.ContenEmail = notificationContent;
        //                notificationToUsers.Add(notification);
        //                success = true;
        //            }
        //        }

        //        if (success == true)
        //        {
        //            // Gửi cho học sinh
        //            Thread sendStudent = new Thread(async () =>
        //            {
        //                foreach (var notiStudent in notificationToUsers)
        //                {
        //                    tbl_Notification notification = new tbl_Notification();

        //                    notification.Title = "Thông Báo Ngày Nghỉ";
        //                    notification.ContentEmail = notiStudent.ContenEmail;
        //                    notification.Content = "Lịch nghỉ sẽ diễn ra vào ngày " + notiStudent.sDate + " đến hết ngày " + notiStudent.eDate + ". Chúc bạn có một kì nghỉ hạnh phúc bên gia đình và người thân!";
        //                    notification.Type = 4;
        //                    notification.Category = 0;
        //                    foreach (var student in studentInformation)
        //                    {
        //                        notification.UserId = student.UserInformationId;
        //                        await NotificationService.Send(notification, user, true);
        //                    }

        //                }
        //            });
        //            sendStudent.Start();

        //            // Gửi cho phụ huynh
        //            Thread sendTeacher = new Thread(async () =>
        //            {
        //                foreach (var notiParent in notificationToUsers)
        //                {
        //                    tbl_Notification notification = new tbl_Notification();

        //                    notification.Title = "Thông Báo Ngày Nghỉ";
        //                    notification.ContentEmail = notiParent.ContenEmail;
        //                    notification.Content = "Lịch nghỉ sẽ diễn ra vào ngày " + notiParent.sDate + " đến hết ngày " + notiParent.eDate + ". Chúc bạn có một kì nghỉ hạnh phúc bên gia đình và người thân!";
        //                    notification.Type = 4;
        //                    notification.Category = 0;
        //                    foreach (var parent in parentInformation)
        //                    {
        //                        notification.UserId = parent.UserInformationId;
        //                        await NotificationService.Send(notification, user, true);
        //                    }
        //                }
        //            });
        //            sendTeacher.Start();
        //        }
        //    }
        //}

        public static async Task<List<UserExport>> PrepareDataToExport(UserExportSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new UserExportSearch();
                string branchIds = baseSearch.BranchIds ?? "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    branchIds = user.BranchIds;
                int saleId = 0;
                if (user.RoleId == ((int)RoleEnum.sale))
                    saleId = user.UserInformationId;
                string sql = $"Get_UserExport " +
                    $"@RoleIds = N'{baseSearch.RoleIds ?? ""}'," +
                    $"@BranchIds = N'{branchIds}'," +
                    $"@SaleId = {saleId}";
                var data = await db.Set<UserExport>().FromSqlRaw(sql).ToListAsync();
                if (!data.Any()) return new List<UserExport>();
                return data;
            }
        }

        // Xuất file mẫu học sinh để import
        public static async Task<string> ExportExampleStudent(int branchId, string baseUrl, tbl_UserInformation user)
        {
            string url = "";
            string templateName = "MauThemHocVien.xlsx";
            string folderToSave = "SampleExcelStudent";
            string fileNameToSave = $"MẫuThêmHọcSinh";
            string convertBranchId = branchId.ToString();

            using (var db = new lmsDbContext())
            {
                try
                {
                    List<ListDropDown> dropDowns = new List<ListDropDown>();
                    var learningNeeds = new List<tbl_LearningNeed>();
                    var learningPurposes = new List<tbl_Purpose>();
                    var learningSources = new List<tbl_Source>();
                    learningNeeds = db.tbl_LearningNeed.Where(x => x.Enable == true).ToList();
                    learningPurposes = db.tbl_Purpose.Where(x => x.Enable == true).ToList();
                    learningSources = db.tbl_Source.Where(x => x.Enable == true).ToList();
                    var salers = new List<tbl_UserInformation>();
                    List<string> genders = new List<string> { "Nam", "Nữ", "Khác" };

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
                    // Gán dữ liệu
                    var dataGender = genders.Select(x => new DropDown()
                    {
                        name = x,
                    }).ToList();

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


                    // Gắn vào từng cột
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "B",
                        dataDropDown = dataGender,
                    });
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "H",
                        dataDropDown = dataLearningNeed,
                    });
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "I",
                        dataDropDown = datalearningPurpose,
                    });
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "J",
                        dataDropDown = datalearningSource,
                    });
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "K",
                        dataDropDown = dataSaler,
                    });

                    url = ExcelExportService.ExportTemplate(folderToSave, templateName, fileNameToSave, dropDowns, baseUrl);
                }
                catch (Exception e)
                {
                    throw e;
                }

                return url;
            }
        }

        // Lưu dữ liệu từ file excel học viên
        public static async Task<List<tbl_UserInformation>> InsertStudentExcel(List<InsertStudentExcel> itemModel, int branchId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var students = new List<tbl_UserInformation>();
                        var studentToShow = new List<tbl_UserInformation>();
                        var checkUserInformation = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.student && !string.IsNullOrEmpty(x.Email) && !string.IsNullOrEmpty(x.Mobile)).ToListAsync();
                        var learningNeed = await db.tbl_LearningNeed.Where(x => x.Enable == true).Select(x => new { x.Id, x.Name }).ToListAsync();
                        var purpose = await db.tbl_Purpose.Where(x => x.Enable == true).Select(x => new { x.Id, x.Name }).ToListAsync();
                        var source = await db.tbl_Source.Where(x => x.Enable == true).Select(x => new { x.Id, x.Name }).ToListAsync();
                        var listSaler = db.tbl_UserInformation.Where(x => x.RoleId == (int)RoleEnum.sale && x.Enable == true).ToList();
                        var branch = db.tbl_Branch.FirstOrDefault(x => x.Id == branchId);
                        if (branch == null)
                            throw new Exception("Không tìm thấy trung tâm");

                        foreach (var st in itemModel)
                        {
                            var checkUserName = checkUserInformation.FirstOrDefault(x => x.UserName.ToUpper() == st.UserName.ToUpper());
                            var checkEmail = checkUserInformation.FirstOrDefault(x => x.Email.ToUpper() == st.Email.ToUpper());
                            var student = new tbl_UserInformation();
                            student.FullName = st.FullName;
                            student.DOB = st.DOB;
                            student.Password = st.Password.ToString();
                            student.BranchIds = branchId.ToString();

                            if (checkUserName != null)
                            {
                                throw new Exception("Tên đăng nhập này đã tồn tại: " + st.UserName);
                            }
                            else
                            {
                                student.UserName = st.UserName;
                            }
                            if (checkEmail != null)
                            {
                                throw new Exception("Email này đã tồn tại: " + st.Email);
                            }
                            else
                            {
                                student.Email = st.Email;
                            }

                            // Kiểm tra số điện thoại
                            var checkMobile = checkUserInformation.Any(x => x.Mobile == st.Mobile);
                            if (checkMobile == true)
                            {
                                throw new Exception("Số điện thoại " + st.Mobile + " đã tồn tại trong hệ thống");
                            }
                            else
                            {
                                if (itemModel.Count(x => x.Mobile == st.Mobile) == 1)
                                {
                                    student.Mobile = st.Mobile;
                                }
                                else throw new Exception("Số điện thoại " + st.Mobile + " đang bị trùng. Vui lòng kiểm tra lại thông tin");
                            }

                            if (st.Gender.Contains("Nam"))
                            {
                                student.Gender = 1;
                            }
                            else if (st.Gender.Contains("Nữ"))
                            {
                                student.Gender = 2;
                            }
                            else if (st.Gender.Contains("Khác"))
                            {
                                student.Gender = 0;
                            }
                            else
                            {
                                throw new Exception("Vui lòng chọn giới tính");
                            }

                            // Check nhu cầu học
                            if (!string.IsNullOrEmpty(st.LearningNeedName))
                            {
                                var checkLearningNeed = learningNeed.FirstOrDefault(x => x.Name.Contains(st.LearningNeedName));
                                if (checkLearningNeed != null)
                                    student.LearningNeedId = checkLearningNeed.Id;
                                else throw new Exception("Nhu cầu học " + "(" + st.LearningNeedName + ")" + " không tồn tại trong hệ thống");
                            }
                            else
                                student.LearningNeedId = 0;

                            // Check mục đích học
                            if (!string.IsNullOrEmpty(st.PurposeName))
                            {
                                var checkPurpose = purpose.FirstOrDefault(x => x.Name.Contains(st.PurposeName));
                                if (checkPurpose != null)
                                    student.PurposeId = checkPurpose.Id;
                                else throw new Exception("Mục tiêu học " + "(" + st.PurposeName + ")" + " không tồn tại trong hệ thống");
                            }
                            else student.PurposeId = 0;

                            // Check nguồn khách hàng
                            if (!string.IsNullOrEmpty(st.SourceName))
                            {
                                var checkSource = source.FirstOrDefault(x => x.Name.Contains(st.SourceName));
                                if (checkSource != null)
                                    student.SourceId = checkSource.Id;
                                else throw new Exception("Nguồn khách hàng " + "(" + st.SourceName + ")" + " không tồn tại trong hệ thống");
                            }
                            else student.SourceId = 0;

                            // Nếu trung tâm có tư vấn viên thì bắt buộc phải chọn tư vấn viên và ngược lại
                            string bId = branch.Id.ToString();

                            var saleInBranch = listSaler.Where(x => x.BranchIds.Contains(bId)).ToList();
                            if (saleInBranch.Count == 0)
                            {
                                student.SaleId = 0;
                            }
                            else
                            {
                                var userCodeSplit = st.SaleName.Split('-')[0].Trim();
                                int startIndex = userCodeSplit.IndexOf('[') + 1;
                                int endIndex = userCodeSplit.IndexOf(']');
                                string userCode = userCodeSplit.Substring(startIndex, endIndex - startIndex);
                                var checkSaler = listSaler.FirstOrDefault(x => x.UserCode.ToUpper() == userCode.ToUpper());
                                if (checkSaler != null)
                                {
                                    student.SaleId = checkSaler.UserInformationId;
                                }
                                else throw new Exception("Không tìm thấy tư vấn viên có tên " + st.SaleName + " trong trung tâm " + branch.Name);
                            }
                            students.Add(student);
                        }
                        foreach (var std in students)
                        {
                            var model = new tbl_UserInformation(std);
                            if (user.RoleId == ((int)RoleEnum.sale))
                                model.SaleId = user.UserInformationId;
                            model.LearningStatus = 5;
                            model.LearningStatusName = "Đang học";
                            model.StatusId = 0;
                            model.RoleId = (int)RoleEnum.student;
                            model.RoleName = "Học viên";
                            model.SaleId = model.SaleId;
                            model.CreatedBy = model.ModifiedBy = user.FullName;
                            string baseCode = "HV";
                            model.ActiveDate = DateTime.Now;
                            model.CreatedOn = DateTime.Now;
                            model.ModifiedOn = DateTime.Now;
                            model.Enable = true;
                            model.IsReceiveMailNotification = true;
                            int count = await db.tbl_UserInformation.CountAsync(x =>
                                        x.CreatedOn.Value.Year == model.CreatedOn.Value.Year
                                        && x.CreatedOn.Value.Month == model.CreatedOn.Value.Month
                                        && x.CreatedOn.Value.Day == model.CreatedOn.Value.Day);
                            model.UserCode = AssetCRM.InitCode(baseCode, DateTime.Now, count + studentToShow.Count);
                            studentToShow.Add(model);
                            db.tbl_UserInformation.Add(model);
                        }

                        await db.SaveChangesAsync();

                        ////thông báo cho tư vấn viên

                        //Thread sendNoti = new Thread(async () =>
                        //{
                        //    foreach (var data in students)
                        //    {
                        //        UserInfoParam param = new UserInfoParam { UserId = data.UserInformationId };
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
                        return studentToShow;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception(ex.Message);
                    }
                }
            }
        }

        // Xuất file mẫu nhân viên để import
        public static async Task<string> ExportExampleEmployee(tbl_UserInformation user, string baseUrl)
        {
            string url = "";
            string templateName = "MauThemNhanVien.xlsx";
            string folderToSave = "SampleExcelEmployee";
            string fileNameToSave = $"MẫuThêmNhânViên";

            using (var db = new lmsDbContext())
            {
                try
                {
                    List<ListDropDown> dropDowns = new List<ListDropDown>();

                    List<string> genders = new List<string> { "Nam", "Nữ", "Khác" };

                    // 1-Admin, 2-Giáo viên, 4-Quản lý, 5-Tư vấn viên, 6-Kế toán, 7-Học vụ
                    List<string> roles = new List<string> { "Admin", "Giáo Viên", "Quản lý", "Tư vấn viên", "Kế toán", "Học vụ" };

                    // Gán dữ liệu
                    var dataGender = genders.Select(x => new DropDown()
                    {
                        name = x,
                    }).ToList();

                    var dataRole = roles.Select(x => new DropDown()
                    {
                        name = x,
                    }).ToList();
                    // Gắn vào từng cột
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "B",
                        dataDropDown = dataGender,
                    });
                    dropDowns.Add(new ListDropDown()
                    {
                        columnName = "F",
                        dataDropDown = dataRole,
                    });

                    url = ExcelExportService.ExportTemplate(folderToSave, templateName, fileNameToSave, dropDowns, baseUrl);
                }
                catch (Exception e)
                {
                    throw e;
                }

                return url;
            }
        }

        // Lưu dữ liệu từ file excel nhân viên
        public static async Task<List<tbl_UserInformation>> InsertEmployeeExcel(List<InsertEmployeeExcel> itemModel, int branchId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var employees = new List<tbl_UserInformation>();
                var employeeToShow = new List<tbl_UserInformation>();
                var checkUserInformation = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.student && !string.IsNullOrEmpty(x.Email) && !string.IsNullOrEmpty(x.Mobile)).ToListAsync();
                var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == branchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");

                foreach (var e in itemModel)
                {
                    var checkUserName = checkUserInformation.FirstOrDefault(x => x.UserName == e.UserName);
                    var checkEmail = checkUserInformation.FirstOrDefault(x => x.Email.ToUpper() == e.Email.ToUpper());
                    var employee = new tbl_UserInformation();
                    employee.FullName = e.FullName;
                    // Check validate username và email
                    if (checkUserName != null)
                    {
                        throw new Exception("Tên đăng nhập " + e.UserName + " đã tồn tại");
                    }
                    else
                    {
                        employee.UserName = e.UserName;
                    }
                    if (checkEmail != null)
                    {
                        throw new Exception("Email " + e.Email + " đã tồn tại");
                    }
                    else
                    {
                        employee.Email = e.Email;
                    }
                    // Check giới tính
                    if (e.Gender.Contains("Nam"))
                    {
                        employee.Gender = 1;
                    }
                    else if (e.Gender.Contains("Nữ"))
                    {
                        employee.Gender = 2;
                    }
                    else if (e.Gender.Contains("Khác"))
                    {
                        employee.Gender = 0;
                    }
                    else
                    {
                        throw new Exception("Vui lòng chọn giới tính");
                    }

                    // Check chức vụ
                    // 1-Admin, 2-Giáo viên, 4-Quản lý, 5-Tư vấn viên, 6-Kế toán, 7-Học vụ
                    if (e.RoleName.ToUpper().Contains("ADMIN"))
                    {
                        employee.RoleId = (int)RoleEnum.admin;
                    }
                    else if (e.RoleName.ToUpper().Contains("GIÁO VIÊN"))
                    {
                        employee.RoleId = (int)RoleEnum.teacher;
                    }
                    else if (e.RoleName.ToUpper().Contains("QUẢN LÝ"))
                    {
                        employee.RoleId = (int)RoleEnum.manager;
                    }
                    else if (e.RoleName.ToUpper().Contains("TƯ VẤN VIÊN"))
                    {
                        employee.RoleId = (int)RoleEnum.sale;
                    }
                    else if (e.RoleName.ToUpper().Contains("KẾ TOÁN"))
                    {
                        employee.RoleId = (int)RoleEnum.accountant;
                    }
                    else if (e.RoleName.ToUpper().Contains("HỌC VỤ"))
                    {
                        employee.RoleId = (int)RoleEnum.academic;
                    }
                    else
                    {
                        throw new Exception("Vui lòng chọn chức vụ");
                    }

                    employee.RoleName = e.RoleName;
                    employee.DOB = e.DOB;
                    var checkMobile = checkUserInformation.Any(x => x.Mobile == e.Mobile);
                    if (checkMobile == true)
                    {
                        throw new Exception("Số điện thoại " + e.Mobile + " đã tồn tại trong hệ thống");
                    }
                    else
                    {
                        if (itemModel.Count(x => x.Mobile == e.Mobile) == 1)
                        {
                            employee.Mobile = e.Mobile;
                        }
                        else throw new Exception("Số điện thoại " + e.Mobile + " đang bị trùng. Vui lòng kiểm tra lại thông tin");
                    }
                    employee.Password = Encryptor.Encrypt(e.Password.ToString());
                    employee.BranchIds = branchId.ToString();
                    employees.Add(employee);
                }
                foreach (var std in employees)
                {
                    var model = new tbl_UserInformation(std);
                    if (user.RoleId == ((int)RoleEnum.sale))
                        model.SaleId = user.UserInformationId;
                    else model.SaleId = await CustomerService.GetSaleRadom(model.BranchIds, 2);
                    model.LearningStatus = 5;
                    model.LearningStatusName = "Đang học";
                    model.StatusId = 0;
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    string baseCode = "";
                    if (model.RoleId == (int)RoleEnum.admin)
                    {
                        baseCode = "QTV";
                    }
                    else if (model.RoleId != (int)RoleEnum.admin && model.RoleId != (int)RoleEnum.student && model.RoleId != (int)RoleEnum.parents)
                    {
                        baseCode = "NV";
                    }
                    model.ActiveDate = DateTime.Now;
                    model.CreatedOn = DateTime.Now;
                    model.ModifiedOn = DateTime.Now;
                    model.Enable = true;
                    model.IsReceiveMailNotification = true;
                    int count = await db.tbl_UserInformation.CountAsync(x =>
                                x.CreatedOn.Value.Year == model.CreatedOn.Value.Year
                                && x.CreatedOn.Value.Month == model.CreatedOn.Value.Month
                                && x.CreatedOn.Value.Day == model.CreatedOn.Value.Day);
                    model.UserCode = AssetCRM.InitCode(baseCode, DateTime.Now, count + employeeToShow.Count);
                    employeeToShow.Add(model);
                    db.tbl_UserInformation.Add(model);
                }

                await db.SaveChangesAsync();

                ////thông báo cho tư vấn viên

                //Thread sendNoti = new Thread(async () =>
                //{
                //    foreach (var data in employees)
                //    {
                //        UserInfoParam param = new UserInfoParam { UserId = data.UserInformationId };
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

                return employeeToShow;
            }
        }

        // Lấy tất cả tư vấn viên để lọc khách hàng (Bao gồm các tư vấn viên bị xóa hoặc bị khóa)
        public static async Task<AppDomainResult> GetAllSaler(SalerSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) return new AppDomainResult { TotalRow = 0, Data = null };

                if (user.RoleId != ((int)RoleEnum.admin))
                    baseSearch.BranchIds = user.BranchIds;

                string sql = $"Get_Saler @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@SaleIds = N'{baseSearch.SaleIds ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'";
                var data = await db.SqlQuery<SalerDTO>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                return new AppDomainResult { TotalRow = totalRow, Data = data };
            }
        }

        public static async Task<bool> StudentsForSaler(List<SetStudentForSaler> itemModel, int branchId, int saleId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var userInformation = await db.tbl_UserInformation.Where(x => x.Enable == true).ToListAsync();

                        // Kiểm tra tư vấn viên có thuộc chi nhánh hay không
                        var saler = userInformation.FirstOrDefault(x => x.UserInformationId == saleId && x.Enable == true);
                        var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == branchId && x.Enable == true);
                        if (saler == null) throw new Exception("Không tìm thấy tư vấn viên");
                        if (branch == null) throw new Exception("Không tìm thấy trung tâm");
                        if (saler.BranchIds.Contains(branch.Id.ToString()) == false) throw new Exception("Tư vấn viên " + saler.FullName + " không có trong trung tâm " + branch.Name);

                        // Lấy list học viên ra trước để dùng
                        var studentList = userInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.student).ToList();

                        foreach (var c in itemModel)
                        {
                            // Kiểm tra học viên có tồn tại hay không
                            var student = studentList.FirstOrDefault(x => x.UserInformationId == c.StudentId);
                            if (student == null) throw new Exception("Học viên có ID = " + c.StudentId + " không tồn tại");

                            // Kiểm tra học viên có cùng trung tâm với tư vấn viên hay không
                            if (string.IsNullOrEmpty(student.BranchIds))
                                throw new Exception("Học viên " + student.FullName + " chưa có trung tâm. Vui lòng cập nhật trung tâm cho học viên trước!");
                            if (saler.BranchIds.Contains(student.BranchIds.ToString()) == false)
                                throw new Exception("Tư vấn viên " + saler.FullName + " và học sinh " + student.FullName + " không ở cùng một trung tâm");

                            // Thay đổi tư vấn viên cho học viên
                            student.SaleId = saleId;
                            student.ModifiedBy = user.FullName;
                            student.ModifiedOn = DateTime.Now;
                            await db.SaveChangesAsync();
                        }
                        tran.Commit();
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

        public static async Task<AppDomainResult> GetStudentForSaler(StudentForSalerSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StudentForSalerSearch();
                if (user.RoleId != ((int)RoleEnum.admin))
                    baseSearch.BranchIds = user.BranchIds;
                string sql = $"Get_Student_For_Saler @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@StudentIds = N'{baseSearch.StudentIds ?? ""}'," +
                    $"@SaleIds = N'{baseSearch.SaleIds ?? ""}'," +
                    $"@IsAssign = N'{baseSearch.IsAssign ?? false}'," +
                    $"@LearningNeedIds = N'{baseSearch.LearningNeedIds ?? ""}'," +
                    $"@PurposeIds = N'{baseSearch.PurposeIds ?? ""}'," +
                    $"@SourceIds = N'{baseSearch.SourceIds ?? ""}'";
                var data = await db.SqlQuery<StudentForSalerDTO>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                return new AppDomainResult { TotalRow = totalRow, Data = data };
            }
        }

        public static async Task<List<EmplyeeBranchDTO>> GetEmployeeInBranch(int employeeId)
        {
            using (var db = new lmsDbContext())
            {
                var employee = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == employeeId && x.Enable == true);
                if (employee == null)
                    throw new Exception("Không tìm thấy thông tin nhân viên");
                var splitBranch = employee.BranchIds.Split(',');
                var bracnhs = await db.tbl_Branch.Where(x => x.Enable == true).ToListAsync();
                var employeeBranchs = new List<EmplyeeBranchDTO>();
                if (bracnhs.Count != 0)
                {
                    foreach (var b in bracnhs)
                    {
                        var employeeBranch = new EmplyeeBranchDTO();
                        employeeBranch.BranchId = b.Id;
                        employeeBranch.BranchName = b.Name;
                        if (splitBranch.Length == 0)
                            employeeBranch.IsInBranch = false;
                        else if (splitBranch.Contains(b.Id.ToString()))
                            employeeBranch.IsInBranch = true;
                        else
                            employeeBranch.IsInBranch = false;

                        employeeBranchs.Add(employeeBranch);
                    }
                }
                return employeeBranchs;
            }
        }

        public static async Task<tbl_UserInformation> UpdateEmployeeBranch(EmployeeBranchUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var employee = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == itemModel.EmployeeId && x.Enable == true);
                if (employee == null)
                    throw new Exception("Không tìm thấy thông tin nhân viên");
                var manager = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == itemModel.ManagerId && x.Enable == true);
                if (manager == null)
                    throw new Exception("Không tìm thấy thông tin quản lý");
                int[] splitBranch = itemModel.BranchIds.Split(',').Select(int.Parse).ToArray();
                foreach (var b in splitBranch)
                {
                    var bracnh = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Enable == true && x.Id == b);
                    if (bracnh == null) throw new Exception("Không tìm thấy thông tin chi nhánh có Id = " + b);
                    if (!employee.BranchIds.Contains(b.ToString()))
                    {
                        if (user.RoleId != (int)RoleEnum.admin)
                        {
                            if (!manager.BranchIds.Contains(b.ToString()))
                                throw new Exception("Bạn không được phép thêm nhân viên vào chi nhánh " + bracnh.Name + ". Do bạn không quản lý chi nhánh này!");
                        }
                    }
                }
                //bool allBranchesExist = await Task.WhenAll(splitBranch.Select(b => db.tbl_Branch.AnyAsync(x => x.Id == b && x.Enable == true)))
                //                  .ContinueWith(task => task.Result.All(exists => exists));

                employee.BranchIds = itemModel.BranchIds;
                await db.SaveChangesAsync();
                return employee;
            }
        }
    }
}