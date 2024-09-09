using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LMSCore.Services
{
    public class ScoreboardTemplateService
    {
        /// <summary>
        /// lấy danh sách bảng điểm mẫu
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public static async Task<AppDomainResult> GetAll(ScoreBoardTemplateSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ScoreBoardTemplateSearch();
                string sql = $"Get_ScoreBoardTemplate " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Search = N'{baseSearch.Search}'";
                var data = await db.SqlQuery<Get_ScoreBoardTemplate>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_ScoreBoardTemplate(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        /// <summary>
        /// Tìm kiếm bảng điểm mẫu theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<tbl_ScoreBoardTemplate> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await db.tbl_ScoreboardTemplate.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// thêm mới bảng điểm mẫu
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_ScoreBoardTemplate> Insert(ScoreboardTemplateCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using(var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var model = new tbl_ScoreBoardTemplate(itemModel);
                        var checkCode = await db.tbl_ScoreboardTemplate.SingleOrDefaultAsync(x => x.Enable == true && x.Code.ToUpper() == itemModel.Code.ToUpper());
                        if (checkCode != null)
                            throw new Exception("Mã đã tồn tại");
                        model.CreatedBy = model.ModifiedBy = user.FullName;
                        db.tbl_ScoreboardTemplate.Add(model);
                        await db.SaveChangesAsync();
                        //thêm cột điểm trung bình vào bảng điểm mẫu mới tạo
                        var column = new tbl_ScoreColumnTemplate
                        {
                            ScoreBoardTemplateId = model.Id,
                            Name = "Điểm trung bình",
                            Factor = 0,
                            Index = 1,
                            Type = 2,
                            TypeName = "Điểm trung bình",
                            Enable = true,
                            CreatedBy = user.FullName,
                            CreatedOn = DateTime.Now,
                            ModifiedBy = user.FullName,
                            ModifiedOn = DateTime.Now
                        };
                        db.tbl_ScoreColumnTemplate.Add(column);
                        await db.SaveChangesAsync();
                        tran.Commit();
                        return model;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
                
            }
        }

        /// <summary>
        /// Chỉnh sửa bảng điểm mẫu
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_ScoreBoardTemplate> Update(ScoreboardTemplateUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_ScoreboardTemplate.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Name = itemModel.Name ?? entity.Name;
                    entity.Code = itemModel.Code ?? entity.Code;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await db.SaveChangesAsync();
                    return entity;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Xóa bảng điểm mẫu
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_ScoreboardTemplate.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
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