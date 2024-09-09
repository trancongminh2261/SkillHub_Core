/*using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Services
{
    public class ColumnTypeService
    {
        public static async Task<tbl_ScoreColumnTemplate> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await db.tbl_ColumnType.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task<tbl_ScoreColumnTemplate> Insert(ColumnTypeCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var model = new tbl_ScoreColumnTemplate(itemModel);
                    if (itemModel.Type == 1 && itemModel.ScoreFactor == null)
                        throw new Exception("Vui lòng nhập hệ số điểm");
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    db.tbl_ColumnType.Add(model);
                    await db.SaveChangesAsync();
                    return model;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task<tbl_ScoreColumnTemplate> Update(ColumnTypeUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    //validate input
                    var entity = await db.tbl_ColumnType.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                    entity.Name = itemModel.Name ?? entity.Name;
                    entity.ScoreFactor = itemModel.ScoreFactor ?? entity.ScoreFactor;
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

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_ColumnType.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
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
}*/