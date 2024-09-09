using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class QuestionInVideoService
    {
        public static async Task<tbl_QuestionInVideo> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_QuestionInVideo.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_QuestionInVideo> Insert(QuestionInVideoCreate questionInVideoCreate, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var validate = await db.tbl_Product.AnyAsync(x => x.Id == questionInVideoCreate.VideoCourseId);
                    if (!validate)
                        throw new Exception("Không tìm thấy khoá học");
                    var model = new tbl_QuestionInVideo(questionInVideoCreate);

                    //Kiểm tra xem học viên đã mua khóa này hay chưa
                    if (user.RoleId == ((int)RoleEnum.student))
                    {
                        var checkBought = await db.tbl_VideoCourseStudent
                            .AnyAsync(x => x.VideoCourseId == questionInVideoCreate.VideoCourseId && x.UserId == user.UserInformationId && x.Enable == true);
                        if(!checkBought)
                            throw new Exception("Bạn chưa mua khóa học này");
                    }

                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    model.UserId = user.UserInformationId;
                    db.tbl_QuestionInVideo.Add(model);
                    await db.SaveChangesAsync();
                    return model;
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
        }
        public static async Task Delete(int id, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_QuestionInVideo.SingleOrDefaultAsync(x => x.Id == id);
                    if (user.RoleId != ((int)RoleEnum.admin) && entity.UserId != user.UserInformationId)
                        throw new Exception("Không có quyền xoá");
                    entity.Enable = false;
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public static async Task<AppDomainResult> GetAll(QuestionInVideoSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {

                //Kiểm tra xem học viên đã mua khóa này hay chưa
                if (user.RoleId == ((int)RoleEnum.student))
                {
                    var checkBought = await db.tbl_VideoCourseStudent
                        .AnyAsync(x => x.VideoCourseId == baseSearch.VideoCourseId && x.UserId == user.UserInformationId && x.Enable == true);
                    if (!checkBought)
                        throw new Exception("Bạn chưa mua khóa học này");
                }

                if (baseSearch == null) return new AppDomainResult { TotalRow = 0, Data = null };
                string sql = $"Get_QuestionInVideo @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@VideoCourseId = {baseSearch.VideoCourseId}";
                var data = await db.SqlQuery<Get_QuestionInVideo>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_QuestionInVideo(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}