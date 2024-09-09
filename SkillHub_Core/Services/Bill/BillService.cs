using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Areas.Request.BillCreate;
using static LMSCore.Models.lmsEnum;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ICSharpCode.SharpZipLib.Core;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using LMSCore.Services.Class;
using Hangfire;
using LMSCore.Services.PaymentApprove;
using LMSCore.Services.PaymentSession;
using static LMSCore.Services.Class.ClassService;

namespace LMSCore.Services.Bill
{
    public class BillService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public BillService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Đăng ký gói học phí
        /// </summary>
        /// <returns></returns>
        public static async Task<tbl_Bill> RegisterTuitionPackage(RegisterTuitionPackageModel itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId && x.Enable == true);
                        if (student == null)
                            throw new Exception("Không tìm thấy học viên");
                        var tuitionPackage = await db.tbl_TuitionPackage.SingleOrDefaultAsync(x => x.Id == itemModel.TuitionPackageId);
                        if (tuitionPackage == null)
                            throw new Exception("Không tìm thấy gói");
                        var paymentmethod = await db.tbl_PaymentMethod.SingleOrDefaultAsync(x => x.Id == itemModel.PaymentMethodId);
                        if (paymentmethod == null)
                            throw new Exception("Không tìm thấy phương thức thanh toán");
                        var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                        if (branch == null)
                            throw new Exception("Không tìm thấy chi nhánh");
                        var _Class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.ClassId);
                        if (_Class == null)
                            throw new Exception("Không tìm thấy lớp");
                        if (_Class.PaymentType == 1)
                            throw new Exception("Không thể đóng trước cho lớp thanh toán một lần");

                        var model = new tbl_Bill(itemModel);
                        model.ModifiedBy = model.CreatedBy = userLog.FullName;
                        string baseCode = "B";
                        int count = await db.tbl_Bill.CountAsync(x =>
                                    x.CreatedOn.Value.Year == model.CreatedOn.Value.Year
                                    && x.CreatedOn.Value.Month == model.CreatedOn.Value.Month
                                    && x.CreatedOn.Value.Day == model.CreatedOn.Value.Day);
                        model.Code = AssetCRM.InitCode(baseCode, DateTime.Now, count + 1);
                        model.FullName = student?.FullName;
                        model.UserCode = student?.UserCode;
                        model.UserEmail = student?.Email;
                        model.UserPhone = student?.Mobile;

                        db.tbl_Bill.Add(model);
                        await db.SaveChangesAsync();
                        double totalPrice = _Class.Price * tuitionPackage.Months;
                        var billDetail = new tbl_BillDetail
                        {
                            BillId = model.Id,
                            ClassId = _Class.Id,
                            CreatedBy = userLog.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = true,
                            ModifiedBy = userLog.FullName,
                            ModifiedOn = DateTime.Now,
                            Quantity = tuitionPackage.Months,
                            Price = _Class.Price,
                            StudentId = student.UserInformationId,
                            TotalPrice = totalPrice
                        };
                        db.tbl_BillDetail.Add(billDetail);
                        await db.SaveChangesAsync();

                        var product = new ProductModel();
                        var proramClass = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == _Class.ProgramId);
                        product.ClassName = _Class.Name;
                        product.Price = totalPrice;
                        product.PriceMonth = _Class.Price;
                        product.ProgramName = proramClass?.Name;
                        product.Code = proramClass?.Code;
                        product.TotalMonth = tuitionPackage.Months;
                        model.Products.Add(product);

                        int monthlyTuitionStatus = 2;
                        var paymentAllow = await db.tbl_PaymentAllow.AnyAsync(x => x.UserId == userLog.UserInformationId && x.Enable == true);
                        if (!paymentAllow && userLog.RoleId != (int)RoleEnum.admin
                            && userLog.RoleId != (int)RoleEnum.accountant
                            && userLog.RoleId != (int)RoleEnum.manager
                            && itemModel.Paid > 0)
                        {
                            double money = itemModel.Paid;
                            itemModel.Paid = 0;
                            model.Paid = 0;
                            var paymentApprove = new tbl_PaymentApprove
                            {
                                BillId = model.Id,
                                CreatedBy = userLog.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = userLog.FullName,
                                ModifiedOn = DateTime.Now,
                                Money = money,
                                Note = itemModel.Note,
                                Status = 1,
                                StatusName = "Chờ duyệt",
                                UserId = userLog.UserInformationId
                            };
                            db.tbl_PaymentApprove.Add(paymentApprove);
                            monthlyTuitionStatus = 1;
                            await db.SaveChangesAsync();
                        }

                        double reduced = 0;
                        if (tuitionPackage.DiscountType == 1)
                        {
                            reduced = tuitionPackage.Discount;
                        }
                        else if (tuitionPackage.DiscountType == 2)
                        {
                            reduced = Math.Round(totalPrice * tuitionPackage.Discount / 100, 0);
                        }

                        double usedMoneyReserve = 0;
                        double debtTemp = totalPrice - (reduced + model.Paid);
                        if (itemModel.ClassReserveId != 0)// Sử dụng tiền bảo lưu để thanh toán
                        {
                            var classReserve = await db.tbl_ClassReserve.SingleOrDefaultAsync(x => x.Id == itemModel.ClassReserveId);
                            if (classReserve == null)
                                throw new Exception("Không tìm thấy thông tin bảo lưu");
                            if (classReserve.MoneyRemaining > debtTemp)
                            {
                                usedMoneyReserve = debtTemp;
                                classReserve.MoneyRemaining -= debtTemp;
                            }
                            else
                            {
                                usedMoneyReserve = classReserve.MoneyRemaining;
                                classReserve.MoneyRemaining = 0;
                                classReserve.Status = 2;
                                classReserve.StatusName = "Đã học lại";
                            }
                            classReserve.MoneyUsed += usedMoneyReserve;
                            await db.SaveChangesAsync();
                            model.UsedMoneyReserve = usedMoneyReserve;
                            model.ClassReserveId = classReserve.Id;
                        }

                        model.TotalPrice = totalPrice;
                        model.DiscountPrice = reduced;
                        model.Reduced = reduced;
                        model.Debt = totalPrice - (reduced + model.Paid + usedMoneyReserve);

                        if (model.Paid > 0)
                        {
                            string printContent = await PaymentSessionService.GetPrintContent(
                                    1,
                                    model.StudentId,
                                    $"Thanh toán {tbl_Bill.GetTypeName(model.Type)}",
                                    model.Paid,
                                    userLog.FullName,
                                    student.FullName,
                                    student.UserCode
                                    );
                            var paymentSession = new tbl_PaymentSession
                            {
                                BillId = model.Id,
                                BranchId = model.BranchId,
                                CreatedBy = userLog.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = userLog.FullName,
                                ModifiedOn = DateTime.Now,
                                Type = 1,
                                TypeName = "Thu",
                                PaymentMethodId = itemModel.PaymentMethodId,
                                Reason = $"Thanh toán {tbl_Bill.GetTypeName(model.Type)}",
                                UserId = model.StudentId,
                                Note = model.Note,
                                Value = model.Paid,
                                PrintContent = printContent,
                                PaymentDate = itemModel.PaymentDate,
                            };

                            db.tbl_PaymentSession.Add(paymentSession);
                            await db.SaveChangesAsync();
                            var sale = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.SaleId && x.Enable == true);
                            //nếu tìm thấy tư vấn viên
                            if (sale != null)
                            {
                                if (itemModel.Type == 1 || itemModel.Type == 3 || itemModel.Type == 5)
                                {
                                    tbl_ConsultantRevenue consultantRevenue = new tbl_ConsultantRevenue();
                                    consultantRevenue.SaleId = sale.UserInformationId;
                                    consultantRevenue.StudentId = model.StudentId;
                                    consultantRevenue.BillId = model.Id;
                                    consultantRevenue.PaymentSessionId = paymentSession.Id;
                                    consultantRevenue.TotalPrice = model.TotalPrice;
                                    consultantRevenue.AmountPaid = model.Paid;
                                    consultantRevenue.Enable = true;
                                    consultantRevenue.CreatedOn = DateTime.Now;
                                    consultantRevenue.ModifiedOn = DateTime.Now;
                                    consultantRevenue.CreatedBy = consultantRevenue.ModifiedBy = userLog.FullName;
                                    db.tbl_ConsultantRevenue.Add(consultantRevenue);
                                    await db.SaveChangesAsync();
                                }

                                // cái này là chiến dịch hoa hồng gì đó =)))
                                if (itemModel.Type == 1 || itemModel.Type == 3)
                                {
                                    tbl_SaleRevenue saleRevenue = new tbl_SaleRevenue();
                                    saleRevenue.SaleId = sale.UserInformationId;
                                    saleRevenue.BillId = model.Id;
                                    saleRevenue.Value = model.Paid;
                                    saleRevenue.CreatedBy = saleRevenue.ModifiedBy = userLog.FullName;
                                    db.tbl_SaleRevenue.Add(saleRevenue);
                                    //await SaleRevenueService.Insert(saleRevenue, user);
                                }
                            }
                            /*if (itemModel.Type == 1 || itemModel.Type == 3 || itemModel.Type == 5)
                            {
                                var sale = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.SaleId && x.Enable == true && x.RoleId == 5);
                                if (sale != null) // tìm thấy tư vấn viên
                                {
                                    tbl_SaleRevenue saleRevenue = new tbl_SaleRevenue();
                                    saleRevenue.SaleId = sale.UserInformationId;
                                    saleRevenue.BillId = model.Id;
                                    saleRevenue.Value = model.Paid;
                                    saleRevenue.CreatedBy = saleRevenue.ModifiedBy = userLog.FullName;
                                    db.tbl_SaleRevenue.Add(saleRevenue);
                                    //await SaleRevenueService.Insert(saleRevenue, user);
                                }
                            }*/
                        }

                        //Thêm học viên vào lớp
                        if (tuitionPackage.Months <= 0)
                            throw new Exception("Thời gian sửa dụng gói không phù hợp");
                        if (!_Class.StartDay.HasValue)
                            throw new Exception("Chưa cấu hình thời gian học không thể đăng ký");
                        DateTime startTime = _Class.StartDay.Value;
                        if (DateTime.Now > startTime)
                            startTime = DateTime.Now;
                        var hasStudentInClass = await db.tbl_StudentInClass.AnyAsync(x => x.StudentId == itemModel.StudentId
                         && x.ClassId == itemModel.ClassId && x.Enable == true);
                        if (!hasStudentInClass)
                        {
                            var countStudent = await db.tbl_StudentInClass.CountAsync(x => x.ClassId == _Class.Id && x.Enable == true);
                            if (countStudent >= _Class.MaxQuantity)
                                throw new Exception("Lớp đã đủ học viên");

                            db.tbl_StudentInClass.Add(new tbl_StudentInClass
                            {
                                BranchId = _Class.BranchId,
                                BillDetailId = billDetail.Id,
                                ClassId = _Class.Id,
                                StudentId = itemModel.StudentId,
                                Warning = false,
                                Note = "",
                                Type = 1,
                                TypeName = "Chính thức",
                                Enable = true,
                                CreatedBy = userLog.FullName,
                                CreatedOn = DateTime.Now,
                                ModifiedBy = userLog.FullName,
                                ModifiedOn = DateTime.Now,
                            });
                            student.LearningStatus = 5;
                            student.LearningStatusName = "Đang Học";
                            await db.SaveChangesAsync();

                        }
                        else
                        {
                            //Đăng ký tháng tiếp theo
                            var lastTuition = await db.tbl_MonthlyTuition
                                .Where(x => x.StudentId == student.UserInformationId && x.ClassId == _Class.Id && x.Enable == true)
                                .OrderByDescending(x => x.CreatedOn)
                                .FirstOrDefaultAsync();
                            if (lastTuition != null)
                            {
                                startTime = new DateTime(lastTuition.Year, lastTuition.Month, 01).AddMonths(1);
                            }
                        }
                        for (int i = 1; i <= tuitionPackage.Months; i++)
                        {
                            db.tbl_MonthlyTuition.Add(new tbl_MonthlyTuition
                            {
                                BillId = model.Id,
                                ClassId = _Class.Id,
                                CreatedBy = userLog.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = userLog.FullName,
                                ModifiedOn = DateTime.Now,
                                Month = startTime.Month,
                                Year = startTime.Year,
                                Status = monthlyTuitionStatus,
                                StatusName = monthlyTuitionStatus == 2 ? "Đã thanh toán" : monthlyTuitionStatus == 1 ? "Chưa thanh toán" : "",
                                StudentId = student.UserInformationId,
                            });
                            startTime = startTime.AddMonths(1);
                        }
                        await db.SaveChangesAsync();

                        //kiểm tra xem có cần tạo hợp đồng hay không
                        if (itemModel.IsCreateContract == true)
                        {
                            var contract = new tbl_Contract();
                            contract.Name = "Hợp đồng cam kết chất lượng đầu ra";
                            contract.StudentId = itemModel.StudentId ?? 0;
                            contract.Content = itemModel.Content;
                            contract.Enable = true;
                            contract.CreatedOn = DateTime.Now;
                            contract.ModifiedOn = DateTime.Now;
                            contract.ModifiedBy = contract.CreatedBy = userLog.FullName;
                            db.tbl_Contract.Add(contract);
                            await db.SaveChangesAsync();
                        }

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
        public static async Task<tbl_Bill> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Bill.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                {
                    // Sản phẩm
                    var detail = await db.tbl_BillDetail.Where(x => x.BillId == data.Id && x.Enable == true).ToListAsync();
                    if (data.Type == (int)BillType.BuyComboPack && detail.Any())
                    {
                        var combo = await db.tbl_Combo.FirstOrDefaultAsync(x => x.Id == detail[0].ComboId && x.Enable == true);
                        data.ComboName = combo?.Name;
                        data.SDateCombo = combo?.StartDate;
                        data.EDateCombo = combo?.EndDate;
                    }
                    foreach (var item in detail)
                    {
                        var product = new ProductModel();
                        var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == item.ClassId);
                        if (_class != null)
                        {
                            var proramClass = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == _class.ProgramId);
                            if (_class.PaymentType == 1)
                            {
                                product.ClassName = _class.Name;
                                product.Price = _class.Price;
                                if (proramClass != null)
                                    product.GradeName = db.tbl_Grade.FirstOrDefault(x => x.Id == proramClass.GradeId)?.Name;
                                product.ProgramName = proramClass?.Name;
                                product.Code = proramClass?.Code;
                            }
                            else if (_class.PaymentType == 2)
                            {
                                var tuitionPackage = await db.tbl_TuitionPackage.SingleOrDefaultAsync(x => x.Id == data.TuitionPackageId);
                                if (tuitionPackage != null)
                                {
                                    product.Price = (double)tuitionPackage.Months * _class.Price;
                                    product.TotalMonth = tuitionPackage.Months;
                                }
                                product.ClassName = _class.Name;
                                product.PriceMonth = _class.Price;
                                product.ProgramName = proramClass?.Name;
                                product.Code = proramClass?.Code;
                            }
                        }
                        else
                        {
                            var program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == item.ProgramId);
                            product.Price = program?.Price ?? 0;
                            product.ProgramName = program?.Name;
                            if (program != null)
                                product.GradeName = db.tbl_Grade.FirstOrDefault(x => x.Id == program.GradeId)?.Name;
                            product.Code = program?.Code;
                        }
                        data.Products.Add(product);
                    }
                    // Thông tin học viên
                    var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.StudentId);
                    data.FullName = student?.FullName;
                    data.UserCode = student?.UserCode;
                    data.UserEmail = student?.Email;
                    data.UserPhone = student?.Mobile;
                    // Thông tin giảm giá
                    var getDiscound = await db.tbl_Discount.SingleOrDefaultAsync(x => x.Id == data.DiscountId);
                    if (getDiscound != null)
                    {
                        data.DiscountCode = getDiscound?.Code;
                        data.DiscountId = getDiscound?.Id;
                        if (getDiscound.Type == 1)
                            data.DiscountPrice = getDiscound?.Value;
                        else if (getDiscound.Type == 2)
                            data.DiscountPrice = (data.TotalPrice * getDiscound?.Value) / 100;
                    }
                }
                return data;
            }
        }
        public static async Task<tbl_Template> Print(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Bill.SingleOrDefaultAsync(x => x.Id == id);
                if (data == null)
                    throw new Exception("Không tìm thấy thông tin thanh toán");

                var template = await db.tbl_Template.FirstOrDefaultAsync(x => x.Type == 8);
                if (template == null)
                    throw new Exception("Không tìm thấy mẫu in");


                if (string.IsNullOrEmpty(template.Content))
                    throw new Exception("Mẫu in chưa có nội dung");
                return template;
            }
        }
        public class Get_ClassAvailable
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Thumbnail { get; set; }
            public int MaxQuantity { get; set; }
            public int StudentQuantity { get; set; }
            /// <summary>
            /// true = có thể đăng ký
            /// </summary>
			public bool Fit { get; set; }
            public string Note { get; set; }
            public double? Price { get; set; }
            public bool? IsMonthly { get; set; }
            public DateTime? StartDay { get; set; }
            public DateTime? EndDay { get; set; }
            public int? TotalLesson { get; set; }
            public int? CompletedLesson { get; set; }
            /// <summary>
            /// 1 - Thanh toán một lần
            /// 2 - Thanh toán theo tháng
            /// </summary>
            public int PaymentType { get; set; }
            public string PaymentTypeName
            {
                get
                {
                    return PaymentType == 1 ? "Thanh toán một lần" : PaymentType == 2 ? "Thanh toán theo tháng" : "";
                }
            }
            public int? ProgramId { get; set; }
            public string ProgramName { get; set; }
            public int? GradeId { get; set; }
            public string GradeName { get; set; }

        }
        public class GetClassAvailableSearch
        {
            public int StudentId { get; set; }
            public int BranchId { get; set; }
            public string Search { get; set; }
            public int ProgramId { get; set; }
            /// <summary>
            /// 1 - Thanh toán một lần
            /// 2 - Thanh toán theo tháng
            /// </summary>
            public int PaymentType { get; set; }
        }
        /// <summary>
        /// Lấy danh sách lớp học khi đăng ký
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public static async Task<List<Get_ClassAvailable>> GetClassAvailable(GetClassAvailableSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_ClassAvailable " +
                    $"@StudentId = {baseSearch.StudentId}," +
                    $"@BranchId = {baseSearch.BranchId}," +
                    $"@PaymentType = {baseSearch.PaymentType}," +
                    $"@ProgramId = {baseSearch.ProgramId}," +
                    $"@Search = N'{baseSearch.Search}'";
                var data = await db.SqlQuery<Get_ClassAvailable>(sql);
                return data;
            }
        }
        /// <summary>
        /// Đăng ký học
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        //public static async Task<tbl_Bill> Insert(BillCreate itemModel, tbl_UserInformation user, lmsDbContext db)
        //{
        //    try
        //    {
        //        var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId && x.RoleId == 3);
        //        if (student == null)
        //            throw new Exception("Không tìm thấy học viên");
        //        var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
        //        if (branch == null)
        //            throw new Exception("Không tìm thấy trung tâm");
        //        // thêm Bill
        //        var model = new tbl_Bill(itemModel);
        //        model.CreatedBy = model.ModifiedBy = user.FullName;
        //        db.tbl_Bill.Add(model);
        //        await db.SaveChangesAsync();
        //        //Kiểm tra quyền thanh toán, nếu không có quyền thanh toán sẽ gửi yêu cầu duyệt
        //        var paymentAllow = await db.tbl_PaymentAllow.AnyAsync(x => x.UserId == user.UserInformationId && x.Enable == true);
        //        if (!paymentAllow && user.RoleId != ((int)RoleEnum.admin) && user.RoleId != ((int)RoleEnum.accountant))
        //        {
        //            double money = itemModel.Paid;
        //            itemModel.Paid = 0;
        //            model.Paid = 0;
        //            var paymentApprove = new tbl_PaymentApprove
        //            {
        //                BillId = model.Id,
        //                CreatedBy = user.FullName,
        //                CreatedOn = DateTime.Now,
        //                Enable = true,
        //                ModifiedBy = user.FullName,
        //                ModifiedOn = DateTime.Now,
        //                Money = money,
        //                Note = itemModel.Note,
        //                Status = 1,
        //                StatusName = "Chờ duyệt",
        //                UserId = user.UserInformationId
        //            };
        //            db.tbl_PaymentApprove.Add(paymentApprove);
        //            await db.SaveChangesAsync();
        //        }

        //        double totalPrice = 0;
        //        double reduced = 0;
        //        if (itemModel.Details != null && itemModel.Details.Count > 0)
        //        {
        //            foreach (var item in itemModel.Details)
        //            {
        //                if (itemModel.Type == 1 || itemModel.Type == 7 || itemModel.Type == 6)//Đăng ký học || đăng ký học sau bảo lưu || chuyển lớp
        //                {
        //                    var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == item.ClassId);
        //                    if (_class != null)
        //                    {
        //                        if (_class.Status == 3)
        //                            throw new Exception("Lớp học đã kết thúc");
        //                        var detail = new tbl_BillDetail(item);
        //                        detail.CreatedBy = detail.ModifiedBy = user.FullName;
        //                        detail.BillId = model.Id;
        //                        detail.ProgramId = _class.ProgramId;
        //                        detail.Quantity = _class.IsMonthly ? item.NumberOfMonths : 1;
        //                        detail.MonthAvailable = _class.IsMonthly ? item.NumberOfMonths : 0;
        //                        detail.Price = _class.Price;
        //                        detail.TotalPrice = detail.Price * item.Quantity;
        //                        detail.StudentId = model.StudentId;
        //                        totalPrice += detail.TotalPrice;

        //                        db.tbl_BillDetail.Add(detail);
        //                        var checkExist = await db.tbl_StudentInClass.AnyAsync(x => x.StudentId == model.StudentId && x.ClassId == _class.Id && x.Enable == true);
        //                        if (checkExist)
        //                            throw new Exception($"Học viên đã có trong lớp {_class.Name}");
        //                        await db.SaveChangesAsync();
        //                        db.tbl_StudentInClass.Add(new tbl_StudentInClass
        //                        {
        //                            BillDetailId = detail.Id,
        //                            BranchId = _class.BranchId,
        //                            ModifiedBy = user.FullName,
        //                            ClassId = _class.Id,
        //                            CreatedBy = user.FullName,
        //                            CreatedOn = DateTime.Now,
        //                            Enable = true,
        //                            ModifiedOn = DateTime.Now,
        //                            Type = 1,
        //                            TypeName = "Chính thức",
        //                            Note = "",
        //                            StudentId = model.StudentId,
        //                        });
        //                        student.LearningStatus = 2;
        //                        student.LearningStatusName = "Đang học";

        //                        // thêm lịch sử học viên  
        //                        var learningHistoryService = new LearningHistoryService(db);
        //                        await learningHistoryService.Insert(new LearningHistoryCreate
        //                        {
        //                            StudentId = itemModel.StudentId,
        //                            Content = $"Hẹn đăng ký lớp {_class.Name}"
        //                        });

        //                        //tạo monthDebt để biết học viên này đã đóng tháng nó vào rồi (mặc định luôn là 1)
        //                        if (_class.IsMonthly)
        //                        {
        //                            for (int i = 0; i < detail.Quantity; i++)
        //                            {
        //                                tbl_StudentMonthlyDebt monthlyDebt = new tbl_StudentMonthlyDebt()
        //                                {
        //                                    StudentId = student.UserInformationId,
        //                                    ClassId = _class.Id,
        //                                    Price = _class.Price,
        //                                    Month = DateTime.Now.AddMonths(i),
        //                                    IsPaymentDone = true,
        //                                    Enable = true,
        //                                    BranchId = _class.BranchId,
        //                                    CreatedBy = user.FullName,
        //                                    CreatedOn = DateTime.Now
        //                                };
        //                                db.tbl_StudentMonthlyDebt.Add(monthlyDebt);
        //                            }
        //                        }
        //                    }
        //                    else //Đăng ký đặt lớp chương trình học
        //                    {
        //                        if (item.ClassId.HasValue && item.ClassId != 0)
        //                            throw new Exception("Không tìm thấy lớp học");//Bắt trường hợp FE truyền sai lớp học

        //                        var program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == item.ProgramId);
        //                        if (program == null)
        //                            throw new Exception("Không tìm thấy chương trình học");
        //                        var detail = new tbl_BillDetail(item);
        //                        detail.CreatedBy = detail.ModifiedBy = user.FullName;
        //                        detail.BillId = model.Id;
        //                        detail.Quantity = 1;
        //                        detail.Price = program.Price;
        //                        detail.TotalPrice = program.Price;
        //                        detail.StudentId = model.StudentId;
        //                        totalPrice += program.Price;
        //                        db.tbl_BillDetail.Add(detail);
        //                        tbl_ClassRegistration classRegistration = new tbl_ClassRegistration
        //                        {
        //                            BranchId = model.BranchId,
        //                            StudentId = model.StudentId,
        //                            ModifiedBy = user.FullName,
        //                            Price = detail.TotalPrice,
        //                            Status = 1,
        //                            StatusName = "Chờ xếp lớp",
        //                            ProgramId = program.Id,
        //                            CreatedBy = user.FullName,
        //                            CreatedOn = DateTime.Now,
        //                            Enable = true,
        //                            ModifiedOn = DateTime.Now,
        //                        };
        //                        db.tbl_ClassRegistration.Add(classRegistration);

        //                        // thêm lịch sử học viên  
        //                        var learningHistoryService = new LearningHistoryService(db);
        //                        await learningHistoryService.Insert(new LearningHistoryCreate
        //                        {
        //                            StudentId = itemModel.StudentId,
        //                            Content = $"Đăng ký chương trình học {program.Name}"
        //                        });

        //                        //Lưu thông tin khảo sát ngày học của học viên
        //                        if (item.Expectations != null && item.Expectations.Count > 0)
        //                        {
        //                            foreach (var exp in item.Expectations)
        //                            {
        //                                var expectation = new tbl_StudentExpectation(exp);
        //                                expectation.ClassRegistrationId = classRegistration.Id;
        //                                expectation.StudentId = itemModel.StudentId;
        //                                expectation.CreatedBy = expectation.ModifiedBy = user.FullName;
        //                                db.tbl_StudentExpectation.Add(expectation);
        //                            }
        //                        }

        //                    }
        //                }
        //                else if (itemModel.Type == 2) //Mua dịch vụ
        //                {
        //                    var cart = await db.tbl_Cart.SingleOrDefaultAsync(x => x.Id == item.CartId && x.Enable == true);
        //                    if (cart == null)
        //                        throw new Exception("Không tìm thấy giỏ hàng");
        //                    var product = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == cart.ProductId);
        //                    if (product == null)
        //                        throw new Exception("Không tìm thấy sản phẩm");
        //                    var detail = new tbl_BillDetail(item);
        //                    detail.CreatedBy = detail.ModifiedBy = user.FullName;
        //                    detail.BillId = model.Id;
        //                    detail.ProductId = product.Id;
        //                    detail.Quantity = cart.Quantity;
        //                    detail.Price = product.Price;
        //                    detail.TotalPrice = product.Price * cart.Quantity;
        //                    detail.StudentId = model.StudentId;
        //                    totalPrice += detail.TotalPrice;
        //                    db.tbl_BillDetail.Add(detail);
        //                    cart.Enable = false;//Xóa giỏ
        //                }
        //                else if (itemModel.Type == 3) //Đăng ký lớp dạy kèm
        //                {
        //                    var program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == item.ProgramId);
        //                    if (program == null)
        //                        throw new Exception("Không tìm thấy chương trình học");

        //                    var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == item.CurriculumId);
        //                    if (curriculum == null)
        //                        throw new Exception("Không tìm thấy giáo trình");

        //                    var _class = new tbl_Class
        //                    {
        //                        AcademicId = 0,
        //                        BranchId = model.BranchId,
        //                        CreatedBy = user.FullName,
        //                        CreatedOn = DateTime.Now,
        //                        CurriculumId = item.CurriculumId,
        //                        Enable = true,
        //                        GradeId = program.GradeId,
        //                        Name = $"Dạy kèm {student.FullName} - {student.UserCode} [{program.Name} - {curriculum.Name}]",
        //                        ModifiedBy = user.FullName,
        //                        ModifiedOn = DateTime.Now,
        //                        Price = program.Price,
        //                        ProgramId = item.ProgramId,
        //                        Status = 2,
        //                        StatusName = "Đang diễn ra",
        //                        TeacherId = 0,
        //                        Type = 3,
        //                        TypeName = "Dạy kèm"
        //                    };
        //                    db.tbl_Class.Add(_class);
        //                    await db.SaveChangesAsync();

        //                    //tạo giáo trình
        //                    if (item.CurriculumId != null)
        //                    {
        //                        var curriculumInClass = new tbl_CurriculumInClass
        //                        {
        //                            ClassId = _class.Id,
        //                            CurriculumId = item.CurriculumId,
        //                            Name = curriculum.Name,
        //                            IsComplete = false,
        //                            CompletePercent = 0,
        //                            CreatedBy = user.FullName,
        //                            CreatedOn = DateTime.Now,
        //                            Enable = true
        //                        };
        //                        db.tbl_CurriculumInClass.Add(curriculumInClass);
        //                        await db.SaveChangesAsync();
        //                        var curriculumDetails = await db.tbl_CurriculumDetail.Where(x => x.Enable == true && x.CurriculumId == curriculumInClass.CurriculumId).ToListAsync();
        //                        if (curriculumDetails.Any())
        //                        {
        //                            foreach (var itemCurDetail in curriculumDetails)
        //                            {
        //                                var curDetailInClass = new tbl_CurriculumDetailInClass
        //                                {
        //                                    CurriculumIdInClass = curriculumInClass.Id,
        //                                    CurriculumDetailId = itemCurDetail.Id,
        //                                    IsComplete = false,
        //                                    Name = itemCurDetail.Name,
        //                                    Index = itemCurDetail.Index,
        //                                    CompletePercent = 0,
        //                                    Enable = true,
        //                                    CreatedBy = user.FullName,
        //                                    CreatedOn = DateTime.Now,
        //                                    ModifiedBy = user.FullName,
        //                                    ModifiedOn = DateTime.Now,
        //                                };
        //                                db.tbl_CurriculumDetailInClass.Add(curDetailInClass);
        //                                db.SaveChanges();
        //                                //Thêm cái file vào chương
        //                                var file = await db.tbl_FileInCurriculumDetail.Where(x => x.Enable == true && x.CurriculumDetailId == itemCurDetail.Id).ToListAsync();
        //                                if (file.Any())
        //                                {
        //                                    foreach (var itemFile in file)
        //                                    {
        //                                        var fileCreate = new tbl_FileCurriculumInClass
        //                                        {
        //                                            CurriculumDetailId = curDetailInClass.Id,
        //                                            FileCurriculumId = itemFile.Id,
        //                                            IsComplete = false,
        //                                            IsHide = false,
        //                                            FileName = itemFile.FileName,
        //                                            FileUrl = itemFile.FileUrl,
        //                                            Index = itemFile.Index,
        //                                            ClassId = model.Id,
        //                                            Enable = true,
        //                                            CreatedBy = user.FullName,
        //                                            CreatedOn = DateTime.Now,
        //                                            ModifiedBy = user.FullName,
        //                                            ModifiedOn = DateTime.Now
        //                                        };
        //                                        db.tbl_FileCurriculumInClass.Add(fileCreate);
        //                                        await db.SaveChangesAsync();
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }

        //                    var studentInClass = new tbl_StudentInClass
        //                    {
        //                        BranchId = model.BranchId,
        //                        ClassId = _class.Id,
        //                        CreatedBy = user.FullName,
        //                        CreatedOn = DateTime.Now,
        //                        Enable = true,
        //                        ModifiedBy = user.FullName,
        //                        ModifiedOn = DateTime.Now,
        //                        Note = "",
        //                        Type = 1,
        //                        TypeName = "Chính thức",
        //                        Warning = false,
        //                        StudentId = student.UserInformationId,
        //                    };
        //                    db.tbl_StudentInClass.Add(studentInClass);

        //                    var detail = new tbl_BillDetail(item);
        //                    detail.CreatedBy = detail.ModifiedBy = user.FullName;
        //                    detail.BillId = model.Id;
        //                    detail.ProductId = 0;
        //                    detail.Quantity = 1;
        //                    detail.Price = program.Price;
        //                    detail.TotalPrice = _class.Price;
        //                    detail.ProgramId = program.Id;
        //                    detail.CurriculumId = item.CurriculumId;
        //                    detail.StudentId = model.StudentId;
        //                    totalPrice += _class.Price;
        //                    db.tbl_BillDetail.Add(detail);

        //                    await db.SaveChangesAsync();

        //                }


        //                await db.SaveChangesAsync();
        //            }


        //            //Tạo voucher giảm tiền cho các lớp vào trễ
        //            var totalReduce = itemModel.Reduce ?? 0;
        //            if (totalReduce > 0)
        //            {
        //                tbl_Discount discount = new tbl_Discount()
        //                {
        //                    Code = $"REDUCEBILL_{model.Id}",
        //                    Type = 1,
        //                    TypeName = "Giảm tiền",
        //                    PackageType = 1,
        //                    PackageTypeName = "Gói lẻ",
        //                    Value = totalReduce,
        //                    Status = 1,
        //                    Note = $"Voucher giảm tiền các buổi chưa học",
        //                    Expired = DateTime.Now.AddMinutes(30),
        //                    Quantity = 1,
        //                    UsedQuantity = 1,
        //                    MaxDiscount = totalPrice,
        //                    Enable = true,
        //                    CreatedOn = DateTime.Now,
        //                    CreatedBy = user.FullName,
        //                };
        //                db.tbl_Discount.Add(discount);
        //                reduced += totalReduce;
        //                await db.SaveChangesAsync();
        //                //model.LessonDiscountId = discount.Id;
        //            }
        //        }
        //        else
        //        {
        //            totalPrice = itemModel.Price;
        //        }

        //        if (itemModel.DiscountId.HasValue && totalPrice > 0 && itemModel.DiscountId > 0 && itemModel.Type != 4)
        //        {
        //            var discount = await db.tbl_Discount.SingleOrDefaultAsync(x => x.Id == itemModel.DiscountId);
        //            if (discount == null)
        //                throw new Exception("Không tìm thấy khuyến mãi");
        //            if (discount.PackageType == 1 && itemModel.Details.Count() > 1)
        //                throw new Exception("Khuyến mãi dành cho mua lẻ");
        //            if (discount.PackageType == 2 && itemModel.Details.Count() == 1)
        //                throw new Exception("Khuyến mãi dành cho gói combo");
        //            if (discount.Status == 2)
        //                throw new Exception("Khuyến mãi đã hết hạn");
        //            if (discount.Quantity <= discount.UsedQuantity)
        //                throw new Exception("Khuyến mãi đã dùng hết");

        //            //Tính khuyến mãi
        //            double newReduce = 0;
        //            if (discount.Type == 1)
        //                newReduce = discount.Value;
        //            else
        //            {
        //                newReduce = (totalPrice / 100) * discount.Value;
        //                if (newReduce > discount.MaxDiscount)
        //                    newReduce = discount.MaxDiscount ?? 0;
        //            }
        //            discount.UsedQuantity += 1;
        //            reduced += newReduce;
        //        }
        //        model.TotalPrice = totalPrice;
        //        model.Reduced = reduced;
        //        model.Debt = (totalPrice - (reduced + model.Paid));
        //        if (model.Paid > 0)
        //        {
        //            string printContent = await PaymentSessionService.GetPrintContent(
        //                    1,
        //                    model.StudentId,
        //                    $"Thanh toán {tbl_Bill.GetTypeName(model.Type)}",
        //                    model.Paid,
        //                    user.FullName,
        //                    student.FullName,
        //                    student.UserCode
        //                    );
        //            db.tbl_PaymentSession.Add(new tbl_PaymentSession
        //            {
        //                BranchId = model.BranchId,
        //                CreatedBy = user.FullName,
        //                CreatedOn = DateTime.Now,
        //                Enable = true,
        //                ModifiedBy = user.FullName,
        //                ModifiedOn = DateTime.Now,
        //                Type = 1,
        //                TypeName = "Thu",
        //                PaymentMethodId = itemModel.PaymentMethodId,
        //                Reason = $"Thanh toán {(model.Type == 1 ? "đăng ký học" : model.Type == 2 ? "mua dịch vụ" : "")}",
        //                UserId = model.StudentId,
        //                Note = model.Note,
        //                Value = model.Paid,
        //                PrintContent = printContent
        //            });
        //            if (itemModel.Type == 1 || itemModel.Type == 3 || itemModel.Type == 7)
        //            {
        //                var sale = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.SaleId && x.Enable == true && x.RoleId == 5);
        //                if (sale != null) // tìm thấy tư vấn viên
        //                {
        //                    tbl_SaleRevenue saleRevenue = new tbl_SaleRevenue();
        //                    saleRevenue.SaleId = sale.UserInformationId;
        //                    saleRevenue.BillId = model.Id;
        //                    saleRevenue.Value = model.Paid;
        //                    saleRevenue.CreatedBy = saleRevenue.ModifiedBy = user.FullName;
        //                    db.tbl_SaleRevenue.Add(saleRevenue);
        //                    //await SaleRevenueService.Insert(saleRevenue, user);
        //                }
        //            }
        //        }
        //        if (model.Debt <= 0)
        //        {
        //            model.CompleteDate = DateTime.Now;
        //            var details = await db.tbl_BillDetail
        //                .Where(x => x.BillId == model.Id && x.CartId.HasValue && x.CartId != 0 && x.Enable == true)
        //                .ToListAsync();
        //            if (details.Any())
        //            {
        //                foreach (var item in details)
        //                {
        //                    var product = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == item.ProductId);
        //                    if (product.Type == 1)//Khóa video thì tạo mã active
        //                    {
        //                        List<string> activeCodes = await db.tbl_VideoActiveCode.Select(i => i.ActiveCode).ToListAsync();
        //                        for (int i = 1; i <= item.Quantity; i++)
        //                        {
        //                            string activeCode = AssetCRM.RandomStringWithText(10);
        //                            while (activeCodes.Any(ac => ac.Contains(activeCode)))
        //                            {
        //                                activeCode = AssetCRM.RandomStringWithText(10);
        //                            }
        //                            var videoActiveCode = new tbl_VideoActiveCode
        //                            {
        //                                ActiveCode = activeCode,
        //                                StudentId = item.StudentId,
        //                                ProductId = product.Id,
        //                                BillDetailId = item.Id,
        //                                CreatedBy = user.FullName,
        //                                CreatedOn = DateTime.Now,
        //                                Enable = true,
        //                                IsUsed = false,
        //                                ModifiedBy = user.FullName,
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                            db.tbl_VideoActiveCode.Add(videoActiveCode);
        //                            await db.SaveChangesAsync();
        //                        }
        //                    }
        //                    else if (product.Type == 2)//Thêm bộ đề cho học viên
        //                    {
        //                        var packageStudent = new tbl_PackageStudent
        //                        {
        //                            CreatedBy = user.FullName,
        //                            CreatedOn = DateTime.Now,
        //                            Enable = true,
        //                            ModifiedBy = user.FullName,
        //                            ModifiedOn = DateTime.Now,
        //                            PackageId = product.Id,
        //                            StudentId = item.StudentId
        //                        };
        //                        db.tbl_PackageStudent.Add(packageStudent);
        //                        product.TotalStudent += 1;
        //                        await db.SaveChangesAsync();
        //                    }
        //                    else if (product.Type == 3)//Thêm lượt chấm cho học viên
        //                    {
        //                        var markQuantity = await db.tbl_MarkQuantity.FirstOrDefaultAsync(x => x.StudentId == item.StudentId && x.Enable == true);
        //                        if (markQuantity == null)
        //                        {
        //                            markQuantity = new tbl_MarkQuantity
        //                            {
        //                                CreatedBy = user.FullName,
        //                                CreatedOn = DateTime.Now,
        //                                Enable = true,
        //                                ModifiedBy = user.FullName,
        //                                ModifiedOn = DateTime.Now,
        //                                Quantity = product.MarkQuantity,
        //                                StudentId = item.StudentId,
        //                                UsedQuantity = 0,
        //                            };
        //                            db.tbl_MarkQuantity.Add(markQuantity);
        //                        }
        //                        else
        //                        {
        //                            markQuantity.Quantity += product.MarkQuantity;
        //                            markQuantity.ModifiedBy = user.FullName;
        //                            markQuantity.ModifiedOn = DateTime.Now;
        //                        }
        //                        await db.SaveChangesAsync();
        //                    }
        //                }
        //            }
        //        }

        //        //Lưu chi tiết số tháng trả trước cho học viên khi đóng theo tháng - Tại màn hình nợ học phí tháng
        //        if (itemModel.Classes != null)
        //        {
        //            var item = itemModel.Classes;
        //            var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == item.ClassId);
        //            if (_class != null)
        //            {
        //                if (_class.Status == 3)
        //                    throw new Exception("Lớp học đã kết thúc");
        //                var detail = new tbl_BillDetail(item);
        //                detail.CreatedBy = detail.ModifiedBy = user.FullName;
        //                detail.BillId = model.Id;
        //                detail.ProgramId = _class.ProgramId;
        //                detail.Quantity = item.Quantity;
        //                detail.MonthAvailable = item.Quantity;
        //                detail.Price = _class.Price;
        //                detail.TotalPrice = detail.Price * item.Quantity;
        //                detail.StudentId = model.StudentId;
        //                detail.CreatedOn = detail.ModifiedOn = DateTime.Now;
        //                detail.Enable = true;
        //                db.tbl_BillDetail.Add(detail);

        //                // thêm lịch sử học viên  
        //                var learningHistoryService = new LearningHistoryService(db);

        //                await learningHistoryService.Insert(new LearningHistoryCreate
        //                {
        //                    StudentId = itemModel.StudentId,
        //                    Content = $"Đóng học phí tháng cho lớp {_class.Name}"
        //                });
        //            }
        //        }

        //        //thông báo tạo hóa đơn cho học viên                
        //        var sendStudent = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == model.StudentId);
        //        //param 
        //        ParamOnDetail param = new ParamOnDetail { id = model.Id };
        //        string paramString = JsonConvert.SerializeObject(param);
        //        tbl_Notification notification = new tbl_Notification()
        //        {
        //            Title = configuration.GetSection("MySettings:ProjectName").Value.ToString() + " thông báo hóa đơn",
        //            Content = "Bạn có hóa đơn mới. Giá trị của hóa đơn là " + String.Format("{0:0,0}", model.TotalPrice) + ". Vui lòng kiểm tra.",
        //            UserId = sendStudent.UserInformationId,
        //            IsSeen = false,
        //            CreatedBy = user.FullName,
        //            CreatedOn = DateTime.Now,
        //            Type = 1,
        //            ParamString = paramString,
        //            Enable = true
        //        };
        //        db.tbl_Notification.Add(notification);
        //        await db.SaveChangesAsync();
        //        Thread threadStudent = new Thread(() =>
        //        {
        //            AssetCRM.OneSignalPushNotifications(notification.Title, notification.Content, sendStudent.OneSignal_DeviceId);
        //            if (sendStudent.IsReceiveMailNotification == true)
        //                AssetCRM.SendMail(sendStudent.Email, notification.Title, notification.Content);
        //        });
        //        threadStudent.Start();

        //        //thông báo cho phụ huynh nếu có
        //        if (sendStudent.ParentId.HasValue)
        //        {
        //            tbl_UserInformation parent = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == sendStudent.ParentId && x.Enable == true);
        //            if (parent != null)
        //            {
        //                notification.Title = configuration.GetSection("MySettings:ProjectName").Value.ToString() + " thông báo hóa đơn";
        //                notification.Content = "Học viên " + sendStudent.FullName + " có hóa đơn mới. Giá trị của hóa đơn là " + String.Format("{0:0,0}", model.TotalPrice) + ". Vui lòng kiểm tra.";
        //                notification.UserId = parent.UserInformationId;
        //                db.tbl_Notification.Add(notification);
        //                await db.SaveChangesAsync();
        //                Thread threadParent = new Thread(() =>
        //                {
        //                    AssetCRM.OneSignalPushNotifications(notification.Title, notification.Content, parent.OneSignal_DeviceId);
        //                    if (parent.IsReceiveMailNotification == true)
        //                        AssetCRM.SendMail(parent.Email, notification.Title, notification.Content);
        //                });
        //                threadParent.Start();
        //            }
        //        }
        //        //thông báo cho admin
        //        var admins = await db.tbl_UserInformation.Where(x => x.RoleId == (int)RoleEnum.admin && x.Enable == true && x.UserInformationId != user.UserInformationId).ToListAsync();
        //        foreach (var ad in admins)
        //        {
        //            notification.Title = "Hóa đơn mới được tạo";
        //            notification.Content = user.FullName + " đã tạo hóa đơn mới. Giá trị của hóa đơn là " + String.Format("{0:0,0}", model.TotalPrice) + ". Vui lòng kiểm tra";
        //            notification.UserId = ad.UserInformationId;
        //            db.tbl_Notification.Add(notification);
        //            await db.SaveChangesAsync();
        //            Thread threadAd = new Thread(() =>
        //            {
        //                AssetCRM.OneSignalPushNotifications(notification.Title, notification.Content, ad.OneSignal_DeviceId);
        //                if (ad.IsReceiveMailNotification == true)
        //                    AssetCRM.SendMail(ad.Email, notification.Title, notification.Content);
        //            });
        //            threadAd.Start();
        //        }

        //        await db.SaveChangesAsync();

        //        return model;
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        // Hàm để kiểm tra xem nếu user đang đăng nhập tạo bill thì có trãi qua giai đoạn Admin duyệt hay không
        public static async Task<bool> CheckPaymentAllow(double paid, tbl_UserInformation user, lmsDbContext db)
        {
            var paymentAllow = await db.tbl_PaymentAllow.AnyAsync(x => x.UserId == user.UserInformationId && x.Enable == true);
            if (!paymentAllow && user.RoleId != (int)RoleEnum.admin
                && user.RoleId != (int)RoleEnum.accountant
                && user.RoleId != (int)RoleEnum.manager
                && paid > 0)
            {
                return false;
            }

            return true;
        }

        public static async Task<tbl_Bill> InsertV2(BillCreateV2 itemModel, tbl_UserInformation user, lmsDbContext db, string baseUrl)
        {
            try
            {
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId && x.RoleId == 3);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");
                // thêm Bill
                var model = new tbl_Bill(itemModel);
                model.FullName = student?.FullName;
                model.UserCode = student?.UserCode;
                model.UserEmail = student?.Email;
                model.UserPhone = student?.Mobile;

                // Tạo mẫu gửi mail
                var sendStudent = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == model.StudentId);
                var getDiscound = await db.tbl_Discount.SingleOrDefaultAsync(x => x.Id == itemModel.DiscountId);
                if (getDiscound != null)
                {
                    model.DiscountCode = getDiscound?.Code;
                    model.DiscountId = getDiscound?.Id;
                    if (getDiscound.Type == 1)
                        model.DiscountPrice = getDiscound?.Value;
                    else if (getDiscound.Type == 2)
                        model.DiscountPrice = (itemModel.Price * getDiscound?.Value) / 100;
                }

                string contentBillToAdmin = "";
                string contentBillToStudent = "";
                string contentBillToParent = "";
                string contentToStudent = "";
                string contentToParent = "";
                string notificationContentToStudent = "";
                string notificationContentToParent = "";
                string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
                //var httpContext = HttpContext.Current;
                //var pathViews = Path.Combine(httpContext.Server.MapPath("~/Views"));
                var pathViews = Path.Combine(_hostingEnvironment.ContentRootPath, "Views");
                contentBillToAdmin = File.ReadAllText($"{pathViews}/Base/Mail/Bill/MailBill.cshtml");
                contentBillToStudent = File.ReadAllText($"{pathViews}/Base/Mail/Bill/MailBill.cshtml");
                contentBillToParent = File.ReadAllText($"{pathViews}/Base/Mail/Bill/MailBill.cshtml");
                contentToStudent = File.ReadAllText($"{pathViews}/Base/Mail/Class/AppendStudent.cshtml");
                contentToParent = File.ReadAllText($"{pathViews}/Base/Mail/Class/AppendStudent.cshtml");

                // Thông tin URL gắn vào thông báo
                UrlNotificationModels urlNotification = new UrlNotificationModels();

                // Kiểm tra quyền thanh toán
                var checkPaymentAllow = await CheckPaymentAllow(itemModel.Paid, user, db);
                if (checkPaymentAllow == false) model.IsApproved = false;
                else model.IsApproved = true;


                //var paymentAllow = await db.tbl_PaymentAllow.AnyAsync(x => x.UserId == user.UserInformationId && x.Enable == true);
                //if (!paymentAllow && user.RoleId != ((int)RoleEnum.admin)
                //    && user.RoleId != ((int)RoleEnum.accountant)
                //    && user.RoleId != ((int)RoleEnum.manager)
                //    && model.Paid > 0)
                //{
                //    // Nếu không có quyền thanh toán thì IsApprove = false
                //    model.IsApprove = false;
                //}
                //else 
                //    model.IsApprove = true;

                model.CreatedBy = model.ModifiedBy = user.FullName;

                string baseCode = "B";
                int count = await db.tbl_Bill.CountAsync(x =>
                            x.CreatedOn.Value.Year == model.CreatedOn.Value.Year
                            && x.CreatedOn.Value.Month == model.CreatedOn.Value.Month
                            && x.CreatedOn.Value.Day == model.CreatedOn.Value.Day);
                model.Code = AssetCRM.InitCode(baseCode, DateTime.Now, count + 1);

                db.tbl_Bill.Add(model);
                await db.SaveChangesAsync();
                //Kiểm tra quyền thanh toán, nếu không có quyền thanh toán sẽ gửi yêu cầu duyệt
                if (checkPaymentAllow == false)
                {
                    double money = itemModel.Paid;
                    itemModel.Paid = 0;
                    model.Paid = 0;
                    var paymentApprove = new tbl_PaymentApprove
                    {
                        BillId = model.Id,
                        CreatedBy = user.FullName,
                        CreateById = user.UserInformationId,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        Money = money,
                        Note = itemModel.Note,
                        PaymentMethodId = itemModel.PaymentMethodId,
                        PaymentDate = itemModel.PaymentDate,
                        Status = 1,
                        StatusName = "Chờ duyệt",
                        UserId = user.UserInformationId,
                    };
                    db.tbl_PaymentApprove.Add(paymentApprove);
                    await db.SaveChangesAsync();
                    BackgroundJob.Schedule(() => PaymentApproveNotification.NotifySendAPaymentApprovalRequest(new PaymentApproveNotificationRequest.NotifySendAPaymentApprovalRequestRequest
                    {
                        PaymentApproveId = paymentApprove.Id,
                        CurrentUser = user
                    }), TimeSpan.FromSeconds(2));
                }

                double totalPrice = 0;
                double reduced = 0;
                if (itemModel.Details.Any())
                {
                    foreach (var item in itemModel.Details)
                    {
                        if (itemModel.Type == 1)//Đăng ký học
                        {
                            var product = new ProductModel();
                            var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == item.ClassId);
                            if (_class != null)
                            {
                                var programClass = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == _class.ProgramId);
                                product.ClassName = _class.Name;
                                product.ProgramName = programClass?.Name;
                                product.Price = _class.Price;
                                product.Code = programClass?.Code;

                                if (_class.Status == 3)
                                    throw new Exception("Lớp học đã kết thúc");
                                var detail = new tbl_BillDetail(item);
                                detail.CreatedBy = detail.ModifiedBy = user.FullName;
                                detail.BillId = model.Id;
                                detail.ProgramId = _class.ProgramId;
                                detail.Quantity = 1;
                                //detail.MonthAvailable = _class.IsMonthly ? item.NumberOfMonths : 0;
                                detail.Price = _class.Price;
                                detail.TotalPrice = detail.Price * item.Quantity;
                                detail.StudentId = model.StudentId;
                                totalPrice += detail.TotalPrice;

                                db.tbl_BillDetail.Add(detail);
                                var checkExist = await db.tbl_StudentInClass.AnyAsync(x => x.StudentId == model.StudentId && x.ClassId == _class.Id && x.Enable == true);
                                if (checkExist)
                                    throw new Exception($"Học viên đã có trong lớp {_class.Name}");
                                await db.SaveChangesAsync();

                                var countStudent = await db.tbl_StudentInClass.CountAsync(x => x.ClassId == _class.Id && x.Enable == true);
                                if (countStudent >= _class.MaxQuantity)
                                    throw new Exception("Lớp đã đủ học viên");

                                db.tbl_StudentInClass.Add(new tbl_StudentInClass
                                {
                                    BillDetailId = detail.Id,
                                    BranchId = _class.BranchId,
                                    ModifiedBy = user.FullName,
                                    ClassId = _class.Id,
                                    CreatedBy = user.FullName,
                                    CreatedOn = DateTime.Now,
                                    Enable = true,
                                    ModifiedOn = DateTime.Now,
                                    Type = 1,
                                    TypeName = "Chính thức",
                                    Note = "",
                                    StudentId = model.StudentId,
                                });
                                student.LearningStatus = 5;
                                student.LearningStatusName = "Đang học";

                                // thêm lịch sử học viên  
                                var learningHistoryService = new LearningHistoryService(db);
                                await learningHistoryService.Insert(new LearningHistoryCreate
                                {
                                    StudentId = itemModel.StudentId,
                                    Content = $"Đăng ký học vào lớp {_class.Name}"
                                });

                                string urlClass = "class=" + _class.Id + "&curriculum=" + _class.CurriculumId + "&branch=" + _class.BranchId + "&scoreBoardTemplateId=" + _class.ScoreboardTemplateId;
                                string urlEmailClass = urlNotification.url + urlNotification.urlDetailClass + urlClass;

                                // Thông báo mail đến học viên và phụ huynh của học viên
                                contentToStudent = contentToStudent.Replace("{item1}", "học sinh");
                                contentToStudent = contentToStudent.Replace("{item2}", "bạn");
                                contentToStudent = contentToStudent.Replace("{item3}", _class.Name);
                                if (_class.StartDay == null)
                                    contentToStudent = contentToStudent.Replace("{item4}", "Chưa có");
                                else
                                    contentToStudent = contentToStudent.Replace("{item4}", _class.StartDay.Value.ToString("dd/MM/yyyy"));
                                contentToStudent = contentToStudent.Replace("{item5}", _class.TypeName);
                                contentToStudent = contentToStudent.Replace("{item6}", projectName);
                                contentToStudent = contentToStudent.Replace("{Url}", $"<a href=\"{urlEmailClass}\" target=\"_blank\">");

                                notificationContentToStudent = @"<div>" + contentToStudent + @"</div>";

                                //// Gửi học sinh
                                //Thread sendStudentRegister = new Thread(async () =>
                                //{
                                //    tbl_Notification notificationSendStudentRegister = new tbl_Notification();

                                //    notificationSendStudentRegister.Title = "THÔNG BÁO NHẬN LỚP";
                                //    notificationSendStudentRegister.ContentEmail = notificationContentToStudent;
                                //    notificationSendStudentRegister.Content = "Bạn được thêm vào lớp " + _class.Name + ". Vui lòng kiểm tra lớp học. Chúc bạn sẽ có một buổi học vui vẻ";
                                //    notificationSendStudentRegister.Type = 5;
                                //    notificationSendStudentRegister.UserId = model.StudentId;
                                //    notificationSendStudentRegister.Url = urlClass;
                                //    notificationSendStudentRegister.Category = 0;
                                //    notificationSendStudentRegister.AvailableId = _class.Id;
                                //    await NotificationService.Send(notificationSendStudentRegister, user, true);

                                //});
                                //sendStudentRegister.Start();

                                List<NotificationToParent> notificationToParent = new List<NotificationToParent>();
                                var p = new NotificationToParent();
                                int parentId = 0;

                                var dataStudent = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == model.StudentId);
                                var dataParent = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == dataStudent.ParentId);
                                if (dataParent != null)
                                {
                                    contentToParent = contentToParent.Replace("{item1}", "phụ huynh học sinh");
                                    contentToParent = contentToParent.Replace("{item2}", "học sinh " + dataStudent.FullName);
                                    contentToParent = contentToParent.Replace("{item3}", _class.Name);
                                    if (_class.StartDay == null) contentToParent = contentToParent.Replace("{item4}", "Chưa có");
                                    else contentToParent = contentToParent.Replace("{item4}", _class.StartDay.Value.ToString("dd/MM/yyyy"));
                                    contentToParent = contentToParent.Replace("{item5}", _class.TypeName);
                                    contentToParent = contentToParent.Replace("{item6}", projectName);
                                    contentToParent = contentToParent.Replace("{Url}", $"<a href=\"{urlEmailClass}\" target=\"_blank\">");

                                    notificationContentToParent = @"<div>" + contentToParent + @"</div>";

                                    parentId = dataParent.UserInformationId;
                                    p.contentEmail = notificationContentToParent;
                                    p.studentName = dataStudent.FullName;
                                    p.parentId = parentId;
                                    notificationToParent.Add(p);
                                }

                                // Gửi phụ huynh
                                //Thread sendParent = new Thread(async () =>
                                //{
                                //    foreach (var parentInfo in notificationToParent)
                                //    {
                                //        tbl_Notification notificationSendToParent = new tbl_Notification();
                                //        notificationSendToParent.Title = "THÔNG BÁO NHẬN LỚP";
                                //        notificationSendToParent.ContentEmail = parentInfo.contentEmail;
                                //        notificationSendToParent.Content = "Học sinh " + parentInfo.studentName + " được thêm vào lớp " + _class.Name + ". Vui lòng kiểm tra lớp học.";
                                //        notificationSendToParent.Type = 5;
                                //        notificationSendToParent.UserId = parentInfo.parentId;
                                //        notificationSendToParent.Url = urlClass;
                                //        notificationSendToParent.Category = 0;
                                //        notificationSendToParent.AvailableId = _class.Id;
                                //        await NotificationService.Send(notificationSendToParent, user, true);
                                //    }
                                //});
                                //sendParent.Start();

                                //kiểm tra xem có cần tạo hợp đồng hay không
                                if (itemModel.IsCreateContract == true)
                                {
                                    var contract = new tbl_Contract();
                                    contract.Name = "Hợp đồng cam kết chất lượng đầu ra";
                                    contract.StudentId = itemModel.StudentId ?? 0;
                                    contract.Content = itemModel.Content;
                                    contract.Enable = true;
                                    contract.CreatedOn = DateTime.Now;
                                    contract.ModifiedOn = DateTime.Now;
                                    contract.ModifiedBy = contract.CreatedBy = user.FullName;
                                    db.tbl_Contract.Add(contract);
                                    await db.SaveChangesAsync();
                                }
                            }
                            else //Đăng ký đặt lớp chương trình học
                            {
                                if (item.ClassId.HasValue && item.ClassId != 0)
                                    throw new Exception("Không tìm thấy lớp học");//Bắt trường hợp FE truyền sai lớp học

                                var program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == item.ProgramId);
                                if (program == null)
                                    throw new Exception("Không tìm thấy chương trình học");

                                product.ProgramName = program?.Name;
                                product.Price = program.Price;
                                product.Code = program?.Code;

                                var detail = new tbl_BillDetail(item);
                                detail.CreatedBy = detail.ModifiedBy = user.FullName;
                                detail.BillId = model.Id;
                                detail.Quantity = 1;
                                detail.Price = program.Price;
                                detail.TotalPrice = program.Price;
                                detail.StudentId = model.StudentId;
                                totalPrice += program.Price;
                                db.tbl_BillDetail.Add(detail);
                                tbl_ClassRegistration classRegistration = new tbl_ClassRegistration
                                {
                                    BranchId = model.BranchId,
                                    StudentId = model.StudentId,
                                    ModifiedBy = user.FullName,
                                    Price = detail.TotalPrice,
                                    Status = 1,
                                    StatusName = "Chờ xếp lớp",
                                    ProgramId = program.Id,
                                    CreatedBy = user.FullName,
                                    CreatedOn = DateTime.Now,
                                    Enable = true,
                                    ModifiedOn = DateTime.Now,
                                };
                                db.tbl_ClassRegistration.Add(classRegistration);

                                // thêm lịch sử học viên  
                                var learningHistoryService = new LearningHistoryService(db);
                                await learningHistoryService.Insert(new LearningHistoryCreate
                                {
                                    StudentId = itemModel.StudentId,
                                    Content = $"Đăng ký chương trình học {program.Name}"
                                });

                                //Lưu thông tin khảo sát ngày học của học viên
                                if (item.Expectations != null && item.Expectations.Count > 0)
                                {
                                    foreach (var exp in item.Expectations)
                                    {
                                        var expectation = new tbl_StudentExpectation(exp);
                                        expectation.ClassRegistrationId = classRegistration.Id;
                                        expectation.StudentId = itemModel.StudentId;
                                        expectation.CreatedBy = expectation.ModifiedBy = user.FullName;
                                        db.tbl_StudentExpectation.Add(expectation);
                                    }
                                }
                                //kiểm tra xem có cần tạo hợp đồng hay không
                                if (itemModel.IsCreateContract == true)
                                {
                                    var contract = new tbl_Contract();
                                    contract.Name = "Hợp đồng cam kết chất lượng đầu ra";
                                    contract.StudentId = itemModel.StudentId ?? 0;
                                    contract.Content = itemModel.Content;
                                    contract.Enable = true;
                                    contract.CreatedOn = DateTime.Now;
                                    contract.ModifiedOn = DateTime.Now;
                                    contract.ModifiedBy = contract.CreatedBy = user.FullName;
                                    db.tbl_Contract.Add(contract);
                                    await db.SaveChangesAsync();
                                }
                            }
                            model.Products.Add(product);
                        }
                        else if (itemModel.Type == 2) //Mua dịch vụ
                        {
                            var cart = await db.tbl_Cart.SingleOrDefaultAsync(x => x.Id == item.CartId && x.Enable == true);
                            if (cart == null)
                                throw new Exception("Không tìm thấy giỏ hàng");
                            var product = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == cart.ProductId);
                            if (product == null)
                                throw new Exception("Không tìm thấy sản phẩm");
                            var detail = new tbl_BillDetail(item);
                            detail.CreatedBy = detail.ModifiedBy = user.FullName;
                            detail.BillId = model.Id;
                            detail.ProductId = product.Id;
                            detail.Quantity = cart.Quantity;
                            detail.Price = product.Price;
                            detail.TotalPrice = product.Price * cart.Quantity;
                            detail.StudentId = model.StudentId;
                            totalPrice += detail.TotalPrice;
                            db.tbl_BillDetail.Add(detail);
                            cart.Enable = false;//Xóa giỏ
                        }
                        else if (itemModel.Type == 3) //Đăng ký lớp dạy kèm
                        {
                            var program = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == item.ProgramId);
                            if (program == null)
                                throw new Exception("Không tìm thấy chương trình học");

                            var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == item.CurriculumId);
                            if (curriculum == null)
                                throw new Exception("Không tìm thấy giáo trình");

                            var _class = new tbl_Class
                            {
                                AcademicId = 0,
                                BranchId = model.BranchId,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                CurriculumId = item.CurriculumId,
                                Enable = true,
                                GradeId = program.GradeId,
                                Name = $"Dạy kèm {student.FullName} - {student.UserCode} [{program.Name} - {curriculum.Name}]",
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                Price = program.Price,
                                ProgramId = item.ProgramId,
                                Status = 2,
                                StatusName = "Đang diễn ra",
                                TeacherId = 0,
                                Type = 3,
                                TypeName = "Dạy kèm"
                            };
                            db.tbl_Class.Add(_class);
                            await db.SaveChangesAsync();

                            //tạo giáo trình
                            if (item.CurriculumId != null)
                            {
                                var curriculumInClass = new tbl_CurriculumInClass
                                {
                                    ClassId = _class.Id,
                                    CurriculumId = item.CurriculumId,
                                    Name = curriculum.Name,
                                    IsComplete = false,
                                    CompletePercent = 0,
                                    CreatedBy = user.FullName,
                                    CreatedOn = DateTime.Now,
                                    Enable = true
                                };
                                db.tbl_CurriculumInClass.Add(curriculumInClass);
                                await db.SaveChangesAsync();
                                var curriculumDetails = await db.tbl_CurriculumDetail.Where(x => x.Enable == true && x.CurriculumId == curriculumInClass.CurriculumId).ToListAsync();
                                if (curriculumDetails.Any())
                                {
                                    foreach (var itemCurDetail in curriculumDetails)
                                    {
                                        var curDetailInClass = new tbl_CurriculumDetailInClass
                                        {
                                            CurriculumIdInClass = curriculumInClass.Id,
                                            CurriculumDetailId = itemCurDetail.Id,
                                            IsComplete = false,
                                            Name = itemCurDetail.Name,
                                            Index = itemCurDetail.Index,
                                            CompletePercent = 0,
                                            Enable = true,
                                            CreatedBy = user.FullName,
                                            CreatedOn = DateTime.Now,
                                            ModifiedBy = user.FullName,
                                            ModifiedOn = DateTime.Now,
                                        };
                                        db.tbl_CurriculumDetailInClass.Add(curDetailInClass);
                                        db.SaveChanges();
                                        //Thêm cái file vào chương
                                        var file = await db.tbl_FileInCurriculumDetail.Where(x => x.Enable == true && x.CurriculumDetailId == itemCurDetail.Id).ToListAsync();
                                        if (file.Any())
                                        {
                                            foreach (var itemFile in file)
                                            {
                                                var fileCreate = new tbl_FileCurriculumInClass
                                                {
                                                    CurriculumDetailId = curDetailInClass.Id,
                                                    FileCurriculumId = itemFile.Id,
                                                    IsComplete = false,
                                                    IsHide = false,
                                                    FileName = itemFile.FileName,
                                                    FileUrl = itemFile.FileUrl,
                                                    Index = itemFile.Index,
                                                    ClassId = model.Id,
                                                    Enable = true,
                                                    CreatedBy = user.FullName,
                                                    CreatedOn = DateTime.Now,
                                                    ModifiedBy = user.FullName,
                                                    ModifiedOn = DateTime.Now
                                                };
                                                db.tbl_FileCurriculumInClass.Add(fileCreate);
                                                await db.SaveChangesAsync();
                                            }
                                        }
                                    }
                                }
                            }

                            var studentInClass = new tbl_StudentInClass
                            {
                                BranchId = model.BranchId,
                                ClassId = _class.Id,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                Note = "",
                                Type = 1,
                                TypeName = "Chính thức",
                                Warning = false,
                                StudentId = student.UserInformationId,
                            };
                            db.tbl_StudentInClass.Add(studentInClass);

                            var detail = new tbl_BillDetail(item);
                            detail.CreatedBy = detail.ModifiedBy = user.FullName;
                            detail.BillId = model.Id;
                            detail.ProductId = 0;
                            detail.Quantity = 1;
                            detail.Price = program.Price;
                            detail.TotalPrice = _class.Price;
                            detail.ProgramId = program.Id;
                            detail.CurriculumId = item.CurriculumId;
                            detail.StudentId = model.StudentId;
                            totalPrice += _class.Price;
                            db.tbl_BillDetail.Add(detail);

                            await db.SaveChangesAsync();

                        }
                        else if (itemModel.Type == 6)// Phí chuyển lớp
                        {
                            var classChange = await db.tbl_ClassChange.SingleOrDefaultAsync(x => x.Id == item.ClassChangeId);
                            if (classChange == null)
                                throw new Exception("Không tìm thấy thông tin chuyển lớp");
                            totalPrice = itemModel.Price;
                            var billDetail = new tbl_BillDetail
                            {
                                BillId = model.Id,
                                ClassChangeId = classChange.Id,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                Price = totalPrice,
                                TotalPrice = totalPrice,
                                NewClassId = classChange.NewClassId,
                                OldClassId = classChange.OldClassId,
                                StudentId = model.StudentId,
                                Quantity = 1
                            };
                            db.tbl_BillDetail.Add(billDetail);
                            await db.SaveChangesAsync();

                        }
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    totalPrice = itemModel.Price;
                }

                if (itemModel.DiscountId.HasValue && totalPrice > 0 && itemModel.DiscountId > 0 && itemModel.Type != 4)
                {
                    var discount = await db.tbl_Discount.SingleOrDefaultAsync(x => x.Id == itemModel.DiscountId);
                    if (discount == null)
                        throw new Exception("Không tìm thấy khuyến mãi");
                    if (discount.PackageType == 1 && itemModel.Details.Count() > 1)
                        throw new Exception("Khuyến mãi dành cho mua lẻ");
                    if (discount.PackageType == 2 && itemModel.Details.Count() == 1)
                        throw new Exception("Khuyến mãi dành cho gói combo");
                    if (discount.Status == 2)
                        throw new Exception("Khuyến mãi đã hết hạn");
                    if (discount.Quantity <= discount.UsedQuantity)
                        throw new Exception("Khuyến mãi đã dùng hết");

                    //Tính khuyến mãi
                    double newReduce = 0;
                    if (discount.Type == 1)
                        newReduce = discount.Value;
                    else
                    {
                        newReduce = totalPrice / 100 * discount.Value;
                        if (newReduce > discount.MaxDiscount)
                            newReduce = discount.MaxDiscount ?? 0;
                    }
                    discount.UsedQuantity += 1;
                    reduced += newReduce;
                }
                double usedMoneyReserve = 0;
                double debtTemp = totalPrice - (reduced + model.Paid);
                if (itemModel.ClassReserveId != 0 && itemModel.ClassReserveId.HasValue)// Sử dụng tiền bảo lưu để thanh toán
                {
                    var classReserve = await db.tbl_ClassReserve.SingleOrDefaultAsync(x => x.Id == itemModel.ClassReserveId.Value);
                    if (classReserve == null)
                        throw new Exception("Không tìm thấy thông tin bảo lưu");
                    if (classReserve.MoneyRemaining > debtTemp)
                    {
                        usedMoneyReserve = debtTemp;
                        classReserve.MoneyRemaining -= debtTemp;
                    }
                    else
                    {
                        usedMoneyReserve = classReserve.MoneyRemaining;
                        classReserve.MoneyRemaining = 0;
                        classReserve.Status = 2;
                        classReserve.StatusName = "Đã học lại";
                    }
                    classReserve.MoneyUsed += usedMoneyReserve;
                    await db.SaveChangesAsync();
                    model.UsedMoneyReserve = usedMoneyReserve;
                    model.ClassReserveId = classReserve.Id;
                }

                model.TotalPrice = totalPrice;
                model.Reduced = reduced;
                model.Debt = totalPrice - (reduced + model.Paid + usedMoneyReserve);
                if (model.Paid > 0)
                {
                    string printContent = await PaymentSessionService.GetPrintContent(
                            1,
                            model.StudentId,
                            $"Thanh toán {tbl_Bill.GetTypeName(model.Type)}",
                            model.Paid,
                            user.FullName,
                            student.FullName,
                            student.UserCode
                            );
                    var paymentSession = new tbl_PaymentSession
                    {
                        BillId = model.Id,
                        BranchId = model.BranchId,
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        Type = 1,
                        TypeName = "Thu",
                        PaymentMethodId = itemModel.PaymentMethodId,
                        Reason = $"Thanh toán {tbl_Bill.GetTypeName(model.Type)}",
                        UserId = model.StudentId,
                        Note = model.Note,
                        Value = model.Paid,
                        PrintContent = printContent,
                        PaymentDate = itemModel.PaymentDate ?? DateTime.Now
                    };
                    db.tbl_PaymentSession.Add(paymentSession);
                    await db.SaveChangesAsync();

                    // Chuẩn bị thông tin và tạo thời gian gắn vào tên QRCode để không bị trùng tên file
                    if (sendStudent != null)
                    {
                        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string nameFile = "ThuChi_" + sendStudent.FullName.Replace(" ", "");
                        string folderToSave = "QRCodePaymentSession";
                        string name = $"{nameFile}_{timestamp}";
                        string host = urlNotification.url;
                        string data = host + urlNotification.urlPayMentSessionDetail + paymentSession.Id;
                        // Mặc định lưu ảnh với định dạng jpg
                        var QRCode = AssetCRM.CreateQRCodeV2(data, name, folderToSave, baseUrl);

                        // Lưu QRCode vào PaymentSession vừa tạo ở trên
                        var paymentSessionEntity = await db.tbl_PaymentSession.FirstOrDefaultAsync(x => x.Id == paymentSession.Id);
                        paymentSessionEntity.QRCode = QRCode;
                        await db.SaveChangesAsync();
                    }
                    //Tính doanh thu cho tư vấn viên
                    //nếu hóa đơn này là đăng ký học hoặc trả phí hàng tháng thì tính doanh thu
                    var sale = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.SaleId && x.Enable == true);
                    //nếu tìm thấy tư vấn viên
                    if (sale != null)
                    {
                        if (itemModel.Type == 1 || itemModel.Type == 3 || itemModel.Type == 5)
                        {
                            tbl_ConsultantRevenue consultantRevenue = new tbl_ConsultantRevenue();
                            consultantRevenue.SaleId = sale.UserInformationId;
                            consultantRevenue.StudentId = model.StudentId;
                            consultantRevenue.BillId = model.Id;
                            consultantRevenue.PaymentSessionId = paymentSession.Id;
                            consultantRevenue.TotalPrice = model.TotalPrice;
                            consultantRevenue.AmountPaid = model.Paid;
                            consultantRevenue.CreatedOn = DateTime.Now;
                            consultantRevenue.ModifiedOn = DateTime.Now;
                            consultantRevenue.Enable = true;
                            consultantRevenue.CreatedBy = consultantRevenue.ModifiedBy = user.FullName;
                            db.tbl_ConsultantRevenue.Add(consultantRevenue);
                            await db.SaveChangesAsync();
                        }
                        // cái này là chiến dịch hoa hồng gì đó =)))
                        if (itemModel.Type == 1 || itemModel.Type == 3)
                        {
                            tbl_SaleRevenue saleRevenue = new tbl_SaleRevenue();
                            saleRevenue.SaleId = sale.UserInformationId;
                            saleRevenue.BillId = model.Id;
                            saleRevenue.Value = model.Paid;
                            saleRevenue.CreatedBy = saleRevenue.ModifiedBy = user.FullName;
                            db.tbl_SaleRevenue.Add(saleRevenue);
                            //await SaleRevenueService.Insert(saleRevenue, user);
                        }
                    }
                }
                if (model.Debt <= 0)
                {
                    model.CompleteDate = DateTime.Now;
                    var details = await db.tbl_BillDetail
                        .Where(x => x.BillId == model.Id && x.CartId.HasValue && x.CartId != 0 && x.Enable == true)
                        .ToListAsync();
                    if (details.Any())
                    {
                        foreach (var item in details)
                        {
                            var product = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == item.ProductId);
                            if (product.Type == 1)//Khóa video thì tạo mã active
                            {
                                List<string> activeCodes = await db.tbl_VideoActiveCode.Select(i => i.ActiveCode).ToListAsync();
                                for (int i = 1; i <= item.Quantity; i++)
                                {
                                    string activeCode = AssetCRM.RandomStringWithText(10);
                                    while (activeCodes.Any(ac => ac.Contains(activeCode)))
                                    {
                                        activeCode = AssetCRM.RandomStringWithText(10);
                                    }
                                    var videoActiveCode = new tbl_VideoActiveCode
                                    {
                                        ActiveCode = activeCode,
                                        StudentId = item.StudentId,
                                        ProductId = product.Id,
                                        BillDetailId = item.Id,
                                        CreatedBy = user.FullName,
                                        CreatedOn = DateTime.Now,
                                        Enable = true,
                                        IsUsed = false,
                                        ModifiedBy = user.FullName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    db.tbl_VideoActiveCode.Add(videoActiveCode);
                                    await db.SaveChangesAsync();
                                }
                            }
                            else if (product.Type == 2)//Thêm bộ đề cho học viên
                            {
                                var packageStudent = new tbl_PackageStudent
                                {
                                    CreatedBy = user.FullName,
                                    CreatedOn = DateTime.Now,
                                    Enable = true,
                                    ModifiedBy = user.FullName,
                                    ModifiedOn = DateTime.Now,
                                    PackageId = product.Id,
                                    StudentId = item.StudentId
                                };
                                db.tbl_PackageStudent.Add(packageStudent);
                                product.TotalStudent += 1;
                                await db.SaveChangesAsync();
                            }
                            else if (product.Type == 3)//Thêm lượt chấm cho học viên
                            {
                                var markQuantity = await db.tbl_MarkQuantity.FirstOrDefaultAsync(x => x.StudentId == item.StudentId && x.Enable == true);
                                if (markQuantity == null)
                                {
                                    markQuantity = new tbl_MarkQuantity
                                    {
                                        CreatedBy = user.FullName,
                                        CreatedOn = DateTime.Now,
                                        Enable = true,
                                        ModifiedBy = user.FullName,
                                        ModifiedOn = DateTime.Now,
                                        Quantity = product.MarkQuantity,
                                        StudentId = item.StudentId,
                                        UsedQuantity = 0,
                                    };
                                    db.tbl_MarkQuantity.Add(markQuantity);
                                }
                                else
                                {
                                    markQuantity.Quantity += product.MarkQuantity;
                                    markQuantity.ModifiedBy = user.FullName;
                                    markQuantity.ModifiedOn = DateTime.Now;
                                }
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
                await db.SaveChangesAsync();

                BackgroundJob.Schedule(() => BillNotification.NotifyStudentSuccessfulClassRegistration(new BillNotificationRequest.NotifyStudentSuccessfulClassRegistrationRequest
                {
                    BillId = model.Id,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(2));
                BackgroundJob.Schedule(() => BillNotification.NotifyParentsSuccessfulClassRegistration(new BillNotificationRequest.NotifyParentsSuccessfulClassRegistrationRequest
                {
                    BillId = model.Id,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(4));

                BackgroundJob.Schedule(() => BillNotification.NotifyStudentClassPlacementRegistration(new BillNotificationRequest.NotifyStudentClassPlacementRegistrationRequest
                {
                    BillId = model.Id,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(6));
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static async Task<tbl_Bill> InsertBuyCombo(BillByComboCreate itemModel, tbl_UserInformation user, lmsDbContext db, string baseUrl)
        {
            try
            {
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId && x.RoleId == 3);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");

                var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId);
                if (branch == null)
                    throw new Exception("Không tìm thấy trung tâm");

                var combo = await db.tbl_Combo.SingleOrDefaultAsync(x => x.Id == itemModel.ComboId && x.Enable == true);
                if (branch == null)
                    throw new Exception("Không tìm thấy gói combo");
                if (combo.Status == (int)ComboStatus.CommingSoon)
                    throw new Exception("Gói combo này chưa mở bán");
                else if (combo.Status == (int)ComboStatus.HasEnded)
                    throw new Exception("Gói combo này đã hết hạn");
                else if (string.IsNullOrEmpty(combo.ProgramIds))
                    throw new Exception("Không tìm thấy chương trình học trong combo này");

                // thêm Bill
                var model = new tbl_Bill(itemModel);
                model.FullName = student?.FullName;
                model.UserCode = student?.UserCode;
                model.UserEmail = student?.Email;
                model.UserPhone = student?.Mobile;

                // Tạo mẫu gửi mail
                var sendStudent = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == model.StudentId);
                var getDiscound = await db.tbl_Discount.SingleOrDefaultAsync(x => x.Id == itemModel.DiscountId);
                if (getDiscound != null)
                {
                    model.DiscountCode = getDiscound?.Code;
                    model.DiscountId = getDiscound?.Id;
                    if (getDiscound.Type == 1)
                        model.DiscountPrice = getDiscound?.Value;
                    else if (getDiscound.Type == 2)
                        model.DiscountPrice = (itemModel.Price * getDiscound?.Value) / 100;
                }

                // Thông tin URL gắn vào thông báo
                UrlNotificationModels urlNotification = new UrlNotificationModels();

                // Kiểm tra quyền thanh toán
                var checkPaymentAllow = await CheckPaymentAllow(itemModel.Paid, user, db);
                if (checkPaymentAllow == false) model.IsApproved = false;
                else model.IsApproved = true;

                model.CreatedBy = model.ModifiedBy = user.FullName;

                string baseCode = "B";
                int count = await db.tbl_Bill.CountAsync(x =>
                            x.CreatedOn.Value.Year == model.CreatedOn.Value.Year
                            && x.CreatedOn.Value.Month == model.CreatedOn.Value.Month
                            && x.CreatedOn.Value.Day == model.CreatedOn.Value.Day);
                model.Code = AssetCRM.InitCode(baseCode, DateTime.Now, count + 1);

                db.tbl_Bill.Add(model);
                await db.SaveChangesAsync();

                //Kiểm tra quyền thanh toán, nếu không có quyền thanh toán sẽ gửi yêu cầu duyệt
                if (checkPaymentAllow == false)
                {
                    double money = itemModel.Paid;
                    itemModel.Paid = 0;
                    model.Paid = 0;
                    var paymentApprove = new tbl_PaymentApprove
                    {
                        BillId = model.Id,
                        CreatedBy = user.FullName,
                        CreateById = user.UserInformationId,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        Money = money,
                        Note = itemModel.Note,
                        PaymentMethodId = itemModel.PaymentMethodId,
                        PaymentDate = itemModel.PaymentDate,
                        Status = 1,
                        StatusName = "Chờ duyệt",
                        UserId = user.UserInformationId,
                    };
                    db.tbl_PaymentApprove.Add(paymentApprove);
                    await db.SaveChangesAsync();
                    BackgroundJob.Schedule(() => PaymentApproveNotification.NotifySendAPaymentApprovalRequest(new PaymentApproveNotificationRequest.NotifySendAPaymentApprovalRequestRequest
                    {
                        PaymentApproveId = paymentApprove.Id,
                        CurrentUser = user
                    }), TimeSpan.FromSeconds(2));
                }

                double totalPrice = 0;
                double reduced = 0;

                var programInCombo = await db.tbl_Program.Where(x => x.Enable == true && ("," + combo.ProgramIds + ",").Contains("," + x.Id + ",")).ToListAsync();
                if (!programInCombo.Any())
                    throw new Exception("Không tìm thấy chương trình trong combo");
                foreach (var item in programInCombo)
                {
                    var detail = new tbl_BillDetail(new BillDetailCreateV2());
                    detail.CreatedBy = detail.ModifiedBy = user.FullName;
                    detail.ComboId = combo.Id;
                    detail.BillId = model.Id;
                    detail.ProgramId = item.Id;
                    detail.Quantity = 1;
                    detail.Price = item.Price;
                    detail.TotalPrice = item.Price;
                    detail.StudentId = model.StudentId;
                    totalPrice += detail.TotalPrice;
                    db.tbl_BillDetail.Add(detail);
                    //Lưu thông tin khảo sát ngày học của học viên
                    tbl_ClassRegistration classRegistration = new tbl_ClassRegistration
                    {
                        BranchId = model.BranchId,
                        StudentId = model.StudentId,
                        ModifiedBy = user.FullName,
                        Price = detail.TotalPrice,
                        Status = 1,
                        StatusName = "Chờ xếp lớp",
                        ProgramId = item.Id,
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedOn = DateTime.Now,
                    };
                    db.tbl_ClassRegistration.Add(classRegistration);
                    await db.SaveChangesAsync();
                    var expectations = new List<StudentExpectationCreate>();
                    expectations = itemModel.Programs.FirstOrDefault(x => x.Id == item.Id)?.Expectations;
                    if (expectations != null)
                        foreach (var exp in expectations)
                        {
                            var expectation = new tbl_StudentExpectation(exp);
                            expectation.ClassRegistrationId = classRegistration.Id;
                            expectation.StudentId = itemModel.StudentId;
                            expectation.CreatedBy = expectation.ModifiedBy = user.FullName;
                            db.tbl_StudentExpectation.Add(expectation);
                        }

                    await db.SaveChangesAsync();
                }
                //Trừ tiền % combo
                model.TotalPriceCombo = totalPrice;
                if (combo.Type == (int)ComboType.Money)
                    model.ComboReduced = combo.Value ?? 0;
                else if (combo.Type == (int)ComboType.Percent)
                    model.ComboReduced = (model.TotalPriceCombo * (combo.Value ?? 0)) / 100;
                totalPrice = model.TotalPriceCombo.Value - (model.ComboReduced ?? 0);
                totalPrice = totalPrice < 0 ? 0 : totalPrice;

                if (itemModel.DiscountId.HasValue && totalPrice > 0 && itemModel.DiscountId > 0)
                {
                    var discount = await db.tbl_Discount.SingleOrDefaultAsync(x => x.Id == itemModel.DiscountId);
                    if (discount == null)
                        throw new Exception("Không tìm thấy khuyến mãi");
                    if (discount.PackageType != 2)
                        throw new Exception("Vui lòng mã khuyến mãi của gói combo");
                    if (discount.Status == 2)
                        throw new Exception("Khuyến mãi đã hết hạn");
                    if (discount.Quantity <= discount.UsedQuantity)
                        throw new Exception("Khuyến mãi đã dùng hết");

                    //Tính khuyến mãi
                    double newReduce = 0;
                    if (discount.Type == 1)
                        newReduce = discount.Value;
                    else
                    {
                        newReduce = totalPrice / 100 * discount.Value;
                        if (newReduce > discount.MaxDiscount)
                            newReduce = discount.MaxDiscount ?? 0;
                    }
                    discount.UsedQuantity += 1;
                    reduced += newReduce;
                }
                double usedMoneyReserve = 0;
                double debtTemp = totalPrice - (reduced + model.Paid);
                if (itemModel.ClassReserveId != 0 && itemModel.ClassReserveId.HasValue)// Sử dụng tiền bảo lưu để thanh toán
                {
                    var classReserve = await db.tbl_ClassReserve.SingleOrDefaultAsync(x => x.Id == itemModel.ClassReserveId.Value);
                    if (classReserve == null)
                        throw new Exception("Không tìm thấy thông tin bảo lưu");
                    if (classReserve.MoneyRemaining > debtTemp)
                    {
                        usedMoneyReserve = debtTemp;
                        classReserve.MoneyRemaining -= debtTemp;
                    }
                    else
                    {
                        usedMoneyReserve = classReserve.MoneyRemaining;
                        classReserve.MoneyRemaining = 0;
                        classReserve.Status = 2;
                        classReserve.StatusName = "Đã học lại";
                    }
                    classReserve.MoneyUsed += usedMoneyReserve;
                    await db.SaveChangesAsync();
                    model.UsedMoneyReserve = usedMoneyReserve;
                    model.ClassReserveId = classReserve.Id;
                }

                model.TotalPrice = totalPrice;
                model.Reduced = reduced;
                model.Debt = totalPrice - (reduced + model.Paid + usedMoneyReserve);
                if (model.Paid > 0)
                {
                    var paymentSession = new tbl_PaymentSession
                    {
                        BillId = model.Id,
                        BranchId = model.BranchId,
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        Type = 1,
                        TypeName = "Thu",
                        PaymentMethodId = itemModel.PaymentMethodId,
                        Reason = $"Thanh toán {tbl_Bill.GetTypeName(model.Type)}",
                        UserId = model.StudentId,
                        Note = model.Note,
                        Value = model.Paid,
                        PaymentDate = itemModel.PaymentDate ?? DateTime.Now
                    };
                    db.tbl_PaymentSession.Add(paymentSession);
                    await db.SaveChangesAsync();

                    // Chuẩn bị thông tin và tạo thời gian gắn vào tên QRCode để không bị trùng tên file
                    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string nameFile = "ThuChi_" + sendStudent.FullName.Replace(" ", "");
                    string folderToSave = "QRCodePaymentSession";
                    string name = $"{nameFile}_{timestamp}";
                    string host = urlNotification.url;
                    string data = host + urlNotification.urlPayMentSessionDetail + paymentSession.Id;
                    // Mặc định lưu ảnh với định dạng jpg
                    var QRCode = AssetCRM.CreateQRCodeV2(data, name, folderToSave, baseUrl);

                    // Lưu QRCode vào PaymentSession vừa tạo ở trên
                    var paymentSessionEntity = await db.tbl_PaymentSession.FirstOrDefaultAsync(x => x.Id == paymentSession.Id);
                    paymentSessionEntity.QRCode = QRCode;
                    await db.SaveChangesAsync();

                    //Tính doanh thu cho tư vấn viên
                    //nếu hóa đơn này là đăng ký học hoặc trả phí hàng tháng thì tính doanh thu
                    var sale = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.SaleId && x.Enable == true);
                    //nếu tìm thấy tư vấn viên
                    if (sale != null)
                    {
                        tbl_ConsultantRevenue consultantRevenue = new tbl_ConsultantRevenue();
                        consultantRevenue.SaleId = sale.UserInformationId;
                        consultantRevenue.StudentId = model.StudentId;
                        consultantRevenue.BillId = model.Id;
                        consultantRevenue.PaymentSessionId = paymentSession.Id;
                        consultantRevenue.TotalPrice = model.TotalPrice;
                        consultantRevenue.AmountPaid = model.Paid;
                        consultantRevenue.CreatedOn = DateTime.Now;
                        consultantRevenue.ModifiedOn = DateTime.Now;
                        consultantRevenue.Enable = true;
                        consultantRevenue.CreatedBy = consultantRevenue.ModifiedBy = user.FullName;
                        db.tbl_ConsultantRevenue.Add(consultantRevenue);
                        await db.SaveChangesAsync();
                        // cái này là chiến dịch hoa hồng gì đó =)))
                        tbl_SaleRevenue saleRevenue = new tbl_SaleRevenue();
                        saleRevenue.SaleId = sale.UserInformationId;
                        saleRevenue.BillId = model.Id;
                        saleRevenue.Value = model.Paid;
                        saleRevenue.CreatedBy = saleRevenue.ModifiedBy = user.FullName;
                        db.tbl_SaleRevenue.Add(saleRevenue);
                        //await SaleRevenueService.Insert(saleRevenue, user);
                    }
                }
                if (model.Debt <= 0)
                    model.CompleteDate = DateTime.Now;
                // thêm lịch sử học viên  
                var learningHistoryService = new LearningHistoryService(db);
                await learningHistoryService.Insert(new LearningHistoryCreate
                {
                    StudentId = itemModel.StudentId,
                    Content = $"Mua gói combo {combo?.Name}"
                });

                //kiểm tra xem có cần tạo hợp đồng hay không
                if (itemModel.IsCreateContract == true)
                {
                    var contract = new tbl_Contract();
                    contract.Name = "Hợp đồng cam kết chất lượng đầu ra";
                    contract.StudentId = itemModel.StudentId ?? 0;
                    contract.Content = itemModel.Content;
                    contract.Enable = true;
                    contract.CreatedOn = DateTime.Now;
                    contract.ModifiedOn = DateTime.Now;
                    contract.ModifiedBy = contract.CreatedBy = user.FullName;
                    db.tbl_Contract.Add(contract);
                    await db.SaveChangesAsync();
                }
                await db.SaveChangesAsync();

                BackgroundJob.Schedule(() => BillNotification.NotifyStudentSuccessfulByCombo(new BillNotificationRequest.NotifyStudentSuccessfulByComboRequest
                {
                    BillId = model.Id,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(2));

                BackgroundJob.Schedule(() => BillNotification.NotifyParentsSuccessfulByCombo(new BillNotificationRequest.NotifyParentsSuccessfulByComboRequest
                {
                    BillId = model.Id,
                    CurrentUser = user
                }), TimeSpan.FromSeconds(4));

                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var bill = await db.tbl_Bill.SingleOrDefaultAsync(x => x.Id == id);
                        if (bill == null)
                            throw new Exception("Không tìn thấy đơn");
                        if (bill.Type != 1 && bill.Type != 5 && bill.Type != 4)
                            throw new Exception($"Không thể hủy đơn {bill.TypeName}");
                        bill.Enable = false;
                        await db.SaveChangesAsync();
                        var billDetails = await db.tbl_BillDetail.Where(x => x.BillId == bill.Id && x.Enable == true).ToListAsync();
                        if (billDetails.Any())
                        {
                            if (bill.Type == 1)
                            {
                                foreach (var item in billDetails)
                                {
                                    item.Enable = false;
                                    if (item.ClassId != null && item.ClassId != 0)// đăng ký vào lớp
                                    {
                                        var studentInClass = await db.tbl_StudentInClass
                                            .FirstOrDefaultAsync(x => x.ClassId == item.ClassId && x.StudentId == bill.StudentId && x.Enable == true);
                                        if (studentInClass != null)
                                            studentInClass.Enable = false;
                                    }
                                    else// hẹn đăng ký
                                    {
                                        var classRegistration = await db.tbl_ClassRegistration
                                            .FirstOrDefaultAsync(x => x.ProgramId == item.ProgramId && x.StudentId == bill.StudentId && x.Enable == true);
                                        if (classRegistration != null)
                                            classRegistration.Enable = false;
                                    }
                                }
                            }
                            else if (bill.Type == 5)
                            {
                                foreach (var item in billDetails)
                                {
                                    item.Enable = false;
                                }
                                var monthlyTuitions = await db.tbl_MonthlyTuition.Where(x => x.BillId == bill.Id && x.Enable == true)
                                    .ToListAsync();
                                if (monthlyTuitions.Any())
                                {
                                    foreach (var monthlyTuition in monthlyTuitions)
                                    {
                                        monthlyTuition.Enable = false;
                                    }
                                }
                            }
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
        public class ClassReserveOptionModel
        {
            public int Id { get; set; }
            public int? ClassId { get; set; }
            public string ClassName { get; set; }
            /// <summary>
            /// 1 - Đang bảo lưu
            /// 2 - Đã học lại
            /// 3 - Đã hoàn tiền
            /// 4 - Hết hạn bảo lưu
            /// </summary>
            public int? Status { get; set; }
            public string StatusName { get; set; }
            public string Note { get; set; }
            /// <summary>
            /// Số tiền đã dùng
            /// </summary>
            public double MoneyUsed { get; set; }
            /// <summary>
            /// Số tiền còn lại
            /// </summary>
            public double MoneyRemaining { get; set; }
            /// <summary>
            /// Số tiên đã bảo lưu
            /// </summary>
            public double MoneyRefund { get; set; }
            public double Price { get; set; }
            public DateTime? CreatedOn { get; set; }
        }
        public static async Task<List<ClassReserveOptionModel>> GetClassReserveOption(ClassReserveOptionSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var result = new List<ClassReserveOptionModel>();
                var data = await db.tbl_ClassReserve.Where(x => x.StudentId == baseSearch.StudentId && x.Enable == true && x.Status == 1)
                    .Select(x => new
                    {
                        x.Id,
                        x.ClassId,
                        x.CreatedOn,
                        x.MoneyRefund,
                        x.MoneyRemaining,
                        x.MoneyUsed,
                        x.Note,
                        x.Status,
                        x.Price,
                        x.StatusName,
                    }).ToListAsync();
                if (data.Any())
                {
                    result = (from i in data
                              select new ClassReserveOptionModel
                              {
                                  Id = i.Id,
                                  ClassId = i.ClassId,
                                  CreatedOn = i.CreatedOn,
                                  MoneyRefund = i.MoneyRefund,
                                  MoneyRemaining = i.MoneyRemaining,
                                  MoneyUsed = i.MoneyUsed,
                                  Note = i.Note,
                                  Status = i.Status,
                                  StatusName = i.StatusName,
                                  Price = i.Price ?? 0,
                                  ClassName = Task.Run(() => ClassService.GetById(i.ClassId ?? 0)).Result?.Name
                              }).ToList();
                }
                return result;
            }
        }
        public static async Task<BillCreate> ValidateMonthlyBill(MonthlyBillCreate itemModel, lmsDbContext db)
        {
            var result = new BillCreate();
            var student = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId);
            if (student == null)
                throw new Exception("Vui lòng chọn học viên");
            var paymentMethod = await db.tbl_PaymentMethod.FirstOrDefaultAsync(x => x.Id == itemModel.PaymentMethodId);
            if (paymentMethod == null)
                throw new Exception("Vui lòng chọn phương thức thanh toán");
            var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == itemModel.BranchId);
            if (branch == null)
                throw new Exception("Vui lòng chọn chi nhánh");
            var itemClass = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == itemModel.ClassId);
            if (itemClass == null)
                throw new Exception("Vui lòng chọn lớp");
            if (!itemClass.IsMonthly)
                throw new Exception("Chỉ được thanh toán lớp theo tháng");
            if (itemClass.EndDay.HasValue && itemClass.EndDay.Value.Month < itemModel.Month)
                throw new Exception($"Lớp học này sẽ được kết thúc trong tháng {itemClass.EndDay.Value.Month}, không thể thanh toán tháng {itemModel.Month}");

            //Lấy những tháng chưa đóng của học viên trong lớp này
            var monthDebt = await db.tbl_StudentMonthlyDebt
                .FirstOrDefaultAsync(x => x.ClassId == itemModel.ClassId && x.Enable == true
                && x.StudentId == student.UserInformationId && x.Month.HasValue && x.Month.Value.Month == itemModel.Month);
            if (monthDebt != null && monthDebt.IsPaymentDone == true)
                throw new Exception("Học phí tháng này đã được thanh toán");


            result.Price = itemClass.Price;
            if (result.Price < result.Paid)
                throw new Exception("Vui lòng thanh toán đủ số tiền tối thiểu");

            result.StudentId = student.UserInformationId;
            result.PaymentMethodId = paymentMethod.Id;
            result.BranchId = branch.Id;
            result.Type = itemModel.Type;
            result.Paid = itemModel.Paid;
            result.PaymentAppointmentDate = itemModel.PaymentAppointmentDate;
            MonthlyDebtCreate debtCreate = new MonthlyDebtCreate()
            {
                ClassId = itemModel.ClassId,
                Quantity = 1
            };
            result.Classes = debtCreate;
            ////Lấy những tháng chưa đóng của học viên trong lớp này
            //var monthlyDebts = await db.tbl_StudentMonthlyDebt
            //    .Where(x => x.ClassId == itemModel.ClassId && x.Enable == true && x.IsPaymentDone == false 
            //    && x.StudentId == student.UserInformationId).ToListAsync();

            //if (monthlyDebts != null && monthlyDebts.Count > 0)
            //{
            //    //cập nhật trạng thái nợ
            //    int debtCount = monthlyDebts.Count();
            //    monthlyDebts = monthlyDebts.OrderBy(x => x.Month).ToList();
            //    int delta = (int)(itemModel.Quantity > debtCount ? debtCount : debtCount - itemModel.Quantity);
            //    for (int i = 0; i < delta; i++)
            //        monthlyDebts[i].IsPaymentDone = true;

            //    await db.SaveChangesAsync();
            //    //trừ đi các buổi chưa thanh toán
            //    if (debtCount < itemModel.Quantity)
            //    {
            //        MonthlyDebtCreate debtCreate = new MonthlyDebtCreate()
            //        {
            //            ClassId = itemModel.ClassId,
            //            Quantity = (int)(itemModel.Quantity - debtCount)
            //        };
            //        result.Classes = debtCreate;
            //    }
            //}

            return result;
        }
        public class PaymentCreate
        {
            public int Id { get; set; }
            public double Paid { get; set; }
            public string Note { get; set; }
            /// <summary>
            /// Ngày hẹn thanh toán
            /// </summary>
            public DateTime? PaymentAppointmentDate { get; set; }
            /// <summary>
            /// Ngày hóa đơn được thanh toán
            /// </summary>
            public DateTime? PaymentDate { get; set; }
            [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
            public int? PaymentMethodId { get; set; }
        }
        /// <summary>
        /// Thanh toán
        /// </summary>
        /// <returns></returns>
        public static async Task<tbl_Bill> Payment(PaymentCreate itemModel, string baseUrl, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entity = await db.tbl_Bill.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                        if (entity == null)
                            throw new Exception("Không tìm thấy hóa đơn");
                        if (itemModel.Paid == 0)
                            throw new Exception("Số tiền thanh toán không phù hợp");

                        //Kiểm tra quyền thanh toán, nếu không có quyền thanh toán sẽ gửi yêu cầu duyệt
                        var checkPaymentAllow = await CheckPaymentAllow(itemModel.Paid, user, db);
                        //var paymentAllow = await db.tbl_PaymentAllow.AnyAsync(x => x.UserId == user.UserInformationId && x.Enable == true);
                        if (checkPaymentAllow == false)
                        {
                            double money = itemModel.Paid;
                            itemModel.Paid = 0;
                            var paymentApprove = new tbl_PaymentApprove
                            {
                                BillId = entity.Id,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                Money = money,
                                Note = itemModel.Note,
                                PaymentMethodId = itemModel.PaymentMethodId,
                                PaymentDate = itemModel.PaymentDate,
                                CreateById = user.UserInformationId,
                                Status = 1,
                                StatusName = "Chờ duyệt",
                                UserId = user.UserInformationId
                            };
                            db.tbl_PaymentApprove.Add(paymentApprove);
                            await db.SaveChangesAsync();

                            BackgroundJob.Schedule(() => PaymentApproveNotification.NotifySendAPaymentApprovalRequest(new PaymentApproveNotificationRequest.NotifySendAPaymentApprovalRequestRequest
                            {
                                PaymentApproveId = paymentApprove.Id,
                                CurrentUser = user
                            }), TimeSpan.FromSeconds(2));

                            var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == entity.BranchId);

                            string content = $"Bạn đã yêu cầu duyệt thanh toán tại trung tâm {branch?.Name} vào thời gian {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}, vui lòng đợi duyệt.";
                            ParamOnList param = new ParamOnList { Search = paymentApprove.BillCode };
                            string paramString = JsonConvert.SerializeObject(param);
                            //Thread send = new Thread(async () =>
                            //{
                            //    await NotificationService.Send(new tbl_Notification
                            //    {
                            //        Content = content,
                            //        ContentEmail = content,
                            //        Title = "Yêu cầu duyệt thanh toán",
                            //        UserId = user.UserInformationId,
                            //        Type = 3,
                            //        ParamString = paramString,
                            //    }, user);
                            //    await NotificationService.Send(new tbl_Notification
                            //    {
                            //        Content = content,
                            //        ContentEmail = content,
                            //        Title = "Yêu cầu duyệt thanh toán",
                            //        UserId = entity.StudentId,
                            //        Type = 3,
                            //        ParamString = paramString,
                            //    }, user);
                            //});
                        }
                        else
                        {
                            if (checkPaymentAllow == false)
                                entity.IsApproved = false;
                            else
                                entity.IsApproved = true;
                            entity.Paid += itemModel.Paid;
                            entity.Debt = entity.TotalPrice - (entity.Reduced + entity.Paid);
                            entity.PaymentAppointmentDate = itemModel.PaymentAppointmentDate ?? entity.PaymentAppointmentDate;
                            entity.ModifiedOn = DateTime.Now;
                            entity.ModifiedBy = user.FullName;
                            if (entity.Debt <= 0)
                            {
                                entity.CompleteDate = DateTime.Now;

                                if (entity.Type == 5)
                                {
                                    var monthlytuitions = await db.tbl_MonthlyTuition.Where(x => x.BillId == entity.Id && x.Enable == true).ToListAsync();
                                    if (monthlytuitions.Any())
                                    {
                                        foreach (var item in monthlytuitions)
                                        {
                                            item.Status = 2;
                                            item.StatusName = "Đã thanh toán";
                                        }
                                        await db.SaveChangesAsync();
                                    }
                                }
                                else
                                {
                                    var details = await db.tbl_BillDetail
                                                .Where(x => x.BillId == entity.Id && x.CartId.HasValue && x.CartId != 0 && x.Enable == true)
                                                .ToListAsync();
                                    foreach (var item in details)
                                    {
                                        var product = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == item.ProductId);
                                        if (product.Type == 1)//Khóa video thì tạo mã active
                                        {
                                            List<string> activeCodes = await db.tbl_VideoActiveCode.Select(i => i.ActiveCode).ToListAsync();
                                            for (int i = 1; i <= item.Quantity; i++)
                                            {
                                                string activeCode = AssetCRM.RandomStringWithText(10);
                                                while (activeCodes.Any(ac => ac.Contains(activeCode)))
                                                {
                                                    activeCode = AssetCRM.RandomStringWithText(10);
                                                }
                                                var videoActiveCode = new tbl_VideoActiveCode
                                                {
                                                    ActiveCode = activeCode,
                                                    StudentId = item.StudentId,
                                                    ProductId = product.Id,
                                                    BillDetailId = item.Id,
                                                    CreatedBy = user.FullName,
                                                    CreatedOn = DateTime.Now,
                                                    Enable = true,
                                                    IsUsed = false,
                                                    ModifiedBy = user.FullName,
                                                    ModifiedOn = DateTime.Now
                                                };
                                                db.tbl_VideoActiveCode.Add(videoActiveCode);
                                                await db.SaveChangesAsync();
                                            }
                                        }
                                        else if (product.Type == 2)//Thêm bộ đề cho học viên
                                        {
                                            var packageStudent = new tbl_PackageStudent
                                            {
                                                CreatedBy = user.FullName,
                                                CreatedOn = DateTime.Now,
                                                Enable = true,
                                                ModifiedBy = user.FullName,
                                                ModifiedOn = DateTime.Now,
                                                PackageId = product.Id,
                                                StudentId = item.StudentId
                                            };
                                            db.tbl_PackageStudent.Add(packageStudent);
                                            product.TotalStudent += 1;
                                            await db.SaveChangesAsync();
                                        }
                                        else if (product.Type == 3)//Thêm lượt chấm cho học viên
                                        {
                                            var markQuantity = await db.tbl_MarkQuantity.FirstOrDefaultAsync(x => x.StudentId == item.StudentId && x.Enable == true);
                                            if (markQuantity == null)
                                            {
                                                markQuantity = new tbl_MarkQuantity
                                                {
                                                    CreatedBy = user.FullName,
                                                    CreatedOn = DateTime.Now,
                                                    Enable = true,
                                                    ModifiedBy = user.FullName,
                                                    ModifiedOn = DateTime.Now,
                                                    Quantity = product.MarkQuantity,
                                                    StudentId = item.StudentId,
                                                    UsedQuantity = 0,
                                                };
                                                db.tbl_MarkQuantity.Add(markQuantity);
                                            }
                                            else
                                            {
                                                markQuantity.Quantity += product.MarkQuantity;
                                                markQuantity.ModifiedBy = user.FullName;
                                                markQuantity.ModifiedOn = DateTime.Now;
                                            }
                                            await db.SaveChangesAsync();
                                        }
                                    }
                                }
                            }
                            var paymentSession = new tbl_PaymentSession
                            {
                                BillId = entity.Id,
                                BranchId = entity.BranchId,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                PaymentDate = itemModel.PaymentDate,
                                Enable = true,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                PaymentMethodId = itemModel.PaymentMethodId,
                                Reason = $"Thanh toán {tbl_Bill.GetTypeName(entity.Type)}",
                                UserId = entity.StudentId,
                                Note = itemModel.Note,
                                Type = 1,
                                TypeName = "Thu",
                                Value = itemModel.Paid,
                                PrintContent = Task.Run(() => PaymentSessionService.GetPrintContent(
                                    1,
                                    entity.StudentId,
                                    $"Thanh toán {tbl_Bill.GetTypeName(entity.Type)}",
                                    entity.Paid,
                                    user.FullName
                                    )).Result,
                            };
                            db.tbl_PaymentSession.Add(paymentSession);
                            await db.SaveChangesAsync();
                            // Chuẩn bị thông tin và tạo thời gian gắn vào tên QRCode để không bị trùng tên file
                            var userInfor = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == entity.StudentId);
                            if (userInfor != null)
                            {
                                // Thông tin URL gắn vào QRCode
                                UrlNotificationModels urlNotification = new UrlNotificationModels();
                                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                string nameFile = "ThuChi_" + userInfor.FullName.Replace(" ", "");
                                string folderToSave = "QRCodePaymentSession";
                                string name = $"{nameFile}_{timestamp}";
                                string host = urlNotification.url;
                                string data = host + urlNotification.urlPayMentSessionDetail + paymentSession.Id;
                                // Mặc định lưu ảnh với định dạng jpg
                                var QRCode = AssetCRM.CreateQRCodeV2(data, name, folderToSave, baseUrl);

                                // Lưu QRCode vào PaymentSession vừa tạo ở trên
                                var paymentSessionEntity = await db.tbl_PaymentSession.FirstOrDefaultAsync(x => x.Id == paymentSession.Id);
                                paymentSessionEntity.QRCode = QRCode;
                                await db.SaveChangesAsync();
                            }

                            // tính hoa hồng và tính doanh thu cho sale
                            var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.StudentId && x.Enable == true);
                            if (student != null)
                            {
                                var sale = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.SaleId && x.Enable == true && x.RoleId == 5);
                                if (sale != null) // tìm thấy tư vấn viên
                                {
                                    if (entity.Type == 1 || entity.Type == 3 || entity.Type == 5)
                                    {
                                        //tính doanh thu
                                        tbl_ConsultantRevenue consultantRevenue = new tbl_ConsultantRevenue();
                                        consultantRevenue.SaleId = sale.UserInformationId;
                                        consultantRevenue.StudentId = student.UserInformationId;
                                        consultantRevenue.BillId = itemModel.Id;
                                        consultantRevenue.PaymentSessionId = paymentSession.Id;
                                        consultantRevenue.TotalPrice = entity.TotalPrice;
                                        consultantRevenue.AmountPaid = itemModel.Paid;
                                        consultantRevenue.Enable = true;
                                        consultantRevenue.CreatedOn = DateTime.Now;
                                        consultantRevenue.ModifiedOn = DateTime.Now;
                                        consultantRevenue.CreatedBy = consultantRevenue.ModifiedBy = user.FullName;
                                        db.tbl_ConsultantRevenue.Add(consultantRevenue);
                                        await db.SaveChangesAsync();
                                    }
                                    //chiến dịch hoa hồng
                                    if (entity.Type == 1 || entity.Type == 3)
                                    {
                                        //tính hoa hồng
                                        tbl_SaleRevenue saleRevenue = new tbl_SaleRevenue();
                                        saleRevenue.SaleId = sale.UserInformationId;
                                        saleRevenue.BillId = itemModel.Id;
                                        saleRevenue.Value = itemModel.Paid;
                                        saleRevenue.CreatedBy = saleRevenue.ModifiedBy = user.FullName;
                                        saleRevenue.Enable = true;
                                        sale.CreatedOn = DateTime.Now;
                                        sale.ModifiedOn = DateTime.Now;
                                        db.tbl_SaleRevenue.Add(saleRevenue);
                                        //await SaleRevenueService.Insert(saleRevenue, user);
                                    }
                                }
                            }
                            await db.SaveChangesAsync();
                        }
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
        public class PaymentCreateV2
        {
            public int Id { get; set; }
            public double Paid { get; set; }
            public string Note { get; set; }
            /// <summary>
            /// Ngày hẹn thanh toán
            /// </summary>
            public DateTime? PaymentAppointmentDate { get; set; }
            [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
            public int? PaymentMethodId { get; set; }
            [Required(ErrorMessage = "Vui lòng chọn ngày thanh toán")]
            public DateTime? PaymentDate { get; set; }
        }
        // Thêm field PaymentDate khi thanh toán
        public static async Task<tbl_Bill> PaymentV2(PaymentCreateV2 itemModel, string baseUrl, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entity = await db.tbl_Bill.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                        if (entity == null)
                            throw new Exception("Không tìm thấy hóa đơn");
                        if (itemModel.Paid == 0)
                            throw new Exception("Số tiền thanh toán không phù hợp");

                        //Kiểm tra quyền thanh toán, nếu không có quyền thanh toán sẽ gửi yêu cầu duyệt
                        var paymentAllow = await db.tbl_PaymentAllow.AnyAsync(x => x.UserId == user.UserInformationId && x.Enable == true);
                        if (!paymentAllow && user.RoleId != (int)RoleEnum.admin
                            && user.RoleId != (int)RoleEnum.accountant
                            && user.RoleId != (int)RoleEnum.manager
                            && itemModel.Paid > 0)
                        {
                            double money = itemModel.Paid;
                            itemModel.Paid = 0;
                            var paymentApprove = new tbl_PaymentApprove
                            {
                                BillId = entity.Id,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                PaymentMethodId = itemModel.PaymentMethodId,
                                PaymentDate = itemModel.PaymentDate,
                                CreateById = user.UserInformationId,
                                Money = money,
                                Note = itemModel.Note,
                                Status = 1,
                                StatusName = "Chờ duyệt",
                                UserId = user.UserInformationId
                            };
                            db.tbl_PaymentApprove.Add(paymentApprove);
                            await db.SaveChangesAsync();

                            BackgroundJob.Schedule(() => PaymentApproveNotification.NotifySendAPaymentApprovalRequest(new PaymentApproveNotificationRequest.NotifySendAPaymentApprovalRequestRequest
                            {
                                PaymentApproveId = paymentApprove.Id,
                                CurrentUser = user
                            }), TimeSpan.FromSeconds(2));

                            var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == entity.BranchId);

                            string content = $"Bạn đã yêu cầu duyệt thanh toán tại trung tâm {branch?.Name} vào thời gian {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}, vui lòng đợi duyệt.";
                            ParamOnList param = new ParamOnList { Search = paymentApprove.BillCode };
                            string paramString = JsonConvert.SerializeObject(param);
                            //Thread send = new Thread(async () =>
                            //{
                            //    await NotificationService.Send(new tbl_Notification
                            //    {
                            //        Content = content,
                            //        ContentEmail = content,
                            //        Title = "Yêu cầu duyệt thanh toán",
                            //        UserId = user.UserInformationId,
                            //        Type = 3,
                            //        ParamString = paramString,
                            //    }, user);
                            //    await NotificationService.Send(new tbl_Notification
                            //    {
                            //        Content = content,
                            //        ContentEmail = content,
                            //        Title = "Yêu cầu duyệt thanh toán",
                            //        UserId = entity.StudentId,
                            //        Type = 3,
                            //        ParamString = paramString,
                            //    }, user);
                            //});
                        }
                        else
                        {
                            entity.Paid += itemModel.Paid;
                            entity.Debt = entity.TotalPrice - (entity.Reduced + entity.Paid);
                            entity.PaymentAppointmentDate = itemModel.PaymentAppointmentDate ?? entity.PaymentAppointmentDate;
                            entity.ModifiedOn = DateTime.Now;
                            entity.ModifiedBy = user.FullName;
                            if (entity.Debt <= 0)
                            {
                                entity.CompleteDate = DateTime.Now;

                                if (entity.Type == 5)
                                {
                                    var monthlytuitions = await db.tbl_MonthlyTuition.Where(x => x.BillId == entity.Id && x.Enable == true).ToListAsync();
                                    if (monthlytuitions.Any())
                                    {
                                        foreach (var item in monthlytuitions)
                                        {
                                            item.Status = 2;
                                            item.StatusName = "Đã thanh toán";
                                        }
                                        await db.SaveChangesAsync();
                                    }
                                }
                                else
                                {
                                    var details = await db.tbl_BillDetail
                                                .Where(x => x.BillId == entity.Id && x.CartId.HasValue && x.CartId != 0 && x.Enable == true)
                                                .ToListAsync();
                                    foreach (var item in details)
                                    {
                                        var product = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == item.ProductId);
                                        if (product.Type == 1)//Khóa video thì tạo mã active
                                        {
                                            List<string> activeCodes = await db.tbl_VideoActiveCode.Select(i => i.ActiveCode).ToListAsync();
                                            for (int i = 1; i <= item.Quantity; i++)
                                            {
                                                string activeCode = AssetCRM.RandomStringWithText(10);
                                                while (activeCodes.Any(ac => ac.Contains(activeCode)))
                                                {
                                                    activeCode = AssetCRM.RandomStringWithText(10);
                                                }
                                                var videoActiveCode = new tbl_VideoActiveCode
                                                {
                                                    ActiveCode = activeCode,
                                                    StudentId = item.StudentId,
                                                    ProductId = product.Id,
                                                    BillDetailId = item.Id,
                                                    CreatedBy = user.FullName,
                                                    CreatedOn = DateTime.Now,
                                                    Enable = true,
                                                    IsUsed = false,
                                                    ModifiedBy = user.FullName,
                                                    ModifiedOn = DateTime.Now
                                                };
                                                db.tbl_VideoActiveCode.Add(videoActiveCode);
                                                await db.SaveChangesAsync();
                                            }
                                        }
                                        else if (product.Type == 2)//Thêm bộ đề cho học viên
                                        {
                                            var packageStudent = new tbl_PackageStudent
                                            {
                                                CreatedBy = user.FullName,
                                                CreatedOn = DateTime.Now,
                                                Enable = true,
                                                ModifiedBy = user.FullName,
                                                ModifiedOn = DateTime.Now,
                                                PackageId = product.Id,
                                                StudentId = item.StudentId
                                            };
                                            db.tbl_PackageStudent.Add(packageStudent);
                                            product.TotalStudent += 1;
                                            await db.SaveChangesAsync();
                                        }
                                        else if (product.Type == 3)//Thêm lượt chấm cho học viên
                                        {
                                            var markQuantity = await db.tbl_MarkQuantity.FirstOrDefaultAsync(x => x.StudentId == item.StudentId && x.Enable == true);
                                            if (markQuantity == null)
                                            {
                                                markQuantity = new tbl_MarkQuantity
                                                {
                                                    CreatedBy = user.FullName,
                                                    CreatedOn = DateTime.Now,
                                                    Enable = true,
                                                    ModifiedBy = user.FullName,
                                                    ModifiedOn = DateTime.Now,
                                                    Quantity = product.MarkQuantity,
                                                    StudentId = item.StudentId,
                                                    UsedQuantity = 0,
                                                };
                                                db.tbl_MarkQuantity.Add(markQuantity);
                                            }
                                            else
                                            {
                                                markQuantity.Quantity += product.MarkQuantity;
                                                markQuantity.ModifiedBy = user.FullName;
                                                markQuantity.ModifiedOn = DateTime.Now;
                                            }
                                            await db.SaveChangesAsync();
                                        }
                                    }
                                }
                            }
                            var paymentSession = new tbl_PaymentSession
                            {
                                BillId = entity.Id,
                                BranchId = entity.BranchId,
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                PaymentMethodId = itemModel.PaymentMethodId,
                                Reason = $"Thanh toán {tbl_Bill.GetTypeName(entity.Type)}",
                                UserId = entity.StudentId,
                                Note = itemModel.Note,
                                Type = 1,
                                TypeName = "Thu",
                                Value = itemModel.Paid,
                                PrintContent = Task.Run(() => PaymentSessionService.GetPrintContent(
                                    1,
                                    entity.StudentId,
                                    $"Thanh toán {tbl_Bill.GetTypeName(entity.Type)}",
                                    entity.Paid,
                                    user.FullName
                                    )).Result,
                                PaymentDate = itemModel.PaymentDate
                            };
                            db.tbl_PaymentSession.Add(paymentSession);
                            await db.SaveChangesAsync();
                            // Chuẩn bị thông tin và tạo thời gian gắn vào tên QRCode để không bị trùng tên file
                            var userInfor = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == entity.StudentId);
                            if (userInfor != null)
                            {
                                // Thông tin URL gắn vào QRCode
                                UrlNotificationModels urlNotification = new UrlNotificationModels();
                                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                string nameFile = "ThuChi_" + userInfor.FullName.Replace(" ", "");
                                string folderToSave = "QRCodePaymentSession";
                                string name = $"{nameFile}_{timestamp}";
                                string host = urlNotification.url;
                                string data = host + urlNotification.urlPayMentSessionDetail + paymentSession.Id;
                                // Mặc định lưu ảnh với định dạng jpg
                                var QRCode = AssetCRM.CreateQRCodeV2(data, name, folderToSave, baseUrl);

                                // Lưu QRCode vào PaymentSession vừa tạo ở trên
                                var paymentSessionEntity = await db.tbl_PaymentSession.FirstOrDefaultAsync(x => x.Id == paymentSession.Id);
                                paymentSessionEntity.QRCode = QRCode;
                                await db.SaveChangesAsync();
                            }

                            // tính hoa hồng và tính doanh thu cho sale
                            var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.StudentId && x.Enable == true);
                            if (student != null)
                            {
                                var sale = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == student.SaleId && x.Enable == true && x.RoleId == 5);
                                if (sale != null) // tìm thấy tư vấn viên
                                {
                                    if (entity.Type == 1 || entity.Type == 3 || entity.Type == 5)
                                    {
                                        //tính doanh thu
                                        tbl_ConsultantRevenue consultantRevenue = new tbl_ConsultantRevenue();
                                        consultantRevenue.SaleId = sale.UserInformationId;
                                        consultantRevenue.StudentId = student.UserInformationId;
                                        consultantRevenue.BillId = itemModel.Id;
                                        consultantRevenue.PaymentSessionId = paymentSession.Id;
                                        consultantRevenue.TotalPrice = entity.TotalPrice;
                                        consultantRevenue.AmountPaid = itemModel.Paid;
                                        consultantRevenue.Enable = true;
                                        consultantRevenue.CreatedOn = DateTime.Now;
                                        consultantRevenue.ModifiedOn = DateTime.Now;
                                        consultantRevenue.CreatedBy = consultantRevenue.ModifiedBy = user.FullName;
                                        db.tbl_ConsultantRevenue.Add(consultantRevenue);
                                        await db.SaveChangesAsync();
                                    }
                                    //chiến dịch hoa hồng
                                    if (entity.Type == 1 || entity.Type == 3)
                                    {
                                        //tính hoa hồng
                                        tbl_SaleRevenue saleRevenue = new tbl_SaleRevenue();
                                        saleRevenue.SaleId = sale.UserInformationId;
                                        saleRevenue.BillId = itemModel.Id;
                                        saleRevenue.Value = itemModel.Paid;
                                        saleRevenue.CreatedBy = saleRevenue.ModifiedBy = user.FullName;
                                        saleRevenue.Enable = true;
                                        sale.CreatedOn = DateTime.Now;
                                        sale.ModifiedOn = DateTime.Now;
                                        db.tbl_SaleRevenue.Add(saleRevenue);
                                        //await SaleRevenueService.Insert(saleRevenue, user);
                                    }
                                }
                            }
                            await db.SaveChangesAsync();
                        }
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

        public class BillResult : AppDomainResult
        {
            public double SumtotalPrice { get; set; }
            public double SumPaid { get; set; }
            public double SumDebt { get; set; }
            public double SumReduced { get; set; }
            public int Type_All { get; set; }
            public int Type_Regis { get; set; }
            public int Type_Service { get; set; }
            public int Type_Tutorial { get; set; }
            public int Type_Manual { get; set; }
            public int Type_Monthly { get; set; }
            public int Type_ClassChange { get; set; }
        }
        public static async Task<BillResult> GetAll(BillSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new BillSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin && user.RoleId != (int)RoleEnum.student && user.RoleId != (int)RoleEnum.parents)
                    myBranchIds = user.BranchIds;
                if (user.RoleId == (int)RoleEnum.student)
                    baseSearch.StudentIds = user.UserInformationId.ToString();
                string sql = $"Get_Bill @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@Type = {baseSearch.Type ?? 0}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Id = {baseSearch.Id}," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@Status = {baseSearch.Status}," +
                    $"@StudentIds = N'{baseSearch.StudentIds ?? ""}'," +
                    $"@ParentIds = N'{baseSearch.ParentIds ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@FromDate = N'{baseSearch.FromDate ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'";
                var data = await db.SqlQuery<Get_Bill>(sql);
                if (!data.Any()) return new BillResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Bill(i)).ToList();
                double sumtotalPrice = data[0].SumTotalPrice;
                double sumPaid = data[0].SumPaid;
                double sumDebt = data[0].SumDebt;
                double sumReduced = data[0].SumReduced;
                int Type_All = data[0].Type_All;
                int Type_Regis = data[0].Type_Regis;
                int Type_Service = data[0].Type_Service;
                int Type_Tutorial = data[0].Type_Tutorial;
                int Type_Manual = data[0].Type_Manual;
                int Type_Monthly = data[0].Type_Monthly;
                int Type_ClassChange = data[0].Type_ClassChange;

                return new BillResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    Success = true,
                    SumDebt = sumDebt,
                    SumPaid = sumPaid,
                    SumReduced = sumReduced,
                    SumtotalPrice = sumtotalPrice,
                    Type_All = Type_All,
                    Type_Regis = Type_Regis,
                    Type_Service = Type_Service,
                    Type_Tutorial = Type_Tutorial,
                    Type_Manual = Type_Manual,
                    Type_Monthly = Type_Monthly,
                    Type_ClassChange = Type_ClassChange,
                };
            }
        }

        public static async Task<BillResult> GetAppointmentDueSoon(AppointmentDueSoonSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new AppointmentDueSoonSearch();
                string myBranchIds = "";
                if (user.RoleId != (int)RoleEnum.admin)
                    myBranchIds = user.BranchIds;
                if (user.RoleId == (int)RoleEnum.student)
                    baseSearch.StudentIds = user.UserInformationId.ToString();
                string sql = $"Get_AppointmentDueSoon @Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@Type = {baseSearch.Type ?? 0}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@StudentIds = N'{baseSearch.StudentIds ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@ToDate = N'{baseSearch.ToDate ?? ""}'";
                var data = await db.SqlQuery<Get_Bill>(sql);
                if (!data.Any()) return new BillResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Bill(i)).ToList();
                double sumtotalPrice = data[0].SumTotalPrice;
                double sumPaid = data[0].SumPaid;
                double sumDebt = data[0].SumDebt;
                int Type_All = data[0].Type_All;
                int Type_Regis = data[0].Type_Regis;
                int Type_Service = data[0].Type_Service;
                int Type_Tutorial = data[0].Type_Tutorial;
                int Type_Manual = data[0].Type_Manual;
                int Type_Monthly = data[0].Type_Monthly;
                int Type_ClassChange = data[0].Type_ClassChange;

                return new BillResult
                {
                    TotalRow = totalRow,
                    Data = result,
                    Success = true,
                    SumDebt = sumDebt,
                    SumPaid = sumPaid,
                    SumtotalPrice = sumtotalPrice,
                    Type_All = Type_All,
                    Type_Regis = Type_Regis,
                    Type_Service = Type_Service,
                    Type_Tutorial = Type_Tutorial,
                    Type_Manual = Type_Manual,
                    Type_Monthly = Type_Monthly,
                    Type_ClassChange = Type_ClassChange,
                };
            }
        }

        //public static async Task<AppDomainResult> GetDiscountHistory(BillSearch baseSearch, tbl_UserInformation user)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        if (baseSearch == null) baseSearch = new BillSearch();
        //        string myBranchIds = "";
        //        if (user.RoleId != ((int)RoleEnum.admin))
        //            myBranchIds = user.BranchIds;
        //        if (user.RoleId == ((int)RoleEnum.student))
        //            baseSearch.StudentIds = user.UserInformationId.ToString();
        //        string sql = $"Get_DiscountHistory @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
        //            $"@PageSize = {baseSearch.PageSize}," +
        //            $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
        //            $"@StudentIds = N'{baseSearch.StudentIds ?? ""}'," +
        //            $"@MyBranchIds = N'{myBranchIds ?? ""}'";
        //        var data = await db.SqlQuery<Get_Bill>(sql);
        //        if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
        //        var totalRow = data[0].TotalRow;
        //        return new AppDomainResult { TotalRow = totalRow, Data = data };
        //    }
        //}
        public class DiscountHistoryModel
        {
            public int Id { get; set; }
            public string BillCode { get; set; }
            public int DiscountId { get; set; }
            public string DiscountCode { get; set; }
            public double Reduced { get; set; }
            public DateTime? CreatedOn { get; set; }
        }
        public class DiscountHistorySearch
        {
            public int StudentId { get; set; }
            public DiscountHistorySearch()
            {

            }
        }
        public static async Task<List<DiscountHistoryModel>> GetDiscountHistory(DiscountHistorySearch baseSearch, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new DiscountHistorySearch();
                if (userLog.RoleId == (int)RoleEnum.student)
                    baseSearch.StudentId = userLog.UserInformationId;
                var result = new List<DiscountHistoryModel>();

                var bills = await db.tbl_Bill.Where(x => x.StudentId == baseSearch.StudentId && x.Enable == true && x.DiscountId != 0 && x.DiscountId.HasValue)
                    .Select(x => new
                    {
                        x.Id,
                        x.Code,
                        x.DiscountId,
                        x.Reduced,
                        x.CreatedOn
                    }).ToListAsync();
                if (!bills.Any())
                    return result;
                var discounts = await db.tbl_Discount.Where(x => x.Enable == true)
                    .Select(x => new
                    {
                        x.Id,
                        x.Code
                    }).ToListAsync();
                if (!discounts.Any())
                    return result;

                result = (from b in bills
                          join d in discounts on b.DiscountId equals d.Id
                          select new DiscountHistoryModel
                          {
                              Id = b.Id,
                              BillCode = b.Code,
                              DiscountId = b.DiscountId ?? 0,
                              DiscountCode = d.Code,
                              Reduced = b.Reduced,
                              CreatedOn = b.CreatedOn
                          }).ToList();
                return result;
            }
        }
        public static async Task<List<Get_BillDetail>> GetDetail(int billId)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_BillDetail @BillId= {billId}";
                var data = await db.SqlQuery<Get_BillDetail>(sql);
                if (data.Any())
                    foreach (var d in data)
                    {
                        if (d.Type == (int)lmsEnum.BillType.BuyComboPack)
                        {
                            d.ComboProgram = await db.tbl_BillDetail.Where(x => x.Enable == true && x.BillId == d.BillId)
                                .Select(x => new ComboProgram()
                                {
                                    ProgramId = x.ProgramId ?? 0,
                                    TotalPrice = x.TotalPrice,
                                }).ToListAsync();
                            if (d.ComboProgram.Any())
                                foreach (var p in d.ComboProgram)
                                {
                                    var program = await GetProgram(p.ProgramId, db);
                                    p.ProgramCode = program.Item1;
                                    p.ProgramName = program.Item2;
                                    p.GradeName = program.Item3;
                                }
                        }
                    }
                return data;
            }
        }
        public static async Task<(string, string, string)> GetProgram(int programId, lmsDbContext db)
        {
            var gradeName = string.Empty;
            var program = await db.tbl_Program.FirstOrDefaultAsync(x => x.Enable == true && x.Id == programId);
            if (program != null)
            {
                var grade = await db.tbl_Grade.FirstOrDefaultAsync(x => x.Enable == true && x.Id == program.GradeId);
                gradeName = grade?.Name;
            }
            return (program?.Code, program?.Name, gradeName);
        }
        //public static async Task PaymentNotification()
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        try
        //        {
        //            DateTime now = DateTime.UtcNow.AddHours(7);
        //            DateTime firstDay = new DateTime(now.Year, now.Month, now.Day, 00, 00, 00);
        //            DateTime lastDay = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
        //            var title = configuration.GetSection("MySettings:ProjectName").Value.ToString() + " thông báo hóa đơn";

        //            var data = await db.tbl_Bill.Where(x => x.Enable == true && x.Debt > 0 && x.PaymentAppointmentDate != null && x.PaymentAppointmentDate >= firstDay && x.PaymentAppointmentDate <= lastDay).ToListAsync();
        //            if (!data.Any())
        //                return;
        //            foreach (var item in data)
        //            {
        //                ParamOnList param = new ParamOnList { Search = item.Code };
        //                string paramString = JsonConvert.SerializeObject(param);
        //                var user = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == item.StudentId);
        //                if (user != null)
        //                {

        //                    tbl_Notification notification = new tbl_Notification()
        //                    {
        //                        Title = title,
        //                        Content = "Bạn có hóa đơn " + item.Code + " đã đến hạn thanh toán " + item.PaymentAppointmentDate.Value.ToString("dd/MM/yyyy") + " với số tiền là " + String.Format("{0:0,0}", item.Debt) + ".",
        //                        UserId = user.UserInformationId,
        //                        IsSeen = false,
        //                        CreatedBy = user.FullName,
        //                        CreatedOn = DateTime.Now,
        //                        Type = 0,
        //                        ParamString = paramString,
        //                        Enable = true
        //                    };
        //                    db.tbl_Notification.Add(notification);
        //                    await db.SaveChangesAsync();
        //                    Thread threadStudent = new Thread(() =>
        //                    {
        //                        AssetCRM.OneSignalPushNotifications(notification.Title, notification.Content, user.OneSignal_DeviceId);
        //                        if (user.IsReceiveMailNotification == true)
        //                            AssetCRM.SendMail(user.Email, notification.Title, notification.Content);
        //                    });
        //                    threadStudent.Start();
        //                }
        //                if (user.ParentId.HasValue)
        //                {
        //                    var parent = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == user.ParentId);
        //                    if (parent != null)
        //                    {
        //                        tbl_Notification notification = new tbl_Notification()
        //                        {
        //                            Title = title,
        //                            Content = "Học viên " + user.FullName + " có hóa đơn " + item.Code + " đã đến hạn thanh toán " + item.PaymentAppointmentDate.Value.ToString("dd/MM/yyyy") + " với số tiền là " + String.Format("{0:0,0}", item.Debt) + ".",
        //                            UserId = parent.UserInformationId,
        //                            IsSeen = false,
        //                            CreatedBy = user.FullName,
        //                            CreatedOn = DateTime.Now,
        //                            Type = 0,
        //                            ParamString = paramString,
        //                            Enable = true
        //                        };
        //                        db.tbl_Notification.Add(notification);
        //                        await db.SaveChangesAsync();
        //                        Thread threadParent = new Thread(() =>
        //                        {
        //                            AssetCRM.OneSignalPushNotifications(notification.Title, notification.Content, parent.OneSignal_DeviceId);
        //                            if (parent.IsReceiveMailNotification == true)
        //                                AssetCRM.SendMail(parent.Email, notification.Title, notification.Content);
        //                        });
        //                        threadParent.Start();
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            AssetCRM.Writelog("Bill", "NotificationPayment", 1, e.Message + e.InnerException);
        //        }
        //    }
        //}
        public class TuitionPackageOption
        {
            public int Id { get; set; }
            public string Code { get; set; }
            /// <summary>
            /// Số tháng
            /// </summary>
            public int Months { get; set; }
            /// <summary>
            /// 1 - Giảm theo số tiền
            /// 2 - Giảm theo phần trăm 
            /// </summary>
            public int DiscountType { get; set; }
            public string DiscountTypeName { get; set; }
            public double Discount { get; set; }
            public double? MaxDiscount { get; set; }
            public string Description { get; set; }
        }
        public static async Task<List<TuitionPackageOption>> GetTuitionPackageOption()
        {
            using (var db = new lmsDbContext())
            {
                return await db.tbl_TuitionPackage.Where(x => x.Enable == true)
                    .Select(x => new TuitionPackageOption
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Discount = x.Discount,
                        MaxDiscount = x.MaxDiscount,
                        DiscountType = x.DiscountType,
                        DiscountTypeName = x.DiscountTypeName,
                        Months = x.Months
                    }).ToListAsync();
            }
        }
        public static async Task<string> ExportBill(int BillId, string path, string pathViews, string domain)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var link = string.Empty;
                    var item = await db.tbl_Bill.SingleOrDefaultAsync(x => x.Id == BillId && x.Enable == true);
                    if (item != null)
                    {
                        string content = "";
                        string nullItem = "";
                        var nameFile = "";
                        int count = 1;
                        var row = "<tr>";
                        DateTime paymentAppointmentDate = DateTime.Now;

                        // Tìm thông tin học sinh trong bill
                        var student = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == item.StudentId);

                        //Tìm lớp học (ClassId) và khóa học (ProgramId) trong bill detail (1 bill có nhiều bill detail)
                        var billDetail = await db.tbl_BillDetail.Where(x => x.BillId == item.Id && x.Enable == true).ToListAsync();

                        //Tìm discount
                        var discount = await db.tbl_Discount.FirstOrDefaultAsync(x => x.Id == item.DiscountId);

                        // Đường dẫn
                        if (item.Type == 1)
                        {
                            content = File.ReadAllText($"{pathViews}/Base/BillRegisterToStudy.cshtml");
                        }
                        else if (item.Type == 2)
                        {
                            content = File.ReadAllText($"{pathViews}/Base/BillBuyServices.cshtml");
                        }
                        else if (item.Type == 3)
                        {
                            content = File.ReadAllText($"{pathViews}/Base/BillBuyServices.cshtml");
                        }
                        else if (item.Type == 4)
                        {
                            content = File.ReadAllText($"{pathViews}/Base/BillCreateManually.cshtml");
                        }
                        else if (item.Type == 5)
                        {
                            content = File.ReadAllText($"{pathViews}/Base/BillMonthlyTuition.cshtml");
                        }
                        else if (item.Type == 6)
                        {
                            content = File.ReadAllText($"{pathViews}/Base/BillClassChangeFee.cshtml");
                        }
                        //Gán record
                        var today = DateTime.Now.ToString("ddMMyyyHHmmss");

                        if (item.Type == 1)
                        {
                            nameFile = $"BillĐăngKýHọc{AssetCRM.RemoveUnicode(student.FullName).Replace(" ", "")}_{today}_report.pdf";
                        }
                        else if (item.Type == 2)
                        {
                            nameFile = $"BillMuaDịchVụ{AssetCRM.RemoveUnicode(student.FullName).Replace(" ", "")}_{today}_report.pdf";
                        }
                        else if (item.Type == 3)
                        {
                            //nameFile = $"BillMuaDịchVụ{AssetCRM.RemoveUnicode(student.FullName).Replace(" ", "")}_{today}_report.pdf";
                        }
                        else if (item.Type == 4)
                        {
                            nameFile = $"BillTạoThủCông{AssetCRM.RemoveUnicode(student.FullName).Replace(" ", "")}_{today}_report.pdf";
                        }
                        else if (item.Type == 5)
                        {
                            nameFile = $"BillHọcPhíHằngTháng{AssetCRM.RemoveUnicode(student.FullName).Replace(" ", "")}_{today}_report.pdf";
                        }
                        else if (item.Type == 6)
                        {
                            nameFile = $"BillPhíChuyểnLớp{AssetCRM.RemoveUnicode(student.FullName).Replace(" ", "")}_{today}_report.pdf";
                        }

                        string savePath = $"{path}/ExportBill/{nameFile}";
                        link = $"{domain}/Upload/ExportBill/{nameFile}";
                        var a = new StringBuilder();

                        // Phần header thông tin của file PDF
                        content = content.Replace("{createDay}", item.CreatedOn.ToString());
                        content = content.Replace("{createBy}", item.CreatedBy);
                        content = content.Replace("{billCode}", item.Code);
                        content = content.Replace("{typeName}", item.TypeName);

                        // Bảng cho type đăng ký lớp
                        if (item.Type == 1)
                        {
                            foreach (var bill in billDetail)
                            {
                                var findClass = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == bill.ClassId && x.Enable == true);
                                var program = await db.tbl_Program.FirstOrDefaultAsync(x => x.Id == bill.ProgramId && x.Enable == true);



                                if (count == 1)
                                {
                                    if (findClass != null)
                                    {
                                        content = content.Replace("{item1}", findClass.Name);
                                    }
                                    else
                                    {
                                        content = content.Replace("{item1}", nullItem);
                                    }
                                    if (program != null)
                                    {
                                        content = content.Replace("{item2}", program.Name);
                                    }
                                    else
                                    {
                                        content = content.Replace("{item1}", nullItem);
                                    }
                                    content = content.Replace("{item3}", bill.Price.ToString("#,##0"));
                                    content = content.Replace("{item4}", bill.Quantity.ToString());

                                    if (item.PaymentAppointmentDate != null)
                                    {
                                        paymentAppointmentDate = (DateTime)item.PaymentAppointmentDate;
                                        content = content.Replace("{item5}", paymentAppointmentDate.ToString("dd/MM/yyyy"));
                                    }
                                    else
                                    {
                                        content = content.Replace("{item5}", nullItem);
                                    }
                                    content = content.Replace("{item6}", item.Note);
                                }
                                else
                                {
                                    row = "<tr>";
                                    if (findClass != null)
                                    {
                                        row += $"<td>{findClass.Name}</td>";
                                    }
                                    else
                                    {
                                        row += "<td></td>";
                                    }

                                    if (program != null)
                                    {
                                        row += $"<td>{program.Name}</td>";
                                    }
                                    else
                                    {
                                        row += "<td></td>";
                                    }

                                    row += $"<td>{bill.Price.ToString("#,##0")}</td>";
                                    row += $"<td>{bill.Quantity}</td>";
                                    row += "</tr>";

                                    a.Append(row);
                                }
                                count++;
                            }
                        }

                        // Bảng cho type mua dịch vụ
                        if (item.Type == 2)
                        {
                            foreach (var bill in billDetail)
                            {
                                var product = await db.tbl_Product.FirstOrDefaultAsync(x => x.Id == bill.ProductId && x.Enable == true);
                                if (count == 1)
                                {
                                    if (product != null)
                                    {
                                        content = content.Replace("{item1}", product.Name);
                                    }
                                    else
                                    {
                                        content = content.Replace("{item1}", nullItem);
                                    }
                                    content = content.Replace("{item2}", bill.Price.ToString("#,##0"));
                                    content = content.Replace("{item3}", bill.Quantity.ToString());
                                    content = content.Replace("{item4}", item.TotalPrice.ToString("#,##0"));
                                }
                                else
                                {
                                    row = "<tr>";
                                    if (product != null)
                                    {
                                        row += $"<td>{product.Name}</td>";
                                    }
                                    else
                                    {
                                        row += "<td></td>";
                                    }
                                    row += $"<td>{bill.Price.ToString("#,##0")}</td>";
                                    row += $"<td>{bill.Quantity}</td>";
                                    row += $"<td>{item.TotalPrice.ToString("#,##0")}</td>";
                                    row += "</tr>";

                                    a.Append(row);
                                }
                            }
                            count++;
                        }

                        if (item.Type == 3) { }

                        // Bảng cho type tạo thủ công
                        if (item.Type == 4)
                        {
                            content = content.Replace("{item1}", item.Note);
                        }

                        // Bẳng cho type học phí hằng tháng
                        if (item.Type == 5)
                        {
                            foreach (var bill in billDetail)
                            {
                                var product = await db.tbl_Product.FirstOrDefaultAsync(x => x.Id == bill.ProductId && x.Enable == true);
                                var findClass = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == bill.ClassId && x.Enable == true);
                                if (count == 1)
                                {
                                    if (findClass != null)
                                    {
                                        content = content.Replace("{item1}", findClass.Name);
                                    }
                                    else
                                    {
                                        content = content.Replace("{item1}", nullItem);
                                    }
                                    content = content.Replace("{item2}", bill.Price.ToString("#,##0"));
                                    content = content.Replace("{item3}", bill.Quantity.ToString());
                                    content = content.Replace("{item4}", item.TotalPrice.ToString("#,##0"));
                                    if (item.PaymentAppointmentDate != null)
                                    {
                                        paymentAppointmentDate = (DateTime)item.PaymentAppointmentDate;
                                        content = content.Replace("{item5}", paymentAppointmentDate.ToString("dd/MM/yyyy"));
                                    }
                                    else
                                    {
                                        content = content.Replace("{item5}", nullItem);
                                    }
                                    content = content.Replace("{item6}", item.Note);
                                }
                                else
                                {
                                    row = "<tr>";
                                    if (findClass != null)
                                    {
                                        row += $"<td>{findClass.Name}</td>";
                                    }
                                    else
                                    {
                                        row += "<td></td>";
                                    }
                                    row += $"<td>{bill.Price.ToString("#,##0")}</td>";
                                    row += $"<td>{bill.Quantity}</td>";
                                    row += $"<td>{item.TotalPrice.ToString("#,##0")}</td>";
                                    row += "</tr>";

                                    a.Append(row);
                                }
                            }
                            count++;
                        }

                        // Bảng cho type phí chuyển lớp
                        if (item.Type == 6)
                        {
                            foreach (var bill in billDetail)
                            {
                                var findOldClass = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == bill.OldClassId && x.Enable == true);
                                var findNewClass = await db.tbl_Class.FirstOrDefaultAsync(x => x.Id == bill.NewClassId && x.Enable == true);
                                if (count == 1)
                                {
                                    if (findOldClass != null)
                                    {
                                        content = content.Replace("{item1}", findOldClass.Name);
                                    }
                                    else
                                    {
                                        content = content.Replace("{item1}", nullItem);
                                    }
                                    if (findNewClass != null)
                                    {
                                        content = content.Replace("{item2}", findNewClass.Name);
                                    }
                                    else
                                    {
                                        content = content.Replace("{item2}", nullItem);
                                    }
                                    content = content.Replace("{item3}", bill.Price.ToString("#,##0"));
                                    if (item.PaymentAppointmentDate != null)
                                    {
                                        paymentAppointmentDate = (DateTime)item.PaymentAppointmentDate;
                                        content = content.Replace("{item4}", paymentAppointmentDate.ToString("dd/MM/yyyy"));
                                    }
                                    else
                                    {
                                        content = content.Replace("{item4}", nullItem);
                                    }
                                    content = content.Replace("{item5}", item.Note);
                                }
                                else
                                {
                                    row = "<tr>";
                                    if (findOldClass != null)
                                    {
                                        row += $"<td>{findOldClass.Name}</td>";
                                    }
                                    else
                                    {
                                        row += "<td></td>";
                                    }
                                    if (findNewClass != null)
                                    {
                                        row += $"<td>{findNewClass.Name}</td>";
                                    }
                                    else
                                    {
                                        row += "<td></td>";
                                    }
                                    row += $"<td>{bill.Price.ToString("#,##0")}</td>";
                                    row += "</tr>";

                                    a.Append(row);
                                }
                            }
                            count++;
                        }

                        // Thêm hàng nếu có có nhiều thành phần
                        content = content.Replace("{RL}", a.ToString());

                        // Bảng giá
                        if (discount != null)
                        {
                            content = content.Replace("{discountCode}", discount.Code);
                        }
                        else
                        {
                            content = content.Replace("{discountCode}", nullItem);
                        }
                        content = content.Replace("{reduced}", item.Reduced.ToString("#,##0"));
                        content = content.Replace("{usedMoneyReserve}", item.UsedMoneyReserve.ToString());
                        content = content.Replace("{userCode}", student.UserCode);
                        content = content.Replace("{fullName}", student.FullName);
                        content = content.Replace("{total}", item.TotalPrice.ToString("#,##0"));
                        content = content.Replace("{paid}", item.Paid.ToString("#,##0"));
                        content = content.Replace("{unpaid}", item.Debt.ToString("#,##0"));

                        using (MemoryStream stream = new MemoryStream())
                        {
                            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
                            {
                                Path = path
                            });
                            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
                            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                            {
                                Headless = true,
                                ExecutablePath = browserFetcher.RevisionInfo(BrowserFetcher.DefaultRevision).ExecutablePath
                            });
                            var page = await browser.NewPageAsync();
                            await page.EmulateMediaTypeAsync(MediaType.Screen);
                            await page.SetContentAsync(content);
                            var pdfContent = await page.PdfStreamAsync(new PdfOptions
                            {
                                Format = PaperFormat.A4,
                                PrintBackground = true
                            });
                            await pdfContent.CopyToAsync(stream);

                            byte[] bytes = stream.ToArray();
                            stream.Close();
                            await browser.CloseAsync();

                            var newStream = new MemoryStream(bytes);

                            using (var fileStream = File.Create(savePath))
                            {
                                newStream.WriteTo(fileStream);
                            }
                        }
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy hóa đơn");
                    }
                    return link;
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
        }

        //public static async Task AutoNotifyDebtDaily()
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        var env = WebHostEnvironment.Environment;
        //        DateTime today = DateTime.Now;
        //        string content = "";
        //        string notificationContent = "";
        //        string nullItem = "Chưa có";
        //        int count = 1;
        //        var a = new StringBuilder();
        //        string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
        //        //var appRootPath = HttpRuntime.AppDomainAppPath;
        //        //var pathViews = Path.Combine(appRootPath, "Views");
        //        var pathViews = Path.Combine(env.ContentRootPath, "Views");
        //        content = System.IO.File.ReadAllText($"{pathViews}/Base/Mail/Bill/ListDebt.cshtml");
        //        // Lấy thông tin build
        //        var bills = await db.tbl_Bill.Where(x => x.Enable == true && x.Debt != 0).ToListAsync();
        //        // Tính tổng nợ
        //        decimal sumDebt = (decimal)bills.Sum(x => x.Debt);
        //        decimal totalDebt = Math.Floor(sumDebt);
        //        var admins = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.admin).ToListAsync();
        //        UrlNotificationModels urlNotification = new UrlNotificationModels();
        //        string url = urlNotification.urlBill;
        //        string urlEmail = urlNotification.url + url;
        //        if (bills.Count != 0)
        //        {

        //            foreach (var dataBill in bills)
        //            {
        //                // Thông tin khách hàng
        //                var customer = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == dataBill.StudentId);
        //                // Thông tin mã giảm giá
        //                var discount = await db.tbl_Discount.FirstOrDefaultAsync(x => x.Id == dataBill.DiscountId);
        //                // Thông tin chi nhánh
        //                var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == dataBill.BranchId);

        //                if (count == 1)
        //                {
        //                    content = content.Replace("{Today}", today.ToString("dd/MM/yyyy"));
        //                    content = content.Replace("{BranchName}", branch.Name);
        //                    content = content.Replace("{CodeBill}", dataBill.Code);
        //                    content = content.Replace("{TypeBill}", dataBill.TypeName);
        //                    content = content.Replace("{UserCode}", customer.UserCode);
        //                    content = content.Replace("{UserName}", customer.FullName);
        //                    content = content.Replace("{TotalPrice}", dataBill.TotalPrice.ToString("#,##0"));
        //                    content = content.Replace("{Paid}", dataBill.Paid.ToString("#,##0"));
        //                    content = content.Replace("{Debt}", dataBill.Debt.ToString("#,##0"));
        //                    if (discount == null) content = content.Replace("{DiscountCode}", nullItem);
        //                    else content = content.Replace("{DiscountCode}", discount.Code);
        //                    content = content.Replace("{Reduced}", dataBill.Reduced.ToString("#,##0"));
        //                    if (string.IsNullOrEmpty(dataBill.PaymentAppointmentDate.ToString())) content = content.Replace("{PaymentAppointmentDate}", nullItem);
        //                    else content = content.Replace("{PaymentAppointmentDate}", dataBill.PaymentAppointmentDate.Value.ToString("dd/MM/yyyy"));
        //                    content = content.Replace("{CreatedBy}", dataBill.CreatedBy);
        //                    content = content.Replace("{CreatedOn}", dataBill.CreatedOn.Value.ToString("dd/MM/yyyy"));
        //                    notificationContent = @"<div>" + content + @"</div>";
        //                }
        //                else
        //                {
        //                    string row = "<tr>";
        //                    row += $"<td class='custom-td' >{branch.Name}</td>";
        //                    row += $"<td class='custom-td'> <div>Mã: <strong>{dataBill.Code}</strong></div> <div>Loại: {dataBill.TypeName}</div> </td>";
        //                    row += $"<td class='custom-td'> <div>Mã: <strong>{customer.UserCode}</strong></div> <div>Tên KH: {customer.FullName}</div> </td>";
        //                    row += $"<td class='custom-td'> <div>Tổng tiền: <strong>{dataBill.TotalPrice.ToString("#,##0")}</strong></div> <div>Đã thanh toán: {dataBill.Paid.ToString("#,##0")} <div>Chưa thanh toán: <strong><span style='color:red;'>{dataBill.Debt.ToString("#,##0")}</span></strong></div></td>";
        //                    if (discount == null) row += $"<td class='custom-td'> <div>Mã giảm: {nullItem}</div> <div>Số tiền giảm: {dataBill.Reduced.ToString("#,##0")}</div> </td>";
        //                    else row += $"<td class='custom-td'> <div>Mã giảm: {discount.Code}</div> <div>Số tiền giảm: {dataBill.Reduced.ToString("#,##0")}</div> </td>";
        //                    if (string.IsNullOrEmpty(dataBill.PaymentAppointmentDate.ToString())) row += $"<td class='custom-td'>{nullItem}</td>";
        //                    else row += $"<td class='custom-td'>{dataBill.PaymentAppointmentDate.Value.ToString("dd/MM/yyyy")}</td>";
        //                    row += $"<td class='custom-td'> <div>Người tạo: {dataBill.CreatedBy}</div> <div>Ngày tạo: {dataBill.CreatedOn.Value.ToString("dd/MM/yyyy")}</div> </td>";
        //                    row += "</tr>";

        //                    a.Append(row);
        //                }
        //                count++;
        //            }
        //            content = content.Replace("{RL}", a.ToString());
        //            //Chữ kí
        //            content = content.Replace("{ProjectName}", projectName);
        //            content = content.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

        //            content = content.Replace("{TotalDebt}", totalDebt.ToString("#,##0"));
        //            notificationContent = @"<div>" + content + @"</div>";
        //            Thread sendTeacher = new Thread(async () =>
        //            {
        //                foreach (var ad in admins)
        //                {
        //                    tbl_Notification notification = new tbl_Notification();

        //                    notification.Title = "Thông Báo Tổng Công Nợ";
        //                    notification.ContentEmail = notificationContent;
        //                    notification.Content = "Tổng công nợ hôm nay là " + totalDebt.ToString("#,##0") + ". Vui lòng kiểm tra!";
        //                    notification.Type = 0;
        //                    notification.Category = 1;
        //                    notification.Url = url;
        //                    notification.UserId = ad.UserInformationId;
        //                    await NotificationService.Send(notification, ad, true);
        //                }
        //            });
        //            sendTeacher.Start();
        //        }
        //    }
        //}

        public class RemindNotification
        {
            public int StudentId = 0;
            public int ParentId = 0;
            public string StudentName = "";
            public string BillCode = "";
            public string contentRemind = "";
            public string Url = "";
        }

        public static async Task AutoRemindFeePaymentDueForStudent()
        {
            using (var db = new lmsDbContext())
            {
                var env = WebHostEnvironment.Environment.ContentRootPath;
                DateTime today = DateTime.Now;
                string nullItem = "Chưa có";
                string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
                var pathViews = Path.Combine(env, "Views");

                tbl_UserInformation user = new tbl_UserInformation
                {
                    FullName = "Tự động"
                };

                List<RemindNotification> remindNotificationToStudents = new List<RemindNotification>();
                // Lấy thông tin build
                var bills = await db.tbl_Bill.Where(x => x.Enable == true && x.Debt != 0).ToListAsync();
                bills = bills.Where(x => x.PaymentAppointmentDate?.Date == today.Date).ToList();
                if (bills.Count != 0)
                {
                    foreach (var b in bills)
                    {
                        string contentStudent = "";
                        contentStudent = File.ReadAllText($"{pathViews}/Base/Mail/Bill/PaymentReminder.cshtml");

                        var remindNotificationToStudent = new RemindNotification();
                        // Thông tin mã giảm giá
                        var discount = await db.tbl_Discount.FirstOrDefaultAsync(x => x.Id == b.DiscountId);
                        // Thông tin chi nhánh
                        var branch = await db.tbl_Branch.FirstOrDefaultAsync(x => x.Id == b.BranchId);
                        // Thông tin khách hàng
                        var customer = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == b.StudentId);

                        UrlNotificationModels urlNotification = new UrlNotificationModels();
                        string url = urlNotification.urlDetailStudent + customer.UserInformationId;
                        string urlEmail = urlNotification.url + url;

                        contentStudent = contentStudent.Replace("{TitleFullName}", "học sinh " + customer.FullName);
                        contentStudent = contentStudent.Replace("{FullName}", "Bạn");
                        contentStudent = contentStudent.Replace("{BranchName}", branch.Name);
                        contentStudent = contentStudent.Replace("{CodeBill}", b.Code);
                        contentStudent = contentStudent.Replace("{TypeBill}", b.TypeName);
                        contentStudent = contentStudent.Replace("{UserCode}", customer.UserCode);
                        contentStudent = contentStudent.Replace("{UserName}", customer.FullName);
                        contentStudent = contentStudent.Replace("{TotalPrice}", b.TotalPrice.ToString("#,##0"));
                        contentStudent = contentStudent.Replace("{Paid}", b.Paid.ToString("#,##0"));
                        contentStudent = contentStudent.Replace("{Debt}", b.Debt.ToString("#,##0"));
                        if (discount == null) contentStudent = contentStudent.Replace("{DiscountCode}", nullItem);
                        else contentStudent = contentStudent.Replace("{DiscountCode}", discount.Code);
                        contentStudent = contentStudent.Replace("{Reduced}", b.Reduced.ToString("#,##0"));
                        if (string.IsNullOrEmpty(b.PaymentAppointmentDate.ToString())) contentStudent = contentStudent.Replace("{PaymentAppointmentDate}", nullItem);
                        else contentStudent = contentStudent.Replace("{PaymentAppointmentDate}", b.PaymentAppointmentDate.Value.ToString("dd/MM/yyyy"));
                        contentStudent = contentStudent.Replace("{ProjectName}", projectName);
                        contentStudent = contentStudent.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

                        var notificationContent = @"<div>" + contentStudent + @"</div>";

                        remindNotificationToStudent.StudentId = customer.UserInformationId;
                        remindNotificationToStudent.StudentName = customer.FullName;
                        remindNotificationToStudent.BillCode = b.Code;
                        remindNotificationToStudent.contentRemind = notificationContent;
                        remindNotificationToStudent.Url = url;
                        remindNotificationToStudents.Add(remindNotificationToStudent);
                    }
                    //Thread sendStudent = new Thread(async () =>
                    //{
                    //    foreach (var st in remindNotificationToStudents)
                    //    {
                    //        tbl_Notification notification = new tbl_Notification();
                    //        notification.Title = "Thông Báo Đến Hạn Thanh Toán Hóa Đơn";
                    //        notification.ContentEmail = st.contentRemind;
                    //        notification.Content = "Bạn có hóa đơn đã đến hẹn thanh toán, mã hóa đơn là " + st.BillCode + ". Vui lòng kiểm tra!";
                    //        notification.Type = 0;
                    //        notification.UserId = st.StudentId;
                    //        notification.Category = 1;
                    //        notification.Url = st.Url;
                    //        notification.AvailableId = st.StudentId;
                    //        await NotificationService.Send(notification, user, true);
                    //    }
                    //});
                    //sendStudent.Start();
                }
            }
        }

        public static async Task AutoRemindFeePaymentDueForParent()
        {
            using (var db = new lmsDbContext())
            {
                DateTime today = DateTime.Now;
                string nullItem = "Chưa có";
                string projectName = configuration.GetSection("MySettings:ProjectName").Value.ToString();
                var appRootPath = WebHostEnvironment.Environment.ContentRootPath;
                var pathViews = Path.Combine(appRootPath, "Views");

                tbl_UserInformation user = new tbl_UserInformation
                {
                    FullName = "Tự động"
                };

                List<RemindNotification> remindNotificationToParents = new List<RemindNotification>();
                // Lấy thông tin build
                var bills = await db.tbl_Bill.Where(x => x.Enable == true && x.Debt != 0).ToListAsync();
                bills = bills.Where(x => x.PaymentAppointmentDate?.Date == today.Date).ToList();
                if (bills.Count != 0)
                {
                    foreach (var dataBill in bills)
                    {
                        string contentParent = "";
                        contentParent = File.ReadAllText($"{pathViews}/Base/Mail/Bill/PaymentReminder.cshtml");

                        var remindNotificationToParent = new RemindNotification();

                        // Thông tin khách hàng
                        var customer = db.tbl_UserInformation.FirstOrDefault(x => x.UserInformationId == dataBill.StudentId);
                        // Thông tin mã giảm giá
                        var discount = db.tbl_Discount.FirstOrDefault(x => x.Id == dataBill.DiscountId);
                        // Thông tin chi nhánh
                        var branch = db.tbl_Branch.FirstOrDefault(x => x.Id == dataBill.BranchId);
                        // Thông tin phụ huynh
                        var parent = db.tbl_UserInformation.FirstOrDefault(x => x.UserInformationId == customer.ParentId);

                        UrlNotificationModels urlNotification = new UrlNotificationModels();
                        string url = urlNotification.urlDetailStudent + customer.UserInformationId;
                        string urlEmail = urlNotification.url + url;
                        if (parent != null)
                        {
                            contentParent = contentParent.Replace("{TitleFullName}", "phụ huynh học sinh " + customer.FullName);
                            contentParent = contentParent.Replace("{FullName}", "Học sinh " + customer.FullName);
                            contentParent = contentParent.Replace("{BranchName}", branch.Name);
                            contentParent = contentParent.Replace("{CodeBill}", dataBill.Code);
                            contentParent = contentParent.Replace("{TypeBill}", dataBill.TypeName);
                            contentParent = contentParent.Replace("{UserCode}", customer.UserCode);
                            contentParent = contentParent.Replace("{UserName}", customer.FullName);
                            contentParent = contentParent.Replace("{TotalPrice}", dataBill.TotalPrice.ToString("#,##0"));
                            contentParent = contentParent.Replace("{Paid}", dataBill.Paid.ToString("#,##0"));
                            contentParent = contentParent.Replace("{Debt}", dataBill.Debt.ToString("#,##0"));
                            if (discount == null) contentParent = contentParent.Replace("{DiscountCode}", nullItem);
                            else contentParent = contentParent.Replace("{DiscountCode}", discount.Code);
                            contentParent = contentParent.Replace("{Reduced}", dataBill.Reduced.ToString("#,##0"));
                            if (string.IsNullOrEmpty(dataBill.PaymentAppointmentDate.ToString())) contentParent = contentParent.Replace("{PaymentAppointmentDate}", nullItem);
                            else contentParent = contentParent.Replace("{PaymentAppointmentDate}", dataBill.PaymentAppointmentDate.Value.ToString("dd/MM/yyyy"));
                            contentParent = contentParent.Replace("{ProjectName}", projectName);
                            contentParent = contentParent.Replace("{Url}", $"<a href=\"{urlEmail}\" target=\"_blank\">");

                            var notificationContent = @"<div>" + contentParent + @"</div>";

                            remindNotificationToParent.StudentId = customer.UserInformationId;
                            remindNotificationToParent.ParentId = parent.UserInformationId;
                            remindNotificationToParent.StudentName = customer.FullName;
                            remindNotificationToParent.BillCode = dataBill.Code;
                            remindNotificationToParent.contentRemind = notificationContent;
                            remindNotificationToParent.Url = url;
                            remindNotificationToParents.Add(remindNotificationToParent);
                        }
                    }
                    //Thread sendParent = new Thread(async () =>
                    //{
                    //    foreach (var p in remindNotificationToParents)
                    //    {
                    //        tbl_Notification notification = new tbl_Notification();

                    //        notification.Title = "Thông Báo Đến Hạn Thanh Toán Hóa Đơn";
                    //        notification.ContentEmail = p.contentRemind;
                    //        notification.Content = "Học sinh " + p.StudentName + " có hóa đơn đã đến hẹn thanh toán, mã hóa đơn là " + p.BillCode + ". Vui lòng kiểm tra!";
                    //        notification.Type = 0;
                    //        notification.UserId = p.ParentId;
                    //        notification.Category = 1;
                    //        notification.Url = p.Url;
                    //        notification.AvailableId = p.StudentId;
                    //        await NotificationService.Send(notification, user, true);
                    //    }
                    //});
                    //sendParent.Start();
                }
            }
        }
    }
}