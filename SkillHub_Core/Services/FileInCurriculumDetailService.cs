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
    public class FileInCurriculumDetailService
    {
        public static async Task<tbl_FileInCurriculumDetail> Insert(FileInCurriculumDetailCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var model = new tbl_FileInCurriculumDetail(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                model.Index = 1;
                var fileCurriculum = await db.tbl_FileInCurriculumDetail.Where(x => x.CurriculumDetailId == itemModel.CurriculumDetailId && x.Enable == true).OrderBy(x => x.Index).ToListAsync();
                if (fileCurriculum.Any())
                {
                    model.Index = fileCurriculum.Select(x => x.Index).LastOrDefault() + 1;
                }                   
                db.tbl_FileInCurriculumDetail.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }

        public static async Task<tbl_FileInCurriculumDetail> Update(FileInCurriculumDetailUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_FileInCurriculumDetail.Where(x => x.Enable == true && x.Id == itemModel.Id).SingleOrDefaultAsync();
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.ModifiedBy = user.FullName;
                //Nếu vị trí muốn đổi có file tồn tại thì hoán đổi vị trí cho nhau
                if (itemModel.Index != null)
                {                   
                    var fileOfIndexExsist = await db.tbl_FileInCurriculumDetail.SingleOrDefaultAsync(x => x.Enable == true && x.CurriculumDetailId == entity.CurriculumDetailId && x.Index == itemModel.Index);
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
                var entity = await db.tbl_FileInCurriculumDetail.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<List<FileInCurriculumDetailModel>> GetByCurriculumDetail(int curriculumDetailId)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_FileInCurriculumDetail.Where(x => x.CurriculumDetailId == curriculumDetailId && x.Enable == true).OrderBy(x => x.Index).ToListAsync();
                return (from i in data
                        select new FileInCurriculumDetailModel(i)).ToList();
            }
        }

        public static async Task<List<tbl_FileInCurriculumDetail>> FileUpdateCurriculumDetailIndex(List<FileInCurriculumDetailUpdate> request, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                List<tbl_FileInCurriculumDetail> fileCurriculumDetails = new List<tbl_FileInCurriculumDetail>();
                foreach (var item in request)
                {
                    tbl_FileInCurriculumDetail curriculumDetail = await db.tbl_FileInCurriculumDetail.SingleOrDefaultAsync(x => x.Id == item.Id && x.Enable == true);
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