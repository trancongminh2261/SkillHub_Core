using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Services
{
    public class JobPositionService
    {
        public static async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SearchOptions();
                string sql = $"Get_JobPosition " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Search = N'{baseSearch.Search}'";
                var data = await db.SqlQuery<Get_JobPosition>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_JobPosition(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        public static async Task<tbl_JobPosition> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await db.tbl_JobPosition.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task<tbl_JobPosition> Insert(JobPositionCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {

                try
                {
                    var model = new tbl_JobPosition(itemModel);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    db.tbl_JobPosition.Add(model);
                    await db.SaveChangesAsync();

                    return model;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        public static async Task<tbl_JobPosition> Update(JobPositionUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_JobPosition.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Name = itemModel.Name ?? entity.Name;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await db.SaveChangesAsync();
                    return entity;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_JobPosition.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Enable = false;
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}