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
using LMSCore.Services.PaymentSession;

namespace LMSCore.Services
{
    public class RefundService
    {
        public static async Task<tbl_Refund> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Refund.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_Refund> Insert(RefundCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var model = new tbl_Refund(itemModel);
                        var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                        if (branch == null)
                            throw new Exception("Không tìm thấy chi nhánh");
                        if (itemModel.Price <= 0)
                            throw new Exception("Số tiền không phù hợp");
                        var paymentMethod = await db.tbl_PaymentMethod.SingleOrDefaultAsync(x => x.Id == itemModel.PaymentMethodId);
                        if (paymentMethod == null)
                            throw new Exception("Hình thức thanh toán không phù hợp");
                        if (itemModel.Type == 2)
                        {
                            var classReserve = await db.tbl_ClassReserve.SingleOrDefaultAsync(x => x.Id == itemModel.ClassReserveId);
                            if (classReserve == null)
                                throw new Exception("Không tìm thấy thông tin bảo lưu");
                            if (classReserve.Status != 1)
                            {
                                string mess = "";
                                switch (classReserve.Status)
                                {
                                    case 2: mess = "Học viên đã học lại"; break;
                                    case 3: mess = "Đã hoàn tiền cho học viên"; break;
                                    case 4: mess = "Đã hết hạn bảo lưu"; break;
                                }
                                throw new Exception(mess);
                            }
                            classReserve.Status = 3;
                            classReserve.StatusName = "Đã hoàn tiền";
                            classReserve.MoneyRefund = itemModel.Price;
                            model.StudentId = classReserve.StudentId;
                            await db.SaveChangesAsync();
                        }
                        else if (itemModel.Type == 3)
                        {
                            var classRegistration = await db.tbl_ClassRegistration.SingleOrDefaultAsync(x => x.Id == itemModel.ClassRegistrationId);
                            if (classRegistration == null)
                                throw new Exception("Không tìm thấy thông tin chờ xếp lớp");
                            if (classRegistration.Status != 1)
                            {
                                string mess = "";
                                switch (classRegistration.Status)
                                {
                                    case 2: mess = "Đã xếp lớp"; break;
                                    case 3: mess = "Đã hoàn tiền cho học viên"; break;
                                }
                                throw new Exception(mess);
                            }
                            classRegistration.Status = 3;
                            classRegistration.StatusName = "Đã hoàn tiền";
                            model.StudentId = classRegistration.StudentId;
                            await db.SaveChangesAsync();
                        }
                        else if (itemModel.Type == 4)
                        {
                            var bill = await db.tbl_Bill.SingleOrDefaultAsync(x => x.Id == itemModel.BillId && x.Enable == true);
                            if (bill == null)
                                throw new Exception("Không tìm thấy thông tin thanh toán");

                            var checkRefund = await db.tbl_Refund
                                .AnyAsync(x => x.BillId == bill.Id && x.Status == 1 && x.Enable == true);
                            if (checkRefund)
                                throw new Exception($"Đã tạo yêu cầu hoàn tiền cho đơn hàng {bill.Code}, vui lòng đợi duyệt");

                            if (bill.Debt >= 0)
                                throw new Exception("Khách hàng không thánh toán dư");
                            double debt = -bill.Debt;
                            if (itemModel.Price > debt)
                                throw new Exception($"Khách hàng thanh toán dư {(String.Format("{0:0,0}", debt))} VNĐ, không thể hoàn quá số tiền này");
                            bill.Debt += itemModel.Price;
                            bill.Paid -= itemModel.Price;

                        }
                        else
                        {
                            var checkStudent = await db.tbl_UserInformation.AnyAsync(x => x.UserInformationId == itemModel.StudentId && x.Enable == true);
                            if (!checkStudent) 
                                throw new Exception("Không tìm thấy học viên");
                        }
                        model.CreatedBy = model.ModifiedBy = user.FullName;
                        db.tbl_Refund.Add(model);
                        await db.SaveChangesAsync();

                        // thêm lịch sử học viên  
                        var learningHistoryService = new LearningHistoryService(db);
                        await learningHistoryService.Insert(new LearningHistoryCreate
                        {
                            StudentId = itemModel.StudentId,
                            Content = $"Chờ hoàn: {(String.Format("{0:0,0}", itemModel.Price))}+VNĐ"
                        });
                        //
                        tran.Commit();
                        return model;
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
        }

