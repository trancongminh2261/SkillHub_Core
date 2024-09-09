using LMSCore.Areas.Models;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using LMSCore.DTO.SaveGradesInClass;
using System.Linq;
using LMSCore.Areas.Request;

namespace LMSCore.Services.SaveGradesInClass
{
    public class SaveGradesInClassService : DomainService
    {
        public SaveGradesInClassService(lmsDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<AppDomainResult<SaveGradesInClassDTO>> GetAll(SaveGradesInClassSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new SaveGradesInClassSearch();

            string sql = $"Get_SaveGradesInClass @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassTranscriptId = {baseSearch.ClassTranscriptId}";
            var data = await dbContext.SqlQuery<SaveGradesInClassDTO>(sql);
            if (!data.Any()) return new AppDomainResult<SaveGradesInClassDTO> { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult<SaveGradesInClassDTO>()
            {
                TotalRow = totalRow ?? 0,
                Data = data
            };
        }
        public async Task<IList<SaveGradesInClassDTO>> InsertOrUpdate(List<SaveGradesInClassPost> data, tbl_UserInformation currentUser)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var result = new List<SaveGradesInClassDTO>();
                    if (data != null && data.Any())
                    {
                        foreach (var itemModel in data)
                        {
                            var student = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId);
                            if (itemModel.Details.Any() && itemModel.Details != null)
                            {
                                foreach (var item in itemModel.Details)
                                {
                                    var convertDTO = new SaveGradesInClassDTO();
                                    var entity = await dbContext.tbl_SaveGradesInClass.SingleOrDefaultAsync(x => x.ClassTranscriptId == itemModel.ClassTranscriptId && x.ClassTranscriptDetailId == item.ClassTranscriptDetailId && x.StudentId == itemModel.StudentId && x.Enable == true);

                                    if (entity == null)
                                    {
                                        entity = new tbl_SaveGradesInClass(itemModel);
                                        entity.ClassTranscriptDetailId = item.ClassTranscriptDetailId;
                                        entity.Value = item.Value;
                                        entity.CreatedBy = entity.ModifiedBy = currentUser.FullName;
                                        entity.CreatedOn = DateTime.Now;
                                        entity.ModifiedOn = DateTime.Now;
                                        dbContext.tbl_SaveGradesInClass.Add(entity);
                                        convertDTO = new SaveGradesInClassDTO(entity);
                                    }
                                    else
                                    {
                                        entity.Value = item.Value ?? entity.Value;
                                        entity.ModifiedBy = currentUser.FullName;
                                        entity.ModifiedOn = DateTime.Now;
                                        convertDTO = new SaveGradesInClassDTO(entity);
                                    }

                                    await dbContext.SaveChangesAsync();
                                    result.Add(convertDTO);
                                }
                            }
                        }
                    }
                    tran.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }
    }
}
