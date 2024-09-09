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
using Microsoft.AspNetCore.Mvc;

namespace LMSCore.Services
{
    public class TagService
    {
        public static async Task<tbl_Tag> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Tag.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_Tag> Insert(TagCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_Tag(itemModel);
                var category = await db.tbl_TagCategory.SingleOrDefaultAsync(x => x.Id == itemModel.TagCategoryId.Value);
                if (category == null)
                    throw new Exception("Không tìm thấy danh mục");
                var checkExist = await db.tbl_Tag.AnyAsync(x => x.Name.ToUpper() == itemModel.Name.ToUpper() && x.Enable == true && x.TagCategoryId == category.Id);
                if (checkExist)
                    throw new Exception("Đã tồn tại danh mục bộ lọc này");
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_Tag.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_Tag> Update(TagUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Tag.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (data == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                data.Name = itemModel.Name ?? data.Name;
                var checkExist = await db.tbl_Tag.AnyAsync(x => x.Name.ToUpper() == data.Name.ToUpper() && x.Enable == true && x.Id != itemModel.Id && x.TagCategoryId == data.TagCategoryId);
                if (checkExist)
                    throw new Exception("Đã tồn tại danh mục bộ lọc này");
                await db.SaveChangesAsync();
                return data;
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Tag.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        //public static async Task<AppDomainResult> GetAll(TagSearch baseSearch)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        if (baseSearch == null) baseSearch = new TagSearch();
        //        var l = await db.tbl_Tag.Where(x => x.Enable == true
        //        && x.TagCategoryId == (baseSearch.TagCategoryId == 0 ? x.TagCategoryId : baseSearch.TagCategoryId)).OrderBy(x => x.Name).ToListAsync();
        //        int totalRow = l.Count();
        //        var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
        //        return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
        //    }
        //}

        public static async Task<AppDomainResult> GetAll(TagSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TagSearch();

                string sql = $"Get_Tag @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Type = {baseSearch.Type ?? 0}," +
                    $"@TagCategoryId = N'{baseSearch.TagCategoryId}'";
                var data = await db.SqlQuery<Get_Tag>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Tag(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}