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
using static LMSCore.Services.Class.ClassService;

namespace LMSCore.Services.ClassReserve
{
    public class ClassReserveService
    {
        public static async Task<tbl_ClassReserve> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_ClassReserve.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }

        public static async Task<ReserveProvi> GetCalc(OldClassSearch request, lmsDbContext db)
        {
            try
            {
                var result = new ReserveProvi();
                var data = await ClassChangeService.GetOldClass(request, db);
                result.TotalLesson = data.TotalLesson;
                result.CompletedLesson = data.CompletedLesson;

                int total = result.TotalLesson ?? 0;
                int completed = result.CompletedLesson ?? 0;
                double percent = total == 0 ? 0 : (double)(total - completed) / total;
                result.Paid = Math.Round(data.Price * percent);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static async Task<ReserveProvi> PaidCalc(OldClassSearch request)
        {
            using (var db = new lmsDbContext())
            {
                return await GetCalc(request, db);
            }
        }
        public static async Task<tbl_ClassReserve> Insert(ClassReserveCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_ClassReserve(itemModel);
                var studentInClass = await db.tbl_StudentInClass.FirstOrDefaultAsync(x => x.Id == itemModel.StudentInClassId && x.Enable == true);
                if (studentInClass == null)
                    throw new Exception("Không tìm thấy học viên trong lớp");
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == studentInClass.ClassId);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");
                if (_class.Status == 3)
                    throw new Exception("Lớp đã kết thúc không thể bảo lưu");
                model.StudentId = studentInClass.StudentId;
                model.ClassId = _class.Id;
                model.BranchId = _class.BranchId;

                //tính lại số buổi bảo lưu
                //var result = await GetCalc(new OldClassSearch { StudentInClassId = itemModel.StudentInClassId }, db);
                //if (result != null)
                //{
                //    model.LessonRemaining = result.TotalLesson - result.CompletedLesson;
                //}
                model.MoneyRemaining = model.Price ?? 0;
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_ClassReserve.Add(model);
                studentInClass.Enable = false;
                await db.SaveChangesAsync();

                // thêm lịch sử học viên  
                var learningHistoryService = new LearningHistoryService(db);
                await learningHistoryService.Insert(new LearningHistoryCreate
                {
                    StudentId = studentInClass.StudentId,
                    Content = $"Học viên bảo lưu"
                });

                return model;
            }
        }
        public static async Task<tbl_ClassReserve> Update(ClassReserveUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_ClassReserve.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Note = itemModel.Note ?? entity.Note;
                entity.Expires = itemModel.Expires ?? entity.Expires;
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
                var entity = await db.tbl_ClassReserve.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }

        //public static async Task AddToClassFromReserve(AddToClassFromReserveModel model, tbl_UserInformation user, lmsDbContext db)
        //{
        //    var classReserve = await db.tbl_ClassReserve.FirstOrDefaultAsync(x => x.Id == model.ClassReserveId && x.Enable == true);
        //    if (classReserve == null)
        //        throw new Exception("Không tìm thấy thông tin bảo lưu");
        //    if (classReserve.Status != 1)
        //    {
        //        string mess = "";
        //        switch (classReserve.Status)
        //        {
        //            case 2: mess = "Học viên đã học lại"; break;
        //            case 3: mess = "Đã hoàn tiền cho học viên"; break;
        //            case 4: mess = "Dã hết hạn bảo lưu"; break;
        //        }
        //        throw new Exception(mess);
        //    }

        //    var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == model.ClassId);
        //    if (_class == null)
        //        throw new Exception("Không tìm thấy lớp học");
        //    if (_class.Status == 3)
        //        throw new Exception("Lớp học đã kết thúc");

        //    var countStudentInClass = await db.tbl_StudentInClass.CountAsync(x => x.ClassId == _class.Id && x.Enable == true);
        //    if (_class.MaxQuantity < countStudentInClass)
        //        throw new Exception("Lớp không đủ chỗ");

        //    var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == classReserve.StudentId);
        //    if (student == null)
        //        throw new Exception("Không tìm thấy học viên");

        //    var checkExist = await db.tbl_StudentInClass.AnyAsync(x => x.StudentId == classReserve.StudentId && x.ClassId == _class.Id && x.Enable == true);
        //    if (checkExist)
        //        throw new Exception($"Học viên {student.FullName} đã có trong lớp {_class.Name}");

        //    //thanh toán đăng ký học
        //    BillCreate request = new BillCreate();
        //    request.StudentId = student.UserInformationId;
        //    request.PaymentMethodId = model.PaymentMethodId;
        //    request.PaymentAppointmentDate = model.PaymentAppointmentDate;
        //    request.BranchId = model.BranchId;
        //    request.Note = model.Note;
        //    request.Paid = model.Paid;
        //    request.Type = 7;
        //    BillDetailCreate detailItem = new BillDetailCreate { ClassId = _class.Id };
        //    request.Details = new List<BillDetailCreate> { detailItem };
        //    //tính tiền phải trả
        //    request.Price = _class.Price - (classReserve.Price ?? 0);

