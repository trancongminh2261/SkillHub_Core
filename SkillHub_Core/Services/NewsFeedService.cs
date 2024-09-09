using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using Newtonsoft.Json;
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
    public class NewsFeedService
    {
        public static async Task<tbl_NewsFeed> Insert(NewsFeedCreate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_NewsFeed(itemModel);
                //Check chọn nhóm hoặc trung tâm
                if ((itemModel.ListBranchId == null || itemModel.ListBranchId == new List<int>()) && itemModel.NewsFeedGroupId == null)
                    throw new Exception("Vui lòng chọn nhóm hoặc trung tâm");
                //Trường hợp gắn background
                model.IsBackGround = false;
                if (!string.IsNullOrEmpty(itemModel.BackGroundUrl))
                    model.IsBackGround = true;
                model.CreatedIdBy = userLogin.UserInformationId;
                //Trường hợp chọn trung tâm
                if (itemModel.ListBranchId != null)
                {
                    model.BranchIds = string.Join(",", itemModel.ListBranchId).ToString();
                }
                //Trường hợp thêm file
                if (itemModel.FileListCreate.Any())
                {
                    model.Files = JsonConvert.SerializeObject(itemModel.FileListCreate);
                    model.TotalFile = itemModel.FileListCreate.Count();
                }
                model.CreatedBy = userLogin.FullName;
                model.NewsFeedGroupId = model.NewsFeedGroupId ?? 0;
                model.TotalLike = 0;
                model.TotalComment = 0;
                db.tbl_NewsFeed.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }

        public static async Task<tbl_NewsFeed> Update(NewsFeedUpdate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_NewsFeed.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (userLogin.RoleId != (int)RoleEnum.admin && entity.CreatedIdBy != userLogin.UserInformationId)
                {
                    throw new Exception("Không có quyền thao tác");
                }
                //Kiểm tra thay đổi trung tâm
                if (itemModel.ListBranchId != null)
                {
                    entity.BranchIds = itemModel.ListBranchId.Any() ? string.Join(",", itemModel.ListBranchId).ToString() : null;
                }
                //Kiểm tra thay đổi background
                if (itemModel.BackGroundUrl != null)
                {
                    entity.BackGroundUrl = itemModel.BackGroundUrl;
                    entity.IsBackGround = !string.IsNullOrEmpty(itemModel.BackGroundUrl) ? true : false;
                }
                //Kiểm tra thay đổi file
                if (itemModel.FileListUpdate != null)
                {
                    entity.Files = itemModel.FileListUpdate.Any() ? JsonConvert.SerializeObject(itemModel.FileListUpdate) : null;
                    entity.TotalFile = itemModel.FileListUpdate.Count();
                }
                entity.NewsFeedGroupId = itemModel.NewsFeedGroupId ?? entity.NewsFeedGroupId;
                entity.Content = itemModel.Content ?? entity.Content;
                entity.Color = itemModel.Color ?? entity.Color;
                entity.ModifiedBy = userLogin.FullName;
                //Kiểm tra branch vs group không được add chung
                if (entity.NewsFeedGroupId != null && entity.BranchIds != null)
                    throw new Exception("Vui lòng chọn nhóm hoặc trung tâm");
                await db.SaveChangesAsync();
                return entity;
            }
        }

        public static async Task Delete(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_NewsFeed.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu !");
                if (userLogin.RoleId != (int)RoleEnum.admin && entity.CreatedIdBy != userLogin.UserInformationId)
                {
                    throw new Exception("Không có quyền thao tác");
                }
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }

        public static async Task<AppDomainResult> GetAll(NewsFeedSearch baseSearch, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new NewsFeedSearch();
                string sql = $"Get_NewsFeed @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Search = N'{baseSearch.Search ?? ""}'," +
                    $"@NewsFeedGroupId = N'{(baseSearch.NewsFeedGroupId == null ? 0 : baseSearch.NewsFeedGroupId)}'," +
                    $"@BranchIds = N'{(baseSearch.BranchIds == null ? 0 : baseSearch.BranchIds)}'," +
                    $"@MyBranchIds = N'{(userLogin.RoleId == ((int)RoleEnum.admin) ? "" : (userLogin.BranchIds == null ? "0" : userLogin.BranchIds))}'," +
                    $"@AccountLogin = {userLogin.UserInformationId}," +
                    $"@RoleId = {userLogin.RoleId}";
                var data = await db.SqlQuery<Get_NewsFeed>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = data[0].TotalRow ?? 0;
                var result = data.Select(i => new tbl_NewsFeed(i)).OrderByDescending(x=>x.Pin).ThenByDescending(x=>x.PinDate).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }

        public static async Task<tbl_NewsFeed> GetById(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_NewsFeed.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    return null;
                //Info branch
                if (!string.IsNullOrEmpty(entity.BranchIds))
                {
                    var branchIdList = entity.BranchIds.Split(',').Select(y => Convert.ToInt32(y)).ToList();
                    entity.BranchIdList = branchIdList;
                    entity.BranchNameList = await db.tbl_Branch.Where(x => branchIdList.Contains(x.Id) && x.Enable == true).Select(x => x.Name).ToListAsync();
                }
                //Info creater in group
                if (entity.NewsFeedGroupId != null && entity.NewsFeedGroupId != 0)
                {
                    var group = await db.tbl_NewsFeedGroup.SingleOrDefaultAsync(x => x.Id == entity.NewsFeedGroupId && x.Enable == true);
                    if (group != null)
                        entity.GroupName = group.Name ?? string.Empty;
                    var userInGroup = await db.tbl_UserInNFGroup.SingleOrDefaultAsync(x => x.UserId == entity.CreatedIdBy && x.Enable == true && x.NewsFeedGroupId == entity.NewsFeedGroupId);
                    entity.TypeNameGroup = userInGroup != null ? userInGroup.TypeName : string.Empty;
                }
                //Info creater
                var user = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == entity.CreatedIdBy && x.Enable == true);
                //Info comment
                //Info like
                var like = await db.tbl_NewsFeedLike.CountAsync(x => x.NewsFeedId == entity.Id && x.CreatedIdBy == userLogin.UserInformationId && x.IsLike == true);
                entity.IsLike = like;
                entity.RoleName = user == null ? null : user.RoleName;
                entity.Avatar = user == null ? null : user.Avatar;
                return entity;
            }
        }


        public static async Task Pin(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_NewsFeed.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu !");
                entity.Pin = !entity.Pin;
                if(entity.Pin == false)
                    entity.PinDate = null;
                else
                    entity.PinDate = DateTime.Now;
                await db.SaveChangesAsync();
            }

        }
    }
}