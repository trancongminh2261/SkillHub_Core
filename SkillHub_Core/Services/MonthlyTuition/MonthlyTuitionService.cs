using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class MonthlyTuitionService : DomainService
    {
        public MonthlyTuitionService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_MonthlyTuition> GetById(int id)
        {
            return await dbContext.tbl_MonthlyTuition.SingleOrDefaultAsync(x => x.Id == id);
        }
        public class AddMonthlyTuitionModel
        {
            /// <summary>
            /// Lớp học
            /// </summary>
            [Required(ErrorMessage = "Vui lòng chọn lớp")]
            public int? ClassId { get; set; }
            /// <summary>
            /// Tháng
            /// </summary>
            public int Month { get; set; }
            /// <summary>
            /// Năm
            /// </summary>
            public int Year { get; set; }
        }
        public async Task AddItems(AddMonthlyTuitionModel itemModel, tbl_UserInformation userLog)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var _class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.ClassId);
                    if (_class == null)
                        throw new Exception("Không tìm thấy giáo viên");
                    var date = new DateTime(itemModel.Year,itemModel.Month,01);
                    var studentIds = await dbContext.tbl_StudentInClass
                        .Where(x => x.ClassId == _class.Id && x.Enable == true && x.Type == 1 && x.StudentId.HasValue)
                        .Select(x => x.StudentId).ToListAsync();
                    if (!studentIds.Any())
                        throw new Exception("Không tìm thấy học viên học chính thức");
                    foreach (var studentId in studentIds)
                    {
                        await AddItem(new AddItemModel
                        {
                            StudentId = studentId.Value,
                            Class = _class,
                            CreateBy = userLog.FullName,
                            Date = date
                        });
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
        public class AddItemModel
        {
            public int StudentId { get; set; }
            public tbl_Class Class { get; set; }
            public DateTime Date { get; set; }
            public string CreateBy { get; set; }
        }
        public async Task AddItem(AddItemModel itemModel)
        {
            var hasTuition = await dbContext.tbl_MonthlyTuition
                .AnyAsync(x => x.ClassId == itemModel.Class.Id && x.StudentId == itemModel.StudentId && x.Year == itemModel.Date.Year && x.Month == itemModel.Date.Month && x.Enable == true);
            if (hasTuition)
                return;

            var bill = new tbl_Bill
            {
                BranchId = itemModel.Class.BranchId ?? 0,
                ClassReserveId = 0,
                CreatedBy = itemModel.CreateBy,
                CreatedOn = DateTime.Now,
                Debt = itemModel.Class.Price,
                DiscountId = 0,
                Enable = true,
                ModifiedBy = itemModel.CreateBy,
                ModifiedOn = DateTime.Now,
                Note = $"Học phí tháng {itemModel.Date.Month} [Lớp: {itemModel.Class.Name}] ",
                Paid = 0,
                StudentId = itemModel.StudentId,
                TotalPrice = itemModel.Class.Price,
                Type = 5,
                TypeName = "Học phí hằng tháng",
            };

            string baseCode = "B";
            int count = await dbContext.tbl_Bill.CountAsync(x =>
                        x.CreatedOn.Value.Year == bill.CreatedOn.Value.Year
                        && x.CreatedOn.Value.Month == bill.CreatedOn.Value.Month
                        && x.CreatedOn.Value.Day == bill.CreatedOn.Value.Day);
            bill.Code = AssetCRM.InitCode(baseCode, DateTime.Now, count + 1);

            dbContext.tbl_Bill.Add(bill);
            await dbContext.SaveChangesAsync();

            var billDetail = new tbl_BillDetail
            {
                BillId = bill.Id,
                ClassId = itemModel.Class.Id,
                CreatedBy = itemModel.CreateBy,
                CreatedOn = DateTime.Now,
                Enable = true,
                ModifiedBy = itemModel.CreateBy,
                ModifiedOn = DateTime.Now,
                Quantity = 1,
                Price = itemModel.Class.Price,
                StudentId = itemModel.StudentId,
                TotalPrice = itemModel.Class.Price
            };
            dbContext.tbl_BillDetail.Add(billDetail);

            dbContext.tbl_MonthlyTuition.Add(new tbl_MonthlyTuition
            {
                BillId = bill.Id,
                ClassId = itemModel.Class.Id,
                CreatedBy = itemModel.CreateBy,
                CreatedOn = DateTime.Now,
                Enable = true,
                ModifiedBy = itemModel.CreateBy,
                ModifiedOn = DateTime.Now,
                Month = itemModel.Date.Month,
                Year = itemModel.Date.Year,
                Status = 1,
                StatusName = "Chưa thanh toán",
                StudentId = itemModel.StudentId,
            });
            await dbContext.SaveChangesAsync();
        }
        public async Task<AppDomainResult> GetAll(MonthlyTuitionSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new MonthlyTuitionSearch();
            string sql = $"Get_MonthlyTuition @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@StudentId = {baseSearch.StudentId}," +
                $"@ClassId = {baseSearch.ClassId}," +
                $"@Month = {baseSearch.Month}," +
                $"@Year = {baseSearch.Year}," +
                $"@Search = N'{baseSearch.Search ?? ""}'";
            var data = await dbContext.SqlQuery<Get_MonthlyTuition>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            var result = data.Select(i => new tbl_MonthlyTuition
            { 
                Id = i.Id,
                BillId = i.BillId,
                ClassId = i.ClassId,
                ClassName = i.ClassName,
                CreatedBy = i.CreatedBy,
                CreatedOn = i.CreatedOn,
                Enable = i.Enable,
                ModifiedBy = i.ModifiedBy,
                ModifiedOn = i.ModifiedOn,
                Month = i.Month,
                Year = i.Year,
                Status = i.Status,
                StatusName = i.StatusName,
                StudentCode = i.StudentCode,
                StudentId = i.StudentId,
                StudentName = i.StudentName,
                Price = Task.Run(()=> GetPrice(i.BillId)).Result
            }).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
        public async Task<double> GetPrice(int billId)
        {
            var billDetail = await dbContext.tbl_BillDetail.Where(x => x.BillId == billId && x.Enable == true).FirstOrDefaultAsync();
            if (billDetail == null)
                return 0;
            return billDetail.Price;
        }
        
    }
}