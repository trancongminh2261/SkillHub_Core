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
    public class PackageSectionService
    {
        public static async Task<tbl_PackageSection> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_PackageSection.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_PackageSection> Insert(PackageSectionCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var package = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == itemModel.PackageId);
                if (package == null)
                    throw new Exception("Không tìm thấy bộ đề");
                if(package.Type != 2)
                    throw new Exception("Không tìm thấy bộ đề");
                var model = new tbl_PackageSection(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_PackageSection.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_PackageSection> Update(PackageSectionUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_PackageSection.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Name = itemModel.Name ?? entity.Name;
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
                var entity = await db.tbl_PackageSection.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(PackageSectionSearch baseSearch, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new PackageSectionSearch();
                if (userLog.RoleId == ((int)RoleEnum.student))
                {
                    var hasPackage = await db.tbl_PackageStudent.AnyAsync(x => x.PackageId == baseSearch.PackageId && x.StudentId == userLog.UserInformationId && x.Enable == true);
                    if (!hasPackage)
                    {
                        var package = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == baseSearch.PackageId);
                        if (package == null)
                            return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                        if(package.Price == 0)
                        {
                            var packageStudent = new tbl_PackageStudent
                            {
                                CreatedBy = "Tham gia miễn phí",
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = "Tham gia miễn phí",
                                ModifiedOn = DateTime.Now,
                                StudentId = userLog.UserInformationId,
                                PackageId = package.Id,
                            };
                            db.tbl_PackageStudent.Add(packageStudent);
                            await db.SaveChangesAsync();
                        }
                        else
                            return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                    }
                }
                var l = await db.tbl_PackageSection.Where(x => x.Enable == true
                && x.PackageId == (baseSearch.PackageId == 0 ? x.PackageId : baseSearch.PackageId)).OrderBy(x => x.Name).ToListAsync();

                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
    }
}