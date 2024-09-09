using LMSCore.Areas.Request;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using LMSCore.DTO.ClassTranscriptDetail;
using System.Linq;

namespace LMSCore.Services.ClassTranscriptDetail
{
    public class ClassTranscriptDetailService : DomainService
    {
        public ClassTranscriptDetailService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<ClassTranscriptDetailDTO> GetById(int id)
        {
            var data = await dbContext.tbl_ClassTranscriptDetail.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data != null)
            {
                return new ClassTranscriptDetailDTO(data);
            }
            return null;
        }
        public async Task<ClassTranscriptDetailDTO> Insert(ClassTranscriptDetailPost request, tbl_UserInformation currentUser)
        {
            var data = new tbl_ClassTranscriptDetail(request);
            var hasSample = await dbContext.tbl_ClassTranscript.AnyAsync(x => x.Id == request.ClassTranscriptId && x.Enable == true);
            if (!hasSample)
                throw new Exception("Không tìm thấy bảng điểm");
            data.Index = await NewIndex(request.ClassTranscriptId);
            data.CreatedBy = data.ModifiedBy = currentUser.FullName;
            dbContext.tbl_ClassTranscriptDetail.Add(data);
            await dbContext.SaveChangesAsync();
            return await GetById(data.Id);
        }
        public async Task<int> NewIndex(int classTranscriptId)
        {
            var lastIndex = await dbContext.tbl_ClassTranscriptDetail
                .Where(x => x.ClassTranscriptId == classTranscriptId && x.Enable == true)
                .OrderByDescending(x => x.Index)
                .FirstOrDefaultAsync();
            if (lastIndex == null)
                return 1;
            return lastIndex.Index + 1;
        }
        public async Task ReloadIndex(int classTranscriptId)
        {
            var details = await dbContext.tbl_ClassTranscriptDetail
                .Where(x => x.ClassTranscriptId == classTranscriptId && x.Enable == true)
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
        public async Task<ClassTranscriptDetailDTO> Update(ClassTranscriptDetailPut request, tbl_UserInformation currentUser)
        {
            var data = await dbContext.tbl_ClassTranscriptDetail.SingleOrDefaultAsync(x => x.Id == request.Id && x.Enable == true);
            if (data == null)
                throw new Exception("Không tìm thấy dữ liệu");
            data.Name = request.Name ?? data.Name;
            data.MaxValue = request.MaxValue ?? data.MaxValue;
            data.ModifiedBy = currentUser.FullName;
            data.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return await GetById(data.Id);
        }
        public async Task Delete(int id, tbl_UserInformation currentUser)
        {
            var data = await dbContext.tbl_ClassTranscriptDetail.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data == null)
                throw new Exception("Không tìm thấy dữ liệu");
            data.Enable = false;
            data.ModifiedBy = currentUser.FullName;
            data.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            await ReloadIndex(data.ClassTranscriptId);
        }
        public async Task<IList<ClassTranscriptDetailDTO>> GetByClassTranscript(int classTranscriptId)
        {
            var data = await dbContext.tbl_ClassTranscriptDetail
                .Where(x => x.Enable == true && x.ClassTranscriptId == classTranscriptId)
                .Select(x => new ClassTranscriptDetailDTO
                {
                    Id = x.Id,
                    Type = x.Type,
                    CreatedBy = x.CreatedBy,
                    CreatedOn = x.CreatedOn,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedOn = x.ModifiedOn,
                    Name = x.Name,
                    Index = x.Index,
                    MaxValue = x.MaxValue,
                    ClassTranscriptId = x.ClassTranscriptId
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
                        var entity = await dbContext.tbl_ClassTranscriptDetail.SingleOrDefaultAsync(x => x.Id == item.Id);
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
