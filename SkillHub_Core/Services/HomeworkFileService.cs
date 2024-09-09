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
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace LMSCore.Services
{
    public class HomeworkFileService
    {
        public static async Task<tbl_HomeworkFile> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_HomeworkFile.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                    data.TypeName = HomeworkFileTypeName(data.Type);
                return data;
            }
        }
        public static async Task<tbl_HomeworkFile> Insert(HomeworkFileCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_HomeworkFile(itemModel);
                model.UserId = user.UserInformationId;
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_HomeworkFile.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_HomeworkFile> Update(HomeworkFileUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_HomeworkFile.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.File = itemModel.File ?? entity.File;
                entity.ModifiedOn = DateTime.Now;
                entity.ModifiedBy = user.FullName;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_HomeworkFile.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<List<tbl_HomeworkFile>> GetAll(HomeworkFileSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_HomeworkFile.Where(x => x.Enable == true
                && x.HomeworkId == baseSearch.HomeworkId
                && (baseSearch.UserId == null || x.UserId == baseSearch.UserId)
                && x.Type == baseSearch.Type
                ).OrderByDescending(x => x.CreatedOn).ToListAsync();
                data.ForEach(x => x.TypeName = HomeworkFileTypeName(x.Type));
                return data;
            }
        }
    }
}