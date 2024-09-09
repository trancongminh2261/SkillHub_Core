using LMSCore.Areas.Models;
using LMSCore.Models;

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class ConsultantRevenueService : DomainService
    {
        public ConsultantRevenueService(lmsDbContext dbContext) : base(dbContext) { }
        public class Sale_ConsultantRevenue
        {
            public int SaleId { get; set; }
            public string SaleName { get; set; }
            public string SaleCode { get; set; }
            public double TotalRevenue { get; set; }
        }
        public async Task<AppDomainResult> GetAllSale(AllSaleSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new AllSaleSearch();
            List<Sale_ConsultantRevenue> result = new List<Sale_ConsultantRevenue>();
            var listSale = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.sale).ToListAsync();
            if(baseSearch.Search != null)
                listSale = listSale.Where(x=> x.FullName.Contains(baseSearch.Search) || x.UserCode.Contains(baseSearch.Search)).ToList();
            if (listSale.Count > 0)
            {
                foreach(var sale in listSale)
                {
                    double totalRevenue = 0.0;
                    var listRevenue = await dbContext.tbl_ConsultantRevenue.Where(x => x.Enable == true && x.SaleId == sale.UserInformationId && x.CreatedOn.Value.Month == baseSearch.Month && x.CreatedOn.Value.Year == baseSearch.Year).ToListAsync();
                    if (listRevenue.Count > 0)
                        totalRevenue = listRevenue.Sum(x => x.AmountPaid);
                    var data = new Sale_ConsultantRevenue
                    {
                        SaleId = sale.UserInformationId,
                        SaleName = sale.FullName,
                        SaleCode = sale.UserCode,
                        TotalRevenue = totalRevenue,
                    };
                    result.Add(data);
                }
            }
            
            if (!result.Any())
                return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
            var totalRow = result.Count;
            result = result.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result, Success = true };
        }

        public async Task<AppDomainResult> GetBySale(ConsultantRevenueSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new ConsultantRevenueSearch();
            if (!baseSearch.Year.HasValue)
                baseSearch.Year = DateTime.Now.Year;
            if (!baseSearch.Month.HasValue)
                baseSearch.Month = DateTime.Now.Month;
            string sql = $"Get_ConsultantRevenue @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@Year = N'{baseSearch.Year}'," +
                $"@Month = N'{baseSearch.Month}'," +
                $"@SaleId = {baseSearch.SaleId}";
            var data = await dbContext.SqlQuery<Get_ConsultantRevenue>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            var result = data.Select(i => new tbl_ConsultantRevenue(i)).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
    }
}