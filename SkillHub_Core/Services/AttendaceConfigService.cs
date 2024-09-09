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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace LMSCore.Services
{
    public class AttendaceConfigService
    {
        public static async Task<tbl_AttendaceConfig> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_AttendaceConfig.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                    data.AttendaceTypeModel = GetAttendaceType(data.AttendanceTypeIds);
                return data;
            }
        }
        public static async Task<tbl_AttendaceConfig> GetAttendaceConfigActive()
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_AttendaceConfig.SingleOrDefaultAsync(x => x.Enable == true && x.Active == true);
                if (data != null)
                    data.AttendaceTypeModel = GetAttendaceType(data.AttendanceTypeIds);
                return data;
            }
        }
        public static async Task<tbl_AttendaceConfig> Insert(AttendaceConfigCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (itemModel.Active)
                {
                    var check = await db.tbl_AttendaceConfig.AnyAsync(x => x.Active == itemModel.Active && x.Enable == true);
                    if (check)
                        throw new Exception("Đã có điểm danh khác đang hoạt động !");
                }
                var attendanceTypeModel = new List<tbl_AttendaceType>();
                string attendanceTypeIds = string.Empty;
                if (itemModel.AttendanceTypes.Any())
                {
                    // tạo type điểm danh 
                    foreach (var type in itemModel.AttendanceTypes)
                    {
                        var insType = await InsertType(type, user);
                        attendanceTypeModel.Add(insType);
                        attendanceTypeIds += (string.IsNullOrEmpty(attendanceTypeIds) ? insType.Id : "," + insType.Id);
                    }
                }
                else
                    throw new Exception("Vui lòng chọn loại điểm danh!");
                var model = new tbl_AttendaceConfig(itemModel);
                model.AttendanceTypeIds = attendanceTypeIds;
                model.AttendaceTypeModel = attendanceTypeModel.OrderBy(x => x.Id).ToList();
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_AttendaceConfig.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_AttendaceType> InsertType(AttendaceTypeCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_AttendaceType(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_AttendaceType.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_AttendaceConfig> Update(AttendaceConfigUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_AttendaceConfig.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (itemModel.Active ?? false)
                {
                    var check = await db.tbl_AttendaceConfig.AnyAsync(x => x.Active == itemModel.Active && x.Enable == true && x.Id != entity.Id);
                    if (check)
                        throw new Exception("Đã có điểm danh khác đang hoạt động !");
                }
                string attendanceTypeIds = string.Empty;
                var attendanceTypeModel = new List<tbl_AttendaceType>();
                if (itemModel.AttendanceTypes == null) itemModel.AttendanceTypes = new List<AttendaceTypeUpdate>();
                if (itemModel.AttendanceTypes.Any())
                {
                    var attendanceTypes = new List<tbl_AttendaceType>();
                    foreach (var item in itemModel.AttendanceTypes)
                    {
                        var dataItem = await db.tbl_AttendaceType.FirstOrDefaultAsync(x => x.Enable == true && x.Id == item.Id);
                        if (dataItem != null)
                            attendanceTypes.Add(dataItem);
                    }
                    // Lấy loại điểm danh cũ
                    var oldAttendanceType = await db.tbl_AttendaceType.Where(x => ("," + entity.AttendanceTypeIds + ",").Contains("," + x.Id + ",")).ToListAsync();
                    // Xóa type 
                    var delAttendanceType = oldAttendanceType.Except(attendanceTypes).ToList();
                    if (delAttendanceType.Any())
                        foreach (var del in delAttendanceType)
                            await DeleteType(del.Id);
                    // Cập nhật
                    var upAttendanceType = itemModel.AttendanceTypes.Where(x => x.Id != 0).ToList();
                    if (upAttendanceType.Any())
                        foreach (var up in upAttendanceType)
                        {
                            var update = await UpdateType(up, user);
                            attendanceTypeModel.Add(update);
                        }
                    // Thêm mới
                    var inAttendanceType = itemModel.AttendanceTypes.Where(x => x.Id == 0).ToList();
                    if (inAttendanceType.Any())
                        foreach (var insert in inAttendanceType)
                        {
                            var ins = await InsertType(new AttendaceTypeCreate()
                            {
                                TypeId = insert.TypeId,
                                TimeAttendace = insert.TimeAttendace,
                            }, user);
                            insert.Id = ins.Id;
                            attendanceTypeModel.Add(ins);
                        }
                    attendanceTypeIds += string.Join(",", upAttendanceType.Select(x => x.Id));
                    attendanceTypeIds += string.IsNullOrEmpty(attendanceTypeIds) ? "" : ",";
                    attendanceTypeIds += string.Join(",", inAttendanceType.Select(x => x.Id));
                }
                else
                {
                    attendanceTypeIds = entity.AttendanceTypeIds;
                    attendanceTypeModel = GetAttendaceType(entity.AttendanceTypeIds);
                }
                entity.Name = itemModel.Name ?? entity.Name;
                entity.Description = itemModel.Description ?? entity.Description;
                entity.AttendanceTypeIds = attendanceTypeIds;
                entity.Active = itemModel.Active ?? entity.Active;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                entity.AttendaceTypeModel = attendanceTypeModel.OrderBy(x => x.Id).ToList();
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task<tbl_AttendaceType> UpdateType(AttendaceTypeUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_AttendaceType.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.TypeId = itemModel.TypeId;
                entity.TypeName = itemModel.TypeName;
                entity.TimeAttendace = itemModel.TimeAttendace;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_AttendaceConfig.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                if (!string.IsNullOrEmpty(entity.AttendanceTypeIds))
                {
                    var type = await db.tbl_AttendaceType.Where(x => ("," + entity.AttendanceTypeIds + ",").Contains("," + x.Id + ",")).ToListAsync();
                    if (type.Any())
                        foreach (var item in type)
                        {
                            await DeleteType(item.Id);
                        }
                }
                await db.SaveChangesAsync();
            }
        }
        public static async Task DeleteType(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_AttendaceType.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(AttendaceConfigSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new AttendaceConfigSearch();
                string sql = $"GetAttendaceConfig @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@AttendanceTypeIds = '{baseSearch.AttendanceTypeIds}'," +
                    $"@Search = N'{baseSearch.Search}'";
                if (baseSearch.Active != null)
                    sql += $", @Active = {baseSearch.Active}";
                var data = await db.SqlQuery<Get_AttendaceConfig>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_AttendaceConfig(i)).ToList();
                result.ForEach(async x => x.AttendaceTypeModel = GetAttendaceType(x.AttendanceTypeIds));
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
        public static List<tbl_AttendaceType> GetAttendaceType(string ids)
        {
            using (var db = new lmsDbContext())
            {
                var filterIds = "," + ids + ",";
                var result = db.tbl_AttendaceType.Where(x => filterIds.Contains("," + x.Id + ",")).ToList();
                return result;
            }
        }
        public class AttendaceTypeOption
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public static List<AttendaceTypeOption> GetAttendaceTypeOption()
        {
            using (var db = new lmsDbContext())
            {
                var data = lmsEnum.ListAttendaceType()
                    .Select(x => new AttendaceTypeOption
                    {
                        Id = x.Key,
                        Name = x.Value,
                    }).ToList();
                return data;
            }
        }
    }
}