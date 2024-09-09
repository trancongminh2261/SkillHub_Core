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
    public class TimeLineService
    {
        public static async Task<tbl_TimeLine> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TimeLine.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_TimeLine> Insert(TimeLineCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var _class = await db.tbl_Class.AnyAsync(x => x.Id == itemModel.ClassId.Value);
                if (!_class)
                    throw new Exception("Không tìm thấy lớp học");
                var model = new tbl_TimeLine(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_TimeLine.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TimeLine.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(TimeLineSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TimeLineSearch();
                var l = await db.tbl_TimeLine
                    .Where(x => x.Enable == true && x.ClassId == baseSearch.ClassId.Value)
                    .OrderByDescending(x => x.Id).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
    }
}