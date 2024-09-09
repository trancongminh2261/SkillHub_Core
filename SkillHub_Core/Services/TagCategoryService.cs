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
    public class TagCategoryService
    {
        public static async Task<tbl_TagCategory> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TagCategory.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_TagCategory> Insert(TagCategoryCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_TagCategory(itemModel);
                var checkExist = await db.tbl_TagCategory.AnyAsync(x => x.Name.ToUpper() == itemModel.Name.ToUpper() && x.Enable == true && x.Type == itemModel.Type);
                if (checkExist)
                    throw new Exception("Đã tồn tại danh mục bộ lọc này");
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_TagCategory.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_TagCategory> Update(TagCategoryUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TagCategory.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (data == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                data.Name = itemModel.Name ?? data.Name;
                var checkExist = await db.tbl_TagCategory.AnyAsync(x => x.Name.ToUpper() == data.Name.ToUpper() && x.Enable == true && x.Id != itemModel.Id && x.Type == data.Type);
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
                var entity = await db.tbl_TagCategory.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(TagCategorySearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TagCategorySearch();
                var l = await db.tbl_TagCategory.Where(x => x.Enable == true
                && x.Type == (baseSearch.Type == 0 ? x.Type : baseSearch.Type)).OrderByDescending(x => x.Id).ToListAsync();
                int totalRow = l.Count();
                if(totalRow == 0)
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
    }
}