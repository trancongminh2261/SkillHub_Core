using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class ContractService
    {
        public static async Task<tbl_Contract> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Contract.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<string> GetTemplate(int studentId)
        {
            using (var db = new lmsDbContext())
            {
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == studentId);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                string result = "";
                var template = await db.tbl_Template.FirstOrDefaultAsync(x => x.Type == 1 && x.Enable == true);
                if (template != null)
                    result = template.Content;
                string bod = string.Empty;
                if(student.DOB.HasValue)
                    bod = student.DOB.Value.ToString("dd/MM/yyyy");
                result = result.Replace("{HoVaTen}", student.FullName);
                result = result.Replace("{MaHocVien}", student.UserCode);
                result = result.Replace("{SoDienThoai}", student.Mobile);
                result = result.Replace("{EmailHocVien}", student.Email);
                result = result.Replace("{NgaySinh}", bod);
                result = result.Replace("{Ngay}", DateTime.Now.Day.ToString());
                result = result.Replace("{Thang}", DateTime.Now.Month.ToString());
                result = result.Replace("{Nam}", DateTime.Now.Year.ToString());

                return result;
            }
        }
        public static async Task<tbl_Contract> Insert(ContractCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var model = new tbl_Contract(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_Contract.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_Contract> Update(ContractUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Contract.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Name = itemModel.Name ?? entity.Name;
                entity.Content = itemModel.Content ?? entity.Content;
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
                var entity = await db.tbl_Contract.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(ContractSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ContractSearch();
                if (user.RoleId == ((int)RoleEnum.student))
                    baseSearch.StudentId = user.UserInformationId;
                string sql = $"Get_Contract @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@StudentId = N'{baseSearch.StudentId}'";
                var data = await db.SqlQuery<Get_Contract>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Contract(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}