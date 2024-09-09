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
    public class PaymentAllowService
    {
        public static async Task<tbl_PaymentAllow> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_PaymentAllow.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task AddItems(PaymentAllowCreates itemModel,tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (userLog.RoleId != 1)
                            throw new Exception("Không được phép thực hiện");
                        if (!itemModel.UserIds.Any())
                            throw new Exception("Không tìm thấy nhân viên");
                        foreach (var item in itemModel.UserIds)
                        {
                            var user = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.UserInformationId == item && x.Enable == true);
                            if (user == null)
                                throw new Exception("Không tìm thấy nhân viên");
                            if (user.RoleId == ((int)RoleEnum.student) || user.RoleId == ((int)RoleEnum.parents))
                                throw new Exception("Không thể cấp quyền cho người dùng này");
                            var checkExist = await db.tbl_PaymentAllow.AnyAsync(x => x.UserId == item && x.Enable == true);
                            if (checkExist)
                                continue;

                            var model = new tbl_PaymentAllow
                            {
                                CreatedBy = userLog.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                UserId = item
                            };
                            db.tbl_PaymentAllow.Add(model);
                            await db.SaveChangesAsync();
                        }
                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_PaymentAllow.SingleOrDefaultAsync(x => x.Id == id);
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
                var l = await db.tbl_PaymentAllow.Where(x => x.Enable == true).OrderByDescending(x => x.Id).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };

                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                result = (from i in result
                          join u in db.tbl_UserInformation.ToList() on i.UserId equals u.UserInformationId
                          select new tbl_PaymentAllow
                          {
                              CreatedBy = i.CreatedBy,
                              CreatedOn = i.CreatedOn,
                              Enable = i.Enable,
                              FullName = u.FullName,
                              Id = i.Id,
                              ModifiedBy = i.ModifiedBy,
                              ModifiedOn = i.ModifiedOn,
                              UserCode = u.UserCode,
                              UserId = i.UserId,
                          }).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
        public class Get_UserAvailable_PaymentAllow
        {
            public int UserInformationId { get; set; }
			public string FullName { get; set; }
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public string UserCode { get; set; }
        }
        public static async Task<List<Get_UserAvailable_PaymentAllow>> GetUserAvailable()
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_UserAvailable_PaymentAllow";
                var data = await db.SqlQuery<Get_UserAvailable_PaymentAllow>(sql);
                return data;
            }
        }
    }
}