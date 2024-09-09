using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.StudentDeviceDTO;
using LMSCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.DTO.StudentDeviceDTO.StudentDeviceDTO;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services.StudentDevice
{
    public class StudentDeviceService : DomainService
    {
        private IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private readonly IWebHostEnvironment _hostingEnvironment;
        public StudentDeviceService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public static async Task<tbl_Config> GetDeviceConfig()
        {
            using (var db = new lmsDbContext())
            {
                var deviceConfig = await db.tbl_Config.FirstOrDefaultAsync(x => x.Code == "AllowDeviceLimit");
                if (deviceConfig == null) return null;
                return deviceConfig;
            }
        }

        public static async Task<List<DeviceConfigTypeDTO>> GetDeviceConfigStatus()
        {
            var deviceConfig = new List<DeviceConfigTypeDTO>()
            {
                new DeviceConfigTypeDTO
                {
                    Id = (int)AllowRegister.Allow,
                    Name = GetAllowRegister((int)AllowRegister.Allow)
                },
                new DeviceConfigTypeDTO
                {
                    Id = (int)AllowRegister.UnAllow,
                    Name = GetAllowRegister((int)AllowRegister.UnAllow)
                }
            };
            return deviceConfig;
        }

        public static async Task<tbl_Config> UpdateDeviceConfig(DeviceConfigUpdate itemModel)
        {
            using (var db = new lmsDbContext())
            {
                var deviceConfig = await db.tbl_Config.FirstOrDefaultAsync(x => x.Id == itemModel.Id);
                if (deviceConfig == null)
                    throw new Exception("Không tìm thấy thông tin!");
                if (itemModel.Allowed.HasValue)
                {
                    deviceConfig.Value = GetAllowRegister(itemModel.Allowed);
                    if (deviceConfig.Value == null)
                        throw new Exception("Vui lòng chọn đúng trạng thái!");
                }
                deviceConfig.Quantity = itemModel.Quantity ?? deviceConfig.Quantity;
                await db.SaveChangesAsync();

                return deviceConfig;
            }
        }

        public static async Task<AppDomainResult> GetAll(GetStudentDeviceLimitSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_StudentDevice @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                        $"@PageSize = {baseSearch.PageSize}," +
                        $"@StudentId = N'{baseSearch.StudentId ?? 0}'," +
                        $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'";
                var data = await db.SqlQuery<Get_StudentDevice>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                return new AppDomainResult { TotalRow = totalRow, Data = data };
            }
        }

        public static async Task Insert(StudentDeviceCreate itemModel)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    if (itemModel.RoleId == (int)RoleEnum.student)
                    {
                        var entity = new tbl_StudentDevice
                        {
                            UserId = itemModel.UserId,
                            UserName = itemModel.UserName,
                            RoleId = itemModel.RoleId,
                            DeviceName = itemModel.DeviceName,
                            OS = itemModel.OS,
                            Browser = itemModel.Browser,
                            Allowed = true,
                            Enable = true,
                            CreatedBy = itemModel.UserName,
                            CreatedOn = DateTime.Now,
                            ModifiedBy = itemModel.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        var studentDevice = await db.tbl_StudentDevice.Where(x => x.Enable == true && x.UserId == itemModel.UserId && x.Allowed.HasValue).ToListAsync();
                        var studentDeviceLimit = await db.tbl_Config.FirstOrDefaultAsync(x => x.Code == "AllowDeviceLimit");
                        if (studentDeviceLimit != null && studentDeviceLimit.Value == "Allow")
                        {
                            var checkDeviceAllow = studentDevice.Any(x => !string.IsNullOrEmpty(x.DeviceName) && x.DeviceName.ToUpper().Trim() == itemModel.DeviceName.ToUpper().Trim() && x.Allowed == true);
                            if (!checkDeviceAllow)
                            {
                                if (studentDevice.Where(x => x.Allowed == true).Count() >= studentDeviceLimit.Quantity)
                                {
                                    var checkDeviceUnAllow = studentDevice.Any(x => !string.IsNullOrEmpty(x.DeviceName) && x.DeviceName.ToUpper().Trim() == itemModel.DeviceName.ToUpper().Trim() && x.Allowed == false);
                                    if (!checkDeviceUnAllow)
                                    {
                                        entity.Allowed = false;
                                        await db.tbl_StudentDevice.AddAsync(entity);
                                        await db.SaveChangesAsync();
                                    }
                                    throw new Exception("Bạn đã đăng nhập quá số lượng thiết bị cho phép vui lòng liên hệ quản trị viên để được hỗ trợ!");
                                }
                                else if (studentDevice.Where(x => x.Allowed == true).Count() < studentDeviceLimit.Quantity)
                                {
                                    await db.tbl_StudentDevice.AddAsync(entity);
                                    await db.SaveChangesAsync();
                                }
                            }
                        }
                        else
                        {
                            await db.tbl_StudentDevice.AddAsync(entity);
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public static async Task<tbl_StudentDevice> Update(StudentDeviceUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var studentDevice = await db.tbl_StudentDevice.FirstOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (studentDevice == null)
                    throw new Exception("Không tìm thấy thông tin thiết bị của học viên!");
                studentDevice.Allowed = itemModel.Allowed ?? studentDevice.Allowed;
                studentDevice.ModifiedOn = DateTime.Now;
                studentDevice.ModifiedBy = user.FullName;
                await db.SaveChangesAsync();
                return studentDevice;
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var studentDevice = await db.tbl_StudentDevice.FirstOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (studentDevice == null)
                    throw new Exception("Không tìm thấy thông tin thiết bị của học viên!");
                studentDevice.Enable = false;
                await db.SaveChangesAsync();
            }
        }
    }
}
