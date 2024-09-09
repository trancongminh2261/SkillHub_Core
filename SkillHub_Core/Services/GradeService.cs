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
    public class GradeService
    {
        public static async Task<tbl_Grade> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Grade.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_Grade> Insert(GradeCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var checkCode = await db.tbl_Grade.AnyAsync(x => x.Code.ToUpper() == itemModel.Code.ToUpper() && x.Enable == true);
                if (checkCode)
                    throw new Exception("Mã đã tồn tại");
                var model = new tbl_Grade(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_Grade.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_Grade> Update(GradeUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Grade.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (itemModel.Code != null)
                {
                    var checkCode = await db.tbl_Grade.AnyAsync(x => x.Code.ToUpper() == itemModel.Code.ToUpper() && x.Enable == true && x.Id != entity.Id);
                    if (checkCode)
                        throw new Exception("Mã đã tồn tại");
                }
                entity.Code = itemModel.Code ?? entity.Code;
                entity.Name = itemModel.Name ?? entity.Name;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Grade.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                // xóa chương trình
                var program = await db.tbl_Program.Where(x => x.GradeId == id).ToListAsync();
                if (program.Any())
                    foreach (var item in program)
                        await ProgramService.Delete(item.Id);
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(GradeSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new GradeSearch();

                var l = await db.tbl_Grade.Where(x => x.Enable == true
                && (x.Name.Contains(baseSearch.Name) || string.IsNullOrEmpty(baseSearch.Name))
                && (x.Code.Contains(baseSearch.Code) || string.IsNullOrEmpty(baseSearch.Code))
                ).OrderBy(x => x.Name).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };

                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
        public static async Task<List<tbl_Grade>> GetDropdown()
        {
            using (var db = new lmsDbContext())
            {
                return await db.tbl_Grade.Where(x => x.Enable == true).ToListAsync();
            }
        }
    }
}