using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;
using LMSCore.DTO.StaffDTO;
using System.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using LMSCore.Utilities;
using LMSCore.LMS;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace LMSCore.Services.Staff
{
    public class StaffService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        private UserInformation userinformationService;
        public StaffService(lmsDbContext dbContext) : base(dbContext)
        {
            userinformationService = new UserInformation(dbContext,_hostingEnvironment);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của nhân viên
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StaffDetailDTO> GetById(int id)
        {
            var result = new StaffDetailDTO();
            var data = await dbContext.tbl_UserInformation
                .SingleOrDefaultAsync(x => x.UserInformationId == id 
                && CoreContants.ListStaffEnum.Contains(x.RoleId ?? 0) 
                && x.Enable == true);
            if (data == null)
                return null;
            result.Id = data.UserInformationId;
            result.Information = new StaffInformation
            {
                Avatar = data.Avatar,
                AvatarReSize = data.AvatarReSize,
                CreatedBy = data.CreatedBy,
                CreatedOn = data.CreatedOn,
                DOB = data.DOB,
                Email = data.Email,
                Extension = data.Extension,
                FullName = data.FullName,
                Gender = data.Gender,
                Mobile = data.Mobile,
                ModifiedBy = data.ModifiedBy,
                ModifiedOn = data.ModifiedOn,
                RoleId = data.RoleId,
                RoleName = data.RoleName,
                StatusId = data.StatusId,
                UserCode = data.UserCode,
                BranchIds = data.BranchIds
            };
            var area = await dbContext.tbl_Area.SingleOrDefaultAsync(x => x.Id == data.AreaId);
            var district = await dbContext.tbl_District.SingleOrDefaultAsync(x => x.Id == data.DistrictId);
            var ward = await dbContext.tbl_Ward.SingleOrDefaultAsync(x => x.Id == data.DistrictId);
            result.Address = new StaffAddress
            {
                Address = data.Address,
                AreaId = data.AreaId,
                DistrictId = data.DistrictId,
                WardId = data.WardId,
                AreaName = area?.Name,
                DistrictName = district?.Name,
                WardName = ward?.Name
            };
            result.Bank = new StaffBank
            {
                BankAccountName = data.BankAccountName,
                BankAccountNumber = data.BankAccountNumber,
                BankBranch = data.BankBranch,
                BankName = data.BankName
            };
            result.Account = new StaffAccount
            {
                UserName = data.UserName
            };
            return result;
        }
        /// <summary>
        /// Lấy thông tin cá nhân
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<StaffDetailDTO> GetMe(tbl_UserInformation currentUser)
        {
            var result = new StaffDetailDTO();
            var data = currentUser;
            result.Id = data.UserInformationId;
            result.Information = new StaffInformation
            {
                Avatar = data.Avatar,
                AvatarReSize = data.AvatarReSize,
                CreatedBy = data.CreatedBy,
                CreatedOn = data.CreatedOn,
                DOB = data.DOB,
                Email = data.Email,
                Extension = data.Extension,
                FullName = data.FullName,
                Gender = data.Gender,
                Mobile = data.Mobile,
                ModifiedBy = data.ModifiedBy,
                ModifiedOn = data.ModifiedOn,
                RoleId = data.RoleId,
                RoleName = data.RoleName,
                StatusId = data.StatusId,
                UserCode = data.UserCode,
                BranchIds = data.BranchIds
            };
            var area = await dbContext.tbl_Area.SingleOrDefaultAsync(x => x.Id == data.AreaId);
            var district = await dbContext.tbl_District.SingleOrDefaultAsync(x => x.Id == data.DistrictId);
            var ward = await dbContext.tbl_Ward.SingleOrDefaultAsync(x => x.Id == data.DistrictId);
            result.Address = new StaffAddress
            {
                Address = data.Address,
                AreaId = data.AreaId,
                DistrictId = data.DistrictId,
                WardId = data.WardId,
                AreaName = area?.Name,
                DistrictName = district?.Name,
                WardName = ward?.Name
            };
            result.Bank = new StaffBank
            {
                BankAccountName = data.BankAccountName,
                BankAccountNumber = data.BankAccountNumber,
                BankBranch = data.BankBranch,
                BankName = data.BankName
            };
            result.Account = new StaffAccount
            {
                UserName = data.UserName
            };
            return result;
        }
        /// <summary>
        /// Admin có thể xem tất cả
        /// Quản lý có thể xem tất cả nhân viên thuộc chi nhánh của mình ngoài quản lý khác
        /// Kế toán có thể xem tất cả nhân thuộc chi nhánh của mình ngoại trừ quản lý và kế toán khác
        /// Học vụ có thể xem danh sách giáo viên
        /// Các nhân viên còn lại chỉ xem được thông tin của mình
        /// </summary>
        /// <param name="curentUser"></param>
        /// <param name="staffId"></param>
        /// <param name="role"></param>
        /// <param name="branchIds"></param>
        /// <returns></returns>
        public async Task<bool> HasPermission(tbl_UserInformation curentUser, int staffId, int role, string branchIds)
        {
            if (staffId != 0)
            {
                var staff = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == staffId);
                role = staff?.RoleId ?? 0;
                branchIds = staff.BranchIds;
            }
            if (curentUser.RoleId == ((int)RoleEnum.admin))
            {
                return true;
            }
            else if (curentUser.RoleId == ((int)RoleEnum.manager))
            {
                if (role == ((int)RoleEnum.admin) || (role == ((int)RoleEnum.manager) && curentUser.UserInformationId != staffId))
                    return false;
                if (!CoreContants.ExistsBranch(curentUser.BranchIds, branchIds))
                    return false;
            }
            else if (curentUser.RoleId == ((int)RoleEnum.accountant))
            {
                if (role == ((int)RoleEnum.admin)
                || role == ((int)RoleEnum.manager)
                || (role == ((int)RoleEnum.accountant) && curentUser.UserInformationId != staffId))
                    return false;
                if (!CoreContants.ExistsBranch(curentUser.BranchIds, branchIds))
                    return false;
            }
            else if (curentUser.RoleId == ((int)RoleEnum.academic))
            {
                if (role != ((int)RoleEnum.teacher) && role != ((int)RoleEnum.academic) && curentUser.UserInformationId != staffId)
                    return false;
                if (!CoreContants.ExistsBranch(curentUser.BranchIds, branchIds))
                    return false;
            }
            else
            {
                if (curentUser.UserInformationId != staffId)
                    return false;
            }
            return true;
        }
        
        public async Task<StaffDetailDTO> Insert(StaffDetailPost itemModel, tbl_UserInformation currentUser)
        {
            using (var tran = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (itemModel == null)
                        throw new Exception("không tìm thấy dữ liệu");
                    string password = "";
                    if (!string.IsNullOrEmpty(itemModel.Account.UserName))
                    {
                        if (string.IsNullOrEmpty(itemModel.Account.Password))
                            throw new Exception("Vui lòng nhập mật khẩu");
                        await userinformationService.ValidUserName(itemModel.Account.UserName, 0);
                        password = Encryptor.Encrypt(itemModel.Account.Password);
                    }
                    if (!CoreContants.IsStaff(itemModel.Information.RoleId.Value))
                        throw new Exception("Chức vụ không phù hợp");
                    await ValidBranch(itemModel.Information.BranchIds);
                    if (!string.IsNullOrEmpty(itemModel.Information.Mobile))
                    {
                        if (!CoreContants.IsValidPhoneNumber(itemModel.Information.Mobile))
                            throw new Exception("Số điện thoại không hợp lệ");
                    }
                    if (!string.IsNullOrEmpty(itemModel.Information.Email))
                    {

                        if (!CoreContants.IsValidEmail(itemModel.Information.Email))
                            throw new Exception("Email không hợp lệ");
                    }

                    var entity = new tbl_UserInformation
                    {
                        UserCode = await InitCode(itemModel.Information.RoleId.Value),
                        ActiveDate = DateTime.Now,
                        Address = itemModel.Address.Address,
                        RoleId = itemModel.Information.RoleId,
                        AreaId = itemModel.Address.AreaId,
                        Avatar = itemModel.Information.Avatar,
                        AvatarReSize = itemModel.Information.AvatarReSize,
                        BankAccountName = itemModel.Bank.BankAccountName,
                        BankAccountNumber = itemModel.Bank.BankAccountNumber,
                        BankBranch = itemModel.Bank.BankBranch,
                        BankName = itemModel.Bank.BankName,
                        BranchIds = itemModel.Information.BranchIds,
                        CommissionConfigId = null,
                        CreatedBy = currentUser.FullName,
                        CreatedDateKeyForgot = null,
                        CreatedOn = itemModel.CreatedOn,
                        CustomerId = 0,
                        DistrictId = itemModel.Address.DistrictId,
                        DOB = itemModel.Information.DOB,
                        Email = itemModel.Information.Email,
                        Enable = true,
                        Extension = itemModel.Information.Extension,
                        FullName = itemModel.Information.FullName,
                        Gender = itemModel.Information.Gender,
                        IsReceiveMailNotification = false,
                        JobId = null,
                        KeyForgotPassword = null,
                        LearningNeedId = null,
                        LearningStatus = 0,
                        LearningStatusName = null,
                        Mobile = itemModel.Information.Mobile,
                        ModifiedBy = currentUser.FullName,
                        ModifiedOn = itemModel.CreatedOn,
                        OneSignal_DeviceId = null,
                        ParentId = null,
                        Password = password,
                        PurposeId = null,
                        RefreshToken = null,
                        RefreshTokenExpires = null,
                        RoleName = CoreContants.GetRoleName(itemModel.Information.RoleId.Value),
                        SaleId = 0,
                        SourceId = null,
                        StatusId = ((int)lmsEnum.AccountStatus.active),
                        UserName = itemModel.Account.UserName,
                        WardId = itemModel.Address.WardId
                    };
                    await dbContext.tbl_UserInformation.AddAsync(entity);
                    await dbContext.SaveChangesAsync();
                    await AssginToProgram(entity.UserInformationId, itemModel.AssginToProgram, currentUser);
                    await tran.CommitAsync();
                    return await GetById(entity.UserInformationId);
                }
                catch (Exception e)
                {
                    await tran.RollbackAsync();
                    throw new Exception(e.Message);
                }
            }
        }
        private async Task ValidBranch(string branchIds)
        {
            if (string.IsNullOrEmpty(branchIds))
                throw new Exception("Vui lòng chọn chi nhánh");
            var listBranchId = branchIds.Split(',');
            foreach (var branchId in listBranchId)
            {
                var hasBranch = await dbContext.tbl_Branch.AnyAsync(x => x.Id.ToString() == branchId);
                if (!hasBranch)
                    throw new Exception("Vui lòng chọn chi nhánh");
            }
        }
        private async Task AssginToProgram(int teacherId,AssginToProgramPost itemModel, tbl_UserInformation currentUser)
        {
            if (itemModel == null)
                return;
            if (itemModel.Items.Any())
            {
                foreach (var item in itemModel.Items)
                {
                    var hasProgram = await dbContext.tbl_Program.AnyAsync(x => x.Id == item.ProgramId);
                    if (!hasProgram)
                        throw new Exception("Không tìm thấy chương trình học");
                    var teacherInProgram = new tbl_TeacherInProgram
                    {
                        CreatedBy = currentUser.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = currentUser.FullName,
                        ModifiedOn = DateTime.Now,
                        ProgramId = item.ProgramId,
                        TeacherId = teacherId,
                        TeachingFee = item.TeachingFee
                    };
                    await dbContext.tbl_TeacherInProgram.AddAsync(teacherInProgram);
                }
                await dbContext.SaveChangesAsync();
            }
        }
        private async Task<string> InitCode(int roleId)
        {
            string baseCode = roleId == ((int)RoleEnum.admin) ? "QTV" 
                                        : roleId == ((int)RoleEnum.manager) ? "QL" : "NV";
            var now = DateTime.Now;
            int count = 0;
            switch (baseCode)
            {
                case "QTV":
                        count = await dbContext.tbl_UserInformation.CountAsync(x => (
                        x.RoleId == ((int)RoleEnum.admin)
                        && x.CreatedOn.Value.Year == now.Year
                        && x.CreatedOn.Value.Month == now.Month
                        && x.CreatedOn.Value.Day == now.Day));
                    break;
                case "QL":
                    count = await dbContext.tbl_UserInformation.CountAsync(x => (
                    x.RoleId == ((int)RoleEnum.manager)
                    && x.CreatedOn.Value.Year == now.Year
                    && x.CreatedOn.Value.Month == now.Month
                    && x.CreatedOn.Value.Day == now.Day));
                    break;
                case "NV":
                    count = await dbContext.tbl_UserInformation.CountAsync(x => (
                    CoreContants.ListStaffEnum.Contains(x.RoleId.Value) 
                    && x.RoleId != ((int)RoleEnum.manager)
                    && x.RoleId != ((int)RoleEnum.admin)
                    && x.CreatedOn.Value.Year == now.Year
                    && x.CreatedOn.Value.Month == now.Month
                    && x.CreatedOn.Value.Day == now.Day));
                    break;
            }
            return AssetCRM.InitCode(baseCode, now, count + 1);
        }
        public async Task<StaffDetailDTO> UpdateMe(StaffDetailMePut itemModel, tbl_UserInformation currentUser)
        {
            var entity = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == currentUser.UserInformationId);
            if (entity == null)
                throw new Exception("Không tìm thấy dữ liệu");
            string password = entity.Password;
            if (!string.IsNullOrEmpty(itemModel.Account.UserName))
            {
                await userinformationService.ValidUserName(itemModel.Account.UserName, entity.UserInformationId);
                if (!string.IsNullOrEmpty(itemModel.Account.Password))
                    password = Encryptor.Encrypt(itemModel.Account.Password);
            }
            if (!string.IsNullOrEmpty(itemModel.Information.Mobile))
            {
                if (!CoreContants.IsValidPhoneNumber(itemModel.Information.Mobile))
                    throw new Exception("Số điện thoại không hợp lệ");
            }
            if (!string.IsNullOrEmpty(itemModel.Information.Email))
            {

                if (!CoreContants.IsValidEmail(itemModel.Information.Email))
                    throw new Exception("Email không hợp lệ");
            }

            entity.FullName = itemModel.Information.FullName ?? entity.FullName;
            entity.DOB = itemModel.Information.DOB ?? entity.DOB;
            entity.Gender = itemModel.Information.Gender ?? entity.Gender;
            entity.Mobile = itemModel.Information.Mobile ?? entity.Mobile;
            entity.Email = itemModel.Information.Email ?? entity.Email;
            entity.Avatar = itemModel.Information.Avatar ?? entity.Avatar;
            entity.AvatarReSize = itemModel.Information.AvatarReSize ?? entity.AvatarReSize;
            entity.Extension = itemModel.Information.Extension ?? entity.Extension;

            entity.Address = itemModel.Address.Address ?? entity.Address;
            entity.AreaId = itemModel.Address.AreaId ?? entity.AreaId;
            entity.DistrictId = itemModel.Address.DistrictId ?? entity.DistrictId;
            entity.WardId = itemModel.Address.WardId ?? entity.WardId;

            entity.UserName = itemModel.Account.UserName ?? entity.UserName;
            entity.Password = password;

            entity.ModifiedBy = currentUser.FullName;
            entity.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return await GetMe(currentUser);
        }
        public async Task<StaffDetailDTO> Update(StaffDetailPut itemModel, tbl_UserInformation currentUser)
        {
            var entity = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.Id);
            if (entity == null)
                throw new Exception("Không tìm thấy dữ liệu");
            string password = entity.Password;
            if (!string.IsNullOrEmpty(itemModel.Account.UserName))
            {
                await userinformationService.ValidUserName(itemModel.Account.UserName, entity.UserInformationId);
                if(!string.IsNullOrEmpty(itemModel.Account.Password))
                    password = Encryptor.Encrypt(itemModel.Account.Password);
            }
            if (!string.IsNullOrEmpty(itemModel.Information.Mobile))
            {
                if (!CoreContants.IsValidPhoneNumber(itemModel.Information.Mobile))
                    throw new Exception("Số điện thoại không hợp lệ");
            }
            if (!string.IsNullOrEmpty(itemModel.Information.Email))
            {

                if (!CoreContants.IsValidEmail(itemModel.Information.Email))
                    throw new Exception("Email không hợp lệ");
            }

            entity.FullName = itemModel.Information.FullName ?? entity.FullName;
            entity.DOB = itemModel.Information.DOB ?? entity.DOB;
            entity.Gender = itemModel.Information.Gender ?? entity.Gender;
            entity.Mobile = itemModel.Information.Mobile ?? entity.Mobile;
            entity.Email = itemModel.Information.Email ?? entity.Email;
            entity.Avatar = itemModel.Information.Avatar ?? entity.Avatar;
            entity.AvatarReSize = itemModel.Information.AvatarReSize ?? entity.AvatarReSize;
            entity.Extension = itemModel.Information.Extension ?? entity.Extension;
            entity.StatusId = itemModel.Information.StatusId ?? entity.StatusId;

            entity.Address = itemModel.Address.Address ?? entity.Address;
            entity.AreaId = itemModel.Address.AreaId ?? entity.AreaId;
            entity.DistrictId = itemModel.Address.DistrictId ?? entity.DistrictId;
            entity.WardId = itemModel.Address.WardId ?? entity.WardId;


            entity.BankAccountNumber = itemModel.Bank.BankAccountNumber ?? entity.BankAccountNumber;
            entity.BankAccountName = itemModel.Bank.BankAccountName ?? entity.BankAccountName;
            entity.BankName = itemModel.Bank.BankName ?? entity.BankName;
            entity.BankBranch = itemModel.Bank.BankBranch ?? entity.BankBranch;

            entity.UserName = itemModel.Account.UserName ?? entity.UserName;
            entity.Password = password;

            entity.ModifiedBy = currentUser.FullName;
            entity.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return await GetById(entity.UserInformationId);
        }
        public async Task Delete(int id, tbl_UserInformation currentUser)
        {
            var entity = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == id
                            && CoreContants.ListStaffEnum.Contains(x.RoleId ?? 0)
                            && x.Enable == true);
            if (entity == null)
                throw new Exception("Không tìm thấy dữ liệu");
            entity.Enable = false;
            entity.ModifiedBy = currentUser.FullName;
            entity.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
        }
        public async Task<bool> HasPermissionBranch(int branchId, tbl_UserInformation currentUser)
        {
            return await Task.Run(() =>
                {
                    if (currentUser.RoleId == ((int)RoleEnum.admin))
                        return true;
                    if (string.IsNullOrEmpty(currentUser.BranchIds))
                        return false;
                    var listBranchIds = currentUser.BranchIds.Split(',');
                    foreach (var item in listBranchIds)
                    {
                        if (item == branchId.ToString())
                            return true;
                    }
                    return false;
                }
            );
        }
        /// <summary>
        /// Admin có thể xem tất cả
        /// Quản lý có thể xem tất cả nhân viên thuộc chi nhánh của mình ngoài quản lý khác
        /// Kế toán có thể xem tất cả nhân thuộc chi nhánh của mình ngoại trừ quản lý và kế toán khác
        /// Học vụ có thể xem danh sách giáo viên
        /// Các nhân viên còn lại chỉ xem được thông tin của mình
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<AppDomainResult<StaffDTO>> Get(StaffSearch baseSearch, tbl_UserInformation currentUser)
        {
            string select = "select u.UserInformationId ";
            string selectTotalRow = "select COUNT(u.UserInformationId) ";

            StringBuilder body = new StringBuilder();
            body.Append($"from tbl_UserInformation u ");
            body.Append($"where u.Enable = 1 ");
            if(!string.IsNullOrEmpty(baseSearch.Search))
                body.Append($"and (u.UserCode like N'%{baseSearch.Search}%' or u.FullName like N'%{baseSearch.Search}%' or u.Mobile like N'%{baseSearch.Search}%' or u.Email like N'%{baseSearch.Search}%') ");
            if(!string.IsNullOrEmpty(baseSearch.RoleIds))
                body.Append($"and u.RoleId in (select value from string_split('{baseSearch.RoleIds}',',')) ");
            body.Append($"and '{baseSearch.BranchId}' in (select value from string_split(u.BranchIds,',')) ");
            var listStaffEnum = CoreContants.ListStaffEnum;
            if (currentUser.RoleId == ((int)RoleEnum.manager))
            {
                listStaffEnum = listStaffEnum.Where(x => x != ((int)RoleEnum.admin) && x != ((int)RoleEnum.manager)).ToList();
            }
            else if (currentUser.RoleId == ((int)RoleEnum.accountant))
            {
                listStaffEnum = listStaffEnum.Where(x =>
                x != ((int)RoleEnum.admin)
                && x != ((int)RoleEnum.manager)
                && x != ((int)RoleEnum.accountant)
                ).ToList();
            }
            else if (currentUser.RoleId == ((int)RoleEnum.academic))
            {
                listStaffEnum = new List<int> { ((int)RoleEnum.teacher) };
            }
            else if (currentUser.RoleId != ((int)RoleEnum.admin))
            {
                listStaffEnum = new List<int> { 0 };
            }
            body.Append($"and u.RoleId in (select value from string_split('{string.Join(',', listStaffEnum)}',',')) ");
            string orderBy = "";
            switch (baseSearch.Sort)
            {
                case 1: orderBy = "order by u.FullName asc "; break;
                case 2: orderBy = "order by u.FullName desc "; break;
                case 3: orderBy = "order by u.RoleName asc "; break;
                case 4: orderBy = "order by u.RoleName desc "; break;
                default: orderBy = "order by u.UserInformationId desc "; break;
            }
            string pagination = $"offset (({baseSearch.PageIndex} - 1)*{baseSearch.PageSize}) rows fetch next {baseSearch.PageSize} rows only ";
            string sqlQueryTotalRow = selectTotalRow + body.ToString();
            string sqlQuery = select + body.ToString() + orderBy + pagination;

            var totalRow = await DapperQuery<int>(sqlQueryTotalRow);
            var l = await DapperQuery<int>(sqlQuery);
            var data = await GetDetails(l);
            return new AppDomainResult<StaffDTO> 
            { 
                TotalRow = totalRow[0], 
                Data = data
            };
        }
        public async Task<List<StaffDTO>> GetDetails(List<int> l)
        {
            string sqlQuery = "select u.UserInformationId as Id, " +
                "u.FullName, u.UserName, u.UserCode, u.DOB, u.Gender, u.Mobile, u.Email, " +
                "u.RoleId, u.RoleName, u.Avatar, u.AvatarReSize, u.CreatedOn, u.CreatedBy, u.ModifiedOn, u.ModifiedBy, " +
                "( select b.Id, b.Name " +
                "from tbl_Branch b " +
                "where b.Id IN (select value from string_split(u.BranchIds, ',')) " +
                "for json path ) as BranchesJson " +
                "from tbl_UserInformation u " +
                $"where u.UserInformationId in (select value from string_split('{string.Join(',',l)}',',')) ";
            return await DapperQuery<StaffDTO>(sqlQuery);
        }
    }
}
