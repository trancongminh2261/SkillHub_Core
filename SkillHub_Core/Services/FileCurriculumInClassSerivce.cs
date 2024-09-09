using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class FileCurriculumInClassSerivce
    {
        public static async Task<tbl_FileCurriculumInClass> Insert(FileInCurriculumDetailCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var curriculumDetailInClass = await db.tbl_CurriculumDetailInClass.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumDetailId);
                if (curriculumDetailInClass == null)
                    throw new Exception("Không tìm thấy chủ đề");
                var model = new tbl_FileCurriculumInClass(itemModel);
                model.IsComplete = false;
                model.IsHide = true;
                model.ClassId = 0;
                model.CreatedBy = model.ModifiedBy = user.FullName;
                model.Index = 1;
                var fileCurriculum = await db.tbl_FileCurriculumInClass.Where(x => x.CurriculumDetailId == itemModel.CurriculumDetailId && x.Enable == true).OrderBy(x => x.Index).ToListAsync();
                if (fileCurriculum.Any())
                {
                    model.Index = fileCurriculum.Select(x => x.Index).LastOrDefault() + 1;
                }
                db.tbl_FileCurriculumInClass.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }

        public static async Task<tbl_FileCurriculumInClass> Update(FileCurriculumInClassUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_FileCurriculumInClass.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy");
                entity.ModifiedBy = user.FullName;
                //Nếu vị trí muốn đổi có file tồn tại thì hoán đổi vị trí cho nhau
                if (itemModel.Index != null)
                {
                    var fileOfIndexExsist = await db.tbl_FileCurriculumInClass.SingleOrDefaultAsync(x => x.Enable == true && x.CurriculumDetailId == entity.CurriculumDetailId && x.Index == itemModel.Index);
                    if (fileOfIndexExsist != null)
                    {
                        fileOfIndexExsist.Index = entity.Index;
                    }
                    entity.Index = itemModel.Index;
                }
                await db.SaveChangesAsync();
                return entity;
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_FileCurriculumInClass.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }

        public static async Task Hide(int id)
        {
            using (var db = new lmsDbContext())
            {
                var file = await db.tbl_FileCurriculumInClass.SingleOrDefaultAsync(x => x.Id == id);
                if (file == null)
                    throw new Exception("Không tìm thấy tài liệu");
                file.IsHide = !file.IsHide;
                await db.SaveChangesAsync();
            }
        }

        public static async Task<List<Get_FileCurriculumInClass>> GetFileCurriculumInClass(FilesCurriculumInClassSearch baseSearch,tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new FilesCurriculumInClassSearch();
                string sql = $"Get_FileCurriculumInClass " +
                    $"@CurriculumDetailId = {(baseSearch.CurriculumDetailInClassId == null ? 0 : baseSearch.CurriculumDetailInClassId)}, " +
                    $"@RoleId = {userLog.RoleId} ";
                var data = await db.SqlQuery<Get_FileCurriculumInClass>(sql);
                return data;
            }
        }

        public static async Task Complete(int id, tbl_UserInformation user)
        {
            try
            {
                using (var db = new lmsDbContext())
                {
                    var entity = await db.tbl_FileCurriculumInClass.SingleOrDefaultAsync(x => x.Id == id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy");
                    entity.IsComplete = true;
                    entity.ModifiedOn = DateTime.Now;
                    entity.ModifiedBy = user.FullName;
                    await db.SaveChangesAsync();
                    await UpdateCompletePercent(entity.CurriculumDetailId.Value);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static async Task UpdateCompletePercent(int curriculumDetailId)
        {
            try
            {
                using (var db = new lmsDbContext())
                {
                    var curriculumDetail = await db.tbl_CurriculumDetailInClass.SingleOrDefaultAsync(x => x.Id == curriculumDetailId);
                    if (curriculumDetail == null) return;
                    var files = await db.tbl_FileCurriculumInClass.Where(x => x.CurriculumDetailId == curriculumDetailId && x.Enable == true).ToListAsync();
                    if (files.Any())
                    {
                        //Tính phần trăm hoàn thành cho các chương trong giáo trình
                        int count = files.Count();
                        int completes = files.Where(x => x.IsComplete == true).Count();
                        curriculumDetail.CompletePercent = (completes * 100) / count;
                        if (curriculumDetail.CompletePercent == 100)
                            curriculumDetail.IsComplete = true;
                        await db.SaveChangesAsync();
                        //Tính phần trăm hoàn thành cho giáo trình
                        var curriculumInClass = await db.tbl_CurriculumInClass.SingleOrDefaultAsync(x => x.Id == curriculumDetail.CurriculumIdInClass);
                        if (curriculumInClass == null) return;
                        var detailsInCurriculum = await db.tbl_CurriculumDetailInClass.Where(x => x.CurriculumIdInClass == curriculumInClass.Id && x.Enable == true).ToListAsync();
                        if (detailsInCurriculum.Any())
                        {
                            int countDIC = detailsInCurriculum.Count();
                            int completesDIC = detailsInCurriculum.Where(x => x.IsComplete == true && x.Enable == true).Count();
                            curriculumInClass.CompletePercent = (completesDIC * 100) / countDIC;
                            if (curriculumInClass.CompletePercent == 100)
                                curriculumInClass.IsComplete = true;
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<List<tbl_FileCurriculumInClass>> FileCurriculumInClassIndex(List<FileCurriculumInClassUpdate> request, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                List<tbl_FileCurriculumInClass> fileCurriculumDetails = new List<tbl_FileCurriculumInClass>();
                foreach (var item in request)
                {
                    tbl_FileCurriculumInClass curriculumDetail = await db.tbl_FileCurriculumInClass.SingleOrDefaultAsync(x => x.Id == item.Id && x.Enable == true);
                    if (curriculumDetail != null)
                    {
                        curriculumDetail.Index = item.Index;
                        curriculumDetail.ModifiedBy = user.FullName;
                        curriculumDetail.ModifiedOn = DateTime.Now;
                        fileCurriculumDetails.Add(curriculumDetail);
                    }
                }
                if (fileCurriculumDetails.Any())
                {
                    await db.SaveChangesAsync();
                }

                return fileCurriculumDetails;
            }
        }
    }
}