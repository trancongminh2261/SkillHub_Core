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
using LMSCore.DTO.StudentDTO;
using OfficeOpenXml;
using Hangfire.Annotations;

namespace LMSCore.Services.Student
{
    public class StudentService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        private UserInformation userinformationService;
        public StudentService(lmsDbContext dbContext) : base(dbContext)
        {
            userinformationService = new UserInformation(dbContext, _hostingEnvironment);
        }
        public async Task<StudentDetailDTO> GetById(int Id)
        {
            var result = new StudentDetailDTO();
            var data = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == Id);
            if (data == null)
                return null;
            int branchId = 0;
            int.TryParse(data.BranchIds.Trim(), out branchId);
            var branch = await dbContext.tbl_Branch.SingleOrDefaultAsync(x => x.Id == branchId);
            var job = await dbContext.tbl_Job.SingleOrDefaultAsync(x => x.Id == data.JobId);
            var learningNeed = await dbContext.tbl_LearningNeed.SingleOrDefaultAsync(x => x.Id == data.LearningNeedId);
            var purpose = await dbContext.tbl_Purpose.SingleOrDefaultAsync(x => x.Id == data.PurposeId);
            var sale = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.SaleId);
            var source = await dbContext.tbl_Source.SingleOrDefaultAsync(x => x.Id == data.SourceId);
            result.Information = new StudentInformation
            {
                ActiveDate = data.ActiveDate,
                Avatar = data.Avatar,
                AvatarReSize = data.AvatarReSize,
                BranchId = branch?.Id ?? 0,
                BranchName = branch?.Name,
                CreatedBy = data.CreatedBy,
                CreatedOn = data.CreatedOn,
                CustomerId = data.CustomerId,
                DOB = data.DOB,
                Email = data.Email,
                Extension = data.Extension,
                FullName = data.FullName,
                Gender = data.Gender,
                JobId = data.JobId,
                JobName = job?.Name,
                LearningNeedId = data.LearningNeedId,
                LearningNeedName = learningNeed?.Name,
                LearningStatus = data.LearningStatus,
                Mobile = data.Mobile,
                ModifiedBy = data.ModifiedBy,
                ModifiedOn = data.ModifiedOn,
                PurposeId = data.PurposeId,
                PurposeName = purpose?.Name,
                SaleId = data.SaleId,
                SaleName = sale?.FullName,
                SourceId = data.SourceId,
                SourceName = source?.Name,
                StatusId = data.StatusId,
                UserCode = data.UserCode,
            };

            var parent = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.ParentId);
            if (parent != null)
            {
                result.Parents = new StudentParents
                {
                    Address = parent.Address,
                    UserName = parent.UserName,
                    Email = parent.Email,
                    Extension = parent.Extension,
                    FullName = parent.FullName,
                    Gender = parent.Gender,
                    Mobile = parent.Mobile,
                    StatusId = parent.StatusId,
                    Avatar = parent.Avatar,
                    AvatarReSize = parent.AvatarReSize,
                };
            }

            var area = await dbContext.tbl_Area.SingleOrDefaultAsync(x => x.Id == data.AreaId);
            var district = await dbContext.tbl_District.SingleOrDefaultAsync(x => x.Id == data.DistrictId);
            var ward = await dbContext.tbl_Ward.SingleOrDefaultAsync(x => x.Id == data.WardId);
            result.Address = new StudentAddress
            {
                Address = data.Address,
                AreaId = data.AreaId,
                AreaName = area?.Name,
                DistrictId = data.DistrictId,
                DistrictName = district?.Name,
                WardId = data.WardId,
                WardName = ward?.Name
            };
            result.Acount = new StudentAcount
            {
                UserName = data.UserName
            };
            return result;

        }
        /// <summary>
        /// Xem thông tin cá nhân
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<StudentDetailDTO> GetMe(tbl_UserInformation currentUser)
        {
            var result = new StudentDetailDTO();
            var data = currentUser;
            int branchId = 0;
            int.TryParse(data.BranchIds.Trim(), out branchId);
            var branch = await dbContext.tbl_Branch.SingleOrDefaultAsync(x => x.Id == branchId);
            var job = await dbContext.tbl_Job.SingleOrDefaultAsync(x => x.Id == data.JobId);
            var learningNeed = await dbContext.tbl_LearningNeed.SingleOrDefaultAsync(x => x.Id == data.LearningNeedId);
            var purpose = await dbContext.tbl_Purpose.SingleOrDefaultAsync(x => x.Id == data.PurposeId);
            var sale = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.SaleId);
            var source = await dbContext.tbl_Source.SingleOrDefaultAsync(x => x.Id == data.SourceId);
            result.Information = new StudentInformation
            {
                ActiveDate = data.ActiveDate,
                Avatar = data.Avatar,
                AvatarReSize = data.AvatarReSize,
                BranchId = branch?.Id ?? 0,
                BranchName = branch?.Name,
                CreatedBy = data.CreatedBy,
                CreatedOn = data.CreatedOn,
                CustomerId = data.CustomerId,
                DOB = data.DOB,
                Email = data.Email,
                Extension = data.Extension,
                FullName = data.FullName,
                Gender = data.Gender,
                JobId = data.JobId,
                JobName = job?.Name,
                LearningNeedId = data.LearningNeedId,
                LearningNeedName = learningNeed?.Name,
                LearningStatus = data.LearningStatus,
                Mobile = data.Mobile,
                ModifiedBy = data.ModifiedBy,
                ModifiedOn = data.ModifiedOn,
                PurposeId = data.PurposeId,
                PurposeName = purpose?.Name,
                SaleId = data.SaleId,
                SaleName = sale?.FullName,
                SourceId = data.SourceId,
                SourceName = source?.Name,
                StatusId = data.StatusId,
                UserCode = data.UserCode,
            };

            var parent = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.ParentId);
            if (parent != null)
            {
                result.Parents = new StudentParents
                {
                    Address = parent.Address,
                    UserName = parent.UserName,
                    Email = parent.Email,
                    Extension = parent.Extension,
                    FullName = parent.FullName,
                    Gender = parent.Gender,
                    Mobile = parent.Mobile,
                    StatusId = parent.StatusId,
                    Avatar = parent.Avatar,
                    AvatarReSize = parent.AvatarReSize,
                };
            }

            var area = await dbContext.tbl_Area.SingleOrDefaultAsync(x => x.Id == data.AreaId);
            var district = await dbContext.tbl_District.SingleOrDefaultAsync(x => x.Id == data.DistrictId);
            var ward = await dbContext.tbl_Ward.SingleOrDefaultAsync(x => x.Id == data.WardId);
            result.Address = new StudentAddress
            {
                Address = data.Address,
                AreaId = data.AreaId,
                AreaName = area?.Name,
                DistrictId = data.DistrictId,
                DistrictName = district?.Name,
                WardId = data.WardId,
                WardName = ward?.Name
            };
            result.Acount = new StudentAcount
            {
                UserName = data.UserName
            };
            return result;
        }
        /// <summary>
        /// Mỗi học viên chỉ thuộc chi nhánh duy nhất
        /// Admin có thể xem tất cả 
        /// Học viên chỉ xem được thông tin của mình 
        /// Phụ huynh có thể xem được thông tin của con mình 
        /// Sale chỉ xem được thông tin học viên mình hỗ trợ
        /// Các nhân viên khác có thể xem thông tin học viên thuộc chi nhánh của mình
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="studentId"></param>
        /// <param name="branchId"></param>
        /// <returns></returns>
        public async Task<bool> HasPermission(tbl_UserInformation currentUser, int studentId, int branchId)
        {

            var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentId);
            if (student != null)
            {
                int.TryParse(student.BranchIds.Trim(), out branchId);
            }

            if (currentUser.RoleId == ((int)RoleEnum.admin))
                return true;
            else if (currentUser.RoleId == ((int)RoleEnum.student))
            {
                if (currentUser.UserInformationId != studentId)
                    return false;
            }
            else if (currentUser.RoleId == ((int)RoleEnum.parents))
            {
                if (student.ParentId != currentUser.UserInformationId)
                    return false;
            }
            else if (currentUser.RoleId == ((int)RoleEnum.sale))
            {
                if (student != null)
                {
                    if (student.SaleId != currentUser.UserInformationId)
                        return false;
                }

                if (string.IsNullOrEmpty(currentUser.BranchIds))
                    return false;
                var branchIds = currentUser.BranchIds.Split(',');
                if (!branchIds.Any(x => x == branchId.ToString()))
                    return false;
            }
            else
            {
                if (string.IsNullOrEmpty(currentUser.BranchIds))
                    return false;
                var branchIds = currentUser.BranchIds.Split(',');
                if (!branchIds.Any(x => x == branchId.ToString()))
                    return false;
            }
            return true;
        }
        //public async Task<StudentDetailDTO> Insert(StudentDetailPost itemModel, tbl_UserInformation currentUser)
        //{
        //    using (var tran = await dbContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            if (itemModel == null)
        //                throw new Exception("không tìm thấy dữ liệu");
        //            string password = "";
        //            if (!string.IsNullOrEmpty(itemModel.Account.UserName))
        //            {
        //                if (string.IsNullOrEmpty(itemModel.Account.Password))
        //                    throw new Exception("Vui lòng nhập mật khẩu");
        //                await userinformationService.ValidUserName(itemModel.Account.UserName, 0);
        //                password = Encryptor.Encrypt(itemModel.Account.Password);
        //            }
        //            if (!string.IsNullOrEmpty(itemModel.Information.Mobile))
        //            {
        //                if (!CoreContants.IsValidPhoneNumber(itemModel.Information.Mobile))
        //                    throw new Exception("Số điện thoại không hợp lệ");
        //            }
        //            if (!string.IsNullOrEmpty(itemModel.Information.Email))
        //            {
        //                if (!CoreContants.IsValidEmail(itemModel.Information.Email))
        //                    throw new Exception("Email không hợp lệ");
        //            }
        //            var hasBranch = await dbContext.tbl_Branch.AnyAsync(x => x.Id == itemModel.Information.BranchId);
        //            if (!hasBranch)
        //                throw new Exception("Không tìm thấy chi nhánh");
        //            if (itemModel.Information.SaleId.HasValue)
        //            {
        //                var hasSale = await dbContext.tbl_UserInformation.AnyAsync(x => x.UserInformationId == itemModel.Information.SaleId);
        //                if (!hasSale)
        //                    throw new Exception("Không tìm thấy tư vấn viên");
        //            }
        //            var entity = new tbl_UserInformation
        //            {
        //                ActiveDate = DateTime.Now,
        //                Address = itemModel.Address.Address,
        //                AreaId = itemModel.Address.AreaId,
        //                Avatar = itemModel.Information.Avatar,
        //                AvatarReSize = itemModel.Information.AvatarReSize,
        //                BankAccountName = "",
        //                BankAccountNumber = "",
        //                BankBranch = "",
        //                BankName = "",
        //                BranchIds = itemModel.Information.BranchId.ToString(),
        //                CommissionConfigId = 0,
        //                CreatedBy = currentUser.FullName,
        //                CreatedDateKeyForgot = null,
        //                CreatedOn = DateTime.Now,
        //                CustomerId = 0,
        //                DistrictId = itemModel.Address.DistrictId,
        //                DOB = itemModel.Information.DOB,
        //                Email = itemModel.Information.Email,
        //                Enable = true,
        //                Extension = itemModel.Information.Extension,
        //                FullName = itemModel.Information.FullName,
        //                Gender = itemModel.Information.Gender,
        //                IsReceiveMailNotification = false,
        //                JobId = itemModel.Information.JobId,
        //                KeyForgotPassword = null,
        //                LearningNeedId = itemModel.Information.LearningNeedId,
        //                LearningStatus = itemModel.Information.LearningStatus,
        //                LearningStatusName = itemModel.Information.LearningStatusName,
        //                Mobile = itemModel.Information.Mobile,
        //                ModifiedBy = currentUser.FullName,
        //                ModifiedOn = DateTime.Now,
        //                OneSignal_DeviceId = null,
        //                ParentId = 0,
        //                Password = password,
        //                PurposeId = itemModel.Information.PurposeId,
        //                RefreshToken = null,
        //                RefreshTokenExpires = null,
        //                RoleId = ((int)RoleEnum.student),
        //                RoleName = CoreContants.GetRoleName(((int)RoleEnum.student)),
        //                SaleId = itemModel.Information.SaleId ?? 0,
        //                SourceId = itemModel.Information.SourceId,
        //                StatusId = ((int)AccountStatus.active),
        //                UserCode = await InitCode(),
        //                UserName = itemModel.Account.UserName,
        //                WardId = itemModel.Address.WardId,
        //            };
        //            await dbContext.tbl_UserInformation.AddAsync(entity);
        //            await dbContext.SaveChangesAsync();
        //            //Phần này FE cần check thêm nếu username đã tồn tại thì hỏi lại có phải thông tin này không
        //            //Nếu đã có thì dùng lại thông tin đó, còn không phải bắt nhập username mới
        //            if (itemModel.Parents != null)
        //            {
        //                var parent = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => 
        //                                !string.IsNullOrEmpty(x.UserName)
        //                                && !string.IsNullOrEmpty(itemModel.Parents.UserName)
        //                                && x.UserName.ToUpper() == itemModel.Parents.UserName.ToUpper());
        //                if (parent != null)
        //                {
        //                    string parentsPassword = parent.Password;
        //                    if (!string.IsNullOrEmpty(itemModel.Parents.Password))
        //                    {
        //                        parentsPassword = Encryptor.Encrypt(itemModel.Parents.Password);
        //                    }
        //                    parent.Password = parentsPassword;
        //                    parent.FullName = itemModel.Parents.FullName ?? parent.FullName;
        //                    parent.Mobile = itemModel.Parents.Mobile ?? parent.Mobile;
        //                    parent.Email = itemModel.Parents.Email ?? parent.Email;
        //                    parent.Address = itemModel.Parents.Address ?? parent.Address;
        //                    parent.Extension = itemModel.Parents.Extension ?? parent.Extension;
        //                    await dbContext.SaveChangesAsync();
        //                }
        //                else
        //                {
        //                    string parentsPassword = "";
        //                    if (!string.IsNullOrEmpty(itemModel.Parents.Password))
        //                    {
        //                        parentsPassword = Encryptor.Encrypt(itemModel.Parents.Password);
        //                    }
        //                    var parents = new tbl_UserInformation
        //                    {
        //                        ActiveDate = DateTime.Now,
        //                        Address = "",
        //                        AreaId = 0,
        //                    };
        //                }
        //            }
        //            await tran.CommitAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            await tran.RollbackAsync();
        //            throw new Exception(e.Message);
        //        }
        //    }
        //}
        private async Task<string> InitCode()
        {
            string baseCode = "HV";
            var now = DateTime.Now;
            int count = await dbContext.tbl_UserInformation.CountAsync(x => (
                    x.RoleId == ((int)RoleEnum.student)
                    && x.CreatedOn.Value.Year == now.Year
                    && x.CreatedOn.Value.Month == now.Month
                    && x.CreatedOn.Value.Day == now.Day));
            return AssetCRM.InitCode(baseCode, now, count + 1);
        }
        //assign to sale
    }
}
