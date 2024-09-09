using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace LMSCore.Services.ScheduleRecord
{
    public class ScheduleRecordService : DomainService
    {
        public ScheduleRecordService(lmsDbContext dbContext) : base(dbContext) { }
        public async Task<tbl_ScheduleRecord> GetById(int id)
        {
            return await dbContext.tbl_ScheduleRecord.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        public async Task<List<tbl_ScheduleRecord>> GetAll(ScheduleRecordSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new ScheduleRecordSearch();
            var result = await dbContext.tbl_ScheduleRecord.Where(x => x.Enable == true && x.ScheduleId == baseSearch.ScheduleId).ToListAsync();
            return result;
        }

        public async Task<tbl_ScheduleRecord> Insert(ScheduleRecordCreate itemModel, tbl_UserInformation userLogin)
        {
            try
            {
                var model = new tbl_ScheduleRecord(itemModel);
                model.CreatedBy = userLogin.FullName;
                dbContext.tbl_ScheduleRecord.Add(model);           
                await dbContext.SaveChangesAsync();
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<tbl_ScheduleRecord> Update(ScheduleRecordUpdate itemModel, tbl_UserInformation user)
        {
            try
            {
                var entity = await dbContext.tbl_ScheduleRecord.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy bản ghi");
                entity.Name = itemModel.Name ?? entity.Name;
                entity.UrlLink = itemModel.UrlLink ?? entity.UrlLink;
                entity.Type = itemModel.Type ?? entity.Type;
                entity.TypeName = tbl_ScheduleRecord.GetTypeName(entity.Type);
                entity.ModifiedBy = user.FullName;
                await dbContext.SaveChangesAsync();
                return entity;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var entity = await dbContext.tbl_ScheduleRecord.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy bản ghi");
                entity.Enable = false;
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
