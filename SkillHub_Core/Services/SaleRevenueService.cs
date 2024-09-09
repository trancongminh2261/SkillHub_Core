using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class SaleRevenueService : DomainService
    {
        public SaleRevenueService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_SaleRevenue> GetById(int id)
        {
            return await dbContext.tbl_SaleRevenue.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<AppDomainResult> GetAll(SaleRevenueSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new SaleRevenueSearch();
            string sql = $"Get_SaleRevenue @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@SaleId = {baseSearch.SaleId}";
            var data = await dbContext.SqlQuery<Get_SaleRevenue>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }

        // mỗi lần hv thanh toán khóa học, hv trả tiền còn nợ thì thêm 
        public static async Task Insert(SaleRevenueCreate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                //if (itemModel.Value < 0)
                //    throw new Exception("Số tiền không hợp lý");
                using (var db = new lmsDbContext())
                {
                    //var check = await db.tbl_UserInformation.AnyAsync(x => x.UserInformationId == itemModel.SaleId && x.Enable == true && x.RoleId == 5);
                    //if (check) 
                    // Đã tìm thấy nhân viên đó mới gọi hàm này
                    //throw new Exception("Không tìm thấy tư vấn viên này"); 
                    var model = new tbl_SaleRevenue(itemModel);
                    model.CreatedBy = model.ModifiedBy = userLog.FullName;
                    db.tbl_SaleRevenue.Add(model);
                    await db.SaveChangesAsync();
                    //return model;
                    // tính hoa hồng 
                    await CommissionService.InsertOrUpdate(itemModel.SaleId);

                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}