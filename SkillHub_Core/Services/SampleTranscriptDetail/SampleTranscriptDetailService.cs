using LMSCore.Areas.Request;
using LMSCore.DTO.SampleTranscriptDetail;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services.SampleTranscriptDetail
{
    public class SampleTranscriptDetailService : DomainService
    {
        public SampleTranscriptDetailService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<SampleTranscriptDetailDTO> GetById(int id)
        {
            var data = await dbContext.tbl_SampleTranscriptDetail.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data != null)
            {
                return new SampleTranscriptDetailDTO(data);
            }
            return null;
        }
        public async Task<SampleTranscriptDetailDTO> Insert(SampleTranscriptDetailPost request, tbl_UserInformation currentUser)
        {
            var data = new tbl_SampleTranscriptDetail(request);
            var hasSample = await dbContext.tbl_SampleTranscript.AnyAsync(x => x.Id == request.SampleTranscriptId && x.Enable == true);
            if (!hasSample)
                throw new Exception("Không tìm thấy bảng điểm");
            data.Index = await NewIndex(request.SampleTranscriptId);
            data.CreatedBy = data.ModifiedBy = currentUser.FullName;
            dbContext.tbl_SampleTranscriptDetail.Add(data);
            await dbContext.SaveChangesAsync();
            return await GetById(data.Id);
        }
        public async Task<int> NewIndex(int sampleTranscriptId)
        {
            var lastIndex = await dbContext.tbl_SampleTranscriptDetail
                .Where(x => x.SampleTranscriptId == sampleTranscriptId && x.Enable == true)
                .OrderByDescending(x => x.Index)
                .FirstOrDefaultAsync();
            if (lastIndex == null)
                return 1;
            return lastIndex.Index + 1;
        }
        public async Task ReloadIndex(int sampleTranscriptId)
        {
            var details = await dbContext.tbl_SampleTranscriptDetail
                .Where(x => x.SampleTranscriptId == sampleTranscriptId && x.Enable == true)
                .OrderBy(x => x.Index)
                .ToListAsync();
            if (details.Any())
            {
                int index = 1;
                foreach (var item in details)
                {
                    item.Index = index;
                    index++;
                }
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task<SampleTranscriptDetailDTO> Update(SampleTranscriptDetailPut request, tbl_UserInformation currentUser)
        {
            var data = await dbContext.tbl_SampleTranscriptDetail.SingleOrDefaultAsync(x => x.Id == request.Id && x.Enable == true);
            if (data == null)
                throw new Exception("Không tìm thấy dữ liệu");
            data.Name = request.Name ?? data.Name;
            data.Type = request.Type == null ? data.Type : request.Type.ToString();
            data.ModifiedBy = currentUser.FullName;
            data.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return await GetById(data.Id);
        }
        public async Task Delete(int id, tbl_UserInformation currentUser)
        {
            var data = await dbContext.tbl_SampleTranscriptDetail.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data == null)
                throw new Exception("Không tìm thấy dữ liệu");
            data.Enable = false;
            data.ModifiedBy = currentUser.FullName;
            data.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            await ReloadIndex(data.SampleTranscriptId);
        }
        public async Task<IList<SampleTranscriptDetailDTO>> GetBySampleTranscript(int sampleTranscriptId)
        {
            var data = await dbContext.tbl_SampleTranscriptDetail
                .Where(x => x.Enable == true && x.SampleTranscriptId == sampleTranscriptId)
                .Select(x => new SampleTranscriptDetailDTO
                {
                    Id = x.Id,
                    Type = x.Type,
                    CreatedBy = x.CreatedBy,
                    CreatedOn = x.CreatedOn,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedOn = x.ModifiedOn,
                    Name = x.Name,
                    Index = x.Index,
                    SampleTranscriptId = x.SampleTranscriptId
                }).OrderBy(x => x.Index).ToListAsync();
            return data;
        }
        public async Task ChangeIndex(ChangeIndexRequest request, tbl_UserInformation currentUser)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!request.Items.Any())
                        throw new Exception("Không tìm thấy dữ liệu");
                    foreach (var item in request.Items)
                    {
                        var entity = await dbContext.tbl_SampleTranscriptDetail.SingleOrDefaultAsync(x => x.Id == item.Id);
                        if (entity == null)
                            throw new Exception("Không tìm thấy item");
                        entity.Index = item.Index;
                        await dbContext.SaveChangesAsync();
                    }
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
}
