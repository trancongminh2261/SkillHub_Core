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
using LMSCore.Services.Bill;

namespace LMSCore.Services
{
    public class ClassChangeService
    {
        public static async Task<tbl_ClassChange> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_ClassChange.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }

        public static async Task<tbl_BillDetail> GetOldClass(OldClassSearch request, lmsDbContext db)
        {
            var studentInClass = await db.tbl_StudentInClass.FirstOrDefaultAsync(x => x.Id == request.StudentInClassId);
            if (studentInClass == null)
                throw new Exception("Không tìm thấy thông tin học viên trong lớp");

            var data = await db.tbl_BillDetail.SingleOrDefaultAsync(x => x.Enable == true && x.Id == studentInClass.BillDetailId);
            if (data == null)
            {
                var _class = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == studentInClass.ClassId);
                data = new tbl_BillDetail { ClassId = _class.Id};
                data.Price = _class != null ? _class.Price : 0;
            }
            var scheduleCount = db.tbl_Schedule.Where(x => x.Enable == true && x.ClassId == data.ClassId).AsQueryable();
            data.TotalLesson = await scheduleCount.CountAsync();
            data.CompletedLesson = await scheduleCount.Where(x => x.TeacherAttendanceId != 0).CountAsync();
            return data;
        }

        public static async Task<tbl_ClassChange> Insert(ClassChangeCreate itemModel, tbl_UserInformation user, lmsDbContext db, string baseUrl)
        {
            var model = new tbl_ClassChange(itemModel);
            var studentInClass = await db.tbl_StudentInClass.FirstOrDefaultAsync(x => x.Id == itemModel.StudentInClassId && x.Enable == true);
            if (studentInClass == null)
                throw new Exception("Không tìm thấy học viên trong lớp");

            var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentInClass.StudentId);
            if (student == null)
                throw new Exception("Không tìm thấy học viên");

            var oldClass = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == studentInClass.ClassId);
            if (oldClass == null)
                throw new Exception("Không tìm thấy lớp cũ");

            model.StudentId = studentInClass.StudentId;
            model.OldClassId = oldClass.Id;
            model.OldPrice = oldClass.Price;

            var newClass = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.NewClassId);
            if (newClass == null)
                throw new Exception("Không tìm thấy lớp mới");

            if (newClass.Status == 3)
                throw new Exception("Lớp học đã kết thúc");

            model.NewPrice = newClass.Price;
            model.CreatedBy = model.ModifiedBy = user.FullName;
            model.BranchId = newClass.BranchId;
            model.Paid = itemModel.Price;//Số tiền trả thêm
            db.tbl_ClassChange.Add(model);
            studentInClass.Enable = false;//xóa ra khỏi lớp cũ
            oldClass.MaxQuantity -= 1;
            await db.SaveChangesAsync();

            if (itemModel.Price > 0)
            {
                BillCreateV2 bill = new BillCreateV2
                {
                    StudentId = studentInClass.StudentId,
                    PaymentMethodId = itemModel.PaymentMethodId,
                    PaymentAppointmentDate = itemModel.PaymentAppointmentDate,
                    BranchId = itemModel.BranchId,
                    Note = itemModel.Note,
                    Paid = itemModel.Paid,
                    Price = itemModel.Price,
                    Type = 6,
                    Details = new List<BillDetailCreateV2> {
                    new BillDetailCreateV2 { ClassChangeId = model.Id } }
                };
                await BillService.InsertV2(bill, user, db, baseUrl);
            }

            var checkExist = await db.tbl_StudentInClass.AnyAsync(x => x.StudentId == model.StudentId && x.ClassId == newClass.Id && x.Enable == true);
            if (checkExist)
                throw new Exception($"Học viên đã có trong lớp {newClass.Name}");

            var countStudent = await db.tbl_StudentInClass.CountAsync(x => x.ClassId == newClass.Id && x.Enable == true);
            if (countStudent >= newClass.MaxQuantity)
                throw new Exception("Lớp đã đủ học viên không thể chuyển đến");

            var billDetail = await db.tbl_BillDetail.FirstOrDefaultAsync(x => x.ClassChangeId == model.Id);
            db.tbl_StudentInClass.Add(new tbl_StudentInClass
            {
                BillDetailId = billDetail?.Id ?? 0,
                BranchId = newClass.BranchId,
                ModifiedBy = user.FullName,
                ClassId = newClass.Id,
                CreatedBy = user.FullName,
                CreatedOn = DateTime.Now,
                Enable = true,
                ModifiedOn = DateTime.Now,
                Type = 1,
                TypeName = "Chính thức",
                Note = $"Chuyển lớp từ {oldClass.Name} đến {newClass.Name}",
                StudentId = model.StudentId,
            });
            student.LearningStatus = 5;
            student.LearningStatusName = "Đang học";

            // thêm lịch sử học viên  
            var learningHistoryService = new LearningHistoryService(db);
            await learningHistoryService.Insert(new LearningHistoryCreate
            {
                StudentId = student.UserInformationId,
                Content = $"Chuyển lớp từ {oldClass.Name} đến {newClass.Name}"
            });
            await db.SaveChangesAsync();

            return model;
        }

        public static async Task<AppDomainResult> GetAll(ClassChangeSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ClassChangeSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                int mySaleId = 0;
                if (user.RoleId == ((int)RoleEnum.sale))
                    mySaleId = user.UserInformationId;
                string sql = $"Get_ClassChange @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranchIds = N'{baseSearch.BranchIds}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@SaleId = N'{mySaleId}'";
                var data = await db.SqlQuery<Get_ClassChange>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_ClassChange(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

    }
}