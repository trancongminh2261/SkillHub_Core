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
    public class ProductService
    {
        public static async Task<tbl_Product> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                {
                    data.AvatarUserCreate = db.tbl_UserInformation.SingleOrDefault(x => x.UserInformationId == data.UserCreate)?.Avatar;
                    data.TotalPackageSection = await db.tbl_PackageSection.CountAsync(x => x.PackageId == data.Id && x.Enable == true);
                }
                return data;
            }
        }
        public static async Task<tbl_Product> Insert(ProductCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    if (itemModel.Price < 0)
                        throw new Exception("Giá bán không phù hợp");
                    var model = new tbl_Product(itemModel);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    model.TotalRate = 0;
                    model.TotalStudent = 0;
                    model.Active = model.Type == 1 ? false : true;
                    model.UserCreate = user.UserInformationId;
                    model.AvatarUserCreate = user.Avatar;
                    db.tbl_Product.Add(model);
                    await db.SaveChangesAsync();
                    return model;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static async Task<tbl_Product> Update(ProductUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Name = itemModel.Name ?? entity.Name;
                    entity.Thumbnail = itemModel.Thumbnail ?? entity.Thumbnail;
                    entity.Price = itemModel.Price ?? entity.Price;
                    entity.Tags = itemModel.Tags ?? entity.Tags;
                    entity.Description = itemModel.Description ?? entity.Description;
                    entity.Active = itemModel.Active ?? entity.Active;
                    entity.BeforeCourseId = itemModel.BeforeCourseId ?? entity.BeforeCourseId;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    entity.AvatarUserCreate = db.tbl_UserInformation.FirstOrDefault(x => x.UserInformationId == entity.UserCreate)?.Avatar;
                    entity.TotalPackageSection = await GetTotalPackageSection(entity.Id);
                    await db.SaveChangesAsync();
                    return entity;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Enable = false;
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static async Task<AppDomainResult> GetAll(ProductSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ProductSearch();
                string sql = $"Get_Product @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Name = N'{baseSearch.Name ?? ""}'," +
                    $"@Tags = N'{baseSearch.Tags ?? ""}'," +
                    $"@Type = N'{baseSearch.Type}'," +
                    $"@RoleId = {user.RoleId ?? 0}," +
                    $"@UserId = {user.UserInformationId}," +
                    $"@Sort = {baseSearch.Sort}," +
                    $"@SortType = {baseSearch.SortType}";
                var data = await db.SqlQuery<Get_Product>(sql);
                var myCourses = await db.tbl_VideoCourseStudent
                    .Where(x => x.UserId == user.UserInformationId && x.Enable == true).Select(x => x.VideoCourseId).ToListAsync();
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                if (user.RoleId != ((int)RoleEnum.student))
                {
                    var result = (from i in data
                                  select new tbl_Product
                                  {
                                      Active = i.Active,
                                      BeforeCourseId = i.BeforeCourseId,
                                      BeforeCourseName = i.BeforeCourseName,
                                      CreatedBy = i.CreatedBy,
                                      CreatedOn = i.CreatedOn,
                                      Description = i.Description,
                                      Enable = i.Enable,
                                      Price = i.Price,
                                      MarkQuantity = i.MarkQuantity,
                                      Type = i.Type,
                                      Id = i.Id,
                                      ModifiedBy = i.ModifiedBy,
                                      ModifiedOn = i.ModifiedOn,
                                      Name = i.Name,
                                      Tags = i.Tags,
                                      Thumbnail = i.Thumbnail,
                                      TotalRate = i.TotalRate,
                                      TotalStudent = i.TotalStudent,
                                      Disable = false,
                                      AvatarUserCreate = i.AvatarUserCreate,
                                      TotalPackageSection = i.TotalPackageSection,
                                      UserCreate = i.UserCreate
                                  }).ToList();

                    return new AppDomainResult { TotalRow = totalRow, Data = result };
                }
                else
                {
                    var result = (from i in data
                                  join v in myCourses on i.BeforeCourseId equals v into pg
                                  from v in pg.DefaultIfEmpty()
                                  select new ProductByStudent
                                  {
                                      BeforeCourseId = i.BeforeCourseId,
                                      BeforeCourseName = i.BeforeCourseName,
                                      CreatedBy = i.CreatedBy,
                                      CreatedOn = i.CreatedOn,
                                      Description = i.Description,
                                      Enable = i.Enable,
                                      Price = i.Price,
                                      MarkQuantity = i.MarkQuantity,
                                      Type = i.Type,
                                      Id = i.Id,
                                      ModifiedBy = i.ModifiedBy,
                                      ModifiedOn = i.ModifiedOn,
                                      Name = i.Name,
                                      Tags = i.Tags,
                                      Thumbnail = i.Thumbnail,
                                      TotalRate = i.TotalRate,
                                      TotalStudent = i.TotalStudent,
                                      AvatarUserCreate = i.AvatarUserCreate,
                                      TotalPackageSection = i.TotalPackageSection,
                                      UserCreate = i.UserCreate,
                                      Status = Task.Run(() => GetStatus(i.Id, user)).Result,
                                      Disable = user.RoleId != ((int)RoleEnum.student) ? false
                                      : i.BeforeCourseId == 0 ? false
                                      : v.HasValue ? false : true
                                  }).ToList();
                    return new AppDomainResult { TotalRow = totalRow, Data = result };
                }
            }
        }
        public static async Task<int> GetStatus(int productId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var result = 1;

                var videoCourseStudent = await db.tbl_VideoCourseStudent
                    .Where(x => x.UserId == user.UserInformationId && x.VideoCourseId == productId && x.Enable == true).FirstOrDefaultAsync();
                if(videoCourseStudent != null)
                    result = 2;

                var packageStudent = await db.tbl_PackageStudent
                    .Where(x => x.StudentId == user.UserInformationId && x.PackageId == productId && x.Enable == true).FirstOrDefaultAsync();
                if(packageStudent != null)
                    result = 2;

                return result;
            }
        }
        public static async Task<int> GetTotalPackageSection(int productId)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_TotalPackageSection @ProductId = {productId}";
                var data = await db.SqlQuery<tbl_Product>(sql);
                return data.FirstOrDefault().TotalPackageSection;
            }    
        }
    }
}