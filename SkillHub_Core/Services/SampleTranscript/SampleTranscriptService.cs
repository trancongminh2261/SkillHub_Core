using LMSCore.Areas.Models;
using LMSCore.Models;
using System.Threading.Tasks;
using System;
using LMSCore.DTO.SampleTranscript;
using Microsoft.EntityFrameworkCore;
using LMSCore.Areas.Request;
using System.Linq;

namespace LMSCore.Services.SampleTranscript
{
    public class SampleTranscriptService : DomainService
    {
        public SampleTranscriptService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<SampleTranscriptDTO> GetById(int id)
        {
            var data = await dbContext.tbl_SampleTranscript.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data != null)
            {
                return new SampleTranscriptDTO(data);
            }
            return null;
        }
        public async Task<SampleTranscriptDTO> Insert(SampleTranscriptPost request, tbl_UserInformation currentUser)
        {
            var data = new tbl_SampleTranscript(request);
            data.CreatedBy = data.ModifiedBy = currentUser.FullName;
            dbContext.tbl_SampleTranscript.Add(data);
            await dbContext.SaveChangesAsync();
            return await GetById(data.Id);
        }
        public async Task<SampleTranscriptDTO> Update(SampleTranscriptPut request, tbl_UserInformation currentUser)
        {
            var data = await dbContext.tbl_SampleTranscript.SingleOrDefaultAsync(x => x.Id == request.Id && x.Enable == true);
            if (data == null)
                throw new Exception("Không tìm thấy dữ liệu");
            data.Name = request.Name ?? data.Name;
            data.ModifiedBy = currentUser.FullName;
            data.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return await GetById(data.Id);
        }
        public async Task Delete(int id, tbl_UserInformation currentUser)
        {
            var data = await dbContext.tbl_SampleTranscript.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data == null)
                throw new Exception("Không tìm thấy dữ liệu");
            data.Enable = false;
            data.ModifiedBy = currentUser.FullName;
            data.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
        }
        public async Task<AppDomainResult> GetAll(SearchOptions baseSearch)
        {
            if (baseSearch == null)
                baseSearch = new SearchOptions();
            var listId = await dbContext.tbl_SampleTranscript
                .Where(x => x.Enable == true
                && (string.IsNullOrEmpty(baseSearch.Search) || x.Name.Contains(baseSearch.Search)))
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => x.Id).ToListAsync();

            if (!listId.Any())
                return new AppDomainResult() { TotalRow = 0, Data = null };

            int totalRow = listId.Count();
            listId = listId.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
            var data = (from i in listId
                        select Task.Run(() => GetById(i)).Result).ToList();
            return new AppDomainResult()
            {
                TotalRow = totalRow,
                Data = data
            };
        }
    }
}
