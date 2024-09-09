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

namespace LMSCore.Services.Discount
{
    public class DiscountService
    {
        public static async Task<tbl_Discount> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Discount.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_Discount> GetByCode(string code)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Discount.FirstOrDefaultAsync(x => x.Code.ToUpper() == code.ToUpper());
                if (data == null)
                    throw new Exception("Mã khuyến mãi không phù hợp");
                if (data.Status == 2)
                    throw new Exception("Khuyến mãi đã hết hạn");
                if (data.Quantity <= data.UsedQuantity)
                    throw new Exception("Khuyến mãi đã dùng hết");

                return data;
            }
        }
        public static async Task<tbl_Discount> Insert(DiscountCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var checkCode = await db.tbl_Discount.AnyAsync(x => x.Code.ToUpper() == itemModel.Code.ToUpper() && x.Enable == true);
                if (checkCode)
                    throw new Exception("Mã đã tồn tại");
                if (AssetCRM.CheckUnicode(itemModel.Code))
                    throw new Exception("Mã không hợp lệ");
                var model = new tbl_Discount(itemModel);
                if (model.Type == 1)
                    model.MaxDiscount = model.Value;
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_Discount.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }

        public class LessonOver
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            public bool IsMonthly { get; set; }
            public double Discount { get; set; } = 0;
            public int CompletedLesson { get; set; } = 0;
            public int TotalLesson { get; set; } = 0;
        }
        public static async Task<List<LessonOver>> GenerateVoucher(List<int> ClassIds, lmsDbContext db)
        {
            List<LessonOver> result = new List<LessonOver>();
            if (ClassIds == null || ClassIds.Count == 0)
                return result;
            var details = await db.tbl_Class.Where(x => ClassIds.Contains(x.Id)).ToListAsync();
            if (details != null && details.Count > 0)
            {
                //Tính tiền giảm đối với lớp bth và số buổi đã qua trong tháng với lớp theo tháng
                foreach (var detail in details)
                {
                    LessonOver currentClass = new LessonOver() { ClassId = detail.Id, ClassName = detail.Name, IsMonthly = detail.IsMonthly };
                    var schedules = await db.tbl_Schedule.Where(x => x.ClassId == detail.Id && x.Enable == true).ToListAsync();
                    if (!schedules.Any())
                        continue;
                    int totalSchedule = schedules.Count;
                    double reduce = 0;
                    if (detail.IsMonthly) // gói theo tháng
                    {
                        //lấy ngày bắt đầu tháng gần nhất
                        var now = DateTime.Now;
                        var day = detail.StartDay.Value.Day;
                        var year = now.Year;
                        var month = day < now.Day ? now.Month : now.Month - 1;
                        if (month < 1)
                        {
                            month = 12;
                            year--;
                        };
                        var startDay = new DateTime(year, month, day);

                        //lấy những buổi học đã hoàn thành trong tháng này
                        var schedulesInMonth = schedules.Count(x => startDay <= x.StartTime && x.StartTime <= now);
                        var schedulesCompleted = schedules.Count(x => startDay <= x.StartTime && x.StartTime <= now && x.TeacherAttendanceId != 0);

                        if (schedulesInMonth != 0)
                            reduce = detail.Price * schedulesCompleted / schedulesInMonth;
                        currentClass.CompletedLesson = schedulesCompleted;
                        currentClass.Discount = reduce;
                        currentClass.TotalLesson = schedulesCompleted;
                    }
                    else //gói bình thường
                    {
                        //lấy những buổi học đã hoàn thành
                        int completedCount = schedules.Count(x => x.TeacherAttendanceId != 0);

                        //tính tiền giảm dựa trên số buổi đã hoàn thành
                        reduce = detail.Price * completedCount / totalSchedule;
                        currentClass.CompletedLesson = completedCount;
                        currentClass.Discount = reduce;
                        currentClass.TotalLesson = totalSchedule;
                    }
                    result.Add(currentClass);
                    reduce = 0;
                }
            }
            return result;
        }

        public static async Task<tbl_Discount> Update(DiscountUpdate itemModel, tbl_UserInformation user)
        {
            DateTime today = DateTime.Now;
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Discount.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity.Status == 2)
                {
                    throw new Exception("Khuyến mãi đã kết thúc");
                }

                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.MaxDiscount = itemModel.MaxDiscount ?? entity.MaxDiscount;
                entity.Value = itemModel.Value ?? entity.Value;
                if (entity.Type == 1)
                    entity.MaxDiscount = entity.Value;

                entity.Status = itemModel.Status ?? entity.Status;
                entity.StatusName = itemModel.StatusName ?? entity.StatusName;
                entity.Note = itemModel.Note ?? entity.Note;
                if (entity.Status == 2)
                {
                    entity.Expired = today;
                    entity.StatusName = "Đã kết thúc";
                }
                else
                {
                    entity.Expired = itemModel.Expired ?? entity.Expired;
                    if (entity.Expired < today)
                    {
                        entity.Expired = today;
                        entity.Status = 2;
                        entity.StatusName = "Đã kết thúc";
                    }
                }
                entity.Quantity = itemModel.Quantity ?? entity.Quantity;
                if (entity.Quantity == entity.UsedQuantity)
                {
                    entity.Status = 2;
                    entity.StatusName = "Đã kết thúc";
                }
                if (entity.Quantity < entity.UsedQuantity)
                {
                    throw new Exception("Số lượng discount không thể nhỏ hơn số lượng discount đã sử dụng");
                }
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = today;
                entity.BranchIds = itemModel.BranchIds ?? entity.BranchIds;

                await db.SaveChangesAsync();

                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Discount.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(DiscountSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new DiscountSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;
                string sql = $"Get_Discount @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Status = {baseSearch.Status}," +
                    $"@PackageType = {baseSearch.PackageType ?? 0}," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@Code = N'{baseSearch.Code ?? ""}'";
                var data = await db.SqlQuery<Get_Discount>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Discount(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        
    }
}