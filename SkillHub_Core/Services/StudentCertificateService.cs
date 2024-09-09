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
using RestSharp.Validation;

namespace LMSCore.Services
{
    public class StudentCertificateService : DomainService
    {
        public StudentCertificateService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_StudentCertificate> GetById(int id)
        {
            return await dbContext.tbl_StudentCertificate.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task Validate(tbl_StudentCertificate model)
        {
            var existed = await dbContext.tbl_StudentCertificate.AnyAsync(x => x.ClassId == model.ClassId
            && x.StudentId == model.StudentId
            && x.Enable == true);
            if (existed)
                throw new Exception("Học viên đã được cấp chứng chỉ rồi");
            var class_ = await dbContext.tbl_Class.FirstOrDefaultAsync(x => x.Id == model.ClassId
            && x.Enable == true)
                ?? throw new Exception("Không tìm thấy lớp học");
            var student = await dbContext.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == model.StudentId
            && x.Enable == true && x.RoleId == (int)lmsEnum.RoleEnum.student)
            ?? throw new Exception("Không tìm thấy học viên");
            var certificateConfig = await dbContext.tbl_CertificateConfig.FirstOrDefaultAsync(x => x.ClassId == model.ClassId
            && x.Enable == true)
            ?? throw new Exception("Không tìm thấy chứng chỉ của lớp");
            var studentInClass = await dbContext.tbl_StudentInClass.FirstOrDefaultAsync(x => x.StudentId == model.StudentId
            && x.Enable == true && x.ClassId == model.ClassId)
            ?? throw new Exception("Học viên không thuộc lớp này");
        }
        public async Task<tbl_StudentCertificate> Insert(StudentCertificateCreate itemModel, tbl_UserInformation user)
        {
            var entity = new tbl_StudentCertificate(itemModel);
            await Validate(entity);
            var certificateConfig = await dbContext.tbl_CertificateConfig.SingleOrDefaultAsync(x => x.ClassId == entity.ClassId && x.Enable == true);
            entity.CertificateName = certificateConfig.CertificateName;
            entity.CertificateCourse = certificateConfig.CertificateCourse;
            entity.SubTitle = certificateConfig.SubTitle;
            entity.Description = certificateConfig.Description;
            entity.Type = certificateConfig.Type;
            entity.CreatedBy = entity.ModifiedBy = user.FullName;
            dbContext.tbl_StudentCertificate.Add(entity);
            await dbContext.SaveChangesAsync();
            var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.StudentId
            && x.Enable == true && x.RoleId == (int)lmsEnum.RoleEnum.student);
            entity.StudentName = student.FullName;
            return entity;
        }
        public async Task Delete(int id)
        {
            var entity = await dbContext.tbl_StudentCertificate.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (entity == null)
                throw new Exception("Chứng chỉ không tồn tại");
            entity.Enable = false;
            await dbContext.SaveChangesAsync();
        }
        public async Task<tbl_StudentCertificate> GetStudentCertificate(StudentCertificateSearch baseSearch)
        {
            var data = await dbContext.tbl_StudentCertificate.FirstOrDefaultAsync(x => x.ClassId == baseSearch.ClassId
            && x.StudentId == baseSearch.StudentId
            && x.Enable == true);
            if (data != null)
            {
                var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.StudentId
                && x.Enable == true && x.RoleId == (int)lmsEnum.RoleEnum.student);
                data.StudentName = student?.FullName;
            }
            return data;
        }
    }
}