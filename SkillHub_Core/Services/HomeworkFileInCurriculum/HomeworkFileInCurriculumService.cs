using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.HomeworkFileInCurriculumDTO;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services.HomeworkFileInCurriculum
{
    public class HomeworkFileInCurriculumService
    {
        public static async Task<tbl_HomeworkFileInCurriculum> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_HomeworkFileInCurriculum.SingleOrDefaultAsync(x => x.Id == id);
                if (data != null)
                    data.TypeName = lmsEnum.HomeworkFileTypeName(data.Type);
                return data;
            }
        }

        public static async Task<List<tbl_HomeworkFileInCurriculum>> GetAll(HomeworkFileInCurriculumSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_HomeworkFileInCurriculum.Where(x => x.Enable == true
                && x.HomeworkInCurriculumId == baseSearch.HomeworkInCurriculumId
                && x.Type == baseSearch.Type
                ).OrderByDescending(x => x.CreatedOn).ToListAsync();
                data.ForEach(x => x.TypeName = lmsEnum.HomeworkFileTypeName(x.Type));
                return data;
            }
        }

        public static async Task<tbl_HomeworkFileInCurriculum> Insert(HomeworkFileInCurriculumCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_HomeworkFileInCurriculum(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_HomeworkFileInCurriculum.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }

        public static async Task<tbl_HomeworkFileInCurriculum> Update(HomeworkFileInCurriculumUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_HomeworkFileInCurriculum.SingleOrDefaultAsync(x => x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.File = itemModel.File ?? entity.File;
                entity.ModifiedOn = DateTime.Now;
                entity.ModifiedBy = user.FullName;
                await db.SaveChangesAsync();
                return entity;
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_HomeworkFileInCurriculum.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }

        public static async Task DeleteByHomeworkInCurriculumId(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_HomeworkFileInCurriculum.Where(x => x.HomeworkInCurriculumId == id).ToListAsync();
                if (entity.Count != 0)
                {
                    entity.ForEach(x => { x.Enable = false; });
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
