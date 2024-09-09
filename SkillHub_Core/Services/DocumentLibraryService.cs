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
    public class DocumentLibraryService
    {
        public static async Task<tbl_DocumentLibrary> Insert(DocumentLibraryCreate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var model = new tbl_DocumentLibrary(itemModel);
                        var directory = await db.tbl_DocumentLibraryDirectory.SingleOrDefaultAsync(x => x.Enable == true && x.Id == model.DirectoryId);
                        if (directory == null)
                            throw new Exception("Chủ đề không tồn tại");
                        model.CreatedBy = userLogin.FullName;
                        db.tbl_DocumentLibrary.Add(model);
                        await db.SaveChangesAsync();
                        //Cập nhật số lượng tài liệu của chủ đề
                        var totalDocument = await db.tbl_DocumentLibrary.Where(x => x.Enable == true && x.DirectoryId == directory.Id).CountAsync();
                        directory.TotalDocument = totalDocument;
                        await db.SaveChangesAsync();
                        tran.Commit();
                        return model;
                    }
                    catch(Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
        }

        public static async Task<tbl_DocumentLibrary> Update(DocumentLibraryUpdate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_DocumentLibrary.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.ModifiedBy = userLogin.FullName;
                entity.FileUrl = itemModel.FileUrl == null ? entity.FileUrl : string.IsNullOrEmpty(itemModel.FileUrl) ? entity.FileUrl : itemModel.FileUrl;
                entity.FileUrlRead = itemModel.FileUrlRead == null ? entity.FileUrlRead : string.IsNullOrEmpty(itemModel.FileUrlRead) ? entity.FileUrlRead : itemModel.FileUrlRead;
                entity.Background = itemModel.Background == null ? entity.Background : string.IsNullOrEmpty(itemModel.Background) ? entity.Background : itemModel.Background;
                entity.Description = itemModel.Description == null ? entity.Description : string.IsNullOrEmpty(itemModel.Description) ? entity.Description : itemModel.Description;
                await db.SaveChangesAsync();
                return entity;
            }    
        }

        public static async Task<AppDomainResult> GetAll(DocumentLibrarySearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new DocumentLibrarySearch();
                string sql = $"Get_DocumentLibrary " +
                    $"@PageIndex = {baseSearch.PageIndex}, " +
                    $"@PageSize = {baseSearch.PageSize}, " +
                    $"@DirectoryId = N'{(baseSearch.DirectoryId == null ? 0 : baseSearch.DirectoryId)}', " +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType == false ? 0 : 1)}'";
                var data = await db.SqlQuery<Get_DocumentLibrary>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_DocumentLibrary(i)).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }

        public static async Task<tbl_DocumentLibrary> GetById(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_DocumentLibrary.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    return null;
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_DocumentLibrary.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }      

    }
}