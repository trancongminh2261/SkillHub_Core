using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static System.Net.WebRequestMethods;

namespace LMSCore.Services
{
    public class CurriculumVitaeService
    {
        public static async Task<AppDomainResult> GetAll(CurriculumVitaeSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CurriculumVitaeSearch();
                string sql = $"Get_CurriculumVitae " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Search = N'{baseSearch.Search}'," +
                    $"@BranchId = {baseSearch.BranchId ?? 0}," +
                    $"@JobPosition = {baseSearch.JobPosition ?? 0}," +
                    $"@SortType = {(baseSearch.SortType ? 1 : 0)}";
                var data = await db.SqlQuery<Get_CurriculumVitae>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_CurriculumVitae(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        public static async Task<tbl_CurriculumVitae> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await db.tbl_CurriculumVitae.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task ValidateCreate(lmsDbContext db, CurriculumVitaeCreate itemModel)
        {
            //Kiểm tra số điện thoại
            var checkPhone = await db.tbl_CurriculumVitae
                .Where(x => x.Mobile == itemModel.Mobile && x.Enable == true).AnyAsync();
            if (checkPhone)
                throw new Exception($"Số điện thoại {itemModel.Mobile} đã được đăng kí");

            //Kiểm tra mail
            var checkEmail = await db.tbl_CurriculumVitae
                .Where(x => x.Email.ToUpper() == itemModel.Email.ToUpper() && x.Enable == true).AnyAsync();
            if (checkEmail)
                throw new Exception($"Email {itemModel.Email} đã được đăng kí");

            //Kiểm tra trung tâm
            var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId && x.Enable == true);
            if (branch == null)
                throw new Exception("Không tìm thấy trung tâm");

            //Kiểm tra tỉnh thành phố
            if (itemModel.AreaId.HasValue)
            {
                var area = await db.tbl_Area.AnyAsync(x => x.Id == itemModel.AreaId && x.Enable == true);
                if (!area)
                    throw new Exception("Không tìm thấy tỉnh / thành phố");
            }

            //Kiểm tra quận huyện
            if (itemModel.DistrictId.HasValue)
            {
                var district = await db.tbl_District.AnyAsync(x => x.Id == itemModel.DistrictId && x.Enable == true && x.AreaId == itemModel.AreaId);
                if (!district)
                    throw new Exception("Quận / huyện này không thuộc tỉnh / thành phố bạn chọn");
            }

            //Kiểm tra phường xã
            if (itemModel.WardId.HasValue)
            {
                var ward = await db.tbl_Ward.AnyAsync(x => x.Id == itemModel.WardId && x.Enable == true && x.DistrictId == itemModel.DistrictId);
                if (!ward)
                    throw new Exception("Phường / xã này không thuộc quận / huyện mà bạn chọn");
            }

        }

        public static async Task<tbl_CurriculumVitae> Insert(CurriculumVitaeCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var model = new tbl_CurriculumVitae(itemModel);
                    //Validate input
                    await ValidateCreate(db, itemModel);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    db.tbl_CurriculumVitae.Add(model);
                    await db.SaveChangesAsync();
                    return model;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task ValidateUpdate(lmsDbContext db, CurriculumVitaeUpdate itemModel)
        {
            //check xem có tìm được dữ liệu để update không
            var entity = await db.tbl_CurriculumVitae.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value && x.Enable == true);
            if (entity == null)
                throw new Exception("Không tìm thấy dữ liệu");

            //Kiểm tra số điện thoại
            var checkPhone = await db.tbl_CurriculumVitae
                .Where(x => x.Mobile == itemModel.Mobile && x.Mobile != entity.Mobile && x.Enable == true).AnyAsync();
            if (checkPhone)
                throw new Exception($"Số điện thoại {itemModel.Mobile} đã được đăng kí");

            //Kiểm tra mail
            var checkEmail = await db.tbl_CurriculumVitae
                .Where(x => x.Email.ToUpper() == itemModel.Email.ToUpper() && x.Email.ToUpper() == entity.Email.ToUpper() && x.Enable == true).AnyAsync();
            if (checkEmail)
                throw new Exception($"Email {itemModel.Email} đã được đăng kí");

            //Kiểm tra trung tâm
            var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == itemModel.BranchId && x.Enable == true);
            if (branch == null && itemModel.BranchId != null)
                throw new Exception("Không tìm thấy trung tâm");

            //Kiểm tra tỉnh thành phố
            if (itemModel.AreaId.HasValue)
            {
                var area = await db.tbl_Area.AnyAsync(x => x.Id == itemModel.AreaId && x.Enable == true);
                if (!area)
                    throw new Exception("Không tìm thấy tỉnh / thành phố");
            }

            //Kiểm tra quận huyện
            if (itemModel.DistrictId.HasValue)
            {
                var district = await db.tbl_District.AnyAsync(x => x.Id == itemModel.DistrictId && x.Enable == true && x.AreaId == itemModel.AreaId);
                if (!district)
                    throw new Exception("Quận / huyện này không thuộc tỉnh / thành phố bạn chọn");
            }

            //Kiểm tra phường xã
            if (itemModel.WardId.HasValue)
            {
                var ward = await db.tbl_Ward.AnyAsync(x => x.Id == itemModel.WardId && x.Enable == true && x.DistrictId == itemModel.DistrictId);
                if (!ward)
                    throw new Exception("Phường / xã này không thuộc quận / huyện mà bạn chọn");
            }
        }

        public static async Task<tbl_CurriculumVitae> Update(CurriculumVitaeUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    //validate input
                    await ValidateUpdate(db, itemModel);
                    var entity = await db.tbl_CurriculumVitae.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.FullName = itemModel.FullName ?? entity.FullName;
                    entity.Mobile = itemModel.Mobile ?? entity.Mobile;
                    entity.Email = itemModel.Email ?? entity.Email;
                    entity.LinkCV = itemModel.LinkCV ?? entity.LinkCV;
                    entity.BranchId = itemModel.BranchId ?? entity.BranchId;
                    entity.JobPositionId = itemModel.JobPositionId ?? entity.JobPositionId;
                    entity.AreaId = itemModel.AreaId ?? entity.AreaId;
                    entity.DistrictId = itemModel.DistrictId ?? entity.DistrictId;
                    entity.WardId = itemModel.WardId ?? entity.WardId;
                    entity.Address = itemModel.Address ?? entity.Address;                   
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await db.SaveChangesAsync();
                    return entity;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_CurriculumVitae.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Enable = false;
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}