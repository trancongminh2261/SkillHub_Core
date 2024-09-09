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
    public class PackageSkillService
    {
        public static async Task<tbl_PackageSkill> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_PackageSkill.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_PackageSkill> Insert(PackageSkillCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var packageSection = await db.tbl_PackageSection.SingleOrDefaultAsync(x => x.Id == itemModel.PackageSectionId);
                if (packageSection == null)
                    throw new Exception("Không tìm thấy phần");
                var ieltsExam = await db.tbl_IeltsExam.SingleOrDefaultAsync(x => x.Id == itemModel.IeltsExamId);
                if (ieltsExam == null)
                    throw new Exception("Không tìm thấy đề");

                var model = new tbl_PackageSkill(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_PackageSkill.Add(model);

                packageSection.ExamQuatity += 1;

                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_PackageSkill> Update(PackageSkillUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_PackageSkill.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Name = itemModel.Name ?? entity.Name;
                entity.Thumbnail = itemModel.Thumbnail ?? entity.Thumbnail;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_PackageSkill.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;

                var packageSection = await db.tbl_PackageSection.SingleOrDefaultAsync(x => x.Id == entity.PackageSectionId);
                if (packageSection == null)
                    throw new Exception("Không tìm thấy phần");

                packageSection.ExamQuatity -= 1;

                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(PackageSkillSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new PackageSkillSearch();

                var l = await db.tbl_PackageSkill.Where(x => x.Enable == true
                && x.PackageSectionId == baseSearch.PackageSectionId).OrderBy(x => x.Name).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
        public class RankModel
        {
            public List<RankItem> Items { get; set; }
            public RankItem MyRank { get; set; }
            public RankModel()
            {
                Items = new List<RankItem>();
            }
        }
        public class RankItem
        { 
            public int Id { get; set; }
            public int StudentId { get; set; }
            public string StudentName { get; set; }
            public string StudentThumbnail { get; set; }
            /// <summary>
            /// Điểm số
            /// </summary>
            public double MyPoint { get; set; }
            /// <summary>
            /// Thời gian làm bài
            /// </summary>
            public double TimeSpent { get; set; }
            /// <summary>
            /// thứ hạng
            /// </summary>
            public int Rank { get; set; }
        }
        public static async Task<RankModel> GetRank(int packageSkillId, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var result = new RankModel();

                var studentIds = await db.tbl_IeltsExamResult
                    .Where(x => x.ValueId == packageSkillId && x.Type == 4 && x.Enable == true)
                    .Select(x=>x.StudentId).Distinct().ToListAsync();
                if (!studentIds.Any())
                    return result;
                var rankItems = new List<RankItem>();
                foreach (var studentId in studentIds)
                {
                    var ieltsExamResult = await db.tbl_IeltsExamResult
                        .Where(x => x.ValueId == packageSkillId && x.Type == 4 && x.Enable == true && x.StudentId == studentId)
                        .OrderByDescending(x => x.MyPoint).ThenBy(x => x.TimeSpent)
                        .FirstOrDefaultAsync();
                    if (ieltsExamResult != null)
                    {
                        rankItems.Add(new RankItem
                        {
                            Id = ieltsExamResult.Id,
                            MyPoint = ieltsExamResult.MyPoint,
                            StudentId = ieltsExamResult.StudentId,
                            TimeSpent = ieltsExamResult.TimeSpent
                        });
                    }
                }
                rankItems = rankItems.OrderByDescending(x => x.MyPoint).ThenBy(x => x.TimeSpent).ToList();

                var myRankInfo = rankItems.FirstOrDefault(x => x.StudentId == userLog.UserInformationId);
                if (myRankInfo != null)
                {
                    int myRank = rankItems.FindIndex(x => x.Id == myRankInfo.Id) + 1;
                    var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == myRankInfo.StudentId);
                    result.MyRank = new RankItem
                    {
                        Id = myRankInfo.Id,
                        MyPoint = myRankInfo.MyPoint,
                        StudentId = student.UserInformationId,
                        StudentName = student.FullName,
                        StudentThumbnail = student.Avatar,
                        TimeSpent = myRankInfo.TimeSpent,
                        Rank = myRank
                    };
                }

                rankItems = rankItems.Take(5).ToList();
                int rank = 1;
                foreach (var item in rankItems)
                {
                    var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == item.StudentId);
                    result.Items.Add(new RankItem
                    {
                        Id = item.Id,
                        MyPoint = item.MyPoint,
                        StudentId = student.UserInformationId,
                        StudentName = student.FullName,
                        StudentThumbnail = student.Avatar,
                        TimeSpent = item.TimeSpent,
                        Rank = rank
                    });
                    rank++;
                }
                return result;
            }
        }
    }
}