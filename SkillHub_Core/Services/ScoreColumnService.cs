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
    public class ScoreColumnService
    {
        /// <summary>
        /// lấy danh sách cột trong bảng điểm
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public static async Task<AppDomainResult> GetAll(ScoreColumnSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ScoreColumnSearch();
                string sql = $"Get_ScoreColumn " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = {baseSearch.ClassId}";
                var data = await db.SqlQuery<Get_ScoreColumn>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_ScoreColumn(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        /// <summary>
        /// tìm kiếm cột trong bảng điểm theo id 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<tbl_ScoreColumn> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await db.tbl_ScoreColumn.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// thêm mới cột trong bảng điểm
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_ScoreColumn> Insert(ScoreColumnCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var model = new tbl_ScoreColumn(itemModel);
                    //vị trí thêm mới = vị trí lớn nhất + 1
                    var maxIndex = 0;
                    var query = db.tbl_ScoreColumn
                        .Where(x => x.Enable == true && x.ClassId == itemModel.ClassId);

                    if (await query.AnyAsync())
                    {
                        maxIndex = await query.MaxAsync(x => x.Index);
                    }
                    model.Index = maxIndex + 1;
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    db.tbl_ScoreColumn.Add(model);
                    await db.SaveChangesAsync();

                    return model;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        /// <summary>
        /// chỉnh sửa cột trong bảng điểm
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_ScoreColumn> Update(ScoreColumnUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_ScoreColumn.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.ClassId = itemModel.ClassId ?? entity.ClassId;
                    entity.Name = itemModel.Name ?? entity.Name;
                    entity.Factor = itemModel.Factor ?? entity.Factor;
                    //Nếu vị trí muốn đổi có cột điểm nào tồn tại thì hoán đổi vị trí cho nhau
                    /*if (itemModel.Index != null)
                    {
                        var checkIndex = await db.tbl_ScoreColumn.SingleOrDefaultAsync(x => x.Enable == true && x.ClassId == entity.ClassId && x.Index == itemModel.Index);
                        if (checkIndex != null)
                        {
                            checkIndex.Index = entity.Index;
                        }
                        entity.Index = (int)itemModel.Index;
                    }*/
                    entity.Index = itemModel.Index ?? entity.Index;
                    entity.Type = itemModel.Type ?? entity.Type;
                    entity.TypeName = itemModel.TypeName ?? entity.TypeName;
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
        /// Xóa cột trong bảng điểm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_ScoreColumn.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
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
