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
    public class DayOffService
    {
        public static async Task<tbl_DayOff> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_DayOff.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_DayOff> Insert(DayOffCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                itemModel.sDate = itemModel.sDate.Value.Date;
                itemModel.eDate = itemModel.eDate.Value.Date;
                if (itemModel.sDate > itemModel.eDate) throw new Exception("Ngày bắt đầu không thể lớn hơn ngày kết thúc");
                if (user.RoleId != (int)RoleEnum.admin) itemModel.BranchIds = user.BranchIds;
                var model = new tbl_DayOff(itemModel);
                model.BranchIds = itemModel.BranchIds;
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_DayOff.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_DayOff> Update(DayOffUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_DayOff.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if(itemModel.sDate.HasValue)
                    itemModel.sDate = itemModel.sDate.Value.Date;
                if (itemModel.eDate.HasValue)
                    itemModel.eDate = itemModel.eDate.Value.Date;
                if (itemModel.sDate > itemModel.eDate) throw new Exception("Ngày bắt đầu không thể lớn hơn ngày kết thúc");
                entity.Name = itemModel.Name ?? entity.Name;
                entity.eDate = itemModel.eDate ?? entity.eDate;
                entity.sDate = itemModel.sDate ?? entity.sDate;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                if(user.RoleId != (int)RoleEnum.admin)
                {
                    entity.BranchIds = user.BranchIds;
                }
                else
                {
                    entity.BranchIds = itemModel.BranchIds;
                }
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_DayOff.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(DayOffSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new DayOffSearch();
                string myBranchIds = "";
                if (user.RoleId != ((int)RoleEnum.admin))
                    myBranchIds = user.BranchIds;
                string sql = $"Get_DayOff @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@BranchIds = N'{baseSearch.BranchIds ?? ""}'," +
                    $"@MyBranchIds = N'{myBranchIds ?? ""}'";

                var data = await db.SqlQuery<Get_DayOff>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_DayOff(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
     
    }
}