using LMSCore.Areas.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using LMSCore.Models;

namespace LMSCore.Services
{
    public class LeadSwitchTestService : DomainService
    {
        public LeadSwitchTestService(lmsDbContext dbContext) : base(dbContext) { }
        public class Sale_LeadSwitchTest
        {
            public int SaleId { get; set; }
            public string SaleName { get; set; }
            public string SaleCode { get; set; }
            public double TotalLead { get; set; }
            public double TotalLeadSwitchTest { get; set; }
            public double ConversionRate { get; set; }
        }
        public async Task<AppDomainResult> GetAllSale(AllSaleSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new AllSaleSearch();
            if (!baseSearch.Year.HasValue)
                baseSearch.Year = DateTime.Now.Year;
            if (!baseSearch.Month.HasValue)
                baseSearch.Month = DateTime.Now.Month;
            List<Sale_LeadSwitchTest> result = new List<Sale_LeadSwitchTest>();
            var listSale = await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.sale).ToListAsync();
            if (baseSearch.Search != null)
                listSale = listSale.Where(x => x.FullName.Contains(baseSearch.Search) || x.UserCode.Contains(baseSearch.Search)).ToList();
            if (listSale.Count > 0)
            {
                foreach (var sale in listSale)
                {
                    double totalLead = await dbContext.tbl_Customer.Where(x => x.Enable == true && x.SaleId == sale.UserInformationId && x.CreatedOn.Value.Month == baseSearch.Month && x.CreatedOn.Value.Year == baseSearch.Year).CountAsync();
                    double totalLeadSwitchTest = await dbContext.tbl_LeadSwitchTest.Where(x => x.Enable == true && x.SaleId == sale.UserInformationId && x.CreatedOn.Value.Month == baseSearch.Month && x.CreatedOn.Value.Year == baseSearch.Year).CountAsync();
                    double conversionRate = 0;
                    if (totalLeadSwitchTest != 0)
                    {
                        conversionRate = Math.Round(totalLeadSwitchTest / totalLead * 100, 2);
                    }

                    var data = new Sale_LeadSwitchTest
                    {
                        SaleId = sale.UserInformationId,
                        SaleName = sale.FullName,
                        SaleCode = sale.UserCode,
                        TotalLead = totalLead,
                        TotalLeadSwitchTest = totalLeadSwitchTest,
                        ConversionRate = conversionRate
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

        public async Task<AppDomainResult> GetBySale(LeadSwitchTestSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new LeadSwitchTestSearch();
            if (!baseSearch.Year.HasValue)
                baseSearch.Year = DateTime.Now.Year;
            if (!baseSearch.Month.HasValue)
                baseSearch.Month = DateTime.Now.Month;
            string sql = $"Get_LeadSwitchTest @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@Year = N'{baseSearch.Year}'," +
                $"@Month = N'{baseSearch.Month}'," +
                $"@SaleId = {baseSearch.SaleId}";
            var data = await dbContext.SqlQuery<Get_LeadSwitchTest>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            var result = data.Select(i => new tbl_LeadSwitchTest(i)).ToList();
            return new AppDomainResult { TotalRow = totalRow, Data = result };
        }
    }
}