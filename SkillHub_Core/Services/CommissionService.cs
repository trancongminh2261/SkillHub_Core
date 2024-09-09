using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Services
{
    public class CommissionService : DomainService
    {
        public CommissionService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_Commission> GetById(int id)
        {
            return await dbContext.tbl_Commission.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }

        //  tính dựa trên tbl_SaleRevenue
        // lấy chi tiết tiền 1 tư vấn viên kiếm được theo SaleId
        public async Task<AppDomainResult> GetAll(CommissionSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new CommissionSearch();
            string sql = $"Get_Commission @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@Year = {baseSearch.Year}," +
                $"@Month = {baseSearch.Month}";
            var data = await dbContext.SqlQuery<Get_CommissionSearch>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }


        // 1 là tạo Tự động vào cuối tháng, 2 là thanh toán r tạo, th tư vấn viên tháng đó k có đơn coi như k có bảng hoa hồng
        // 3 tạo Tự động vào đầu tháng
        //------------------------Cho tạo Tự động vào đầu tháng-------------------------------------------------//
        public static async Task AutoCreateCommission()
        {
            try
            {
                DateTime today = DateTime.Today;
                DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                bool isStartOfMonth = (today == firstDayOfMonth);
                if (isStartOfMonth)
                {
                    using (var db = new lmsDbContext())
                    {
                        int year = DateTime.Now.Year;
                        int month = DateTime.Now.Month;
                        // lấy chiến dịch
                        var compaign = await db.tbl_CommissionCampaign.FirstOrDefaultAsync(x => x.Year == year && x.Month == month);
                        if (compaign != null)
                        {
                            // danh sách nhân viên
                            var lstSale = await db.tbl_UserInformation.Where(x => x.RoleId == 5).ToListAsync();
                            foreach (var sale in lstSale)
                            {
                                int saleId = sale.UserInformationId;
                                // tổng tiền Nhân viên đó kiếm được
                                var totalTuitionFee = 0;
                                // tính % hoa hồng, lấy theo normDetail ở chiến dịch: CommissionCampaign
                                int percent = 0;

                                var model = new tbl_Commission
                                {
                                    SaleId = saleId,
                                    TotalTuitionFee = totalTuitionFee,
                                    CommissionCampaignId = compaign.Id,
                                    Percent = 0,
                                    Commission = 0,  //totalTuitionFee * (percent * 0.01),
                                    CreatedBy = "Tự động",
                                    CreatedOn = DateTime.Now,
                                    ModifiedBy = "Tự động",
                                    ModifiedOn = DateTime.Now
                                };
                                db.tbl_Commission.Add(model);
                            }
                        await db.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //--------------------------------------------------------------------------------------------------------//
        public static async Task InsertForOneSale(int saleId, tbl_CommissionCampaign compaign)
        {
            try
            {
                using (var db = new lmsDbContext())
                {
                    int year = DateTime.Now.Year;
                    int month = DateTime.Now.Month;
                    //lấy tổng tiền Nhân viên đó kiếm được
                    var totalTuitionFee = CalculateTotalTuitionFee(saleId, month, year);
                    // tính % hoa hồng, lấy theo normDetail ở chiến dịch: CommissionCampaign
                    double percent = 0;
                    //var listNorm = JsonConvert.DeserializeObject<List<tbl_CommissionNorm>>(compaign.NormDetail);
                    //foreach (var norm in listNorm)
                    //{
                    //    if ((totalTuitionFee >= norm.MinNorm && totalTuitionFee <= norm.MaxNorm) || totalTuitionFee > norm.MaxNorm)
                    //    {
                    //        percent = norm.PercentNew > percent ? norm.PercentNew : percent;
                    //    }
                    //}
                    var model = new tbl_Commission
                    {
                        SaleId = saleId,
                        TotalTuitionFee = totalTuitionFee,
                        CommissionCampaignId = compaign.Id,
                        Percent = 0,
                        Commission = totalTuitionFee * (percent * 0.01),
                        CreatedBy = "Tự động",
                        CreatedOn = DateTime.Now,
                        ModifiedBy = "Tự động",
                        ModifiedOn = DateTime.Now
                    };
                    db.tbl_Commission.Add(model);
                    await db.SaveChangesAsync();
                    //return model;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        ////---------------------------------Update hoa hồng-------------------------------------//
        public static async Task Update(int saleId, int year, int month, tbl_CommissionCampaign compaign)
        {
            try
            {
                using (var db = new lmsDbContext())
                {
                    //lấy tổng tiền Nhân viên đó kiếm được
                    var totalTuitionFee = CalculateTotalTuitionFee(saleId, month, year);
                    // tính % hoa hồng, lấy theo normDetail ở chiến dịch: CommissionCampaign
                    double percent = 0;
                    //var listNorm = JsonConvert.DeserializeObject<List<tbl_CommissionNorm>>(compaign.NormDetail);
                    //foreach (var norm in listNorm)
                    //{
                    //    if ((totalTuitionFee >= norm.MinNorm && totalTuitionFee <= norm.MaxNorm) || totalTuitionFee > norm.MaxNorm)
                    //    {
                    //        percent = norm.PercentNew > percent ? norm.PercentNew : percent;
                    //    }
                    //}
                    DateTime datetimeStart = new DateTime(year, month, 1);
                    DateTime datetimeEnd = new DateTime(year, month, 1).AddMonths(1);
                    var entity = await db.tbl_Commission.FirstOrDefaultAsync(x => x.Id == saleId
                                                                                       && x.Enable == true
                                                                                       && x.CreatedOn >= datetimeStart
                                                                                       && x.CreatedOn <= datetimeEnd);
                    entity.SaleId = saleId;
                    entity.TotalTuitionFee = totalTuitionFee;
                    entity.CommissionCampaignId = compaign.Id;
                    entity.Percent = percent;
                    entity.Commission = totalTuitionFee * (percent * 0.01);
                    entity.CreatedBy = "Tự động";
                    entity.CreatedOn = DateTime.Now;
                    entity.ModifiedBy = "Tự động";
                    entity.ModifiedOn = DateTime.Now;
                    await db.SaveChangesAsync();
                    //return entity;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //--------------------------Tính tổng thu theo tháng-------------------------------//
        public static double CalculateTotalTuitionFee(int saleId, int year, int month)
        {
            using (var db = new lmsDbContext())
            {
                var dateS = new DateTime(year, month, 1);
                var dateE = new DateTime(year, month + 1, 1);
                var total = db.tbl_SaleRevenue.Where(x => x.SaleId == saleId
                                                                && x.CreatedOn >= dateS
                                                                && x.CreatedOn <= dateE).Sum(s => s.Value);
                return total;
            }
        }

        public static async Task InsertOrUpdate(int SaleId)
        {
            using (var db = new lmsDbContext())
            {
                int saleId = SaleId;
                //var checkSale = await db.tbl_UserInformation.AnyAsync(x => x.UserInformationId == saleId && x.RoleId == 5);
                //if (!checkSale)
                //    throw new Exception("Không tìm thấy tư vấn viên này");
                // datetime
                DateTime today = DateTime.Today;
                int year = today.Year;
                int month = today.Month;
                DateTime datetimeStart = new DateTime(year, month, 1);
                DateTime datetimeEnd = new DateTime(year, month, 1).AddMonths(1);
                // lấy cmn chiến dịch
                var compaign = await db.tbl_CommissionCampaign.FirstOrDefaultAsync(x => x.Year == year
                                                                                            && x.Month == month
                                                                                            && x.Enable == true);
                if (compaign != null) // k có chiến dịch hoa hồng thì bỏ qua
                {
                    //Kiểm tra thông tin hoa hồng
                    var entity = await db.tbl_Commission.AnyAsync(x => x.Id == SaleId
                                                                                       && x.Enable == true
                                                                                       && x.CreatedOn >= datetimeStart
                                                                                       && x.CreatedOn <= datetimeEnd);

                    if (entity) // nếu có thông tin hoa hồng
                    {
                        await Update(saleId, year, month, compaign);
                    }
                    else
                    {
                        await InsertForOneSale(saleId, compaign);
                    }
                }
            }
        }
        public async Task Delete(int id)
        {
            try
            {
                var entity = await dbContext.tbl_Commission.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy thông tin hoa hồng");
                entity.Enable = false;
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}