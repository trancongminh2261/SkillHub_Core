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

namespace LMSCore.Services
{
    public class NewsFeedLikeService
    {
        public static async Task<tbl_NewsFeedLike> Insert(NewsFeedLikeCreate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var model = new tbl_NewsFeedLike(itemModel);
                        var newsFeed = await db.tbl_NewsFeed.SingleOrDefaultAsync(x => x.Id == model.NewsFeedId && x.Enable == true);
                        if (newsFeed == null)
                            throw new Exception("Bản tin không tồn tại");
                        var checkLikeExists = await db.tbl_NewsFeedLike.SingleOrDefaultAsync(x => x.Enable == true && x.NewsFeedId == newsFeed.Id && x.CreatedIdBy == userLogin.UserInformationId);
                        //Nếu đã tồn tại trong bảng like thì thay đổi giá trị cờ IsLike
                        if (checkLikeExists != null)
                        {
                            checkLikeExists.NewsFeedId = newsFeed.Id;
                            checkLikeExists.IsLike = !checkLikeExists.IsLike;
                        }
                        //Nếu chưa bao giờ like
                        if (checkLikeExists == null)
                        {
                            model.CreatedIdBy = userLogin.UserInformationId;
                            model.IsLike = true;
                            model.CreatedBy = model.ModifiedBy =userLogin.FullName;
                            db.tbl_NewsFeedLike.Add(model);
                        }                       
                        await db.SaveChangesAsync();
                        //Cập nhật lại số lượng like
                        var newsFeedLikes = await db.tbl_NewsFeedLike.Where(x => x.Enable == true && x.NewsFeedId == model.NewsFeedId && x.IsLike == true).ToListAsync();
                        newsFeed.TotalLike = newsFeedLikes.Count();
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

        public static async Task<AppDomainResult> GetAll(int? NewsFeedId)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_NewsFeedLike @NewsFeedId = {(NewsFeedId ?? 0)},";
                var data = await db.SqlQuery<Get_NewsFeedLike>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = data[0].TotalRow ?? 0;
                var result = data.Select(i => new tbl_NewsFeedLike(i)).ToList();
                return new AppDomainResult { Data = result,TotalRow = totalRow ,Success = true };
            }
        }

    }
}