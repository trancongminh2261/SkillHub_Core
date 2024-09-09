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
    public class ScoreColumnTemplateService
    {
        /// <summary>
        /// lấy danh sách cột trong bảng điểm mẫu
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public static async Task<AppDomainResult> GetAll(ScoreColumnTemplateSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ScoreColumnTemplateSearch();
                string sql = $"Get_ScoreColumnTemplate " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ScoreBoardTemplateId = {baseSearch.ScoreBoardTemplateId}";
                var data = await db.SqlQuery<Get_ScoreColumnTemplate>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_ScoreColumnTemplate(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        /// <summary>
        /// lấy cột điểm mẫu theo id 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<tbl_ScoreColumnTemplate> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await db.tbl_ScoreColumnTemplate.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// thêm mới cột trong bảng điểm mẫu
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_ScoreColumnTemplate> Insert(ScoreColumnTemplateCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {

                try
                {
                    var model = new tbl_ScoreColumnTemplate(itemModel);
                    //kiểm tra xem bảng điểm mẫu tồn tại hay không
                    var scoreBoardTemplate = await db.tbl_ScoreboardTemplate.Where(x => x.Enable == true && x.Id == itemModel.ScoreBoardTemplateId).FirstOrDefaultAsync();
                    if (scoreBoardTemplate == null)
                        throw new Exception("Không tìm thấy bảng điểm mẫu");
                    //vị trí thêm mới = vị trí lớn nhất + 1
                    /*var maxIndex = await db.tbl_ScoreColumnTemplate
                                    .Where(x => x.Enable == true && x.ScoreBoardTemplateId == itemModel.ScoreBoardTemplateId)
                                    .MaxAsync(x => x.Index);*/
                    var maxIndex = 0;
                    var query = db.tbl_ScoreColumnTemplate
                        .Where(x => x.Enable == true && x.ScoreBoardTemplateId == itemModel.ScoreBoardTemplateId);

                    if (await query.AnyAsync())
                    {
                        maxIndex = await query.MaxAsync(x => x.Index);
                    }
                    // Logic mới không nhập hệ số khi tạo cột điểm trong bảng điểm nữa
                    //if(itemModel.Type == 1 && itemModel.Factor == null)
                    //{
                    //    throw new Exception("Vui lòng nhập hệ số");
                    //}
                    model.Index = maxIndex + 1;
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    db.tbl_ScoreColumnTemplate.Add(model);
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
        /// chỉnh sửa cột trong bảng điểm mẫu
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_ScoreColumnTemplate> Update(ScoreColumnTemplateUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_ScoreColumnTemplate.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.ScoreBoardTemplateId = itemModel.ScoreBoardTemplateId ?? entity.ScoreBoardTemplateId;
                    entity.Name = itemModel.Name ?? entity.Name;
                    entity.Factor = itemModel.Factor ?? entity.Factor;
                    //Nếu vị trí muốn đổi có cột điểm nào tồn tại thì hoán đổi vị trí cho nhau
                    /*if (itemModel.Index != null)
                    {
                        var checkIndex = await db.tbl_ScoreColumnTemplate.SingleOrDefaultAsync(x => x.Enable == true && x.ScoreBoardTemplateId == entity.ScoreBoardTemplateId && x.Index == itemModel.Index);
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
        /// Xóa cột trong bảng điểm mẫu
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_ScoreColumnTemplate.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
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