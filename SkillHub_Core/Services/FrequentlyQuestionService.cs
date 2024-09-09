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
    public class FrequentlyQuestionService
    {
        public static async Task<tbl_FrequentlyQuestion> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_FrequentlyQuestion.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_FrequentlyQuestion> Insert(FrequentlyQuestionCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                itemModel.RoleIds = ClearRoleId(itemModel.RoleIds);
                var model = new tbl_FrequentlyQuestion(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_FrequentlyQuestion.Add(model);
                await db.SaveChangesAsync();
                model.RoleNames = GetRoleNames(model.RoleIds);
                return model;
            }
        }

        public static async Task<tbl_FrequentlyQuestion> Update(FrequentlyQuestionUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (!string.IsNullOrEmpty(itemModel.RoleIds))
                    itemModel.RoleIds = ClearRoleId(itemModel.RoleIds);
                var entity = await db.tbl_FrequentlyQuestion.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Question = itemModel.Question ?? entity.Question;
                entity.Answer = itemModel.Answer ?? entity.Answer;
                entity.RoleIds = itemModel.RoleIds ?? entity.RoleIds;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                entity.RoleNames = GetRoleNames(entity.RoleIds);
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_FrequentlyQuestion.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(FrequentlyQuestionSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new FrequentlyQuestionSearch();
                var l = await db.tbl_FrequentlyQuestion.Where(x => (baseSearch.RoleId == null || ("," + x.RoleIds + ",").Contains("," + baseSearch.RoleId + ","))
                && x.Enable == true).OrderByDescending(x => x.Id).ToListAsync();

                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };

                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                result.ForEach(x => x.RoleNames = GetRoleNames(x.RoleIds));
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
        public static string ClearRoleId(string roleIds)
        {
            if (string.IsNullOrEmpty(roleIds))
                return string.Empty;
            return String.Join(",", roleIds.Split(',').GroupBy(x => x).Select(x => x.Key));
        }
        public static string GetRoleNames(string roleIds)
        {
            if (string.IsNullOrEmpty(roleIds))
                return string.Empty;
            roleIds = "," + roleIds + ",";
            var roles = GetRoleName().Where(x => roleIds.Contains("," + x.Key + ",")).Select(x => x.Value).ToList();
            return String.Join(", ", roles);
        }
    }
}