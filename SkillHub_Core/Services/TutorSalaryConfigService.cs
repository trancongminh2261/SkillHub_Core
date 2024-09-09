﻿using LMSCore.Areas.Models;
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
    public class TutorSalaryConfigService
    {
        public static async Task<tbl_TutorSalaryConfig> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_TutorSalaryConfig.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_TutorSalaryConfig> Insert(TutorSalaryConfigCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_TutorSalaryConfig(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_TutorSalaryConfig.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_TutorSalaryConfig> Update(TutorSalaryConfigUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TutorSalaryConfig.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Code = itemModel.Code ?? entity.Code;
                entity.Name = itemModel.Name ?? entity.Name;
                entity.Salary = itemModel.Salary?? entity.Salary;
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
                var entity = await db.tbl_TutorSalaryConfig.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new SearchOptions();
                var l = await db.tbl_TutorSalaryConfig.Where(x => x.Enable == true).OrderBy(x => x.Code).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
    }
}