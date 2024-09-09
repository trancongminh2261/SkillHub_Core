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

namespace LMSCore.Services
{
    public class CustomerStatusService
    {
        public static async Task<tbl_CustomerStatus> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_CustomerStatus.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_CustomerStatus> Insert(CustomerStatusCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_CustomerStatus(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                model.Type = 2;
                db.tbl_CustomerStatus.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_CustomerStatus> Update(CustomerStatusUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_CustomerStatus.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (entity.Type != 2)
                {
                    entity.ColorCode = itemModel.ColorCode ?? entity.ColorCode;
                }
                else
                {
                    entity.Name = itemModel.Name ?? entity.Name;
                    entity.ColorCode = itemModel.ColorCode ?? entity.ColorCode;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                }
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_CustomerStatus.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (entity.Type != 2)
                    throw new Exception("Không được phép xóa trạng thái này");
                entity.Enable = false;
                await db.SaveChangesAsync();
                var customerStatus = await db.tbl_CustomerStatus.Where(x => x.Enable == true && x.Index.HasValue).OrderBy(x => x.Index).ToListAsync();
                if (customerStatus.Count != 0)
                {
                    int index = 1;
                    foreach (var h in customerStatus)
                    {
                        h.Index = index;
                        index++;
                    }
                    await db.SaveChangesAsync();
                }
            }
        }
        public static async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SearchOptions();
                var l = await db.tbl_CustomerStatus.Where(x => x.Enable == true).OrderBy(x => x.Index).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }

        public static async Task<AppDomainResult> GetAllV2(SearchOptions baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SearchOptions();
                var l = await db.tbl_CustomerStatus.Where(x => x.Enable == true).OrderBy(x => x.Name).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }

        public static async Task<List<Get_CustomerStatus>> GetStatusCount(CustomerStatusSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CustomerStatusSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin) && user.RoleId != ((int)RoleEnum.student) && user.RoleId != ((int)RoleEnum.parents))
                    myBranchIds = user.BranchIds;
                int saleId = 0;
                if (user.RoleId == ((int)RoleEnum.sale))
                    saleId = user.UserInformationId;
                string sql = $"Get_CustomerStatus @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@SaleId = {saleId}," +
                    $"@Name = N'{baseSearch.Name ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'";
                var data = await db.SqlQuery<Get_CustomerStatus>(sql);
                return data;
            }
        }

        public static async Task<List<Get_CustomerStatus>> GetStatusCountV2(CustomerStatusV2Search baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CustomerStatusV2Search();
                string myBranchIds = baseSearch.BranchIds ?? "";
                if (user.RoleId != ((int)RoleEnum.admin) && user.RoleId != ((int)RoleEnum.student) && user.RoleId != ((int)RoleEnum.parents))
                    myBranchIds = user.BranchIds;
                int saleId = 0;
                if (user.RoleId == ((int)RoleEnum.sale))
                    saleId = user.UserInformationId;
                string sql = $"Get_CustomerStatusV2 @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@SaleId = {saleId}," +
                    $"@Name = N'{baseSearch.Name ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'";
                var data = await db.SqlQuery<Get_CustomerStatus>(sql);
                return data;
            }
        }

        public static async Task<List<tbl_CustomerStatus>> UpdateIndexCustomerStatus(List<IndexCustomerStatusUpdate> request, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var customerStatusData = new List<tbl_CustomerStatus>();
                    var customerStatusModel = await db.tbl_CustomerStatus
                        .Where(x => x.Enable == true)
                        .ToListAsync();
                    var entity = request;
                    foreach (var item in request)
                    {
                        var customerStatus = customerStatusModel.SingleOrDefault(x => x.Id == item.Id);
                        if (customerStatus == null)
                            throw new Exception("Không tìm thấy thông tin ");
                        customerStatus.Index = item.Index;
                        customerStatusData.Add(customerStatus);
                    }
                    await db.SaveChangesAsync();
                    return customerStatusData;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }
}