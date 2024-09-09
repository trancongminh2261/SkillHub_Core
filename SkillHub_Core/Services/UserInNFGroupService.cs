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
    public class UserInNFGroupService
    {
        public static async Task<object> Insert(UserInNFGroupCreate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (userLogin.RoleId != (int)RoleEnum.admin)
                        {
                            var userLoginInGroup = await db.tbl_UserInNFGroup.SingleOrDefaultAsync(x => x.NewsFeedGroupId == itemModel.NewsFeedGroupId && x.UserId == userLogin.UserInformationId && x.Enable == true);
                            if (userLoginInGroup.Type != 1 || userLoginInGroup == null)
                            {
                                throw new Exception("Không có quyền thao tác");
                            }
                        }
                        var group = await db.tbl_NewsFeedGroup.SingleOrDefaultAsync(x => x.Enable == true && x.Id == itemModel.NewsFeedGroupId);
                        if (group == null)
                            throw new Exception("Nhóm không tồn tại !");
                        if (!itemModel.Members.Any())
                            throw new Exception("Vui lòng chọn người dùng !");
                        var usersIntoGroup = new List<Members>();
                        foreach (var member in itemModel.Members)
                        {
                            var user = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == member.UserId && x.Enable == true);
                            if (user == null)
                                throw new Exception("Người dùng không tồn tại !");
                            //Kiểm tra user thêm vào đã có trong nhóm chưa
                            var memberExistsGroup = await db.tbl_UserInNFGroup.AnyAsync(x => x.NewsFeedGroupId == itemModel.NewsFeedGroupId && x.UserId == member.UserId && x.Enable == true);
                            if (memberExistsGroup)
                            {
                                continue;
                            }
                            //Lấy những user không bị trùng trong nhóm
                            usersIntoGroup.Add(member);
                        }
                        var model = new tbl_UserInNFGroup(itemModel);
                        if (usersIntoGroup.Any())
                        {
                            db.tbl_UserInNFGroup.AddRange(usersIntoGroup.Select(x => new tbl_UserInNFGroup
                            {
                                CreatedBy = userLogin.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = true,
                                ModifiedBy = userLogin.FullName,
                                ModifiedOn = DateTime.Now,
                                NewsFeedGroupId = model.NewsFeedGroupId,
                                Type = x.Type,
                                TypeName = x.TypeName,
                                UserId = x.UserId
                            }));
                            group.Members = group.Members + usersIntoGroup.Count();
                            await db.SaveChangesAsync();
                        }
                        await db.SaveChangesAsync();
                        tran.Commit();
                        return usersIntoGroup;
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
        }

        public static async Task<tbl_UserInNFGroup> Update(UserInNFGroupUpdate itemModel, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_UserInNFGroup.SingleOrDefaultAsync(x => x.Enable == true && x.Id == itemModel.Id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu !");
                if (userLogin.RoleId != (int)RoleEnum.admin)
                {
                    var userLoginInGroup = await db.tbl_UserInNFGroup.SingleOrDefaultAsync(x => x.Enable == true && x.NewsFeedGroupId == entity.NewsFeedGroupId && x.UserId == userLogin.UserInformationId);
                    if (userLoginInGroup.Type != 1 || userLoginInGroup == null)
                    {
                        throw new Exception("Không có quyền thao tác !");
                    }
                }
                entity.Type = itemModel.Type ?? entity.Type;
                entity.TypeName = itemModel.TypeName ?? entity.TypeName;
                entity.ModifiedBy = userLogin.FullName ?? entity.ModifiedBy;
                await db.SaveChangesAsync();
                return entity;
            }
        }

        public static async Task Delete(int id, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_UserInNFGroup.SingleOrDefaultAsync(x => x.Enable == true && x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu !");
                if (userLogin.RoleId != (int)RoleEnum.admin)
                {
                    var userLoginInGroup = await db.tbl_UserInNFGroup.SingleOrDefaultAsync(x => x.Enable == true && x.NewsFeedGroupId == entity.NewsFeedGroupId && x.UserId == userLogin.UserInformationId);
                    if (userLoginInGroup.Type != 1 || userLoginInGroup == null)
                    {
                        throw new Exception("Không có quyền thao tác !");
                    }
                }
                entity.Enable = false;
                await db.SaveChangesAsync();
                var newsFeedGroup = await db.tbl_NewsFeedGroup.SingleOrDefaultAsync(x => x.Id == entity.NewsFeedGroupId);
                if (newsFeedGroup == null)
                    throw new Exception("Không tìm thấy nhóm");
                newsFeedGroup.Members = await db.tbl_UserInNFGroup.CountAsync(x => x.NewsFeedGroupId == newsFeedGroup.Id && x.Enable == true);
                await db.SaveChangesAsync();
            }
        }

        public static async Task<tbl_UserInNFGroup> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_UserInNFGroup.SingleOrDefaultAsync(x => x.Enable == true && x.Id == id);
                if (entity == null)
                    return null;
                var userInGroup = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.Enable == true && x.UserInformationId == entity.UserId);
                var data = new tbl_UserInNFGroup(entity)
                {
                    FullName = userInGroup.FullName ?? "",
                    UserCode = userInGroup.UserCode ?? "",
                    RoleName = userInGroup.RoleName ?? "",
                    Avatar = userInGroup.Avatar ?? ""
                };
                return data;
            }
        }

        public static async Task<AppDomainResult> GetAll(UserInNFGroupSearch baseSearch, tbl_UserInformation userLogin)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null)
                    baseSearch = new UserInNFGroupSearch();
                string sql = $"Get_UserInNewsFeedGroup @Search = N'{baseSearch.Search ?? ""}', " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{((string.IsNullOrEmpty(baseSearch.FullName)) ? string.Empty : baseSearch.FullName)}'," +
                    $"@RoleId = N'{((string.IsNullOrEmpty(baseSearch.RoleId)) ? string.Empty : baseSearch.RoleId)}'," +
                    $"@Type = N'{(baseSearch.Type == null ? 0 : baseSearch.Type)}'," +
                    $"@NewsFeedGroupId = N'{(baseSearch.NewsFeedGroupId == null ? 0 : baseSearch.NewsFeedGroupId)}'," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = {(baseSearch.SortType == false ? 0 : 1)}";

                var data = await db.SqlQuery<Get_UserInNFGroup>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_UserInNFGroup(i)).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }

        public static async Task<AppDomainResult> GetUserNotInGroup(int? groupId)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_UserNotInNewsFeedGroup @NewsFeedGroupId = N'{groupId}'";
                var data = await db.SqlQuery<Get_UserNotInNFGroup>(sql);
                if (!data.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                return new AppDomainResult { Data = data, Success = true };
            }
        }

    }
}