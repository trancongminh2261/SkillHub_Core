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
using LMSCore.Utilities;

namespace LMSCore.Services
{
    public class CurriculumDetailService
    {
        public static async Task<tbl_CurriculumDetail> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_CurriculumDetail.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_CurriculumDetail> Insert(CurriculumDetailCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_CurriculumDetail(itemModel);
                var curriculum = await db.tbl_Curriculum.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumId);
                if (curriculum == null)
                    throw new Exception("Không tìm thấy giáo trình");
                model.CreatedBy = model.ModifiedBy = user.FullName;
                model.Index = 1;
                var detailsInCurriculum = await db.tbl_CurriculumDetail.Where(x => x.Enable == true && x.CurriculumId == curriculum.Id && x.Index != null).OrderBy(x => x.Index).ToListAsync();
                if (detailsInCurriculum.Any())
                {
                    model.Index = detailsInCurriculum.Select(x => x.Index).LastOrDefault() + 1;
                }
                db.tbl_CurriculumDetail.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_CurriculumDetail> Update(CurriculumDetailUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_CurriculumDetail.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Name = string.IsNullOrEmpty(itemModel.Name) ? entity.Name : itemModel.Name;
                entity.ModifiedBy = user.FullName;
                //Nếu vị trí muốn đổi có chương nào tồn tại thì hoán đổi vị trí cho nhau
                if (itemModel.Index != null)
                {
                    var detailOfCurriculumExsist = await db.tbl_CurriculumDetail.SingleOrDefaultAsync(x => x.Enable == true && x.CurriculumId == entity.CurriculumId && x.Index == itemModel.Index);
                    if (detailOfCurriculumExsist != null)
                    {
                        detailOfCurriculumExsist.Index = entity.Index;
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
                var entity = await db.tbl_CurriculumDetail.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }

        public static async Task<AppDomainResult> GetAll(CurriculumDetailSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new CurriculumDetailSearch();

                var l = await db.tbl_CurriculumDetail.Where(x => x.Enable == true
                && x.CurriculumId == baseSearch.CurriculumId).OrderBy(x => x.Index).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Total = 0, Success = true };
                var curriculumDetail = (from i in l select new Get_InCurriculumDetail(i)).ToList();
                int totalFile = 0;
                var fileInCurriculumDetail = await db.tbl_FileInCurriculumDetail.Where(x => x.Enable == true).Select(x => new { x.CurriculumDetailId, x.FileName, x.FileUrl, x.Index, x.CreatedOn, x.Id }).ToListAsync();
                foreach (var c in curriculumDetail)
                {
                    var detailData = fileInCurriculumDetail.Where(x => x.CurriculumDetailId == c.Id).OrderBy(x => x.Index).ToList();
                    var detailModels = (from i in detailData select new FileInCurriculumDetailModel(i)).ToList();
                    c.Files = detailModels;
                    totalFile += detailData.Count;
                    if(detailModels.Count != 0)
                    {
                        foreach(var d in detailModels)
                        {
                            double fileSizeInMB = await GetDataConfig.GetFileSizeInMBAsync(d.FileUrl);
                            if (fileSizeInMB > 1024.0)
                                d.FileSize = (fileSizeInMB / 1024.0) + " Gb";
                            else d.FileSize = fileSizeInMB + " Mb";
                        }
                    }
                }
                int totalRow = curriculumDetail.Count();
                var result = curriculumDetail.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Total = totalFile, Success = true };
            }
        }

        public static async Task<List<tbl_CurriculumDetail>> UpdateCurriculumDetailIndex(List<CurriculumDetailUpdate> request, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                List<tbl_CurriculumDetail> curriculumDetails = new List<tbl_CurriculumDetail>();
                foreach (var item in request)
                {
                    tbl_CurriculumDetail curriculumDetail = await db.tbl_CurriculumDetail.SingleOrDefaultAsync(x => x.Id == item.Id && x.Enable == true);
                    if (curriculumDetail != null)
                    {
                        curriculumDetail.Index = item.Index;
                        curriculumDetail.ModifiedBy = user.FullName;
                        curriculumDetail.ModifiedOn = DateTime.Now;
                        curriculumDetails.Add(curriculumDetail);
                    }
                }
                if (curriculumDetails.Any())
                {
                    await db.SaveChangesAsync();
                }

                return curriculumDetails;
            }
        }
    }
}