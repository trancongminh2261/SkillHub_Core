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
using System.Web;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class NewsFeedReplyService
    {
        public static async Task<tbl_NewsFeedReply> Insert(NewsFeedReplyCreate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var model = new tbl_NewsFeedReply(itemModel);
                        var comment = await db.tbl_NewsFeedComment.SingleOrDefaultAsync(x => x.Enable == true && x.Id == model.NewsFeedCommentId);
                        if (comment == null)
                            throw new Exception("Bình luận không tồn tại");
                        model.CreatedBy = model.ModifiedBy = userLogin.FullName;
                        model.CreatedIdBy = userLogin.UserInformationId;
                        db.tbl_NewsFeedReply.Add(model);
                        await db.SaveChangesAsync();
                        //Cập nhật số lượng reply 
                        var replys = await db.tbl_NewsFeedReply.Where(x => x.Enable == true && x.NewsFeedCommentId == comment.Id).ToListAsync();
                        comment.TotalReply = replys.Count();
                        await db.SaveChangesAsync();
                        tran.Commit();
                        return model;
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
        }

        public static async Task<tbl_NewsFeedReply> Update(NewsFeedReplyUpdate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_NewsFeedReply.SingleOrDefaultAsync(x => x.Enable == true && x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (entity.CreatedIdBy != userLogin.UserInformationId)
                    throw new Exception("Không có quyền thao tác");
                entity.Content = itemModel.Content ?? entity.Content;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entity = await db.tbl_NewsFeedReply.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                        if (entity == null)
                            throw new Exception("Không tìm thấy dữ liệu !");
                        var comment = await db.tbl_NewsFeedComment.SingleOrDefaultAsync(x => x.Enable == true && x.Id == entity.NewsFeedCommentId);
                        if (comment == null)
                        {
                            throw new Exception("Bình luận không tồn tại");
                        }
                        var newsFeed = await db.tbl_NewsFeed.SingleOrDefaultAsync(x => x.Enable == true && x.Id == comment.NewsFeedId);
                        if (newsFeed == null)
                        {
                            throw new Exception("Bản tin không tồn tại");
                        }
                        //Admin, người tạo bình luận này người tạo ra reply này và người tạo ra bản tin sẽ được quyền xóa reply này
                        if (userLogin.RoleId != (int)RoleEnum.admin && entity.CreatedIdBy != userLogin.UserInformationId && newsFeed.CreatedIdBy != userLogin.UserInformationId && comment.CreatedIdBy != userLogin.UserInformationId)
                        {
                            throw new Exception("Không có quyền thao tác");
                        }
                        entity.Enable = false;
                        await db.SaveChangesAsync();
                        //Cập nhật lại số lượng reply
                        var replys = await db.tbl_NewsFeedReply.Where(x => x.Enable == true && x.NewsFeedCommentId == comment.Id).ToListAsync();
                        comment.TotalReply = replys.Count();
                        await db.SaveChangesAsync();
                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }

            }
        }

        public static async Task<AppDomainResult> GetAll(NewsFeedReplySearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new NewsFeedReplySearch();
                string sql = $"Get_NewsFeedReply @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@NewsFeedCommentId = N'{(baseSearch.NewsFeedCommentId == null ? 0 : baseSearch.NewsFeedCommentId)}'";
                var data = await db.SqlQuery<Get_NewsFeedReply>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = data[0].TotalRow ?? 0;
                var result = data.Select(i => new tbl_NewsFeedReply(i)).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
    }
}