        public static async Task<tbl_Refund> Update(RefundUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entity = await db.tbl_Refund.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                        if (entity == null)
                            throw new Exception("Không tìm thấy dữ liệu");
                        if(entity.Status == 2)
                            throw new Exception("Yêu cầu hoàn tiền đã duyệt");
                        if (entity.Status == 3)
                            throw new Exception("Yêu cầu hoàn tiền đã hủy");
                        if (itemModel.Status == 3) //Khi hủy sẽ cập nhật lại trạng thái
                        {
                            if (entity.Type == 2)
                            {
                                var classReserve = await db.tbl_ClassReserve.SingleOrDefaultAsync(x => x.Id == entity.ClassReserveId);
                                if (classReserve == null)
                                    throw new Exception("Không tìm thấy thông tin bảo lưu");
                                classReserve.Status = 1;
                                classReserve.StatusName = "Đang bảo lưu";
                                await db.SaveChangesAsync();
                            }
                            else if (entity.Type == 3)
                            {

                                var classRegistration = await db.tbl_ClassRegistration.SingleOrDefaultAsync(x => x.Id == entity.ClassRegistrationId);
                                if (classRegistration == null)
                                    throw new Exception("Không tìm thấy thông tin chờ xếp lớp");
                                classRegistration.Status = 1;
                                classRegistration.StatusName = "Chờ xếp lớp";
                                await db.SaveChangesAsync();
                            }
                            else if (entity.Type == 4)
                            {
                                var bill = await db.tbl_Bill.SingleOrDefaultAsync(x => x.Id == entity.BillId);
                                if (bill == null)
                                    throw new Exception("Không tìm thấy thông tin thanh toán");
                                bill.Debt -= entity.Price;
                                bill.Paid += entity.Price;
                                await db.SaveChangesAsync();
                            }
                        }
                        else if (itemModel.Status == 2)
                        {
                            db.tbl_PaymentSession.Add(new tbl_PaymentSession
                            {
                                BranchId = entity.BranchId,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                Type = 2,
                                TypeName = "Chi",
                                PaymentMethodId = entity.PaymentMethodId,
                                Reason = "Hoàn tiền",
                                UserId = entity.StudentId,
                                Note = entity.Note,
                                Value = entity.Price,
                                PrintContent = Task.Run(() => PaymentSessionService.GetPrintContent(
                                    2,
                                    entity.StudentId ?? 0,
                                    "Hoàn tiền",
                                    entity.Price,
                                    user.FullName
                                    )).Result,
                            });
                            await db.SaveChangesAsync();
                        }
                        entity.Status = itemModel.Status ?? entity.Status;
                        entity.StatusName = itemModel.StatusName ?? entity.StatusName;
                        entity.ModifiedBy = user.FullName;
                        entity.ModifiedOn = DateTime.Now;
                        await db.SaveChangesAsync();

                        // thêm lịch sử học viên  
                        var learningHistoryService = new LearningHistoryService(db);
                        await learningHistoryService.Insert(new LearningHistoryCreate
                        {
                            StudentId = entity.StudentId,
                            Content = $"Đã duyệt, hoàn lại: {(String.Format("{0:0,0}", entity.Price))} VNĐ"
                        });
                        //
                        tran.Commit();
                        return entity;
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
                    
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Refund.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public class RefundResult : AppDomainResult
        {
            public double TotalPrice { get; set; }
            public int AllState { get; set; }
            public int Opened { get; set; }
            public int Approved { get; set; }
            public int Canceled { get; set; }
        }
        public static async Task<RefundResult> GetAll(RefundSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new RefundSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                int mySaleId = 0;
                if (user.RoleId == ((int)RoleEnum.sale))
                    mySaleId = user.UserInformationId;
                string sql = $"Get_Refund @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranhIds = N'{baseSearch.BranchIds}'," +
                    $"@Status = N'{baseSearch.Status}'," +
                    $"@Type = N'{baseSearch.Type}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@SaleId = N'{mySaleId}',"+
                    $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'";
                var data = await db.SqlQuery<Get_Refund>(sql);
                if (!data.Any()) return new RefundResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Refund(i)).ToList();
                var totalPrice = data[0].TotalPrice;
                var approved = data[0].Approved;
                var allState = data[0].AllState;
                var opened = data[0].Opened;
                var canceled = data[0].Canceled;

                return new RefundResult
                { 
                    TotalRow = totalRow, 
                    Data = result,
                    TotalPrice = totalPrice,
                    AllState = allState,
                    Opened = opened,
                    Approved = approved,
                    Canceled = canceled
                };
            }
        }
        public static async Task<RefundStatus> GetRefundStatus(RefundStatusSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new RefundStatusSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                int mySaleId = 0;
                if (user.RoleId == ((int)RoleEnum.sale))
                    mySaleId = user.UserInformationId;
                string sql = $"Get_Refund @Search = N'{baseSearch.Search ?? ""}', @PageIndex = 1," +
                    $"@PageSize = 1," +
                    $"@BranhIds = N'{baseSearch.BranchIds}'," +
                    $"@Status = N''," +
                    $"@Type = N'{baseSearch.Type}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@SaleId = N'{mySaleId}'," +
                    $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'";
                var data = await db.SqlQuery<Get_Refund>(sql);

                if (!data.Any())
                    return new RefundStatus();
                return new RefundStatus
                {
                    AllState = data[0].AllState,
                    Approved = data[0].Approved,
                    Canceled = data[0].Canceled,
                    Opened = data[0].Opened
                };
            }
                
        }
    }
}