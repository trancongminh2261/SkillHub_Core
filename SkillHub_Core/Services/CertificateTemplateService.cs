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
    public class CertificateTemplateService : DomainService
    {
        public CertificateTemplateService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_CertificateTemplate> GetById(int id)
        {
            return await dbContext.tbl_CertificateTemplate.SingleOrDefaultAsync(x => x.Id == id);
        }
        /// <summary>
        /// Lấy mã hướng dẫn
        /// </summary>
        /// <returns></returns>
        public async Task<List<CertificateTemplateGuide>> GetGuide()
        {
            return await Task.Run(() =>
            {
                return tbl_CertificateTemplate.GetGuide();
            });
        }
        /// <summary>
        /// Tải dữ liệu học viên vào chứng chỉ mẫu
        /// </summary>
        /// <param name="input"></param>
        /// <param name="studentId"></param>
        /// <param name="classId"></param>
        /// <returns></returns>
        public async Task<string> ReplaceContent(string input,int studentId, int classId)
        {
            var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentId);
            var _Class = await dbContext.tbl_Class.SingleOrDefaultAsync(x => x.Id == classId);
            int programId = _Class?.ProgramId ?? 0;
            var program = await dbContext.tbl_Program.SingleOrDefaultAsync(x => x.Id == programId);
            int gradeId = program?.GradeId ?? 0;
            var grade = await dbContext.tbl_Grade.SingleOrDefaultAsync(x => x.Id == gradeId);
            string output = input;
            output = output.Replace("{MaHocVien}", student?.UserCode);
            output = output.Replace("{TenHocVien}", student?.FullName);
            output = output.Replace("{NgayCap}", DateTime.Now.Day.ToString());
            output = output.Replace("{ThangCap}", DateTime.Now.Month.ToString());
            output = output.Replace("{NamCap}", DateTime.Now.Year.ToString());
            output = output.Replace("{Lop}", _Class?.Name);
            output = output.Replace("{ChuongTrinh}", program?.Name);
            output = output.Replace("{ChuyenMon}", grade?.Name);
            return output;
        }
        public async Task<tbl_CertificateTemplate> Insert(CertificateTemplateCreate itemModel, tbl_UserInformation user)
        {
            var entity = new tbl_CertificateTemplate(itemModel);
            entity.CreatedBy = entity.ModifiedBy = user.FullName;
            dbContext.tbl_CertificateTemplate.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<tbl_CertificateTemplate> Update(CertificateTemplateUpdate itemModel, tbl_UserInformation user)
        {
            var entity = await dbContext.tbl_CertificateTemplate.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
            if (entity == null)
                throw new Exception("Chứng chỉ không tồn tại");
            entity.Name = itemModel.Name ?? entity.Name;
            entity.Content = itemModel.Content ?? entity.Content;
            entity.Background = itemModel.Background ?? entity.Background;
            entity.Backside = itemModel.Backside ?? entity.Backside;
            entity.Width = itemModel.Width ?? entity.Width;
            entity.Height = itemModel.Height ?? entity.Height;
            entity.ModifiedBy = user.FullName;
            entity.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task Delete(int id)
        {
            var entity = await dbContext.tbl_CertificateTemplate.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (entity == null)
                throw new Exception("Chứng chỉ không tồn tại");
            entity.Enable = false;
            await dbContext.SaveChangesAsync();
        }
        public async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            if (baseSearch == null) baseSearch = new SearchOptions();
            var pg = await dbContext.tbl_CertificateTemplate.Where(x => x.Enable == true
            && (string.IsNullOrEmpty(baseSearch.Search) || x.Name.Contains(baseSearch.Search)))
                .OrderByDescending(x => x.CreatedOn).Select(x => x.Id).ToListAsync();

            if (!pg.Any())
                return new AppDomainResult() { TotalRow = 0, Data = null };
            int totalRow = pg.Count();
            pg = pg.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
            var data = (from i in pg
                        select Task.Run(() => GetById(i)).Result).ToList();
            return new AppDomainResult() { TotalRow = totalRow, Data = data };
        }
    }
}