        //    //tạo mã giảm giá từ số tiền giảm
        //    if (classReserve.Price.HasValue && classReserve.Price.Value > 0)
        //    {
        //        tbl_Discount discount = new tbl_Discount()
        //        {
        //            Code = $"REDUCEBILL_{classReserve.Id}",
        //            Type = 1,
        //            TypeName = "Giảm tiền",
        //            PackageType = 1,
        //            PackageTypeName = "Gói lẻ",
        //            Value = classReserve.Price.Value,
        //            Status = 1,
        //            Note = $"Voucher giảm tiền các buổi chưa học",
        //            Expired = DateTime.Now.AddMinutes(30),
        //            Quantity = 1,
        //            UsedQuantity = 0,
        //            MaxDiscount = classReserve.Price.Value,
        //            Enable = true,
        //            CreatedOn = DateTime.Now,
        //            CreatedBy = user.FullName,
        //        };
        //        db.tbl_Discount.Add(discount);
        //        await db.SaveChangesAsync();
        //        request.DiscountId = discount.Id;
        //    }
        //    //gọi hàm tạo bill 
        //    await BillService.Insert(request, user, db);
        //    classReserve.Status = 2;
        //    classReserve.StatusName = "Đã học lại";
        //    // thêm lịch sử học viên  
        //    var learningHistoryService = new LearningHistoryService(db);
        //    await learningHistoryService.Insert(new LearningHistoryCreate
        //    {
        //        StudentId = student.UserInformationId,
        //        Content = $"Học viên bảo lưu chuyển vào lớp {_class.Name}"
        //    }, user);
        //    //
        //    await db.SaveChangesAsync();
        //}
        public static async Task<AppDomainResult> GetAll(ClassReserveSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ClassReserveSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;
                int mySaleId = 0;
                if (user.RoleId == (int)RoleEnum.sale)
                    mySaleId = user.UserInformationId;
                string sql = $"Get_ClassReserve @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranchIds = N'{baseSearch.BranchIds}'," +
                    $"@Status = N'{baseSearch.Status}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@SaleId = N'{mySaleId}'";
                var data = await db.SqlQuery<Get_ClassReserve>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_ClassReserve(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<ReviewReserve> GetReviewReserve(int studentInClassId)
        {
            using (var db = new lmsDbContext())
            {
                var studentInClass = await db.tbl_StudentInClass.SingleOrDefaultAsync(x => x.Id == studentInClassId);
                if (studentInClass == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == studentInClass.ClassId && x.Enable == true);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp học");

                var result = new ReviewReserve
                {
                    ClassId = _class.Id,
                    ClassName = _class.Name,
                    Price = _class.Price,
                    PaymentType = _class.PaymentType,
                };
                double forecastPrice = 0;
                DateTime now = DateTime.Now;
                if (_class.PaymentType == 1)
                {
                    int totalLesson = await db.tbl_Schedule.CountAsync(x => x.ClassId == _class.Id && x.Enable == true);
                    int remainingLesson = await db.tbl_Schedule.CountAsync(x => x.ClassId == _class.Id && x.Enable == true && x.StartTime >= now);
                    forecastPrice = Math.Round(_class.Price / totalLesson * remainingLesson, 0);
                    result.OnePaymentDetail = new OnePaymentDetail
                    {
                        TotalLesson = totalLesson,
                        RemainingLesson = remainingLesson,
                    };
                }
                else if (_class.PaymentType == 2)
                {
                    var monthlyTuitions = await db.tbl_MonthlyTuition.Where(x => x.ClassId == _class.Id && x.Enable == true && x.StudentId == studentInClass.StudentId).ToListAsync();
                    if (monthlyTuitions.Any())
                    {
                        now = new DateTime(now.Year, now.Month, 1);
                        int totalMonth = monthlyTuitions.Count();
                        int remainingMonth = 0;
                        foreach (var item in monthlyTuitions)
                        {
                            var month = new DateTime(item.Year, item.Month, 1);
                            if (month > now)
                            {
                                remainingMonth++;
                                var billDetail = await db.tbl_BillDetail.FirstOrDefaultAsync(x => x.BillId == item.BillId);
                                if (billDetail != null)
                                {
                                    forecastPrice += billDetail.Price;
                                }
                            }
                        }
                        result.MonthlyDetail = new MonthlyDetail
                        {
                            TotalMonth = totalMonth,
                            RemainingMonth = remainingMonth
                        };
                    }
                }
                result.ForecastPrice = Math.Round(forecastPrice, 0);
                return result;
            }
        }
    }
}