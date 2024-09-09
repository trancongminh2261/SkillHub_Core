using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class CustomerAudioService : DomainService
    {
        public CustomerAudioService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_CustomerAudio> GetById(int id)
        {
            return await dbContext.tbl_CustomerAudio.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<tbl_CustomerAudio> Insert(CustomerAudioCreate itemModel, tbl_UserInformation user)
        {
            var entity = new tbl_CustomerAudio(itemModel);
            entity.UserCreateId = user.UserInformationId;
            entity.CreatedBy = entity.ModifiedBy = user.FullName;
            dbContext.tbl_CustomerAudio.Add(entity);
            await dbContext.SaveChangesAsync();
            entity.UserCreateName = user.FullName;
            return entity;
        }
        public async Task Delete(int id)
        {
            var entity = await dbContext.tbl_CustomerAudio.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (entity == null)
                throw new Exception("Audio không tồn tại");
            entity.Enable = false;
            await dbContext.SaveChangesAsync();
        }
        public async Task<AppDomainResult> GetAll(CustomerAudioSearch baseSearch)
        {
            var data = await dbContext.tbl_CustomerAudio.Where(x => x.Enable == true
            && x.CustomerId == baseSearch.CustomerId)
                .OrderByDescending(x => x.CreatedOn).ToListAsync();
            data.ForEach(x => x.UserCreateName = GetUserName(x.UserCreateId.Value));
            return new AppDomainResult() { TotalRow = data.Count, Data = data };
        }
        public string GetUserName(int userId)
        {
            return dbContext.tbl_UserInformation.SingleOrDefault(x => x.Enable == true
            && x.UserInformationId == userId)?.FullName;
        }
    }
}