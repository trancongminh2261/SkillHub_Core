using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.ClassTranscript;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services.ClassTranscript
{
    public class ClassTranscriptService : DomainService
    {
        public ClassTranscriptService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<ClassTranscriptDTO> GetById(int id)
        {
            var data = await dbContext.tbl_ClassTranscript.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data != null)
            {
                return new ClassTranscriptDTO(data);
            }
            return null;
        }
        public async Task<ClassTranscriptDTO> Insert(ClassTranscriptPost request, tbl_UserInformation currentUser)
        {
            var data = new tbl_ClassTranscript(request);
            data.CreatedBy = data.ModifiedBy = currentUser.FullName;
            dbContext.tbl_ClassTranscript.Add(data);
            await dbContext.SaveChangesAsync();
            //Nếu có chọn bảng điểm mẫu thì clone ra thành bảng điểm lớp
            if (data.SampleTranscriptId.HasValue)
            {
                var sampleTranscrpitDetails = await dbContext.tbl_SampleTranscriptDetail.Where(x => x.SampleTranscriptId == data.SampleTranscriptId && x.Enable == true).ToListAsync();
                if (sampleTranscrpitDetails.Count > 0)
                {
                    foreach (var item in sampleTranscrpitDetails)
                    {
                        var classTranscrpitDetail = new tbl_ClassTranscriptDetail();
                        classTranscrpitDetail.ClassTranscriptId = data.Id;
                        classTranscrpitDetail.Name = item.Name;
                        classTranscrpitDetail.Type = item.Type;
                        classTranscrpitDetail.Index = item.Index;
                        classTranscrpitDetail.Enable = true;
                        classTranscrpitDetail.CreatedBy = currentUser.CreatedBy;
                        classTranscrpitDetail.CreatedOn = DateTime.Now;
                        classTranscrpitDetail.ModifiedBy = currentUser.CreatedBy;
                        classTranscrpitDetail.ModifiedOn = DateTime.Now;
                        dbContext.tbl_ClassTranscriptDetail.Add(classTranscrpitDetail);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            return await GetById(data.Id);
        }
        public async Task<ClassTranscriptDTO> Update(ClassTranscriptPut request, tbl_UserInformation currentUser)
        {
            var data = await dbContext.tbl_ClassTranscript.SingleOrDefaultAsync(x => x.Id == request.Id && x.Enable == true);
            if (data == null)
                throw new Exception("Không tìm thấy dữ liệu");
            data.Name = request.Name ?? data.Name;
            data.Date = request.Date ?? data.Date;
            data.ModifiedBy = currentUser.FullName;
            data.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return await GetById(data.Id);
        }
        public async Task Delete(int id, tbl_UserInformation currentUser)
        {
            var data = await dbContext.tbl_ClassTranscript.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data == null)
                throw new Exception("Không tìm thấy dữ liệu");
            data.Enable = false;
            data.ModifiedBy = currentUser.FullName;
            data.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
        }
        public async Task<AppDomainResult<ClassTranscriptDTO>> GetAll(ClassTranscriptSearch baseSearch)
        {
            if (baseSearch == null)
                baseSearch = new ClassTranscriptSearch();
            var listId = await dbContext.tbl_ClassTranscript
                .Where(x => x.Enable == true
                && x.ClassId == baseSearch.ClassId
                && (string.IsNullOrEmpty(baseSearch.Search) || x.Name.Contains(baseSearch.Search)))
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => x.Id).ToListAsync();

            if (!listId.Any())
                return new AppDomainResult<ClassTranscriptDTO>() { TotalRow = 0, Data = null };

            int totalRow = listId.Count();
            listId = listId.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
            var data = (from i in listId
                        select Task.Run(() => GetById(i)).Result).ToList();
            return new AppDomainResult<ClassTranscriptDTO>()
            {
                TotalRow = totalRow,
                Data = data
            };
        }
    }
}
