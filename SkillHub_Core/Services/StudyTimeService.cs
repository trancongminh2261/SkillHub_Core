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
    public class StudyTimeService
    {
        /// <summary>
        /// Kiểm tra kiểu giờ HH:mm
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool ValidateStudyTime(string time)
        {
            var times = time.Split(':');
            if (times.Length != 2)
                return false;
            int i;
            if (!int.TryParse(times[0], out i))
                return false;
            if (i > 24)
                return false;
            if (!int.TryParse(times[1], out i))
                return false;
            if (i >= 60)
                return false;
            return true;
        }
        public static double GetTime(string sTime, string eTime)
        {

            if (!ValidateStudyTime(sTime) || !ValidateStudyTime(eTime))
                return 0;
            var stimes = sTime.Split(':');
            DateTime st = new DateTime(1970, 1, 1, int.Parse(stimes[0]), int.Parse(stimes[1]), 0);
            var etimes = eTime.Split(':');
            DateTime et = new DateTime(1970, 1, 1, int.Parse(etimes[0]), int.Parse(etimes[1]), 0);
            double result = et.Subtract(st).TotalMinutes;
            return result;
        }
        public static async Task<tbl_StudyTime> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_StudyTime.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_StudyTime> Insert(StudyTimeCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (!ValidateStudyTime(itemModel.StartTime) || !ValidateStudyTime(itemModel.EndTime))
                    throw new Exception("Thời gian không đúng định dạnh");
                double time = GetTime(itemModel.StartTime, itemModel.EndTime);
                if (time <= 0)
                    throw new Exception("Thời gian không phù hợp");
                var model = new tbl_StudyTime(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                model.Time = time;
                model.Name = $"{itemModel.StartTime} - {itemModel.EndTime}";
                db.tbl_StudyTime.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_StudyTime> Update(StudyTimeUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudyTime.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if(itemModel.StartTime != null)
                    if(!ValidateStudyTime(itemModel.StartTime))
                        throw new Exception("Thời gian không đúng định dạnh");
                if (itemModel.EndTime != null)
                    if (!ValidateStudyTime(itemModel.EndTime))
                        throw new Exception("Thời gian không đúng định dạnh");
                entity.StartTime = itemModel.StartTime ?? entity.StartTime;
                entity.EndTime = itemModel.EndTime ?? entity.EndTime;
                double time = GetTime(entity.StartTime, entity.EndTime);
                if (time <= 0)
                    throw new Exception("Thời gian không phù hợp");
                entity.Time = time;
                entity.Name = $"{entity.StartTime} - {entity.EndTime}";
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudyTime.SingleOrDefaultAsync(x => x.Id == id);
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
                if (baseSearch == null) baseSearch = new BranchSearch();

                var l = await db.tbl_StudyTime.Where(x => x.Enable == true).OrderBy(x => x.Name).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };

                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
    }
}