using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class CertificateConfigService : DomainService
    {
        public CertificateConfigService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_CertificateConfig> GetById(int id)
        {
            return await dbContext.tbl_CertificateConfig.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task Validate(tbl_CertificateConfig model)
        {
            var class_ = await dbContext.tbl_Class.FirstOrDefaultAsync(x => x.Id == model.ClassId
            && x.Enable == true)
                ?? throw new Exception("Không tìm thấy lớp học!");
            var checkCertificate = await dbContext.tbl_CertificateConfig.AnyAsync(x=>x.ClassId == model.ClassId
            && x.Enable == true);
            if (checkCertificate)
                throw new Exception("Lớp này đã tạo chứng chỉ không thể tạo mới!");
        }
        public async Task<tbl_CertificateConfig> Insert(CertificateConfigCreate itemModel, tbl_UserInformation user)
        {
            var entity = new tbl_CertificateConfig(itemModel);
            await Validate(entity);
            entity.CreatedBy = entity.ModifiedBy = user.FullName;
            dbContext.tbl_CertificateConfig.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<tbl_CertificateConfig> Update(CertificateConfigUpdate itemModel, tbl_UserInformation user)
        {
            var entity = await dbContext.tbl_CertificateConfig.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
            if (entity == null)
                throw new Exception("Chứng chỉ không tồn tại");
            entity.SubTitle = itemModel.SubTitle ?? entity.SubTitle;
            entity.CertificateName = itemModel.CertificateName ?? entity.CertificateName;
            entity.CertificateCourse = itemModel.CertificateCourse ?? entity.CertificateCourse;
            entity.Type = itemModel.Type ?? entity.Type;
            entity.Description = itemModel.Description ?? entity.Description;
            entity.ModifiedBy = user.FullName;
            entity.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task Delete(int id)
        {
            var entity = await dbContext.tbl_CertificateConfig.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (entity == null)
                throw new Exception("Chứng chỉ không tồn tại");
            entity.Enable = false;
            await dbContext.SaveChangesAsync();
        }
        public async Task<tbl_CertificateConfig> GetByClassId(int classId)
        {
            return await dbContext.tbl_CertificateConfig.SingleOrDefaultAsync(x => x.ClassId == classId && x.Enable == true);
        }
    }
}